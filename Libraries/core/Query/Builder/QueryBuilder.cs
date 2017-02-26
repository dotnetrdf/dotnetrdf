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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Patterns;

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
    public sealed class QueryBuilder : IQueryBuilder
    {
        private readonly DescribeBuilder _describeBuilder;
        private readonly GraphPatternBuilder _rootGraphPatternBuilder = new GraphPatternBuilder();
        private DescribeGraphPatternBuilder _constructGraphPatternBuilder;
        private readonly SelectBuilder _selectBuilder;
        private readonly IList<Func<INamespaceMapper, ISparqlOrderBy>> _buildOrderings = new List<Func<INamespaceMapper, ISparqlOrderBy>>();
        private readonly SparqlQueryType _sparqlQueryType;
        private int _queryLimit = -1;
        private int _queryOffset;
        private INamespaceMapper _prefixes = new NamespaceMapper(true);

        /// <summary>
        /// Gets or sets the namespace mappings for the SPARQL query being built
        /// </summary>
        public INamespaceMapper Prefixes
        {
            get { return _prefixes; }
            set { _prefixes = value; }
        }

        internal GraphPatternBuilder RootGraphPatternBuilder
        {
            get { return _rootGraphPatternBuilder; }
        }

        internal QueryBuilder(SelectBuilder selectBuilder)
            : this(selectBuilder.SparqlQueryType)
        {
            _selectBuilder = selectBuilder;
        }

        internal QueryBuilder(DescribeBuilder describeBuilder)
            : this(describeBuilder.SparqlQueryType)
        {
            _describeBuilder = describeBuilder;
        }

        private QueryBuilder(SparqlQueryType sparqlQueryType)
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
                return Construct();
            }

            var queryBuilder = new QueryBuilder(SparqlQueryType.Construct);
            DescribeGraphPatternBuilder graphPatternBuilder = new DescribeGraphPatternBuilder(new GraphPatternBuilder());
            buildConstructTemplate(graphPatternBuilder);
            return queryBuilder.Construct(graphPatternBuilder);
        }

        /// <summary>
        /// Creates a new CONSTRUCT WHERE query
        /// </summary>
        public static IQueryBuilder Construct()
        {
            return new QueryBuilder(SparqlQueryType.Construct);
        }

        private IQueryBuilder Construct(DescribeGraphPatternBuilder graphPatternBuilder)
        {
            _constructGraphPatternBuilder = graphPatternBuilder;
            return this;
        }

        /// <summary>
        /// Creates a new SELECT * query
        /// </summary>
        public static IQueryBuilder SelectAll()
        {
            return new QueryBuilder(new SelectBuilder(SparqlQueryType.SelectAll));
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
        public static IAssignmentVariableNamePart<ISelectBuilder> Select(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            SelectBuilder selectBuilder = (SelectBuilder)Select(new SparqlVariable[0]);
            return new SelectAssignmentVariableNamePart(selectBuilder, buildAssignmentExpression);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="uris"/>
        /// </summary>
        public static IDescribeBuilder Describe(params Uri[] uris)
        {
            return new DescribeBuilder().And(uris);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="variables"/>
        /// </summary>
        public static IDescribeBuilder Describe(params string[] variables)
        {
            return new DescribeBuilder().And(variables);
        }

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        public IQueryBuilder Distinct()
        {
            if (_selectBuilder != null)
            {
                _selectBuilder.Distinct();
            }
            return this;
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
        public IQueryBuilder OrderBy(string variableName)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variableName));
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
        public IQueryBuilder OrderBy(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, false);
            return this;
        }

        /// <summary>
        /// Adds descending ordering by an expression to the query
        /// </summary>
        public IQueryBuilder OrderByDescending(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, true);
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

        public SparqlQuery BuildQuery()
        {
            SparqlQuery query = new SparqlQuery
                {
                    QueryType = _sparqlQueryType,
                    Limit = _queryLimit,
                    Offset = _queryOffset
                };

            switch (_sparqlQueryType)
            {
                case SparqlQueryType.Construct:
                    BuildConstructGraphPattern(query);
                    break;
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    BuildDecribeVariables(query);
                    break;
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    BuildReturnVariables(query);
                    break;
            }

            BuildRootGraphPattern(query);
            BuildAndChainOrderings(query);

            query.NamespaceMap.Import(Prefixes);

            return query;
        }

        private void BuildDecribeVariables(SparqlQuery query)
        {
            if (_describeBuilder == null) return;

            query.QueryType = _describeBuilder.SparqlQueryType;
            foreach (var describeVariable in _describeBuilder.DescribeVariables)
            {
                query.AddDescribeVariable(describeVariable);
            }
        }

        private void BuildConstructGraphPattern(SparqlQuery query)
        {
            if (_constructGraphPatternBuilder == null) return;

            query.ConstructTemplate = _constructGraphPatternBuilder.BuildGraphPattern(Prefixes);
        }

        private void BuildRootGraphPattern(SparqlQuery query)
        {
            var rootGraphPattern = RootGraphPatternBuilder.BuildGraphPattern(Prefixes);
            if (!rootGraphPattern.IsEmpty)
            {
                query.RootGraphPattern = rootGraphPattern;
            }
        }

        private void BuildReturnVariables(SparqlQuery query)
        {
            if (_selectBuilder == null) return;

            query.QueryType = _selectBuilder.SparqlQueryType;
            foreach (SparqlVariable selectVariable in _selectBuilder.BuildVariables(Prefixes))
            {
                query.AddVariable(selectVariable);
            }
        }

        private void BuildAndChainOrderings(SparqlQuery executableQuery)
        {
            IList<ISparqlOrderBy> orderings = (from orderByBuilder in _buildOrderings
                                               select orderByBuilder(Prefixes)).ToList();
            for (int i = 1; i < orderings.Count; i++)
            {
                orderings[i - 1].Child = orderings[i];
            }
            executableQuery.OrderBy = orderings.FirstOrDefault();
        }

        public IAssignmentVariableNamePart<IQueryBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new BindAssignmentVariableNamePart(this, buildAssignmentExpression);
        }
    }
}
