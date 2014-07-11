using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Interface for SPARQL Queries
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets/Sets the namespaces for the query
        /// </summary>
        INamespaceMapper Namespaces { get; set; }

        /// <summary>
        /// Gets/Sets the Base URI for the Query
        /// </summary>
        Uri BaseUri { get; set; }

        /// <summary>
        /// Gets/Sets the default graphs for the query
        /// </summary>
        IEnumerable<INode> DefaultGraphs { get; set; }

        /// <summary>
        /// Gets/Sets the named graphs for the query
        /// </summary>
        IEnumerable<INode> NamedGraphs { get; set; }

        /// <summary>
        /// Adds a default graph to the query
        /// </summary>
        /// <param name="graphName">Graph name</param>
        void AddDefaultGraph(INode graphName);

        /// <summary>
        /// Removes a default graph from the query
        /// </summary>
        /// <param name="graphName">Graph name</param>
        void RemoveDefaultGraph(INode graphName);

        /// <summary>
        /// Clears the default graphs from the query
        /// </summary>
        void ClearDefaultGraphs();

        /// <summary>
        /// Adds a named graph to the query
        /// </summary>
        /// <param name="graphName">Graph name</param>
        void AddNamedGraph(INode graphName);

        /// <summary>
        /// Removes a named graph from the query
        /// </summary>
        /// <param name="graphName">Graph name</param>
        void RemoveNamedGraph(INode graphName);

        /// <summary>
        /// Clears named graphs
        /// </summary>
        void ClearNamedGraphs();

        /// <summary>
        /// Gets/Sets the Query Type
        /// </summary>
        QueryType QueryType { get; set; }

        /// <summary>
        /// Gets/Sets the limit for the query
        /// </summary>
        long Limit { get; set; }

        /// <summary>
        /// Gets/Sets the offset for the query
        /// </summary>
        long Offset { get; set; }

        /// <summary>
        /// Gets whether there is a limit on this query
        /// </summary>
        /// <returns>True if the limit is greater than or equal to zero, false otherwise</returns>
        bool HasLimit { get; }

        /// <summary>
        /// Gets whether there is an offset on this query
        /// </summary>
        /// <returns>True if the offset is greater than zero, false otherwise</returns>
        bool HasOffset { get; }

        /// <summary>
        /// Gets/Sets the sort conditions that make up the ORDER BY for the query
        /// </summary>
        IEnumerable<ISortCondition> SortConditions { get; set; }

        /// <summary>
        /// Adds a sort condition to the query
        /// </summary>
        /// <param name="condition">Sort Condition</param>
        void AddSortCondition(ISortCondition condition);

        /// <summary>
        /// Removes the sort conditions from the query
        /// </summary>
        void ClearSortConditions();
    }
}
