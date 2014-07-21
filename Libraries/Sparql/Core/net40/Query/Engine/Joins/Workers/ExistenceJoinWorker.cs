using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class ExistenceJoinWorker
        : WrapperJoinWorker
    {
        public ExistenceJoinWorker(IJoinWorker worker) 
            : base(worker) { }

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            return base.Find(lhs, context).Any() ? new Solution().AsEnumerable() : Enumerable.Empty<ISolution>();
        }
    }

    public class NonExistenceJoinWorker
        : WrapperJoinWorker
    {
        public NonExistenceJoinWorker(IJoinWorker worker) 
            : base(worker) { }

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            return base.Find(lhs, context).Any() ? Enumerable.Empty<ISolution>() : new Solution().AsEnumerable();
        }
    }
}
