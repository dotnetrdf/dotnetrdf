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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Handler for Sparql Queries which use the libraries Sparql implementation over a Store which contains RDF data stored on disk in a single file/folder
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effectively acts as a gateway to a Remote Endpoint, queries are executed against the endpoint and the raw Response Stream received is passed directly back to the Client.
    /// </para>
    /// <para>
    /// This Handler supports registering the Handler multiple times in one Web application with each able to use its own settings.
    /// </para>
    /// <para>
    /// Each Handler registered in Web.config may have a prefix for their Configuration variables set by adding a AppSetting key using the virtual path of the handler like so:
    /// <code>&lt;add key="/virtualRoot/sparql/" value="ABC" /&gt;</code>
    /// Then when the Handler at that path is invoked it will look for Configuration variables prefixed with that name.
    /// </para>
    /// <para>
    /// The following Configuration Variables are supported in addition to those supported by the <see cref="BaseSparqlHandler">BaseSparqlHandler</see>:
    /// </para>
    /// <ul>
    /// <li>
    ///     One of the following variables must be specified for the data source, <strong>DataFolder</strong> is checked before <strong>DataFile</strong>.  <strong>DataFile</strong> is ignored if <strong>DataFolder</strong> is specified:
    ///     <ol>
    ///     <li><strong>DataFolder</strong> - A relative path to a Folder containing RDF files that will be loaded into the Store and queried against by the Handler</li>
    ///     <li><strong>DataFile</strong> - A relative path to a single RDF file that will be loaded into the Store and queried against by the Handler</li>
    ///     </ol>
    /// </li>
    /// </ul>
    /// <para>
    /// If the Folder/File does not exist then the Handler will throw an Error when you first attempt to access it.
    /// </para>
    /// <para>
    /// In the event that there are no valid RDF files loaded the Handler will execute queries over an Empty Store
    /// </para>
    /// <para>
    /// As with the <see cref="SparqlHandler">SparqlHandler</see> you can attach both Reasoners and Custom Expression Factories to this Handler, see the <see cref="SparqlHandler">SparqlHandler</see> documentation for details of how to do this.
    /// </para>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseQueryHandler and its concrete implementation QueryHandler",true)]
    public class FileSparqlHandler : BaseSparqlHandler
    {
        private FileSparqlHandlerConfiguration _config;

        /// <summary>
        /// Loads the Configuration for a File Sparql Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <returns></returns>
        protected override BaseSparqlHandlerConfiguration LoadConfig(HttpContext context)
        {
            //Get the Request Path
            //Use this to get the Prefix that we'll use for our Config variables lookup
            String cacheKey = context.Request.Path;

            String configPrefix;
            try
            {
                configPrefix = ConfigurationManager.AppSettings[cacheKey];
            }
            catch
            {
                configPrefix = String.Empty;
            }

            if (context.Cache[cacheKey] == null)
            {
                //No Config Cached so create a new Config object which will load the Config and cache the Manager
                this._config = new FileSparqlHandlerConfiguration(context, cacheKey, configPrefix);
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Retrieve from Cache
                this._config = (FileSparqlHandlerConfiguration)context.Cache[cacheKey];
            }

            return this._config;
        }

        /// <summary>
        /// Updates the Configuration in the Cache to reflect additional Graphs that have been added
        /// </summary>
        /// <param name="context"></param>
        protected void RecacheConfig(HttpContext context)
        {
            //Get the Request Path
            //Use this to get the Prefix that we'll use for our Config variables lookup
            String cacheKey = context.Request.Path;

            String configPrefix;
            try
            {
                configPrefix = ConfigurationManager.AppSettings[cacheKey];
            }
            catch
            {
                configPrefix = String.Empty;
            }

            if (context.Cache[cacheKey] == null)
            {
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                context.Cache[cacheKey] = this._config;
            }
        }

        /// <summary>
        /// Process a Sparql Query by executing against the Data from the File/Folder which has been loaded into an in-memory queryable Triple Store
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="userDefaultGraphs">User specified default Graph(s)</param>
        /// <param name="userNamedGraphs">User specified named Graph(s)</param>
        /// <param name="timeout">User specified timeout</param>
        /// <param name="partialResults">Partial Results setting</param>
        protected override void ProcessQuery(HttpContext context, string query, List<String> userDefaultGraphs, List<String> userNamedGraphs, long timeout, bool partialResults)
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

                
                //Try and parse the Query
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

                //Get the Store from the Cache and execute the Query
                IInMemoryQueryableStore store = this._config.TripleStore;
                int graphCount = store.Graphs.Count;
                Object result = store.ExecuteQuery(sparqlquery);

                //If we're not using a fixed Dataset then we many need to update the cache
                if (store is WebDemandTripleStore)
                {
                    if (graphCount < store.Graphs.Count)
                    {
                        //Need to recache the Config
                        this.RecacheConfig(context);
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
        }
    }
}

#endif