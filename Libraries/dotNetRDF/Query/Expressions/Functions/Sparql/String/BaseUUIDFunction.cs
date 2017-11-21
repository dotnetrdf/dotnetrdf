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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Abstract Base Class for functions that generate UUIDs
    /// </summary>
    public abstract class BaseUUIDFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            Guid uuid = Guid.NewGuid();
            return EvaluateInternal(uuid);
        }

        /// <summary>
        /// Method to be implemented by derived classes to implement the actual logic of turning the generated UUID into a RDF term
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected abstract IValuedNode EvaluateInternal(Guid uuid);

        /// <summary>
        /// Gets the variables used in the expression
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<System.String>(); 
            }
        }

        /// <summary>
        /// Gets the Type of the expression
        /// </summary>
        public virtual SparqlExpressionType Type
        {
            get
            { 
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Functor of the expression
        /// </summary>
        public abstract string Functor
        {
            get;
        }

        /// <summary>
        /// Gets the arguments of the expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get 
            { 
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Applies the transformer to the arguments of this expression
        /// </summary>
        /// <param name="transformer">Transformer</param>
        /// <returns></returns>
        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }

        /// <summary>
        /// Returns whether the function can be parallelised
        /// </summary>
        public virtual bool CanParallelise
        {
            get 
            {
                return true;
            }
        }
    }
}
