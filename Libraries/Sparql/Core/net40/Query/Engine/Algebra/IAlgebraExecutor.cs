using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Algebra
{
    /// <summary>
    /// Interface for algebra executors
    /// </summary>
    public interface IAlgebraExecutor
    {
        /// <summary>
        /// Executes the algebra using a new default context
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns>Enumerable of solutions</returns>
        IEnumerable<ISet> Execute(IAlgebra algebra);

        /// <summary>
        /// Executes the algebra using the given context (or a new default context if it is null)
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Execution context</param>
        /// <returns>Enumberable of solutions</returns>
        IEnumerable<ISet> Execute(IAlgebra algebra, IExecutionContext context);

        IEnumerable<ISet> Execute(Bgp bgp, IExecutionContext context);

        IEnumerable<ISet> Execute(Slice slice, IExecutionContext context);

        IEnumerable<ISet> Execute(Union union, IExecutionContext context);

        IEnumerable<ISet> Execute(NamedGraph namedGraph, IExecutionContext context);

        IEnumerable<ISet> Execute(Filter filter, IExecutionContext context);

        IEnumerable<ISet> Execute(Table table, IExecutionContext context);

        IEnumerable<ISet> Execute(Join join, IExecutionContext context);

        IEnumerable<ISet> Execute(LeftJoin join, IExecutionContext context);

        IEnumerable<ISet> Execute(Minus minus, IExecutionContext context);

        IEnumerable<ISet> Execute(Distinct distinct, IExecutionContext context);

        IEnumerable<ISet> Execute(Reduced reduced, IExecutionContext context);

        IEnumerable<ISet> Execute(Project project, IExecutionContext context);

        IEnumerable<ISet> Execute(OrderBy orderBy, IExecutionContext context);

        IEnumerable<ISet> Execute(Extend extend, IExecutionContext context);

        IEnumerable<ISet> Execute(GroupBy groupBy, IExecutionContext context);

        IEnumerable<ISet> Execute(Service service, IExecutionContext context);

        IEnumerable<ISet> Execute(PropertyPath path, IExecutionContext context);

        IEnumerable<ISet> Execute(TopN topN, IExecutionContext context);

        IEnumerable<ISet> Execute(PropertyFunction propertyFunction, IExecutionContext context);

        IEnumerable<ISet> Execute(IndexJoin indexJoin, IExecutionContext context);
    }
}
