/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Filters
{
    /// <summary>
    /// Abstract Base class for Unary Filters that operate on a single Expression
    /// </summary>
    public abstract class BaseUnaryFilter 
        : ISparqlFilter 
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
            _arg = arg;
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
                return _arg.Variables;
            }
        }

        /// <summary>
        /// Gets the inner expression this Filter uses
        /// </summary>
        public virtual ISparqlExpression Expression
        {
            get
            {
                return _arg;
            }
        }
    }

    /// <summary>
    /// Filter that represents the Sparql BOUND() function
    /// </summary>
    public class BoundFilter 
        : BaseUnaryFilter
    {
        /// <summary>
        /// Creates a new Bound Filter
        /// </summary>
        /// <param name="varTerm">Variable Expression</param>
        public BoundFilter(VariableTerm varTerm)
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
                // If the Input is the Identity Multiset then nothing is Bound so return the Null Multiset
                context.InputMultiset = new NullMultiset();
                return;
            }

#if NET40
            // BOUND is always safe to parallelise
            if (Options.UsePLinqEvaluation)
            {
                context.InputMultiset.SetIDs.ToList().AsParallel().ForAll(i => EvalFilter(context, i));
            }
            else
            {
                foreach (int id in context.InputMultiset.SetIDs.ToList())
                {
                    EvalFilter(context, id);
                }
            }
#else
            foreach (int id in context.InputMultiset.SetIDs.ToList())
            {
                EvalFilter(context, id);
            }
#endif
        }

        private void EvalFilter(SparqlEvaluationContext context, int id)
        {
            try
            {
                if (_arg.Evaluate(context, id) == null)
                {
                    context.InputMultiset.Remove(id);
                }
            }
            catch
            {
                context.InputMultiset.Remove(id);
            }
        }

        /// <summary>
        /// Gets the String representation of the Filter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FILTER(BOUND(" + _arg.ToString() + ")) ";
        }
    }

    /// <summary>
    /// Generic Filter for Filters which take a single sub-expression as an argument
    /// </summary>
    public class UnaryExpressionFilter
        : BaseUnaryFilter
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
                if (!Variables.Any())
                {
                    // If the Filter has no variables and is applied to an Identity Multiset then if the
                    // Filter Expression evaluates to False then the Null Multiset is returned
                    try
                    {
                        if (!_arg.Evaluate(context, 0).AsSafeBoolean())
                        {
                            context.InputMultiset = new NullMultiset();
                        }
                    }
                    catch
                    {
                        // Error is treated as false for Filters so Null Multiset is returned
                        context.InputMultiset = new NullMultiset();
                    }
                }
                else
                {
                    // As no variables are in scope the effect is that the Null Multiset is returned
                    context.InputMultiset = new NullMultiset();
                }
            }
            else
            {
#if NET40
                // Remember that not all expressions are safe to parallelise
                if (Options.UsePLinqEvaluation && this._arg.CanParallelise)
                {
                    context.InputMultiset.SetIDs.ToList().AsParallel().ForAll(i => EvalFilter(context, i));
                }
                else
                {
#endif
                    foreach (int id in context.InputMultiset.SetIDs.ToList())
                    {
                        EvalFilter(context, id);
                    }
#if NET40
                }
#endif
            }
        }

        private void EvalFilter(SparqlEvaluationContext context, int id)
        {
            try
            {
                if (!_arg.Evaluate(context, id).AsSafeBoolean())
                {
                    context.InputMultiset.Remove(id);
                }
            }
            catch
            {
                context.InputMultiset.Remove(id);
            }
        }

        /// <summary>
        /// Gets the String representation of the Filter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FILTER(" + _arg.ToString() + ") ";
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
            _filters.Add(first);
            _filters.Add(second);
        }

        /// <summary>
        /// Creates a new Chain Filter
        /// </summary>
        /// <param name="filters">Filters</param>
        public ChainFilter(IEnumerable<ISparqlFilter> filters)
        {
            _filters.AddRange(filters);
        }

        /// <summary>
        /// Creates a new Chain Filter
        /// </summary>
        /// <param name="first">First Filter</param>
        /// <param name="rest">Additional Filters</param>
        public ChainFilter(ISparqlFilter first, IEnumerable<ISparqlFilter> rest)
        {
            _filters.Add(first);
            _filters.AddRange(rest);
        }

        /// <summary>
        /// Evaluates a filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset) return;

            foreach (ISparqlFilter filter in _filters)
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
            _filters.Add(filter);
        }

        /// <summary>
        /// Gets the String representation of the Filters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (ISparqlFilter filter in _filters)
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
                return (from f in _filters
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
                ISparqlExpression expr = new ConstantTerm(new BooleanNode(null, true));
                if (_filters.Count == 1)
                {
                    expr = _filters[0].Expression;
                }
                else if (_filters.Count >= 2)
                {
                    expr = new AndExpression(_filters[0].Expression, _filters[1].Expression);
                    if (_filters.Count > 2)
                    {
                        for (int i = 2; i < _filters.Count; i++)
                        {
                            expr = new AndExpression(expr, _filters[i].Expression);
                        }
                    }
                }
                return expr;
            }
        }
    }
}