using VDS.RDF.Query.Aggregates;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a SPARQL aggregate
    /// </summary>
    public class AggregateExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateExpression"/> class.
        /// </summary>
        /// <param name="aggregate">The base aggregate.</param>
        public AggregateExpression(ISparqlAggregate aggregate)
        {
            Aggregate = aggregate;
        }

        /// <summary>
        /// The undelrying aggregate
        /// </summary>
        public ISparqlAggregate Aggregate { get; }
    }
}