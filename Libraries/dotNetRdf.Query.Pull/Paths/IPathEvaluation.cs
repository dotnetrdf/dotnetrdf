using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal interface IPathEvaluation
{
    internal IEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input,
        IRefNode? activeGraph, CancellationToken cancellationToken);

    internal IAsyncEnumerable<PathResult> EvaluateAsync(PatternItem stepStart, PullEvaluationContext context,
        ISet? input,
        IRefNode? activeGraph, CancellationToken cancellationToken);
}