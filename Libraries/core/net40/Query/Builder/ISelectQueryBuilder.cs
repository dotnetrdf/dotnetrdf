namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for creating SELECT queries
    /// </summary>
    public interface ISelectQueryBuilder : ICommonQueryBuilder
    {
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        ISelectQueryBuilder Distinct();
        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        ISelectQueryBuilder And(params SparqlVariable[] variables);
        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        ISelectQueryBuilder And(params string[] variables);
    }
}