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
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Zero Length Path in the SPARQL Algebra
    /// </summary>
    public class ZeroLengthPath : BasePathOperator
    {
        /// <summary>
        /// Creates a new Zero Length Path
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="path">Property Path</param>
        public ZeroLengthPath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, path, end) { }

        /// <summary>
        /// Evaluates a Zero Length Path
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (this.AreBothTerms())
            {
                if (this.AreSameTerms())
                {
                    return new IdentityMultiset();
                }
                else
                {
                    return new NullMultiset();
                }
            }

            String subjVar = this.PathStart.VariableName;
            String objVar = this.PathEnd.VariableName;
            context.OutputMultiset = new Multiset();

            //Determine the Triples to which this applies
            IEnumerable<Triple> ts = null;
            if (subjVar != null)
            {
                //Subject is a Variable
                if (context.InputMultiset.ContainsVariable(subjVar))
                {
                    //Subject is Bound
                    if (objVar != null)
                    {
                        //Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            //Object is Bound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[subjVar] != null && s[objVar] != null
                                  from t in context.Data.GetTriplesWithSubjectObject(s[subjVar], s[objVar])
                                  select t);
                        }
                        else
                        {
                            //Object is Unbound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[subjVar] != null
                                  from t in context.Data.GetTriplesWithSubject(s[subjVar])
                                  select t);
                        }
                    }
                    else
                    {
                        //Object is a Term
                        //Preseve sets where the Object Term is equal to the currently bound Subject
                        INode objTerm = ((NodeMatchPattern)this.PathEnd).Node;
                        foreach (ISet s in context.InputMultiset.Sets)
                        {
                            INode temp = s[subjVar];
                            if (temp != null && temp.Equals(objTerm))
                            {
                                context.OutputMultiset.Add(s.Copy());
                            }
                        }
                    }
                }
                else
                {
                    //Subject is Unbound
                    if (objVar != null)
                    {
                        //Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            //Object is Bound
                            ts = (from s in context.InputMultiset.Sets
                                  where s[objVar] != null
                                  from t in context.Data.GetTriplesWithObject(s[objVar])
                                  select t);
                        }
                        else
                        {
                            //Object is Unbound
                            HashSet<INode> nodes = new HashSet<INode>();
                            foreach (Triple t in context.Data.Triples)
                            {
                                nodes.Add(t.Subject);
                                nodes.Add(t.Object);
                            }
                            foreach (INode n in nodes)
                            {
                                Set s = new Set();
                                s.Add(subjVar, n);
                                s.Add(objVar, n);
                                context.OutputMultiset.Add(s);
                            }
                        }
                    }
                    else
                    {
                        //Object is a Term
                        //Create a single set with the Variable bound to the Object Term
                        Set s = new Set();
                        s.Add(subjVar, ((NodeMatchPattern)this.PathEnd).Node);
                        context.OutputMultiset.Add(s);
                    }
                }
            }
            else if (objVar != null)
            {
                //Subject is a Term but Object is a Variable
                if (context.InputMultiset.ContainsVariable(objVar))
                {
                    //Object is Bound
                    //Preseve sets where the Subject Term is equal to the currently bound Object
                    INode subjTerm = ((NodeMatchPattern)this.PathStart).Node;
                    foreach (ISet s in context.InputMultiset.Sets)
                    {
                        INode temp = s[objVar];
                        if (temp != null && temp.Equals(subjTerm))
                        {
                            context.OutputMultiset.Add(s.Copy());
                        }
                    }
                }
                else
                {
                    //Object is Unbound
                    //Create a single set with the Variable bound to the Suject Term
                    Set s = new Set();
                    s.Add(objVar, ((NodeMatchPattern)this.PathStart).Node);
                    context.OutputMultiset.Add(s);
                }
            }
            else
            {
                //Should already have dealt with this earlier (the AreBothTerms() and AreSameTerms() branch)
                throw new RdfQueryException("Reached unexpected point of ZeroLengthPath evaluation");
            }

            //Get the Matches only if we haven't already generated the output
            if (ts != null)
            {
                HashSet<KeyValuePair<INode, INode>> matches = new HashSet<KeyValuePair<INode, INode>>();
                foreach (Triple t in ts)
                {
                    if (this.PathStart.Accepts(context, t.Subject) && this.PathEnd.Accepts(context, t.Object))
                    {
                        matches.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
                    }
                }

                //Generate the Output based on the mathces
                if (matches.Count == 0)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    if (this.PathStart.VariableName == null && this.PathEnd.VariableName == null)
                    {
                        context.OutputMultiset = new IdentityMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new Multiset();
                        foreach (KeyValuePair<INode, INode> m in matches)
                        {
                            Set s = new Set();
                            if (subjVar != null) s.Add(subjVar, m.Key);
                            if (objVar != null) s.Add(objVar, m.Value);
                            context.OutputMultiset.Add(s);
                        }
                    }
                }
            }
            return context.OutputMultiset;
        }

        private bool AreBothTerms()
        {
            return (this.PathStart.VariableName == null && this.PathEnd.VariableName == null);
        }

        private bool AreSameTerms()
        {
            if (this.PathStart is NodeMatchPattern && this.PathEnd is NodeMatchPattern)
            {
                return ((NodeMatchPattern)this.PathStart).Node.Equals(((NodeMatchPattern)this.PathEnd).Node);
            }
            else if (this.PathStart is FixedBlankNodePattern && this.PathEnd is FixedBlankNodePattern)
            {
                return ((FixedBlankNodePattern)this.PathStart).InternalID.Equals(((FixedBlankNodePattern)this.PathEnd).InternalID);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ZeroLengthPath(" + this.PathStart.ToString() + ", " + this.Path.ToString() + ", " + this.PathEnd.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(this.PathStart, new FixedCardinality(this.Path, 0), this.PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }
}
