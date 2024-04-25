using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncSubQueryEvaluation: IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    private readonly PullEvaluationContext _innerContext;

    public AsyncSubQueryEvaluation(IAsyncEvaluation inner, PullEvaluationContext subContext)
    {
        _inner = inner;
        _innerContext = subContext;
    }


    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        ISet innerInput = new Set();
        return _inner.Evaluate(_innerContext, innerInput, null, cancellationToken);
    }
}