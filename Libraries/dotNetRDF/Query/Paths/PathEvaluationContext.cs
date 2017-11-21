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

using System.Collections.Generic;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Evaluation Context for evaluating complex property paths in SPARQL
    /// </summary>
    public class PathEvaluationContext
    {
        private bool _first = true, _last = true, _reverse = false, _earlyAbort = false, _allowNewPaths = false;
        private HashSet<PotentialPath> _incompletePaths = new HashSet<PotentialPath>();
        private HashSet<PotentialPath> _completePaths = new HashSet<PotentialPath>();
        private SparqlEvaluationContext _context;
        private PatternItem _start, _end;

        /// <summary>
        /// Creates a new Path Evaluation Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="end">Start point of the Path</param>
        /// <param name="start">End point of the Path</param>
        public PathEvaluationContext(SparqlEvaluationContext context, PatternItem start, PatternItem end)
        {
            _context = context;
            _start = start;
            _end = end;
            if (_start.VariableName == null && _end.VariableName == null) _earlyAbort = true;
            _start.RigorousEvaluation = true;
            _end.RigorousEvaluation = true;
        }

        /// <summary>
        /// Creates a new Path Evaluation Context copied from the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public PathEvaluationContext(PathEvaluationContext context)
            : this(context.SparqlContext, context.PathStart, context.PathEnd)
        {
            foreach (PotentialPath p in context.Paths)
            {
                _incompletePaths.Add(new PotentialPath(p));
            }
            _completePaths.UnionWith(context.CompletePaths);
            _first = context.IsFirst;
            _last = context.IsLast;
            _reverse = context.IsReversed;
        }

        /// <summary>
        /// Gets the SPARQL Evaluation Context
        /// </summary>
        public SparqlEvaluationContext SparqlContext
        {
            get
            {
                return _context;
            }
        }

        /// <summary>
        /// Gets/Sets whether this is the first part of the Path to be evaluated
        /// </summary>
        public bool IsFirst
        {
            get
            {
                return _first;
            }
            set
            {
                _first = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether this is the last part of the Path to be evaluated
        /// </summary>
        public bool IsLast
        {
            get
            {
                return _last;
            }
            set
            {
                _last = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Path is currently reversed
        /// </summary>
        public bool IsReversed
        {
            get
            {
                return _reverse;
            }
            set
            {
                _reverse = value;
            }
        }

        /// <summary>
        /// Gets the hash set of incomplete paths generated so far
        /// </summary>
        public HashSet<PotentialPath> Paths
        {
            get
            {
                return _incompletePaths;
            }
        }

        /// <summary>
        /// Gets the hash set of complete paths generated so far
        /// </summary>
        public HashSet<PotentialPath> CompletePaths
        {
            get
            {
                return _completePaths;
            }
        }

        /// <summary>
        /// Gets the pattern which is the start of the path
        /// </summary>
        public PatternItem PathStart
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets the pattern which is the end of the path
        /// </summary>
        public PatternItem PathEnd
        {
            get
            {
                return _end;
            }
        }

        /// <summary>
        /// Gets whether pattern evaluation can be aborted early
        /// </summary>
        /// <remarks>
        /// Useful when both the start and end of the path are fixed (non-variables) which means that we can stop evaluating once we find the path (if it exists)
        /// </remarks>
        public bool CanAbortEarly
        {
            get
            {
                return _earlyAbort;
            }
        }

        /// <summary>
        /// Gets/Sets whether new paths can be introduced when not evaluating the first part of the path
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is required when we have a path like ?x foaf:knows* /foaf:knows ?y and ?x is not bound prior to the path being executed.  Since we permit zero-length paths we should return the names of everyone even if they don't know anyone
        /// </para>
        /// <para>
        /// The cases where ?x is already bound are handled elsewhere as we can just introduce zero-length paths for every existing binding for ?x
        /// </para>
        /// </remarks>
        public bool PermitsNewPaths
        {
            get
            {
                return _allowNewPaths;
            }
            set
            {
                _allowNewPaths = value;
            }
        }

        /// <summary>
        /// Adds a new path to the list of current incomplete paths
        /// </summary>
        /// <param name="p">Path</param>
        public void AddPath(PotentialPath p)
        {
            if (!_incompletePaths.Contains(p))
            {
                _incompletePaths.Add(p);
            }
        }

        /// <summary>
        /// Adds a new path to the list of complete paths
        /// </summary>
        /// <param name="p">Path</param>
        public void AddCompletePath(PotentialPath p)
        {
            if (p.IsComplete && !p.IsPartial)
            {
                if (!_completePaths.Contains(p))
                {
                    _completePaths.Add(p);
                }
            }
        }
    }
}
