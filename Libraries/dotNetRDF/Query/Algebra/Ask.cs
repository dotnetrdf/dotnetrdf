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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Ask step of Query Evaluation
    /// </summary>
    /// <remarks>
    /// Used only for ASK queries.  Turns the final Multiset into either an <see cref="IdentityMultiset">IdentityMultiset</see> if the ASK succeeds or a <see cref="NullMultiset">NullMultiset</see> if the ASK fails
    /// </remarks>
    public class Ask 
        : IUnaryOperator
    {
        private readonly ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new ASK
        /// </summary>
        /// <param name="pattern">Inner Pattern</param>
        public Ask(ISparqlAlgebra pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        /// Evaluates the ASK by turning the Results of evaluating the Inner Pattern to either an Identity/Null Multiset depending on whether there were any Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                context.InputMultiset = context.Evaluate(_pattern);
            }
            catch (RdfQueryTimeoutException)
            {
                if (!context.Query.PartialResultsOnTimeout) throw;
            }

            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else
            {
                if (context.InputMultiset.IsEmpty)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return _pattern;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return _pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return _pattern.FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _pattern.FixedVariables; } }

        /// <summary>
        /// Gets the String representation of the Ask
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Ask(" + _pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = _pattern.ToQuery();
            q.QueryType = SparqlQueryType.Ask;
            return q;
        }

        /// <summary>
        /// Throws an exception since an Ask() cannot be converted to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since an Ask() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("An Ask() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Ask(optimiser.Optimise(_pattern));
        }
    }
}
