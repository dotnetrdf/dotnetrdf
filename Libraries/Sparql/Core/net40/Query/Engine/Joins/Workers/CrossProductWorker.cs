using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class CrossProductWorker
        : ReusableJoinWorker
    {
        public CrossProductWorker(IEnumerable<ISolution> rhs)
        {
            if (rhs == null) throw new ArgumentNullException("rhs");
            this.Rhs = rhs;
        }

        public IEnumerable<ISolution> Rhs { get; private set; }

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            return this.Rhs;
        }
    }
}
