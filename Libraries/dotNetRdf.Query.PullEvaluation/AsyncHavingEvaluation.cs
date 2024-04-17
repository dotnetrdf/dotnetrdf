using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncHavingEvaluation : IAsyncEvaluation
{
    private readonly Having _having;
    private readonly IAsyncEvaluation _inner;
    private readonly List<SparqlVariable> _toDrop;
    public AsyncHavingEvaluation(Having having, IAsyncEvaluation inner, List<SparqlVariable> havingVars)
    {
        _having = having;
        _inner = inner;
        _toDrop = havingVars;
    }
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solutionBinding in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            IValuedNode? result =
                _having.HavingClause.Expression.Accept(context.ExpressionProcessor, context, solutionBinding);
            if (result?.AsBoolean() ?? false)
            {
                foreach (SparqlVariable havingVar in _toDrop)
                {
                    solutionBinding.Remove(havingVar.Name);
                }
                yield return solutionBinding;
            }
        }
    }
}