/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// Evaluation Context for evaluating complex property paths in SPARQL.
    /// </summary>
    /// <typeparam name="T">The type of the SPARQL evaluation context to use when evaluating the path.</typeparam>
    public class PathEvaluationContext
    {
        /// <summary>
        /// Creates a new Path Evaluation Context.
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context.</param>
        /// <param name="end">Start point of the Path.</param>
        /// <param name="start">End point of the Path.</param>
        public PathEvaluationContext(SparqlEvaluationContext context, PatternItem start, PatternItem end)
        {
            SparqlContext = context;
            PathStart = start;
            PathEnd = end;
            if (PathStart.IsFixed && PathEnd.IsFixed) CanAbortEarly = true;
            PathStart.RigorousEvaluation = true;
            PathEnd.RigorousEvaluation = true;
        }

        /// <summary>
        /// Creates a new Path Evaluation Context copied from the given Context.
        /// </summary>
        /// <param name="context">Path Evaluation Context.</param>
        public PathEvaluationContext(PathEvaluationContext context)
            : this(context.SparqlContext, context.PathStart, context.PathEnd)
        {
            foreach (PotentialPath p in context.Paths)
            {
                Paths.Add(new PotentialPath(p));
            }
            CompletePaths.UnionWith(context.CompletePaths);
            IsFirst = context.IsFirst;
            IsLast = context.IsLast;
            IsReversed = context.IsReversed;
        }

        /// <summary>
        /// Gets the SPARQL Evaluation Context.
        /// </summary>
        public SparqlEvaluationContext SparqlContext { get; }

        /// <summary>
        /// Gets/Sets whether this is the first part of the Path to be evaluated.
        /// </summary>
        public bool IsFirst { get; set; } = true;

        /// <summary>
        /// Gets/Sets whether this is the last part of the Path to be evaluated.
        /// </summary>
        public bool IsLast { get; set; } = true;

        /// <summary>
        /// Gets/Sets whether the Path is currently reversed.
        /// </summary>
        public bool IsReversed { get; set; }

        /// <summary>
        /// Gets the hash set of incomplete paths generated so far.
        /// </summary>
        public HashSet<PotentialPath> Paths { get; } = new HashSet<PotentialPath>();

        /// <summary>
        /// Gets the hash set of complete paths generated so far.
        /// </summary>
        public HashSet<PotentialPath> CompletePaths { get; } = new HashSet<PotentialPath>();

        /// <summary>
        /// Gets the pattern which is the start of the path.
        /// </summary>
        public PatternItem PathStart { get; }

        /// <summary>
        /// Gets the pattern which is the end of the path.
        /// </summary>
        public PatternItem PathEnd { get; }

        /// <summary>
        /// Gets whether pattern evaluation can be aborted early.
        /// </summary>
        /// <remarks>
        /// Useful when both the start and end of the path are fixed (non-variables) which means that we can stop evaluating once we find the path (if it exists).
        /// </remarks>
        public bool CanAbortEarly { get; }

        /// <summary>
        /// Gets/Sets whether new paths can be introduced when not evaluating the first part of the path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is required when we have a path like ?x foaf:knows* /foaf:knows ?y and ?x is not bound prior to the path being executed.  Since we permit zero-length paths we should return the names of everyone even if they don't know anyone.
        /// </para>
        /// <para>
        /// The cases where ?x is already bound are handled elsewhere as we can just introduce zero-length paths for every existing binding for ?x.
        /// </para>
        /// </remarks>
        public bool PermitsNewPaths { get; set; } = false;

        /// <summary>
        /// Adds a new path to the list of current incomplete paths.
        /// </summary>
        /// <param name="p">Path.</param>
        public void AddPath(PotentialPath p)
        {
            if (!Paths.Contains(p))
            {
                Paths.Add(p);
            }
        }

        /// <summary>
        /// Adds a new path to the list of complete paths.
        /// </summary>
        /// <param name="p">Path.</param>
        public void AddCompletePath(PotentialPath p)
        {
            if (p.IsComplete && !p.IsPartial)
            {
                if (!CompletePaths.Contains(p))
                {
                    CompletePaths.Add(p);
                }
            }
        }
    }
}
