using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine
{
    public interface IAlgebraExecutor
    {
        IEnumerable<ISet> Execute(IAlgebra algebra);

        IEnumerable<ISet> Execute(Bgp bgp);

        IEnumerable<ISet> Execute(Slice slice);
    }
}
