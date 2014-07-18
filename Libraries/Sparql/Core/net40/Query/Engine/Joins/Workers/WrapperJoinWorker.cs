using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    /// <summary>
    /// A decorator for join workers
    /// </summary>
    public abstract class WrapperJoinWorker
        : IJoinWorker
    {
        protected WrapperJoinWorker(IJoinWorker worker)
        {
            if (worker == null) throw new ArgumentNullException();
            this.InnerWorker = worker;
        }

        public IJoinWorker InnerWorker { get; private set; }

        public virtual IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            return this.InnerWorker.Find(lhs, context);
        }

        public bool CanReuse(ISet s, IExecutionContext context)
        {
            return this.InnerWorker.CanReuse(s, context);
        }
    }
}
