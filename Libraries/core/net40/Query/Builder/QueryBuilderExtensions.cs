/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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

        public static IQueryBuilder Filter(this IQueryWithVariablesBuilder describeBuilder, Func<INonAggregateExpressionBuilder, BooleanExpression> expr)
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

        public static IQueryBuilder Graph(this IQueryWithVariablesBuilder describeBuilder, Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Graph(graphUri, buildGraphPattern);
        }

        public static IQueryBuilder Graph(this IQueryWithVariablesBuilder describeBuilder, string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Graph(graphVariable, buildGraphPattern);
        }

        public static IQueryBuilder Service(this IQueryWithVariablesBuilder describeBuilder, Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Service(serviceUri, buildGraphPattern);
        }

        public static IAssignmentVariableNamePart<IQueryBuilder> Bind(this IDescribeBuilder describeBuilder, Func<INonAggregateExpressionBuilder, PrimaryExpression<ISparqlExpression>> buildAssignmentExpression)
        {
            return describeBuilder.GetQueryBuilder().Bind(buildAssignmentExpression);
        }

        /// <summary>
        /// Builds a simple DESCRIBE query without the WHERE part
        /// </summary>
        public static SparqlQuery BuildQuery(this IDescribeBuilder describeBuilder)
        {
            return describeBuilder.GetQueryBuilder().BuildQuery();
        }

        /// <summary>
        /// Adds a group graph pattern or a sub query to the query.
        /// </summary>
        /// <param name="buildGraphPattern"></param>
        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Child(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Child(this IDescribeBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Child(buildGraphPattern);
        }

        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, params ITriplePattern[] triplePatterns)
        {
            queryBuilder.RootGraphPatternBuilder.Where(triplePatterns);
            return queryBuilder;
        }

        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            queryBuilder.RootGraphPatternBuilder.Where(buildTriplePatterns);
            return queryBuilder;
        }

        internal static IQueryBuilder Where(this IQueryBuilder queryBuilder, Func<INamespaceMapper, ITriplePattern[]> buildTriplePatternFunc)
        {
            queryBuilder.RootGraphPatternBuilder.Where(buildTriplePatternFunc);
            return queryBuilder;
        }

        public static IQueryBuilder Optional(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Optional(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, ISparqlExpression expr)
        {
            queryBuilder.RootGraphPatternBuilder.Filter(expr);
            return queryBuilder;
        }

        public static IQueryBuilder Minus(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Minus(buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Graph(this IQueryBuilder queryBuilder, Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Graph(graphUri, buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Graph(this IQueryBuilder queryBuilder, string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Graph(graphVariable, buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Service(this IQueryBuilder queryBuilder, Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Service(serviceUri, buildGraphPattern);
            return queryBuilder;
        }

        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, Func<INonAggregateExpressionBuilder, BooleanExpression> buildExpression)
        {
            queryBuilder.RootGraphPatternBuilder.Filter(buildExpression);
            return queryBuilder;
        }

        public static IQueryBuilder Union(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> firstGraphPattern, params Action<IGraphPatternBuilder>[] otherGraphPatterns)
        {
            queryBuilder.RootGraphPatternBuilder.Union(firstGraphPattern, otherGraphPatterns);
            return queryBuilder;
        }
    }
}