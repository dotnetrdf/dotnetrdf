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
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using VDS.RDF.Configuration;
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
        : BaseHttpConnector, IAsyncStorageServer, IConfigurationSerializable
        , IStorageServer
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
        protected bool _hasCredentials;

        /// <summary>
        /// Repositories Prefix
        /// </summary>
        protected String _repositoriesPrefix = "repositories/";

        private SesameHttpProtocolConnector _sysConnection;

        /// <summary>
        /// Available Sesame template types
        /// </summary>
        protected List<Type> TemplateTypes = new List<Type>
        {
            typeof(SesameMemTemplate),
            typeof(SesameNativeTemplate),
            typeof(SesameHttpTemplate),
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
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
            _username = username;
            _pwd = password;
            _hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
        }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameServer(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameServer(String baseUri, String username, String password, IWebProxy proxy)
            : this(baseUri, username, password)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Gets the IO Behaviour of the server
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.StorageServer;
            }
        }


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
            var templates = new List<IStoreTemplate>();
            object[] args = { id };
            foreach (var t in TemplateTypes)
            {
                try
                {
                    if (Activator.CreateInstance(t, args) is IStoreTemplate template)
                    {
                        templates.Add(template);
                    }
                }
                catch
                {
                    // Ignore and continue
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

                    // Firstly we need to save the Repository Template as a new Context to Sesame
                    createParams.Add("context", sesameTemplate.ContextNode.ToString());
                    HttpWebRequest request = CreateRequest(_repositoriesPrefix + SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                    request.ContentType = MimeTypesHelper.NTriples[0];
                    NTriplesWriter ntwriter = new NTriplesWriter();
                    ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

                    Tools.HttpDebugRequest(request);
                    
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Tools.HttpDebugResponse(response);
                        // If we get then it was OK
                        response.Close();
                    }

                    // Then we need to declare that said Context is of type rep:RepositoryContext
                    Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                    EnsureSystemConnection();
                    _sysConnection.UpdateGraph(String.Empty, repoType.AsEnumerable(), null);

                    return true;
                }
                catch (WebException webEx)
                {
                    throw StorageHelper.HandleHttpError(webEx, "creating a new Store in");
                }
            }
            throw new RdfStorageException("Invalid template, templates must derive from BaseSesameTemplate");
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
            return new SesameHttpProtocolConnector(_baseUri, storeID, _username, _pwd, Proxy);
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
                HttpWebRequest request = CreateRequest(_repositoriesPrefix + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here it completed OK
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
                Tools.HttpDebugRequest(request);

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
            var templates = new List<IStoreTemplate>();
            object[] args = { id };
            foreach (var t in TemplateTypes)
            {
                try
                {
                    if (Activator.CreateInstance(t, args) is IStoreTemplate template)
                    {
                        templates.Add(template);
                    }
                }
                catch
                {
                    // Ignore and continue
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
            if (template is BaseSesameTemplate sesameTemplate)
            {
                // First we need to store the template as a new context in the SYSTEM repository
                var createParams = new Dictionary<string, string>();

                if (template.Validate().Any())
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors")), state);
                    return;
                }

                IGraph g = sesameTemplate.GetTemplateGraph();
                createParams.Add("context", sesameTemplate.ContextNode.ToString());
                HttpWebRequest request = CreateRequest(_repositoriesPrefix + SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                request.ContentType = MimeTypesHelper.NTriples[0];
                NTriplesWriter ntwriter = new NTriplesWriter();

                EnsureSystemConnection();
                _sysConnection.SaveGraphAsync(request, ntwriter, g, (sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        // Then we need to declare that said Context is of type rep:RepositoryContext
                        Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                        _sysConnection.UpdateGraph(String.Empty, repoType.AsEnumerable(), null, (sender2, args2, st2) =>
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
                provider = new SesameHttpProtocolConnector(_baseUri, storeID, _username, _pwd, Proxy);
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
                HttpWebRequest request = CreateRequest(_repositoriesPrefix + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());
                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        Tools.HttpDebugResponse(response);
                        // If we get here it completed OK
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
            // Build the Request Uri
            String requestUri = _baseUri + servicePath;
            if (queryParams != null)
            {
                if (queryParams.Count > 0)
                {
                    requestUri += "?";
                    foreach (String p in queryParams.Keys)
                    {
                        requestUri += p + "=" + HttpUtility.UrlEncode(queryParams[p]) + "&";
                    }
                    requestUri = requestUri.Substring(0, requestUri.Length - 1);
                }
            }

            // Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;

            // Add Credentials if needed
            if (_hasCredentials)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
#if !NETCORE
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + _pwd));
                    request.Headers.Add("Authorization", "Basic " + credentials);
#else
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _pwd));
                    request.Headers["Authorization"] = "Basic " + credentials;
#endif
                }
                else
                {
                    // Leave .Net to cope with HTTP auth challenge response
                    NetworkCredential credentials = new NetworkCredential(_username, _pwd);
                    request.Credentials = credentials;
#if !NETCORE
                    request.PreAuthenticate = true;
#endif
                }
            }

            return ApplyRequestOptions(request);
        }

        /// <summary>
        /// Ensures the connection to the Sesame SYSTEM repository is prepared if it isn't already
        /// </summary>
        protected virtual void EnsureSystemConnection()
        {
            if (_sysConnection == null)
            {
                _sysConnection = new SesameHttpProtocolConnector(_baseUri, SystemRepositoryID, _username, _pwd, Proxy);
            }
        }

        /// <summary>
        /// Disposes of the server
        /// </summary>
        public virtual void Dispose()
        {
            _sysConnection.Dispose();
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode storageServer = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageServer));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));

            context.Graph.Assert(new Triple(manager, rdfType, storageServer));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));

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
