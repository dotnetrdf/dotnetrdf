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
    /// Represents a One or More Path (e.g. rdf:type+) in the SPARQL Algebra
    /// </summary>
    public class OneOrMorePath : BaseArbitraryLengthPathOperator
    {
        /// <summary>
        /// Creates a new One or More Path
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="path">Path</param>
        public OneOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }

        /// <summary>
        /// Evaluates the One or More Path
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            List<List<INode>> paths = new List<List<INode>>();
            BaseMultiset initialInput = context.InputMultiset;
            int step = 0, prevCount = 0, skipCount = 0;

            String subjVar = PathStart.VariableName;
            String objVar = PathEnd.VariableName;
            bool bothTerms = (subjVar == null && objVar == null);
            bool reverse = false;

            if (subjVar == null || (subjVar != null && context.InputMultiset.ContainsVariable(subjVar)) || (objVar != null && !context.InputMultiset.ContainsVariable(objVar)))
            {
                // Work Forwards from the Starting Term or Bound Variable
                // OR if there is no Ending Term or Bound Variable work forwards regardless
                if (subjVar == null)
                {
                    paths.Add(((NodeMatchPattern)PathStart).Node.AsEnumerable().ToList());
                }
                else if (context.InputMultiset.ContainsVariable(subjVar))
                {
                    paths.AddRange((from s in context.InputMultiset.Sets
                                    where s[subjVar] != null
                                    select s[subjVar]).Distinct().Select(n => n.AsEnumerable().ToList()));
                }
            }
            else if (objVar == null || (objVar != null && context.InputMultiset.ContainsVariable(objVar)))
            {
                // Work Backwards from Ending Term or Bound Variable
                if (objVar == null)
                {
                    paths.Add(((NodeMatchPattern)PathEnd).Node.AsEnumerable().ToList());
                }
                else
                {
                    paths.AddRange((from s in context.InputMultiset.Sets
                                    where s[objVar] != null
                                    select s[objVar]).Distinct().Select(n => n.AsEnumerable().ToList()));
                }
                reverse = true;
            }

            if (paths.Count == 0)
            {
                GetPathStarts(context, paths, reverse);
            }

            // Traverse the Paths
            do
            {
                prevCount = paths.Count;
                foreach (List<INode> path in paths.Skip(skipCount).ToList())
                {
                    foreach (INode nextStep in EvaluateStep(context, path, reverse))
                    {
                        List<INode> newPath = new List<INode>(path);
                        newPath.Add(nextStep);
                        paths.Add(newPath);
                    }
                }

                if (step == 0)
                {
                    // Remove any 1 length paths as these denote path starts that couldn't be traversed
                    paths.RemoveAll(p => p.Count == 1);
                    prevCount = paths.Count;
                }

                // Update Counts
                // skipCount is used to indicate the paths which we will ignore for the purposes of
                // trying to further extend since we've already done them once
                step++;
                if (paths.Count == 0) break;
                if (step > 1)
                {
                    skipCount = prevCount;
                }

                // Can short circuit evaluation here if both are terms and any path is acceptable
                if (bothTerms)
                {
                    bool exit = false;
                    foreach (List<INode> path in paths)
                    {
                        if (reverse)
                        {
                            if (PathEnd.Accepts(context, path[0]) && PathStart.Accepts(context, path[path.Count - 1]))
                            {
                                exit = true;
                                break;
                            }
                        }
                        else
                        {
                            if (PathStart.Accepts(context, path[0]) && PathEnd.Accepts(context, path[path.Count - 1]))
                            {
                                exit = true;
                                break;
                            }
                        }
                    }
                    if (exit) break;
                }
            } while (paths.Count > prevCount || (step == 1 && paths.Count == prevCount));

            if (paths.Count == 0)
            {
                // If all path starts lead nowhere then we get the Null Multiset as a result
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                context.OutputMultiset = new Multiset();

                // Evaluate the Paths to check that are acceptable
                HashSet<ISet> returnedPaths = new HashSet<ISet>();
                foreach (List<INode> path in paths)
                {
                    if (reverse)
                    {
                        if (PathEnd.Accepts(context, path[0]) && PathStart.Accepts(context, path[path.Count - 1]))
                        {
                            Set s = new Set();
                            if (!bothTerms)
                            {
                                if (subjVar != null) s.Add(subjVar, path[path.Count - 1]);
                                if (objVar != null) s.Add(objVar, path[0]);
                            }
                            // Make sure to check for uniqueness
                            if (returnedPaths.Contains(s)) continue;
                            context.OutputMultiset.Add(s);
                            returnedPaths.Add(s);

                            // If both are terms can short circuit evaluation here
                            // It is sufficient just to determine that there is one path possible
                            if (bothTerms) break;
                        }
                    }
                    else
                    {
                        if (PathStart.Accepts(context, path[0]) && PathEnd.Accepts(context, path[path.Count - 1]))
                        {
                            Set s = new Set();
                            if (!bothTerms)
                            {
                                if (subjVar != null) s.Add(subjVar, path[0]);
                                if (objVar != null) s.Add(objVar, path[path.Count - 1]);
                            }
                            // Make sure to check for uniqueness
                            if (returnedPaths.Contains(s)) continue;
                            context.OutputMultiset.Add(s);
                            returnedPaths.Add(s);

                            // If both are terms can short circuit evaluation here
                            // It is sufficient just to determine that there is one path possible
                            if (bothTerms) break;
                        }
                    }
                }

                if (bothTerms)
                {
                    // If both were terms transform to an Identity/Null Multiset as appropriate
                    if (context.OutputMultiset.IsEmpty)
                    {
                        context.OutputMultiset = new NullMultiset();
                    }
                    else
                    {
                        context.OutputMultiset = new IdentityMultiset();
                    }
                }
            }

            context.InputMultiset = initialInput;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "OneOrMorePath(" + PathStart.ToString() + ", " + Path.ToString() + ", " + PathEnd.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            PropertyPathPattern pp = new PropertyPathPattern(PathStart, new OneOrMore(Path), PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }
}
