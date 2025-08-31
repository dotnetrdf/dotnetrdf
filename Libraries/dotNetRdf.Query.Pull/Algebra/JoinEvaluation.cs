/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class JoinEvaluation(IEnumerableEvaluation lhs, IEnumerableEvaluation rhs, string[] joinVars)
    : AbstractJoinEvaluation(lhs, rhs)
{
    private readonly LinkedList<ISet> _leftSolutions = new();
    private readonly LinkedList<ISet> _rightSolutions = new();

    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhsResult, IRefNode? activeGraph, bool lhsHasMore, bool rhsHasMore)
    {
        if (rhsHasMore)
        {
            _leftSolutions.AddLast(lhsResult);
        }

        foreach (ISet joinResult in _rightSolutions
                     .Where(r => lhsResult.IsCompatibleWith(r, joinVars))
                     .Select(lhsResult.Join))
        {
            yield return joinResult;
        }
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhsResult, IRefNode? activeGraph, bool lhsHasMore, bool rhsHasMore)
    {
        if (lhsHasMore)
        {
            _rightSolutions.AddLast(rhsResult);
        }

        return _leftSolutions.Where(l => l.IsCompatibleWith(rhsResult, joinVars))
            .Select(l => l.Join(rhsResult));
    }

    protected override IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context)
    {
        _rightSolutions.Clear();
        return null;
    }

    protected override IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context)
    {
        _leftSolutions.Clear();
        return null;
    }
}