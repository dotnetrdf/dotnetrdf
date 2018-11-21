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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building queries with a fluent style API
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SparqlQuery"/> is mutable by definition so calling any of the extension methods in this API will cause the existing query it is called on to be changed.  You can call <see cref="SparqlQuery.Copy()"/> on an existing query to create a new copy if you want to make different queries starting from the same base query
    /// </para>
    /// </remarks>
    public class QueryBuilder : IQueryBuilder
    {
        private readonly GraphPatternBuilder _rootGraphPatternBuilder = new GraphPatternBuilder();
        private readonly IList<Func<INamespaceMapper, ISparqlOrderBy>> _buildOrderings = new List<Func<INamespaceMapper, ISparqlOrderBy>>();
        private readonly IList<Func<INamespaceMapper, ISparqlGroupBy>> _buildGroups = new List<Func<INamespaceMapper, ISparqlGroupBy>>();
        private readonly IList<Func<INamespaceMapper, ISparqlExpression>> _buildHavings = new List<Func<INamespaceMapper, ISparqlExpression>>();
        private SparqlQueryType _sparqlQueryType;
        private int _queryLimit = -1;
        private int _queryOffset;
        private INamespaceMapper _prefixes = new NamespaceMapper(true);
        private InlineDataBuilder _inlineDataOverQuery;

        /// <summary>
        /// Gets or sets the namespace mappings for the SPARQL query being built
        /// </summary>
        public INamespaceMapper Prefixes
        {
            get { return _prefixes; }
            set { _prefixes = value; }
        }

        /// <inheritdoc/>
        public GraphPatternBuilder RootGraphPatternBuilder
        {
            get { return _rootGraphPatternBuilder; }
        }

        /// <inheritdoc/>
        public IGraphPatternBuilder Root
        {
            get { return _rootGraphPatternBuilder; }
        }

        /// <inheritdoc/>
        public SparqlQueryType QueryType
        {
            get { return _sparqlQueryType; }
            protected set { _sparqlQueryType = value; }
        }

        protected internal QueryBuilder(SparqlQueryType sparqlQueryType)
        {
            _sparqlQueryType = sparqlQueryType;
        }

        /// <summary>
        /// Creates a new ASK query
        /// </summary>
        public static IQueryBuilder Ask()
        {
            return new QueryBuilder(SparqlQueryType.Ask);
        }

        /// <summary>
        /// Creates a new CONSTRUCT query
        /// </summary>
        public static IQueryBuilder Construct(Action<IDescribeGraphPatternBuilder> buildConstructTemplate)
        {
            if (buildConstructTemplate == null)
            {
                return new QueryBuilder(SparqlQueryType.Construct);
            }

            return new ConstructBuilder(buildConstructTemplate);
        }

        /// <summary>
        /// Creates a new CONSTRUCT WHERE query
        /// </summary>
        public static IQueryBuilder Construct()
        {
            return Construct(null);
        }

        /// <summary>
        /// Creates a new SELECT * query
        /// </summary>
        public static ISelectBuilder SelectAll()
        {
            return new SelectBuilder(SparqlQueryType.SelectAll);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static ISelectBuilder Select(params SparqlVariable[] variables)
        {
            return new SelectBuilder(SparqlQueryType.Select).And(variables);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static ISelectBuilder Select(params string[] variables)
        {
            SparqlVariable[] sparqlVariables = variables.Select(var => new SparqlVariable(var, true)).ToArray();
            return Select(sparqlVariables);
        }

        /// <summary>
        /// Creates a new SELECT query which will return an expression
        /// </summary>
        public static IAssignmentVariableNamePart<ISelectBuilder> Select<TExpression>(Func<IExpressionBuilder, PrimaryExpression<TExpression>> buildAssignmentExpression)
        {
            SelectBuilder selectBuilder = (SelectBuilder)Select(new SparqlVariable[0]);
            return new SelectAssignmentVariableNamePart<TExpression>(selectBuilder, buildAssignmentExpression);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="uris"/>
        /// </summary>
        public static IDescribeBuilder Describe(params Uri[] uris)
        {
            return new DescribeBuilder(SparqlQueryType.Describe).And(uris);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="variables"/>
        /// </summary>
        public static IDescribeBuilder Describe(params string[] variables)
        {
            return new DescribeBuilder(SparqlQueryType.Describe).And(variables);
        }

        /// <summary>
        /// Applies a LIMIT
        /// </summary>
        /// <param name="limit">Limit value. Pass negative to disable LIMIT</param>
        public IQueryBuilder Limit(int limit)
        {
            _queryLimit = limit;
            return this;
        }

        /// <summary>
        /// Applies an OFFSET
        /// </summary>
        public IQueryBuilder Offset(int offset)
        {
            _queryOffset = offset;
            return this;
        }

        /// <summary>
        /// Adds ascending ordering by a variable to the query
        /// </summary>
        public IQueryBuilder OrderBy(SparqlVariable variable)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variable.Name));
            return this;
        }

        /// <summary>
        /// Adds ascending ordering by a variable to the query
        /// </summary>
        public IQueryBuilder OrderBy(string variableName)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variableName));
            return this;
        }

        /// <summary>
        /// Adds descending ordering by a variable to the query
        /// </summary>
        public IQueryBuilder OrderByDescending(SparqlVariable variable)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variable.Name) { Descending = true });
            return this;
        }

        /// <summary>
        /// Adds descending ordering by a variable to the query
        /// </summary>
        public IQueryBuilder OrderByDescending(string variableName)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variableName) { Descending = true });
            return this;
        }

        /// <summary>
        /// Adds ascending ordering by an expression to the query
        /// </summary>
        public IQueryBuilder OrderBy(Func<IExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, false);
            return this;
        }

        /// <summary>
        /// Adds descending ordering by an expression to the query
        /// </summary>
        public IQueryBuilder OrderByDescending(Func<IExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, true);
            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder GroupBy(SparqlVariable variable)
        {
            _buildGroups.Add(prefixes => new GroupByVariable(variable.Name));
            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder GroupBy(string variableName)
        {
            _buildGroups.Add(prefixes => new GroupByVariable(variableName));
            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder GroupBy(Func<INonAggregateExpressionBuilder, SparqlExpression> buildGroupingExpression)
        {
            _buildGroups.Add(prefixes =>
            {
                var expressionBuilder = new ExpressionBuilder(prefixes);
                var sparqlExpression = buildGroupingExpression(expressionBuilder).Expression;
                return new GroupByExpression(sparqlExpression);
            });
            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder Having(Func<IExpressionBuilder, BooleanExpression> buildHavingConstraint)
        {
            _buildHavings.Add(prefixes => buildHavingConstraint(new ExpressionBuilder(prefixes)).Expression);
            return this;
        }

        private void AppendOrdering(Func<ExpressionBuilder, SparqlExpression> orderExpression, bool descending)
        {
            _buildOrderings.Add(prefixes =>
            {
                var expressionBuilder = new ExpressionBuilder(prefixes);
                var sparqlExpression = orderExpression.Invoke(expressionBuilder).Expression;
                var orderBy = new OrderByExpression(sparqlExpression) { Descending = descending };
                return orderBy;
            });
        }

        /// <inheritdoc />
        public SparqlQuery BuildQuery()
        {
            var query = new SparqlQuery
            {
                QueryType = _sparqlQueryType,
                Limit = _queryLimit,
                Offset = _queryOffset,
            };

            return BuildQuery(query);
        }

        protected virtual SparqlQuery  BuildQuery(SparqlQuery query)
        {
            BuildRootGraphPattern(query);
            BuildGroupByClauses(query);
            BuildHavingClauses(query);
            BuildAndChainOrderings(query);
            _inlineDataOverQuery?.AppendTo(query);

            query.NamespaceMap.Import(Prefixes);

            return query;
        }

        private void BuildRootGraphPattern(SparqlQuery query)
        {
            var rootGraphPattern = RootGraphPatternBuilder.BuildGraphPattern(Prefixes);

            query.RootGraphPattern = rootGraphPattern;
        }

        private void BuildGroupByClauses(SparqlQuery query)
        {
            ISparqlGroupBy rootGroup = null;
            ISparqlGroupBy lastGroup = null;

            foreach (var buildGroup in _buildGroups)
            {
                if (rootGroup == null)
                {
                    rootGroup = lastGroup = buildGroup(Prefixes);
                }
                else
                {
                    lastGroup.Child = buildGroup(Prefixes);
                }
            }

            query.GroupBy = rootGroup;
        }

        private void BuildHavingClauses(SparqlQuery query)
        {
            var filters = (from builder in _buildHavings
                select new UnaryExpressionFilter(builder(Prefixes))).ToList();

            if (filters.Any())
            {
                query.Having = new ChainFilter(filters);
            }
        }

        private void BuildAndChainOrderings(SparqlQuery executableQuery)
        {
            IList<ISparqlOrderBy> orderings = (from orderByBuilder in _buildOrderings select orderByBuilder(Prefixes)).ToList();

            for (int i = 1; i < orderings.Count; i++)
            {
                orderings[i - 1].Child = orderings[i];
            }

            executableQuery.OrderBy = orderings.FirstOrDefault();
        }

        /// <inheritdoc />
        public IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<INonAggregateExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new BindAssignmentVariableNamePart(this, buildAssignmentExpression);
        }

        /// <inheritdoc/>
        public IInlineDataBuilder InlineDataOverQuery(params string[] variables)
        {
            _inlineDataOverQuery = new InlineDataBuilder(variables);
            return _inlineDataOverQuery;
        }
    }
}
