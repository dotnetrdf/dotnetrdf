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
using VDS.RDF.Query.Ordering;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncOrderByEvaluation(OrderBy orderBy, IAsyncEvaluation inner) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IComparer<ISet> comparer = new NoEqualityComparer<ISet>(MakeSetComparer(orderBy.Ordering, context, activeGraph), 1);
        var sorted = new SortedSet<ISet>(comparer);
        ISet? lastElem = null;
        await foreach (ISet solution in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (lastElem != null && comparer.Compare(solution, lastElem) > 0)
            {
                continue;
            }
            sorted.Add(solution);
            if (context.SliceLength.HasValue && sorted.Count == context.SliceLength.Value)
            {
                lastElem = sorted.ElementAt(context.SliceLength.Value - 1);
            }
        }

        foreach (ISet s in sorted) { yield return s;}
    }

    private class NoEqualityComparer<T>(IComparer<T> inner, int valueIfEqual) : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            var ret = inner.Compare(x, y);
            return ret == 0 ? valueIfEqual : ret;
        }
    }
    private static IComparer<ISet> MakeSetComparer(ISparqlOrderBy ordering, PullEvaluationContext context, IRefNode? activeGraph)
    {
        return ordering switch
        {
            OrderByVariable obv => new OrderByVariableSetComparer(obv, context, activeGraph),
            OrderByExpression obe => new OrderByExpressionSetComparer(obe, context, activeGraph),
            _ => throw new RdfQueryException("Unable to process ordering algebra " + ordering.GetType())
        };
    }
    private class OrderByVariableSetComparer : IComparer<ISet>
    {
        private readonly OrderByVariable _ordering;
        private readonly SparqlOrderingComparer _orderingComparer;
        private readonly IComparer<ISet>? _child;
        internal OrderByVariableSetComparer(OrderByVariable ordering, PullEvaluationContext context, IRefNode? activeGraph)
        {
            this._ordering = ordering;
            this._orderingComparer = context.OrderingComparer;
            _child = ordering.Child != null ? MakeSetComparer(ordering.Child, context, activeGraph) : null;
        }
        
        public int Compare(ISet x, ISet y)
        {
            INode xval = x[_ordering.Variable];
            if (xval == null)
            {
                if (y[_ordering.Variable] == null)
                {
                    return _child?.Compare(x, y) ?? 0;
                }

                return _ordering.Descending ? 1 : -1;
            }

            var c = _orderingComparer.Compare(xval, y[_ordering.Variable]);

            if (c == 0 && _child != null)
            {
                return _child.Compare(x, y);
            }

            return c * (_ordering.Descending ? -1 : 1);
        }
    }

    private class OrderByExpressionSetComparer
        : IComparer<ISet>
    {
        private readonly OrderByExpression _ordering;
        private readonly PullEvaluationContext _context;
        private readonly IComparer<ISet>? _child;
        private readonly IRefNode? _activeGraph;

        internal OrderByExpressionSetComparer(OrderByExpression ordering, PullEvaluationContext context, IRefNode? activeGraph)
        {
            _ordering = ordering;
            _context = context;
            _child = ordering.Child != null ? MakeSetComparer(ordering.Child, context, activeGraph) : null;
            _activeGraph = activeGraph;
        }

        public int Compare(ISet x, ISet y)
        {
            if (x.Equals(y))
            {
                return 0;
            }

            try
            {
                INode a = _ordering.Expression.Accept(_context.ExpressionProcessor, _context, new ExpressionContext(x, _activeGraph));
                INode b;
                try
                {
                    b = _ordering.Expression.Accept(_context.ExpressionProcessor, _context, new ExpressionContext(y, _activeGraph));
                }
                catch
                {
                    // If evaluating b errors consider this a NULL and rank a > b
                    return _ordering.Descending ? -1 : 1;
                }

                // If both give a value then compare
                if (a != null)
                {
                    var c = _context.OrderingComparer.Compare(a, b);
                    if (c == 0 && _child != null)
                    {
                        return _child.Compare(x, y);
                    }

                    return c * (_ordering.Descending ? -1 : 1);
                }

                // a is NULL so a < b
                return _ordering.Descending ? 1 : -1;
            }
            catch
            {
                try
                {
                    _ordering.Expression.Accept(_context.ExpressionProcessor, _context, new ExpressionContext(y, _activeGraph));

                    // If evaluating a errors but b evaluates correctly consider a to be NULL and rank a < b
                    return _ordering.Descending ? 1 : -1;
                }
                catch
                {
                    // If both error then use child if any to evaluate, otherwise consider a = b
                    return _child?.Compare(x, y) ?? 0;
                }
            }
        }
    }
}