using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncUnionEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _lhs, _rhs;

    public AsyncUnionEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs)
    {
        _lhs = lhs;
        _rhs = rhs;
    }
    
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerableEx.Merge(_lhs.Evaluate(context, input, activeGraph, cancellationToken),
            _rhs.Evaluate(context, input, activeGraph, cancellationToken));
    }
}