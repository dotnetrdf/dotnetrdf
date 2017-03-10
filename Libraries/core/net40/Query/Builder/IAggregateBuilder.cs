using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating aggregates expressions
    /// </summary>
    public interface IAggregateBuilder
    {
        /// <summary>
        /// Gets a builder which builds a DISTICT aggregate
        /// </summary>
        IAggregateBuilder Distinct { get; }

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(VariableTerm variable);

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(string variable);

        /// <summary>
        /// Creates a SUM aggregate
        /// </summary>
        AggregateExpression Sum(SparqlExpression expression);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(VariableTerm variable);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(string variable);

        /// <summary>
        /// Creates a AVG aggregate
        /// </summary>
        AggregateExpression Avg(SparqlExpression expression);
    }
}
