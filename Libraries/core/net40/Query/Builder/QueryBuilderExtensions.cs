using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public static class QueryBuilderExtensions
    {       
        public static IQueryBuilder Where(this IQueryWithVariablesBuilder describeBuilder, params ITriplePattern[] triplePatterns)
        {
            return describeBuilder.GetQueryBuilder().Where(triplePatterns);
        }

        public static IQueryBuilder Where(this IQueryWithVariablesBuilder describeBuilder, Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            return describeBuilder.GetQueryBuilder().Where(buildTriplePatterns);
        }

        public static IQueryBuilder Optional(this IQueryWithVariablesBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Optional(buildGraphPattern);
        }

        public static IQueryBuilder Filter(this IQueryWithVariablesBuilder describeBuilder, Func<ExpressionBuilder, BooleanExpression> expr)
        {
            return describeBuilder.GetQueryBuilder().Filter(expr);
        }

        public static IQueryBuilder Filter(this IQueryWithVariablesBuilder describeBuilder, ISparqlExpression expr)
        {
            return describeBuilder.GetQueryBuilder().Filter(expr);
        }

        public static IQueryBuilder Minus(this IQueryWithVariablesBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Minus(buildGraphPattern);
        }

        public static IAssignmentVariableNamePart<IQueryBuilder> Bind(this IDescribeBuilder describeBuilder, Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return describeBuilder.GetQueryBuilder().Bind(buildAssignmentExpression);
        }

        public static IQueryBuilder Child(this IDescribeBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Child(buildGraphPattern);
        }

        /// <summary>
        /// Builds a simple DESCRIBE query without the WHERE part
        /// </summary>
        public static SparqlQuery BuildQuery(this IDescribeBuilder describeBuilder)
        {
            return describeBuilder.GetQueryBuilder().BuildQuery();
        }

        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Child(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, params ITriplePattern[] triplePatterns)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Where(triplePatterns);
            return queryBuilder;
        }

        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Where(buildTriplePatterns);
            return queryBuilder;
        }

        internal static IQueryBuilder Where(this IQueryBuilder queryBuilder, Func<INamespaceMapper, ITriplePattern[]> buildTriplePatternFunc)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Where(buildTriplePatternFunc);
            return queryBuilder;
        }

        public static IQueryBuilder Optional(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Optional(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, ISparqlExpression expr)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Filter(expr);
            return queryBuilder;
        }

        public static IQueryBuilder Minus(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Minus(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, Func<ExpressionBuilder, BooleanExpression> buildExpression)
        {
            ((QueryBuilder)queryBuilder).RootGraphPatternBuilder.Filter(buildExpression);
            return queryBuilder;
        }
    }
}