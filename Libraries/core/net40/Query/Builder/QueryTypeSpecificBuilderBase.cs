using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal abstract class QueryTypeSpecificBuilderBase : ICommonQueryBuilder<IQueryBuilder>
    {
        internal abstract SparqlQueryType SparqlQueryType { get; }

        public IQueryBuilder Where(params ITriplePattern[] triplePatterns)
        {
            return CreateQueryBuilder().Where(triplePatterns);
        }

        public IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            return CreateQueryBuilder().Where(buildTriplePatterns);
        }

        public IQueryBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Optional(buildGraphPattern);
        }

        public IQueryBuilder Filter(Func<ExpressionBuilder, BooleanExpression> expr)
        {
            return CreateQueryBuilder().Filter(expr);
        }

        public IQueryBuilder Filter(ISparqlExpression expr)
        {
            return CreateQueryBuilder().Filter(expr);
        }

        public IQueryBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Minus(buildGraphPattern);
        }

        public IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return CreateQueryBuilder().Bind(buildAssignmentExpression);
        }

        public IQueryBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return CreateQueryBuilder().Child(buildGraphPattern);
        }

        protected abstract QueryBuilder CreateQueryBuilder();
    }
}