using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncUnionEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs) : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerableEx.Merge(lhs.Evaluate(context, input, activeGraph, cancellationToken),
            rhs.Evaluate(context, input, activeGraph, cancellationToken));
    }
}