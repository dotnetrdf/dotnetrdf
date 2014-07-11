using System.Collections.Generic;

namespace VDS.RDF.Query.Engine
{
    public interface ISortCondition
        : IComparer<ISet>
    {
        /// <summary>
        /// Gets whether this is an ascending sort condition
        /// </summary>
        /// <returns>True if an ascending sort condition, false if a descending sort condition</returns>
        bool IsAscending { get; }
    }
}
