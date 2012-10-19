using System;
using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public interface IQueryBuilder 
    {
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        IQueryBuilder Distinct();

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
        IQueryBuilder Where(String s, String p, String o);

        /// <summary>
        /// Adds a Triple Pattern to the query
        /// </summary>
        /// <param name="q">Query</param>
        /// <param name="s">Subject to match</param>
        /// <param name="p">Predicate to match</param>
        /// <param name="o">Object to match</param>
        /// <returns></returns>
        IQueryBuilder Where(INode s, INode p, INode o);

        IQueryBuilder Where(ITriplePattern tp);
        IQueryBuilder Where(IEnumerable<Triple> ts);
        IQueryBuilder Where(IEnumerable<ITriplePattern> tps);
        IQueryBuilder Where(GraphPattern gp);
        IQueryBuilder Optional(INode s, INode p, INode o);
        IQueryBuilder Optional(ITriplePattern tp);
        IQueryBuilder Filter(ISparqlExpression expr);
        IQueryBuilder Limit(int limit);
        IQueryBuilder Offset(int offset);
        IQueryBuilder Slice(int limit, int offset);
        SparqlQuery GetExecutableQuery();
    }
}