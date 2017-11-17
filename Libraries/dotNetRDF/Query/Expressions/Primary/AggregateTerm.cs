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
using VDS.RDF.Query.Aggregates;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class for representing Aggregate Expressions which have Numeric Results
    /// </summary>
    public class AggregateTerm 
        : BaseUnaryExpression
    {
        private ISparqlAggregate _aggregate;

        /// <summary>
        /// Creates a new Aggregate Expression Term that uses the given Aggregate
        /// </summary>
        /// <param name="aggregate">Aggregate</param>
        public AggregateTerm(ISparqlAggregate aggregate)
            : base(null)
        {
            _aggregate = aggregate;
        }

        /// <summary>
        /// Evaluates the aggregate expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode aggValue;
            if (context.Binder.IsGroup(bindingID))
            {
                BindingGroup group = context.Binder.Group(bindingID);
                context.Binder.SetGroupContext(true);
                aggValue = _aggregate.Apply(context, group.BindingIDs);
                context.Binder.SetGroupContext(false);
            }
            else
            {
                aggValue = _aggregate.Apply(context);
            }
            return aggValue;
        }

        /// <summary>
        /// Gets the Aggregate this Expression represents
        /// </summary>
        public ISparqlAggregate Aggregate
        {
            get
            {
                return _aggregate;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _aggregate.ToString();
        }

        /// <summary>
        /// Gets the enumeration of variables that are used in the the aggregate expression
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return _aggregate.Expression.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Aggregate;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return _aggregate.Functor;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _aggregate.Arguments;
            }
        }

        /// <summary>
        /// Gets whether the expression can be parallelised
        /// </summary>
        public override bool CanParallelise
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
