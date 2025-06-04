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
using System.Net;
using System.Net.Http;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage;

/// <summary>
/// Abstract Base Class for HTTP based Storage API implementations.
/// </summary>
public abstract class BaseHttpConnector : IDisposable
{
    /// <summary>
    /// Get the HTTP client to be used.
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    /// Get the handler used by the HTTP client.
    /// </summary>
    protected HttpClientHandler HttpClientHandler { get; }

    /// <summary>
    /// Creates a new connector.
    /// </summary>
    protected BaseHttpConnector()
    {
        HttpClientHandler = new HttpClientHandler();
        HttpClient = new HttpClient(HttpClientHandler);
    }

    /// <summary>
    /// Creates a new connector.
    /// </summary>
    /// <param name="clientHandler">The handler to use for HTTP client connections.</param>
    protected BaseHttpConnector(HttpClientHandler clientHandler)
    {
        HttpClientHandler = clientHandler;
        HttpClient = new HttpClient(clientHandler);
    }

    /// <summary>
    /// Sets a Proxy Server to be used.
    /// </summary>
    /// <param name="address">Proxy Address.</param>
    public void SetProxy(string address)
    {
        HttpClientHandler.Proxy = new WebProxy(address);
        HttpClientHandler.UseProxy = true;
    }

    /// <summary>
    /// Sets a Proxy Server to be used.
    /// </summary>
    /// <param name="address">Proxy Address.</param>
    public void SetProxy(Uri address)
    {
        HttpClientHandler.Proxy = new WebProxy(address);
        HttpClientHandler.UseProxy = true;
    }

    /// <summary>
    /// Gets/Sets a Proxy Server to be used.
    /// </summary>
    public IWebProxy Proxy
    {
        get=>HttpClientHandler.Proxy;
        set
        {
            HttpClientHandler.Proxy = value;
            HttpClientHandler.UseProxy = value != null;
        }
    }

    /// <summary>
    /// Clears any in-use credentials so subsequent requests will not use a proxy server.
    /// </summary>
    public void ClearProxy()
    {
        HttpClientHandler.Proxy = null;
    }

    /// <summary>
    /// Sets Credentials to be used for Proxy Server.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    public void SetProxyCredentials(string username, string password)
    {
        if (HttpClientHandler.Proxy != null)
        {
            HttpClientHandler.Proxy.Credentials = new NetworkCredential(username, password);
        }
        else
        {
            throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
        }
    }

    /// <summary>
    /// Sets Credentials to be used for Proxy Server.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="domain">Domain.</param>
    public void SetProxyCredentials(string username, string password, string domain)
    {
        if (HttpClientHandler.Proxy != null)
        {
            HttpClientHandler.Proxy.Credentials = new NetworkCredential(username, password, domain);
        }
        else
        {
            throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
        }
    }

    /// <summary>
    /// Gets/Sets Credentials to be used for Proxy Server.
    /// </summary>
    public ICredentials ProxyCredentials
    {
        get => Proxy?.Credentials;
        set
        {
            if (Proxy != null)
            {
                Proxy.Credentials = value;
            }
            else
            {
                throw new InvalidOperationException("Cannot set Proxy Credentials when Proxy settings have not been provided");
            }
        }
    }

    /// <summary>
    /// Clears the in-use proxy credentials so subsequent requests still use the proxy server but without credentials.
    /// </summary>
    public void ClearProxyCredentials()
    {
        if (Proxy != null)
        {
            Proxy.Credentials = null;
        }
    }

    /// <summary>
    /// Gets/Sets the HTTP Timeouts used specified in milliseconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defaults to 30 seconds (i.e. the default value is 30,000).
    /// </para>
    /// <para>
    /// It is important to understand that this timeout only applies to the HTTP request portions of any operation performed and that the timeout may apply more than once if a POST operation is used since the timeout applies separately to obtaining the request stream to POST the request and obtaining the response stream.  Also the timeout does not in any way apply to subsequent work that may be carried out before the operation can return so if you need a hard timeout on an operation you should manage that yourself.
    /// </para>
    /// <para>
    /// When set to a zero/negative value then the standard .Net timeout of 100 seconds will apply, use <see cref="int.MaxValue"/> if you want the maximum possible timeout i.e. if you expect to launch extremely long running operations.
    /// </para>
    /// <para>
    /// Not supported under Silverlight, Windows Phone and Portable Class Library builds.
    /// </para>
    /// </remarks>
    public int Timeout
    {
        get => (int)HttpClient.Timeout.TotalMilliseconds;
        set => HttpClient.Timeout = TimeSpan.FromMilliseconds(value);
    }


    /// <summary>
    /// Helper method which adds standard configuration information (proxy and timeout settings) to serialized configuration.
    /// </summary>
    /// <param name="objNode">Object Node representing the <see cref="IStorageProvider">IStorageProvider</see> whose configuration is being serialized.</param>
    /// <param name="context">Serialization Context.</param>
    protected void SerializeStandardConfig(INode objNode, ConfigurationSerializationContext context)
    {
        // Basic Authentication
        if (HttpClientHandler.Credentials != null && HttpClientHandler.Credentials is NetworkCredential networkCredential)
        {
            INode username = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode password = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));
            context.Graph.Assert(new Triple(objNode, username, context.Graph.CreateLiteralNode(networkCredential.UserName)));
            context.Graph.Assert(new Triple(objNode, password, context.Graph.CreateLiteralNode(networkCredential.Password)));
        }

        // Timeout
        if (Timeout > 0)
        {
            INode timeout = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyTimeout));
            context.Graph.Assert(new Triple(objNode, timeout, Timeout.ToLiteral(context.Graph)));
        }

        // Proxy configuration
        if (Proxy == null) return;
        INode proxy = context.NextSubject;
        INode usesProxy = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyProxy));
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode proxyType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassProxy));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));
        INode user = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
        INode pwd = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));

        context.Graph.Assert(new Triple(objNode, usesProxy, proxy));
        context.Graph.Assert(new Triple(proxy, rdfType, proxyType));
        context.Graph.Assert(new Triple(proxy, server, context.Graph.CreateLiteralNode((Proxy as WebProxy).Address.AbsoluteUri)));

        if (!(Proxy.Credentials is NetworkCredential)) return;
        var cred = (NetworkCredential)Proxy.Credentials;
        context.Graph.Assert(new Triple(proxy, user, context.Graph.CreateLiteralNode(cred.UserName)));
        context.Graph.Assert(new Triple(proxy, pwd, context.Graph.CreateLiteralNode(cred.Password)));
    }

    /// <summary>
    /// Set the credentials for the connector to use for authenticating HTTP requests.
    /// </summary>
    /// <param name="username">The user name to use.</param>
    /// <param name="password">The password to use.</param>
    public void SetCredentials(string username, string password)
    {
        HttpClientHandler.Credentials = new NetworkCredential(username, password);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Perform cleanup of managed resources held by this class.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            HttpClient?.Dispose();
        }
    }
}
