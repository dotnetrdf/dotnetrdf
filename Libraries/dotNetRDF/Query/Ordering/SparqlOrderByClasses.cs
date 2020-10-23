/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering
{
    /// <summary>
    /// Base Class for implementing Sparql ORDER BYs.
    /// </summary>
    public abstract class BaseOrderBy 
        : ISparqlOrderBy
    {
        /// <summary>
        /// Stores the Evaluation Context.
        /// </summary>
        protected SparqlEvaluationContext _context = null;
        /// <summary>
        /// Modifier used to make ordering Descending.
        /// </summary>
        /// <remarks>Implementations derived from this class should multiply their comparison results by the modifier to automatically provide Ascending/Descending order.</remarks>
        protected int _modifier = 1;

        /// <summary>
        /// Gets/Sets the Child Order By.
        /// </summary>
        public ISparqlOrderBy Child { get; set; } = null;

        /// <summary>
        /// Sets the Evaluation Context for the Ordering.
        /// </summary>
        public SparqlEvaluationContext Context
        {
            set
            {
                _context = value;
                if (Child != null)
                {
                    Child.Context = value;
                }
            }
        }

        /// <summary>
        /// Sets the Ordering to Descending.
        /// </summary>
        public bool Descending
        {
            get
            {
                return (_modifier == -1);
            }
            set
            {
                if (value)
                {
                    _modifier = -1;
                }
                else
                {
                    _modifier = 1;
                }
            }
        }

        /// <summary>
        /// Gets whether the Ordering is Simple.
        /// </summary>
        public abstract bool IsSimple
        {
            get;
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering.
        /// </summary>
        public abstract IEnumerable<string> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used in the Ordering.
        /// </summary>
        public abstract ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Abstract Compare method which derived classes should implement their ordering in.
        /// </summary>
        /// <param name="x">A Set.</param>
        /// <param name="y">A Set.</param>
        /// <returns></returns>
        public abstract int Compare(ISet x, ISet y);

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern.
        /// </summary>
        /// <param name="pattern">Triple Pattern.</param>
        /// <param name="nodeComparer">The node comparer to use.</param>
        /// <returns></returns>
        public abstract IComparer<Triple> GetComparer(IMatchTriplePattern pattern, ISparqlNodeComparer nodeComparer);

        /// <summary>
        /// Gets the String representation of the Order By.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// An ORDER BY which orders on the values bound to a particular variable.
    /// </summary>
    public class OrderByVariable
        : BaseOrderBy
    {
        private string _varname = string.Empty;

        /// <summary>
        /// Creates a new Ordering based on the Value of a given Variable.
        /// </summary>
        /// <param name="name">Variable to order upon.</param>
        public OrderByVariable(string name)
        {
            _varname = name.TrimStart('?', '$');
        }

        /// <summary>
        /// Compares Sets on the basis of their values for the Variable the class was instantiated with.
        /// </summary>
        /// <param name="x">A Set.</param>
        /// <param name="y">A Set.</param>
        /// <returns></returns>
        public override int Compare(ISet x, ISet y)
        {
            INode xval;
            xval = x[_varname];
            if (xval == null)
            {
                if (y[_varname] == null)
                {
                    if (Child != null)
                    {
                        return Child.Compare(x, y);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return _modifier * -1;
                }
            }
            else
            {
                var c = _context.OrderingComparer.Compare(xval, y[_varname]);

                if (c == 0 && Child != null)
                {
                    return Child.Compare(x, y);
                }
                else
                {
                    return _modifier * c;
                }
            }
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern.
        /// </summary>
        /// <param name="pattern">Triple Pattern.</param>
        /// <param name="nodeComparer">The comparer to use for node ordering.</param>
        /// <returns></returns>
        public override IComparer<Triple> GetComparer(IMatchTriplePattern pattern, ISparqlNodeComparer nodeComparer)
        {
            var comparer = new SparqlOrderingComparer(nodeComparer);
            IComparer<Triple> child = Child?.GetComparer(pattern, nodeComparer);
            Func<Triple, Triple, int> compareFunc = null;
            if (_varname.Equals(pattern.Subject.VariableName))
            {
                compareFunc = (x, y) => comparer.Compare(x.Subject, y.Subject);
            }
            else if (_varname.Equals(pattern.Predicate.VariableName))
            {
                compareFunc = (x, y) => comparer.Compare(x.Predicate, y.Predicate);
            }
            else if (_varname.Equals(pattern.Object.VariableName))
            {
                compareFunc = (x, y) => comparer.Compare(x.Object, y.Object);
            }

            return compareFunc == null ? null : new TripleComparer(compareFunc, Descending, child);
        }

        /// <summary>
        /// Gets whether the Ordering is Simple.
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return Child == null || Child.IsSimple;
            }
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering.
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                return Child != null
                    ? _varname.AsEnumerable().Concat(Child.Variables).Distinct()
                    : _varname.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the Variable Expression Term used in the Ordering.
        /// </summary>
        public override ISparqlExpression Expression
        {
            get
            {
                return new VariableTerm(_varname); 
            }
        }

        /// <summary>
        /// Gets the String representation of the Order By.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            if (_modifier == -1)
            {
                output.Append("DESC(");
            }
            else
            {
                output.Append("ASC(");
            }
            output.Append("?");
            output.Append(_varname);
            output.Append(")");

            if (Child != null)
            {
                output.Append(" ");
                output.Append(Child.ToString());
            }
            else
            {
                output.Append(" ");
            }

            return output.ToString();
        }
    }

    /// <summary>
    /// An ORDER BY which orders based on the values of a Sparql Expression.
    /// </summary>
    public class OrderByExpression
        : BaseOrderBy
    {
        private readonly ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Order By using the given Expression.
        /// </summary>
        /// <param name="expr">Expression to order by.</param>
        public OrderByExpression(ISparqlExpression expr)
        {
            _expr = expr;
        }

        /// <summary>
        /// Orders the sets based on the values resulting from evaluating the expression for both solutions.
        /// </summary>
        /// <param name="x">A Set.</param>
        /// <param name="y">A Set.</param>
        /// <returns></returns>
        public override int Compare(ISet x, ISet y)
        {
            if (_context == null)
            {
                return 0;
            }
            else if (x.ID == y.ID)
            {
                return 0;
            }
            else
            {
                INode a, b;
                try
                {
                    a = _expr.Evaluate(_context, x.ID);

                    try
                    {
                        b = _expr.Evaluate(_context, y.ID);
                    }
                    catch
                    {
                        // If evaluating b errors consider this a NULL and rank a > b
                        return _modifier * 1;
                    }

                    // If both give a value then compare
                    if (a != null)
                    {
                        var c = _context.OrderingComparer.Compare(a, b);
                        if (c == 0 && Child != null)
                        {
                            return Child.Compare(x, y);
                        }
                        else
                        {
                            return _modifier * c;
                        }
                    }
                    else
                    {
                        // a is NULL so a < b
                        return _modifier * -1;
                    }
                }
                catch
                {
                    try
                    {
                        b = _expr.Evaluate(_context, y.ID);

                        // If evaluating a errors but b evaluates correctly consider a to be NULL and rank a < b
                        return _modifier * -1;
                    }
                    catch
                    {
                        // If both error then use child if any to evaluate, otherwise consider a = b
                        if (Child != null)
                        {
                            return Child.Compare(x, y);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern.
        /// </summary>
        /// <param name="pattern">Triple Pattern.</param>
        /// <param name="nodeComparer">The comparer to use for node ordering.</param>
        /// <returns></returns>
        public override IComparer<Triple> GetComparer(IMatchTriplePattern pattern, ISparqlNodeComparer nodeComparer)
        {
            if (_expr is VariableTerm)
            {
                IComparer<Triple> child = Child?.GetComparer(pattern, nodeComparer);
                Func<Triple, Triple, int> compareFunc = null;
                var var = _expr.Variables.First();
                var comparer = new SparqlOrderingComparer(nodeComparer);
                if (var.Equals(pattern.Subject.VariableName))
                {
                    compareFunc = (x, y) => comparer.Compare(x.Subject, y.Subject);
                }
                else if (var.Equals(pattern.Predicate.VariableName))
                {
                    compareFunc = (x, y) => comparer.Compare(x.Predicate, y.Predicate);
                }
                else if (var.Equals(pattern.Object.VariableName))
                {
                    compareFunc = (x, y) => comparer.Compare(x.Object, y.Object);
                }

                return compareFunc == null ? null : new TripleComparer(compareFunc, Descending, child);
            }

            return null;
        }

        /// <summary>
        /// Gets whether the Ordering is Simple.
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                if (_expr is VariableTerm)
                {
                    // An Expression Ordering can be simple if that expression is a Variable Term
                    // and the Child Ordering (if any) is simple
                    return Child == null || Child.IsSimple;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering.
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                return Child != null ? _expr.Variables.Concat(Child.Variables).Distinct() : _expr.Variables;
            }
        }

        /// <summary>
        /// Gets the Expression used for Ordering.
        /// </summary>
        public override ISparqlExpression Expression
        {
            get
            {
                return _expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the Order By.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append(_modifier == -1 ? "DESC(" : "ASC(");
            output.Append(_expr);
            output.Append(")");

            if (Child != null)
            {
                output.Append(" ");
                output.Append(Child);
            }
            else
            {
                output.Append(" ");
            }

            return output.ToString();
        }
    }
}