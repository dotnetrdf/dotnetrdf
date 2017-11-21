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
            if (AreBothTerms())
            {
                if (AreSameTerms())
                {
                    return new IdentityMultiset();
                }
                else
                {
                    return new NullMultiset();
                }
            }

            String subjVar = PathStart.VariableName;
            String objVar = PathEnd.VariableName;
            context.OutputMultiset = new Multiset();

            // Determine the Triples to which this applies
            if (subjVar != null)
            {
                // Subject is a Variable
                if (context.InputMultiset.ContainsVariable(subjVar))
                {
                    // Subject is Bound
                    if (objVar != null)
                    {
                        // Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            // Both Subject and Object are Bound
                            foreach (ISet s in context.InputMultiset.Sets.Where(x => x[subjVar] != null && x[objVar] != null && PathStart.Accepts(context, x[subjVar]) && PathEnd.Accepts(context, x[objVar])))
                            {
                                ISet x = new Set();
                                x.Add(subjVar, x[subjVar]);
                                context.OutputMultiset.Add(x);
                                x = new Set();
                                x.Add(objVar, x[objVar]);
                                context.OutputMultiset.Add(x);
                            }
                        }
                        else
                        {
                            // Subject is bound but Object is Unbound
                            foreach (ISet s in context.InputMultiset.Sets.Where(x => x[subjVar] != null && PathStart.Accepts(context, x[subjVar])))
                            {
                                ISet x = s.Copy();
                                x.Add(objVar, x[subjVar]);
                                context.OutputMultiset.Add(x);
                            }
                        }
                    }
                    else
                    {
                        // Object is a Term
                        // Preseve sets where the Object Term is equal to the currently bound Subject
                        INode objTerm = ((NodeMatchPattern)PathEnd).Node;
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
                    // Subject is Unbound
                    if (objVar != null)
                    {
                        // Object is a Variable
                        if (context.InputMultiset.ContainsVariable(objVar))
                        {
                            // Object is Bound but Subject is unbound
                            foreach (ISet s in context.InputMultiset.Sets.Where(x => x[objVar] != null && PathEnd.Accepts(context, x[objVar])))
                            {
                                ISet x = s.Copy();
                                x.Add(subjVar, x[objVar]);
                                context.OutputMultiset.Add(x);
                            }
                        }
                        else
                        {
                            // Subject and Object are Unbound
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
                        // Object is a Term
                        // Create a single set with the Variable bound to the Object Term
                        Set s = new Set();
                        s.Add(subjVar, ((NodeMatchPattern)PathEnd).Node);
                        context.OutputMultiset.Add(s);
                    }
                }
            }
            else if (objVar != null)
            {
                // Subject is a Term but Object is a Variable
                if (context.InputMultiset.ContainsVariable(objVar))
                {
                    // Object is Bound
                    // Preseve sets where the Subject Term is equal to the currently bound Object
                    INode subjTerm = ((NodeMatchPattern)PathStart).Node;
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
                    // Object is Unbound
                    // Create a single set with the Variable bound to the Suject Term
                    Set s = new Set();
                    s.Add(objVar, ((NodeMatchPattern)PathStart).Node);
                    context.OutputMultiset.Add(s);
                }
            }
            else
            {
                // Should already have dealt with this earlier (the AreBothTerms() and AreSameTerms() branch)
                throw new RdfQueryException("Reached unexpected point of ZeroLengthPath evaluation");
            }

            return context.OutputMultiset;
        }

        private bool AreBothTerms()
        {
            return (PathStart.VariableName == null && PathEnd.VariableName == null);
        }

        private bool AreSameTerms()
        {
            if (PathStart is NodeMatchPattern && PathEnd is NodeMatchPattern)
            {
                return ((NodeMatchPattern)PathStart).Node.Equals(((NodeMatchPattern)PathEnd).Node);
            }
            else if (PathStart is FixedBlankNodePattern && PathEnd is FixedBlankNodePattern)
            {
                return ((FixedBlankNodePattern)PathStart).InternalID.Equals(((FixedBlankNodePattern)PathEnd).InternalID);
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
            return "ZeroLengthPath(" + PathStart.ToString() + ", " + Path.ToString() + ", " + PathEnd.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(PathStart, new FixedCardinality(Path, 0), PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }
}
