using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Medusa
{
    public abstract class MedusaAlgebraExecutor
        : IAlgebraExecutor
    {
        protected MedusaAlgebraExecutor(IBgpExecutor executor)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            this.BgpExecutor = executor;
        }

        public IBgpExecutor BgpExecutor { get; private set; }

        /// <summary>
        /// Ensures that a valid context is available
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>A valid non-null context</returns>
        protected virtual IExecutionContext EnsureContext(IExecutionContext context)
        {
            return context ?? new QueryExecutionContext();
        }

        public virtual IEnumerable<ISet> Execute(IAlgebra algebra)
        {
            return Execute(algebra, EnsureContext(null));
        }

        public virtual IEnumerable<ISet> Execute(IAlgebra algebra, IExecutionContext context)
        {
            return algebra.Execute(this, EnsureContext(context));
        }

        public virtual IEnumerable<ISet> Execute(Bgp bgp, IExecutionContext context)
        {
            context = EnsureContext(context);

            // An empty BGP acts as an identity and always matches producing a single empty set
            List<Triple> patterns = bgp.TriplePatterns.ToList();
            if (patterns.Count == 0) return new Set().AsEnumerable();

            // Otherwise build up the enumerable by sequencing the triple pattern matches together
            // TODO Should the query engine schedule the order of patterns or are we expecting the optimizer to do that for us?
            IEnumerable<ISet> results = Enumerable.Empty<ISet>();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (i == 0)
                {
                    // Initial match is unbounded
                    results = this.BgpExecutor.Match(patterns[i], context);
                }
                else
                {
                    // Subsequent matches are bounded by the data returned from previous matches
                    // Essentially we are performing an index join
                    int i1 = i;
                    results = results.SelectMany(s => this.BgpExecutor.Match(patterns[i1], s, context));
                }
            }
            return results;
        }

        public virtual IEnumerable<ISet> Execute(Slice slice, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Zero Limit means we can short circuit any further evaluation
            if (slice.Limit == 0) return Enumerable.Empty<ISet>();

            // Execute the inner algebra
            IEnumerable<ISet> innerResult = this.Execute(slice.InnerAlgebra);

            // Apply Limit and Offset if present
            if (slice.Limit > 0)
            {
                // There is a Limit present, if Offset is also present it applies before the Limit
                return slice.Offset > 0 ? innerResult.Skip(slice.Offset).Take(slice.Limit) : innerResult.Take(slice.Limit);
            }
            // No Limit present but there may be an Offset to apply
            return slice.Offset > 0 ? innerResult.Skip(slice.Offset) : innerResult;
        }

        public IEnumerable<ISet> Execute(Union union, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Union is simply the concatenation of the execution of the two sides
            return union.Lhs.Execute(this, context).Concat(union.Rhs.Execute(this, context));
        }

        public IEnumerable<ISet> Execute(NamedGraph namedGraph, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Variable Graph Name
            if (namedGraph.Graph.NodeType == NodeType.Variable) return context.NamedGraphs.Count > 0 ? new NamedGraphEnumerable(namedGraph, this, context) : Enumerable.Empty<ISet>();

            // Fixed Graph Name
            context = context.PushActiveGraph(namedGraph.Graph);
            return namedGraph.InnerAlgebra.Execute(this, context);
        }

        public IEnumerable<ISet> Execute(Filter filter, IExecutionContext context)
        {
            return filter.InnerAlgebra.Execute(this, context).Where(s =>
            {
                try
                {
                    return filter.Expressions.Select(expr => expr.Evaluate(s, context.CreateExpressionContext())).All(n => n.AsSafeBoolean());
                }
                catch (RdfQueryException)
                {
                    return false;
                }
            });
        }
    }
}