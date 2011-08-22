/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SemWeb;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage;
using SW = SemWeb;

namespace VDS.RDF.Interop.SemWeb
{
    /// <summary>
    /// Provides a dotNetRDF Natively Queryable Store as a StatementSource
    /// </summary>
    public class NativeStoreSource : StatementSource, QueryableSource
    {
        private Graph _g = new Graph();
        private INativelyQueryableStore _store;
        private SemWebMapping _mapping;

        /// <summary>
        /// Creates a new Native Store Source
        /// </summary>
        /// <param name="store">Natively Queryable Store</param>
        public NativeStoreSource(INativelyQueryableStore store)
        {
            this._store = store;
            this._mapping = new SemWebMapping(this._g);
        }

        /// <summary>
        /// Creates a new Native Store Source
        /// </summary>
        /// <param name="manager">Queryable Generic IO Manager</param>
        public NativeStoreSource(IQueryableGenericIOManager manager)
         : this(new NativeTripleStore(manager)) { }

        /// <summary>
        /// Gets the Native Store which this Source is a wrapper around
        /// </summary>
        public INativelyQueryableStore Store
        {
            get
            {
                return this._store;
            }
        }

        #region StatementSource Members

        /// <summary>
        /// Returns false since we have no idea how the underlying Store behaves in this regard
        /// </summary>
        public bool Distinct
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Selects all Statements from the underlying Store and adds them to a SemWeb Sink
        /// </summary>
        /// <param name="sink">Statement Sink</param>
        public void Select(StatementSink sink)
        {
            //Use a CONSTRUCT to get the Statements
            String query = "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}";
            Object results = this._store.ExecuteQuery(query);
            if (results is Graph)
            {
                Graph g = (Graph)results;
                foreach (Triple t in g.Triples)
                {
                    Statement stmt = SemWebConverter.ToSemWeb(t, this._mapping);
                    //Keep returning stuff until it tells us to stop
                    if (!sink.Add(stmt)) return;
                }
            }
        }

        #endregion

        #region QueryableSource Members

        /// <summary>
        /// Returns true regardless of the query as we don't know in advance if the underlying Store can answer the query or not
        /// </summary>
        /// <param name="graph">Graph Pattern</param>
        /// <param name="options">Query Options</param>
        /// <returns></returns>
        public SW.Query.MetaQueryResult MetaQuery(Statement[] graph, SW.Query.QueryOptions options)
        {
            SW.Query.MetaQueryResult result = new SW.Query.MetaQueryResult();
            result.QuerySupported = true;
            return result;
        }

        /// <summary>
        /// Queries the Store using the Graph Pattern specified by the set of Statement Patterns
        /// </summary>
        /// <param name="graph">Graph Pattern</param>
        /// <param name="options">Query Options</param>
        /// <param name="sink">Results Sink</param>
        /// <remarks>
        /// <para>
        /// Implemented by converting the Statement Patterns into a SPARQL SELECT query and executing that against the underlying Store's SPARQL engine
        /// </para>
        /// <para>
        /// The only Query Option that is supported is the Limit option
        /// </para>
        /// </remarks>
        public void Query(Statement[] graph, SW.Query.QueryOptions options, SW.Query.QueryResultSink sink)
        {
            //Implement as a SPARQL SELECT
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.CommandText = "SELECT * WHERE {";

            int p = 0;
            foreach (Statement stmt in graph)
            {
                //Add Subject
                queryString.CommandText += "\n";
                if (stmt.Subject is Variable)
                {
                    queryString.CommandText += stmt.Subject.ToString();
                }
                else
                {
                    queryString.CommandText += "@param" + p;
                    queryString.SetParameter("param" + p, SemWebConverter.FromSemWeb(stmt.Subject, this._mapping));
                    p++;
                }
                queryString.CommandText += " ";

                //Add Predicate
                if (stmt.Predicate is Variable)
                {
                    queryString.CommandText += stmt.Predicate.ToString();
                }
                else
                {
                    queryString.CommandText += "@param" + p;
                    queryString.SetParameter("param" + p, SemWebConverter.FromSemWeb(stmt.Predicate, this._mapping));
                    p++;
                }
                queryString.CommandText += " ";

                //Add Object
                if (stmt.Object is Variable)
                {
                    queryString.CommandText += stmt.Object.ToString();
                }
                else
                {
                    queryString.CommandText += "@param" + p;
                    queryString.SetParameter("param" + p, SemWebConverter.FromSemWeb(stmt.Object, this._mapping));
                    p++;
                }
                queryString.CommandText += " .";
            }

            queryString.CommandText += "}";

            //Execute the Query and convert the Results
            Object results = this._store.ExecuteQuery(queryString.ToString());

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                sink.Init(rset.Variables.Select(v => new Variable(v)).ToArray());
                if (rset.Count > 0)
                {
                    int c = 0;
                    foreach (SparqlResult r in rset)
                    {
                        //Apply Limit if applicable
                        if (options.Limit > 0 && c >= options.Limit)
                        {
                            sink.Finished();
                            return;
                        }

                        //Convert the Set to VariableBindings for SemWeb
                        Variable[] vars = r.Variables.Select(v => new Variable(v)).ToArray();
                        Resource[] resources = r.Variables.Select(v => SemWebConverter.ToSemWeb(r[v], this._mapping)).ToArray();
                        SW.Query.VariableBindings bindings = new SW.Query.VariableBindings(vars, resources);

                        //Keep adding results until the sink tells us to stop
                        if (!sink.Add(bindings))
                        {
                            sink.Finished();
                            return;
                        }
                        c++;
                    }
                    sink.Finished();
                }
                else
                {
                    sink.Finished();
                }
            }
            else
            {
                throw new RdfQueryException("Query returned an unexpected result where a SPARQL Result Set was expected");
            }
        }

