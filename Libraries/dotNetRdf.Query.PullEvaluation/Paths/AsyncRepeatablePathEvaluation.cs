using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncRepeatablePathEvaluation(
    int minIterations,
    int maxIterations,
    IAsyncPathEvaluation stepEvaluation,
    PatternItem pathEnd) : IAsyncPathEvaluation
{
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem pathStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (TryEvaluatePattern(pathStart, input, out INode? startNode))
        {
            await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken, 
                               startNode,  0, new NodeMatchPattern(startNode), new HashSet<INode>()))
            {
                yield return stepResult;
            }
        }
        else
        {
            foreach (INode node in context.GetNodes(pathStart, activeGraph))
            {
                await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken, node, 0,
                                   new NodeMatchPattern(node), new HashSet<INode>()))
                {
                    yield return stepResult;
                }
            }
        }
    }
    
    private static bool TryEvaluatePattern(PatternItem patternItem, ISet? input, [NotNullWhen(returnValue:true)] out INode? node)
    {
        switch (patternItem)
        {
            case VariablePattern vp:
                {
                    if (input != null && input.ContainsVariable(vp.VariableName))
                    {
                        INode? tmp = input[vp.VariableName];
                        if (tmp != null)
                        {
                            node = tmp;
                            return true;
                        }
                    }

                    break;
                }
            case NodeMatchPattern nmp:
                node = nmp.Node;
                return true;
            default:
                throw new RdfQueryException(
                    $"Support for pattern item {patternItem} ({patternItem.GetType()}) is not yet implemented.");
        }

        node = null;
        return false;
    }

    private async IAsyncEnumerable<PathResult> EvaluateStep(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken, INode pathStart, int step, PatternItem stepStart, HashSet<INode> visited)
    {
        if (maxIterations > 0 && step > maxIterations) yield break;
        stepStart = EvaluatePatternItem(stepStart, input);
        if (stepStart is NodeMatchPattern nmp)
        {
            lock (visited)
            {
                if (!visited.Add(nmp.Node)) yield break;
                if (step >= minIterations)
                {
                    yield return new PathResult(pathStart, nmp.Node);
                }
            }
        }

        await foreach (PathResult result in stepEvaluation.Evaluate(stepStart, context, input, activeGraph,
                           cancellationToken))
        {
            await foreach (PathResult nextStepResult in EvaluateStep(context, input, activeGraph, cancellationToken,
                               pathStart, step + 1, new NodeMatchPattern(result.EndNode), visited))
            {
                yield return nextStepResult;
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