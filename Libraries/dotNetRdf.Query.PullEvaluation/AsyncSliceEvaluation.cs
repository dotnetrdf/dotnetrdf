using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncSliceEvaluation : IAsyncEvaluation
{
    private readonly Slice _slice;
    private readonly IAsyncEvaluation _inner;
    public AsyncSliceEvaluation(Slice slice, IAsyncEvaluation inner)
    {
        _slice = slice;
        _inner = inner;
    }

    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        if (_slice.Limit == 0)
        {
            return new ISet[]{}.ToAsyncEnumerable();
        }
        IAsyncEnumerable<ISet> sliced = _inner.Evaluate(context, input, activeGraph, cancellationToken);
        if (_slice.Offset > 0) sliced = sliced.Skip(_slice.Offset);
        if (_slice.Limit >= 0) sliced = sliced.Take(_slice.Limit);
        return sliced;
    }
}