using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncReducedEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    public AsyncReducedEvaluation(IAsyncEvaluation inner)
    {
        _inner = inner;
    }
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        return _inner.Evaluate(context, input, activeGraph, cancellationToken).DistinctUntilChanged();
    }
}