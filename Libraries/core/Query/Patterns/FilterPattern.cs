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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Filter Patterns in SPARQL Queries
    /// </summary>
    /// <remarks>
    /// A Filter Pattern is any FILTER clause that can be executed during the process of executing Triple Patterns rather than after all the Triple Patterns and Child Graph Patterns have been executed
    /// </remarks>
    public class FilterPattern : BaseTriplePattern
    {
        private ISparqlFilter _filter;

        /// <summary>
        /// Creates a new Filter Pattern with the given Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        public FilterPattern(ISparqlFilter filter)
        {
            this._filter = filter;
            this._indexType = TripleIndexType.SpecialFilter;
            this._vars = filter.Variables.Distinct().ToList();
            this._vars.Sort();
        }

        /// <summary>
        /// Evaluates a Filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset)
            {
                //If we get a NullMultiset then the FILTER has no effect since there are already no results
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                if (!this._filter.Variables.Any())
                {
                    //If we get an IdentityMultiset then the FILTER only has an effect if there are no
                    //variables - otherwise it is not in scope and is ignored

                    try
                    {
                        if (!this._filter.Expression.EffectiveBooleanValue(context, 0))
                        {
                            context.OutputMultiset = new NullMultiset();
                            return;
                        }
                    }
                    catch
                    {
                        context.OutputMultiset = new NullMultiset();
                        return;
                    }
                }
            }
            else
            {
                this._filter.Evaluate(context);
            }
            context.OutputMultiset = new IdentityMultiset();
        }

        /// <summary>
        /// Returns that the Pattern is not an accept all (since it's a Filter)
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Filter that this Pattern uses
        /// </summary>
        public ISparqlFilter Filter
        {
            get
            {
                return this._filter;
            }
        }

        /// <summary>
        /// Gets whether the Pattern uses the Default Dataset
        /// </summary>
        public override bool UsesDefaultDataset
        {
            get
            {
                return this._filter.Expression.UsesDefaultDataset();
            }
        }

        /// <summary>
        /// Returns the string representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._filter.ToString();
        }
    }
}
