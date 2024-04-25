using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncSelectEvaluation : IAsyncEvaluation
{
    private readonly Select _select;
    private readonly IAsyncEvaluation _inner;

    internal AsyncSelectEvaluation(Select select, IAsyncEvaluation inner)
    {
        _select = select;
        _inner = inner;
    }

    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return ProcessInnerResult(innerResult, context, activeGraph);
        }
    }

    private ISet ProcessInnerResult(ISet innerResult, PullEvaluationContext context, IRefNode? activeGraph)
    {
        if (_select.IsSelectAll)
        {
            // Filter out auto-generated variables
            foreach (var v in innerResult.Variables)
            {
                if (v.StartsWith(context.AutoVarPrefix))
                {
                    innerResult.Remove(v);
                }
            }

            return innerResult;
        }

        var resultSet = new Set();
        foreach (SparqlVariable sv in _select.SparqlVariables)
        {
            if (sv.IsResultVariable)
            {
                if (sv.Name.StartsWith(context.AutoVarPrefix)) continue;
                INode? variableBinding =
                    sv.IsProjection ? TryProcessProjection(sv, innerResult, context, activeGraph) :
                    innerResult.ContainsVariable(sv.Name) ? innerResult[sv.Name] : null;
                resultSet.Add(sv.Name, variableBinding);
            }
        }

        return resultSet;
    }

    private INode? TryProcessProjection(SparqlVariable sv, ISet innerResult, PullEvaluationContext context,
        IRefNode? activeGraph)
    {
        try
        {
            return sv.Projection.Accept(context.ExpressionProcessor, context,
                new ExpressionContext(innerResult, activeGraph));
        }
        catch (RdfQueryException)
        {
            return null;
        }
    }
}