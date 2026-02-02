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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncLeftJoinEvaluation
    : AbstractAsyncJoinEvaluation
{
    private readonly JoinIndex _leftIndex;
    private readonly JoinIndex _rightIndex;
    private readonly string[] _joinVars;
    private readonly ISet _emptyRhs;
    private readonly ISparqlFilter? _filter;

    public AsyncLeftJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string[] joinVars, string[] rhsVars, ISparqlFilter? filter)
        : base(lhs, rhs)
    {
        _joinVars = joinVars;
        _leftIndex = new JoinIndex(joinVars, trackJoins: true);
        _rightIndex = new JoinIndex(joinVars);
        _emptyRhs = new Set();
        _filter = filter;
        foreach (var variable in rhsVars)
        {
            _emptyRhs.Add(variable, null);
        }
    }

    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhSolution, IRefNode? activeGraph)
    {
        Func<ISet, bool>? filterFunc = _filter == null ? null : (s => Filter(s, context, activeGraph));
        if (_rhsHasMore)
        {
            var lhSolutionIndex = _leftIndex.Add(lhSolution);
            var candidates = _rightIndex.GetMatchIndexes(lhSolution);
            var joinSolutions = candidates
                .Select(ix => _rightIndex[ix].Join(lhSolution))
                .Where(s => filterFunc == null || filterFunc(s))
                .ToList();
            if (joinSolutions.Count > 0)
            {
                _leftIndex.MarkJoined(lhSolutionIndex);
            }
            return joinSolutions;
        }
        else
        {

            var joinSolutions = _rightIndex.GetMatchIndexes(lhSolution)
            .Select(ix => _rightIndex[ix].Join(lhSolution))
            .Where(s => filterFunc == null || filterFunc(s))
            .ToList();
            if (joinSolutions.Count > 0)
            {
                return joinSolutions;
            }
            else
            {
                return [lhSolution.Join(_emptyRhs)];
            }
        }
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph)
    {
        if (_lhsHasMore)
        {
            _rightIndex.Add(rhSolution);
        }

        Func<ISet, bool>? filterFunc = _filter == null ? null : (s => Filter(s, context, activeGraph));
        var candidates = _leftIndex.GetMatchIndexes(rhSolution);
        foreach (var ix in candidates)
        {
            var joinedSolution = _leftIndex[ix].Join(rhSolution);
            if (filterFunc == null || filterFunc(joinedSolution))
            {
                _leftIndex.MarkJoined(ix);
                yield return joinedSolution;
            }
        }
    }

    protected override IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context)
    {
        return null;
    }

    protected override IEnumerable<ISet> OnRhsDone(PullEvaluationContext context)
    {
        return _leftIndex.GetUnjoinedSets().Select(s => s.Join(_emptyRhs));
    }

    private bool Filter(ISet s, PullEvaluationContext context, IRefNode? activeGraph)
    {
        if (_filter == null) return true;
        try
        {
            return _filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(s, activeGraph)).AsSafeBoolean();
        }
        catch (RdfQueryException)
        {
            return false;
        }
    }
}