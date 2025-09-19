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
            _leftIndex.Add(lhSolution);
            return _rightIndex.GetMatches(lhSolution, filterFunc);
        }

        var joinSolutions = _rightIndex.GetMatches(lhSolution, filterFunc)
        .Select(s => s.Join(lhSolution))
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

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph)
    {
        if (_lhsHasMore)
        {
            _rightIndex.Add(rhSolution);
        }

        Func<ISet, bool>? filterFunc = _filter == null ? null : (s => Filter(s, context, activeGraph));
        return _leftIndex.GetMatches(rhSolution, filterFunc)
            .Select(s => s.Join(rhSolution));
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

/*
    private class LhsSolution(ISet set) : ISet
    {
        public bool Joined { get; private set; }

        public bool Equals(ISet? other)
        {
            return set.Equals(other);
        }

        public void Add(string variable, INode value)
        {
            set.Add(variable, value);
        }

        public bool ContainsVariable(string variable)
        {
            return set.ContainsVariable(variable);
        }

        public bool IsCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return set.IsCompatibleWith(s, vars);
        }

        public bool IsMinusCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return set.IsMinusCompatibleWith(s, vars);
        }

        public int ID
        {
            get => set.ID;
            set => set.ID = value;
        }

        public void Remove(string variable)
        {
            set.Remove(variable);
        }

        public INode this[string variable] => set[variable];

        public IEnumerable<INode> Values => set.Values;

        public IEnumerable<string> Variables => set.Variables;

        public ISet Join(ISet other)
        {
            return set.Join(other);
        }

        public ISet? FilterJoin(ISet other, Func<ISet, bool>? filterFunc = null)
        {
            ISet joinResult = set.Join(other);
            if (filterFunc != null)
            {
                if (filterFunc(joinResult) == false)
                {
                    return null;
                }
            }
            Joined = true;
            return set.Join(other);
        }

        public ISet Copy()
        {
            return set.Copy();
        }

        public bool BindsAll(IEnumerable<string> vars)
        {
            return set.BindsAll(vars);
        }
    }
    */
}