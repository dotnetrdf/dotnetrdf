using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Interface for sort conditions
    /// </summary>
    public interface ISortCondition
        : IComparer<ISolution>, IEquatable<ISortCondition>
    {
        /// <summary>
        /// Gets whether this is an ascending sort condition
        /// </summary>
        /// <returns>True if an ascending sort condition, false if a descending sort condition</returns>
        bool IsAscending { get; }

        String ToString();

        String ToString(INodeFormatter formatter);
    }
}
