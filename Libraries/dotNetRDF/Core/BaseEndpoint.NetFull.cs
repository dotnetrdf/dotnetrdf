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
using System.Net;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF
{    
    public abstract partial class BaseEndpoint
    {
        private bool _useCredentialsForProxy;

        #region Credentials and Proxy Server

        /// <summary>
        /// Controls whether the Credentials set with the <see cref="SetCredentials(string,string)">SetCredentials()</see> method or the <see cref="BaseEndpoint.Credentials">Credentials</see>are also used for a Proxy (if used)
        /// </summary>
        public bool UseCredentialsForProxy { get => _useCredentialsForProxy; set { _useCredentialsForProxy = value; } }


        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(string address)
        {
            Proxy = new WebProxy(address);
        }

        /// <summary>
        /// Sets a Proxy Server to be used
        /// </summary>
        /// <param name="address">Proxy Address</param>
        public void SetProxy(Uri address)
        {
            Proxy = new WebProxy(address);
        }

        /// <summary>
        /// Gets/Sets a Proxy Server to be used
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Clears any in-use credentials so subsequent requests will not use a proxy server
        /// </summary>
        public void ClearProxy()
        {
            Proxy = null;
        }

        /// <summary>
        /// Sets Credentials to be used for Proxy Server
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
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
        /// Sets Credentials to be used for Proxy Server
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="domain">Domain</param>
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
        /// Gets/Sets Credentials to be used for Proxy Server
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
        /// Clears the in-use proxy credentials so subsequent requests still use the proxy server but without credentials
        /// </summary>
        public void ClearProxyCredentials()
        {
            if (Proxy != null)
            {
                Proxy.Credentials = null;
            }
        }

        #endregion

        private void SerializeProxyConfiguration(ConfigurationSerializationContext context, 
            INode endpoint, INode user, INode pwd)
        {
            if (Credentials != null && UseCredentialsForProxy)
            {
                INode useCreds = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUseCredentialsForProxy));
                context.Graph.Assert(new Triple(endpoint, useCreds, UseCredentialsForProxy.ToLiteral(context.Graph)));
            }
            if (Proxy is WebProxy webProxy)
            {
                INode proxy = context.NextSubject;
                INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                INode usesProxy = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProxy));
                INode proxyType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassProxy));
                INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));

                context.Graph.Assert(new Triple(endpoint, usesProxy, proxy));
                context.Graph.Assert(new Triple(proxy, rdfType, proxyType));
                context.Graph.Assert(new Triple(proxy, server,
                    context.Graph.CreateLiteralNode(webProxy.Address.AbsoluteUri)));

                if (!UseCredentialsForProxy && Proxy.Credentials != null && 
                    Proxy.Credentials is NetworkCredential cred)
                {
                    context.Graph.Assert(new Triple(proxy, user, context.Graph.CreateLiteralNode(cred.UserName)));
                    context.Graph.Assert(new Triple(proxy, pwd, context.Graph.CreateLiteralNode(cred.Password)));
                }
            }
        }

        /// <summary>
        /// Applies generic request options (timeout, authorization and proxy server) to a request
        /// </summary>
        /// <param name="httpRequest">HTTP Request</param>
        protected void ApplyRequestOptions(HttpWebRequest httpRequest)
        {
            if (Timeout > 0) httpRequest.Timeout = Timeout;

            // Apply Credentials to request if necessary
            if (Credentials != null)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(Credentials.UserName + ":" + Credentials.Password));
                    httpRequest.Headers.Add("Authorization", "Basic " + credentials);
                }
                else
                {
                    // Leave .Net to handle the HTTP auth challenge response itself
                    httpRequest.Credentials = Credentials;
                    httpRequest.PreAuthenticate = true;
                }
            }

            // Use a Proxy if required
            if (Proxy != null)
            {
                httpRequest.Proxy = Proxy;
            }
            if (UseCredentialsForProxy && httpRequest.Proxy != null)
            {
                httpRequest.Proxy.Credentials = Credentials;
            }

            // Disable Keep Alive since it can cause errors when carrying out high volumes of operations or when performing long running operations
            httpRequest.KeepAlive = false;

            // Allow derived classes to provide further customisation
            ApplyCustomRequestOptions(httpRequest);
        }

    }
}
