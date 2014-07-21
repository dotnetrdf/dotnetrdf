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
        IEnumerable<ISolution> Execute(IAlgebra algebra);

        /// <summary>
        /// Executes the algebra using the given context (or a new default context if it is null)
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Execution context</param>
        /// <returns>Enumberable of solutions</returns>
        IEnumerable<ISolution> Execute(IAlgebra algebra, IExecutionContext context);

        IEnumerable<ISolution> Execute(Bgp bgp, IExecutionContext context);

        IEnumerable<ISolution> Execute(Slice slice, IExecutionContext context);

        IEnumerable<ISolution> Execute(Union union, IExecutionContext context);

        IEnumerable<ISolution> Execute(NamedGraph namedGraph, IExecutionContext context);

        IEnumerable<ISolution> Execute(Filter filter, IExecutionContext context);

        IEnumerable<ISolution> Execute(Table table, IExecutionContext context);

        IEnumerable<ISolution> Execute(Join join, IExecutionContext context);

        IEnumerable<ISolution> Execute(LeftJoin join, IExecutionContext context);

        IEnumerable<ISolution> Execute(Minus minus, IExecutionContext context);

        IEnumerable<ISolution> Execute(Distinct distinct, IExecutionContext context);

        IEnumerable<ISolution> Execute(Reduced reduced, IExecutionContext context);

        IEnumerable<ISolution> Execute(Project project, IExecutionContext context);

        IEnumerable<ISolution> Execute(OrderBy orderBy, IExecutionContext context);

        IEnumerable<ISolution> Execute(Extend extend, IExecutionContext context);

        IEnumerable<ISolution> Execute(GroupBy groupBy, IExecutionContext context);

        IEnumerable<ISolution> Execute(Service service, IExecutionContext context);

        IEnumerable<ISolution> Execute(PropertyPath path, IExecutionContext context);

        IEnumerable<ISolution> Execute(TopN topN, IExecutionContext context);

        IEnumerable<ISolution> Execute(PropertyFunction propertyFunction, IExecutionContext context);
    }
}
