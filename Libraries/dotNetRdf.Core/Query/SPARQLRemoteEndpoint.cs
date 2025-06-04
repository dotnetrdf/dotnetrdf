/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Query;

/// <summary>
/// A Class for connecting to a remote SPARQL Endpoint and executing Queries against it.
/// </summary>
[Obsolete("This class is obsolete and will be removed in a future release. Replaced by VDS.RDF.Query.SparqlQueryClient.")]
public class SparqlRemoteEndpoint 
    : BaseEndpoint
{
    private readonly List<string> _defaultGraphUris = new List<string>();
    private readonly List<string> _namedGraphUris = new List<string>();
    private string _resultsAccept, _rdfAccept;

    const int LongQueryLength = 2048;

    #region Constructors

    /// <summary>
    /// Empty Constructor for use by derived classes.
    /// </summary>
    protected SparqlRemoteEndpoint()
    {

    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    public SparqlRemoteEndpoint(Uri endpointUri)
        : base(endpointUri) { }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph Uri.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, string defaultGraphUri)
        : this(endpointUri)
    {
        _defaultGraphUris.Add(defaultGraphUri);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint Uri using the given Default Graph Uri.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri)
        : this(endpointUri)
    {
        if (defaultGraphUri != null)
        {
            _defaultGraphUris.Add(defaultGraphUri.AbsoluteUri);
        }
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, string defaultGraphUri, IEnumerable<string> namedGraphUris)
        : this(endpointUri, defaultGraphUri)
    {
        _namedGraphUris.AddRange(namedGraphUris);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri, IEnumerable<string> namedGraphUris)
        : this(endpointUri, defaultGraphUri)
    {
        _namedGraphUris.AddRange(namedGraphUris);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUri">Default Graph URI to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, Uri defaultGraphUri, IEnumerable<Uri> namedGraphUris)
        : this(endpointUri, defaultGraphUri)
    {
        _namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.AbsoluteUri));
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<string> defaultGraphUris)
        : this(endpointUri)
    {
        _defaultGraphUris.AddRange(defaultGraphUris);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris)
        : this(endpointUri)
    {
        _defaultGraphUris.AddRange(defaultGraphUris.Where(u => u != null).Select(u => u.AbsoluteUri));
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<string> defaultGraphUris, IEnumerable<string> namedGraphUris)
        : this(endpointUri, defaultGraphUris)
    {
        _namedGraphUris.AddRange(namedGraphUris);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<string> defaultGraphUris, IEnumerable<Uri> namedGraphUris)
        : this(endpointUri, defaultGraphUris)
    {
        _namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.AbsoluteUri));
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris, IEnumerable<string> namedGraphUris)
        : this(endpointUri, defaultGraphUris)
    {
        _namedGraphUris.AddRange(namedGraphUris);
    }

    /// <summary>
    /// Creates a new SPARQL Endpoint for the given Endpoint URI using the given Default Graph URI.
    /// </summary>
    /// <param name="endpointUri">Remote Endpoint URI.</param>
    /// <param name="defaultGraphUris">Default Graph URIs to use when Querying the Endpoint.</param>
    /// <param name="namedGraphUris">Named Graph URIs to use when Querying the Endpoint.</param>
    public SparqlRemoteEndpoint(Uri endpointUri, IEnumerable<Uri> defaultGraphUris, IEnumerable<Uri> namedGraphUris)
        : this(endpointUri, defaultGraphUris)
    {
        _namedGraphUris.AddRange(namedGraphUris.Where(u => u != null).Select(u => u.AbsoluteUri));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the Default Graph URIs for Queries made to the SPARQL Endpoint.
    /// </summary>
    public List<string> DefaultGraphs
    {
        get
        {
            return _defaultGraphUris;
        }
    }

    /// <summary>
    /// Gets the List of Named Graphs used in requests.
    /// </summary>
    public List<string> NamedGraphs
    {
        get
        {
            return _namedGraphUris;
        }
    }

    /// <summary>
    /// Gets/Sets the Accept Header sent with ASK/SELECT queries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be used to workaround buggy endpoints which don't like the broad Accept Header that dotNetRDF sends by default.  If not set or explicitly set to null the library uses the default header generated by <see cref="MimeTypesHelper.HttpSparqlAcceptHeader"/>.
    /// </para>
    /// </remarks>
    public string ResultsAcceptHeader
    {
        get
        {
            return (_resultsAccept != null ? _resultsAccept : MimeTypesHelper.HttpSparqlAcceptHeader);
        }
        set
        {
            _resultsAccept = value;
        }
    }

    /// <summary>
    /// Gets/Sets the Accept Header sent with CONSTRUCT/DESCRIBE queries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be used to workaround buggy endpoints which don't like the broad Accept Header that dotNetRDF sends by default.  If not set or explicitly set to null the library uses the default header generated by <see cref="MimeTypesHelper.HttpAcceptHeader"/>.
    /// </para>
    /// </remarks>
    public string RdfAcceptHeader
    {
        get
        {
            return (_rdfAccept != null ? _rdfAccept : MimeTypesHelper.HttpAcceptHeader);
        }
        set
        {
            _rdfAccept = value;
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Makes a Query where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <returns>A SPARQL Result Set.</returns>
    public virtual SparqlResultSet QueryWithResultSet(string sparqlQuery)
    {
        // Ready a ResultSet then invoke the other overload
        var results = new SparqlResultSet();
        QueryWithResultSet(new ResultSetHandler(results), sparqlQuery);
        return results;
    }

    /// <summary>
    /// Makes a Query where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries.
    /// </summary>
    /// <param name="handler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    public virtual void QueryWithResultSet(ISparqlResultsHandler handler, string sparqlQuery)
    {
        try
        {
            // Make the Query
            HttpWebResponse httpResponse = QueryInternal(sparqlQuery, ResultsAcceptHeader);

            // Parse into a ResultSet based on Content Type
            var ctype = httpResponse.ContentType;

            if (ctype.Contains(";"))
            {
                ctype = ctype.Substring(0, ctype.IndexOf(";"));
            }

            ISparqlResultsReader resultsParser = MimeTypesHelper.GetSparqlParser(ctype);
            resultsParser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
            httpResponse.Close();
        }
        catch (WebException webEx)
        {
            if (webEx.Response != null)
            {
            }

            // Some sort of HTTP Error occurred
            throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query, see inner exception for details", webEx);
        }
        catch (RdfException)
        {
            // Some problem with the RDF or Parsing thereof
            throw;
        }
    }

    /// <summary>
    /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <returns>RDF Graph.</returns>
    public virtual IGraph QueryWithResultGraph(string sparqlQuery)
    {
        // Set up an Empty Graph then invoke the other overload
        var g = new Graph();
        g.BaseUri = Uri;
        QueryWithResultGraph(new GraphHandler(g), sparqlQuery);
        return g;
    }

    /// <summary>
    /// Makes a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    public virtual void QueryWithResultGraph(IRdfHandler handler, string sparqlQuery)
    {
        try
        {
            // Make the Query
            using (HttpWebResponse httpResponse = QueryInternal(sparqlQuery, RdfAcceptHeader))
            {
                // Parse into a Graph based on Content Type
                var ctype = httpResponse.ContentType;
                IRdfReader parser = MimeTypesHelper.GetParser(ctype);
                parser.Load(handler, new StreamReader(httpResponse.GetResponseStream()));
                httpResponse.Close();
            }
        }
        catch (WebException webEx)
        {
            if (webEx.Response != null)
            {
            }

            // Some sort of HTTP Error occurred
            throw new RdfQueryException("A HTTP Error occurred when trying to make the SPARQL Query, see inner exception for details", webEx);
        }
        catch (RdfException)
        {
            // Some problem with the RDF or Parsing thereof
            throw;
        }
    }

    /// <summary>
    /// Makes a Query to a SPARQL Endpoint and returns the raw Response.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <returns></returns>
    public virtual HttpWebResponse QueryRaw(string sparqlQuery)
    {
        try
        {
            // Make the Query
            // HACK: Changed to an accept all for the time being to ensure works OK with DBPedia and Virtuoso
            return QueryInternal(sparqlQuery, MimeTypesHelper.Any);
        }
        catch (WebException webEx)
        {
            if (webEx.Response != null)
            {
            }

            // Some sort of HTTP Error occurred
            throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
        }
    }

    /// <summary>
    /// Makes a Query to a SPARQL Endpoint and returns the raw Response.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <param name="mimeTypes">MIME Types to use for the Accept Header.</param>
    /// <returns></returns>
    public virtual HttpWebResponse QueryRaw(string sparqlQuery, string[] mimeTypes)
    {
        try
        {
            // Make the Query
            return QueryInternal(sparqlQuery, MimeTypesHelper.CustomHttpAcceptHeader(mimeTypes));
        }
        catch (WebException webEx)
        {
            if (webEx.Response != null)
            {
            }

            // Some sort of HTTP Error occurred
            throw new RdfQueryException("A HTTP Error occurred while trying to make the SPARQL Query", webEx);
        }
    }

    /// <summary>
    /// Makes a Query where the expected Result is a SparqlResultSet ie. SELECT and ASK Queries.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query String.</param>
    /// <returns>A Sparql Result Set.</returns>
    /// <remarks>
    /// <para>
    /// Allows for implementation of asynchronous querying.  Note that the overloads of QueryWithResultSet() and QueryWithResultGraph() that take callbacks are already implemented asynchronously so you may wish to use those instead if you don't need to explicitly invoke and wait on an async operation.
    /// </para>
    /// </remarks>
    public delegate SparqlResultSet AsyncQueryWithResultSet(string sparqlQuery);

    /// <summary>
    /// Delegate for making a Query where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query String.</param>
    /// <returns>RDF Graph.</returns>
    /// <remarks>Allows for implementation of asynchronous querying.</remarks>
    /// <remarks>
    /// <para>
    /// Allows for implementation of asynchronous querying.  Note that the overloads of QueryWithResultSet() and QueryWithResultGraph() that take callbacks are already implemented asynchronously so you may wish to use those instead if you don't need to explicitly invoke and wait on an async operation.
    /// </para>
    /// </remarks>
    public delegate IGraph AsyncQueryWithResultGraph(string sparqlQuery);

    /// <summary>
    /// Internal method which builds the Query Uri and executes it via GET/POST as appropriate.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query.</param>
    /// <param name="acceptHeader">Accept Header to use for the request.</param>
    /// <returns></returns>
    private HttpWebResponse QueryInternal(string sparqlQuery, string acceptHeader)
    {
        // Patched by Alexander Zapirov to handle situations where the SPARQL Query is very long
        // i.e. would exceed the length limit of the Uri class

        // Build the Query Uri
        var queryUri = new StringBuilder();
        queryUri.Append(Uri.AbsoluteUri);
        var longQuery = true;
        if (HttpMode.Equals("GET") || 
            HttpMode.Equals("AUTO") && sparqlQuery.Length <= LongQueryLength && sparqlQuery.IsAscii())
        {
            longQuery = false;
            try
            {
                queryUri.Append(!Uri.Query.Equals(string.Empty) ? "&query=" : "?query=");
                queryUri.Append(HttpUtility.UrlEncode(sparqlQuery));

                // Add the Default Graph URIs
                foreach (var defaultGraph in _defaultGraphUris)
                {
                    if (defaultGraph.Equals(string.Empty)) continue;
                    queryUri.Append("&default-graph-uri=");
                    queryUri.Append(HttpUtility.UrlEncode(defaultGraph));
                }
                // Add the Named Graph URIs
                foreach (var namedGraph in _namedGraphUris)
                {
                    if (namedGraph.Equals(string.Empty)) continue;
                    queryUri.Append("&named-graph-uri=");
                    queryUri.Append(HttpUtility.UrlEncode(namedGraph));
                }
            }
            catch (UriFormatException)
            {
                if (HttpMode.Equals("GET")) throw;
                longQuery = true;
            }
        }

        // Make the Query via HTTP
        HttpWebResponse httpResponse;
        if ( HttpMode.Equals("AUTO") && (longQuery || queryUri.Length > 2048) || HttpMode == "POST")
        {
            // Long Uri/HTTP POST Mode so use POST
            var postData = new StringBuilder();
            postData.Append("query=");
            postData.Append(HttpUtility.UrlEncode(sparqlQuery));

            // Add the Default Graph URI(s)
            foreach (var defaultGraph in _defaultGraphUris)
            {
                if (defaultGraph.Equals(string.Empty)) continue;
                postData.Append("&default-graph-uri=");
                postData.Append(HttpUtility.UrlEncode(defaultGraph));
            }
            // Add the Named Graph URI(s)
            foreach (var namedGraph in _namedGraphUris)
            {
                if (namedGraph.Equals(string.Empty)) continue;
                postData.Append("&named-graph-uri=");
                postData.Append(HttpUtility.UrlEncode(namedGraph));
            }

            httpResponse = ExecuteQuery(Uri, postData.ToString(), acceptHeader);
        }
        else
        {
            // Make the query normally via GET
            httpResponse = ExecuteQuery(new Uri(queryUri.ToString()), string.Empty, acceptHeader);
        }

        return httpResponse;
    }

    /// <summary>
    /// Internal Helper Method which executes the HTTP Requests against the Sparql Endpoint.
    /// </summary>
    /// <param name="target">Uri to make Request to.</param>
    /// <param name="postData">Data that is to be POSTed to the Endpoint in <strong>application/x-www-form-urlencoded</strong> format.</param>
    /// <param name="accept">The Accept Header that should be used.</param>
    /// <returns>HTTP Response.</returns>
    private HttpWebResponse ExecuteQuery(Uri target, string postData, string accept)
    {
        // Expect errors in this function to be handled by the calling function

        // Set-up the Request
        var httpRequest = (HttpWebRequest)WebRequest.Create(target);

        // Use HTTP GET/POST according to user set preference
        httpRequest.Accept = accept;
        if (!postData.Equals(string.Empty))
        {
            httpRequest.Method = "POST";
            httpRequest.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
            using (var writer = new StreamWriter(httpRequest.GetRequestStream(), new UTF8Encoding(false)))
            {
                writer.Write(postData);
                writer.Close();
            }
        }
        else
        {
            if (HttpMode.Equals("AUTO"))
            {
                httpRequest.Method = postData.Equals(string.Empty) ? "GET" : "POST";
            }
            else
            {
                httpRequest.Method = HttpMode;
            }
        }
        ApplyRequestOptions(httpRequest);

        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

        return httpResponse;
    }

    /// <summary>
    /// Makes a Query asynchronously where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries.
    /// </summary>
    /// <param name="query">SPARQL Query String.</param>
    /// <param name="callback">Callback to invoke when the query completes.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void QueryWithResultSet(string query, SparqlResultsCallback callback, object state)
    {
        var request = (HttpWebRequest)WebRequest.Create(Uri);
        request.Method = "POST";
        request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
        request.Accept = ResultsAcceptHeader;
        ApplyRequestOptions(request);

        try
        {
            request.BeginGetRequestStream(result =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(query));

                            foreach (var u in DefaultGraphs)
                            {
                                writer.Write("&default-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }
                            foreach (var u in NamedGraphs)
                            {
                                writer.Write("&named-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }

                            writer.Close();
                        }

                        request.BeginGetResponse(innerResult =>
                            {
                                try
                                {
                                    using (var response = (HttpWebResponse) request.EndGetResponse(innerResult))
                                    {
                                        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType, false);
                                        var rset = new SparqlResultSet();
                                        parser.Load(rset, new StreamReader(response.GetResponseStream()));

                                        response.Close();
                                        callback(rset, state);
                                    }
                                }
                                catch (SecurityException secEx)
                                {
                                    callback(null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                                }
                                catch (WebException webEx)
                                {
                                    if (webEx.Response != null)
                                    {
                                    }

                                    callback(null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                                }
                                catch (Exception ex)
                                {
                                    callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                                }
                            }, null);
                    }
                    catch (SecurityException secEx)
                    {
                        callback(null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
                        }

                        callback(null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                    }
                    catch (Exception ex)
                    {
                        callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                    }
                }, null);
        }
        catch (Exception ex)
        {
            callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
        }
    }

    /// <summary>
    /// Makes a Query asynchronously where the expected Result is a <see cref="SparqlResultSet">SparqlResultSet</see> i.e. SELECT and ASK Queries.
    /// </summary>
    /// <param name="query">SPARQL Query String.</param>
    /// <param name="handler">Results Handler.</param>
    /// <param name="callback">Callback to invoke when the query completes.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void QueryWithResultSet(ISparqlResultsHandler handler, string query, QueryCallback callback, object state)
    {
        var request = (HttpWebRequest)WebRequest.Create(Uri);
        request.Method = "POST";
        request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
        request.Accept = RdfAcceptHeader;
        ApplyRequestOptions(request);

        try
        {
            request.BeginGetRequestStream(result =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(query));

                            foreach (var u in DefaultGraphs)
                            {
                                writer.Write("&default-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }
                            foreach (var u in NamedGraphs)
                            {
                                writer.Write("&named-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }

                            writer.Close();
                        }

                        request.BeginGetResponse(innerResult =>
                            {
                                try
                                {
                                    using (var response = (HttpWebResponse) request.EndGetResponse(innerResult))
                                    {
                                        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType, false);
                                        parser.Load(handler, new StreamReader(response.GetResponseStream()));

                                        response.Close();
                                        callback(null, handler, state);
                                    }
                                }
                                catch (SecurityException secEx)
                                {
                                    callback(null, handler, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                                }
                                catch (WebException webEx)
                                {
                                    if (webEx.Response != null)
                                    {
                                    }

                                    callback(null, handler, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                                }
                                catch (Exception ex)
                                {
                                    callback(null, handler, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                                }
                            }, null);
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
                        }

                        callback(null, handler, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                    }
                    catch (Exception ex)
                    {
                        callback(null, handler, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                    }
                }, null);
        }
        catch (Exception ex)
        {
            callback(null, handler, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
        }
    }

    /// <summary>
    /// Makes a Query asynchronously where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="query">SPARQL Query String.</param>
    /// <param name="callback">Callback to invoke when the query completes.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void QueryWithResultGraph(string query, GraphCallback callback, object state)
    {
        var request = (HttpWebRequest)WebRequest.Create(Uri);
        request.Method = "POST";
        request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
        request.Accept = RdfAcceptHeader;
        ApplyRequestOptions(request);

        try
        {
            request.BeginGetRequestStream(result =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(result);
                        using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(query));

                            foreach (var u in DefaultGraphs)
                            {
                                writer.Write("&default-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }
                            foreach (var u in NamedGraphs)
                            {
                                writer.Write("&named-graph-uri=");
                                writer.Write(HttpUtility.UrlEncode(u));
                            }

                            writer.Close();
                        }

                        request.BeginGetResponse(innerResult =>
                            {
                                try
                                {

                                    var response = (HttpWebResponse) request.EndGetResponse(innerResult);
                                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                                    var g = new Graph();
                                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                                    callback(g, state);
                                }
                                catch (SecurityException secEx)
                                {
                                    callback(null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                                }
                                catch (WebException webEx)
                                {
                                    if (webEx.Response != null)
                                    {
                                    }

                                    callback(null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                                }
                                catch (Exception ex)
                                {
                                    callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                                }
                            }, null);
                    }
                    catch (SecurityException secEx)
                    {
                        callback(null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
                        }

                        callback(null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                    }
                    catch (Exception ex)
                    {
                        callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                    }
                }, null);
        }
        catch (Exception ex)
        {
            callback(null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
        }
    }

    /// <summary>
    /// Makes a Query asynchronously where the expected Result is an RDF Graph ie. CONSTRUCT and DESCRIBE Queries.
    /// </summary>
    /// <param name="query">SPARQL Query String.</param>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="callback">Callback to invoke when the query completes.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void QueryWithResultGraph(IRdfHandler handler, string query, QueryCallback callback, object state)
    {
        var request = (HttpWebRequest)WebRequest.Create(Uri);
        request.Method = "POST";
        request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
        request.Accept = ResultsAcceptHeader;
        ApplyRequestOptions(request);

        request.BeginGetRequestStream(result =>
        {
            try
            {
                Stream stream = request.EndGetRequestStream(result);
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write("query=");
                    writer.Write(HttpUtility.UrlEncode(query));

                    foreach (var u in DefaultGraphs)
                    {
                        writer.Write("&default-graph-uri=");
                        writer.Write(HttpUtility.UrlEncode(u));
                    }
                    foreach (var u in NamedGraphs)
                    {
                        writer.Write("&named-graph-uri=");
                        writer.Write(HttpUtility.UrlEncode(u));
                    }

                    writer.Close();
                }

                request.BeginGetResponse(innerResult =>
                    {
                        try
                        {
                            var response = (HttpWebResponse) request.EndGetResponse(innerResult);
                            IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                            parser.Load(handler, new StreamReader(response.GetResponseStream()));

                            callback(handler, null, state);
                        }
                        catch (SecurityException secEx)
                        {
                            callback(handler, null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
                        }
                        catch (WebException webEx)
                        {
                            if (webEx.Response != null)
                            {
                            }

                            callback(handler, null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
                        }
                        catch (Exception ex)
                        {
                            callback(handler, null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
                        }
                    }, null);
            }
            catch (SecurityException secEx)
            {
                callback(handler, null, new AsyncError(new RdfQueryException("Calling code does not have permission to access the specified remote endpoint, see inner exception for details", secEx), state));
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                }

                callback(handler, null, new AsyncError(new RdfQueryException("A HTTP error occurred while making an asynchronous query, see inner exception for details", webEx), state));
            }
            catch (Exception ex)
            {
                callback(handler, null, new AsyncError(new RdfQueryException("Unexpected error while making an asynchronous query, see inner exception for details", ex), state));
            }
        }, null);
    }

    #endregion

    /// <summary>
    /// Serializes the Endpoint's Configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode endpoint = context.NextSubject;
        INode endpointClass = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassSparqlQueryEndpoint));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode endpointUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri));
        INode defGraphUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri));
        INode namedGraphUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri));

        context.Graph.Assert(new Triple(endpoint, rdfType, endpointClass));
        context.Graph.Assert(new Triple(endpoint, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(endpoint, endpointUri, context.Graph.CreateUriNode(Uri)));

        foreach (var u in _defaultGraphUris)
        {
            context.Graph.Assert(new Triple(endpoint, defGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
        }
        foreach (var u in _namedGraphUris)
        {
            context.Graph.Assert(new Triple(endpoint, namedGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
        }

        context.NextSubject = endpoint;
        base.SerializeConfiguration(context);
    }
}
