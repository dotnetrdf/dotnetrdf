using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncPathUnionEvaluation(IAsyncPathEvaluation lhs, IAsyncPathEvaluation rhs) : IAsyncPathEvaluation
{
    public IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken)
    {
        return AsyncEnumerableEx.Merge(
            lhs.Evaluate(stepStart, context, input, activeGraph, cancellationToken),
            rhs.Evaluate(stepStart, context, input, activeGraph, cancellationToken));
    }
}