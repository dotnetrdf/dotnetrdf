using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncInversePathEvaluation(IAsyncPathEvaluation inner, PatternItem pathStart, PatternItem pathEnd) : IAsyncPathEvaluation
{
    public IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken)
    {
        return inner.Evaluate(pathEnd, context, input, activeGraph, cancellationToken)
            .Select(r => new PathResult(r.EndNode, r.StartNode));
    }
}