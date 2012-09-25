using System;
using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Provides static and extension methods for building queries with a fluent style API
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SparqlQuery"/> is mutable by definition so calling any of the extension methods in this API will cause the existing query it is called on to be changed.  You can call <see cref="SparqlQuery.Copy()"/> on an existing query to create a new copy if you want to make different queries starting from the same base query
    /// </para>
    /// </remarks>
    public class QueryBuilder : IQueryBuilder
    {
        private readonly SparqlQuery _query = new SparqlQuery();

        private QueryBuilder(SparqlQuery query)
        {
            this._query = query;
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

        /// <summary>
        /// Adds a Triple pattern to the query with the three variable names
        /// </summary>
        /// <param name="q"></param>
        /// <param name="s"></param>
        /// <param name="p"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <remarks>
        /// To mix variables and RDF terms (URIs and Literals) you should use the overload that takes <see cref="INode"/> arguments instead
        /// </remarks>
        public IQueryBuilder Where(String s, String p, String o)
        {
            return Where(_query.CreateVariableNode(s), _query.CreateVariableNode(p), _query.CreateVariableNode(o));
        }

        /// <summary>
        /// Adds a Triple Pattern to the query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="s">Subject to match</param>
        /// <param name="p">Predicate to match</param>
        /// <param name="o">Object to match</param>
        /// <returns></returns>
        public IQueryBuilder Where(INode s, INode p, INode o)
        {
            return Where(new TriplePattern(ToPatternItem(s), ToPatternItem(p), ToPatternItem(o)));
        }

        public IQueryBuilder Where(ITriplePattern tp)
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
            return this;
        }

        public IQueryBuilder Where(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                Where(t.Subject, t.Predicate, t.Object);
            }
            return this;
        }

        public IQueryBuilder Where(IEnumerable<ITriplePattern> tps)
        {
            foreach (ITriplePattern tp in tps)
            {
                Where(tp);
            }
            return this;
        }

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

        public IQueryBuilder Optional(INode s, INode p, INode o)
        {
            return Optional(new TriplePattern(ToPatternItem(s), ToPatternItem(p), ToPatternItem(p)));
        }

        public IQueryBuilder Optional(ITriplePattern tp)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsOptional = true;
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
            return Where(gp);
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
            // QueryBuilder or the retrieved SparqlQuery(s) from
            // being reflected in one another
            return _query.Copy();
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
    }
}
