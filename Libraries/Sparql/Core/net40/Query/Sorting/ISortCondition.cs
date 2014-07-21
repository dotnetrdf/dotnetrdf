using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Sorting
{
    public interface ISortCondition
        : IComparer<ISet>, IEquatable<ISortCondition>
    {
        /// <summary>
        /// Gets whether this is an ascending sort condition
        /// </summary>
        /// <returns>True if an ascending sort condition, false if a descending sort condition</returns>
        bool IsAscending { get; }
    }
}
