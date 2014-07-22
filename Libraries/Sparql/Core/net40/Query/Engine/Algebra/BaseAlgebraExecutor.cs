using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine.Joins;
using VDS.RDF.Query.Engine.Joins.Strategies;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Engine.Algebra
{
    /// <summary>
    /// Abstract implementation of an algebra executor which knows how to execute everything except BGPs and SERVICE clauses
    /// </summary>
    /// <remarks>
    /// This allows derived implementations to provide suitable implementations of BGP execution for their backends and to optionally support federated query
    /// </remarks>
    public abstract class BaseAlgebraExecutor
        : IAlgebraExecutor
    {
        protected BaseAlgebraExecutor(IJoinStrategySelector joinStrategySelector)
        {
            if (joinStrategySelector == null) throw new ArgumentNullException("joinStrategySelector");
            this.JoinStrategySelector = joinStrategySelector;
        }

        /// <summary>
        /// Gets the Join Strategy selector that is used
        /// </summary>
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

        public virtual IEnumerable<ISolution> Execute(IAlgebra algebra)
        {
            return Execute(algebra, EnsureContext(null));
        }

        public virtual IEnumerable<ISolution> Execute(IAlgebra algebra, IExecutionContext context)
        {
            return algebra.Execute(this, EnsureContext(context));
        }

        public abstract IEnumerable<ISolution> Execute(Bgp bgp, IExecutionContext context);

        public virtual IEnumerable<ISolution> Execute(Slice slice, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Zero Limit means we can short circuit any further evaluation
            if (slice.Limit == 0) return Enumerable.Empty<ISolution>();

            // Execute the inner algebra
            IEnumerable<ISolution> innerResult = slice.InnerAlgebra.Execute(this, context);

            // Apply Limit and Offset if present
            if (slice.Limit > 0)
            {
                // There is a Limit present, if Offset is also present it applies before the Limit
                return slice.Offset > 0 ? innerResult.Skip(slice.Offset).Take(slice.Limit) : innerResult.Take(slice.Limit);
            }
            // No Limit present but there may be an Offset to apply
            return slice.Offset > 0 ? innerResult.Skip(slice.Offset) : innerResult;
        }

        public IEnumerable<ISolution> Execute(Union union, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Union is simply the concatenation of the execution of the two sides
            return union.Lhs.Execute(this, context).Concat(union.Rhs.Execute(this, context));
        }

        public IEnumerable<ISolution> Execute(NamedGraph namedGraph, IExecutionContext context)
        {
            context = EnsureContext(context);

            // Variable Graph Name
            if (namedGraph.Graph.NodeType == NodeType.Variable) return context.NamedGraphs.Count > 0 ? new NamedGraphEnumerable(namedGraph, this, context) : Enumerable.Empty<ISolution>();

            // Fixed Graph Name
            context = context.PushActiveGraph(namedGraph.Graph);
            return namedGraph.InnerAlgebra.Execute(this, context);
        }

        public IEnumerable<ISolution> Execute(Filter filter, IExecutionContext context)
        {
            IEnumerable<ISolution> innerResult = filter.InnerAlgebra.Execute(this, context);
            return filter.Expressions.Count > 0 ? new FilterEnumerable(innerResult, filter.Expressions, context) : innerResult;
        }

        public IEnumerable<ISolution> Execute(Table table, IExecutionContext context)
        {
            return table.IsEmpty ? Enumerable.Empty<ISolution>() : table.Data;
        }

        public IEnumerable<ISolution> Execute(Join join, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISolution> lhsResults = join.Lhs.Execute(this, context);
            IEnumerable<ISolution> rhsResults = join.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, this.JoinStrategySelector.Select(join.Lhs, join.Rhs), context);
        }

        public IEnumerable<ISolution> Execute(LeftJoin leftJoin, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISolution> lhsResults = leftJoin.Lhs.Execute(this, context);
            IEnumerable<ISolution> rhsResults = leftJoin.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, new LeftJoinStrategy(this.JoinStrategySelector.Select(leftJoin.Lhs, leftJoin.Rhs), leftJoin.Expressions), context);
        }

        public IEnumerable<ISolution> Execute(Minus minus, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISolution> lhsResults = minus.Lhs.Execute(this, context);
            IEnumerable<ISolution> rhsResults = minus.Rhs.Execute(this, context);

            return new JoinEnumerable(lhsResults, rhsResults, new NonExistenceJoinStrategy(this.JoinStrategySelector.Select(minus.Lhs, minus.Rhs)), context);
        }

        public IEnumerable<ISolution> Execute(Distinct distinct, IExecutionContext context)
        {
            context = EnsureContext(context);
            // TODO Likely want to provide an optimized IEqualityComparer here
            return distinct.InnerAlgebra.Execute(this, context).Distinct();
        }

        public IEnumerable<ISolution> Execute(Reduced reduced, IExecutionContext context)
        {
            context = EnsureContext(context);
            return reduced.InnerAlgebra.Execute(this, context).Reduced();
        }

        public IEnumerable<ISolution> Execute(Project project, IExecutionContext context)
        {
            context = EnsureContext(context);
            return project.InnerAlgebra.Execute(this, context).Select(s => s.Project(project.Projections));
        }

        public IEnumerable<ISolution> Execute(OrderBy orderBy, IExecutionContext context)
        {
            context = EnsureContext(context);
            return orderBy.InnerAlgebra.Execute(this, context).OrderBy(s => s, new SortConditionApplicator(orderBy.SortConditions));
        }

        public IEnumerable<ISolution> Execute(Extend extend, IExecutionContext context)
        {
            context = EnsureContext(context);
            IEnumerable<ISolution> innerResults = extend.InnerAlgebra.Execute(this, context);
            return new ExtendEnumerable(innerResults, extend.Assignments, context);
        }

        public IEnumerable<ISolution> Execute(GroupBy groupBy, IExecutionContext context)
        {
            throw new NotImplementedException("Group By execution is not yet implemented");
        }

        public IEnumerable<ISolution> Execute(Service service, IExecutionContext context)
        {
            if (service.IsSilent) return Enumerable.Empty<ISolution>();
            throw new NotSupportedException("Federated Queries (SERVICE clause) are not supported by this algebra executor");
        }

        public IEnumerable<ISolution> Execute(PropertyPath path, IExecutionContext context)
        {
            throw new NotImplementedException("Property path execution is not yet implemented");
        }

        public IEnumerable<ISolution> Execute(TopN topN, IExecutionContext context)
        {
            context = EnsureContext(context);
            return topN.InnerAlgebra.Execute(this, context).Top(new SortConditionApplicator(topN.SortConditions), topN.N);
        }

        public IEnumerable<ISolution> Execute(PropertyFunction propertyFunction, IExecutionContext context)
        {
            throw new NotImplementedException("Property function execution is not yet implemented");
        }
    }
}