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
        INode ActiveGraph { get; }

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
    }
}
