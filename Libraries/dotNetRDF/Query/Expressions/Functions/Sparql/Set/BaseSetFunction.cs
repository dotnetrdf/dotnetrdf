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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Abstract base class for SPARQL Functions which operate on Sets
    /// </summary>
    public abstract class BaseSetFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Variable Expression Term that the Set function applies to
        /// </summary>
        protected ISparqlExpression _expr;
        /// <summary>
        /// Set that is used in the function
        /// </summary>
        protected List<ISparqlExpression> _expressions = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new SPARQL Set function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        public BaseSetFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
        {
            _expr = expr;
            _expressions.AddRange(set);
        }

        /// <summary>
        /// Gets the value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Variable the function applies to
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return _expr.Variables.Concat(from e in _expressions
                                                   from v in e.Variables
                                                   select v);
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.SetOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract string Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Exception
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return _expr.AsEnumerable<ISparqlExpression>().Concat(_expressions);
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _expr.CanParallelise;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }
}
