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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A Class for connecting to a remote SPARQL Endpoint and executing Queries against it
    /// </summary>
    public class SparqlRemoteEndpoint : BaseEndpoint, IConfigurationSerializable
    {
        private List<String> _defaultGraphUris = new List<string>();
        private List<String> _namedGraphUris = new List<string>();

        #region Constructors

        /// <summary>
        /// Empty Constructor for use by derived classes
        /// </summary>
        protected SparqlRemoteEndpoint()
        {

        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        public SparqlRemoteEndpoint(Uri endpointUri)
            : base(endpointUri) { }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph Uri
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, String defaultGraphUri)
            : this(endpointUri)
        {
            this._defaultGraphUris.Add(defaultGraphUri);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint Uri using the given Default Graph Uri
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri)
            : this(endpointUri)
        {
            if (defaultGraphUri != null)
            {
                this._defaultGraphUris.Add(defaultGraphUri.ToString());
            }
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, String defaultGraphUri, IEnumerable<String> namedGraphUris)
            : this(endpointUri, defaultGraphUri)
        {
            this._namedGraphUris.AddRange(namedGraphUris);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri, IEnumerable<String> namedGraphUris)
            : this(endpointUri, defaultGraphUri)
        {
            this._namedGraphUris.AddRange(namedGraphUris);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri, IEnumerable<Uri> namedGraphUris)
            : this(endpointUri, defaultGraphUri)
        {
            this._namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.ToString()));
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<String> defaultGraphUris)
            : this(endpointUri)
        {
            this._defaultGraphUris.AddRange(defaultGraphUris);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris)
            : this(endpointUri)
        {
            this._defaultGraphUris.AddRange(defaultGraphUris.Where(u => u != null).Select(u => u.ToString()));
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<String> defaultGraphUris, IEnumerable<String> namedGraphUris)
            : this(endpointUri, defaultGraphUris)
        {
            this._namedGraphUris.AddRange(namedGraphUris);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<String> defaultGraphUris, IEnumerable<Uri> namedGraphUris)
            : this(endpointUri, defaultGraphUris)
        {
            this._namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.ToString()));
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris, IEnumerable<String> namedGraphUris)
            : this(endpointUri, defaultGraphUris)
        {
            this._namedGraphUris.AddRange(namedGraphUris);
        }

        /// <summary>
        /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI
        /// </summary>
        /// <param name="endpointUri">Remote Endpoint URI</param>
        /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint</param>
        /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint</param>
        public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris, IEnumerable<Uri> namedGraphUris)
            : this(endpointUri, defaultGraphUris)
        {
            this._namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.ToString()));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Default Graph URIs for Queries made to the SPARQL Endpoint
        /// </summary>
        public List<String> DefaultGraphs
        {
            get
            {
                return this._defaultGraphUris;
            }
        }

        /// <summary>
        /// Gets the List of Named Graphs used in requests
        /// </summary>
        public List<String> NamedGraphs
        {
            get
            {
                return this._namedGraphUris;
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>A SPARQL Result Set</returns>
        public virtual SparqlResultSet QueryWithResultSet(String sparqlQuery)
        {
            try
            {
                //Make the Query
                HttpWebResponse httpResponse = this.QueryInternal(sparqlQuery, MimeTypesHelper.HttpSparqlAcceptHeader);

                //Ready a ResultSet
                SparqlResultSet results = new SparqlResultSet();

                //Parse into a ResultSet based on Content Type
                String ctype = httpResponse.ContentType;

                if (ctype.Contains(";"))
                {
                    ctype = ctype.Substring(0, ctype.IndexOf(";"));
                }

                if (MimeTypesHelper.Sparql.Contains(ctype))
                {
                    ISparqlResultsReader resultsParser = MimeTypesHelper.GetSparqlParser(ctype);
                    resultsParser.Load(results, new StreamReader(httpResponse.GetResponseStream()));
                    httpResponse.Close();
                }
                else
                {
                    httpResponse.Close();
                    throw new RdfParseException("The SPARQL Endpoint returned unexpected Content Type '" + ctype + "', this error may be due to the given URI not returning a SPARQL Result Set");
                }

                return results;
            }
            catch (WebException webEx)
            {
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
            }
            catch (RdfException)
            {
                //Some problem with the RDF or Parsing thereof
                throw;
            }
        }

        /// <summary>
        /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>RDF Graph</returns>
        public virtual Graph QueryWithResultGraph(String sparqlQuery)
        {
            try
            {
                //Make the Query
                HttpWebResponse httpResponse = this.QueryInternal(sparqlQuery, MimeTypesHelper.HttpAcceptHeader);

                //Set up an Empty Graph ready
                Graph g = new Graph();
                g.BaseUri = this.Uri;

                //Parse into a Graph based on Content Type
                String ctype = httpResponse.ContentType;
                IRdfReader parser = MimeTypesHelper.GetParser(ctype);
                parser.Load(g, new BlockingStreamReader(httpResponse.GetResponseStream()));
                if (g.BaseUri.ToString().Equals(this.Uri.ToString(), StringComparison.Ordinal)) g.BaseUri = httpResponse.ResponseUri;
                httpResponse.Close();
                return g;
            }
            catch (WebException webEx)
            {
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred when trying to make the SPARQL Query", webEx);
            }
            catch (RdfException)
            {
                //Some problem with the RDF or Parsing thereof
                throw;
            }
        }

        /// <summary>
        /// Makes a Query to a SPARQL Endpoint and returns the raw Response
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns></returns>
        public virtual HttpWebResponse QueryRaw(String sparqlQuery)
        {
            try
            {
                //Make the Query
                //HACK: Changed to an accept all for the time being to ensure works OK with DBPedia and Virtuoso
                return this.QueryInternal(sparqlQuery, MimeTypesHelper.Any);
            }
            catch (WebException webEx)
            {
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
            }
        }

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>A Sparql Result Set</returns>
        /// <remarks>Allows for implementation of asynchronous querying</remarks>
        public delegate SparqlResultSet AsyncQueryWithResultSet(String sparqlQuery);

        /// <summary>
        /// Delegate for making a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns>RDF Graph</returns>
        /// <remarks>Allows for implementation of asynchronous querying</remarks>
        public delegate Graph AsyncQueryWithResultGraph(String sparqlQuery);

        /// <summary>
        /// Internal method which builds the Query Uri and executes it via GET/POST as appropriate
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <param name="acceptHeader">Accept Header to use for the request</param>
        /// <returns></returns>
        private HttpWebResponse QueryInternal(String sparqlQuery, String acceptHeader)
        {
            //Patched by Alexander Zapirov to handle situations where the SPARQL Query is very long
            //i.e. would exceed the length limit of the Uri class

            //Build the Query Uri
            StringBuilder queryUri = new StringBuilder();
            queryUri.Append(this.Uri.ToString());
            bool longQuery = true;
            if (!this.HttpMode.Equals("POST") && sparqlQuery.Length <= (32766 - queryUri.Length))
            {
                longQuery = false;
                try
                {
                    if (this.Uri.ToString().Contains("?"))
                    {
                        queryUri.Append("&query=");
                    }
                    else
                    {
                        queryUri.Append("?query=");
                    }
                    //queryUri.Append(Uri.EscapeDataString(sparqlQuery));
                    queryUri.Append(HttpUtility.UrlEncode(sparqlQuery));

                    //Add the Default Graph URIs
                    foreach (String defaultGraph in this._defaultGraphUris)
                    {
                        if (!defaultGraph.Equals(String.Empty))
                        {
                            queryUri.Append("&default-graph-uri=");
                            queryUri.Append(Uri.EscapeDataString(defaultGraph));
                        }
                    }
                    //Add the Named Graph URIs
                    foreach (String namedGraph in this._namedGraphUris)
                    {
                        if (!namedGraph.Equals(String.Empty))
                        {
                            queryUri.Append("&named-graph-uri=");
                            queryUri.Append(Uri.EscapeDataString(namedGraph));
                        }
                    }
                }
                catch (UriFormatException)
                {
                    longQuery = true;
                }
            }

            //Make the Query via HTTP
            HttpWebResponse httpResponse;
            if (longQuery || queryUri.Length > 2048 || this.HttpMode == "POST")
            {
                //Long Uri/HTTP POST Mode so use POST
                StringBuilder postData = new StringBuilder();
                postData.Append("query=");
                postData.Append(HttpUtility.UrlEncode(sparqlQuery));

                //Add the Default Graph URI(s)
                foreach (String defaultGraph in this._defaultGraphUris)
                {
                    if (!defaultGraph.Equals(String.Empty))
                    {
                        queryUri.Append("&default-graph-uri=");
                        queryUri.Append(Uri.EscapeDataString(defaultGraph));
                    }
                }
                //Add the Named Graph URI(s)
                foreach (String namedGraph in this._namedGraphUris)
                {
                    if (!namedGraph.Equals(String.Empty))
                    {
                        queryUri.Append("&named-graph-uri=");
                        queryUri.Append(Uri.EscapeDataString(namedGraph));
                    }
                }

                httpResponse = this.ExecuteQuery(this.Uri, postData.ToString(), acceptHeader);
            }
            else
            {
                //Make the query normally via GET
                httpResponse = this.ExecuteQuery(new Uri(queryUri.ToString()), String.Empty, acceptHeader);
            }

            return httpResponse;
        }

        /// <summary>
        /// Internal Helper Method which executes the HTTP Requests against the Sparql Endpoint
        /// </summary>
        /// <param name="target">Uri to make Request to</param>
        /// <param name="postData">Data that is to be POSTed to the Endpoint in <strong>application/x-www-form-urlencoded</strong> format</param>
        /// <param name="accept">The Accept Header that should be used</param>
        /// <returns>HTTP Response</returns>
        private HttpWebResponse ExecuteQuery(Uri target, String postData, String accept)
        {
            //Expect errors in this function to be handled by the calling function

            //Set-up the Request
            HttpWebRequest httpRequest;
            HttpWebResponse httpResponse;
            httpRequest = (HttpWebRequest)WebRequest.Create(target);

            //Use HTTP GET/POST according to user set preference
            httpRequest.Accept = accept;
            if (!postData.Equals(String.Empty))
            {
                httpRequest.Method = "POST";
                httpRequest.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream());
                writer.Write(postData);
                writer.Close();
            }
            else
            {
                httpRequest.Method = this.HttpMode;
            }
#if !SILVERLIGHT
            if (this.Timeout > 0) httpRequest.Timeout = this.Timeout;
#endif

            //Apply Credentials to request if necessary
            if (this.Credentials != null)
            {
                httpRequest.Credentials = this.Credentials;
            }

#if !NO_PROXY
            //Use a Proxy if required
            if (this.Proxy != null)
            {
                httpRequest.Proxy = this.Proxy;
                if (this.UseCredentialsForProxy)
                {
                    httpRequest.Proxy.Credentials = this.Credentials;
                }
            }
#endif

            #if DEBUG
            //HTTP Debugging
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(httpRequest);
            }
            #endif

            httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            #if DEBUG
            //HTTP Debugging
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugResponse(httpResponse);
            }
            #endif

            return httpResponse;
        }

        #endregion

        /// <summary>
        /// Serializes the Endpoint's Configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode endpoint = context.NextSubject;
            INode endpointClass = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassSparqlEndpoint);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode endpointUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEndpointUri);
            INode defGraphUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyDefaultGraphUri);
            INode namedGraphUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyNamedGraphUri);

            context.Graph.Assert(new Triple(endpoint, rdfType, endpointClass));
            context.Graph.Assert(new Triple(endpoint, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(endpoint, endpointUri, context.Graph.CreateUriNode(this.Uri)));

            foreach (String u in this._defaultGraphUris)
            {
                context.Graph.Assert(new Triple(endpoint, defGraphUri, context.Graph.CreateUriNode(new Uri(u))));
            }
            foreach (String u in this._namedGraphUris)
            {
                context.Graph.Assert(new Triple(endpoint, namedGraphUri, context.Graph.CreateUriNode(new Uri(u))));
            }

            context.NextSubject = endpoint;
            base.SerializeConfiguration(context);
        }
    }

    /// <summary>
    /// A Class for connecting to multiple remote SPARQL Endpoints and federating queries over them with the data merging done locally
    /// </summary>
    /// <remarks>
    /// <para>
    /// Queries are federated by executing multiple requesting simultaneously and asynchronously against the endpoints in question with the data then merged locally.  The merging process does not attempt to remove duplicate data it just naively merges the data.
    /// </para>
    /// </remarks>
    public class FederatedSparqlRemoteEndpoint : SparqlRemoteEndpoint
    {
        private List<SparqlRemoteEndpoint> _endpoints = new List<SparqlRemoteEndpoint>();
        private bool _ignoreFailedRequests = false;
        private int _maxSimultaneousRequests = 4;

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint using a given Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public FederatedSparqlRemoteEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Add(endpoint);
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint using the given Endpoints
        /// </summary>
        /// <param name="endpoints">Endpoints</param>
        public FederatedSparqlRemoteEndpoint(IEnumerable<SparqlRemoteEndpoint> endpoints)
        {
            this._endpoints.AddRange(endpoints);
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint by creating a new <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public FederatedSparqlRemoteEndpoint(Uri endpointUri)
        {
            this._endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
        }

        /// <summary>
        /// Creates a new Federated SPARQL Endpoint by creating a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> for each of the given URI
        /// </summary>
        /// <param name="endpointUris">Endpoint URIs</param>
        public FederatedSparqlRemoteEndpoint(IEnumerable<Uri> endpointUris)
        {
            this._endpoints.AddRange(endpointUris.Select(u => new SparqlRemoteEndpoint(u)));
        }

        /// <summary>
        /// Adds a additional endpoint to be used by this endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public void AddEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Add(endpoint);
        }

        /// <summary>
        /// Adds an additional endpoint to be used by this endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public void AddEndpoint(Uri endpointUri)
        {
            this._endpoints.Add(new SparqlRemoteEndpoint(endpointUri));
        }

        /// <summary>
        /// Removes a given endpoint from this endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public void RemoveEndpoint(SparqlRemoteEndpoint endpoint)
        {
            this._endpoints.Remove(endpoint);
        }

        /// <summary>
        /// Removes all endpoints with the given URI from this endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public void RemoveEndpoint(Uri endpointUri)
        {
            this._endpoints.RemoveAll(e => e.Uri.Equals(endpointUri));
        }

        /// <summary>
        /// Gets/Sets whether a failed request on one endpoint should cause the entire request to fail
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default if a request on any of the endpoint fails or times out then the entire request will fail
        /// </para>
        /// </remarks>
        public bool IgnoreFailedRequests
        {
            get
            {
                return this._ignoreFailedRequests;
            }
            set
            {
                this._ignoreFailedRequests = value;
            }
        }

        /// <summary>
        /// Gets/Sets the maximum number of endpoints this endpoint will issue queries to at any one time
        /// </summary>
        public int MaxSimultaneousRequests
        {
            get
            {
                return this._maxSimultaneousRequests;
            }
            set
            {
                if (value >= 1)
                {
                    this._maxSimultaneousRequests = value;
                }
            }
        }

        /// <summary>
        /// Makes a Query to a Sparql Endpoint and returns the raw Response
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if more than one endpoint is in use since for any federated endpoint which used more than one endpoint there is no logical/sensible way to combine the result streams</exception>
        public override HttpWebResponse QueryRaw(string sparqlQuery)
        {
            //If we only have a single endpoint then we can still return a raw response
            if (this._endpoints.Count == 1) return this._endpoints[0].QueryRaw(sparqlQuery);

            //If we have any other number of endpoints we either have no responses or no way of logically/sensibly combining responses
            throw new NotSupportedException("Raw Query is not supported by the Federated Remote Endpoint");
        }

        /// <summary>
        /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns>RDF Graph</returns>
        /// <remarks>
        /// <para>
        /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
        /// </para>
        /// <para>
        /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail</exception>
        /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout</exception>
        public override Graph QueryWithResultGraph(string sparqlQuery)
        {
            //If no endpoints return an Empty Graph
            if (this._endpoints.Count == 0) return new Graph();

            //Fire off all the Asychronous Requests
            List<AsyncQueryWithResultGraph> asyncCalls = new List<AsyncQueryWithResultGraph>();
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            int count = 0;
            foreach (SparqlRemoteEndpoint endpoint in this._endpoints)
            {
                //Limit the number of simultaneous requests we make to the user defined level (default 4)
                //We do this limiting check before trying to issue a request so that when the last request
                //is issued we'll always drop out of the loop and move onto our WaitAll()
                while (count >= this._maxSimultaneousRequests)
                {
                    //First check that the count of active requests is accurate
                    int active = asyncResults.Count(r => !r.IsCompleted);
                    if (active < count)
                    {
                        //Some of the requests have already completed so we don't need to wait
                        count = active;
                        break;
                    }
                    else if (active > count)
                    {
                        //There are more active requests then we thought
                        count = active;
                    }

                    //While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                    //Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                    //main loop and fire off our next request
                    WaitHandle.WaitAny(asyncResults.Select(r => r.AsyncWaitHandle).ToArray());
                    count--;
                }

                //Make an asynchronous query to the next endpoint
                AsyncQueryWithResultGraph d = new AsyncQueryWithResultGraph(endpoint.QueryWithResultGraph);
                asyncCalls.Add(d);
                IAsyncResult asyncResult = d.BeginInvoke(sparqlQuery, null, null);
                asyncResults.Add(asyncResult);
                count++;
            }

            //Wait for all our requests to finish
            int waitTimeout = (base.Timeout > 0) ? base.Timeout : System.Threading.Timeout.Infinite;
            WaitHandle.WaitAll(asyncResults.Select(r => r.AsyncWaitHandle).ToArray(), waitTimeout);

            //Check for and handle timeouts
            if (!this._ignoreFailedRequests && !asyncResults.All(r => r.IsCompleted))
            {
                for (int i = 0; i < asyncCalls.Count; i++)
                {
                    try
                    {
                        asyncCalls[i].EndInvoke(asyncResults[i]);
                    }
                    catch
                    {
                        //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                    }
                }
                throw new RdfQueryTimeoutException("Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " + (base.Timeout / 1000) + " seconds");
            }

            //Now merge all the results together
            HashSet<String> varsSeen = new HashSet<string>();
            Graph merged = new Graph();
            for (int i = 0; i < asyncCalls.Count; i++)
            {
                //Retrieve the result for this call
                AsyncQueryWithResultGraph call = asyncCalls[i];
                Graph g;
                try
                {
                    g = call.EndInvoke(asyncResults[i]);
                }
                catch (Exception ex)
                {
                    if (!this._ignoreFailedRequests)
                    {
                        //Clean up in the event of an error
                        for (int j = i + 1; j < asyncCalls.Count; j++)
                        {
                            try
                            {
                                asyncCalls[j].EndInvoke(asyncResults[j]);
                            }
                            catch
                            {
                                //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                            }
                        }

                        //If a single request fails then the entire query fails
                        throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + this._endpoints[i] + "' failing", ex);
                    }
                    else
                    {
                        //If we're ignoring failed requests we continue here
                        continue;
                    }
                }

                //Merge the result into the final results
                merged.Merge(g);
            }

            return merged;
        }

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns>A Sparql Result Set</returns>
        /// <remarks>
        /// <para>
        /// The query is executed by sending it federating it to all the endpoints this endpoint contains using simultaneous asychronous calls.  Once these calls complete the results are naivley merged together (no duplicate data removal) and returned as a single result.
        /// </para>
        /// <para>
        /// By default if any of the endpoints used return an error then the entire query will fail and an exception will be thrown, this behaviour can be overridden by setting the <see cref="FederatedSparqlRemoteEndpoint.IgnoreFailedRequests">IgnoreFailedRequests</see> property to be true in which case the result will be the merge of the results from all endpoints which successfully provided a result.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if any of the requests to the endpoints fail</exception>
        /// <exception cref="RdfQueryTimeoutException">Thrown if not all the requests complete within the set timeout</exception>
        public override SparqlResultSet QueryWithResultSet(string sparqlQuery)
        {
            //If no endpoints return an empty Result Set
            if (this._endpoints.Count == 0) return new SparqlResultSet();

            //Fire off all the Asychronous Requests
            List<AsyncQueryWithResultSet> asyncCalls = new List<AsyncQueryWithResultSet>();
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            int count = 0;
            foreach (SparqlRemoteEndpoint endpoint in this._endpoints)
            {
                //Limit the number of simultaneous requests we make to the user defined level (default 4)
                //We do this limiting check before trying to issue a request so that when the last request
                //is issued we'll always drop out of the loop and move onto our WaitAll()
                while (count >= this._maxSimultaneousRequests)
                {
                    //First check that the count of active requests is accurate
                    int active = asyncResults.Count(r => !r.IsCompleted);
                    if (active < count)
                    {
                        //Some of the requests have already completed so we don't need to wait
                        count = active;
                        break;
                    }
                    else if (active > count)
                    {
                        //There are more active requests then we thought
                        count = active;
                    }

                    //While the number of requests is at/above the maximum we'll wait for any of the requests to finish
                    //Then we can decrement the count and if this drops back below our maximum then we'll go back into the
                    //main loop and fire off our next request
                    WaitHandle.WaitAny(asyncResults.Select(r => r.AsyncWaitHandle).ToArray());
                    count--;
                }

                //Make an asynchronous query to the next endpoint
                AsyncQueryWithResultSet d = new AsyncQueryWithResultSet(endpoint.QueryWithResultSet);
                asyncCalls.Add(d);
                IAsyncResult asyncResult = d.BeginInvoke(sparqlQuery, null, null);
                asyncResults.Add(asyncResult);
                count++;
            }

            //Wait for all our requests to finish
            int waitTimeout = (base.Timeout > 0) ? base.Timeout : System.Threading.Timeout.Infinite;
            WaitHandle.WaitAll(asyncResults.Select(r => r.AsyncWaitHandle).ToArray(), waitTimeout);

            //Check for and handle timeouts
            if (!this._ignoreFailedRequests && !asyncResults.All(r => r.IsCompleted))
            {
                for (int i = 0; i < asyncCalls.Count; i++)
                {
                    try
                    {
                        asyncCalls[i].EndInvoke(asyncResults[i]);
                    }
                    catch
                    {
                        //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                    }
                }
                throw new RdfQueryTimeoutException("Federated Querying failed due to one/more endpoints failing to return results within the Timeout specified which is currently " + (base.Timeout / 1000) + " seconds");
            }

            //Now merge all the results together
            HashSet<String> varsSeen = new HashSet<string>();
            SparqlResultSet mergedResult = new SparqlResultSet();
            mergedResult.SetEmpty(false);
            for (int i = 0; i < asyncCalls.Count; i++)
            {
                //Retrieve the result for this call
                AsyncQueryWithResultSet call = asyncCalls[i];
                SparqlResultSet partialResult;
                try
                {
                    partialResult = call.EndInvoke(asyncResults[i]);
                }
                catch (Exception ex)
                {
                    if (!this._ignoreFailedRequests)
                    {
                        //Clean up in the event of an error
                        for (int j = i + 1; j < asyncCalls.Count; j++)
                        {
                            try
                            {
                                asyncCalls[j].EndInvoke(asyncResults[j]);
                            }
                            catch
                            {
                                //Exceptions don't matter as we're just ensuring all the EndInvoke() calls are made
                            }
                        }

                        //If a single request fails then the entire query fails
                        throw new RdfQueryException("Federated Querying failed due to the query against the endpoint '" + this._endpoints[i].Uri.ToString() + "' failing", ex);
                    }
                    else
                    {
                        //If we're ignoring failed requests we continue here
                        continue;
                    }
                }

                //Merge the result into the final results
                foreach (String variable in partialResult.Variables)
                {
                    if (!varsSeen.Contains(variable))
                    {
                        mergedResult.AddVariable(variable);
                        varsSeen.Add(variable);
                    }
                }

                foreach (SparqlResult result in partialResult.Results)
                {
                    mergedResult.AddResult(result);
                }
            }

            return mergedResult;
        }

        /// <summary>
        /// Serializes the Endpoint's Configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode endpointObj = context.NextSubject;
            INode endpointClass = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassSparqlEndpoint);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode endpoint = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEndpoint);

            context.Graph.Assert(new Triple(endpointObj, rdfType, endpointClass));
            context.Graph.Assert(new Triple(endpointObj, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            foreach (SparqlRemoteEndpoint ep in this._endpoints)
            {
                //Serialize the child endpoint configuration
                INode epObj = context.Graph.CreateBlankNode();
                context.NextSubject = epObj;
                ep.SerializeConfiguration(context);

                //Link that serialization to this serialization
                context.Graph.Assert(new Triple(endpointObj, endpoint, epObj));
            }
        }
    }
}
