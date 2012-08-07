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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Sesame;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// Represents a connection to a Sesame Server
    /// </summary>
    public class SesameServer
        : BaseHttpConnector, IAsyncStorageServer
#if !NO_SYNC_HTTP
        , IStorageServer
#endif
    {
        /// <summary>
        /// System Repository ID
        /// </summary>
        public const String SystemRepositoryID = "SYSTEM";

        /// <summary>
        /// Base Uri for the Server
        /// </summary>
        protected String _baseUri;
        /// <summary>
        /// Username for accessing the Server
        /// </summary>
        protected String _username;
        /// <summary>
        /// Password for accessing the Server
        /// </summary>
        protected String _pwd;
        /// <summary>
        /// Whether the User has provided credentials for accessing the Server using authentication
        /// </summary>
        protected bool _hasCredentials = false;

        /// <summary>
        /// Repositories Prefix
        /// </summary>
        protected String _repositoriesPrefix = "repositories/";

        private SesameHttpProtocolConnector _sysConnection;

        /// <summary>
        /// Available Sesame template types
        /// </summary>
        protected List<Type> _templateTypes = new List<Type>()
        {
            typeof(SesameMemTemplate),
            typeof(SesameNativeTemplate),
            typeof(SesameHttpTemplate)
        };

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        public SesameServer(String baseUri)
            : this(baseUri, null, null) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public SesameServer(String baseUri, String username, String password)
            : base()
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._username = username;
            this._pwd = password;
            this._hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameServer(String baseUri, WebProxy proxy)
            : this(baseUri, null, null, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameServer(String baseUri, String username, String password, WebProxy proxy)
            : this(baseUri, username, password)
        {
            this.Proxy = proxy;
        }

#endif

        public IOBehaviour IOBehaviour
        {
            get
            {
                return Storage.IOBehaviour.StorageServer;
            }
        }


#if !NO_SYNC_HTTP

        /// <summary>
        /// Gets a default template for creating a store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public virtual IStoreTemplate GetDefaultTemplate(string id)
        {
            return new SesameMemTemplate(id);
        }

        /// <summary>
        /// Gets all available templates for creating a store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public virtual IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] { id };
            foreach (Type t in this._templateTypes)
            {
                try
                {
                    IStoreTemplate template = Activator.CreateInstance(t, args) as IStoreTemplate;
                    if (template != null) templates.Add(template);
                }
                catch
                {
                    //Ignore and continue
                }
            }
            return templates;
        }

        /// <summary>
        /// Creates a new Store based on the given template
        /// </summary>
        /// <param name="template">Template</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Templates must inherit from <see cref="BaseSesameTemplate"/>
        /// </para>
        /// </remarks>
        public virtual bool CreateStore(IStoreTemplate template)
        {
            if (template is BaseSesameTemplate)
            {
                try
                {
                    Dictionary<String, String> createParams = new Dictionary<string, string>();
                    BaseSesameTemplate sesameTemplate = (BaseSesameTemplate)template;
                    if (template.Validate().Any()) throw new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors");
                    IGraph g = sesameTemplate.GetTemplateGraph();

                    //Firstly we need to save the Repository Template as a new Context to Sesame
                    createParams.Add("context", sesameTemplate.ContextNode.ToString());
                    HttpWebRequest request = this.CreateRequest(this._repositoriesPrefix + SesameServer.SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                    request.ContentType = MimeTypesHelper.NTriples[0];
                    NTriplesWriter ntwriter = new NTriplesWriter();
                    ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

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
                        //If we get then it was OK
                        response.Close();
                    }

                    //Then we need to declare that said Context is of type rep:RepositoryContext
                    Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                    this.EnsureSystemConnection();
                    this._sysConnection.UpdateGraph(String.Empty, repoType.AsEnumerable(), null);

                    return true;
                }
                catch (WebException webEx)
                {
                    throw StorageHelper.HandleHttpError(webEx, "creating a new Store in");
                }
            }
            else
            {
                throw new RdfStorageException("Invalid template, templates must derive from BaseSesameTemplate");
            }
        }

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the Store ID requested represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        public virtual IStorageProvider GetStore(string storeID)
        {
#if !NO_PROXY
            return new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
            return new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd);
#endif
        }

        /// <summary>
        /// Deletes the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <remarks>
        /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation
        /// </remarks>
        public virtual void DeleteStore(String storeID)
        {
            try
            {
                HttpWebRequest request = CreateRequest(this._repositoriesPrefix + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());
#if DEBUG
                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                    //If we get here it completed OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "deleting the Store '" + storeID + "' from");
            }
        }

        /// <summary>
        /// Gets the list of available stores
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<String> ListStores()
        {
            try
            {
                HttpWebRequest request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], "GET", new Dictionary<string, string>());
#if DEBUG
                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif
                ListStringsHandler handler = new ListStringsHandler("id");
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    SparqlXmlParser parser = new SparqlXmlParser();
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                    response.Close();
                }
                return handler.Strings;
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "listing Stores from");
            }
        }

