using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Grouping
{
    /// <summary>
    /// Interface for accumulators which are used to caculate aggregates in a streaming fashion
    /// </summary>
    public interface IAccumulator
        : IEquatable<IAccumulator>
    {
        /// <summary>
        /// Accumulates the solution
        /// </summary>
        /// <param name="solution">Solution</param>
        /// <param name="context">Expression Context</param>
        void Accumulate(ISolution solution, IExpressionContext context);

        /// <summary>
        /// Gets the accumulated result
        /// </summary>
        IValuedNode AccumulatedResult { get; }
    }
}
