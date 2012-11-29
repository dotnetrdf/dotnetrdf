using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Common interface for building SPARQL queries and graph patterns
    /// </summary>
    public interface ICommonQueryBuilder
    {
        /// <summary>
        /// Gets the prefix manager, which allows adding prefixes to the query or graph pattern
        /// </summary>
        INamespaceMapper Prefixes { get; }
    }

    /// <summary>
    /// Common interface for building SPARQL queries and graph patterns
    /// </summary>
    public interface ICommonQueryBuilder<TReturnBuilder> : ICommonQueryBuilder where TReturnBuilder : ICommonQueryBuilder
    {
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Where(params ITriplePattern[] triplePatterns);
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Adds an OPTIONAL graph pattern to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a FILTER to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Filter(Func<ExpressionBuilder, BooleanExpression> expr);
        /// <summary>
        /// Adds a FILTER expression to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Filter(ISparqlExpression expr);
        /// <summary>
        /// Adds a MINUS graph pattern to the SPARQL query or graph pattern
        /// </summary>
        TReturnBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a BIND variable assignment to the SPARQL query or graph pattern
        /// </summary>
        AssignmentVariableNamePart<TReturnBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);

        TReturnBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern);
    }
}