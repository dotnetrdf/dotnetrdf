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

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base class for HTTP endpoints
    /// </summary>
    public abstract partial class BaseEndpoint
    {

        #region Credentials and Proxy Server

        /// <summary>
        /// Controls whether the Credentials set with the <see cref="BaseEndpoint.SetCredentials(String,String)">SetCredentials()</see> method or the <see cref="BaseEndpoint.Credentials">Credentials</see>are also used for a Proxy (if used)
        /// </summary>
        public bool UseCredentialsForProxy
        {
            get => false;
            set => throw new PlatformNotSupportedException("Web proxies are not supported in the .NET Core build");
        }

        /// <summary>
        /// Gets/Sets a Proxy Server to be used
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Raised if an attempt is made to set this property on the .NET Core platform. Proxies are not supported on this platform</exception>
        public IWebProxy Proxy
        {
            get => null;
            set => throw new PlatformNotSupportedException("Web proxies are not supported in the .NET Core build");
        }

        #endregion

        
        private void SerializeProxyConfiguration(ConfigurationSerializationContext context,
            INode endpoint, INode user, INode pwd)
        {
            // NetStandard 1.x does not support IWebProxu
        }

        /// <summary>
        /// Applies generic request options (timeout, authorization and proxy server) to a request
        /// </summary>
        /// <param name="httpRequest">HTTP Request</param>
        protected void ApplyRequestOptions(HttpWebRequest httpRequest)
        {
            // Apply Credentials to request if necessary
            if (Credentials != null)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName + ":" + this.Credentials.Password));
                    httpRequest.Headers["Authorization"] = "Basic " + credentials;
                }
                else
                {
                    // Leave .Net to handle the HTTP auth challenge response itself
                    httpRequest.Credentials = Credentials;
                }
            }

            // Allow derived classes to provide further customisation
            ApplyCustomRequestOptions(httpRequest);
        }

        
    }
}
