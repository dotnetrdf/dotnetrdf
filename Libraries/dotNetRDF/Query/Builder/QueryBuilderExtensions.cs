/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public static class QueryBuilderExtensions
    {
        public static IAssignmentVariableNamePart<IQueryBuilder> Bind(this IQueryBuilder describeBuilder, Func<INonAggregateExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return describeBuilder.Bind(buildAssignmentExpression);
        }

        /// <summary>
        /// Add a group graph pattern or a sub query to the query.
        /// </summary>
        /// <param name="childBuilder"></param>
        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, IQueryBuilder childBuilder)
        {
            if ((childBuilder.QueryType & SparqlQueryType.Select) == SparqlQueryType.Select)
            {
                queryBuilder.RootGraphPatternBuilder.Where(new SubQueryPattern(childBuilder.BuildQuery()));
                return queryBuilder;
            }
            else
            {
                throw new ArgumentException("Invalid query type: " + childBuilder.QueryType + "; only Select queries may be used as sub-queries.");
            }
        }

        /// <summary>
        /// Add a group graph pattern or a sub query to the query.
        /// </summary>
        /// <param name="buildGraphPattern"></param>
        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.RootGraphPatternBuilder.Child(buildGraphPattern);
            return queryBuilder;
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

        public static IQueryBuilder Union(this IQueryBuilder queryBuilder, GraphPatternBuilder firstGraphPattern, params GraphPatternBuilder[] otherGraphPatterns)
        {
            queryBuilder.RootGraphPatternBuilder.Union(firstGraphPattern, otherGraphPatterns);
            return queryBuilder;
        }
    }
}