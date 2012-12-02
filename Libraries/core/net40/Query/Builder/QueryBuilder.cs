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
            DescribeGraphPatternBuilder graphPatternBuilder = new DescribeGraphPatternBuilder(new GraphPatternBuilder());
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

        public IQueryBuilder Limit(int limit)
        {
            _queryLimit = limit;
            return this;
        }

        public IQueryBuilder Offset(int offset)
        {
            _queryOffset = offset;
            return this;
        }

        public IQueryBuilder OrderBy(string variableName)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variableName));
            return this;
        }

        public IQueryBuilder OrderByDescending(string variableName)
        {
            _buildOrderings.Add(prefixes => new OrderByVariable(variableName) { Descending = true });
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
            // returns a copy to prevent changes in either
            // QueryBuilder or the retrieved SparqlQuery(variableName) from
            // being reflected in one another
            SparqlQuery executableQuery = new SparqlQuery
                {
                    QueryType = _sparqlQueryType,
                    Limit = _queryLimit,
                    Offset = _queryOffset
                };
            executableQuery.NamespaceMap.Import(Prefixes);

            if (_selectBuilder != null)
            {
                executableQuery.QueryType = _selectBuilder.SparqlQueryType;
                foreach (SparqlVariable selectVariable in _selectBuilder.BuildVariables(Prefixes))
                {
                    executableQuery.AddVariable(selectVariable);
                }
            }
            else if (_describeBuilder != null)
            {
                executableQuery.QueryType = _describeBuilder.SparqlQueryType;
                foreach (var describeVariable in _describeBuilder.DescribeVariables)
                {
                    executableQuery.AddDescribeVariable(describeVariable);
                }
            }

            var rootGraphPattern = RootGraphPatternBuilder.BuildGraphPattern(Prefixes);
            if (!rootGraphPattern.IsEmpty)
            {
                executableQuery.RootGraphPattern = rootGraphPattern;
            }

            if (_constructGraphPatternBuilder != null)
            {
                executableQuery.ConstructTemplate = _constructGraphPatternBuilder.BuildGraphPattern(Prefixes);
            }

            BuildAndChainOrderings(executableQuery);

            return executableQuery;
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

        public IQueryBuilder Child(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            RootGraphPatternBuilder.Child(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Where(params ITriplePattern[] triplePatterns)
        {
            RootGraphPatternBuilder.Where(triplePatterns);
            return this;
        }

        public IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            RootGraphPatternBuilder.Where(buildTriplePatterns);
            return this;
        }

        internal IQueryBuilder Where(Func<INamespaceMapper, ITriplePattern[]> buildTriplePatternFunc)
        {
            RootGraphPatternBuilder.Where(buildTriplePatternFunc);
            return this;
        }

        public IQueryBuilder Optional(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            RootGraphPatternBuilder.Optional(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Filter(ISparqlExpression expr)
        {
            RootGraphPatternBuilder.Filter(expr);
            return this;
        }

        public IQueryBuilder Minus(Action<IGraphPatternBuilder> buildGraphPattern)
        {
            RootGraphPatternBuilder.Minus(buildGraphPattern);
            return this;
        }

        public IQueryBuilder Filter(Func<ExpressionBuilder, BooleanExpression> buildExpression)
        {
            RootGraphPatternBuilder.Filter(buildExpression);
            return this;
        }
    }
}
