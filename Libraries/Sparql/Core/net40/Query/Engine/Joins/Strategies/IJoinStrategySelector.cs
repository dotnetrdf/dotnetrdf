using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Joins.Strategies
{
    /// <summary>
    /// Interface for join strategy selectors
    /// </summary>
    /// <remarks>
    /// A join strategy selector embodies the logic for deciding what join strategy should be used depending on the operators involved
    /// </remarks>
    public interface IJoinStrategySelector
    {
        /// <summary>
        /// Selects a join strategy for the given algebra operators
        /// </summary>
        /// <param name="lhs">Left hand side operator</param>
        /// <param name="rhs">Right hand side operator</param>
        /// <returns></returns>
        IJoinStrategy Select(IAlgebra lhs, IAlgebra rhs);
    }
}
