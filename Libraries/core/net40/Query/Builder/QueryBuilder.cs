using System;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
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
    public sealed class QueryBuilder : IQueryBuilder, IDescribeQueryBuilder, ISelectQueryBuilder
    {
        private readonly SparqlQuery _query;
        private readonly GraphPatternBuilder _rootGraphPatternBuilder;
        private DescribeGraphPatternBuilder _constructGraphPatternBuilder;

        /// <summary>
        /// Gets or sets the namespace mappings for the SPARQL query being built
        /// </summary>
        public INamespaceMapper Prefixes { get; set; }

        private QueryBuilder(SparqlQueryType queryType)
        {
            this._query = new SparqlQuery { QueryType = queryType };
            this.Prefixes = new NamespaceMapper(true);
            _rootGraphPatternBuilder = new GraphPatternBuilder(Prefixes);
        }

        public static IQueryBuilder Ask()
        {
            return new QueryBuilder(SparqlQueryType.Ask);
        }

        public static IQueryBuilder Construct(Action<IDescribeGraphPatternBuilder> buildConstructTemplate)
        {
            if (buildConstructTemplate == null)
            {
                return Construct();
            }

            var queryBuilder = new QueryBuilder(SparqlQueryType.Construct);
            DescribeGraphPatternBuilder graphPatternBuilder = new DescribeGraphPatternBuilder(new GraphPatternBuilder(queryBuilder.Prefixes));
            buildConstructTemplate(graphPatternBuilder);
            return queryBuilder.Construct(graphPatternBuilder);
        }

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
        public static ISelectQueryBuilder SelectAll()
        {
            return new QueryBuilder(SparqlQueryType.SelectAll);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static ISelectQueryBuilder Select(params SparqlVariable[] variables)
        {
            return new QueryBuilder(SparqlQueryType.Select).And(variables);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static ISelectQueryBuilder Select(params string[] variables)
        {
            SparqlVariable[] sparqlVariables = variables.Select(var => new SparqlVariable(var, true)).ToArray();
            return Select(sparqlVariables);
        }

        /// <summary>
        /// Creates a new SELECT query which will return an expression
        /// </summary>
        public static AssignmentVariableNamePart<ISelectQueryBuilder> Select(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            var queryBuilder = Select(new SparqlVariable[0]);
            return new AssignmentVariableNamePart<ISelectQueryBuilder>(queryBuilder, buildAssignmentExpression);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="uris"/>
        /// </summary>
        public static IDescribeQueryBuilder Describe(params Uri[] uris)
        {
            return new QueryBuilder(SparqlQueryType.Describe).And(uris);
        }

        /// <summary>
        /// Creates a new query, which will DESCRIBE the given <paramref name="variables"/>
        /// </summary>
        public static IDescribeQueryBuilder Describe(params string[] variables)
        {
            return new QueryBuilder(SparqlQueryType.Describe).And(variables);
        }

        #region Implementation of IQueryBuilder

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        public IQueryBuilder Distinct()
        {
            if (_query.HasDistinctModifier) return this;
            switch (_query.QueryType)
            {
                case SparqlQueryType.Select:
                    _query.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAll:
                    _query.QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
                case SparqlQueryType.SelectReduced:
                    _query.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAllReduced:
                    _query.QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
            }
            return this;
        }

        public IQueryBuilder Limit(int limit)
        {
            _query.Limit = limit;
            return this;
        }

        public IQueryBuilder Offset(int offset)
        {
            _query.Offset = offset;
            return this;
        }

        public IQueryBuilder OrderBy(string variableName)
        {
            AppendOrdering(new OrderByVariable(variableName), false);
            return this;
        }

        public IQueryBuilder OrderByDescending(string variableName)
        {
            AppendOrdering(new OrderByVariable(variableName), true);
            return this;
        }

        public IQueryBuilder OrderBy(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, false);
            return this;
        }

        public IQueryBuilder OrderByDescending(Func<ExpressionBuilder, SparqlExpression> buildOrderExpression)
        {
            AppendOrdering(buildOrderExpression, true);
            return this;
        }

        private void AppendOrdering(Func<ExpressionBuilder, SparqlExpression> orderExpression, bool descending)
        {
            var expressionBuilder = new ExpressionBuilder(Prefixes);
            var sparqlExpression = orderExpression.Invoke(expressionBuilder).Expression;
            var orderBy = new OrderByExpression(sparqlExpression);
            AppendOrdering(orderBy, descending);
        }

        private void AppendOrdering(ISparqlOrderBy orderBy, bool descending)
        {
            orderBy.Descending = descending;

            if (_query.OrderBy == null)
            {
                _query.OrderBy = orderBy;
            }
            else
            {
                ISparqlOrderBy lastOrderBy = _query.OrderBy;
                while (lastOrderBy.Child != null)
                {
                    lastOrderBy = lastOrderBy.Child;
                }
                lastOrderBy.Child = orderBy;
            }
        }

        public SparqlQuery BuildQuery()
        {
            // returns a copy to prevent changes in either
            // QueryBuilder or the retrieved SparqlQuery(variableName) from
            // being reflected in one another
            SparqlQuery executableQuery = _query.Copy();
            executableQuery.NamespaceMap.Import(Prefixes);
            var rootGraphPattern = _rootGraphPatternBuilder.BuildGraphPattern();
            if (!rootGraphPattern.IsEmpty)
            {
                executableQuery.RootGraphPattern = rootGraphPattern;
            }
            if (_constructGraphPatternBuilder != null)
            {
                executableQuery.ConstructTemplate = _constructGraphPatternBuilder.BuildGraphPattern();
            }
            return executableQuery;
        }

        public AssignmentVariableNamePart<IQueryBuilder> Bind(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new AssignmentVariableNamePart<IQueryBuilder>(this, buildAssignmentExpression);
        }

        public IQueryBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            _rootGraphPatternBuilder.Child(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Where(params ITriplePattern[] triplePatterns)
        {
            _rootGraphPatternBuilder.Where(triplePatterns);
            return this;
        }

        public IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            _rootGraphPatternBuilder.Where(buildTriplePatterns);
            return this;
        }

        public IQueryBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            _rootGraphPatternBuilder.Optional(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Filter(ISparqlExpression expr)
        {
            _rootGraphPatternBuilder.Filter(expr);
            return this;
        }

        public IQueryBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            _rootGraphPatternBuilder.Minus(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Filter(Func<ExpressionBuilder, BooleanExpression> buildExpression)
        {
            _rootGraphPatternBuilder.Filter(buildExpression);
            return this;
        }

        #endregion

        #region Implementation of IDescribeQueryBuilder

        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        public IDescribeQueryBuilder And(params string[] variables)
        {
            foreach (var variableName in variables)
            {
                _query.AddDescribeVariable(new VariableToken(variableName, 0, 0, 0));
            }
            return this;
        }

        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        public IDescribeQueryBuilder And(params Uri[] uris)
        {
            foreach (var uri in uris)
            {
                _query.AddDescribeVariable(new UriToken(string.Format("<{0}>", uri), 0, 0, 0));
            }
            return this;
        }

        #endregion

        #region Implementation of ISelectQueryBuilder

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        public ISelectQueryBuilder And(params SparqlVariable[] variables)
        {
            foreach (var sparqlVariable in variables)
            {
                _query.AddVariable(sparqlVariable.IsResultVariable ? sparqlVariable : CopyVariable(sparqlVariable));
            }
            return this;
        }

        /// <summary>
        /// Adds additional SELECT expression
        /// </summary>
        public AssignmentVariableNamePart<ISelectQueryBuilder> And(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            return new AssignmentVariableNamePart<ISelectQueryBuilder>(this, buildAssignmentExpression);
        }

        private SparqlVariable CopyVariable(SparqlVariable sparqlVariable)
        {
            if (sparqlVariable.IsAggregate)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Aggregate);
            }

            if (sparqlVariable.IsProjection)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Projection);
            }

            return new SparqlVariable(sparqlVariable.Name, true);
        }

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        ISelectQueryBuilder ISelectQueryBuilder.And(params string[] variables)
        {
            return And(variables.Select(var => new SparqlVariable(var, true)).ToArray());
        }

        #endregion
    }
}
