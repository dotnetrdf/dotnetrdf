/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// Represents an AllegroGraph server, may be used to access and manage stores within a catalog on the server
    /// </summary>
    public class AllegroGraphServer
        : SesameServer
    {
        private String _agraphBase;
        private String _catalog;
         
        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base URI for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        public AllegroGraphServer(String baseUri, String catalogID)
            : this(baseUri, catalogID, (String)null, (String)null) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        public AllegroGraphServer(String baseUri)
            : this(baseUri, (String)null) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        public AllegroGraphServer(String baseUri, String catalogID, String username, String password)
            : base(baseUri, username, password)
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
#if NETCORE
            this._agraphBase = this._baseUri.Copy();
#else
            _agraphBase = String.Copy(_baseUri);
#endif
            if (catalogID != null)
            {
                _baseUri += "catalogs/" + catalogID + "/";
            }
            _catalog = catalogID;
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        public AllegroGraphServer(String baseUri, String username, String password)
            : this(baseUri, null, username, password) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, String catalogID, IWebProxy proxy)
            : this(baseUri, catalogID, null, null, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, IWebProxy proxy)
            : this(baseUri, null, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, String catalogID, String username, String password, IWebProxy proxy)
            : this(baseUri, catalogID, username, password)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri,  String username, String password, IWebProxy proxy)
            : this(baseUri, null, username, password, proxy) { }
        
        /// <summary>
        /// Gets a default template for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public override IStoreTemplate GetDefaultTemplate(String id)
        {
            return new StoreTemplate(id, "AllegroGraph", "An AllgroGraph store");
        }

        /// <summary>
        /// Gets all available templates for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public override IEnumerable<IStoreTemplate> GetAvailableTemplates(String id)
        {
            return GetDefaultTemplate(id).AsEnumerable();
        }

        /// <summary>
        /// Creates a new Store (if it doesn't already exist)
        /// </summary>
        /// <param name="template">Template for creating the new Store</param>
        public override bool CreateStore(IStoreTemplate template)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                var createParams = new Dictionary<string, string> {{"override", "false"}};
                request = CreateRequest("repositories/" + template.ID, "*/*", "PUT", createParams);

                Tools.HttpDebugRequest(request);

                using (response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    response.Close();
                }
                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    
                    // Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    if (code == 400)
                    {
                        // OK - Just means the Store already exists
                        return true;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Requests that AllegroGraph deletes a Store
        /// </summary>
        /// <param name="storeID">Store ID</param>
        public override void DeleteStore(String storeID)
        {
            try
            {
                HttpWebRequest request = CreateRequest("repositories/" + storeID, "*/*", "DELETE", new Dictionary<string, string>());
                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "delete");
            }
        }

        /// <summary>
        /// Get the lists of stores available on the Server
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<String> ListStores()
        {
            String data;
            try
            {
                HttpWebRequest request = CreateRequest("repositories", "application/json", "GET", new Dictionary<string, string>());
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        data = reader.ReadToEnd();
                        reader.Close();
                    }
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "list Stores from");
            }

            var json = JArray.Parse(data);
            var stores = new List<string>();
            foreach (var token in json.Children())
            {
                if (token["id"] is JValue id)
                {
                    stores.Add(id.Value.ToString());
                }
            }
            return stores;
        }

        /// <summary>
        /// Gets a Store within the current catalog
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// AllegroGraph groups stores by catalogue, you may only use this method to obtain stores within your current catalogue
        /// </remarks>
        public override IStorageProvider GetStore(String storeID)
        {
            // Otherwise return a new instance
            return new AllegroGraphConnector(_agraphBase, _catalog, storeID, _username, _pwd, Proxy);
        }

        /// <summary>
        /// Gets the List of Stores available  on the server within the current catalog asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        public override void ListStores(AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request = CreateRequest("repositories", "application/json", "GET", new Dictionary<string, string>());
                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        String data;
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            data = reader.ReadToEnd();
                            reader.Close();
                        }

                        var json = JArray.Parse(data);
                        var stores = new List<string>();
                        foreach (var token in json.Children())
                        {
                            if (token["id"] is JValue id)
                            {
                                stores.Add(id.Value.ToString());
                            }
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, stores), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "list Stores from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "list Stores from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "list Stores from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "list Stores from")), state);
            }
        }

        /// <summary>
        /// Gets a default template for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        /// <returns></returns>
        public override void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new StoreTemplate(id)), state);
        }

        /// <summary>
        /// Gets all available templates for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        /// <returns></returns>
        public override void GetAvailableTemplates(String id, AsyncStorageCallback callback, Object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.AvailableTemplates, id, new IStoreTemplate[] { new StoreTemplate(id) }), state);
        }

        /// <summary>
        /// Creates a new Store on the server within the current catalog asynchronously
        /// </summary>
        /// <param name="template">Template to create the store from</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        public override void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
        {
            try
            {
                var createParams = new Dictionary<string, string> {{"override", "false"}};
                var request = CreateRequest("repositories/" + template.ID, "*/*", "PUT", createParams);

                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        Tools.HttpDebugResponse(response);
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template), state);
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
                            if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            
                            // Got a Response so we can analyse the Response Code
                            var response = (HttpWebResponse)webEx.Response;
                            var code = (int)response.StatusCode;
                            if (code == 400)
                            {
                                // 400 just means the store already exists so this is OK
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template), state);
                            }
                            else
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("A HTTP error occurred while trying to create a store, see inner exception for details", webEx)), state);
                            }
                        }
                        else
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("A HTTP error occurred while trying to create a store, see inner exception for details", webEx)), state);
                        }
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("An unexpected error occurred while trying to create a store, see inner exception for details", ex)), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    
                    // Got a Response so we can analyse the Response Code
                    HttpWebResponse response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    if (code == 400)
                    {
                        // 400 just means the store already exists so this is OK
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("A HTTP error occurred while trying to create a store, see inner exception for details", webEx)), state);
                    }
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("A HTTP error occurred while trying to create a store, see inner exception for details", webEx)), state);
                }
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("An unexpected error occurred while trying to create a store, see inner exception for details", ex)), state);
            }
        }

        /// <summary>
        /// Deletes a Store from the server within the current catalog asynchronously
        /// </summary>
        /// <param name="storeId">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        public override void DeleteStore(string storeId, AsyncStorageCallback callback, object state)
        {
            try
            {
                var request = CreateRequest("repositories/" + storeId, "*/*", "DELETE", new Dictionary<string, string>());

                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        Tools.HttpDebugResponse(response);
                        
                        // If we get here then the operation completed OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId, StorageHelper.HandleHttpError(webEx, "delete the Store '" + storeId + "; from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId, StorageHelper.HandleError(ex, "delete the Store '" + storeId + "' from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId, StorageHelper.HandleHttpError(webEx, "delete the Store '" + storeId + "; from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeId, StorageHelper.HandleError(ex, "delete the Store '" + storeId + "' from")), state);
            }
        }

        /// <summary>
        /// Gets a Store within the current catalog asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to call back</param>
        /// <returns></returns>
        /// <remarks>
        /// AllegroGraph groups stores by catalog, you may only use this method to obtain stores within your current catalogue
        /// </remarks>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new AllegroGraphConnector(_agraphBase, _catalog, storeID, _username, _pwd, Proxy)), state);
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected override HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> queryParams)
        {
            // Remove JSON Mime Types from supported Accept types
            // This is a compatability issue with Allegro having a weird custom JSON serialisation
            if (accept.Contains("application/json"))
            {
                accept = accept.Replace("application/json,", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains("text/json"))
            {
                accept = accept.Replace("text/json", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains(",;")) accept = accept.Replace(",;", ",");

            return base.CreateRequest(servicePath, accept, method, queryParams);
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode storageServer = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageServer));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode catalog = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCatalog));

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
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_pwd)));
            }

            SerializeStandardConfig(manager, context);
        }
    }
}
