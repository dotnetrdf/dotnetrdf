/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage.Management
{
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
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._agraphBase = String.Copy(this._baseUri);
            if (catalogID != null)
            {
                this._baseUri += "catalogs/" + catalogID + "/";
            }
            this._catalog = catalogID;
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        public AllegroGraphServer(String baseUri, String username, String password)
            : this(baseUri, null, username, password) { }

#if !NO_PROXY

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, String catalogID, WebProxy proxy)
            : this(baseUri, catalogID, null, null, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, WebProxy proxy)
            : this(baseUri, null, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri, String catalogID, String username, String password, WebProxy proxy)
            : this(baseUri, catalogID, username, password)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphServer(String baseUri,  String username, String password, WebProxy proxy)
            : this(baseUri, null, username, password, proxy) { }

#endif

#if !NO_SYNC_HTTP

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
            return this.GetDefaultTemplate(id).AsEnumerable();
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
                Dictionary<String, String> createParams = new Dictionary<string, string>();
                createParams.Add("override", "false");
                request = this.CreateRequest("repositories/" + template.ID, "*/*", "PUT", createParams);

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    response.Close();
                }
                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    if (code == 400)
                    {
                        //OK - Just means the Store already exists
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
                HttpWebRequest request = this.CreateRequest("repositories/" + storeID, "*/*", "DELETE", new Dictionary<string, string>());

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
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
                HttpWebRequest request = this.CreateRequest("repositories", "application/json", "GET", new Dictionary<string, string>());
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

            JArray json = JArray.Parse(data);
            List<String> stores = new List<string>();
            foreach (JToken token in json.Children())
            {
                JValue id = token["id"] as JValue;
                if (id != null)
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
            //Otherwise return a new instance
            return new AllegroGraphConnector(this._agraphBase, this._catalog, storeID, this._username, this._pwd, this.Proxy);
        }

#endif

        /// <summary>
        /// Gets the List of Stores available  on the server within the current catalog asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        public override void ListStores(AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request = this.CreateRequest("repositories", "application/json", "GET", new Dictionary<string, string>());
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

                        JArray json = JArray.Parse(data);
                        List<String> stores = new List<string>();
                        foreach (JToken token in json.Children())
                        {
                            JValue id = token["id"] as JValue;
                            if (id != null)
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
                Dictionary<String, String> createParams = new Dictionary<string, string>();
                createParams.Add("override", "false");
                HttpWebRequest request = this.CreateRequest("repositories/" + template.ID, "*/*", "PUT", createParams);

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template), state);
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null)
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            }
#endif
                            //Got a Response so we can analyse the Response Code
                            HttpWebResponse response = (HttpWebResponse)webEx.Response;
                            int code = (int)response.StatusCode;
                            if (code == 400)
                            {
                                //400 just means the store already exists so this is OK
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
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    //Got a Response so we can analyse the Response Code
                    HttpWebResponse response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    if (code == 400)
                    {
                        //400 just means the store already exists so this is OK
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
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        public override void DeleteStore(string storeID, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request = this.CreateRequest("repositories/" + storeID, "*/*", "DELETE", new Dictionary<string, string>());

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif
                        //If we get here then the operation completed OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "delete the Store '" + storeID + "; from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "delete the Store '" + storeID + "' from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "delete the Store '" + storeID + "; from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "delete the Store '" + storeID + "' from")), state);
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
#if !NO_PROXY
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new AllegroGraphConnector(this._agraphBase, this._catalog, storeID, this._username, this._pwd, this.Proxy)), state);
#else
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new AllegroGraphConnector(this._agraphBase, this._catalog, storeID, this._username, this._pwd)), state);
#endif
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
            //Remove JSON Mime Types from supported Accept types
            //This is a compatability issue with Allegro having a weird custom JSON serialisation
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
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            if (this._catalog != null)
            {
                context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri.Substring(0, this._baseUri.IndexOf("catalogs/")))));
                context.Graph.Assert(new Triple(manager, catalog, context.Graph.CreateLiteralNode(this._catalog)));
            }
            else
            {
                context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            }

            if (this._username != null && this._pwd != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
            }

            base.SerializeProxyConfig(manager, context);
        }
    }
}
