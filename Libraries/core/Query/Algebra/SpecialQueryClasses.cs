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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Special Algebra Construct for optimising queries of the form SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
    /// </summary>
    public class SelectDistinctGraphs : ISparqlAlgebra
    {
        /// <summary>
        /// Evaluates the Select Distinct Graphs optimisation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.OutputMultiset = new Multiset();
            String var = context.Query.Variables.First(v => v.IsResultVariable).Name;

            foreach (IGraph g in context.Data.Graphs)
            {
                Set s = new Set();
                if (g.BaseUri == null)
                {
                    s.Add(var, null);
                }
                else
                {
                    s.Add(var, g.CreateUriNode());
                }
                context.OutputMultiset.Add(s);
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SelectDistinctGraphs()";
        }
    }
}
