using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Algebra
{
    public interface IAlgebraExecutor
    {
        IEnumerable<ISet> Execute(IAlgebra algebra);

        IEnumerable<ISet> Execute(IAlgebra algebra, IExecutionContext context);

        IEnumerable<ISet> Execute(Bgp bgp, IExecutionContext context);

        IEnumerable<ISet> Execute(Slice slice, IExecutionContext context);

        IEnumerable<ISet> Execute(Union union, IExecutionContext context);

        IEnumerable<ISet> Execute(NamedGraph namedGraph, IExecutionContext context);

        IEnumerable<ISet> Execute(Filter filter, IExecutionContext context);

        IEnumerable<ISet> Execute(Table table, IExecutionContext context);

        IEnumerable<ISet> Execute(Join join, IExecutionContext context);

        IEnumerable<ISet> Execute(LeftJoin join, IExecutionContext context);
    }
}
