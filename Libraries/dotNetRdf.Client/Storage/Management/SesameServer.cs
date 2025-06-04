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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Sesame;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage.Management;

/// <summary>
/// Represents a connection to a Sesame Server.
/// </summary>
public class SesameServer
    : BaseHttpConnector, IAsyncStorageServer, IConfigurationSerializable
    , IStorageServer
{
    /// <summary>
    /// System Repository ID.
    /// </summary>
    public const string SystemRepositoryID = "SYSTEM";

    /// <summary>
    /// Base Uri for the Server.
    /// </summary>
    protected string _baseUri;
    /// <summary>
    /// Username for accessing the Server.
    /// </summary>
    protected string _username;
    /// <summary>
    /// Password for accessing the Server.
    /// </summary>
    protected string _pwd;
    /// <summary>
    /// Whether the User has provided credentials for accessing the Server using authentication.
    /// </summary>
    protected bool _hasCredentials;

    /// <summary>
    /// Repositories Prefix.
    /// </summary>
    protected string _repositoriesPrefix = "repositories/";

    private SesameHttpProtocolConnector _sysConnection;

    /// <summary>
    /// Available Sesame template types.
    /// </summary>
    protected List<Type> TemplateTypes = new List<Type>
    {
        typeof(SesameMemTemplate),
        typeof(SesameNativeTemplate),
        typeof(SesameHttpTemplate),
    };

    /// <summary>
    /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Store.</param>
    public SesameServer(string baseUri)
        : this(baseUri, null, null) { }

    /// <summary>
    /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Store.</param>
    /// <param name="username">Username to use for requests that require authentication.</param>
    /// <param name="password">Password to use for requests that require authentication.</param>
    public SesameServer(string baseUri, string username, string password) 
        : this(baseUri, username, password, null) { }

    /// <summary>
    /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Store.</param>
    /// <param name="proxy">Proxy Server.</param>
    public SesameServer(string baseUri, IWebProxy proxy)
        : this(baseUri, null, null, proxy) { }

    /// <summary>
    /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Store.</param>
    /// <param name="username">Username to use for requests that require authentication.</param>
    /// <param name="password">Password to use for requests that require authentication.</param>
    /// <param name="proxy">Proxy Server.</param>
    public SesameServer(string baseUri, string username, string password, IWebProxy proxy)
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
        _username = username;
        _pwd = password;
        _hasCredentials = (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password));
        if (_hasCredentials) SetCredentials(username, password);
        Proxy = proxy;
    }

    /// <summary>
    /// Gets the IO Behaviour of the server.
    /// </summary>
    public IOBehaviour IOBehaviour => IOBehaviour.StorageServer;


    /// <summary>
    /// Gets a default template for creating a store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public virtual IStoreTemplate GetDefaultTemplate(string id)
    {
        return new SesameMemTemplate(id);
    }

    /// <summary>
    /// Gets all available templates for creating a store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <returns></returns>
    public virtual IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
    {
        var templates = new List<IStoreTemplate>();
        object[] args = { id };
        foreach (Type t in TemplateTypes)
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
    /// Creates a new Store based on the given template.
    /// </summary>
    /// <param name="template">Template.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Templates must inherit from <see cref="BaseSesameTemplate"/>.
    /// </para>
    /// </remarks>
    public virtual bool CreateStore(IStoreTemplate template)
    {
        if (!(template is BaseSesameTemplate sesameTemplate))
        {
            throw new RdfStorageException("Invalid template, templates must derive from BaseSesameTemplate");
        }

        try
        {
            var createParams = new Dictionary<string, string>();
            if (template.Validate().Any())
                throw new RdfStorageException(
                    "Template is not valid, call Validate() on the template to see the list of errors");
            IGraph g = sesameTemplate.GetTemplateGraph();

            // Firstly we need to save the Repository Template as a new Context to Sesame
            createParams.Add("context", sesameTemplate.ContextNode.ToString());
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + SystemRepositoryID + "/statements",
                "*/*", HttpMethod.Post, createParams);
            var ntWriter = new NTriplesWriter();
            request.Content = new GraphContent(g, ntWriter);

            using (HttpResponseMessage response = HttpClient.SendAsync(request).Result)
            {
                // If we get then it was OK
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "creating a new store in");
                }
            }

            // Then we need to declare that said Context is of type rep:RepositoryContext
            var repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"),
                g.CreateUriNode("rep:RepositoryContext"));
            EnsureSystemConnection();
            _sysConnection.UpdateGraph(string.Empty, repoType.AsEnumerable(), null);

            return true;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "creating a new Store in");
        }
    }

    /// <summary>
    /// Gets the Store with the given ID.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <returns></returns>
    /// <remarks>
    /// If the Store ID requested represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
    /// </remarks>
    public virtual IStorageProvider GetStore(string storeId)
    {
        return new SesameHttpProtocolConnector(_baseUri, storeId, _username, _pwd, Proxy);
    }

    /// <summary>
    /// Deletes the Store with the given ID.
    /// </summary>
    /// <param name="storeID">Store ID.</param>
    /// <remarks>
    /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation.
    /// </remarks>
    public virtual void DeleteStore(string storeID)
    {
        try
        {
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + storeID, MimeTypesHelper.Any, HttpMethod.Delete, new Dictionary<string, string>());

            using (var response = HttpClient.SendAsync(request).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "deleting the Store '" + storeID + "' from");
                }
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "deleting the Store '" + storeID + "' from");
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteStoreAsync(string storeId, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateRequest(_repositoriesPrefix + storeId, MimeTypesHelper.Any, HttpMethod.Delete, new Dictionary<string, string>());

        using HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(response, $"deleting the Store '{storeId}' from");
        }
    }

    /// <summary>
    /// Gets the list of available stores.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<string> ListStores()
    {
        try
        {
            HttpRequestMessage request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], HttpMethod.Get, new Dictionary<string, string>());

            var handler = new ListStringsHandler("id");
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "listing Stores from");
            }

            var parser = new SparqlXmlParser();
            parser.Load(handler, new StreamReader(response.Content.ReadAsStreamAsync().Result));
            return handler.Strings;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Stores from");
        }
    }

    /// <summary>
    /// Gets a default template for creating a store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <returns></returns>
    public virtual void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
    {
        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new SesameMemTemplate(id)), state);
    }

    /// <inheritdoc />
    public Task<IStoreTemplate> GetDefaultTemplateAsync(string id, CancellationToken cancellationToken)
    {
        return Task.FromResult<IStoreTemplate>(new SesameMemTemplate(id));
    }

    /// <summary>
    /// Gets all available templates for creating a store.
    /// </summary>
    /// <param name="id">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <returns></returns>
    public virtual void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
    {
        var templates = new List<IStoreTemplate>();
        object[] args = { id };
        foreach (Type t in TemplateTypes)
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

    /// <inheritdoc />
    public Task<IEnumerable<IStoreTemplate>> GetAvailableTemplatesAsync(string id, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => GetAvailableTemplates(id), cancellationToken);
    }

    /// <summary>
    /// Creates a new store based on the given template.
    /// </summary>
    /// <param name="template">Template.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <remarks>
    /// <para>
    /// Template must inherit from <see cref="BaseSesameTemplate"/>.
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
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + SystemRepositoryID + "/statements", "*/*", HttpMethod.Post, createParams);
            request.Content = new GraphContent(g, MimeTypesHelper.NTriples[0]);
            EnsureSystemConnection();
            _sysConnection.SaveGraphAsync(request, g, (sender, args, st) =>
            {
                if (args.WasSuccessful)
                {
                    // Then we need to declare that said Context is of type rep:RepositoryContext
                    var repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                    _sysConnection.UpdateGraph(string.Empty, repoType.AsEnumerable(), null, (sender2, args2, st2) =>
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

    /// <inheritdoc />
    public async Task<string> CreateStoreAsync(IStoreTemplate template, CancellationToken cancellationToken)
    {
        if (!(template is BaseSesameTemplate sesameTemplate))
        {
            throw new RdfStorageException("Invalid template. Templates must derive from BaseSesameTemplate.");
        }

        if (template.Validate().Any())
        {
            throw new RdfStorageException("Template is not valid. Call Validate() on the template to see the list of errors.");
        }

        try
        {
            IGraph g = sesameTemplate.GetTemplateGraph();
            var createParams =
                new Dictionary<string, string>() {{"context", sesameTemplate.ContextNode.ToString()}};
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + SystemRepositoryID + "/statements",
                "*/*", HttpMethod.Post, createParams);
            request.Content = new GraphContent(g, MimeTypesHelper.NTriples[0]);
            EnsureSystemConnection();
            await _sysConnection.SaveGraphAsync(request, cancellationToken);
            var repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"),
                g.CreateUriNode("rep:RepositoryContext"));
            await _sysConnection.UpdateGraphAsync(string.Empty, repoType.AsEnumerable(), null, cancellationToken);
            return template.ID;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "creating a new store in");
        }
    }

    /// <summary>
    /// Gets a store asynchronously.
    /// </summary>
    /// <param name="storeId">Store ID.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <remarks>
    /// If the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself.
    /// </remarks>
    public virtual void GetStore(string storeId, AsyncStorageCallback callback, object state)
    {
        try
        {
            IAsyncStorageProvider provider = new SesameHttpProtocolConnector(_baseUri, storeId, _username, _pwd, Proxy);
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, provider), state);
        }
        catch (Exception e)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeId, e), state);
        }
    }

    /// <inheritdoc/>
    public virtual Task<IAsyncStorageProvider> GetStoreAsync(string storeId, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            new SesameHttpProtocolConnector(_baseUri, storeId, _username, _pwd, Proxy) as IAsyncStorageProvider);
    }

    /// <summary>
    /// Deletes a store asynchronously.
    /// </summary>
    /// <param name="storeID">ID of the store to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void DeleteStore(string storeID, AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + storeID, MimeTypesHelper.Any, HttpMethod.Delete, new Dictionary<string, string>());
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    $"deleting the store {storeID} from")),
                        state);
                }
                else
                {
                    using HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore,
                                StorageHelper.HandleHttpError(response,
                                    $"deleting the store {storeID} from")),
                            state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore), state);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "deleting the Store '" + storeID + "' asynchronously from");
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListStoresAsync(CancellationToken cancellationToken)
    {
        try
        {
            HttpRequestMessage request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0],
                HttpMethod.Get, new Dictionary<string, string>());
            var handler = new ListStringsHandler("id");
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "listing stores from");
            }

            Stream data = await response.Content.ReadAsStreamAsync();
            var parser = new SparqlXmlParser();
            parser.Load(handler, new StreamReader(data));
            return handler.Strings;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing stores from");
        } 
    }

    /// <summary>
    /// Lists the available stores asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future release. Replaced by ListStoresAsync")]
    public virtual void ListStores(AsyncStorageCallback callback, object state)
    {
        HttpRequestMessage request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], HttpMethod.Get, new Dictionary<string, string>());
        var handler = new ListStringsHandler("id");
        try
        {
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled")
                                : StorageHelper.HandleError(requestTask.Exception, "listing stores from")),
                        state);
                }
                else
                {
                    using HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                StorageHelper.HandleHttpError(response, "listing stores from")),
                            state);
                    }
                    else
                    {
                        response.Content.ReadAsStreamAsync().ContinueWith(readTask =>
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled")
                                            : StorageHelper.HandleError(readTask.Exception, "listing stores from")),
                                    state);
                            }
                            else
                            {
                                var parser = new SparqlXmlParser();
                                parser.Load(handler, new StreamReader(readTask.Result));
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, handler.Strings),
                                    state);
                            }
                        });
                    }
                }
            });
        }
        catch (Exception ex)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores from")), state);
        }
    }

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="queryParams">Querystring Parameters.</param>
    /// <returns></returns>
    [Obsolete("This method is obsolete and will be removed in a future release.")]
    protected virtual HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> queryParams)
    {
        // Build the Request Uri
        var requestUri = _baseUri + servicePath;
        if (queryParams != null)
        {
            if (queryParams.Count > 0)
            {
                requestUri += "?";
                foreach (var p in queryParams.Keys)
                {
                    requestUri += p + "=" + HttpUtility.UrlEncode(queryParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }
        }

        // Create our Request
        var request = (HttpWebRequest)WebRequest.Create(requestUri);
        request.Accept = accept;
        request.Method = method;

        // Add Credentials if needed
        if (_hasCredentials)
        {
            var credentials = new NetworkCredential(_username, _pwd);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
        }

        //return ApplyRequestOptions(request);
        return request;
    }

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="queryParams">Querystring Parameters.</param>
    /// <returns></returns>
    protected virtual HttpRequestMessage CreateRequest(string servicePath, string accept, HttpMethod method, Dictionary<string, string> queryParams)
    {
        // Build the Request Uri
        var requestUri = _baseUri + servicePath;
        if (queryParams != null)
        {
            if (queryParams.Count > 0)
            {
                requestUri += "?";
                foreach (var p in queryParams.Keys)
                {
                    requestUri += p + "=" + HttpUtility.UrlEncode(queryParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }
        }

        // Create our Request
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add("Accept", accept);
        return request;
    }


    /// <summary>
    /// Ensures the connection to the Sesame SYSTEM repository is prepared if it isn't already.
    /// </summary>
    protected virtual void EnsureSystemConnection()
    {
        if (_sysConnection == null)
        {
            _sysConnection = new SesameHttpProtocolConnector(_baseUri, SystemRepositoryID, _username, _pwd, Proxy);
        }
    }

    /// <summary>
    /// Disposes of the server.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sysConnection?.Dispose();
        }
        base.Dispose(disposing);
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
