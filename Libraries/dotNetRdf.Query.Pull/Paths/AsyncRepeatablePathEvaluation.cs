/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal class AsyncRepeatablePathEvaluation(
    int minIterations,
    int maxIterations,
    IAsyncPathEvaluation stepEvaluation) : IAsyncPathEvaluation
{
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem pathStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (TryEvaluatePattern(pathStart, input, out INode? startNode))
        {
            await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken, 
                               startNode,  0, new NodeMatchPattern(startNode), []))
            {
                yield return stepResult;
            }
        }
        else
        {
            foreach (INode node in context.GetNodes(pathStart, activeGraph))
            {
                await foreach (PathResult stepResult in EvaluateStep(context, input, activeGraph, cancellationToken, node, 0,
                                   new NodeMatchPattern(node), []))
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