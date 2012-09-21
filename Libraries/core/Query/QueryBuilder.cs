using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public static class QueryBuilder
    {
        /// <summary>
        /// Creates a new SELECT * query
        /// </summary>
        /// <returns></returns>
        public static SparqlQuery SelectAll()
        {
            SparqlQuery q = new SparqlQuery();
            q.QueryType = SparqlQueryType.SelectAll;
            return q;
        }

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public static SparqlQuery Distinct(this SparqlQuery q)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            if (q.HasDistinctModifier) return q;
            switch (q.QueryType)
            {
                case SparqlQueryType.Select:
                    q.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAll:
                    q.QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
                case SparqlQueryType.SelectReduced:
                    q.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAllReduced:
                    q.QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
            }
            return q;
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
        public static SparqlQuery Where(this SparqlQuery q, String s, String p, String o)
        {
            return q.Where(q.CreateVariableNode(s), q.CreateVariableNode(p), q.CreateVariableNode(o));
        }

        /// <summary>
        /// Adds a Triple Pattern to the query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="s">Subject to match</param>
        /// <param name="p">Predicate to match</param>
        /// <param name="o">Object to match</param>
        /// <returns></returns>
        public static SparqlQuery Where(this SparqlQuery q, INode s, INode p, INode o)
        {
            return q.Where(new TriplePattern(s.ToPatternItem(), p.ToPatternItem(), o.ToPatternItem()));
        }

        public static SparqlQuery Where(this SparqlQuery q, ITriplePattern tp)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            if (q.RootGraphPattern == null) q.RootGraphPattern = new GraphPattern();
            switch (tp.PatternType)
            {
                case TriplePatternType.Match:
                case TriplePatternType.Path:
                case TriplePatternType.PropertyFunction:
                case TriplePatternType.SubQuery:
                    q.RootGraphPattern.AddTriplePattern(tp);
                    break;
                case TriplePatternType.LetAssignment:
                case TriplePatternType.BindAssignment:
                    q.RootGraphPattern.AddAssignment((IAssignmentPattern)tp);
                    break;
                case TriplePatternType.Filter:
                    q.RootGraphPattern.AddFilter(((IFilterPattern)tp).Filter);
                    break;
            }
            return q;
        }

        public static SparqlQuery Where(this SparqlQuery q, IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                q = q.Where(t.Subject, t.Predicate, t.Object);
            }
            return q;
        }

        public static SparqlQuery Where(this SparqlQuery q, IEnumerable<ITriplePattern> tps)
        {
            foreach (ITriplePattern tp in tps)
            {
                q = q.Where(tp);
            }
            return q;
        }

        public static SparqlQuery Where(this SparqlQuery q, GraphPattern gp)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            if (q.RootGraphPattern == null)
            {
                q.RootGraphPattern = gp;
            }
            else
            {
                q.RootGraphPattern.AddGraphPattern(gp);
            }
            return q;
        }

        public static SparqlQuery Optional(this SparqlQuery q, INode s, INode p, INode o)
        {
            return q.Optional(new TriplePattern(s.ToPatternItem(), p.ToPatternItem(), o.ToPatternItem()));
        }

        public static SparqlQuery Optional(this SparqlQuery q, ITriplePattern tp)
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
            return q.Where(gp);
        }

        public static SparqlQuery Filter(this SparqlQuery q, ISparqlExpression expr)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            if (q.RootGraphPattern == null) q.RootGraphPattern = new GraphPattern();
            q.RootGraphPattern.AddFilter(new UnaryExpressionFilter(expr));
            return q;
        }

        public static SparqlQuery Limit(this SparqlQuery q, int limit)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            q.Limit = limit;
            return q;
        }

        public static SparqlQuery Offset(this SparqlQuery q, int offset)
        {
            if (q == null) throw new ArgumentNullException("Null query");
            q.Offset = offset;
            return q;
        }

        public static SparqlQuery Slice(this SparqlQuery q, int limit, int offset)
        {
            return q.Limit(limit).Offset(offset);
        }

        /// <summary>
        /// Turns a Node into a Pattern item for use in a Triple Pattern
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        private static PatternItem ToPatternItem(this INode n)
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
        private static ISparqlExpression ToSparqlExpression(this INode n)
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
