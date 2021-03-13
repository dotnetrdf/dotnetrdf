/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for HTTP based Storage API implementations.
    /// </summary>
    public abstract class BaseHttpConnector
    {
        /// <summary>
        /// Creates a new connector.
        /// </summary>
        protected BaseHttpConnector()
        {
            Timeout = 30000;
        }

        /// <summary>
        /// Whether the User has provided credentials for accessing the Store using authentication.
        /// </summary>
        private bool _hasCredentials;

        /// <summary>
        /// Sets a Proxy Server to be used.
        /// </summary>
        /// <param name="address">Proxy Address.</param>
        public void SetProxy(string address)
        {
            Proxy = new WebProxy(address);
        }

        /// <summary>
        /// Sets a Proxy Server to be used.
        /// </summary>
        /// <param name="address">Proxy Address.</param>
        public void SetProxy(Uri address)
        {
            Proxy = new WebProxy(address);
        }

        /// <summary>
        /// Gets/Sets a Proxy Server to be used.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Clears any in-use credentials so subsequent requests will not use a proxy server.
        /// </summary>
        public void ClearProxy()
        {
            Proxy = null;
        }

        /// <summary>
        /// Sets Credentials to be used for Proxy Server.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void SetProxyCredentials(string username, string password)
        {
            if (Proxy != null)
            {
                Proxy.Credentials = new NetworkCredential(username, password);
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
            if (Proxy != null)
            {
                Proxy.Credentials = new NetworkCredential(username, password, domain);
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
        public int Timeout { get; set; }

        /// <summary>
        /// Password for accessing the Store.
        /// </summary>
        protected string Username { get; private set; }

        /// <summary>
        /// Password for accessing the Store.
        /// </summary>
        protected string Password { get; private set; }

        /// <summary>
        /// Helper method which applies standard request options to the request, these currently include proxy settings and HTTP timeout.
        /// </summary>
        /// <param name="request">HTTP Web Request.</param>
        /// <returns>HTTP Web Request with standard options applied.</returns>
        protected HttpWebRequest ApplyRequestOptions(HttpWebRequest request)
        {
            if (Timeout > 0) request.Timeout = Timeout;
            if (Proxy != null)
            {
                request.Proxy = Proxy;
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
        /// Helper method which adds standard configuration information (proxy and timeout settings) to serialized configuration.
        /// </summary>
        /// <param name="objNode">Object Node representing the <see cref="IStorageProvider">IStorageProvider</see> whose configuration is being serialized.</param>
        /// <param name="context">Serialization Context.</param>
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
            if (Proxy == null) return;
            INode proxy = context.NextSubject;
            INode usesProxy = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProxy));
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode proxyType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassProxy));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode user = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));

            context.Graph.Assert(new Triple(objNode, usesProxy, proxy));
            context.Graph.Assert(new Triple(proxy, rdfType, proxyType));
            context.Graph.Assert(new Triple(proxy, server, context.Graph.CreateLiteralNode((Proxy as WebProxy).Address.AbsoluteUri)));

            if (!(Proxy.Credentials is NetworkCredential)) return;
            NetworkCredential cred = (NetworkCredential)Proxy.Credentials;
            context.Graph.Assert(new Triple(proxy, user, context.Graph.CreateLiteralNode(cred.UserName)));
            context.Graph.Assert(new Triple(proxy, pwd, context.Graph.CreateLiteralNode(cred.Password)));
        }

        public void SetCredentials(string username, string password)
        {
            Username = username;
            Password = password;
            _hasCredentials = (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password));
        }
    }
}
