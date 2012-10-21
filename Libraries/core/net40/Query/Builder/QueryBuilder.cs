using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SparqlQuery _query = new SparqlQuery();

        private QueryBuilder(SparqlQuery query)
        {
            this._query = query;
            this.NamespaceMapper = new NamespaceMapper();
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

        private void Where(ITriplePattern tp)
        {
            if (_query == null) throw new ArgumentNullException("Null query");
            if (_query.RootGraphPattern == null) _query.RootGraphPattern = new GraphPattern();
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                case TriplePatternType.Path:
                case TriplePatternType.PropertyFunction:
                case TriplePatternType.SubQuery:
                    _query.RootGraphPattern.AddTriplePattern(tp);
                    break;
                case TriplePatternType.LetAssignment:
                case TriplePatternType.BindAssignment:
                    _query.RootGraphPattern.AddAssignment((IAssignmentPattern)tp);
                    break;
                case TriplePatternType.Filter:
                    _query.RootGraphPattern.AddFilter(((IFilterPattern)tp).Filter);
                    break;
            }
            return;
        }

        [Obsolete("Consider either leaving it here, adding a relevant method to triple pattern builder")]
        public IQueryBuilder Where(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                Where(tpb => tpb.Subject(t.Subject).Predicate(t.Predicate).Object(t.Object));
            }
            return this;
        }

        public IQueryBuilder Where(params ITriplePattern[] triplePatterns)
        {
            foreach (ITriplePattern tp in triplePatterns)
            {
                Where(tp);
            }
            return this;
        }

        public IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            var builder = new TriplePatternBuilder(NamespaceMapper);
            buildTriplePatterns(builder);
            return Where(builder.Patterns);
        }

        [Obsolete("Either make private completely or replace with an Action<IGraphPatternBuilder>")]
        public IQueryBuilder Where(GraphPattern gp)
        {
            if (_query == null) throw new ArgumentNullException("Null query");
            if (_query.RootGraphPattern == null)
            {
                _query.RootGraphPattern = gp;
            }
            else
            {
                _query.RootGraphPattern.AddGraphPattern(gp);
            }
            return this;
        }

        public IQueryBuilder Optional(params ITriplePattern[] triplePatterns)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsOptional = true;

            foreach (var tp in triplePatterns)
            {
                switch (tp.PatternType)
                {
                    case TriplePatternType.Match:
                    case TriplePatternType.Path:
                    case TriplePatternType.PropertyFunction:
                    case TriplePatternType.SubQuery:
                        gp.AddTriplePattern(tp);
                        break;
                    case TriplePatternType.BindAssignment:
                    case TriplePatternType.LetAssignment:
                        gp.AddAssignment((IAssignmentPattern)tp);
                        break;
                    case TriplePatternType.Filter:
                        gp.AddFilter(((IFilterPattern)tp).Filter);
                        break;
                }
            }
            return Where(gp);
        }

        public IQueryBuilder Optional(Action<ITriplePatternBuilder> buildTriplePatterns)
        {
            var builder = new TriplePatternBuilder(NamespaceMapper);
            buildTriplePatterns(builder);
            return Optional(builder.Patterns);
        }

        public IQueryBuilder Filter(ISparqlExpression expr)
        {
            if (_query == null) throw new ArgumentNullException("Null query");
            if (_query.RootGraphPattern == null) _query.RootGraphPattern = new GraphPattern();
            _query.RootGraphPattern.AddFilter(new UnaryExpressionFilter(expr));
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
            executableQuery.Optimise();
            return executableQuery;
        }

        /// <summary>
        /// Turns a Node into a Pattern item for use in a Triple Pattern
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        private static PatternItem ToPatternItem(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return new BlankNodePattern(((IBlankNode)n).InternalID);
                case NodeType.GraphLiteral:
                    throw new NotSupportedException("Graph Literals are not usable in SPARQL queries");
                case NodeType.Literal:
                case NodeType.Uri:
                    return new NodeMatchPattern(n);
                case NodeType.Variable:
                    return new VariablePattern(((IVariableNode)n).VariableName);
                default:
                    throw new NotSupportedException("Unknown Node types are not usable in SPARQL queries");
            }
        }

        /// <summary>
        /// Turns a Node into a SPARQL Expression Term
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        private static ISparqlExpression ToSparqlExpression(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    throw new NotSupportedException("Temporary variables are not permitted in SPARQL Expression");
                case NodeType.GraphLiteral:
                    throw new NotSupportedException("Graph Literals are not usable in SPARQL queries");
                case NodeType.Literal:
                case NodeType.Uri:
                    return new ConstantTerm(n);
                case NodeType.Variable:
                    return new VariableTerm(((IVariableNode)n).VariableName);
                default:
                    throw new NotSupportedException("Unknown Node types are not usable in SPARQL queries");
            }
        }

        public INamespaceMapper NamespaceMapper { get; set; }
    }
}
