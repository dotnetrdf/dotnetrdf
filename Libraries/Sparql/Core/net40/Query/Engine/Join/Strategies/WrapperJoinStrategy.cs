using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Join.Strategies
{
    /// <summary>
    /// A decorator for join strategies
    /// </summary>
    public abstract class WrapperJoinStrategy
        : IJoinStrategy
    {
        public WrapperJoinStrategy(IJoinStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            this.InnerStrategy = strategy;
        }

        public IJoinStrategy InnerStrategy { get; private set; }

        public virtual IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return this.InnerStrategy.PrepareWorker(rhs);
        }

        public virtual ISet Join(ISet lhs, ISet rhs)
        {
            return this.InnerStrategy.Join(lhs, rhs);
        }
    }
}
