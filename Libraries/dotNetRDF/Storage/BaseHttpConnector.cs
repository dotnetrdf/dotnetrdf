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
using System.Text;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for HTTP based Storage API implementations
    /// </summary>
    /// <remarks>
    /// <para>
    /// As .NET Core does not currently provide an implementaton of the System.Net.WebProxy class, this implementation
    /// is restricted in functionality. Many of the methods it exposes will raise a PlatformNotSupported exception,
    /// but are currently required to enable dependent classes to compile. As such it is strongly recommended that
    /// developers targeting .NET Core should NOT plan on making any use of the proxy capabilities of this class.
    /// </para>
    /// </remarks>
    public abstract class BaseHttpConnector
    {
        /// <summary>
        /// Creates a new connector
        /// </summary>
        protected BaseHttpConnector()
        {
            Timeout = 30000;
        }

        /// <summary>
        /// Whether the User has provided credentials for accessing the Store using authentication
        /// </summary>
        private bool _hasCredentials;

        private IWebProxy _proxy;

        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(String address)
        {
            _proxy = new WebProxy(address);
        }

        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(Uri address)
        {
            _proxy = new WebProxy(address);
        }

        /// <summary>
        /// Gets/Sets a Proxy Server to be used
        /// </summary>
        public IWebProxy Proxy
        {
            get
            {
                return _proxy;
            }
            set
            {
                _proxy = value;
            }
        }

        /// <summary>
        /// Clears any in-use credentials so subsequent requests will not use a proxy server
        /// </summary>
        public void ClearProxy()
        {
            _proxy = null;
        }

        /// <summary>
        /// Sets Credentials to be used for Proxy Server
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public void SetProxyCredentials(String username, String password)
        {
            if (_proxy != null)
            {
                _proxy.Credentials = new NetworkCredential(username, password);
            }
            else
            {
                throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
            }
        }

        /// <summary>
        /// Sets Credentials to be used for Proxy Server
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="domain">Domain</param>
        public void SetProxyCredentials(String username, String password, String domain)
        {
            if (_proxy != null)
            {
                _proxy.Credentials = new NetworkCredential(username, password, domain);
            }
            else
            {
                throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
            }
        }

        /// <summary>
        /// Gets/Sets Credentials to be used for Proxy Server
        /// </summary>
        public ICredentials ProxyCredentials
        {
            get
            {
                if (_proxy != null)
                {
                    return _proxy.Credentials;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_proxy != null)
                {
                    _proxy.Credentials = value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
                }
            }
        }

        /// <summary>
        /// Clears the in-use proxy credentials so subsequent requests still use the proxy server but without credentials
        /// </summary>
        public void ClearProxyCredentials()
        {
            if (_proxy != null)
            {
                _proxy.Credentials = null;
            }
        }

        /// <summary>
        /// Gets/Sets the HTTP Timeouts used specified in milliseconds
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defaults to 30 seconds (i.e. the default value is 30,000)
        /// </para>
        /// <para>
        /// It is important to understand that this timeout only applies to the HTTP request portions of any operation performed and that the timeout may apply more than once if a POST operation is used since the timeout applies separately to obtaining the request stream to POST the request and obtaining the response stream.  Also the timeout does not in any way apply to subsequent work that may be carried out before the operation can return so if you need a hard timeout on an operation you should manage that yourself.
        /// </para>
        /// <para>
        /// When set to a zero/negative value then the standard .Net timeout of 100 seconds will apply, use <see cref="int.MaxValue"/> if you want the maximum possible timeout i.e. if you expect to launch extremely long running operations.
        /// </para>
        /// <para>
        /// Not supported under Silverlight, Windows Phone and Portable Class Library builds
        /// </para>
        /// </remarks>
        public int Timeout { get; set; }

        /// <summary>
        /// Password for accessing the Store
        /// </summary>
        protected string Username { get; private set; }

        /// <summary>
        /// Password for accessing the Store
        /// </summary>
        protected string Password { get; private set; }

        /// <summary>
        /// Helper method which applies standard request options to the request, these currently include proxy settings and HTTP timeout
        /// </summary>
        /// <param name="request">HTTP Web Request</param>
        /// <returns>HTTP Web Request with standard options applied</returns>
        protected HttpWebRequest ApplyRequestOptions(HttpWebRequest request)
        {
            if (Timeout > 0) request.Timeout = Timeout;
            if (_proxy != null)
            {
                request.Proxy = _proxy;
            }

            // Add Credentials if needed
            if (_hasCredentials)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Username + ":" + this.Password));
                    request.Headers["Authorization"] = "Basic " + credentials;
                }
                else
                {
                    // Leave .Net to cope with HTTP auth challenge response
                    NetworkCredential credentials = new NetworkCredential(Username, Password);
                    request.Credentials = credentials;
                    request.PreAuthenticate = true;
                }
            }
            // Disable Keep Alive since it can cause errors when carrying out high volumes of operations or when performing long running operations
            request.KeepAlive = false;
            return request;
        }

        /// <summary>
        /// Helper method which adds standard configuration information (proxy and timeout settings) to serialized configuration
        /// </summary>
        /// <param name="objNode">Object Node representing the <see cref="IStorageProvider">IStorageProvider</see> whose configuration is being serialized</param>
        /// <param name="context">Serialization Context</param>
        protected void SerializeStandardConfig(INode objNode, ConfigurationSerializationContext context)
        {
            // Basic Authentication
            if (Username != null && Password != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode password = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(objNode, username, context.Graph.CreateLiteralNode(Username)));
                context.Graph.Assert(new Triple(objNode, password, context.Graph.CreateLiteralNode(Password)));
            }

            // Timeout
            if (Timeout > 0)
            {
                INode timeout = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyTimeout));
                context.Graph.Assert(new Triple(objNode, timeout, Timeout.ToLiteral(context.Graph)));
            }

            // Proxy configuration
            if (_proxy == null) return;
            INode proxy = context.NextSubject;
            INode usesProxy = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProxy));
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode proxyType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassProxy));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode user = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));

            context.Graph.Assert(new Triple(objNode, usesProxy, proxy));
            context.Graph.Assert(new Triple(proxy, rdfType, proxyType));
            context.Graph.Assert(new Triple(proxy, server, context.Graph.CreateLiteralNode((_proxy as WebProxy).Address.AbsoluteUri)));

            if (!(_proxy.Credentials is NetworkCredential)) return;
            NetworkCredential cred = (NetworkCredential)_proxy.Credentials;
            context.Graph.Assert(new Triple(proxy, user, context.Graph.CreateLiteralNode(cred.UserName)));
            context.Graph.Assert(new Triple(proxy, pwd, context.Graph.CreateLiteralNode(cred.Password)));
        }

        public void SetCredentials(string username, string password)
        {
            Username = username;
            Password = password;
            _hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
        }
    }

    /// <summary>
    /// Abstract Base Class for HTTP Based <see cref="IAsyncStorageProvider">IAsyncStorageProvider</see> implementations
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is expected that most classes extending from this will also then implement <see cref="IStorageProvider"/> separately for their synchronous communication, this class purely provides partial helper implementations for the asynchronous communication
    /// </para>
    /// </remarks>
    public abstract class BaseAsyncHttpConnector
        : BaseHttpConnector, IAsyncStorageProvider
    {
        private readonly DoRequestSequenceDelgate _d;

        /// <summary>
        /// Creates a new Base Async HTTP Connector
        /// </summary>
        protected BaseAsyncHttpConnector()
        {
            _d = new DoRequestSequenceDelgate(DoRequestSequence);
        }

        /// <summary>
        /// Gets the parent server (if any)
        /// </summary>
        public virtual IStorageServer ParentServer
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the parent server (if any)
        /// </summary>
        public virtual IAsyncStorageServer AsyncParentServer
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            LoadGraph(g, graphUri.ToSafeString(), callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void LoadGraph(IGraph g, String graphUri, AsyncStorageCallback callback, Object state)
        {
            LoadGraph(new GraphHandler(g), graphUri, (sender, args, st) => callback(sender, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g, args.Error), st), state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            LoadGraph(handler, graphUri.ToSafeString(), callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public abstract void LoadGraph(IRdfHandler handler, String graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Helper method for doing async load operations, callers just need to provide an appropriately prepared HTTP request
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <param name="handler">Handler to load with</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected internal void LoadGraphAsync(HttpWebRequest request, IRdfHandler handler, AsyncStorageCallback callback, Object state)
        {
            Tools.HttpDebugRequest(request);
            request.BeginGetResponse(r =>
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                    Tools.HttpDebugResponse(response);
                    // Parse the retrieved RDF
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));

                    // If we get here then it was OK
                    response.Close();

                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                }
                catch (WebException webEx)
                {
                    if (webEx.Response != null)
                    {
                        Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                        if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                            return;
                        }
                    }
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler, new RdfStorageException("A HTTP Error occurred while trying to load a Graph from the Store", webEx)), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler, StorageHelper.HandleError(ex, "loading a Graph asynchronously from")), state);
                }
            }, state);
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public abstract void SaveGraph(IGraph g, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Helper method for doing async save operations, callers just need to provide an appropriately perpared HTTP requests and a RDF writer which will be used to write the data to the request body
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <param name="writer">RDF Writer</param>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected internal void SaveGraphAsync(HttpWebRequest request, IRdfWriter writer, IGraph g, AsyncStorageCallback callback, Object state)
        {
            request.BeginGetRequestStream(r =>
            {
                try
                {
                    Stream reqStream = request.EndGetRequestStream(r);
                    writer.Save(g, new StreamWriter(reqStream));

                    Tools.HttpDebugRequest(request);
                    
                    request.BeginGetResponse(r2 =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                            Tools.HttpDebugResponse(response);
                            // If we get here then it was OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                        }
                    }, state);
                }
                catch (WebException webEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                }
            }, state);
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            UpdateGraph(graphUri.ToSafeString(), additions, removals, callback, state);
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public abstract void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Helper method for doing async update operations, callers just need to provide an appropriately prepared HTTP request and a RDF writer which will be used to write the data to the request body
        /// </summary>
        /// <param name="request">HTTP Request</param>
        /// <param name="writer">RDF writer</param>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="ts">Triples</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected internal void UpdateGraphAsync(HttpWebRequest request, IRdfWriter writer, Uri graphUri, IEnumerable<Triple> ts, AsyncStorageCallback callback, Object state)
        {
            Graph g = new Graph();
            g.Assert(ts);

            request.BeginGetRequestStream(r =>
            {
                try
                {
                    Stream reqStream = request.EndGetRequestStream(r);
                    writer.Save(g, new StreamWriter(reqStream));

                    Tools.HttpDebugRequest(request);

                    request.BeginGetResponse(r2 =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                            Tools.HttpDebugResponse(response);
                            // If we get here then it was OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                        }
                    }, state);
                }
                catch (WebException webEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                }
            }, state);
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            DeleteGraph(graphUri.ToSafeString(), callback, state);
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public abstract void DeleteGraph(String graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Helper method for doing async delete operations, callers just need to provide an appropriately prepared HTTP request
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <param name="allow404">Whether a 404 response counts as success</param>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected internal void DeleteGraphAsync(HttpWebRequest request, bool allow404, String graphUri, AsyncStorageCallback callback, Object state)
        {
            Tools.HttpDebugRequest(request);
            request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);

                        // Assume if returns to here we deleted the Graph OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                        // Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                        if (webEx.Response == null || (webEx.Response != null && (!allow404 || ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound)))
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store asynchronously", webEx)), state);
                        }
                        else
                        {
                            // Consider a 404 as a success in some cases
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                        }
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
                    }
                }, state);
        }

        /// <summary>
        /// Lists the Graphs in the Store asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void ListGraphs(AsyncStorageCallback callback, object state)
        {
            if (this is IAsyncQueryableStorage)
            {
                // Use ListUrisHandler and make an async query to list the graphs, when that returns we invoke the correct callback
                ListUrisHandler handler = new ListUrisHandler("g");
                ((IAsyncQueryableStorage)this).Query(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", (sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, handler.Uris), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, args.Error), state);
                    }
                }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, new RdfStorageException("Underlying store does not supported listing graphs asynchronously or has failed to appropriately override this method")), state);
            }
        }

        /// <summary>
        /// Indicates whether the Store is ready to accept requests
        /// </summary>
        public abstract bool IsReady
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store is read only
        /// </summary>
        public abstract bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public abstract IOBehaviour IOBehaviour
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports Triple level updates via the <see cref="BaseAsyncHttpConnector.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple},AsyncStorageCallback,Object)">UpdateGraph()</see> method
        /// </summary>
        public abstract bool UpdateSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports Graph deletion via the <see cref="BaseAsyncHttpConnector.DeleteGraph(Uri, AsyncStorageCallback, Object)">DeleteGraph()</see> method
        /// </summary>
        public abstract bool DeleteSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports listing graphs via the <see cref="BaseAsyncHttpConnector.ListGraphs(AsyncStorageCallback, Object)">ListGraphs()</see> method
        /// </summary>
        public abstract bool ListGraphsSupported
        {
            get;
        }

        /// <summary>
        /// Diposes of the Store
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Helper method for doing async operations where a sequence of HTTP requests must be run
        /// </summary>
        /// <param name="requests">HTTP requests</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected internal void MakeRequestSequence(IEnumerable<HttpWebRequest> requests, AsyncStorageCallback callback, Object state)
        {
            _d.BeginInvoke(requests, callback, state, MakeRequestSequenceCallback, callback);
        }

        private void MakeRequestSequenceCallback(IAsyncResult r)
        {
            AsyncStorageCallback callback = r.AsyncState as AsyncStorageCallback;
            try
            {
                _d.EndInvoke(r);
                // callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown), null);
            }
            catch (Exception ex)
            {
                if (callback != null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown, new RdfStorageException("Unexpected error while making a sequence of asynchronous requests to the Store, see inner exception for details", ex)), null);
                }
            }
        }

        private delegate void DoRequestSequenceDelgate(IEnumerable<HttpWebRequest> requests, AsyncStorageCallback callback, Object state);

        private void DoRequestSequence(IEnumerable<HttpWebRequest> requests, AsyncStorageCallback callback, Object state)
        {
            ManualResetEvent signal = new ManualResetEvent(false);
            foreach (HttpWebRequest request in requests)
            {
                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);

                        // This request worked OK, close the response and carry on
                        response.Close();
                        signal.Set();
                    }
                    catch (WebException webEx)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown, new RdfStorageException("A HTTP Error occurred while making a sequence of asynchronous requests to the Store, see inner exception for details", webEx)), state);
                        return;
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown, new RdfStorageException("Unexpected error while making a sequence of asynchronous requests to the Store, see inner exception for details", ex)), state);
                        return;
                    }
                }, state);

                signal.WaitOne();
                signal.Reset();
            }

            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown), state);
        }
    }
}
