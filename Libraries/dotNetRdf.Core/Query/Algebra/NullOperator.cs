/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a part of the algebra that has been determined to not return any results in advance and so can be replaced with this operator which always returns null.
    /// </summary>
    /// <remarks>
    /// Primarily intended for use with Algebra Optimisers which are rewriting the algebra to run against an out of memory dataset (e.g. SQL based) where it may be easily possible to determine if a triple pattern will match in advance of actually returning the matches.
    /// </remarks>
    public class NullOperator 
        : ISparqlAlgebra, ITerminalOperator
    {
        private List<string> _vars = new List<string>();

        /// <summary>
        /// Creates a new Null Operator.
        /// </summary>
        /// <param name="variables">Variables in the algebra that this null is replacing.</param>
        public NullOperator(IEnumerable<string> variables)
        {
            _vars.AddRange(variables);
        }


        /// <summary>
        /// Gets the variables used in this algebra.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return _vars;
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<string> FixedVariables
        {
            get { return Enumerable.Empty<string>(); }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<string> FloatingVariables
        {
            get { return Enumerable.Empty<string>(); }
        }

        /// <summary>
        /// Throws an error since a null operator cannot be transformed back into a query.
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed back to a Query");
        }

        /// <summary>
        /// Throws an error since a null operator cannot be transformed back into a query.
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed into a Graph Pattern");
        }

        /// <summary>
        /// Gets the string representation of the algebra.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NullOperator()";
        }

        public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
        {
            return processor.ProcessNullOperator(this, context);
        }

        public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
        {
            return visitor.VisitNullOperator(this);
        }
    }
}
