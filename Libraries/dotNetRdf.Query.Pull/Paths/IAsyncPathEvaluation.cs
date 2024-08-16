using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal interface IAsyncPathEvaluation
{
    public IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input,
        IRefNode? activeGraph, CancellationToken cancellationToken);
}