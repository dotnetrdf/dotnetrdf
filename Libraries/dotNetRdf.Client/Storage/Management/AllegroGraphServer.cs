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
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage.Management;

/// <summary>
/// Represents an AllegroGraph server, may be used to access and manage stores within a catalog on the server.
/// </summary>
public class AllegroGraphServer
    : SesameServer
{
    private readonly string _agraphBase;
    private readonly string _catalog;
     
    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base URI for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    public AllegroGraphServer(string baseUri, string catalogID)
        : this(baseUri, catalogID, (string)null, (string)null) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    public AllegroGraphServer(string baseUri)
        : this(baseUri, (string)null) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    public AllegroGraphServer(string baseUri, string catalogID, string username, string password)
        : base(baseUri, username, password)
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
        _catalog = catalogID;
    }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    public AllegroGraphServer(string baseUri, string username, string password)
        : this(baseUri, null, username, password) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphServer(string baseUri, string catalogID, IWebProxy proxy)
        : this(baseUri, catalogID, null, null, proxy) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphServer(string baseUri, IWebProxy proxy)
        : this(baseUri, null, proxy) { }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store.
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="catalogID">Catalog ID.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphServer(string baseUri, string catalogID, string username, string password, IWebProxy proxy)
        : this(baseUri, catalogID, username, password)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher).
    /// </summary>
    /// <param name="baseUri">Base Uri for the Store.</param>
    /// <param name="username">Username for connecting to the Store.</param>
    /// <param name="password">Password for connecting to the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public AllegroGraphServer(string baseUri, string username, string password, IWebProxy proxy)
        : this(baseUri, null, username, password, proxy) { }
    
    /// <summary>
    /// Gets a default template for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public override IStoreTemplate GetDefaultTemplate(string id)
    {
        return new StoreTemplate(id, "AllegroGraph", "An AllgroGraph store");
    }

    /// <summary>
    /// Gets all available templates for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public override IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
    {
        return GetDefaultTemplate(id).AsEnumerable();
    }

    /// <summary>
    /// Creates a new Store (if it doesn't already exist).
    /// </summary>
    /// <param name="template">Template for creating the new Store.</param>
    public override bool CreateStore(IStoreTemplate template)
    {
        try
        {
            var createParams = new Dictionary<string, string> {{"override", "false"}};
            HttpRequestMessage request =
                CreateRequest("repositories/" + template.ID, "*/*", HttpMethod.Put, createParams);

            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
            {
                // A 409 just means that the store already exists
                return true;
            }

            throw StorageHelper.HandleHttpError(response, "creating store in");
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "creating store in");
        }
    }

    /// <summary>
    /// Requests that AllegroGraph deletes a Store.
    /// </summary>
    /// <param name="storeID">Store ID.</param>
    public override void DeleteStore(string storeID)
    {
        try
        {
            HttpRequestMessage request = CreateRequest("repositories/" + storeID, "*/*", HttpMethod.Delete, new Dictionary<string, string>());

            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "delete");
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "delete");
        }
    }

    /// <summary>
    /// Get the lists of stores available on the Server.
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<string> ListStores()
    {
        string data;
        try
        {
            HttpRequestMessage request = CreateRequest("repositories", "application/json", HttpMethod.Get, new Dictionary<string, string>());
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "list Stores from");
            }

            using var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
            data = reader.ReadToEnd();
            reader.Close();
        }
        catch (WebException webEx)
        {
            throw StorageHelper.HandleHttpError(webEx, "list Stores from");
        }

        var json = JArray.Parse(data);
        var stores = new List<string>();
        foreach (JToken token in json.Children())
        {
            if (token["id"] is JValue id && id.Value != null)
            {
                stores.Add(id.Value.ToString());
            }
        }
        return stores;
    }

    /// <summary>
    /// Gets a Store within the current catalog.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    /// <remarks>
    /// AllegroGraph groups stores by catalogue, you may only use this method to obtain stores within your current catalogue.
    /// </remarks>
    public override IStorageProvider GetStore(string storeId)
    {
        // Otherwise return a new instance
        return new AllegroGraphConnector(_agraphBase, _catalog, storeId, _username, _pwd, Proxy);
    }

    /// <summary>
    /// Gets the List of Stores available  on the server within the current catalog asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    [Obsolete("Replaced with ListStoresAsync(CancellationToken).")]
    public override void ListStores(AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = CreateRequest("repositories", "application/json", HttpMethod.Get,
                new Dictionary<string, string>());
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception, "listing stores from")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                StorageHelper.HandleHttpError(response, "listing stores from")),
                            state);
                    }
                    else
                    {
                        response.Content.ReadAsStringAsync().ContinueWith(readTask =>
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled.")
                                            : StorageHelper.HandleError(requestTask.Exception,
                                                "listing stores from")),
                                    state);
                            }
                            else
                            {
                                var json = JArray.Parse(readTask.Result);
                                var stores = new List<string>();
                                foreach (JToken token in json.Children())
                                {
                                    if (token["id"] is JValue id)
                                    {
                                        stores.Add(id.Value.ToString());
                                    }
                                }

                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, stores), state);
                            }
                        });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                    StorageHelper.HandleError(ex, "list Stores from")), state);
        }
    }

    /// <summary>
    /// Gets a default template for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    /// <returns></returns>
    public override void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new StoreTemplate(id)), state);
    }

    /// <summary>
    /// Gets all available templates for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    /// <returns></returns>
    public override void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.AvailableTemplates, id, new IStoreTemplate[] { new StoreTemplate(id) }), state);
    }

    /// <summary>
    /// Creates a new Store on the server within the current catalog asynchronously.
    /// </summary>
    /// <param name="template">Template to create the store from.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    public override void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
    {
        try
        {
            var createParams = new Dictionary<string, string> {{"override", "false"}};
            HttpRequestMessage request =
                CreateRequest("repositories/" + template.ID, "*/*", HttpMethod.Put, createParams);
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore,
                            template.ID,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception, "creating store on")
                        ), state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict))
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore,
                                template.ID,
                                StorageHelper.HandleHttpError(response, "creating store on")), state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template),
                            state);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID,
                    new RdfStorageException(
                        "An unexpected error occurred while trying to create a store, see inner exception for details",
                        ex)), state);
        }
    }

    /// <summary>
    /// Deletes a Store from the server within the current catalog asynchronously.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to callback.</param>
    public override void DeleteStore(string storeId, AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = CreateRequest("repositories/" + storeId, "*/*", HttpMethod.Delete,
                new Dictionary<string, string>());
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception, "deleting a store from")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId,
                                StorageHelper.HandleHttpError(response, "deleting a store from")),
                            state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId),
                            state);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId,
                    StorageHelper.HandleError(ex, "delete the Store '" + storeId + "' from")), state);
        }
    }

    /// <summary>
    /// Gets a Store within the current catalog asynchronously.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to call back.</param>
    /// <returns></returns>
    /// <remarks>
    /// AllegroGraph groups stores by catalog, you may only use this method to obtain stores within your current catalogue.
    /// </remarks>
    public override void GetStore(string storeId, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, new AllegroGraphConnector(_agraphBase, _catalog, storeId, _username, _pwd, Proxy)), state);
    }

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="queryParams">Querystring Parameters.</param>
    /// <returns></returns>
    [Obsolete]
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

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="queryParams">Querystring Parameters.</param>
    /// <returns></returns>
    protected override HttpRequestMessage CreateRequest(string servicePath, string accept, HttpMethod method, Dictionary<string, string> queryParams)
    {
        // Remove JSON Mime Types from supported Accept types
        // This is a compatibility issue with Allegro having a weird custom JSON serialisation
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

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode storageServer = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageServer));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));
        INode catalog = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyCatalog));

        context.Graph.Assert(new Triple(manager, rdfType, storageServer));
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

        if (_username != null && _pwd != null)
        {
            INode username = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));
            context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_username)));
            context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_pwd)));
        }

        SerializeStandardConfig(manager, context);
    }
}
