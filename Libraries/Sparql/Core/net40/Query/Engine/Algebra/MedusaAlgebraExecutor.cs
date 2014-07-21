using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine.Joins;
using VDS.RDF.Query.Engine.Joins.Strategies;

namespace VDS.RDF.Query.Engine.Algebra
{
    public class MedusaAlgebraExecutor
        : IAlgebraExecutor
    {
        public MedusaAlgebraExecutor(IBgpExecutor executor)
            : this(executor, new DefaultJoinStrategySelector()) { }

        public MedusaAlgebraExecutor(IBgpExecutor executor, IJoinStrategySelector joinStrategySelector)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            if (joinStrategySelector == null) throw new ArgumentNullException("joinStrategySelector");
            this.BgpExecutor = executor;
            this.JoinStrategySelector = joinStrategySelector;
        }

        public IBgpExecutor BgpExecutor { get; private set; }

        public IJoinStrategySelector JoinStrategySelector { get; private set; }

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
            IEnumerable<ISet> innerResult = slice.InnerAlgebra.Execute(this, context);

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
            IEnumerable<ISet> innerResult = filter.InnerAlgebra.Execute(this, context);
            return filter.Expressions.Count > 0 ? new FilterEnumerable(innerResult, filter.Expressions, context) : innerResult;
        }

        public IEnumerable<ISet> Execute(Table table, IExecutionContext context)
        {
            return table.IsEmpty ? Enumerable.Empty<ISet>() : table.Data;
        }

        public IEnumerable<ISet> Execute(Join join, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISet> lhsResults = join.Lhs.Execute(this, context);
            IEnumerable<ISet> rhsResults = join.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, this.JoinStrategySelector.Select(join.Lhs, join.Rhs), context);
        }

        public IEnumerable<ISet> Execute(LeftJoin leftJoin, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISet> lhsResults = leftJoin.Lhs.Execute(this, context);
            IEnumerable<ISet> rhsResults = leftJoin.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, new LeftJoinStrategy(this.JoinStrategySelector.Select(leftJoin.Lhs, leftJoin.Rhs), leftJoin.Expressions), context);
        }

        public IEnumerable<ISet> Execute(Minus minus, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISet> lhsResults = minus.Lhs.Execute(this, context);
            IEnumerable<ISet> rhsResults = minus.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, new NonExistenceJoinStrategy(this.JoinStrategySelector.Select(minus.Lhs, minus.Rhs)), context);
        }

        public IEnumerable<ISet> Execute(Distinct distinct, IExecutionContext context)
        {
            context = EnsureContext(context);
            // TODO Likely want to provide an optimized IEqualityComparer here
            return distinct.InnerAlgebra.Execute(this, context).Distinct();
        }

        public IEnumerable<ISet> Execute(Reduced reduced, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(Project project, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(OrderBy orderBy, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(Extend extend, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(GroupBy groupBy, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(Service service, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(PropertyPath path, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(TopN topN, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(PropertyFunction propertyFunction, IExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(IndexJoin indexJoin, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}