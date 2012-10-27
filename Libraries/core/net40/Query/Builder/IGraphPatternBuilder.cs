using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public interface IGraphPatternBuilder
    {
        IGraphPatternBuilder Where(params ITriplePattern[] triplePatterns);
        IGraphPatternBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);
        IGraphPatternBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern);
        [Obsolete("Introduce IExpressionBuilder (overload)")]
        IGraphPatternBuilder Filter(ISparqlExpression expr);

        IGraphPatternBuilder Optional(params ITriplePattern[] triplePatterns);
    }
}