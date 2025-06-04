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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Class for connecting to an AllegroGraph Store.
/// </summary>
/// <remarks>
/// <para>
/// Connection to AllegroGraph is based on their new HTTP Protocol which is an extension of the <a href="http://www.openrdf.org/doc/sesame2/system/ch08.html">Sesame 2.0 HTTP Protocol</a>.  The specification for the AllegroGraph protocol can be found. <a href="http://www.franz.com/agraph/support/documentation/current/new-http-server.html">here</a>
/// </para>
/// <para>
/// If you wish to use a Store which is part of the Root Catalog on an AllegroGraph 4.x and higher server you can either use the constructor overloads that omit the <strong>catalogID</strong> parameter or pass in null as the value for that parameter.
/// </para>
/// </remarks>
public class AllegroGraphConnector
    : BaseSesameHttpProtocolConnector, IAsyncUpdateableStorage
    , IUpdateableStorage
{
    private string _agraphBase;
    private readonly string _catalog;
     
    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base URI for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="storeID">Store ID.</param>
    public AllegroGraphConnector(string baseUri, string catalogID, string storeID)
        : this(baseUri, catalogID, storeID, (string)null, (string)null) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="storeID">Store ID.</param>
    public AllegroGraphConnector(string baseUri, string storeID)
        : this(baseUri, null, storeID) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    public AllegroGraphConnector(string baseUri, string catalogID, string storeID, string username, string password)
        : base(baseUri, storeID, username, password)
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
#if NETCORE
        this._agraphBase = this._baseUri.Copy();
#else
        _agraphBase = string.Copy(_baseUri);
#endif
        if (catalogID != null)
        {
            _baseUri += "catalogs/" + catalogID + "/";
        }
        _store = storeID;
        _catalog = catalogID;
        _updatePath = string.Empty;

        _server = new AllegroGraphServer(_baseUri, _catalog);
    }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    public AllegroGraphConnector(string baseUri, string storeID, string username, string password)
        : this(baseUri, null, storeID, username, password) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphConnector(string baseUri, string catalogID, string storeID, IWebProxy proxy)
        : this(baseUri, catalogID, storeID, null, null, proxy) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphConnector(string baseUri, string storeID, IWebProxy proxy)
        : this(baseUri, null, storeID, proxy) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphConnector(string baseUri, string catalogID, string storeID, string username, string password, IWebProxy proxy)
        : this(baseUri, catalogID, storeID, username, password)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="storeID">Store ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphConnector(string baseUri, string storeID, string username, string password, IWebProxy proxy)
        : this(baseUri, null, storeID, username, password, proxy) { }

    /// <summary>
    /// Gets the Catalog under which the repository you are connected to is located.
    /// </summary>
    [Description("The Catalog under which the repository is located.  If using the Root Catalog on AllegroGrah 4+ <ROOT> will be displayed.")]
    public string Catalog => _catalog ?? "<ROOT>";

    /// <summary>
    /// Return the string to use as the value of the HTTP Accept header when retrieving SPARQL tabular results.
    /// </summary>
    /// <remarks>Allegrograph servers return XML results with a declared version of XML 1.1 which cannot be
    /// handled by the .NET XML parser. To work around this, we only request JSON SPARQL results.</remarks>
    protected override string SparqlAcceptHeader
    {
        get
        {
            var accept = new HashSet<MediaTypeWithQualityHeaderValue>(
                MimeTypesHelper.SparqlResultsJson.Select(r => new MediaTypeWithQualityHeaderValue(r)));
            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;
            foreach (MediaTypeWithQualityHeaderValue mt in accept)
            {
                header.Add(mt);
            }

            return header.ToString();
        }
    }

    /// <summary>
    /// Makes a SPARQL Update request to the Allegro Graph server.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    public virtual void Update(string sparqlUpdate)
    {
        try
        {
            // Create the Request
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _updatePath, MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

            // Build the Post Data and add to the Request Body
            request.Content = new FormUrlEncodedContent(new []{new KeyValuePair<string, string>("query", sparqlUpdate)});

            // Get the Response and process based on the Content Type
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "updating");
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "updating");
        }
    }

    /// <summary>
    /// Makes a SPARQL Update request to the Allegro Graph server.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Update(string sparqlUpdate, AsyncStorageCallback callback, object state)
    {
        try
        {
            // Create the Request
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _updatePath,
                MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

            // Build the Post Data and add to the Request Body
            request.Content =
                new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", sparqlUpdate)});
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                            sparqlUpdate,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled")
                                : StorageHelper.HandleError(requestTask.Exception, "updating")),
                        state);
                }
                else
                {
                    using HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                sparqlUpdate,
                                StorageHelper.HandleHttpError(response, "updating")),
                            state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate,
                    StorageHelper.HandleError(ex, "updating")), state);
        }
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(string sparqlUpdate, CancellationToken cancellationToken)
    {
        try
        {
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _updatePath,
                MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

            // Build the Post Data and add to the Request Body
            request.Content =
                new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", sparqlUpdate)});
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "updating");
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "updating");
        }
    }

    /// <summary>
    /// Does nothing as AllegroGraph does not require the same query escaping that Sesame does.
    /// </summary>
    /// <param name="query">Query to escape.</param>
    /// <returns></returns>
    protected override string EscapeQuery(string query)
    {
        return query;
    }

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="queryParams">Querystring Parameters.</param>
    /// <returns></returns>
    [Obsolete("This method is obsolete and will be removed in a future release. Use CreateRequest(string, string, HttpMethod, Dictionary<string, string>) instead.")]
    protected override HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> queryParams)
    {
        // Remove JSON Mime Types from supported Accept types
        // This is a compatability issue with Allegro having a weird custom JSON serialisation
        if (accept.Contains("application/json"))
        {
            accept = accept.Replace("application/json,", string.Empty);
            if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
        }
        if (accept.Contains("text/json"))
        {
            accept = accept.Replace("text/json", string.Empty);
            if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
        }
        if (accept.Contains(",;")) accept = accept.Replace(",;", ",");

        return base.CreateRequest(servicePath, accept, method, queryParams);
    }

    /// <inheritdoc />
    protected override HttpRequestMessage CreateRequest(string servicePath, string accept, HttpMethod method, Dictionary<string, string> queryParams)
    {
        // Remove JSON Mime Types from supported Accept types
        // This is a compatibility issue with Allegro having a weird custom JSON serialization
        if (accept.Contains("application/json"))
        {
            accept = accept.Replace("application/json,", string.Empty);
            if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
        }
        if (accept.Contains("text/json"))
        {
            accept = accept.Replace("text/json", string.Empty);
            if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
        }
        if (accept.Contains(",;")) accept = accept.Replace(",;", ",");
        return base.CreateRequest(servicePath, accept, method, queryParams);
    }

    /// <inheritdoc />
    protected override string GetSaveContentType()
    {
        // AllegroGraph rejects application/n-triples as it still expects to get text/plain so have to use that instead
        return "text/plain";
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (_catalog != null)
        {
            return "[AllegroGraph] Store '" + _store + "' in Catalog '" + _catalog + "' on Server '" + _baseUri.Substring(0, _baseUri.IndexOf("catalogs/")) + "'";
        }
        return "[AllegroGraph] Store '" + _store + "' in Root Catalog on Server '" + _baseUri + "'";
    }

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
        INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
        INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
        INode catalog = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCatalog));
        INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));

        context.Graph.Assert(new Triple(manager, rdfType, genericManager));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        if (_catalog != null)
        {
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri.Substring(0, _baseUri.IndexOf("catalogs/")))));
            context.Graph.Assert(new Triple(manager, catalog, context.Graph.CreateLiteralNode(_catalog)));
        }
        else
        {
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
        }
        context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(_store)));
        
        if (HttpClientHandler?.Credentials is NetworkCredential networkCredential)
        {
            INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
            context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(networkCredential.UserName)));
            context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(networkCredential.Password)));
        }

        SerializeStandardConfig(manager, context);
    }
}
