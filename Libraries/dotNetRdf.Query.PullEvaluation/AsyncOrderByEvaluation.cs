using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Ordering;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncOrderByEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    private readonly SortedSet<ISet> _sorted;
    internal AsyncOrderByEvaluation(OrderBy orderBy, PullEvaluationContext context, IAsyncEvaluation inner)
    {
        IComparer<ISet> comparer = new NoEqualityComparer<ISet>(MakeSetComparer(orderBy.Ordering, context), 1);
        _sorted = new SortedSet<ISet>(comparer);
        _inner = inner;
    }
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solution in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            _sorted.Add(solution);
        }

        foreach (ISet s in _sorted) { yield return s;}
    }

    private class NoEqualityComparer<T>(IComparer<T> inner, int valueIfEqual) : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            var ret = inner.Compare(x, y);
            return ret == 0 ? valueIfEqual : ret;
        }
    }
    private static IComparer<ISet> MakeSetComparer(ISparqlOrderBy ordering, PullEvaluationContext context)
    {
        return ordering switch
        {
            OrderByVariable obv => new OrderByVariableSetComparer(obv, context),
            OrderByExpression obe => new OrderByExpressionSetComparer(obe, context),
            _ => throw new RdfQueryException("Unable to process ordering algebra " + ordering.GetType())
        };
    }
    private class OrderByVariableSetComparer : IComparer<ISet>
    {
        private readonly OrderByVariable _ordering;
        private readonly SparqlOrderingComparer _orderingComparer;
        private readonly IComparer<ISet>? _child;
        internal OrderByVariableSetComparer(OrderByVariable ordering, PullEvaluationContext context)
        {
            this._ordering = ordering;
            this._orderingComparer = context.OrderingComparer;
            _child = ordering.Child != null ? MakeSetComparer(ordering.Child, context) : null;
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
        internal OrderByExpressionSetComparer(OrderByExpression ordering, PullEvaluationContext context)
        {
            _ordering = ordering;
            _context = context;
            _child = ordering.Child != null ? MakeSetComparer(ordering.Child, context) : null;
        }
        public int Compare(ISet x, ISet y)
        {
            if (x.Equals(y))
            {
                return 0;
            }

            try
            {
                INode a = _ordering.Expression.Accept(_context.ExpressionProcessor, _context, x);
                INode b;
                try
                {
                    b = _ordering.Expression.Accept(_context.ExpressionProcessor, _context, y);
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
                    _ordering.Expression.Accept(_context.ExpressionProcessor, _context, y);

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