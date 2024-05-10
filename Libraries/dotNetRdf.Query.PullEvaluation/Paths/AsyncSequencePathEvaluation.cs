using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncSequencePathEvaluation(IAsyncPathEvaluation lhs, IAsyncPathEvaluation rhs, string joinVar)
    : IAsyncPathEvaluation
{
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem pathStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PathResult leftResult in lhs.Evaluate(pathStart, context, input, activeGraph, cancellationToken))
        {
            await foreach (PathResult rightResult in rhs.Evaluate(new NodeMatchPattern(leftResult.EndNode), context,
                               input, activeGraph, cancellationToken))
            {
                yield return new PathResult(leftResult.StartNode, rightResult.EndNode);
            }
        }
    }
}