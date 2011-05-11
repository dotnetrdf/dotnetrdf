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
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Query.Filters
{
    /// <summary>
    /// Abstract Base class for Unary Filters that operate on a single Expression
    /// </summary>
    public abstract class BaseUnaryFilter : ISparqlFilter 
    {
        /// <summary>
        /// Expression which is the Argument to the Filter
        /// </summary>
        protected ISparqlExpression _arg;

        /// <summary>
        /// Creates a new Base Unary Filter
        /// </summary>
        /// <param name="arg">Argument to the Filter</param>
        public BaseUnaryFilter(ISparqlExpression arg)
        {
            this._arg = arg;
        }

        /// <summary>
        /// Evaluates a filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public abstract void Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the String representation of the Filter
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the enumeration of Variables used in the Filter
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return this._arg.Variables;
            }
        }

        /// <summary>
        /// Gets the inner expression this Filter uses
        /// </summary>
        public virtual ISparqlExpression Expression
        {
            get
            {
                return this._arg;
            }
        }
    }

    /// <summary>
    /// Filter that represents the Sparql BOUND() function
    /// </summary>
    public class BoundFilter : BaseUnaryFilter
    {
        /// <summary>
        /// Creates a new Bound Filter
        /// </summary>
        /// <param name="varTerm">Variable Expression</param>
        public BoundFilter(VariableExpressionTerm varTerm)
            : base(varTerm) { }

        /// <summary>
        /// Evaluates a filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset) return;
            if (context.InputMultiset is IdentityMultiset)
            {
                //If the Input is the Identity Multiset then nothing is Bound so return the Null Multiset
                context.InputMultiset = new NullMultiset();
                return;
            }

            foreach (int id in context.InputMultiset.SetIDs.ToList())
            {
                try
                {
                    if (this._arg.Value(context, id) == null)
                    {
                        context.InputMultiset.Remove(id);
                    }
                }
                catch
                {
                    context.InputMultiset.Remove(id);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the Filter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FILTER(BOUND(" + this._arg.ToString() + ")) ";
        }
    }

    /// <summary>
    /// Generic Filter for Filters which take a single sub-expression as an argument
    /// </summary>
    public class UnaryExpressionFilter : BaseUnaryFilter
    {
        /// <summary>
        /// Creates a new Unary Expression Filter which filters on the basis of a single sub-expression
        /// </summary>
        /// <param name="expr">Expression to filter with</param>
        public UnaryExpressionFilter(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Evaluates a filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset) return;

            if (context.InputMultiset is IdentityMultiset)
            {
                if (!this.Variables.Any())
                {
                    //If the Filter has no variables and is applied to an Identity Multiset then if the
                    //Filter Expression evaluates to False then the Null Multiset is returned
                    try
                    {
                        if (!this._arg.EffectiveBooleanValue(context, 0))
                        {
                            context.InputMultiset = new NullMultiset();
                        }
                    }
                    catch
                    {
                        //Error is treated as false for Filters so Null Multiset is returned
                        context.InputMultiset = new NullMultiset();
                    }
                }
                else
                {
                    //As no variables are in scope the effect is that the Null Multiset is returned
                    context.InputMultiset = new NullMultiset();
                }
            }
            else
            {
                foreach (int id in context.InputMultiset.SetIDs.ToList())
                {
                    try
                    {
                        if (!this._arg.EffectiveBooleanValue(context, id))
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                    catch
                    {
                        context.InputMultiset.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the Filter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FILTER(" + this._arg.ToString() + ") ";
        }
    }

    /// <summary>
    /// Generic Filter for use where multiple Filters are applied on a single Graph Pattern
    /// </summary>
    public class ChainFilter : ISparqlFilter
    {
        private List<ISparqlFilter> _filters = new List<ISparqlFilter>();

        /// <summary>
        /// Creates a new Chain Filter
        /// </summary>
        /// <param name="first">First Filter</param>
        /// <param name="second">Second Filter</param>
        public ChainFilter(ISparqlFilter first, ISparqlFilter second)
        {
            this._filters.Add(first);
            this._filters.Add(second);
        }

        /// <summary>
        /// Creates a new Chain Filter
        /// </summary>
        /// <param name="filters">Filters</param>
        public ChainFilter(IEnumerable<ISparqlFilter> filters)
        {
            this._filters.AddRange(filters);
        }

        /// <summary>
        /// Creates a new Chain Filter
        /// </summary>
        /// <param name="first">First Filter</param>
        /// <param name="rest">Additional Filters</param>
        public ChainFilter(ISparqlFilter first, IEnumerable<ISparqlFilter> rest)
        {
            this._filters.Add(first);
            this._filters.AddRange(rest);
        }

        /// <summary>
        /// Evaluates a filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset) return;

            foreach (ISparqlFilter filter in this._filters)
            {
                filter.Evaluate(context);
            }
        }

        /// <summary>
        /// Adds an additional Filter to the Filter Chain
        /// </summary>
        /// <param name="filter">A Filter to add</param>
        protected internal void Add(ISparqlFilter filter)
        {
            this._filters.Add(filter);
        }

        /// <summary>
        /// Gets the String representation of the Filters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (ISparqlFilter filter in this._filters)
            {
                output.Append(filter.ToString() + " ");
            }
            return output.ToString();
        }

        /// <summary>
        /// Gets the enumeration of Variables used in the chained Filters
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from f in this._filters
                        from v in f.Variables
                        select v);
            }
        }

        /// <summary>
        /// Gets the Inner Expression used by the Chained Filters
        /// </summary>
        /// <remarks>
        /// Equivalent to ANDing all the Chained Filters expressions
        /// </remarks>
        public ISparqlExpression Expression
        {
            get
            {
                ISparqlExpression expr = new BooleanExpressionTerm(true);
                if (this._filters.Count == 1)
                {
                    expr = this._filters[0].Expression;
                }
                else if (this._filters.Count >= 2)
                {
                    expr = new AndExpression(this._filters[0].Expression, this._filters[1].Expression);
                    if (this._filters.Count > 2)
                    {
                        for (int i = 2; i < this._filters.Count; i++)
                        {
                            expr = new AndExpression(expr, this._filters[i].Expression);
                        }
                    }
                }
                return expr;
            }
        }
    }
}