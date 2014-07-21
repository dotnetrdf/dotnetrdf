using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// Abstract join strategy that does a straight join between sets to be joined
    /// </summary>
    public abstract class BaseJoinStrategy 
        : IJoinStrategy
    {
        public abstract IJoinWorker PrepareWorker(IEnumerable<ISolution> rhs);

        public ISolution Join(ISolution lhs, ISolution rhs)
        {
            return lhs.Join(rhs);
        }
    }
}