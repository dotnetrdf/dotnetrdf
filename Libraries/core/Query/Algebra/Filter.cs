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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Filter
    /// </summary>
    public class Filter : IFilter
    {
        private ISparqlAlgebra _pattern;
        private ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Filter
        /// </summary>
        /// <param name="pattern">Algebra the Filter applies over</param>
        /// <param name="filter">Filter to apply</param>
        public Filter(ISparqlAlgebra pattern, ISparqlFilter filter)
        {
            this._pattern = pattern;
            this._filter = filter;
        }

        /// <summary>
        /// Applies the Filter over the results of evaluating the inner pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Apply the Pattern first
            context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

            if (context.InputMultiset is NullMultiset)
            {
                //If we get a NullMultiset then the FILTER has no effect since there are already no results
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                if (this._filter.Variables.Any())
                {
                    //If we get an IdentityMultiset then the FILTER only has an effect if there are no
                    //variables - otherwise it is not in scope and causes the Output to become Null
                    context.InputMultiset = new NullMultiset();
                }
                else
                {
                    try
                    {
                        if (!this._filter.Expression.EffectiveBooleanValue(context, 0))
                        {
                            context.OutputMultiset = new NullMultiset();
                            return context.OutputMultiset;
                        }
                    }
                    catch
                    {
                        context.OutputMultiset = new NullMultiset();
                        return context.OutputMultiset;
                    }
                }
            }
            else
            {
                this._filter.Evaluate(context);
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
                return (this._pattern.Variables.Concat(this._filter.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the Filter to be used
        /// </summary>
        public ISparqlFilter SparqlFilter
        {
            get
            {
                return this._filter;
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
        /// Gets the String representation of the FILTER
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String filter = this._filter.ToString();
            filter = filter.Substring(7, filter.Length - 8);
            return "Filter(" + this._pattern.ToString() + ", " + filter + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = this._pattern.ToGraphPattern();
            GraphPattern f = new GraphPattern();
            f.AddFilter(this._filter);
            p.AddGraphPattern(f);
            return p;
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            if (optimiser is IExpressionTransformer)
            {
                return new Filter(optimiser.Optimise(this._pattern), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(this._filter.Expression)));
            }
            else
            {
                return new Filter(optimiser.Optimise(this._pattern), this._filter);
            }
        }
    }
}