#endif

        /// <summary>
        /// Gets a default template for creating a store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public virtual void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new SesameMemTemplate(id)), state);
        }

        /// <summary>
        /// Gets all available templates for creating a store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public virtual void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] { id };
            foreach (Type t in this._templateTypes)
            {
                try
                {
                    IStoreTemplate template = Activator.CreateInstance(t, args) as IStoreTemplate;
                    if (template != null) templates.Add(template);
                }
                catch
                {
                    //Ignore and continue
                }
            }
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.AvailableTemplates, id, templates), state);
        }

        /// <summary>
        /// Creates a new store based on the given template
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <para>
        /// Template must inherit from <see cref="BaseSesameTemplate"/>
        /// </para>
        /// </remarks>
        public virtual void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
        {
            if (template is BaseSesameTemplate)
            {
                //First we need to store the template as a new context in the SYSTEM repository
                Dictionary<String, String> createParams = new Dictionary<string, string>();
                BaseSesameTemplate sesameTemplate = (BaseSesameTemplate)template;

                if (template.Validate().Any())
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors")), state);
                    return;
                }

                IGraph g = sesameTemplate.GetTemplateGraph();
                createParams.Add("context", sesameTemplate.ContextNode.ToString());
                HttpWebRequest request = this.CreateRequest(this._repositoriesPrefix + SesameServer.SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                request.ContentType = MimeTypesHelper.NTriples[0];
                NTriplesWriter ntwriter = new NTriplesWriter();

                this.EnsureSystemConnection();
                this._sysConnection.SaveGraphAsync(request, ntwriter, g, (sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        //Then we need to declare that said Context is of type rep:RepositoryContext
                        Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                        this._sysConnection.UpdateGraph(String.Empty, repoType.AsEnumerable(), null, (sender2, args2, st2) =>
                        {
                            if (args.WasSuccessful)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template), state);
                            }
                            else
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(args.Error, "creating a new Store in")), state);
                            }
                        }, st);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, StorageHelper.HandleError(args.Error, "creating a new Store in")), state);
                    }
                }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, new RdfStorageException("Invalid template, templates must derive from BaseSesameTemplate")), state);
            }
        }

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        public virtual void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            try
            {
                IAsyncStorageProvider provider;
#if !NO_PROXY
                provider = new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                provider = new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, provider), state);
            }
            catch (Exception e)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, e), state);
            }
        }

        /// <summary>
        /// Deletes a store asynchronously
        /// </summary>
        /// <param name="storeID">ID of the store to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void DeleteStore(String storeID, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request = CreateRequest(this._repositoriesPrefix + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());
#if DEBUG
                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                        if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                        //If we get here it completed OK
                        response.Close();
                    }
                    catch (WebException webEx)
                    {
                        throw StorageHelper.HandleHttpError(webEx, "deleting the Store '" + storeID + "' from");
                    }
                    catch (Exception ex)
                    {
                        throw StorageHelper.HandleError(ex, "deleting the Store '" + storeID + "' asynchronously from");
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "deleting the Store '" + storeID + "' from");
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "deleting the Store '" + storeID + "' asynchronously from");
            }
        }

        /// <summary>
        /// Lists the available stores asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void ListStores(AsyncStorageCallback callback, Object state)
        {
            HttpWebRequest request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], "GET", new Dictionary<string, string>());
            ListStringsHandler handler = new ListStringsHandler("id");
            try
            {
                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        SparqlXmlParser parser = new SparqlXmlParser();
                        parser.Load(handler, new StreamReader(response.GetResponseStream()));
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, handler.Strings), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores from")), state);
            }
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
        {
            //Build the Request Uri
            String requestUri = this._baseUri + servicePath;
            if (queryParams != null)
            {
                if (queryParams.Count > 0)
                {
                    requestUri += "?";
                    foreach (String p in queryParams.Keys)
                    {
                        requestUri += p + "=" + Uri.EscapeDataString(queryParams[p]) + "&";
                    }
                    requestUri = requestUri.Substring(0, requestUri.Length - 1);
                }
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;

            //Add Credentials if needed
            if (this._hasCredentials)
            {
                NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
                request.Credentials = credentials;
            }

            return base.GetProxiedRequest(request);
        }

        protected virtual void EnsureSystemConnection()
        {
            if (this._sysConnection == null)
            {
#if !NO_PROXY
                this._sysConnection = new SesameHttpProtocolConnector(this._baseUri, SesameServer.SystemRepositoryID, this._username, this._pwd, this.Proxy);
#else
                this._sysConnection = new SesameHttpProtocolConnector(this._baseUri, SesameServer.SystemRepositoryID, this._username, this._pwd);
#endif
            }
        }

        public virtual void Dispose()
        {
            this._sysConnection.Dispose();
        }
    }
}
