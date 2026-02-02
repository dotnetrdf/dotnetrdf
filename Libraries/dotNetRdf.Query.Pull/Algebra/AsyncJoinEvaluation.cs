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

using VDS.Common.Collections;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncJoinEvaluation : AbstractAsyncJoinEvaluation
{
    private readonly LinkedList<ISet> _leftSolutions = new();
    private readonly LinkedList<ISet> _rightSolutions = new();
    private readonly string[] _joinVars;
    private readonly JoinIndex _leftIndex;
    private readonly JoinIndex _rightIndex;

    public AsyncJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string[] joinVars) : base(lhs, rhs)
    {
        this._joinVars = joinVars;
        this._leftIndex = new JoinIndex(joinVars);
        this._rightIndex = new JoinIndex(joinVars);
    }



    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhsResult, IRefNode? activeGraph)
    {
        if (_rhsHasMore)
        {
            // TODO: Can remove _leftSolutions and expose the list of values in the JoinIndex instead.
            _leftSolutions.AddLast(lhsResult);
            _leftIndex.Add(lhsResult);
        }

        foreach (ISet rhsResult in _rightIndex.GetMatches(lhsResult).Where(r => r.IsCompatibleWith(lhsResult, _joinVars)))
        {
            yield return lhsResult.Join(rhsResult);
        }
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhsResult, IRefNode? activeGraph)
    {
        if (_lhsHasMore)
        {
            _rightSolutions.AddLast(rhsResult);
            _rightIndex.Add(rhsResult);
        }
        foreach (ISet leftResult in _leftIndex.GetMatches(rhsResult).Where(l => l.IsCompatibleWith(rhsResult, _joinVars)))
        {
            yield return leftResult.Join(rhsResult);
        }
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