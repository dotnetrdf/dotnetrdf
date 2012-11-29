using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building graph patterns
    /// </summary>
    public interface IGraphPatternBuilder : ICommonQueryBuilder<IGraphPatternBuilder>
    {
        IGraphPatternBuilder Union(Action<IGraphPatternBuilder> buildGraphPattern);
    }
}