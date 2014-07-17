using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

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
            List<Triple> patterns = bgp.TriplePatterns.ToList();
            if (patterns.Count == 0) return new Set().AsEnumerable();

            IEnumerable<ISet> results = Enumerable.Empty<ISet>();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (i == 0)
                {
                    results = this.BgpExecutor.Match(patterns[i], context);
                }
                else
                {
                    int i1 = i;
                    results = results.SelectMany(s => this.BgpExecutor.Match(patterns[i1], s, context));
                }
            }
            return results;
        }

        public virtual IEnumerable<ISet> Execute(Slice slice, IExecutionContext context)
        {
            if (slice.Limit == 0) return Enumerable.Empty<ISet>();

            IEnumerable<ISet> innerResult = this.Execute(slice.InnerAlgebra);
            if (slice.Limit > 0)
            {
                return slice.Offset > 0 ? innerResult.Skip(slice.Offset).Take(slice.Limit) : innerResult.Take(slice.Limit);
            }
            return slice.Offset > 0 ? innerResult.Skip(slice.Offset) : innerResult;
        }

        public IEnumerable<ISet> Execute(Union union, IExecutionContext context)
        {
            context = EnsureContext(context);
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
    }
}
