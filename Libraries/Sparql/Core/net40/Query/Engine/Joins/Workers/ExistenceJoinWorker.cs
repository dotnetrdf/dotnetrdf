using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class ExistenceJoinWorker
        : WrapperJoinWorker
    {
        public ExistenceJoinWorker(IJoinWorker worker) 
            : base(worker) { }

        public override IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            return base.Find(lhs, context).Any() ? new Set().AsEnumerable() : Enumerable.Empty<ISet>();
        }
    }

    public class NonExistenceJoinWorker
        : WrapperJoinWorker
    {
        public NonExistenceJoinWorker(IJoinWorker worker) 
            : base(worker) { }

        public override IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            return base.Find(lhs, context).Any() ? Enumerable.Empty<ISet>() : new Set().AsEnumerable();
        }
    }
}
