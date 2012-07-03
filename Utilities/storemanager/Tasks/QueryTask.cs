/*

Copyright Robert Vesse 2009-12
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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class QueryTask : NonCancellableTask<Object>
    {
        private String _query;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlQuery _q;
        private GenericQueryProcessor _processor;
        private bool _usePaging = false;
        private int _pageSize = 1000;

        public QueryTask(IQueryableStorage manager, String query)
            : base("SPARQL Query")
        {
            this._processor = new GenericQueryProcessor(manager);
            this._query = query;
        }

        public QueryTask(IQueryableStorage manager, String query, int pageSize)
            : this(manager, query)
        {
            this._usePaging = true;
            this._pageSize = pageSize;
        }

        protected override object RunTaskInternal()
        {
            //Firstly try and parse the Query
            this._q = this._parser.ParseFromString(this._query);

            //Then apply it to the Manager using the GenericQueryProcessor
            try
            {
                //Check that paging can be used if it was enabled
                if (this._usePaging)
                {
                    if (this._q.Limit >= 0 || this._q.Offset > 0)
                    {
                        throw new RdfQueryException("Cannot apply query paging when the SPARQL Query already contains an explicit LIMIT and/or OFFSET clause");
                    }
                    else if (this._q.QueryType == SparqlQueryType.Ask)
                    {
                        throw new RdfQueryException("Cannot apply query paging to an ASK Query");
                    }
                }

                int offset = 0;
                TimeSpan totalTime = TimeSpan.Zero;

                switch (this._q.QueryType)
                {
                    case SparqlQueryType.Ask:
                        SparqlResultSet blnResult = this._processor.ProcessQuery(this._q) as SparqlResultSet;
                        if (blnResult == null) throw new RdfQueryException("Store did not return a SPARQL Result Set for the ASK query as was expected");
                        return blnResult;

                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        Graph g = new Graph();
                        g.NamespaceMap.Import(this._q.NamespaceMap);

                        do
                        {
                            if (this._usePaging)
                            {
                                this._q.Limit = this._pageSize;
                                this._q.Offset = offset;
                            }
                            Object result = this._processor.ProcessQuery(this._q);
                            totalTime += this._q.QueryExecutionTime.Value;

                            if (!(result is IGraph)) throw new RdfQueryException("SPARQL Query did not return a RDF Graph as expected");
                            IGraph temp = (IGraph)result;

                            //If no further results can halt
                            if (temp.Triples.Count == 0) break;
                            offset += this._pageSize;

                            //Merge the partial result into the final result
                            g.Merge(temp);

                        } while (this._usePaging);

                        this.Information = "Query Completed OK (Took " + totalTime.ToString() + ")";
                        return g;

                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        SparqlResultSet results = new SparqlResultSet();
                        ResultSetHandler handler = new ResultSetHandler(results);
                        try
                        {
                            handler.StartResults();

                            do
                            {
                                if (this._usePaging)
                                {
                                    this._q.Limit = this._pageSize;
                                    this._q.Offset = offset;
                                }
                                Object result = this._processor.ProcessQuery(this._q);
                                totalTime += this._q.QueryExecutionTime.Value;

                                if (!(result is SparqlResultSet)) throw new RdfQueryException("SPARQL Query did not return a SPARQL Result Set as expected");
                                SparqlResultSet rset = (SparqlResultSet)result;
                                foreach (String var in rset.Variables)
                                {
                                    handler.HandleVariable(var);
                                }

                                //If no further results can halt
                                if (rset.Count == 0) break;
                                offset += this._pageSize;

                                //Merge the partial result into the final result
                                foreach (SparqlResult r in rset)
                                {
                                    handler.HandleResult(r);
                                }
                            } while (this._usePaging);

                            handler.EndResults(true);
                        }
                        catch
                        {
                            handler.EndResults(false);
                            throw;
                        }
                        this.Information = "Query Completed OK (Took " + totalTime.ToString() + ")";
                        return results;

                    default:
                        throw new RdfQueryException("Cannot evaluate an unknown query type");
                }
            }
            catch
            {
                //Try and show the execution time if possible
                try 
                {
                    this.Information = "Query Failed (Took " + this._q.QueryExecutionTime.Value.ToString() + ")";
                } 
                catch 
                {
                    this.Information = "Query Failed";
                }
                throw;
            }
        }

        public override bool IsCancellable
        {
            get 
            {
                return false; 
            }
        }

        public SparqlQuery Query
        {
            get
            {
                return this._q;
            }
        }
    }
}
