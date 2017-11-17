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
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
using System.Web;

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// Abstract implementation of a management connection to a Stardog server using the HTTP protocol
    /// </summary>
    public abstract class BaseStardogServer
        : BaseHttpConnector, IAsyncStorageServer, IConfigurationSerializable
          , IStorageServer
    {
        /// <summary>
        /// The base URI of the Stardog server
        /// </summary>
        protected readonly string BaseUri;

        /// <summary>
        /// The URI of the admin API
        /// </summary>
        protected readonly string AdminUri;

        /// <summary>
        /// The username to use for the connection
        /// </summary>
        protected readonly string Username;

        /// <summary>
        /// The password to use for the connection
        /// </summary>
        protected readonly string Password;
        
        /// <summary>
        /// True if a user name and password are specified, false otherwise
        /// </summary>
        protected readonly bool HasCredentials = false;

        /// <summary>
        /// Available Stardog template types
        /// </summary>
        private readonly List<Type> _templateTypes = new List<Type>()
            {
                typeof (StardogMemTemplate),
                typeof (StardogDiskTemplate),
            };

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        public BaseStardogServer(String baseUri)
            : this(baseUri, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public BaseStardogServer(String baseUri, String username, String password)
            : base()
        {
            BaseUri = baseUri;
            if (!BaseUri.EndsWith("/")) BaseUri += "/";
            AdminUri = BaseUri + "admin/";

            Username = username;
            Password = password;
            HasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseStardogServer(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseStardogServer(String baseUri, String username, String password, IWebProxy proxy)
            : this(baseUri, username, password)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Gets the IO Behaviour of the server
        /// </summary>
        public virtual IOBehaviour IOBehaviour
        {
            get { return IOBehaviour.StorageServer; }
        }

        #region IStorageServer Members

        /// <summary>
        /// Lists the database available on the server
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<string> ListStores()
        {
            // GET /admin/databases - application/json
            HttpWebRequest request = CreateAdminRequest("databases", "application/json", "GET", new Dictionary<string, string>());
            Tools.HttpDebugRequest(request);

            try
            {
                List<String> stores = new List<string>();
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    String data = null;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        data = reader.ReadToEnd();
                    }
                    if (String.IsNullOrEmpty(data)) throw new RdfStorageException("Invalid Empty response from Stardog when listing Stores");

                    JObject obj = JObject.Parse(data);
                    JArray dbs = (JArray) obj["databases"];
                    foreach (JValue db in dbs.OfType<JValue>())
                    {
                        stores.Add(db.Value.ToString());
                    }

                    response.Close();
                }
                return stores;
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "listing Stores from");
            }
        }

        /// <summary>
        /// Gets a default template for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public virtual IStoreTemplate GetDefaultTemplate(string id)
        {
            return new StardogDiskTemplate(id);
        }

        /// <summary>
        /// Gets all available templates for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <returns></returns>
        public virtual IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] {id};
            foreach (Type t in _templateTypes)
            {
                try
                {
                    if (Activator.CreateInstance(t, args) is IStoreTemplate template) templates.Add(template);
                }
                catch
                {
                    // Ignore and continue
                }
            }
            return templates;
        }

        /// <summary>
        /// Creates a new Store based off the given template
        /// </summary>
        /// <param name="template">Template</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Templates must inherit from <see cref="BaseStardogTemplate"/>
        /// </para>
        /// <para>
        /// Uses some code based off on answers <a href="http://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data">here</a> to help do the multipart form data request.
        /// </para>
        /// </remarks>
        public virtual bool CreateStore(IStoreTemplate template)
        {
            if (template is BaseStardogTemplate)
            {
                // POST /admin/databases
                // Creates a new database; expects a multipart request with a JSON specifying database name, options and filenames followed by (optional) file contents as a multipart POST request.
                try
                {
                    // Get the Template
                    BaseStardogTemplate stardogTemplate = (BaseStardogTemplate) template;
                    IEnumerable<String> errors = stardogTemplate.Validate();
                    if (errors.Any()) throw new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors");
                    JObject jsonTemplate = stardogTemplate.GetTemplateJson();
                    Console.WriteLine(jsonTemplate.ToString());

                    // Create the request and write the JSON
                    HttpWebRequest request = CreateAdminRequest("databases", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    String boundary = StorageHelper.HttpMultipartBoundary;
                    byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] terminatorBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    using (Stream stream = request.GetRequestStream())
                    {
                        // Boundary
                        stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        // Then the root Item
                        String templateItem = String.Format(StorageHelper.HttpMultipartContentTemplate, "root", jsonTemplate.ToString());
                        byte[] itemBytes = Encoding.UTF8.GetBytes(templateItem);
                        stream.Write(itemBytes, 0, itemBytes.Length);
                        // Then terminating boundary
                        stream.Write(terminatorBytes, 0, terminatorBytes.Length);
                        stream.Close();
                    }

                    Tools.HttpDebugRequest(request);

                    // Make the request
                    using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    {
                        Tools.HttpDebugResponse(response);

                        // If we get here it completed OK
                        response.Close();
                    }
                    return true;
                }
                catch (WebException webEx)
                {
                    throw StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' in");
                }
            }
            else
            {
                throw new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate");
            }
        }

        /// <summary>
        /// Deletes a Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        public virtual void DeleteStore(string storeID)
        {
            // DELETE /admin/databases/{db}
            HttpWebRequest request = CreateAdminRequest("databases/" + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());

            Tools.HttpDebugRequest(request);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    // If we get here then it completed OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " from");
            }
        }

        /// <summary>
        /// Gets a provider for the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        public abstract IStorageProvider GetStore(string storeID);

        #endregion

        #region IAsyncStorageServer Members

        /// <summary>
        /// Lists all databases available on the server
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void ListStores(AsyncStorageCallback callback, object state)
        {
            // GET /admin/databases - application/json
            HttpWebRequest request = CreateAdminRequest("databases", "application/json", "GET", new Dictionary<string, string>());

            Tools.HttpDebugRequest(request);

            try
            {
                List<String> stores = new List<string>();
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);

                            Tools.HttpDebugResponse(response);

                            String data = null;
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                data = reader.ReadToEnd();
                            }
                            if (String.IsNullOrEmpty(data)) throw new RdfStorageException("Invalid Empty response from Stardog when listing Stores");

                            JObject obj = JObject.Parse(data);
                            JArray dbs = (JArray) obj["databases"];
                            foreach (JValue db in dbs.OfType<JValue>())
                            {
                                stores.Add(db.Value.ToString());
                            }

                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, stores), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores asynchronously from")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores asynchronously from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores asynchronously from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores asynchronously from")), state);
            }
        }

        /// <summary>
        /// Gets a default template for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public virtual void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new StardogDiskTemplate(id)), state);
        }

        /// <summary>
        /// Gets all available templates for creating a new Store
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public virtual void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] {id};
            foreach (Type t in _templateTypes)
            {
                try
                {
                    if (Activator.CreateInstance(t, args) is IStoreTemplate template) templates.Add(template);
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
        /// Template must inherit from <see cref="BaseStardogTemplate"/>
        /// </para>
        /// </remarks>
        public virtual void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
        {
            if (template is BaseStardogTemplate)
            {
                // POST /admin/databases
                // Creates a new database; expects a multipart request with a JSON specifying database name, options and filenames followed by (optional) file contents as a multipart POST request.
                try
                {
                    // Get the Template
                    BaseStardogTemplate stardogTemplate = (BaseStardogTemplate) template;
                    IEnumerable<String> errors = stardogTemplate.Validate();
                    if (errors.Any()) throw new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors");
                    JObject jsonTemplate = stardogTemplate.GetTemplateJson();
                    Console.WriteLine(jsonTemplate.ToString());

                    // Create the request and write the JSON
                    HttpWebRequest request = CreateAdminRequest("databases", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    String boundary = StorageHelper.HttpMultipartBoundary;
                    byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] terminatorBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    request.BeginGetRequestStream(r =>
                        {
                            try
                            {
                                using (Stream stream = request.EndGetRequestStream(r))
                                {
                                    // Boundary
                                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                    // Then the root Item
                                    String templateItem = String.Format(StorageHelper.HttpMultipartContentTemplate, "root", jsonTemplate.ToString());
                                    byte[] itemBytes = Encoding.UTF8.GetBytes(templateItem);
                                    stream.Write(itemBytes, 0, itemBytes.Length);
                                    // Then terminating boundary
                                    stream.Write(terminatorBytes, 0, terminatorBytes.Length);
                                    stream.Close();
                                }

                                Tools.HttpDebugRequest(request);

                                // Make the request
                                request.BeginGetResponse(r2 =>
                                    {
                                        try
                                        {
                                            using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2))
                                            {
                                                Tools.HttpDebugResponse(response);

                                                // If we get here it completed OK
                                                response.Close();
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID), state);
                                        }
                                        catch (WebException webEx)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                                        }
                                        catch (Exception ex)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                                        }
                                    }, state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                            }
                        }, state);
                }
                catch (WebException webEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                }
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate")), state);
            }
        }

        /// <summary>
        /// Deletes a database from the server
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void DeleteStore(string storeID, AsyncStorageCallback callback, object state)
        {
            // DELETE /admin/databases/{db}
            HttpWebRequest request = CreateAdminRequest("databases/" + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());

            Tools.HttpDebugRequest(request);

            try
            {
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);

                            Tools.HttpDebugResponse(response);

                            // If we get here then it completed OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " asynchronously from")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "deleting Store " + storeID + " asynchronously from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " asynchronously from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "deleting Store " + storeID + " asynchronously from")), state);
            }
        }

        /// <summary>
        /// Gets a database from the server
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public abstract void GetStore(string storeID, AsyncStorageCallback callback, object state);

        #endregion

        /// <summary>
        /// Create a request to the Stardog server's Admin API
        /// </summary>
        /// <param name="servicePath">The admin API service path</param>
        /// <param name="accept">Accept header content</param>
        /// <param name="method">HTTP method to use</param>
        /// <param name="requestParams">Additional request parameters</param>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateAdminRequest(string servicePath, string accept, string method, Dictionary<String, String> requestParams)
        {
            // Build the Request Uri
            string requestUri = AdminUri + servicePath;
            if (requestParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in requestParams.Keys)
                {
                    requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            // Create our Request
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            request = ApplyRequestOptions(request);

            // Add the special Stardog Headers
#if !NETCORE
            request.Headers.Add("SD-Protocol", "1.0");
#else
            request.Headers["SD-Protocol"] = "1.0";
#endif

            // Add Credentials if needed
            if (HasCredentials)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
#if !NETCORE
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password));
                    request.Headers.Add("Authorization", "Basic " + credentials);
