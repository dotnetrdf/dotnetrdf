/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering
{
    /// <summary>
    /// Base Class for implementing Sparql ORDER BYs
    /// </summary>
    public abstract class BaseOrderBy : ISparqlOrderBy
    {
        /// <summary>
        /// Holds the Child Order By (if any)
        /// </summary>
        protected ISparqlOrderBy _child = null;
        /// <summary>
        /// Stores the Evaluation Context
        /// </summary>
        protected SparqlEvaluationContext _context = null;
        /// <summary>
        /// Modifier used to make ordering Descending
        /// </summary>
        /// <remarks>Implementations derived from this class should multiply their comparison results by the modifier to automatically provide Ascending/Descending order</remarks>
        protected int _modifier = 1;

        /// <summary>
        /// Gets/Sets the Child Order By
        /// </summary>
        public ISparqlOrderBy Child
        {
            get
            {
                return this._child;
            }
            set
            {
                this._child = value;
            }
        }

        /// <summary>
        /// Sets the Evaluation Context for the Ordering
        /// </summary>
        public SparqlEvaluationContext Context
        {
            set
            {
                this._context = value;
                if (this._child != null)
                {
                    this._child.Context = value;
                }
            }
        }

        /// <summary>
        /// Sets the Ordering to Descending
        /// </summary>
        public bool Descending
        {
            get
            {
                return (this._modifier == -1);
            }
            set
            {
                if (value)
                {
                    this._modifier = -1;
                }
                else
                {
                    this._modifier = 1;
                }
            }
        }

        /// <summary>
        /// Gets whether the Ordering is Simple
        /// </summary>
        public abstract bool IsSimple
        {
            get;
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering
        /// </summary>
        public abstract IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used in the Ordering
        /// </summary>
        public abstract ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Abstract Compare method which derived classes should implement their ordering in
        /// </summary>
        /// <param name="x">A Set</param>
        /// <param name="y">A Set</param>
        /// <returns></returns>
        public abstract int Compare(ISet x, ISet y);

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern
        /// </summary>
        /// <param name="pattern">Triple Pattern</param>
        /// <returns></returns>
        public abstract IComparer<Triple> GetComparer(TriplePattern pattern);

        /// <summary>
        /// Gets the String representation of the Order By
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// An ORDER BY which orders on the values bound to a particular variable
    /// </summary>
    public class OrderByVariable : BaseOrderBy
    {
        private SparqlOrderingComparer _comparer = new SparqlOrderingComparer();
        private String _varname = String.Empty;

        /// <summary>
        /// Creates a new Ordering based on the Value of a given Variable
        /// </summary>
        /// <param name="name">Variable to order upon</param>
        public OrderByVariable(String name)
        {
            this._varname = name.Substring(1);
        }

        /// <summary>
        /// Compares Sets on the basis of their values for the Variable the class was instaniated with
        /// </summary>
        /// <param name="x">A Set</param>
        /// <param name="y">A Set</param>
        /// <returns></returns>
        public override int Compare(ISet x, ISet y)
        {
            INode xval;
            xval = x[this._varname];
            if (xval == null)
            {
                if (y[this._varname] == null)
                {
                    return 0;
                }
                else
                {
                    return this._modifier * -1;
                }
            }
            else
            {
                //int c = xval.CompareTo(y[this._varname]);
                int c = this._comparer.Compare(xval, y[this._varname]);

                if (c == 0 && this._child != null)
                {
                    return this._modifier * this._child.Compare(x, y);
                }
                else
                {
                    return this._modifier * c;
                }
            }
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern
        /// </summary>
        /// <param name="pattern">Triple Pattern</param>
        /// <returns></returns>
        public override IComparer<Triple> GetComparer(TriplePattern pattern)
        {
            IComparer<Triple> child = (this._child == null) ? null : this._child.GetComparer(pattern);
            Func<Triple, Triple, int> compareFunc = null;
            if (this._varname.Equals(pattern.Subject.VariableName))
            {
                compareFunc = (x, y) => this._comparer.Compare(x.Subject, y.Subject);
            }
            else if (this._varname.Equals(pattern.Predicate.VariableName))
            {
                compareFunc = (x, y) => this._comparer.Compare(x.Predicate, y.Predicate);
            }
            else if (this._varname.Equals(pattern.Object.VariableName))
            {
                compareFunc = (x, y) => this._comparer.Compare(x.Object, y.Object);
            }

            if (compareFunc == null) return null;
            return new TripleComparer(compareFunc, this.Descending, child);
        }

        /// <summary>
        /// Gets whether the Ordering is Simple
        /// </summary>
        public override bool IsSimple
        {
            get 
            {
                if (this._child != null)
                {
                    //An ordering on a Variable is always simple so whether the Ordering is simple
                    //depends on whether the Child Ordering is simple
                    return this._child.IsSimple;
                }
                else
                {
                    //An ordering on a Variable is always simple
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                if (this._child != null)
                {
                    return this._varname.AsEnumerable<String>().Concat(this._child.Variables).Distinct();
                }
                else
                {
                    return this._varname.AsEnumerable<String>();
                }
            }
        }

        /// <summary>
        /// Gets the Variable Expression Term used in the Ordering
        /// </summary>
        public override ISparqlExpression Expression
        {
            get
            {
                return new VariableExpressionTerm(this._varname); 
            }
        }

        /// <summary>
        /// Gets the String representation of the Order By
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._modifier == -1)
            {
                output.Append("DESC(");
            }
            else
            {
                output.Append("ASC(");
            }
            output.Append("?");
            output.Append(this._varname);
            output.Append(")");

            if (this._child != null)
            {
                output.Append(" ");
                output.Append(this._child.ToString());
            }
            else
            {
                output.Append(" ");
            }

            return output.ToString();
        }
    }

    /// <summary>
    /// An ORDER BY which orders based on the values of a Sparql Expression
    /// </summary>
    public class OrderByExpression : BaseOrderBy
    {
        private SparqlOrderingComparer _comparer = new SparqlOrderingComparer();
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Order By using the given Expression
        /// </summary>
        /// <param name="expr">Expression to order by</param>
        public OrderByExpression(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Orders the sets based on the values resulting from evaluating the expression for both solutions
        /// </summary>
        /// <param name="x">A Set</param>
        /// <param name="y">A Set</param>
        /// <returns></returns>
        public override int Compare(ISet x, ISet y)
        {
            if (this._context == null)
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
                    a = this._expr.Value(this._context, x.ID);

                    try
                    {
                        b = this._expr.Value(this._context, y.ID);
                    }
                    catch
                    {
                        //If evaluating b errors consider this a NULL and rank a > b
                        return this._modifier * 1;
                    }

                    //If both give a value then compare
                    if (a != null)
                    {
                        int c = this._comparer.Compare(a, b);
                        if (c == 0 && this._child != null)
                        {
                            return this._modifier * this._child.Compare(x, y);
                        }
                        else
                        {
                            return this._modifier * c;
                        }
                    }
                    else
                    {
                        //a is NULL so a < b
                        return this._modifier * -1;
                    }
                }
                catch
                {
                    //If evaluating a errors consider this a NULL and rank a < b
                    return this._modifier * -1;
                }

            }
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern
        /// </summary>
        /// <param name="pattern">Triple Pattern</param>
        /// <returns></returns>
        public override IComparer<Triple> GetComparer(TriplePattern pattern)
        {
            if (this._expr is VariableExpressionTerm)
            {
                IComparer<Triple> child = (this._child == null) ? null : this._child.GetComparer(pattern);
                Func<Triple, Triple, int> compareFunc = null;
                String var = this._expr.Variables.First();
                if (var.Equals(pattern.Subject.VariableName))
                {
                    compareFunc = (x, y) => this._comparer.Compare(x.Subject, y.Subject);
                }
                else if (var.Equals(pattern.Predicate.VariableName))
                {
                    compareFunc = (x, y) => this._comparer.Compare(x.Predicate, y.Predicate);
                }
                else if (var.Equals(pattern.Object.VariableName))
                {
                    compareFunc = (x, y) => this._comparer.Compare(x.Object, y.Object);
                }

                if (compareFunc == null) return null;
                return new TripleComparer(compareFunc, this.Descending, child);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets whether the Ordering is Simple
        /// </summary>
        public override bool IsSimple
        {
            get 
            {
                if (this._expr is VariableExpressionTerm)
                {
                    //An Expression Ordering can be simple if that expression is a Variable Term
                    //and the Child Ordering (if any) is simple
                    if (this._child != null)
                    {
                        return this._child.IsSimple;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (this._child != null)
                {
                    return this._expr.Variables.Concat(this._child.Variables).Distinct();
                }
                else
                {
                    return this._expr.Variables;
                }
            }
        }

        /// <summary>
        /// Gets the Expression used for Ordering
        /// </summary>
        public override ISparqlExpression Expression
        {
            get
            {
                return this._expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the Order By
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._modifier == -1)
            {
                output.Append("DESC(");
            }
            else
            {
                output.Append("ASC(");
            }
            output.Append(this._expr.ToString());
            output.Append(")");

            if (this._child != null)
            {
                output.Append(" ");
                output.Append(this._child.ToString());
            }
            else
            {
                output.Append(" ");
            }

            return output.ToString();
        }
    }
}