using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine
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
    }
}
