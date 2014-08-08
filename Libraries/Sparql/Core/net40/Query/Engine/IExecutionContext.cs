using System;
using System.Collections.Generic;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine
{
    /// <summary>
    /// Interface for query engine execution context
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// Gets the current Active Graph
        /// </summary>
        /// <remarks>
        /// This is either an actual graph name of the special value <see cref="Quad.DefaultGraphNode"/> used to indicate that the query default graph is used.  When set to the latter value then the <see cref="DefaultGraphs"/> property is used to indicate what graphs form the actual query default graph.
        /// </remarks>
        INode ActiveGraph { get; }

        /// <summary>
        /// Gets the defualt graphs for the execution context
        /// </summary>
        /// <remarks>
        /// If this enumerable returns zero results then the query must behave default graph is empty.  If it contains any values then the query must behave as if the default graph is the merge of all the mentioned graphs.  If it contains only the special value <see cref="Quad.DefaultGraphNode"/> then the default graph is the one provided by the underlying dataset.
        /// </remarks>
        ICollection<INode> DefaultGraphs { get; }
            
        /// <summary>
        /// Gets the named graphs for the execution context
        /// </summary>
        /// <remarks>
        /// If this enumerable returns zero results then the dataset must be treated as if it contains no named graphs
        /// </remarks>
        ICollection<INode> NamedGraphs { get; }
            
        /// <summary>
        /// Creates a new execution context with the given Active Graph
        /// </summary>
        /// <param name="graphName">Graph Name</param>
        /// <returns>New execution context</returns>
        IExecutionContext PushActiveGraph(INode graphName);

        /// <summary>
        /// Creates a new expression context
        /// </summary>
        /// <returns>New expression context</returns>
        IExpressionContext CreateExpressionContext();

        /// <summary>
        /// Gets the effective now for the purposes of the execution
        /// </summary>
        /// <remarks>
        /// Must return a constant value throughout the life of the execution, may be late bound i.e. does not need to be initialized until the first time it is accessed
        /// </remarks>
        DateTimeOffset EffectiveNow { get; }
    }
}
