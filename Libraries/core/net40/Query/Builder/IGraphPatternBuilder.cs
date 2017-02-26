using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building graph patterns
    /// </summary>
    public interface IGraphPatternBuilder
    {
        /// <summary>
        /// Creates a UNION of multiple graph patterns. If <paramref name="unionedGraphPatternBuilders"/> is null or empty,
        /// acts as a call to the <see cref="Child"/> method.
        /// </summary>
        IGraphPatternBuilder Union(Action<IGraphPatternBuilder> buildFirstGraphPattern, params Action<IGraphPatternBuilder>[] unionedGraphPatternBuilders);
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Where(params ITriplePattern[] triplePatterns);
        /// <summary>
        /// Adds triple patterns to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        /// <summary>
        /// Adds an OPTIONAL graph pattern to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a FILTER to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Filter(Func<ExpressionBuilder, BooleanExpression> expr);
        /// <summary>
        /// Adds a FILTER expression to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Filter(ISparqlExpression expr);
        /// <summary>
        /// Adds a MINUS graph pattern to the SPARQL query or graph pattern
        /// </summary>
        IGraphPatternBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a GRAPH graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Graph(Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a GRAPH graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Graph(string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a SERVICE graph pattern to the graph pattern
        /// </summary>
        IGraphPatternBuilder Service(Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern);
        /// <summary>
        /// Adds a BIND variable assignment to the graph pattern
        /// </summary>
        IAssignmentVariableNamePart<IGraphPatternBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);
        /// <summary>
        /// Addsa "normal" child graph pattern
        /// </summary>
        IGraphPatternBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern);
    }
}