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
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra Operator which provides full text query capabilities for a query
    /// </summary>
    /// <remarks>
    /// The evaluation of this operator simply registers the search provider with the Evaluation Context such that any <see cref="FullTextMatchPropertyFunction"/> instances are honoured
    /// </remarks>
    public class FullTextQuery
        : IUnaryOperator
    {
        private readonly IFullTextSearchProvider _provider;

        /// <summary>
        /// Creates a new Full Text Query algebra
        /// </summary>
        /// <param name="searchProvider">Search Provider</param>
        /// <param name="algebra">Inner Algebra</param>
        public FullTextQuery(IFullTextSearchProvider searchProvider, ISparqlAlgebra algebra)
        {
            this._provider = searchProvider;
            this.InnerAlgebra = algebra;
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get;
            private set;
        }

        /// <summary>
        /// Transforms the algebra
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(Optimisation.IAlgebraOptimiser optimiser)
        {
            return new FullTextQuery(this._provider, optimiser.Optimise(this.InnerAlgebra));
        }

        /// <summary>
        /// Evaluates the algebra
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context[FullTextHelper.ContextKey] = this._provider;
            return context.Evaluate(this.InnerAlgebra);
        }

        /// <summary>
        /// Gets the variables used in the algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this.InnerAlgebra.Variables;
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return this.InnerAlgebra.FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return this.InnerAlgebra.FixedVariables; } } 

        /// <summary>
        /// Converts the algebra into a query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            return this.InnerAlgebra.ToQuery();
        }

        /// <summary>
        /// Converts the algebra into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public Patterns.GraphPattern ToGraphPattern()
        {
            return this.InnerAlgebra.ToGraphPattern();
        }

        /// <summary>
        /// Gets the string representaiton of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FullTextQuery(" + this.InnerAlgebra.ToString() + ")";
        }
    }
}
