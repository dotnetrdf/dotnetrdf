using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncRepeatablePathEvaluation(
    int minIterations,
    int maxIterations,
    IAsyncPathEvaluation stepEvaluation,
    PatternItem pathStart,
    PatternItem pathEnd) : IAsyncEvaluation
{
    private readonly HashSet<INode> _visitedNodes = new HashSet<INode>();

    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        return EvaluateStep(context, input, activeGraph, cancellationToken, 1, pathStart);
    }

    private async IAsyncEnumerable<ISet> EvaluateStep(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken, int step, PatternItem stepStart)
    {
        if (step > maxIterations) yield break;
        stepStart = EvaluatePatternItem(stepStart, input);
        await foreach (PathResult result in stepEvaluation.Evaluate(stepStart, context, input, activeGraph,
                           cancellationToken))
        {
            if (_visitedNodes.Add(result.PathEnd))
            {
                if (step >= minIterations)
                {
                    // Can potentially return a result
                    ISet output = input != null ? new Set(input) : new Set();
                    if (pathStart.Accepts(context, result.PathStart, output) &&
                        pathEnd.Accepts(context, result.PathEnd, output))
                    {
                        yield return output;
                    }
                }

                await foreach (ISet nextStepResult in EvaluateStep(context, input, activeGraph, cancellationToken, step + 1,
                                   new NodeMatchPattern(result.PathEnd)))
                {
                    yield return nextStepResult;
                }
            }
        }
    }

    private static PatternItem EvaluatePatternItem(PatternItem patternItem, ISet? input)
    {
        if (patternItem is VariablePattern vp &&
            input != null &&
            input.ContainsVariable(vp.VariableName))
        {
            return new NodeMatchPattern(input[vp.VariableName]);
        }

        return patternItem;
    }
}