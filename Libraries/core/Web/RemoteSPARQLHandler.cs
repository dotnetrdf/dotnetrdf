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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Net;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Handler for Sparql Queries where the Sparql Endpoint is provided by some remote Endpoint
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
    /// <li><strong>EndpointURI</strong> (<em>Required</em>) - Sets the Remote Sparql Endpoint Uri to which queries will be sent for processing.</li>
    /// <li><strong>SupportsTimeout</strong> (<em>Optional</em>) - Sets whether the Remote Endpoint supports specifying a query timeout.  Defaults to false (Disabled).  If Timeout is not supported then the Handler will simulate the ability to set a Timeout by setting a HTTP Timeout on the HTTP request to the remote endpoint, in this mode partial results behaviour is not supported.</li>
    /// <li><strong>TimeoutField</strong> (<em>Optional</em>) - Sets the querystring field name used to specify the timeout when supported, defaults to <em>timeout</em></li>
    /// <li><strong>SupportsPartialResults</strong> (<em>Optional</em>) - Sets whether the Remote Endpoint supports specifying the partial results behaviour in the event of a query timeout.  Defaults to false (disabled)</li>
    /// <li><strong>PartialResultsField</strong> (<em>Optional</em>) - Sets the querystring field name used to specify the partial results behaviour when supported, defaults to <em>partialResults</em></li>
    /// </ul>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseQueryHandler and its concrete implementation QueryHandler",true)]
    public class RemoteSparqlHandler : BaseSparqlHandler
    {
        private RemoteSparqlHandlerConfiguration _config;

        /// <summary>
        /// Processes a Query by sending a HTTP Request to a Remote Sparql Endpoint and passing the raw response stream back to the Client
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="userDefaultGraphs">User specified default Graph(s)</param>
        /// <param name="userNamedGraphs">User specified names Graph(s)</param>
        /// <param name="timeout">User specified timeout</param>
        /// <param name="partialResults">Partial Results setting</param>
        /// <remarks>
        /// The HTTP Accept header passed to the remote endpoint will be the same Accept header which this Handler receives.  If the remote endpoint does content negotiation correctly the Client should get the results in a format they can understand
        /// </remarks>
        protected override void ProcessQuery(HttpContext context, String query, List<String> userDefaultGraphs, List<String> userNamedGraphs, long timeout, bool partialResults)
        {
            try {

                //Build the Query Uri
                StringBuilder queryUri = new StringBuilder();
                queryUri.Append(this._config.EndpointURI.ToString());
                if (queryUri.ToString().Contains("?"))
                {
                    queryUri.Append("&query=");
                }
                else
                {
                    queryUri.Append("?query=");
                }
                queryUri.Append(Uri.EscapeDataString(query));

                //Add the Default Graph URIs
                if (userDefaultGraphs.Count > 0)
                {
                    foreach (String userDefaultGraph in userDefaultGraphs)
                    {
                        if (!userDefaultGraph.Equals(String.Empty))
                        {
                            queryUri.Append("&default-graph-uri=");
                            queryUri.Append(Uri.EscapeDataString(userDefaultGraph));
                        }
                    }
                } 
                else
                {
                    queryUri.Append("&default-graph-uri=");
                    queryUri.Append(Uri.EscapeDataString(this._config.DefaultGraphURI));
                }

                //Add the Named Graph URIs
                if (userNamedGraphs.Count > 0)
                {
                    foreach (String userNamedGraph in userNamedGraphs)
                    {
                        if (!userNamedGraph.Equals(String.Empty))
                        {
                            queryUri.Append("&named-graph-uri=");
                            queryUri.Append(Uri.EscapeDataString(userNamedGraph));
                        }
                    }
                }

                if (this._config.SupportsTimeout)
                {
                    //Use Sparql Endpoints Timeout
                    queryUri.Append("&" + this._config.TimeoutField + "=");

                    if (timeout > 0)
                    {
                        queryUri.Append(timeout);
                    }
                    else
                    {
                        queryUri.Append(this._config.DefaultTimeout);
                    }
                }

                //Create a HTTP Request
                HttpWebRequest request;
                if (queryUri.Length > 2048)
                {
                    //Long Query Compatability - use POST instead
                    request = (HttpWebRequest)WebRequest.Create(this._config.EndpointURI.ToString());
                    request.Method = "POST";
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                    queryUri = queryUri.Remove(0, this._config.EndpointURI.ToString().Length + 1);
                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(queryUri.ToString());
                    writer.Close();
                }
                else
                {
                    //Normal GET Query
                    request = (HttpWebRequest)WebRequest.Create(queryUri.ToString());
                    request.Method = "GET";
                }
                request.Accept = String.Join(",", context.Request.AcceptTypes);
                
                if (!this._config.SupportsTimeout)
                {
                    //Use HTTP Request Timeout
                    if (timeout > 0)
                    {
                        request.Timeout = (int)timeout;
                    }
                    else
                    {
                        request.Timeout = (int)this._config.DefaultTimeout;
                    }
                }

                //Get the Response and send to Client
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                context.Response.ContentType = response.ContentType;
                context.Response.Clear();
                Tools.StreamCopy(response.GetResponseStream(), context.Response.OutputStream);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", query, ex);
            }
        }

        /// <summary>
        /// Loads the Configuration for the Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
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
                this._config = new RemoteSparqlHandlerConfiguration(context, cacheKey, configPrefix);
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, this._config.CacheDuration, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Retrieve from Cache
                this._config = (RemoteSparqlHandlerConfiguration)context.Cache[cacheKey];
            }

            return this._config;
        }

    }
}

#endif