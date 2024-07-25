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
    private List<string> _messages = new();
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem pathStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        HashSet<INode> visited = new();
        if (minIterations == 0)
        {
            var haveStartTerm = TryEvaluatePattern(pathStart, input, out INode? startTerm);
            var haveEndTerm = TryEvaluatePattern(pathEnd, input, out INode? endTerm);
            if (haveStartTerm && haveEndTerm && startTerm != null && startTerm.Equals(endTerm))
            {
                visited.Add(startTerm);
                yield return new PathResult(startTerm, startTerm);
            } else if (haveStartTerm && startTerm != null && pathEnd is VariablePattern)
            {
                visited.Add(startTerm);
                yield return new PathResult(startTerm, startTerm);
            }
            else if (haveEndTerm && endTerm != null && pathStart is VariablePattern)
            {
                visited.Add(endTerm);
                yield return new PathResult(endTerm, endTerm);
            }
            else if (!haveStartTerm && !haveEndTerm && pathStart is VariablePattern && pathEnd is VariablePattern)
            {
                // Every node in the graph is a match for iteration 0!
                foreach (var startNode in context.Data.Graphs.SelectMany(g => g.Nodes).Distinct())
                {
                    yield return new PathResult(startNode, startNode);
                    await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken,
                                       1, new NodeMatchPattern(startNode), new HashSet<INode> { startNode }))
                    {
                        yield return stepResult;
                    }
                }
                yield break;
            }
        }

        await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken, 1, pathStart, visited))
        {
            yield return stepResult;
        }

        Console.WriteLine(_messages);
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
        [EnumeratorCancellation] CancellationToken cancellationToken, int step, PatternItem stepStart, HashSet<INode> visited)
    {
        if (maxIterations > 0 && step > maxIterations) yield break;
        stepStart = EvaluatePatternItem(stepStart, input);
        if (stepStart is NodeMatchPattern nmp)
        {
            lock (visited)
            {
                if (!visited.Add(nmp.Node)) yield break;
            }
        }
        _messages.Add($"EvaluateStep {step} with start {stepStart} with visited {String.Join(", ", visited)}");
        await foreach (PathResult result in stepEvaluation.Evaluate(stepStart, context, input, activeGraph,
                           cancellationToken))
        {
            if (step >= minIterations)
            {
                var output = new Set();
                if (pathEnd.Accepts(context, result.EndNode, output))
                {
                    _messages.Add($"Yield result {result.StartNode}, {result.EndNode}");
                    yield return result;
                }
            }
            await foreach (PathResult nextStepResult in EvaluateStep(context, input, activeGraph, cancellationToken,
                               step + 1, new NodeMatchPattern(result.EndNode), visited))
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