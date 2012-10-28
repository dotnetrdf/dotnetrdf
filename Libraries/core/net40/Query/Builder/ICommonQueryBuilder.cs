using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Common interface for building SPARQL queries 
    /// </summary>
    public interface ICommonQueryBuilder
    {
        /// <summary>
        /// Adds triple patterns to the SPARQL query
        /// </summary>
        IQueryBuilder Where(params ITriplePattern[] triplePatterns);
        /// <summary>
        /// Adds triple patterns to the SPARQL query
        /// </summary>
        IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Adds an OPTIONAL graph pattern to the SPARQL query
        /// </summary>
        IQueryBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);

        [Obsolete("Introduce IExpressionBuilder (overload)")]
        IQueryBuilder Filter(ISparqlExpression expr);

        /// <summary>
        /// Gets the prefix manager, which allows adding prefixes to the query
        /// </summary>
        INamespaceMapper Prefixes { get; }
    }
}