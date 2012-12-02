using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building graph patterns
    /// </summary>
    public interface IGraphPatternBuilder : ICommonQueryBuilder<IGraphPatternBuilder>
    {
        /// <summary>
        /// Creates a UNION of the current graph pattern and a new one
        /// </summary>
        IGraphPatternBuilder Union(Action<IGraphPatternBuilder> buildGraphPattern);
    }
}