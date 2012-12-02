using System;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for building SPARQL queries 
    /// </summary>
    public interface IQueryBuilder : ICommonQueryBuilder<IQueryBuilder>
    {
        /// <summary>
        /// Gets the prefix manager, which allows adding prefixes to the query or graph pattern
        /// </summary>
        INamespaceMapper Prefixes { get; set; }
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        IQueryBuilder Distinct();
        /// <summary>
        /// Applies a LIMIT
        /// </summary>
        /// <param name="limit">Limit value. Pass negative to disable LIMIT</param>
        IQueryBuilder Limit(int limit);
        /// <summary>
        /// Applies an OFFSET
        /// </summary>
        IQueryBuilder Offset(int offset);

        IQueryBuilder OrderBy(string variableName);

        IQueryBuilder OrderByDescending(string variableName);

        IQueryBuilder OrderBy(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression);

        IQueryBuilder OrderByDescending(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression);
        /// <summary>
        /// Builds and returns a <see cref="SparqlQuery"/>
        /// </summary>
        SparqlQuery BuildQuery();
    }
}