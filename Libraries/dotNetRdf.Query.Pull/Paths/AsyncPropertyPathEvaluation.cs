using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal class AsyncPropertyPathEvaluation(INode predicate, PatternItem pathStart, PatternItem pathEnd) : IAsyncPathEvaluation
{
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (Triple t in context.GetTriples(new TriplePattern(stepStart, new NodeMatchPattern(predicate), pathEnd),
                     input, activeGraph))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new PathResult(t.Subject, t.Object);
        }
    }
}