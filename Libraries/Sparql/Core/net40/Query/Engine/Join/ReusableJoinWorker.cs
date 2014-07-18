using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Join
{
    /// <summary>
    /// Abstract implementation of a reusable join worker
    /// </summary>
    public abstract class ReusableJoinWorker
        : IJoinWorker
    {
        public abstract IEnumerable<ISet> Find(ISet lhs, IExecutionContext context);

        /// <summary>
        /// Always return true since the work is always reusable
        /// </summary>
        /// <param name="s">Left hand side set</param>
        /// <param name="context"></param>
        /// <returns>True</returns>
        public bool CanReuse(ISet s, IExecutionContext context)
        {
            return true;
        }
    }
}
