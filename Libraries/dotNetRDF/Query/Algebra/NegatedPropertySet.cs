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
        private readonly List<INode> _properties = new List<INode>();
        private readonly PatternItem _start, _end;
        private readonly bool _inverse;

        /// <summary>
        /// Creates a new Negated Property Set
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="properties">Negated Properties</param>
        /// <param name="inverse">Whether this is a set of Inverse Negated Properties</param>
        public NegatedPropertySet(PatternItem start, PatternItem end, IEnumerable<Property> properties, bool inverse)
        {
            _start = start;
            _end = end;
            _properties.AddRange(properties.Select(p => p.Predicate));
            _inverse = inverse;
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
                return _start;
            }
        }

        /// <summary>
        /// Gets the Path End
        /// </summary>
        public PatternItem PathEnd
        {
            get
            {
                return _end;
            }
        }

        /// <summary>
        /// Gets the Negated Properties
        /// </summary>
        public IEnumerable<INode> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Gets whether this is a set of Inverse Negated Properties
        /// </summary>
        public bool Inverse
        {
            get
            {
                return _inverse;
            }
        }

        /// <summary>
        /// Evaluates the Negated Property Set
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            IEnumerable<Triple> ts;
            String subjVar = _start.VariableName;
            String objVar = _end.VariableName;
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

            // Q: Should this not go at the start of evaluation?
            if (_inverse)
            {
                String temp = objVar;
                objVar = subjVar;
                subjVar = temp;
            }
            foreach (Triple t in ts)
            {
                if (!_properties.Contains(t.Predicate))
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
                if (_start.VariableName != null)
                {
                    if (_end.VariableName != null)
                    {
                        return _start.VariableName.AsEnumerable().Concat(_end.VariableName.AsEnumerable());
                    }
                    else
                    {
                        return _start.VariableName.AsEnumerable();
                    }
                }
                else if (_end.VariableName != null)
                {
                    return _end.VariableName.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<String>();
                }
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get { return Variables; }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        /// Transforms the Algebra back into a SPARQL QUery
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = ToGraphPattern();
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
            if (_inverse)
            {
                pp = new PropertyPathPattern(PathStart, new NegatedSet(Enumerable.Empty<Property>(), _properties.Select(p => new Property(p))), PathEnd);
            }
            else
            {
                pp = new PropertyPathPattern(PathStart, new NegatedSet(_properties.Select(p => new Property(p)), Enumerable.Empty<Property>()), PathEnd);
            }
            gp.AddTriplePattern(pp);
            return gp;
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("NegatedPropertySet(");
            output.Append(_start.ToString());
            output.Append(", {");
            for (int i = 0; i < _properties.Count; i++)
            {
                output.Append(_properties[i].ToString());
                if (i < _properties.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append("}, ");
            output.Append(_end.ToString());
            output.Append(')');

            return output.ToString();
        }
    }
}
