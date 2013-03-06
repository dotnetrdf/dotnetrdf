/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Having Clause
    /// </summary>
    public class Having
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private ISparqlFilter _having;

        /// <summary>
        /// Creates a new Having Clause
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="having">Having Clause</param>
        public Having(ISparqlAlgebra pattern, ISparqlFilter having)
        {
            this._pattern = pattern;
            this._having = having;
        }

        /// <summary>
        /// Evaluates the Having Clause
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

            if (context.Query != null)
            {
                if (context.Query.Having != null)
                {
                    context.Query.Having.Evaluate(context);
                }
            }
            else if (this._having != null)
            {
                this._having.Evaluate(context);
            }

            context.OutputMultiset = context.InputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the HAVING clause used
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null and has a HAVING clause then that is applied rather than the clause with which the Having algebra is instantiated
        /// </remarks>
        public ISparqlFilter HavingClause
        {
            get
            {
                return this._having;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Having(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            if (this._having != null)
            {
                q.Having = this._having;
            }
            return q;
        }

        /// <summary>
        /// Throws an exception since a Having() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Having() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Having() cannot be converted to a Graph Pattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Having(optimiser.Optimise(this._pattern), this._having);
        }
    }
}
