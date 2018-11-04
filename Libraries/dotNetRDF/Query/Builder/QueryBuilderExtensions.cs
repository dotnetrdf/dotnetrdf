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
    /// <summary>
    /// Provides methods to build root graph pattern directly from the query builder
    /// </summary>
    public static class QueryBuilderExtensions
    {
        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Where(VDS.RDF.Query.Patterns.ITriplePattern[])"/>
        /// </summary>
        public static IQueryBuilder Where(this IQueryWithVariablesBuilder describeBuilder, params ITriplePattern[] triplePatterns)
        {
            return describeBuilder.GetQueryBuilder().Where(triplePatterns);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Where(Action{ITriplePatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Where(this IQueryWithVariablesBuilder describeBuilder, Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            return describeBuilder.GetQueryBuilder().Where(buildTriplePatterns);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Optional"/>
        /// </summary>
        public static IQueryBuilder Optional(this IQueryWithVariablesBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Optional(buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Filter(Func{INonAggregateExpressionBuilder, BooleanExpression})"/>
        /// </summary>
        public static IQueryBuilder Filter(this IQueryWithVariablesBuilder describeBuilder, Func<INonAggregateExpressionBuilder, BooleanExpression> expr)
        {
            return describeBuilder.GetQueryBuilder().Filter(expr);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Filter(ISparqlExpression)"/>
        /// </summary>
        public static IQueryBuilder Filter(this IQueryWithVariablesBuilder describeBuilder, ISparqlExpression expr)
        {
            return describeBuilder.GetQueryBuilder().Filter(expr);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Minus"/>
        /// </summary>
        public static IQueryBuilder Minus(this IQueryWithVariablesBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Minus(buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Graph(System.Uri,System.Action{VDS.RDF.Query.Builder.IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Graph(this IQueryWithVariablesBuilder describeBuilder, Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Graph(graphUri, buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Graph(System.String,System.Action{VDS.RDF.Query.Builder.IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Graph(this IQueryWithVariablesBuilder describeBuilder, string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Graph(graphVariable, buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Service"/>
        /// </summary>
        public static IQueryBuilder Service(this IQueryWithVariablesBuilder describeBuilder, Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Service(serviceUri, buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Bind"/>
        /// </summary>
        public static IAssignmentVariableNamePart<IQueryBuilder> Bind(this IDescribeBuilder describeBuilder, Func<INonAggregateExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return describeBuilder.GetQueryBuilder().Bind(buildAssignmentExpression);
        }

        /// <summary>
        /// Build a simple DESCRIBE query without the WHERE part.
        /// </summary>
        public static SparqlQuery BuildQuery(this IDescribeBuilder describeBuilder)
        {
            return describeBuilder.GetQueryBuilder().BuildQuery();
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Child(IQueryBuilder)"/>
        /// </summary>
        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, IQueryBuilder childBuilder)
        {
            if ((childBuilder.QueryType & SparqlQueryType.Select) == SparqlQueryType.Select)
            {
                queryBuilder.Root.Where(new SubQueryPattern(childBuilder.BuildQuery()));
                return queryBuilder;
            }
            else
            {
                throw new ArgumentException("Invalid query type: " + childBuilder.QueryType + "; only Select queries may be used as sub-queries.");
            }
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Child(Action{IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Child(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Child(buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Child(Action{IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Child(this IDescribeBuilder describeBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            return describeBuilder.GetQueryBuilder().Child(buildGraphPattern);
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Where(VDS.RDF.Query.Patterns.ITriplePattern[])"/>
        /// </summary>
        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, params ITriplePattern[] triplePatterns)
        {
            queryBuilder.Root.Where(triplePatterns);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Where(Action{ITriplePatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            queryBuilder.Root.Where(buildTriplePatterns);
            return queryBuilder;
        }

        internal static IQueryBuilder Where(this IQueryBuilder queryBuilder, Func<INamespaceMapper, ITriplePattern[]> buildTriplePatternFunc)
        {
            queryBuilder.RootGraphPatternBuilder.Where(buildTriplePatternFunc);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Optional"/>
        /// </summary>
        public static IQueryBuilder Optional(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Optional(buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Filter(ISparqlExpression)"/>
        /// </summary>
        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, ISparqlExpression expr)
        {
            queryBuilder.Root.Filter(expr);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Minus"/>
        /// </summary>
        public static IQueryBuilder Minus(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Minus(buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Graph(Uri, Action{IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Graph(this IQueryBuilder queryBuilder, Uri graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Graph(graphUri, buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Graph(String, Action{IGraphPatternBuilder})"/>
        /// </summary>
        public static IQueryBuilder Graph(this IQueryBuilder queryBuilder, string graphVariable, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Graph(graphVariable, buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Service"/>
        /// </summary>
        public static IQueryBuilder Service(this IQueryBuilder queryBuilder, Uri serviceUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            queryBuilder.Root.Service(serviceUri, buildGraphPattern);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Filter(Func{INonAggregateExpressionBuilder, BooleanExpression})"/>
        /// </summary>
        public static IQueryBuilder Filter(this IQueryBuilder queryBuilder, Func<INonAggregateExpressionBuilder, BooleanExpression> buildExpression)
        {
            queryBuilder.Root.Filter(buildExpression);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Union(Action{IGraphPatternBuilder},IGraphPatternBuilder[])"/>
        /// </summary>
        public static IQueryBuilder Union(this IQueryBuilder queryBuilder, Action<IGraphPatternBuilder> firstGraphPattern, params Action<IGraphPatternBuilder>[] otherGraphPatterns)
        {
            queryBuilder.Root.Union(firstGraphPattern, otherGraphPatterns);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.Union(GraphPatternBuilder,VDS.RDF.Query.Builder.GraphPatternBuilder[])"/>
        /// </summary>
        public static IQueryBuilder Union(this IQueryBuilder queryBuilder, GraphPatternBuilder firstGraphPattern, params GraphPatternBuilder[] otherGraphPatterns)
        {
            queryBuilder.Root.Union(firstGraphPattern, otherGraphPatterns);
            return queryBuilder;
        }

        /// <summary>
        /// See <see cref="IGraphPatternBuilder.InlineData"/>
        /// </summary>
        public static IInlineDataBuilder InlineData(this IQueryBuilder queryBuilder, params string[] variables)
        {
            return queryBuilder.Root.InlineData(variables);
        }
    }
}