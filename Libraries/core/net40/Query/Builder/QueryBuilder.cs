using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides static and extension methods for building queries with a fluent style API
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SparqlQuery"/> is mutable by definition so calling any of the extension methods in this API will cause the existing query it is called on to be changed.  You can call <see cref="SparqlQuery.Copy()"/> on an existing query to create a new copy if you want to make different queries starting from the same base query
    /// </para>
    /// </remarks>
    public sealed class QueryBuilder : IQueryBuilder
    {
        private readonly SparqlQuery _query;
        private readonly GraphPatternBuilder _rootGraphPatternBuilder;

        public INamespaceMapper Prefixes { get; set; }

        private QueryBuilder(SparqlQuery query)
        {
            this._query = query;
            this.Prefixes = new NamespaceMapper();
            _rootGraphPatternBuilder = new GraphPatternBuilder(Prefixes);
        }

        /// <summary>
        /// Creates a new SELECT * query
        /// </summary>
        /// <returns></returns>
        public static IQueryBuilder SelectAll()
        {
            SparqlQuery q = new SparqlQuery();
            q.QueryType = SparqlQueryType.SelectAll;
            return new QueryBuilder(q);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static IQueryBuilder Select(params SparqlVariable[] variables)
        {
            SparqlQuery q = new SparqlQuery();
            q.QueryType = SparqlQueryType.Select;
            foreach (var sparqlVariable in variables)
            {
                q.AddVariable(sparqlVariable);
            }
            return new QueryBuilder(q);
        }

        /// <summary>
        /// Creates a new SELECT query which will return the given 
        /// <paramref name="variables"/>
        /// </summary>
        /// <param name="variables">query result variables</param>
        public static IQueryBuilder Select(params string[] variables)
        {
            SparqlVariable[] sparqlVariables = variables.Select(var => new SparqlVariable(var, true)).ToArray();
            return Select(sparqlVariables);
        }

        public static IQueryBuilder Describe(Uri uri)
        {
            SparqlQuery q = new SparqlQuery();
            q.QueryType = SparqlQueryType.Describe;
            q.AddDescribeVariable(new UriToken(string.Format("<{0}>", uri),0,0,0));
            return new QueryBuilder(q);
        }

        #region Implementation of IQueryBuilder

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public IQueryBuilder Distinct()
        {
            if (_query == null) throw new ArgumentNullException("Null query");
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
            if (_query == null) throw new ArgumentNullException("Null query");
            _query.Limit = limit;
            return this;
        }

        public IQueryBuilder Offset(int offset)
        {
            if (_query == null) throw new ArgumentNullException("Null query");
            _query.Offset = offset;
            return this;
        }

        public IQueryBuilder Slice(int limit, int offset)
        {
            return Limit(limit).Offset(offset);
        }

        public SparqlQuery GetExecutableQuery()
        {
            // returns a copy to prevent changes in either
            // QueryBuilder or the retrieved SparqlQuery(variableName) from
            // being reflected in one another
            SparqlQuery executableQuery = _query.Copy();
            executableQuery.NamespaceMap.Import(Prefixes);
            executableQuery.RootGraphPattern = _rootGraphPatternBuilder.BuildGraphPattern();
            return executableQuery;
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

        public IQueryBuilder Optional(params ITriplePattern[] triplePatterns)
        {
            _rootGraphPatternBuilder.Optional(triplePatterns);
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

        #endregion
    }
}
