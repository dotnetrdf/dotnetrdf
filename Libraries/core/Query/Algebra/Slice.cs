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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Slice Operation in the SPARQL Algebra
    /// </summary>
    public class Slice : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private int _limit = -1, _offset = 0;
        private bool _detectSettings = true;

        /// <summary>
        /// Creates a new Slice modifier which will detect LIMIT and OFFSET from the query
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public Slice(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Creates a new Slice modifier which uses a specific LIMIT and OFFSET
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="limit">Limit</param>
        /// <param name="offset">Offset</param>
        public Slice(ISparqlAlgebra pattern, int limit, int offset)
            : this(pattern)
        {
            this._limit = Math.Max(-1, limit);
            this._offset = Math.Max(0, offset);
            this._detectSettings = false;
        }

        /// <summary>
        /// Evaluates the Slice by applying the appropriate LIMIT and OFFSET to the Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Detect the Offset and Limit from the Query if required
            int limit = this._limit, offset = this._offset;
            if (this._detectSettings)
            {
                if (context.Query != null)
                {
                    limit = Math.Max(-1, context.Query.Limit);
                    offset = Math.Max(0, context.Query.Offset);
                }
            }

            IEnumerable<String> vars;
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                vars = (context.Query != null) ? context.Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name) : context.InputMultiset.Variables;
            }
            else
            {
                vars = context.InputMultiset.Variables;
            }

            if (limit == 0)
            {
                //If Limit is Zero we can skip evaluation
                context.OutputMultiset = new Multiset(vars);
                return context.OutputMultiset;
            }
            else
            {
                context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

                if (offset > 0)
                {
                    if (offset > context.InputMultiset.Count)
                    {
                        //If the Offset is greater than the count return nothing
                        context.OutputMultiset = new Multiset(vars);
                        return context.OutputMultiset;
                    }
                    else
                    {
                        //Otherwise discard the relevant number of Bindings
                        foreach (int id in context.InputMultiset.SetIDs.Take(offset).ToList())
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }
                if (limit > 0)
                {
                    if (context.InputMultiset.Count > limit)
                    {
                        foreach (int id in context.InputMultiset.SetIDs.Skip(limit).ToList())
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
        /// Gets the Limit in use (-1 indicates no Limit)
        /// </summary>
        public int Limit
        {
            get
            {
                return this._limit;
            }
        }

        /// <summary>
        /// Gets the Offset in use (0 indicates no Offset)
        /// </summary>
        public int Offset
        {
            get
            {
                return this._offset;
            }
        }

        /// <summary>
        /// Gets whether the Algebra will detect the Limit and Offset to use from the provided query
        /// </summary>
        public bool DetectFromQuery
        {
            get
            {
                return this._detectSettings;
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
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Slice(" + this._pattern.ToString() + ", LIMIT " + this._limit + ", OFFSET " + this._offset + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            q.Limit = this._limit;
            q.Offset = this._offset;
            return q;
        }

        /// <summary>
        /// Throws an exception since a Slice() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Slice() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Slice() cannot be converted to a Graph Pattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Slice(this._pattern, this._limit, this._offset);
        }
    }
}
