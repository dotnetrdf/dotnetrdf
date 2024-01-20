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
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return ProcessInnerResult(innerResult);
        }
    }

    private ISet ProcessInnerResult(ISet innerResult)
    {
        var resultSet = new Set();
        foreach (SparqlVariable sv in _select.SparqlVariables)
        {
            if (sv.IsResultVariable)
            {
                resultSet.Add(sv.Name, innerResult[sv.Name]);
            }
            else if (sv.IsProjection)
            {
                throw new NotImplementedException("Projection in SELECT not yet implemented");
            }
            else if (sv.IsAggregate)
            {
                throw new NotImplementedException("Aggregation in SELECT not yet implemented");
            }
        }

        return resultSet;
    } 
}