using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Join
{
    /// <summary>
    /// Interface for join workers
    /// </summary>
    public interface IJoinWorker
    {
        /// <summary>
        /// Finds all sets that can be joined with the given left hand side set
        /// </summary>
        /// <param name="lhs">Left hand side set</param>
        /// <returns>Enumerable of sets from the right hand side</returns>
        IEnumerable<ISet> Find(ISet lhs);

        /// <summary>
        /// Gets whether this worker can be reused for the given set
        /// </summary>
        /// <param name="s">Set</param>
        /// <returns>True if the worker can be reused, false otherwise</returns>
        bool CanReuse(ISet s);
    }
}
