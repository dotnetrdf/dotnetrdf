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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// Abstract Base Class for Aggregate Functions
    /// </summary>
    public abstract class BaseAggregate 
        : ISparqlAggregate
    {
        /// <summary>
        /// Expression that the aggregate operates over
        /// </summary>
        protected ISparqlExpression _expr;
        /// <summary>
        /// Whether a DISTINCT modifer is applied
        /// </summary>
        protected bool _distinct = false;

        /// <summary>
        /// Base Constructor for Aggregates
        /// </summary>
        /// <param name="expr">Expression that the aggregate is over</param>
        public BaseAggregate(ISparqlExpression expr)
        {
            _expr = expr;
        }

        /// <summary>
        /// Base Constructor for Aggregates
        /// </summary>
        /// <param name="expr">Expression that the aggregate is over</param>
        /// <param name="distinct">Whether a Distinct modifer is applied</param>
        public BaseAggregate(ISparqlExpression expr, bool distinct)
            : this(expr)
        {
            _distinct = distinct;
        }

        /// <summary>
        /// Applies the Aggregate to the Result Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public IValuedNode Apply(SparqlEvaluationContext context)
        {
            return Apply(context, context.Binder.BindingIDs);
        }

        /// <summary>
        /// Applies the Aggregate to the Result Binder
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Enumerable of Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public abstract IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs);

        /// <summary>
        /// Expression that the Aggregate executes over
        /// </summary>
        public ISparqlExpression Expression
        {
            get
            {
                return _expr;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Aggregate;
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _expr.AsEnumerable();
            }
        }
    }
}
