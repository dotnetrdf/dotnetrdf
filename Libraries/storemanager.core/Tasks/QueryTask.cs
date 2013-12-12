/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    /// <summary>
    /// Task for running queries
    /// </summary>
    public class QueryTask
        : NonCancellableTask<Object>
    {
        private readonly String _query;
        private readonly IQueryableStorage _manager;
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlQuery _q;
        private readonly GenericQueryProcessor _processor;
        private readonly bool _usePaging = false;
        private readonly int _pageSize = 1000;

        /// <summary>
        /// Creates a new Query Task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="query">Query</param>
        public QueryTask(IQueryableStorage manager, String query)
            : base("SPARQL Query")
        {
            this._manager = manager;
            this._processor = new GenericQueryProcessor(manager);
            this._query = query;
        }

        /// <summary>
        /// Creates a new Query Task
        /// </summary>
        /// <param name="manager">Storage Provider</param>
        /// <param name="query">Query</param>
        /// <param name="pageSize">Page Size</param>
        public QueryTask(IQueryableStorage manager, String query, int pageSize)
            : this(manager, query)
        {
            this._usePaging = true;
            this._pageSize = pageSize;
        }

        /// <summary>
        /// Runs the task
        /// </summary>
        /// <returns></returns>
        protected override object RunTaskInternal()
        {
            try
            {
                //Firstly try and parse the Query
                this._q = this._parser.ParseFromString(this._query);
            }
            catch
            {
                this.Information = "Query is not valid SPARQL 1.0/1.1 - will attempt to evaluate it but underlying store may reject the query...";
            }

            // Successfuly parsed query
            if (this._q != null)
            {
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
                                IGraph temp = (IGraph) result;

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
                                    SparqlResultSet rset = (SparqlResultSet) result;
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
                    if (this._q.QueryExecutionTime.HasValue)
                    {
                        this.Information = "Query Failed (Took " + this._q.QueryExecutionTime.Value + ")";
                    }
                    else
                    {
                        this.Information = "Query Failed";
                    }
                    throw;
                }
            }

            // Unsuccessfully parsed query - may be using syntax extensions specific to the target store
            DateTime start = DateTime.Now;
            try
            {
                if (this._usePaging)
                {
                    throw new RdfQueryException("Cannot apply paging to a Query that we cannot parse as a valid SPARQL 1.0/1.1 query");
                }
                Object results = this._manager.Query(this._query);
                this.Information = "Query Completed OK (Took " + (DateTime.Now - start) + ")";
                return results;
            }
            catch
            {
                //Try and show the execution time if possible
                try
                {
                    this.Information = "Query Failed (Took " + (DateTime.Now - start) + ")";
                }
                catch
                {
                    this.Information = "Query Failed";
                }
                throw;
            }
        }

        /// <summary>
        /// Returns that the task cannot be cancelled
        /// </summary>
        public override bool IsCancellable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the query (assuming it is valid standard SPARQL)
        /// </summary>
        public SparqlQuery Query
        {
            get { return this._q; }
        }
    }
}