using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    /// <summary>
    /// A decorator for join workers
    /// </summary>
    public abstract class WrapperJoinWorker
        : IJoinWorker
    {
        public WrapperJoinWorker(IJoinWorker worker)
        {
            if (worker == null) throw new ArgumentNullException();
            this.InnerWorker = worker;
        }

        public IJoinWorker InnerWorker { get; private set; }

        public virtual IEnumerable<ISet> Find(ISet lhs)
        {
            return this.InnerWorker.Find(lhs);
        }

        public bool CanReuse(ISet s)
        {
            return this.InnerWorker.CanReuse(s);
        }
    }
}
