using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Common interface for building SPARQL queries 
    /// </summary>
    public interface ICommonQueryBuilder<out TReturnBuilder>
    {
        /// <summary>
        /// Adds triple patterns to the SPARQL query
        /// </summary>
        TReturnBuilder Where(params ITriplePattern[] triplePatterns);
        /// <summary>
        /// Adds triple patterns to the SPARQL query
        /// </summary>
        TReturnBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Adds an OPTIONAL graph pattern to the SPARQL query
        /// </summary>
        TReturnBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a FILTER to the SPARQL query
        /// </summary>
        TReturnBuilder Filter(Func<ExpressionBuilder, BooleanExpression> expr);
        /// <summary>
        /// Gets the prefix manager, which allows adding prefixes to the query
        /// </summary>
        INamespaceMapper Prefixes { get; }
    }
}