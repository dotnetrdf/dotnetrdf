using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// A wrapper for other join strategies that causes the RHS to be materialized whenever a worker is prepared
    /// </summary>
    public class MaterializedJoinStrategy
        : WrapperJoinStrategy
    {
        public MaterializedJoinStrategy(IJoinStrategy strategy) 
            : base(strategy) {}

        public override IJoinWorker PrepareWorker(IEnumerable<ISet> rhs)
        {
            return base.PrepareWorker(rhs.ToList());
        }
    }
}