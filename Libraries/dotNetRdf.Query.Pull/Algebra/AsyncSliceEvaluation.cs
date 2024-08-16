using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncSliceEvaluation(Slice slice, IAsyncEvaluation inner) : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        if (slice.Limit == 0)
        {
            return new ISet[]{}.ToAsyncEnumerable();
        }
        IAsyncEnumerable<ISet> sliced = inner.Evaluate(context, input, activeGraph, cancellationToken);
        if (slice.Offset > 0) sliced = sliced.Skip(slice.Offset);
        if (slice.Limit >= 0) sliced = sliced.Take(slice.Limit);
        return sliced;
    }
}