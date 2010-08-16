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

#if !NO_WEB && !NO_ASP && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Handler for Sparql Queries which use the Sparql implementation of one the libraries supported Triple Stores which provide their own Sparql implementations
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effectively acts as a front-end to the Sparql engine of a native Triple Store for which dotNetRDF provides integration
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
    /// <li><strong>StoreType</strong> (<em>Required</em>) - Sets the type of Native Store being used, supported values are found in the <see cref="HandlerStoreTypes">HandlerStoreTypes</see> enumeration.  Any of the backing stores currently supported by dotNetRDF can be used with the Handler</li>
    /// <li><strong>StoreName</strong> (<em>Required/Optional</em>) - Sets the name of the Store (required for Talis, Sesame and Allegro; optional for Virtuoso; not used for 4store).  Defaults to the default Native Quad Store database <strong>DB</strong> for Virtuoso</li>
    /// <li><strong>StoreServer</strong> (<em>Optional</em>) - Sets the server that Store is running on, defaults to <strong>localhost</strong>.  For 4store, AllegroGraph and Sesame this is the Base Uri for accessing the server.</li>
    /// <li><strong>StoreCatalog</strong> (<em>Required/Optional</em>) - Sets the Catalog ID (for use with Allegro only)</li>
    /// <li><strong>StorePort</strong> (<em>Optional</em>) - Sets the server port that the Store is running on, defaults to <strong>1111</strong> (Only used by Virtuoso).  For 4store, AllegroGraph and Sesame any port information should be included in the Uri set in the <strong>StoreServer</strong> setting</li>
    /// <li><strong>StoreUser</strong> (<em>Required/Optional</em>) - Sets the Username for connecting to the Store (optional for Talis, Sesame and Allegro; required for Virtuoso; not used for 4store)</li>
    /// <li><strong>StorePassword</strong> (<em>Required/Optional</em>) - Sets the Password for connecting to the Store (optional for Talis, Sesame and Allegro; required for Virtuoso; not used for 4store)</li>
    /// </ul>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseQueryHandler and its concrete implementation QueryHandler",true)]
    public class NativeSparqlHandler : BaseSparqlHandler
    {
        private NativeSparqlHandlerConfiguration _config;

        /// <summary>
        /// Loads the Configuration for a Native Sparql Handler
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
                this._config = new NativeSparqlHandlerConfiguration(context, cacheKey, configPrefix);
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Retrieve from Cache
                this._config = (NativeSparqlHandlerConfiguration)context.Cache[cacheKey];
            }

            return this._config;
        }

        /// <summary>
        /// Process a Sparql Query by executing against the Native Stores Sparql implementation
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
                //Try to parse the Query bearing in mind that it may contain syntax we consider invalid
                //but which the Native Store accepts i.e. vendor specific Sparql extensions
                Object result;
                try
                {
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

                    //Set the Named Graph URIs
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

                    //Execute the Query against the Store
                    result = this._config.TripleStore.ExecuteQuery(sparqlquery.ToString());
                }
                catch (RdfParseException)
                {
                    //Parser Errors we ignore and attempt to submit the query anyway
                    //This simply implies that the query contains syntax which we don't support
                    result = this._config.TripleStore.ExecuteQuery(query);
                }
                catch
                {
                    //Other Errors we throw upwards to our Error Handlers
                    throw;
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