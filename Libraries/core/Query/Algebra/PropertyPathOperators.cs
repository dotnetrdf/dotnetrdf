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
    /// Interface for Property Path Operators
    /// </summary>
    public interface IPathOperator : ISparqlAlgebra
    {
        /// <summary>
        /// Gets the Path Start
        /// </summary>
        PatternItem PathStart
        {
            get;
        }

        /// <summary>
        /// Gets the Path End
        /// </summary>
        PatternItem PathEnd
        {
            get;
        }

        /// <summary>
        /// Gets the Property Path
        /// </summary>
        ISparqlPath Path
        {
            get;
        }
    }

    /// <summary>
    /// Abstract Base Class for Path Operators
    /// </summary>
    public abstract class BasePathOperator : IPathOperator
    {
        private PatternItem _start, _end;
        private ISparqlPath _path;
        private HashSet<String> _vars = new HashSet<string>();

        /// <summary>
        /// Creates a new Path Operator
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="path">Property Path</param>
        /// <param name="end">Path End</param>
        public BasePathOperator(PatternItem start, ISparqlPath path, PatternItem end)
        {
            this._start = start;
            this._end = end;
            this._path = path;

            if (this._start.VariableName != null) this._vars.Add(this._start.VariableName);
            if (this._end.VariableName != null) this._vars.Add(this._end.VariableName);
        }

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
        /// Gets the Property Path
        /// </summary>
        public ISparqlPath Path
        {
            get 
            { 
                return this._path;
            }
        }

        /// <summary>
        /// Evaluates the Property Path
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <returns></returns>
        public abstract BaseMultiset Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Transforms the Algebra back into a Query
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
        /// Transforms the Algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public abstract GraphPattern ToGraphPattern();

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// Abstract Base Class for Arbitrary Length Path Operators
    /// </summary>
    public abstract class BaseArbitraryLengthPathOperator : BasePathOperator
    {
        /// <summary>
        /// Creates a new Arbitrary Lengh Path Operator
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="end">Path End</param>
        /// <param name="path">Property Path</param>
        public BaseArbitraryLengthPathOperator(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, path, end) { }

        /// <summary>
        /// Determines the starting points for Path evaluation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="paths">Paths</param>
        /// <param name="reverse">Whether to evaluate Paths in reverse</param>
        protected void GetPathStarts(SparqlEvaluationContext context, List<List<INode>> paths, bool reverse)
        {
            HashSet<KeyValuePair<INode, INode>> nodes = new HashSet<KeyValuePair<INode, INode>>();
            if (this.Path is Property)
            {
                INode predicate = ((Property)this.Path).Predicate;
                foreach (Triple t in context.Data.GetTriplesWithPredicate(predicate))
                {
                    if (reverse)
                    {
                        nodes.Add(new KeyValuePair<INode, INode>(t.Object, t.Subject));
                    }
                    else
                    {
                        nodes.Add(new KeyValuePair<INode, INode>(t.Subject, t.Object));
                    }
                }
            }
            else
            {
                BaseMultiset initialInput = context.InputMultiset;
                context.InputMultiset = new IdentityMultiset();
                VariablePattern x = new VariablePattern("?x");
                VariablePattern y = new VariablePattern("?y");
                Bgp bgp = new Bgp(new PropertyPathPattern(x, this.Path, y));

                BaseMultiset results = context.Evaluate(bgp);//bgp.Evaluate(context);
                context.InputMultiset = initialInput;

                if (!results.IsEmpty)
                {
                    foreach (ISet s in results.Sets)
                    {
                        if (s["x"] != null && s["y"] != null)
                        {
                            if (reverse)
                            {
                                nodes.Add(new KeyValuePair<INode, INode>(s["y"], s["x"]));
                            }
                            else
                            {
                                nodes.Add(new KeyValuePair<INode, INode>(s["x"], s["y"]));
                            }
                        }
                    }
                }
            }

            paths.AddRange(nodes.Select(kvp => new List<INode>(new INode[] { kvp.Key, kvp.Value })));
        }

        /// <summary>
        /// Evaluates a setp of the Path
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="path">Paths</param>
        /// <param name="reverse">Whether to evaluate Paths in reverse</param>
        /// <returns></returns>
        protected List<INode> EvaluateStep(SparqlEvaluationContext context, List<INode> path, bool reverse)
        {
            if (this.Path is Property)
            {
                HashSet<INode> nodes = new HashSet<INode>();
                INode predicate = ((Property)this.Path).Predicate;
                IEnumerable<Triple> ts = (reverse ? context.Data.GetTriplesWithPredicateObject(predicate, path[path.Count - 1]) : context.Data.GetTriplesWithSubjectPredicate(path[path.Count - 1], predicate));
                foreach (Triple t in ts)
                {
                    if (reverse)
                    {
                        if (!path.Contains(t.Subject))
                        {
                            nodes.Add(t.Subject);
                        }
                    }
                    else
                    {
                        if (!path.Contains(t.Object))
                        {
                            nodes.Add(t.Object);
                        }
                    }
                }
                return nodes.ToList();
            }
            else
            {
                List<INode> nodes = new List<INode>();

                BaseMultiset initialInput = context.InputMultiset;
                Multiset currInput = new Multiset();
                VariablePattern x = new VariablePattern("?x");
                VariablePattern y = new VariablePattern("?y");
                Set temp = new Set();
                if (reverse)
                {
                    temp.Add("y", path[path.Count - 1]);
                }
                else
                {
                    temp.Add("x", path[path.Count - 1]);
                }
                currInput.Add(temp);
                context.InputMultiset = currInput;

                Bgp bgp = new Bgp(new PropertyPathPattern(x, this.Path, y));
                BaseMultiset results = context.Evaluate(bgp);//bgp.Evaluate(context);
                context.InputMultiset = initialInput;

                if (!results.IsEmpty)
                {
                    foreach (ISet s in results.Sets)
                    {
                        if (reverse)
                        {
                            if (s["x"] != null)
                            {
                                if (!path.Contains(s["x"]))
                                {
                                    nodes.Add(s["x"]);
                                }
                            }
                        }
                        else
                        {
                            if (s["y"] != null)
                            {
                                if (!path.Contains(s["y"]))
                                {
                                    nodes.Add(s["y"]);
                                }
                            }
                        }
                    }
                }

                return nodes;
            }
        }
    }
}
