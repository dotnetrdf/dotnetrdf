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
            this._context = context;
            this._start = start;
            this._end = end;
            if (this._start.VariableName == null && this._end.VariableName == null) this._earlyAbort = true;
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
                this._incompletePaths.Add(new PotentialPath(p));
            }
            this._completePaths.UnionWith(context.CompletePaths);
            this._first = context.IsFirst;
            this._last = context.IsLast;
            this._reverse = context.IsReversed;
        }

        /// <summary>
        /// Gets the SPARQL Evaluation Context
        /// </summary>
        public SparqlEvaluationContext SparqlContext
        {
            get
            {
                return this._context;
            }
        }

        /// <summary>
        /// Gets/Sets whether this is the first part of the Path to be evaluated
        /// </summary>
        public bool IsFirst
        {
            get
            {
                return this._first;
            }
            set
            {
                this._first = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether this is the last part of the Path to be evaluated
        /// </summary>
        public bool IsLast
        {
            get
            {
                return this._last;
            }
            set
            {
                this._last = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Path is currently reversed
        /// </summary>
        public bool IsReversed
        {
            get
            {
                return this._reverse;
            }
            set
            {
                this._reverse = value;
            }
        }

        /// <summary>
        /// Gets the hash set of incomplete paths generated so far
        /// </summary>
        public HashSet<PotentialPath> Paths
        {
            get
            {
                return this._incompletePaths;
            }
        }

        /// <summary>
        /// Gets the hash set of complete paths generated so far
        /// </summary>
        public HashSet<PotentialPath> CompletePaths
        {
            get
            {
                return this._completePaths;
            }
        }

        /// <summary>
        /// Gets the pattern which is the start of the path
        /// </summary>
        public PatternItem PathStart
        {
            get
            {
                return this._start;
            }
        }

        /// <summary>
        /// Gets the pattern which is the end of the path
        /// </summary>
        public PatternItem PathEnd
        {
            get
            {
                return this._end;
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
                return this._earlyAbort;
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
                return this._allowNewPaths;
            }
            set
            {
                this._allowNewPaths = value;
            }
        }

        /// <summary>
        /// Adds a new path to the list of current incomplete paths
        /// </summary>
        /// <param name="p">Path</param>
        public void AddPath(PotentialPath p)
        {
            if (!this._incompletePaths.Contains(p))
            {
                this._incompletePaths.Add(p);
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
                if (!this._completePaths.Contains(p))
                {
                    this._completePaths.Add(p);
                }
            }
        }
    }
}
