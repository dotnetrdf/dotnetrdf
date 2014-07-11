using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Results
{
    public interface IQueryResults
    {
        /// <summary>
        /// Gets whether this result represents a tabular result
        /// </summary>
        bool IsTabular { get; }

        /// <summary>
        /// Gets whether this result represents a graph result
        /// </summary>
        bool IsGraph { get; }

        /// <summary>
        /// Gets whether this result represents a boolean result
        /// </summary>
        bool IsBoolean { get; }

        /// <summary>
        /// Gets the tabular results (if any)
        /// </summary>
        ITabularResults Table { get; }

        /// <summary>
        /// Gets the graph result (if any)
        /// </summary>
        IGraph Graph { get; }

        /// <summary>
        /// Gets the boolean result (if any)
        /// </summary>
        bool? Boolean { get; }
    }
}