#else
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Username + ":" + this.Password));
                    request.Headers["Authorization"] = "Basic " + credentials;
#endif
                }
                else
                {
                    // Leave .Net to cope with HTTP auth challenge response
                    NetworkCredential credentials = new NetworkCredential(Username, Password);
                    request.Credentials = credentials;
#if !NETCORE
                    request.PreAuthenticate = true;
#endif
                }
            }

            return request;
        }

        /// <summary>
        /// Disposes of the server
        /// </summary>
        public virtual void Dispose()
        {
            // Nothing to do
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
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(BaseUri)));

            if (Username != null && Password != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(Username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(Password)));
            }

            SerializeStandardConfig(manager, context);
        }

        /// <summary>
        /// Static Class containing constants relevant to provisioning new Stardog stores
        /// </summary>
        public static class DatabaseOptions
        {
            /// <summary>
            /// Constants for valid Stardog Options
            /// </summary>
            public const String Online = "database.online",
                                IcvActiveGraphs = "icv.active.graphs",
                                IcvEnabled = "icv.enabled",
                                IcvReasoningType = "icv.reasoning.type",
                                IndexDifferentialEnableLimit = "index.differential.enable.limit",
                                IndexDifferentialMergeLimit = "index.differential.merge.limit",
                                IndexLiteralsCanonical = "index.literals.canonical",
                                IndexNamedGraphs = "index.named.graphs",
                                IndexPersistTrue = "index.persist.true",
                                IndexPersistSync = "index.persist.sync",
                                IndexStatisticsAutoUpdate = "index.statistics.update.automatic",
                                IndexType = "index.type",
                                ReasoningAutoConsistency = "reasoning.consistency.automatic",
                                ReasoningPunning = "reasoning.punning.enabled",
                                ReasoningSchemaGraphs = "reasoning.schema.graphs",
                                SearchEnabled = "search.enabled",
                                SearchReIndexMode = "search.reindex.mode",
                                TransactionsDurable = "transactions.durable";

            /// <summary>
            /// Constants for valid Stardog Database types
            /// </summary>
            public const String DatabaseTypeDisk = "disk",
                                DatabaseTypeMemory = "memory";

            /// <summary>
            /// Constanst for valid Search Re-Index Modes
            /// </summary>
            public const String SearchReIndexModeSync = "sync",
                                SearchReIndexModeAsync = "async";

            /// <summary>
            /// Constants for special named graph URIs
            /// </summary>
            public const String SpecialNamedGraphDefault = "default",
                                SpecialNamedGraphUnionAll = "*";

            /// <summary>
            /// Constants for various Stardog reasoning settings
            /// </summary>
            public const StardogReasoningMode DefaultIcvReasoningMode = StardogReasoningMode.None;

            /// <summary>
            /// Constant for various Stardog integer settings
            /// </summary>
            public const int DefaultMinDifferentialIndexLimit = 1000000,
                             DefaultMaxDifferentialIndexLimit = 10000;

            /// <summary>
            /// Constants for various Stardog boolean flags
            /// </summary>
            public const bool DefaultCanonicaliseLiterals = true,
                              DefaultNamedGraphIndexing = true,
                              DefaultPersistIndex = false,
                              DefaultPersistIndexSync = true,
                              DefaultAutoUpdateStats = true,
                              DefaultIcvEnabled = false,
                              DefaultConsistencyChecking = false,
                              DefaultPunning = false,
                              DefaultFullTextSearch = false,
                              DefaultDurableTransactions = false;

            /// <summary>
            /// Pattern for valid Stardog database names
            /// </summary>
            public const String ValidDatabaseNamePattern = "^[A-Za-z]{1}[A-Za-z0-9_-]*$";

            /// <summary>
            /// Validates whether a Database Name is valid
            /// </summary>
            /// <param name="name">Database Name</param>
            /// <returns></returns>
            public static bool IsValidDatabaseName(String name)
            {
                return !String.IsNullOrEmpty(name) && Regex.IsMatch(name, ValidDatabaseNamePattern);
            }

            /// <summary>
            /// Validates whether a Database Type is valid
            /// </summary>
            /// <param name="type">Database Type</param>
            /// <returns></returns>
            public static bool IsValidDatabaseType(String type)
            {
                switch (type.ToLower())
                {
                    case DatabaseTypeDisk:
                    case DatabaseTypeMemory:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Validates whether a Search Re-Index Mode is valid
            /// </summary>
            /// <param name="mode">Mode</param>
            /// <returns></returns>
            public static bool IsValidSearchReIndexMode(String mode)
            {
                switch (mode.ToLower())
                {
                    case SearchReIndexModeAsync:
                    case SearchReIndexModeSync:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Validates whether a Named Graph URI is valid
            /// </summary>
            /// <param name="uri">URI</param>
            /// <returns></returns>
            public static bool IsValidNamedGraph(String uri)
            {
                if (String.IsNullOrEmpty(uri)) return false;
                if (uri.Equals(SpecialNamedGraphDefault) || uri.Equals(SpecialNamedGraphUnionAll))
                {
                    return true;
                }
                else
                {
                    try
                    {
                        Uri u = new Uri(uri);
                        return u.IsAbsoluteUri;
                    }
                    catch (UriFormatException)
                    {
                        return false;
                    }
                }
            }

            // The legal value of icv.active.graphs is a list of named graph identifiers. See reasoning.schema.graphs below for syntactic sugar URIs for default graph and all named graphs.

            // The legal value of icv.reasoning.type is one of the reasoning levels (i.e, one of the following strings): NONE, RDFS, QL, RL, EL, DL.

            // The legal value of index.differential.* is an integer.

            // The legal value of index.type is the string "disk" or "memory" (case-insensitive).

            // The legal value of reasoning.schema.graphs is a list of named graph identifiers, including (optionally) the special names, tag:stardog:api:context:default and tag:stardog:api:context:all, which represent the default graph and the union of all named graphs and the default graph, respectively. In the context of database configurations only, Stardog will recognize default and * as shorter forms of those URIs, respectively.

            // The legal value of search.reindex.mode is one of the strings sync or async (case insensitive) or a legal Quartz cron expression

            // Config Option	Mutability	Default	API
            // Config Option	Mutability	Default	API
            // database.name	false	{NO DEFAULT}	DatabaseOptions.NAME
            // database.online	false6	true	DatabaseOptions.ONLINE
            // icv.active.graphs	false	default	DatabaseOptions.ICV_ACTIVE_GRAPHS
            // icv.enabled	true	false	DatabaseOptions.ICV_ENABLED
            // icv.reasoning.type	true	NONE	DatabaseOptions.ICV_REASONING_TYPE
            // index.differential.enable.limit	true	1000000	IndexOptions.DIFF_INDEX_MIN_LIMIT
            // index.differential.merge.limit	true	10000	IndexOptions.DIFF_INDEX_MAX_LIMIT
            // index.literals.canonical	false	true	IndexOptions.CANONICAL_LITERALS
            // index.named.graphs	false	true	IndexOptions.INDEX_NAMED_GRAPHS
            // index.persist	true	false	IndexOptions.PERSIST
            // index.persist.sync	true	true	IndexOptions.SYNC
            // index.statistics.update.automatic	true	true	IndexOptions.AUTO_STATS_UPDATE
            // index.type	false	Disk	IndexOptions.INDEX_TYPE
            // reasoning.consistency.automatic	true	false	DatabaseOptions.CONSISTENCY_AUTOMATIC
            // reasoning.punning.enabled	false	false	DatabaseOptions.PUNNING_ENABLED
            // reasoning.schema.graphs	true	default	DatabaseOptions.SCHEMA_GRAPHS
            // search.enabled	false	false	DatabaseOptions.SEARCHABLE
            // search.reindex.mode	false	wait	DatabaseOptions.SEARCH_REINDEX_MODE
            // transactions.durable	true	false	DatabaseOptions.TRANSACTIONS_DURABLE
        }
    }

    /// <summary>
    /// Management connection for Stardog 1.* servers
    /// </summary>
    public class StardogV1Server
        : BaseStardogServer
    {
        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        public StardogV1Server(String baseUri)
            : this(baseUri, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV1Server(String baseUri, String username, String password)
            : base(baseUri, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Server(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Server(String baseUri, String username, String password, IWebProxy proxy)
            : base(baseUri, username, password, proxy)
        {
        }
        

        /// <summary>
        /// Gets a provider for the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        public override IStorageProvider GetStore(string storeID)
        {
            return new StardogV1Connector(BaseUri, storeID, Username, Password, Proxy);
        }

        /// <summary>
        /// Gets a database from the server
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new StardogV1Connector(BaseUri, storeID, Username, Password, Proxy)), state);
        }
    }

    /// <summary>
    /// Management connection for Stardog 2.* servers
    /// </summary>
    public class StardogV2Server
        : StardogV1Server
    {
        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        public StardogV2Server(String baseUri)
            : this(baseUri, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV2Server(String baseUri, String username, String password)
            : base(baseUri, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Server(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Server(String baseUri, String username, String password, IWebProxy proxy)
            : base(baseUri, username, password, proxy)
        {
        }



        /// <summary>
        /// Gets a provider for the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        public override IStorageProvider GetStore(string storeID)
        {
            return new StardogV2Connector(BaseUri, storeID, Username, Password, Proxy);
        }

        /// <summary>
        /// Gets a database from the server
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new StardogV2Connector(BaseUri, storeID, Username, Password, Proxy)), state);
        }
    }

    /// <summary>
    /// Management connection for Stardog 3.* servers
    /// </summary>
    public class StardogV3Server
        : StardogV2Server
    {
        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        public StardogV3Server(String baseUri)
            : this(baseUri, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV3Server(String baseUri, String username, String password)
            : base(baseUri, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV3Server(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV3Server(String baseUri, String username, String password, IWebProxy proxy)
            : base(baseUri, username, password, proxy)
        {
        }
        

        /// <summary>
        /// Gets a provider for the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        public override IStorageProvider GetStore(string storeID)
        {
            return new StardogV3Connector(BaseUri, storeID, Username, Password, Proxy);
        }

        /// <summary>
        /// Gets a database from the server
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new StardogV3Connector(BaseUri, storeID, Username, Password, Proxy)), state);
        }
    }

    /// <summary>
    /// Management connection for Stardog servers running the latest version, current this is 3.*
    /// </summary>
    public class StardogServer
        : StardogV3Server
    {
        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        public StardogServer(String baseUri)
            : this(baseUri, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogServer(String baseUri, String username, String password)
            : base(baseUri, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogServer(String baseUri, IWebProxy proxy)
            : this(baseUri, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogServer(String baseUri, String username, String password, IWebProxy proxy)
            : base(baseUri, username, password, proxy)
        {
        }
    }
}
