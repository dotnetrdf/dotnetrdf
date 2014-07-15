using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Medusa
{
    public class MedusaAlgebraExecutor
        : IAlgebraExecutor
    {
        public IEnumerable<ISet> Execute(IAlgebra algebra)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(Bgp bgp)
        {
            List<Triple> patterns = bgp.TriplePatterns.ToList();
            if (patterns.Count == 0) return new Set().AsEnumerable();

            throw new NotImplementedException();
        }

        public IEnumerable<ISet> Execute(Slice slice)
        {
            if (slice.Limit == 0) return Enumerable.Empty<ISet>();

            IEnumerable<ISet> innerResult = this.Execute(slice.InnerAlgebra);
            if (slice.Limit > 0)
            {
                return slice.Offset > 0 ? innerResult.Skip(slice.Offset).Take(slice.Limit) : innerResult.Take(slice.Limit);
            }
            return slice.Offset > 0 ? innerResult.Skip(slice.Offset) : innerResult;
        }
    }
}
