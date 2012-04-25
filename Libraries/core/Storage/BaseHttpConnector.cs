/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
using System.Linq;
using System.Net;
using VDS.RDF.Parsing;
using VDS.RDF.Configuration;
using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for HTTP based <see cref="IGenericIOManager">IGenericIOManager</see> implementations
    /// </summary>
    /// <remarks>
    /// <para>
    /// Does not actually implement the interface rather it provides common functionality around HTTP Proxying
    /// </para>
    /// <para>
    /// If the library is compiled with the NO_PROXY symbol then this code adds no functionality
    /// </para>
    /// </remarks>
    public abstract class BaseHttpConnector
        : IAsyncStorageProvider
    {
#if !NO_PROXY
        private WebProxy _proxy;
        
        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(String address)
        {
            this._proxy = new WebProxy(address);
        }

        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(Uri address)
        {
            this._proxy = new WebProxy(address);
        }

        /// <summary>
        /// Gets/Sets a Proxy Server to be used
        /// </summary>
        public WebProxy Proxy
        {
            get
            {
                return this._proxy;
            }
            set
            {
                this._proxy = value;
            }
        }

        /// <summary>
        /// Clears any in-use credentials so subsequent requests will not use a proxy server
        /// </summary>
        public void ClearProxy()
        {
            this._proxy = null;
        }

        /// <summary>
        /// Sets Credentials to be used for Proxy Server
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public void SetProxyCredentials(String username, String password)
        {
            if (this._proxy != null)
            {
                this._proxy.Credentials = new NetworkCredential(username, password);
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
            if (this._proxy != null)
            {
                this._proxy.Credentials = new NetworkCredential(username, password, domain);
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
                if (this._proxy != null)
                {
                    return this._proxy.Credentials;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this._proxy != null)
                {
                    this._proxy.Credentials = value;
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
            if (this._proxy != null)
            {
                this._proxy.Credentials = null;
            }
        }

#endif

        /// <summary>
        /// Adds Proxy Server to requests if used
        /// </summary>
        /// <param name="request">HTTP Web Request</param>
        /// <returns></returns>
        protected HttpWebRequest GetProxiedRequest(HttpWebRequest request)
        {
#if !NO_PROXY
            if (this._proxy != null)
            {
                request.Proxy = this._proxy;
            }
#endif
            return request;
        }

        /// <summary>
        /// Helper method which adds proxy configuration to serialization
        /// </summary>
        /// <param name="objNode">Object Node representing the <see cref="IGenericIOManager">IGenericIOManager</see> whose configuration is being serialized</param>
        /// <param name="context">Serialization Context</param>
        protected void SerializeProxyConfig(INode objNode, ConfigurationSerializationContext context)
        {
#if !NO_PROXY
            if (this._proxy != null)
            {
                INode proxy = context.NextSubject;
                INode usesProxy = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyProxy);
                INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                INode proxyType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassProxy);
                INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
                INode user = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);

                context.Graph.Assert(new Triple(objNode, usesProxy, proxy));
                context.Graph.Assert(new Triple(proxy, rdfType, proxyType));
                context.Graph.Assert(new Triple(proxy, server, context.Graph.CreateLiteralNode(this._proxy.Address.ToString())));

                if (this._proxy.Credentials is NetworkCredential)
                {
                    NetworkCredential cred = (NetworkCredential)this._proxy.Credentials;
                    context.Graph.Assert(new Triple(proxy, user, context.Graph.CreateLiteralNode(cred.UserName)));
                    context.Graph.Assert(new Triple(proxy, pwd, context.Graph.CreateLiteralNode(cred.Password)));
                }
            }
#endif
        }

        public virtual void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            this.LoadGraph(g, graphUri.ToSafeString(), callback, state);
        }

        public virtual void LoadGraph(IGraph g, String graphUri, AsyncStorageCallback callback, Object state)
        {
            this.LoadGraph(new GraphHandler(g), graphUri, (sender, args, st) =>
                {
                    callback(sender, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadGraph, g, args.Error), st);
                }, state);
        }

        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            this.LoadGraph(handler, graphUri.ToSafeString(), callback, state);
        }

        public abstract void LoadGraph(IRdfHandler handler, String graphUri, AsyncStorageCallback callback, Object state);

        protected void LoadGraphAsync(HttpWebRequest request, IRdfHandler handler, AsyncStorageCallback callback, Object state)
        {
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
                    //Parse the retrieved RDF
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));

                    //If we get here then it was OK
                    response.Close();

                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler), state);
                }
                catch (WebException webEx)
                {
                    if (webEx.Response != null)
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                        }
#endif
                    }
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler, new RdfStorageException("A HTTP Error occurred while trying to load a Graph from the Store", webEx)), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler, new RdfStorageException("Unexpected error trying to load a graph from the store asynchronously, see inner exception for details", ex)), state);
                }
            }, state);
        }

        public abstract void SaveGraph(IGraph g, AsyncStorageCallback callback, Object state);

        protected void SaveGraphAsync(HttpWebRequest request, IRdfWriter writer, IGraph g, AsyncStorageCallback callback, Object state)
        {
            request.BeginGetRequestStream(r =>
            {
                try
                {
                    Stream reqStream = request.EndGetRequestStream(r);
                    writer.Save(g, new StreamWriter(reqStream));

#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
#endif
                    
                    request.BeginGetResponse(r2 =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            //If we get here then it was OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g), state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            }
#endif
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g, new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store asynchronously", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g, new RdfStorageException("Unexpected Error trying to save the Graph to the store asynchronously, see inner exception for details", ex)), state);
                        }
                    }, state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g, new RdfStorageException("Unexpected error writing the Graph to the store asynchronously, see inner exception for details", ex)), state);
                }
            }, state);
        }

        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals, callback, state);
        }

        public abstract void UpdateGraph(String graphUri, System.Collections.Generic.IEnumerable<Triple> additions, System.Collections.Generic.IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        protected void UpdateGraphAsync(HttpWebRequest request, IRdfWriter writer, Uri graphUri, IEnumerable<Triple> ts, AsyncStorageCallback callback, Object state)
        {
            Graph g = new Graph();
            g.Assert(ts);

            request.BeginGetRequestStream(r =>
            {
                try
                {
                    Stream reqStream = request.EndGetRequestStream(r);
                    writer.Save(g, new StreamWriter(reqStream));

#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
#endif

                    request.BeginGetResponse(r2 =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            //If we get here then it was OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri), state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            }
#endif
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri, new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store asynchronously", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri, new RdfStorageException("Unexpected error while trying to update a Graph in the Store asynchronously, see inner exception for details", ex)), state);
                        }
                    }, state);
                }
                catch (WebException webEx)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri, new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store asynchronously", webEx)), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri, new RdfStorageException("Unexpected error while trying to update a Graph in the Store asynchronously, see inner exception for details", ex)), state);
                }
            }, state);
        }

        public virtual void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            this.DeleteGraph(graphUri.ToSafeString(), callback, state);
        }

        public abstract void DeleteGraph(String graphUri, AsyncStorageCallback callback, Object state);

        protected void DeleteGraphAsync(HttpWebRequest request, bool allow404, String graphUri, AsyncStorageCallback callback, Object state)
        {
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

                        //Assume if returns to here we deleted the Graph OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri.ToSafeUri()), state);
                    }
                    catch (WebException webEx)
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                        }
#endif
                        //Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                        if (webEx.Response == null || (webEx.Response != null && (!allow404 || ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound)))
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store asynchronously", webEx)), state);
                        }
                        else
                        {
                            //Consider a 404 as a success in some cases
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri.ToSafeUri()), state);
                        }
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("Unexpected error while trying to delete a Graph from the Store asynchronously, see inner exception for details", ex)), state);
                    }
                }, state);
        }

        public virtual void ListGraphs(AsyncStorageCallback callback, object state)
        {
            if (this is IAsyncQueryableStorage)
            {
                //Use ListUrisHandler and make an async query to list the graphs, when that returns we invoke the correct callback
                ListUrisHandler handler = new ListUrisHandler("g");
                ((IAsyncQueryableStorage)this).Query(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", (sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.ListGraphs, handler.Uris), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.ListGraphs, args.Error), state);
                    }
                }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.ListGraphs, new RdfStorageException("Underlying store does not supported listing graphs asynchronously or has failed to appropriately override this method")), state);
            }
        }

        public abstract bool IsReady
        {
            get;
        }

        public abstract bool IsReadOnly
        {
            get;
        }

        public abstract IOBehaviour IOBehaviour
        {
            get;
        }

        public abstract bool UpdateSupported
        {
            get;
        }

        public abstract bool DeleteSupported
        {
            get;
        }

        public abstract bool ListGraphsSupported
        {
            get;
        }

        public abstract void Dispose();
    }
}
