/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Negated Property Set in the SPARQL Algebra
    /// </summary>
    public class NegatedPropertySet : ISparqlAlgebra
    {
        private List<INode> _properties = new List<INode>();
        private PatternItem _start, _end;
        private bool _inverse;

        /// <summary>
        /// Creates a new Negated Property Set
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="properties">Negated Properties</param>
        /// <param name="inverse">Whether this is a set of Inverse Negated Properties</param>
        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties, bool inverse)
        {
            this._start = start;
            this._end = end;
            this._properties.AddRange(properties.Select(p => p.Predicate));
            this._inverse = inverse;
        }

        /// <summary>
        /// Creates a new Negated Property Set
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="properties">Negated Properties</param>
        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties)
            : this(start, end, properties, false) { }

        /// <summary>
        /// Gets the Path Start
        /// </summary>
        public PatternItem PathStart
        {
            get
            {
                return this._start;
            }
        }

        /// <summary>
        /// Gets the Path End
        /// </summary>
        public PatternItem PathEnd
        {
            get
            {
                return this._end;
            }
        }

        /// <summary>
        /// Gets the Negated Properties
        /// </summary>
        public IEnumerable<INode> Properties
        {
            get
            {
                return this._properties;
            }
        }

        /// <summary>
        /// Gets whether this is a set of Inverse Negated Properties
        /// </summary>
        public bool Inverse
        {
            get
            {
                return this._inverse;
            }
        }

        #region ISparqlAlgebra Members

        /// <summary>
        /// Evaluates the Negated Property Set
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            IEnumerable<Triple> ts;
            String subjVar = this._start.VariableName;
            String objVar = this._end.VariableName;
            if (subjVar != null && context.InputMultiset.ContainsVariable(subjVar))
            {
                if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null && s[objVar] != null
                          from t in context.Data.GetTriplesWithSubjectObject(s[subjVar], s[objVar])
                          select t);
                }
                else
                {
                    ts = (from s in context.InputMultiset.Sets
                          where s[subjVar] != null
                          from t in context.Data.GetTriplesWithSubject(s[subjVar])
                          select t);
                }
            }
            else if (objVar != null && context.InputMultiset.ContainsVariable(objVar))
            {
                ts = (from s in context.InputMultiset.Sets
                      where s[objVar] != null
                      from t in context.Data.GetTriplesWithObject(s[objVar])
                      select t);
            }
            else
            {
                ts = context.Data.Triples;
            }

            context.OutputMultiset = new Multiset();

            //Q: Should this not go at the start of evaluation?
            if (this._inverse)
            {
                String temp = objVar;
                objVar = subjVar;
                subjVar = temp;
            }
            foreach (Triple t in ts)
            {
                if (!this._properties.Contains(t.Predicate))
                {
                    Set s = new Set();
                    if (subjVar != null) s.Add(subjVar, t.Subject);
                    if (objVar != null) s.Add(objVar, t.Object);
                    context.OutputMultiset.Add(s);
                }
            }

            if (subjVar == null && objVar == null)
            {
                if (context.OutputMultiset.Count == 0)
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
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (this._start.VariableName != null)
                {
                    if (this._end.VariableName != null)
                    {
                        return this._start.VariableName.AsEnumerable().Concat(this._end.VariableName.AsEnumerable());
                    }
                    else
                    {
                        return this._start.VariableName.AsEnumerable();
                    }
                }
                else if (this._end.VariableName != null)
                {
                    return this._end.VariableName.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<String>();
                }
            }
        }

        /// <summary>
        /// Transforms the Algebra back into a SPARQL QUery
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            return q;
        }

        /// <summary>
        /// Transforms the Algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp;
            if (this._inverse)
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(Enumerable.Empty<Property>(), this._properties.Select(p => new Property(p))), this.PathEnd);
            }
            else
            {
                pp = new PropertyPathPattern(this.PathStart, new NegatedSet(this._properties.Select(p => new Property(p)), Enumerable.Empty<Property>()), this.PathEnd);
            }
            gp.AddTriplePattern(pp);
            return gp;
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("NegatedPropertySet(");
            output.Append(this._start.ToString());
            output.Append(", {");
            for (int i = 0; i < this._properties.Count; i++)
            {
                output.Append(this._properties[i].ToString());
                if (i < this._properties.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append("}, ");
            output.Append(this._end.ToString());
            output.Append(')');

            return output.ToString();
        }
    }
}
