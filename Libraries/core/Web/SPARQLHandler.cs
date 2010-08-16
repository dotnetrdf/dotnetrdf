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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_WEB && !NO_ASP && !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Handler for Sparql Queries which uses the libraries in-memory Sparql implementation against a SQL backed Store
    /// </summary>
    /// <remarks>
    /// <p>
    /// <strong>Warning:</strong> This class is now considered obsolete since it is renamed to <see cref="SqlSparqlHandler">SqlSparqlHandler</see> from the 0.3.0 release onwards in order to more clearly indicate its function and differentiate it from the other SPARQL Handlers in the API.  Please switch your existing code to use <see cref="SqlSparqlHandler">SqlSparqlHandler</see>
    /// </p>
    /// <p>
    /// This Handler supports registering the Handler multiple times in one Web application with each able to use its own settings.
    /// </p>
    /// <p>
    /// Each Handler registered in Web.config may have a prefix for their Configuration variables set by adding a AppSetting key using the virtual path of the handler like so:
    /// <code>&lt;add key="/virtualRoot/sparql/" value="ABC" /&gt;</code>
    /// Then when the Handler at that path is invoked it will look for Configuration variables prefixed with that name.
    /// </p>
    /// <p>
    /// The following Configuration Variables are supported in addition to those supported by the <see cref="BaseSparqlHandler">BaseSparqlHandler</see>:
    /// </p>
    /// <ul>
    /// <li><strong>DBType</strong> (<em>Required for non-Microsoft SQL Server Stores</em>) - Sets the Database Type to use, defaults to Microsoft SQL Server if not set</li>
    /// <li><strong>DBServer</strong> (<em>Required</em>) - Sets the Database Server for the underlying Store</li>
    /// <li><strong>DBName</strong> (<em>Required</em>) - Sets the Database Name for the underlying Store</li>
    /// <li><strong>DBUser</strong> (<em>Required</em>) - Sets the Database Username for the underlying Store</li>
    /// <li><strong>DBPassword</strong> (<em>Required</em>) - Sets the Database Password for the underlying Store</li>
    /// <li><strong>DBPort</strong> (<em>Optional for Non-Native Virtuoso Stores</em>) - Sets the Database Port for the underlying Store, defaults to the Virtuoso default port of 1111</li>
    /// <li><strong>LoadMode</strong> (<em>Optional</em>) - Controls how and when the Handler should load the store into memory.  The value of this should be one of the values from <see cref="SparqlLoadMode">SparqlLoadMode</see>, see the API for the enumeration for information on how each mode behaves.  Defaults to <see cref="SparqlLoadMode.OnDemand">OnDemand</see></li>
    /// <li>
    ///     You can attach one/more Inference Engines to the Store using the following configuration variables.  The <strong>X</strong> in these examples indicate a 1 based index:
    ///     <ul>
    ///         <li><strong>ReasonerX</strong> (<em>Optional</em>) - Adds an instance of the given <see cref="IInferenceEngine">IInferenceEngine</see> implementing class specified by a fully qualified type name as a Reasoner to the Store</li>
    ///         <li><strong>ReasonerAssemblyX</strong> (<em>Optional</em>) - Specifies the name of the assembly that the Reasoner type is located in, if this variable is not specified it is assumed that the Reasoner is a type from dotNetRDF</li>
    ///         <li><strong>ReasonerRulesGraphX</strong> (<em>Optional</em>) - Specifies the Uri of the Graph that is used to initialise the Reasoner</li>
    ///     </ul>
    ///     For example:
    ///     <code>
    ///     &lt;add key="Reasoner1" value="VDS.RDF.Query.Inference.StaticRdfsReasoner" /&gt;
    ///     &lt;add key="ReasonerRulesGraph1" value="http://example.org/myClassSchema" /&gt;
    ///     </code>
    /// </li>
    /// <li>
    ///     You can also attach one/more Custom Expression Factories to the Store using the following configuration variables.  The <strong>X</strong> in these examples indicate a 1 based index:
    ///     <ul>
    ///         <li><strong>ExpressionFactoryX</strong> (<em>Optional</em>) - Adds an instance of the given <see cref="ISparqlCustomExpressionFactory">ISparqlCustomExpressionFactory</see> implementing class specified by a fully qualified type name as a Custom Expression Factory to the SPARQL Parser</li>
    ///         <li><strong>ExpressionFactoryAssemblyX</strong> (<em>Optional</em>) - Specifies the name of the assembly that the Custom Expression Factory type is located in, if this variable is not specified it is assumed that the Factory is a type from dotNetRDF</li>
    ///     </ul>
    ///     For example:
    ///     <code>
    ///     &lt;add key="ExpressionFactory1" value="VDS.RDF.Query.Expressions.ArqFunctionFactory" /&gt;
    ///     </code>
    ///     A Custom Expression Factory allows you to create a means to define your own custom extension functions and have them executed as part of queries made using this Handler.  For details on how to do this take a look at the <a href="http://www.dotnetrdf.org/content.asp?pageID=SPARQL%20Extension%20Functions">online documentation</a>
    /// </li>
    /// </ul>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseQueryHandler and its concrete implementation QueryHandler", true)]
    public class SparqlHandler : BaseSparqlHandler
    {
        private SqlSparqlHandlerConfiguration _config;
        private List<Uri> _commonNamespaces = new List<Uri>() {
            new Uri(NamespaceMapper.RDF),
            new Uri(NamespaceMapper.RDFS),
            new Uri(NamespaceMapper.XMLSCHEMA)
        };

        private const int MinLoad = 10, MaxLoad = 50;

        /// <summary>
        /// Processes the Sparql Query Request
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="userDefaultGraphs">User specified default Graph(s)</param>
        /// <param name="userNamedGraphs">User specified named Graph(s)</param>
        /// <param name="timeout">User specified timeout</param>
        /// <param name="partialResults">Partial Results setting</param>
        protected override void ProcessQuery(HttpContext context, String query, List<String> userDefaultGraphs, List<String> userNamedGraphs, long timeout, bool partialResults)
        {
            try
            {
                //Register any Custom Expression Factories
                if (this._config.HasExpressionFactories)
                {
                    foreach (ISparqlCustomExpressionFactory factory in this._config.ExpressionFactories)
                    {
                        SparqlExpressionFactory.AddCustomFactory(factory);
                    }
                }

                //Try to parse the Query
                SparqlQueryParser queryparser = new SparqlQueryParser();
                SparqlQuery sparqlquery = queryparser.ParseFromString(query);

                //Set the Default Graph URIs (if any)
                if (userDefaultGraphs.Count > 0)
                {
                    //Default Graph Uri specified by default-graph-uri parameter or Web.config settings
                    foreach (String userDefaultGraph in userDefaultGraphs)
                    {
                        if (!userDefaultGraph.Equals(String.Empty))
                        {
                            sparqlquery.AddDefaultGraph(new Uri(userDefaultGraph));
                        }
                    }
                }
                else if (!this._config.DefaultGraphURI.Equals(String.Empty))
                {
                    //Only applies if the Query doesn't specify any Default Graph
                    if (!sparqlquery.DefaultGraphs.Any())
                    {
                        sparqlquery.AddDefaultGraph(new Uri(this._config.DefaultGraphURI));
                    }
                }

                //Set the Named Graph URIs (if any)
                if (userNamedGraphs.Count > 0)
                {
                    foreach (String userNamedGraph in userNamedGraphs)
                    {
                        if (!userNamedGraph.Equals(String.Empty))
                        {
                            sparqlquery.AddNamedGraph(new Uri(userNamedGraph));
                        }
                    }
                }

                //Set Timeout setting
                if (timeout > 0)
                {
                    sparqlquery.Timeout = timeout;
                }
                else
                {
                    sparqlquery.Timeout = this._config.DefaultTimeout;
                }

                //Set Partial Results Setting                 
                sparqlquery.PartialResultsOnTimeout = partialResults;

                //Get the Request Path
                //Use this as a Cache Key and to lookup the Prefix that we'll use for our Config variables lookup later
                String requestPath = context.Request.Path;

                //Have we got the Store cached?
                IInMemoryQueryableStore store = (IInMemoryQueryableStore)context.Cache[requestPath];
                if (store == null) throw new RdfQueryException("No Store is available to be queried - required Store is not in Cache as expected");

                //Execute the Query
                int graphCount = 0;
                switch (this._config.LoadMode)
                {
                    case SparqlLoadMode.OnDemandEnhanced:
                    case SparqlLoadMode.OnDemandAggressive:
                        //Analyse the Query to look for URIs used in Triple Patterns
                        graphCount = store.Graphs.Count;
                        this.LoadOnDemand(sparqlquery, (OnDemandTripleStore)store);

                        break;
                    default:
                        //No special actions
                        break;
                }
                Object result = store.ExecuteQuery(sparqlquery);

                //Recache an on-demand store after the query has occurred to ensure that loaded data remains cached
                if (store is OnDemandTripleStore)
                {
                    if (graphCount < store.Graphs.Count && this._config.CacheDuration > 0)
                    {
                        context.Cache.Add(requestPath, store, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                    }
                }

                //Add an additional Header indicating if we timed out
                if (sparqlquery.QueryTime >= sparqlquery.Timeout)
                {
                    try
                    {
                        context.Response.Headers.Add("X-SPARQL-TimedOut", "true");
                    }
                    catch (PlatformNotSupportedException)
                    {
                        context.Response.AddHeader("X-SPARQL-TimedOut", "true");
                    }
                }

                //Send Results to Client
                this.ProcessResults(context, result);

            }
            catch (RdfParseException parseEx)
            {
                HandleErrors(context, "Parsing Error", query, parseEx);
            }
            catch (RdfQueryException queryEx)
            {
                HandleErrors(context, "Query Error", query, queryEx);
            }
            catch (RdfException rdfEx)
            {
                HandleErrors(context, "RDF Error", query, rdfEx);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", query, ex);
            }
            finally
            {
                if (this._config.CacheDuration == 0)
                {
                    context.Cache.Remove(context.Request.Path);
                }
            }
        }

        /// <summary>
        /// Loads the Configuration for the Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <returns></returns>
        protected override BaseSparqlHandlerConfiguration LoadConfig(HttpContext context)
        {
            //Get the Request Path
            //Use this to get the Prefix that we'll use for our Config variables lookup
            String requestPath = context.Request.Path;

            String configPrefix;
            try
            {
                configPrefix = ConfigurationManager.AppSettings[requestPath];
            }
            catch
            {
                configPrefix = String.Empty;
            }

            if (context.Cache[requestPath] == null)
            {
                //No Store Cached so create a new Config object which will load the Config and cache the Store
                this._config = new SqlSparqlHandlerConfiguration(context, requestPath, configPrefix);
                context.Cache.Add(requestPath + "Config", this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Store Cached so might have Config cached also
                if (context.Cache[requestPath + "Config"] == null)
                {
                    //No Cached Config so create a new Config object to load it
                    //Store won't be loaded as the Config loader will detect that it is already cached
                    this._config = new SqlSparqlHandlerConfiguration(context, requestPath, configPrefix);
                    context.Cache.Add(requestPath + "Config", this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
                }
                else
                {
                    //Retrieve from Cache
                    this._config = (SqlSparqlHandlerConfiguration)context.Cache[requestPath + "Config"];
                }
            }

            return this._config;
        }

        private void LoadOnDemand(SparqlQuery query, OnDemandTripleStore store)
        {
            if (this._config.Manager is IDotNetRDFStoreManager)
            {
                if (query.RootGraphPattern != null)
                {
                    this.LoadOnDemandGraphPattern(query.RootGraphPattern, store);
                }
            }
        }

        private void LoadOnDemandGraphPattern(GraphPattern gp, OnDemandTripleStore store)
        {
            foreach (TriplePattern p in gp.TriplePatterns)
            {
                this.LoadOnDemandPatternItem(p.Subject, store);
                this.LoadOnDemandPatternItem(p.Predicate, store);
                this.LoadOnDemandPatternItem(p.Object, store);
            }
            foreach (GraphPattern cgp in gp.ChildGraphPatterns)
            {
                this.LoadOnDemandGraphPattern(cgp, store);
            }
        }

        private void LoadOnDemandPatternItem(PatternItem item, OnDemandTripleStore store)
        {
            if (item is NodeMatchPattern)
            {
                NodeMatchPattern p = (NodeMatchPattern)item;
                if (p.Node.NodeType == NodeType.Uri)
                {
                    UriNode uri = (UriNode)p.Node;
                    if (this._config.LoadMode == SparqlLoadMode.OnDemandAggressive || !this._commonNamespaces.Any(u => u.IsBaseOf(uri.Uri)))
                    {
                        //Find all the Graphs that contain this Node
                        try
                        {
                            this._config.Manager.Open(true);

                            String id = this._config.Manager.SaveNode(uri);
                            String lookup = "SELECT DISTINCT graphUri FROM GRAPHS INNER JOIN GRAPH_TRIPLES G ON GRAPHS.graphID=G.graphID INNER JOIN TRIPLES T ON G.tripleID=T.tripleID LEFT OUTER JOIN NODES X ON tripleSubject=X.nodeID LEFT OUTER JOIN NODES Y ON triplePredicate=Y.nodeID LEFT OUTER JOIN NODES Z ON tripleObject=Z.nodeID WHERE X.nodeID=" + id + " OR Y.nodeID=" + id + " OR Z.nodeID=" + id;
                            DataTable data = this._config.Manager.ExecuteQuery(lookup);

                            //See how many (if any) Graphs we got
                            if (data.Rows.Count > 0)
                            {
                                int i = 0;
                                int loaded = 0;

                                //Decide how many Graphs we should Load
                                int loadLimit = Math.Max(Math.Min(MinLoad, data.Rows.Count), Math.Min(MaxLoad, data.Rows.Count / 4));

                                while (loaded < loadLimit && i < data.Rows.Count)
                                {
                                    Uri graphUri = new Uri(data.Rows[i]["graphUri"].ToString());
                                    if (!store.Graphs.Any(g => g.BaseUri.GetEnhancedHashCode() == graphUri.GetEnhancedHashCode()))
                                    {
                                        store.Graphs.Contains(graphUri);
                                        loaded++;
                                    }
                                    i++;
                                }
                            }

                            this._config.Manager.Close(true);
                        }
                        catch
                        {
                            this._config.Manager.Close(true);
                        }
                    }
                }
            }
        }

    }
}

#endif