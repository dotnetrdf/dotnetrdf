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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
using System.Web;

namespace VDS.RDF.Storage.Management;

/// <summary>
/// Abstract implementation of a management connection to a Stardog server using the HTTP protocol.
/// </summary>
public abstract class BaseStardogServer
    : BaseHttpConnector, IAsyncStorageServer, IConfigurationSerializable
      , IStorageServer
{
    /// <summary>
    /// The base URI of the Stardog server.
    /// </summary>
    protected readonly string _baseUri;

    /// <summary>
    /// The URI of the admin API.
    /// </summary>
    protected readonly string _adminUri;

    /// <summary>
    /// True if a user name and password are specified, false otherwise.
    /// </summary>
    protected bool HasCredentials { get; }

    /// <summary>
    /// Available Stardog template types.
    /// </summary>
    private readonly List<Type> _templateTypes = new List<Type>()
        {
            typeof (StardogMemTemplate),
            typeof (StardogDiskTemplate),
        };

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    protected BaseStardogServer(string baseUri)
        : this(baseUri, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    protected BaseStardogServer(string baseUri, string username, string password)
        : base()
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
        _adminUri = _baseUri + "admin/";
        SetCredentials(username, password);
        HasCredentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogServer(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Starddog server.
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="httpClientHandler"></param>
    protected BaseStardogServer(string baseUri, HttpClientHandler httpClientHandler) :
        base(httpClientHandler)
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
        _adminUri = _baseUri + "admin/";
        HasCredentials = httpClientHandler.Credentials != null;
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogServer(string baseUri, string username, string password, IWebProxy proxy)
        : this(baseUri, username, password)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Gets the IO Behaviour of the server.
    /// </summary>
    public virtual IOBehaviour IOBehaviour
    {
        get { return IOBehaviour.StorageServer; }
    }

    #region IStorageServer Members

    /// <summary>
    /// Lists the database available on the server.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<string> ListStores()
    {
        // GET /admin/databases - application/json
        HttpRequestMessage request = CreateAdminRequest("databases", "application/json", HttpMethod.Get,
            new Dictionary<string, string>());

        try
        {
            var stores = new List<string>();
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(data))
            {
                throw new RdfStorageException("Invalid Empty response from Stardog when listing Stores");
            }

            var obj = JObject.Parse(data);
            var dbs = (JArray) obj["databases"];
            foreach (JValue db in dbs.OfType<JValue>())
            {
                stores.Add(db.Value.ToString());
            }
            return stores;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Stores from");
        }
    }

    /// <summary>
    /// Gets a default template for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public virtual IStoreTemplate GetDefaultTemplate(string id)
    {
        return new StardogDiskTemplate(id);
    }

    /// <summary>
    /// Gets all available templates for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public virtual IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
    {
        var templates = new List<IStoreTemplate>();
        var args = new object[] {id};
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
    /// Creates a new Store based off the given template.
    /// </summary>
    /// <param name="template">Template.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Templates must inherit from <see cref="BaseStardogTemplate"/>.
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
                HttpRequestMessage request = BuildCreateStoreRequestMessage(template);

                // Make the request
                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, $"creating a new Store '{template.ID}' in");
                }
                // If we get here it completed OK
                return true;
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, $"creating a new Store '{template.ID}' in");
            }
        }

        throw new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate");
    }

    private HttpRequestMessage BuildCreateStoreRequestMessage(IStoreTemplate template)
    {
        // Get the Template
        var stardogTemplate = (BaseStardogTemplate) template;
        IEnumerable<string> errors = stardogTemplate.Validate();
        if (errors.Any())
            throw new RdfStorageException(
                "Template is not valid, call Validate() on the template to see the list of errors");
        JObject jsonTemplate = stardogTemplate.GetTemplateJson();

        // Create the request and write the JSON
        HttpRequestMessage request = CreateAdminRequest("databases", MimeTypesHelper.Any, HttpMethod.Post,
            new Dictionary<string, string>());
        request.Content =
            new MultipartFormDataContent(StorageHelper.HttpMultipartBoundary)
            {
                {new StringContent(jsonTemplate.ToString()), "root"},
            };
        return request;
    }

    /// <summary>
    /// Deletes a Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    public virtual void DeleteStore(string storeId)
    {
        // DELETE /admin/databases/{db}
        HttpRequestMessage request = CreateAdminRequest("databases/" + storeId, MimeTypesHelper.Any, HttpMethod.Delete, new Dictionary<string, string>());

        try
        {
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, $"deleting Store {storeId} from");
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, $"deleting Store {storeId} from");
        }
    }

    /// <summary>
    /// Gets a provider for the Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    public abstract IStorageProvider GetStore(string storeId);

    #endregion

    #region IAsyncStorageServer Members

    /// <summary>
    /// Lists all databases available on the server.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void ListStores(AsyncStorageCallback callback, object state)
    {
        // GET /admin/databases - application/json
        HttpRequestMessage request = CreateAdminRequest("databases", "application/json", HttpMethod.Get, new Dictionary<string, string>());

        try
        {
            var stores = new List<string>();
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    "listing Stores asynchronously from")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                StorageHelper.HandleHttpError(response, "listing Stores asynchronously from")),
                            state);
                    }
                    else
                    {
                        response.Content.ReadAsStringAsync().ContinueWith(readTask =>
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled.")
                                            : StorageHelper.HandleError(requestTask.Exception,
                                                "listing Stores asynchronously from")),
                                    state);
                            }
                            else
                            {
                                var obj = JObject.Parse(readTask.Result);
                                var dbs = (JArray)obj["databases"];
                                foreach (JValue db in dbs.OfType<JValue>())
                                {
                                    stores.Add(db.Value.ToString());
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
                    StorageHelper.HandleError(ex, "listing Stores asynchronously from")), state);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListStoresAsync(CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateAdminRequest("databases", "application/json", HttpMethod.Get, new Dictionary<string, string>());
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(response, "listing stores from");
        }

        var result = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(result);
        var dbs = (JArray)obj["databases"];
        if (dbs == null) throw new RdfStorageException("The server did not provide the expected JSON response when listing stores.");
        return dbs.OfType<JValue>().Select(db => db.Value.ToString()).ToList();
    }

    /// <summary>
    /// Gets a default template for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <returns></returns>
    public virtual void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new StardogDiskTemplate(id)), state);
    }

    /// <inheritdoc />
    public virtual Task<IStoreTemplate> GetDefaultTemplateAsync(string id, CancellationToken cancellationToken)
    {
        return Task.FromResult(new StardogDiskTemplate(id) as IStoreTemplate);
    }

    /// <summary>
    /// Gets all available templates for creating a new Store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <returns></returns>
    public virtual void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
    {
        IEnumerable<IStoreTemplate> templates = MakeStoreTemplatesList(id);
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.AvailableTemplates, id, templates), state);
    }

    private IEnumerable<IStoreTemplate> MakeStoreTemplatesList(string id)
    {
        var templates = new List<IStoreTemplate>();
        var args = new object[] {id};
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

    /// <inheritdoc />
    public virtual Task<IEnumerable<IStoreTemplate>> GetAvailableTemplatesAsync(string id, CancellationToken cancellationToken)
    {
        return Task.FromResult(MakeStoreTemplatesList(id));
    }

    /// <summary>
    /// Creates a new store based on the given template.
    /// </summary>
    /// <param name="template">Template.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <remarks>
    /// <para>
    /// Template must inherit from <see cref="BaseStardogTemplate"/>.
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
                HttpRequestMessage request = BuildCreateStoreRequestMessage(template);
                HttpClient.SendAsync(request).ContinueWith(requestTask =>
                {
                    if (requestTask.IsCanceled || requestTask.IsFaulted)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore,
                                template.ID,
                                requestTask.IsCanceled
                                    ? new RdfStorageException("The operation was cancelled.")
                                    : StorageHelper.HandleError(requestTask.Exception,
                                        $"creating a new Store '{template.ID}' asynchronously in")),
                            state);
                    }
                    else
                    {
                        HttpResponseMessage response = requestTask.Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore,
                                    template.ID,
                                    StorageHelper.HandleHttpError(response,
                                        $"creating a new Store '{template.ID}' asynchronously in")),
                                state);
                        }
                        else
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID),
                                state);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID,
                        StorageHelper.HandleError(ex,
                            "creating a new Store '" + template.ID + "' asynchronously in")), state);
            }
        }
        else
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate")), state);
        }
    }

    /// <inheritdoc />
    public virtual async Task<string> CreateStoreAsync(IStoreTemplate template, CancellationToken cancellationToken)
    {
        if (!(template is BaseStardogTemplate))
        {
            throw new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate");
        }

        try
        {
            HttpRequestMessage request = BuildCreateStoreRequestMessage(template);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, $"creating a new store '{template.ID}' in");
            }

            return template.ID;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, $"creating a new store '{template.ID}' in");
        }
    }

    /// <summary>
    /// Deletes a database from the server.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void DeleteStore(string storeId, AsyncStorageCallback callback, object state)
    {
        try
        {
            // DELETE /admin/databases/{db}
            HttpRequestMessage request = CreateAdminRequest("databases/" + storeId, MimeTypesHelper.Any,
                HttpMethod.Delete, new Dictionary<string, string>());
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(
                            AsyncStorageOperation.DeleteStore,
                            storeId,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    $"deleting Store {storeId} asynchronously from")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(
                                AsyncStorageOperation.DeleteStore,
                                storeId,
                                StorageHelper.HandleHttpError(response,
                                    $"deleting Store {storeId} asynchronously from")),
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
                    StorageHelper.HandleError(ex, "deleting Store " + storeId + " asynchronously from")), state);
        }
    }

    /// <inheritdoc />
    public async Task DeleteStoreAsync(string storeId, CancellationToken cancellationToken)
    {
        try
        {
            HttpRequestMessage request = CreateAdminRequest("databases/" + storeId, MimeTypesHelper.Any,
                HttpMethod.Delete, new Dictionary<string, string>());
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, $"deleting store '{storeId}' from");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, $"deleting store '{storeId}' from");
        }
    }

    /// <summary>
    /// Gets a database from the server.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public abstract void GetStore(string storeId, AsyncStorageCallback callback, object state);

    /// <inheritdoc />
    public abstract Task<IAsyncStorageProvider> GetStoreAsync(string storeId, CancellationToken cancellationToken);

    #endregion

    /// <summary>
    /// Create a request to the Stardog server's Admin API.
    /// </summary>
    /// <param name="servicePath">The admin API service path.</param>
    /// <param name="accept">Accept header content.</param>
    /// <param name="method">HTTP method to use.</param>
    /// <param name="requestParams">Additional request parameters.</param>
    /// <returns></returns>
    [Obsolete("This method is obsolete and will be removed in a future release.")]
    protected virtual HttpWebRequest CreateAdminRequest(string servicePath, string accept, string method, Dictionary<string, string> requestParams)
    {
        // Build the Request Uri
        var requestUri = _adminUri + servicePath;
        if (requestParams.Count > 0)
        {
            requestUri += "?";
            foreach (var p in requestParams.Keys)
            {
                requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
            }
            requestUri = requestUri.Substring(0, requestUri.Length - 1);
        }

        // Create our Request
        var request = (HttpWebRequest) WebRequest.Create(requestUri);
        request.Accept = accept;
        request.Method = method;
        //request = ApplyRequestOptions(request);

        // Add the special Stardog Headers
        request.Headers.Add("SD-Protocol", "1.0");

        // Add Credentials if needed
        if (HasCredentials)
        {
            //var credentials = new NetworkCredential(Username, Password);
            //request.Credentials = credentials;
            request.Credentials = HttpClientHandler.Credentials;
            request.PreAuthenticate = true;
        }

        return request;
    }


    /// <summary>
    /// Create a request to the Stardog server's Admin API.
    /// </summary>
    /// <param name="servicePath">The admin API service path.</param>
    /// <param name="accept">Accept header content.</param>
    /// <param name="method">HTTP method to use.</param>
    /// <param name="requestParams">Additional request parameters.</param>
    /// <returns></returns>
    protected virtual HttpRequestMessage CreateAdminRequest(string servicePath, string accept, HttpMethod method, Dictionary<string, string> requestParams)
    {
        // Build the Request Uri
        var requestUri = _adminUri + servicePath;
        if (requestParams.Count > 0)
        {
            requestUri += "?";
            foreach (var p in requestParams.Keys)
            {
                requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
            }
            requestUri = requestUri.Substring(0, requestUri.Length - 1);
        }

        // Create our Request
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add("Accept", accept);

        // Add the special Stardog Headers
        request.Headers.Add("SD-Protocol", "1.0");

        return request;
    }
    
    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode storageServer = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageServer));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));

        context.Graph.Assert(new Triple(manager, rdfType, storageServer));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));

        if (HttpClientHandler?.Credentials is NetworkCredential networkCredential)
        {
            INode username = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));
            context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(networkCredential.UserName)));
            context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(networkCredential.Password)));
        }

        SerializeStandardConfig(manager, context);
    }

    /// <summary>
    /// Static Class containing constants relevant to provisioning new Stardog stores.
    /// </summary>
    public static class DatabaseOptions
    {
        /// <summary>
        /// Constants for valid Stardog Options.
        /// </summary>
        public const string Online = "database.online",
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
        /// Constants for valid Stardog Database types.
        /// </summary>
        public const string DatabaseTypeDisk = "disk",
                            DatabaseTypeMemory = "memory";

        /// <summary>
        /// Constanst for valid Search Re-Index Modes.
        /// </summary>
        public const string SearchReIndexModeSync = "sync",
                            SearchReIndexModeAsync = "async";

        /// <summary>
        /// Constants for special named graph URIs.
        /// </summary>
        public const string SpecialNamedGraphDefault = "default",
                            SpecialNamedGraphUnionAll = "*";

        /// <summary>
        /// Constants for various Stardog reasoning settings.
        /// </summary>
        public const StardogReasoningMode DefaultIcvReasoningMode = StardogReasoningMode.None;

        /// <summary>
        /// Constant for various Stardog integer settings.
        /// </summary>
        public const int DefaultMinDifferentialIndexLimit = 1000000,
                         DefaultMaxDifferentialIndexLimit = 10000;

        /// <summary>
        /// Constants for various Stardog boolean flags.
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
        /// Pattern for valid Stardog database names.
        /// </summary>
        public const string ValidDatabaseNamePattern = "^[A-Za-z]{1}[A-Za-z0-9_-]*$";

        /// <summary>
        /// Validates whether a Database Name is valid.
        /// </summary>
        /// <param name="name">Database Name.</param>
        /// <returns></returns>
        public static bool IsValidDatabaseName(string name)
        {
            return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, ValidDatabaseNamePattern);
        }

        /// <summary>
        /// Validates whether a Database Type is valid.
        /// </summary>
        /// <param name="type">Database Type.</param>
        /// <returns></returns>
        public static bool IsValidDatabaseType(string type)
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
        /// Validates whether a Search Re-Index Mode is valid.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns></returns>
        public static bool IsValidSearchReIndexMode(string mode)
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
        /// Validates whether a Named Graph URI is valid.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <returns></returns>
        public static bool IsValidNamedGraph(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return false;
            if (uri.Equals(SpecialNamedGraphDefault) || uri.Equals(SpecialNamedGraphUnionAll))
            {
                return true;
            }
            else
            {
                try
                {
                    var u = new Uri(uri);
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
/// Management connection for Stardog 1.* servers.
/// </summary>
public class StardogV1Server
    : BaseStardogServer
{
    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    public StardogV1Server(string baseUri)
        : this(baseUri, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogV1Server(string baseUri, string username, string password)
        : base(baseUri, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV1Server(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV1Server(string baseUri, string username, string password, IWebProxy proxy)
        : base(baseUri, username, password, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog server.
    /// </summary>
    /// <param name="baseUri">Base URI of the server.</param>
    /// <param name="httpClientHandler">Handler to configure outgoing HTTP requests.</param>
    public StardogV1Server(string baseUri, HttpClientHandler httpClientHandler)
        : base(baseUri, httpClientHandler)
    {
    }

    /// <summary>
    /// Gets a provider for the Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    public override IStorageProvider GetStore(string storeId)
    {
        return new StardogV1Connector(_baseUri, storeId, HttpClientHandler);
    }

    /// <summary>
    /// Gets a database from the server.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void GetStore(string storeId, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, new StardogV1Connector(_baseUri, storeId, HttpClientHandler)), state);
    }

    /// <inheritdoc />
    public override Task<IAsyncStorageProvider> GetStoreAsync(string storeId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new StardogV1Connector(_baseUri, storeId, HttpClientHandler) as IAsyncStorageProvider);
    }
}

/// <summary>
/// Management connection for Stardog 2.* servers.
/// </summary>
public class StardogV2Server
    : StardogV1Server
{
    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    public StardogV2Server(string baseUri)
        : this(baseUri, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogV2Server(string baseUri, string username, string password)
        : base(baseUri, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Server(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV2Server(string baseUri, string username, string password, IWebProxy proxy)
        : base(baseUri, username, password, proxy)
    {
    }



    /// <summary>
    /// Gets a provider for the Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    public override IStorageProvider GetStore(string storeId)
    {
        return new StardogV2Connector(_baseUri, storeId, HttpClientHandler);
    }

    /// <summary>
    /// Gets a database from the server.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void GetStore(string storeId, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, new StardogV2Connector(_baseUri, storeId, HttpClientHandler)), state);
    }
}

/// <summary>
/// Management connection for Stardog 3.* servers.
/// </summary>
public class StardogV3Server
    : StardogV2Server
{
    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    public StardogV3Server(string baseUri)
        : this(baseUri, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogV3Server(string baseUri, string username, string password)
        : base(baseUri, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV3Server(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogV3Server(string baseUri, string username, string password, IWebProxy proxy)
        : base(baseUri, username, password, proxy)
    {
    }
    

    /// <summary>
    /// Gets a provider for the Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    public override IStorageProvider GetStore(string storeId)
    {
        return new StardogV3Connector(_baseUri, storeId, HttpClientHandler);
    }

    /// <summary>
    /// Gets a database from the server.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void GetStore(string storeId, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, new StardogV3Connector(_baseUri, storeId, HttpClientHandler)), state);
    }
}

/// <summary>
/// Management connection for Stardog servers running the latest version, current this is 3.*.
/// </summary>
public class StardogServer
    : StardogV3Server
{
    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    public StardogServer(string baseUri)
        : this(baseUri, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public StardogServer(string baseUri, string username, string password)
        : base(baseUri, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogServer(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    public StardogServer(string baseUri, string username, string password, IWebProxy proxy)
        : base(baseUri, username, password, proxy)
    {
    }
}
