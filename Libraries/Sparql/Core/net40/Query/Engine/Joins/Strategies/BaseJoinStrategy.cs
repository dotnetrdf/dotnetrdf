using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// Abstract join strategy that does a straight join between sets to be joined
    /// </summary>
    public abstract class BaseJoinStrategy 
        : IJoinStrategy
    {
        public abstract IJoinWorker PrepareWorker(IEnumerable<ISet> rhs);

        public ISet Join(ISet lhs, ISet rhs)
        {
            return lhs.Join(rhs);
        }
    }
}