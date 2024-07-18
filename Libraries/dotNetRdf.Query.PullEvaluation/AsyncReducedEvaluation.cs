using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncReducedEvaluation(IAsyncEvaluation inner) : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        return inner.Evaluate(context, input, activeGraph, cancellationToken).DistinctUntilChanged();
    }
}