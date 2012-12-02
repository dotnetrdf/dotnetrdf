using System;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for building SPARQL queries 
    /// </summary>
    public interface IQueryBuilder
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
        /// <summary>
        /// Adds ascending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderBy(string variableName);
        /// <summary>
        /// Adds descending ordering by a variable to the query
        /// </summary>
        IQueryBuilder OrderByDescending(string variableName);
        /// <summary>
        /// Adds ascending ordering by an expression to the query
        /// </summary>
        IQueryBuilder OrderBy(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression);
        /// <summary>
        /// Adds descending ordering by an expression to the query
        /// </summary>
        IQueryBuilder OrderByDescending(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression);
        /// <summary>
        /// Builds and returns a <see cref="SparqlQuery"/>
        /// </summary>
        SparqlQuery BuildQuery();
        /// <summary>
        /// Adds a BIND variable assignment to the root graph pattern
        /// </summary>
        IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);
    }
}