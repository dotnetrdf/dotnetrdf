using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    public class ProductJoinWorker
        : ReusableJoinWorker
    {
        public ProductJoinWorker(IEnumerable<ISet> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            this.Rhs = rhs;
        }

        public IEnumerable<ISet> Rhs { get; private set; }

        public override IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            return this.Rhs;
        }
    }
}
