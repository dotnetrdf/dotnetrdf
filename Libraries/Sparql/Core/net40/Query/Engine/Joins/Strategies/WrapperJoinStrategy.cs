using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// A decorator for join strategies
    /// </summary>
    public abstract class WrapperJoinStrategy
        : IJoinStrategy
    {
        protected WrapperJoinStrategy(IJoinStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            this.InnerStrategy = strategy;
        }

        public IJoinStrategy InnerStrategy { get; private set; }

        public virtual IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs)
        {
            return this.InnerStrategy.PrepareWorker(rhs);
        }

        public virtual ISolution Join(ISolution lhs, ISolution rhs)
        {
            return this.InnerStrategy.Join(lhs, rhs);
        }
    }
}
