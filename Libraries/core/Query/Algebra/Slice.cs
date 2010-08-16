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

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Slice Operation in the SPARQL Algebra
    /// </summary>
    public class Slice : ISparqlAlgebra
    {
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new Slice
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public Slice(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the Slice by applying the appropriate LIMIT and OFFSET to the Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (context.Query.Limit == 0)
            {
                //If Limit is Zero we can skip evaluation
                context.OutputMultiset = new Multiset(context.Query.Variables.Select(v => v.Name));
                return context.OutputMultiset;
            }
            else
            {
                context.InputMultiset = this._pattern.Evaluate(context);

                if (context.Query.Offset > 0)
                {
                    if (context.Query.Offset > context.InputMultiset.Count)
                    {
                        //If the Offset is greater than the count return nothing
                        context.OutputMultiset = new Multiset(context.Query.Variables.Select(v => v.Name));
                        return context.OutputMultiset;
                    }
                    else
                    {
                        //Otherwise discard the relevant number of Bindings
                        foreach (int id in context.InputMultiset.SetIDs.Take(context.Query.Offset).ToList())
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }
                if (context.Query.Limit > 0)
                {
                    if (context.InputMultiset.Count > context.Query.Limit)
                    {
                        foreach (int id in context.InputMultiset.SetIDs.Skip(context.Query.Limit).ToList())
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Slice(" + this._pattern.ToString() + ")";
        }
    }
}