        #endregion

        #region SelectableSource Members

        /// <summary>
        /// Returns whether the Store contains Statements matching a given Template
        /// </summary>
        /// <param name="template">Template</param>
        /// <returns></returns>
        public bool Contains(Statement template)
        {
            List<ITriplePattern> patterns = this.TemplateToTriplePatterns(template);
            StringBuilder query = new StringBuilder();
            query.AppendLine("ASK WHERE {");
            foreach (ITriplePattern pattern in patterns)
            {
                query.AppendLine(pattern.ToString());
            }
            query.AppendLine("}");

            return this.ContainsInternal(query.ToString());    
        }

        /// <summary>
        /// Returns whether the Store contains Statements which contain the given Resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public bool Contains(Resource resource)
        {
            //Implemented as a SPARQL ASK
            String query = "ASK WHERE {{" + resource.ToString() + " ?p ?o} UNION {?s " + resource.ToString() + " ?o} UNION {?s ?p " + resource.ToString() + "}}";
            return this.ContainsInternal(query);
        }

        /// <summary>
        /// Internal Helper for making the query and extracting the result from the Result Set
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        private bool ContainsInternal(String query)
        {
            Object results = this._store.ExecuteQuery(query);
            if (results is SparqlResultSet)
            {
                return ((SparqlResultSet)results).Result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Selects Statements that match a given Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="sink">Sink</param>
        /// <remarks>
        /// Currently not implemented
        /// </remarks>
        public void Select(SelectFilter filter, StatementSink sink)
        {
            //Implement as a SPARQL SELECT which UNIONS over all the possibles
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects Statements that match a given Template
        /// </summary>
        /// <param name="template">Statement Template</param>
        /// <param name="sink">Sink</param>
        public void Select(Statement template, StatementSink sink)
        {
            //Implement as a SPARQL SELECT
            List<ITriplePattern> patterns = this.TemplateToTriplePatterns(template);
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * WHERE {");
            foreach (ITriplePattern pattern in patterns)
            {
                query.AppendLine(pattern.ToString() + ".");
            }
            query.AppendLine("}");

            //Get the Results
            Object results = this._store.ExecuteQuery(query.ToString());
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Entity s = (template.Subject != null) ? template.Subject : SemWebConverter.ToSemWebEntity(r["s"], this._mapping);
                    Entity p = (template.Predicate != null) ? template.Predicate : SemWebConverter.ToSemWebEntity(r["p"], this._mapping);
                    Resource o = (template.Object != null) ? template.Object : SemWebConverter.ToSemWeb(r["o"], this._mapping);
                    Statement stmt = new Statement(s, p, o);
                    //Keep returning stuff until the sink tells us to stop
                    if (!sink.Add(stmt)) return;
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method which converts a SemWeb resource into a PatternItem for use in a SPARQL Triple Pattern
        /// </summary>
        /// <param name="r">Resource</param>
        /// <param name="mapping">Mapping of Variables &amp; Blank Nodes to Pattern Items</param>
        /// <returns></returns>
        private PatternItem FromSemWeb(Resource r, Dictionary<String, PatternItem> mapping)
        {
            if (r is Variable)
            {
                if (mapping.ContainsKey(r.ToString()))
                {
                    return mapping[r.ToString()];
                }
                else
                {
                    PatternItem temp = new VariablePattern(r.ToString());
                    mapping.Add(r.ToString(), temp);
                    return temp;
                }
            }
            else if (r is BNode)
            {
                if (mapping.ContainsKey(r.ToString()))
                {
                    return mapping[r.ToString()];
                }
                else
                {
                    PatternItem temp = new BlankNodePattern(r.ToString().Substring(2));
                    mapping.Add(r.ToString(), temp);
                    return temp;
                }
            }
            else
            {
                return new NodeMatchPattern(SemWebConverter.FromSemWeb(r, this._mapping));
            }
        }

        private List<ITriplePattern> TemplateToTriplePatterns(Statement template)
        {
            //Implemented as a SPARQL ASK
            List<ITriplePattern> patterns = new List<ITriplePattern>();
            Dictionary<String, PatternItem> mapping = new Dictionary<string, PatternItem>();

            //Build a Triple Pattern for each part of the Statement
            PatternItem s, p, o;
            s = (template.Subject == null) ? new VariablePattern("?s") : this.FromSemWeb(template.Subject, mapping);
            p = (template.Predicate == null) ? new VariablePattern("?p") : this.FromSemWeb(template.Predicate, mapping);
            o = (template.Object == null) ? new VariablePattern("?o") : this.FromSemWeb(template.Object, mapping);
            patterns.Add(new TriplePattern(s, p, o));

            //If any of the parts were a Blank Node apply an IsBlank() FILTER
            if (template.Subject is BNode && !(template.Subject is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(s.VariableName)))));
            if (template.Predicate is BNode && !(template.Predicate is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(p.VariableName)))));
            if (template.Object is BNode && !(template.Object is Variable)) patterns.Add(new FilterPattern(new UnaryExpressionFilter(new IsBlankFunction(new VariableExpressionTerm(o.VariableName)))));

            return patterns;
        }

        #endregion
    }
}
