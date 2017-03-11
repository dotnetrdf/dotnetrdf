namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building SPARQL expressions, including aggregates
    /// </summary>
    public interface IExpressionBuilder : INonAggregateExpressionBuilder, IAggregateBuilder
    {
    }
}