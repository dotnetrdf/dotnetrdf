using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Pull.Paths;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncPropertyPathPatternEvaluation(IAsyncPathEvaluation pathEvaluation, PatternItem pathStart, PatternItem pathEnd) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (PathResult pathResult in pathEvaluation.Evaluate(pathStart, context, input, activeGraph,
                           cancellationToken).Distinct().WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Set output = input != null ? new Set(input) : new Set();
            if (pathStart.Accepts(context, pathResult.StartNode, output) &&
                pathEnd.Accepts(context, pathResult.EndNode, output))
            {
                yield return output;
            }
        }
    }
}