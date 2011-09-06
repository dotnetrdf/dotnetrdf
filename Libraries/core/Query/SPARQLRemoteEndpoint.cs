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
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A Class for connecting to a remote SPARQL Endpoint and executing Queries against it
    /// </summary>
    public class SparqlRemoteEndpoint : BaseEndpoint, IConfigurationSerializable
    {
        private List<String> _defaultGraphUris = new List<string>();
        private List<String> _namedGraphUris = new List<string>();

        const int LongQueryLength = 2048;

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

#if !SILVERLIGHT

        /// <summary>
        /// Makes a Query where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>A SPARQL Result Set</returns>
        public virtual SparqlResultSet QueryWithResultSet(String sparqlQuery)
        {
            //Ready a ResultSet then invoke the other overload
            SparqlResultSet results = new SparqlResultSet();
            this.QueryWithResultSet(new ResultSetHandler(results), sparqlQuery);
            return results;
        }

        /// <summary>
        /// Makes a Query where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        public virtual void QueryWithResultSet(ISparqlResultsHandler handler, String sparqlQuery)
        {
            try
            {
                //Make the Query
                HttpWebResponse httpResponse = this.QueryInternal(sparqlQuery, MimeTypesHelper.HttpSparqlAcceptHeader);

                //Parse into a ResultSet based on Content Type
                String ctype = httpResponse.ContentType;

                if (ctype.Contains(";"))
                {
                    ctype = ctype.Substring(0, ctype.IndexOf(";"));
                }

                if (MimeTypesHelper.Sparql.Contains(ctype))
                {
                    ISparqlResultsReader resultsParser = MimeTypesHelper.GetSparqlParser(ctype);
                    resultsParser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                    httpResponse.Close();
                }
                else
                {
                    httpResponse.Close();
                    throw new RdfParseException("The SPARQL Endpoint returned unexpected Content Type '" + ctype + "', this error may be due to the given URI not returning a SPARQL Result Set");
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query, see inner exception for details", webEx);
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
        public virtual IGraph QueryWithResultGraph(String sparqlQuery)
        {
            //Set up an Empty Graph then invoke the other overload
            Graph g = new Graph();
            g.BaseUri = this.Uri;
            this.QueryWithResultGraph(new GraphHandler(g), sparqlQuery);
            return g;
        }

        /// <summary>
        /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        public virtual void QueryWithResultGraph(IRdfHandler handler, String sparqlQuery)
        {
            try
            {
                //Make the Query
                using (HttpWebResponse httpResponse = this.QueryInternal(sparqlQuery, MimeTypesHelper.HttpAcceptHeader))
                {
                    //Parse into a Graph based on Content Type
                    String ctype = httpResponse.ContentType;
                    IRdfReader parser = MimeTypesHelper.GetParser(ctype);
                    parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                    httpResponse.Close();
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred when trying to make the SPARQL Query, see inner exception for details", webEx);
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
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
            }
        }

        /// <summary>
        /// Makes a Query to a SPARQL Endpoint and returns the raw Response
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <param name="mimeTypes">MIME Types to use for the Accept Header</param>
        /// <returns></returns>
        public virtual HttpWebResponse QueryRaw(String sparqlQuery, String[] mimeTypes)
        {
            try
            {
                //Make the Query
                return this.QueryInternal(sparqlQuery, MimeTypesHelper.CustomHttpAcceptHeader(mimeTypes));
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
            }
        }

        /// <summary>
        /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query String</param>
        /// <returns>A Sparql Result Set</returns>
        /// <remarks>
        /// <para>
        /// Allows for implementation of asynchronous querying.  Note that the overloads of QueryWithResultSet() and QueryWithResultGraph() that take callbacks are already implemented asynchronously so you may wish to use those instead if you don't need to explicitly invoke and wait on an async operation.
        /// </para>
        /// </remarks>
        public delegate SparqlResultSet AsyncQueryWithResultSet(String sparqlQuery);

        /// <summary>
        /// Delegate for making a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query String</param>
        /// <returns>RDF Graph</returns>
        /// <remarks>Allows for implementation of asynchronous querying</remarks>
        /// <remarks>
        /// <para>
        /// Allows for implementation of asynchronous querying.  Note that the overloads of QueryWithResultSet() and QueryWithResultGraph() that take callbacks are already implemented asynchronously so you may wish to use those instead if you don't need to explicitly invoke and wait on an async operation.
        /// </para>
        /// </remarks>
        public delegate IGraph AsyncQueryWithResultGraph(String sparqlQuery);

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
            if (!this.HttpMode.Equals("POST") && sparqlQuery.Length <= LongQueryLength)
            {
                longQuery = false;
                try
                {
                    if (!this.Uri.Query.Equals(String.Empty))
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
                using (StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    writer.Write(postData);
                    writer.Close();
                }
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

#endif
        /// <summary>
        /// Makes a Query asynchronously where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries
        /// </summary>
        /// <param name="query">SPARQL Query String</param>
        /// <param name="callback">Callback to invoke when the query completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void QueryWithResultSet(String query, SparqlResultsCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.Method = "POST";
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            request.Accept = MimeTypesHelper.HttpSparqlAcceptHeader;

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetRequestStream(result =>
            {
                Stream stream = request.EndGetRequestStream(result);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write("query=");
                    writer.Write(HttpUtility.UrlEncode(query));

                    foreach (String u in this.DefaultGraphs)
                    {
                        writer.Write("&default-graph-uri=");
                        writer.Write(Uri.EscapeDataString(u));
                    }
                    foreach (String u in this.NamedGraphs)
                    {
                        writer.Write("&named-graph-uri=");
                        writer.Write(Uri.EscapeDataString(u));
                    }

                    writer.Close();
                }

                request.BeginGetResponse(innerResult =>
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(innerResult))
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType, false);
                            SparqlResultSet rset = new SparqlResultSet();
                            parser.Load(rset, new StreamReader(response.GetResponseStream()));

                            response.Close();
                            callback(rset, state);
                        }
                    }, null);
            }, null);

        }

        /// <summary>
        /// Makes a Query asynchronously where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries
        /// </summary>
        /// <param name="query">SPARQL Query String</param>
        /// <param name="callback">Callback to invoke when the query completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void QueryWithResultGraph(String query, GraphCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.Method = "POST";
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetRequestStream(result =>
            {
                Stream stream = request.EndGetRequestStream(result);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write("query=");
                    writer.Write(HttpUtility.UrlEncode(query));

                    foreach (String u in this.DefaultGraphs)
                    {
                        writer.Write("&default-graph-uri=");
                        writer.Write(Uri.EscapeDataString(u));
                    }
                    foreach (String u in this.NamedGraphs)
                    {
                        writer.Write("&named-graph-uri=");
                        writer.Write(Uri.EscapeDataString(u));
                    }

                    writer.Close();
                }

                request.BeginGetResponse(innerResult =>
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(innerResult);
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    callback(g, state);
                }, null);
            }, null);
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
}
