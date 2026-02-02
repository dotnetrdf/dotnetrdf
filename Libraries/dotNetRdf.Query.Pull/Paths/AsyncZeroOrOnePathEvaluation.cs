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

using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal class AsyncZeroOrOnePathEvaluation(IAsyncEvaluation pathEvaluation, PatternItem pathStart, PatternItem pathEnd)
    : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var haveStartTerm = TryEvaluatePattern(pathStart, input, out INode? startTerm);
        var haveEndTerm = TryEvaluatePattern(pathEnd, input, out INode? endTerm);
        if (haveStartTerm && haveEndTerm && startTerm != null && endTerm != null)
        {
            if (startTerm.Equals(endTerm))
            {
                yield return input ?? new Set();
            }
            else if (await pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).AnyAsync(cancellationToken))
            {
                yield return input ?? new Set();
            }
        }
        else if (haveStartTerm && pathEnd is VariablePattern vp)
        {
            Set result = input == null ? new Set() : new Set(input);
            result.Add(vp.VariableName, startTerm);
            yield return result;
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                if (startTerm == null || !startTerm.Equals(solution[vp.VariableName]))
                {
                    yield return solution;
                }
            }
        }
        else if (haveEndTerm && pathStart is VariablePattern svp)
        {
            var result = new Set(input);
            result.Add(svp.VariableName, endTerm);
            yield return result;
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                if (endTerm == null || !endTerm.Equals(solution[svp.VariableName]))
                {
                    yield return solution;
                }
            }
        }
        else
        {
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return solution;
            }
        }
    }

    private static bool TryEvaluatePattern(PatternItem patternItem, ISet? input, out INode? node)
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
}