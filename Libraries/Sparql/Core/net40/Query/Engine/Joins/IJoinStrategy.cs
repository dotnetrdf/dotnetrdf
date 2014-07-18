using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Joins
{
    /// <summary>
    /// Interface for join strategies
    /// </summary>
    public interface IJoinStrategy
    {
        /// <summary>
        /// Prepares a new join worker
        /// </summary>
        /// <param name="rhs">The enumerable that represents the RHS of the join</param>
        /// <returns>Join worker</returns>
        IJoinWorker PrepareWorker(IEnumerable<ISet> rhs);

        /// <summary>
        /// Joins the two sets together
        /// </summary>
        /// <param name="lhs">Left hand side set</param>
        /// <param name="rhs">Right hand side set</param>
        /// <returns>Joined set</returns>
        ISet Join(ISet lhs, ISet rhs);
    }
}
