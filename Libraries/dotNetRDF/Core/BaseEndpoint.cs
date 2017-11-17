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
using VDS.RDF.Configuration;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base class for HTTP endpoints
    /// </summary>
    public abstract partial class BaseEndpoint
        : IConfigurationSerializable
    {
        private int _timeout = 30000;
        private string _httpMode = "AUTO";

        /// <summary>
        /// Creates a new Base Endpoint
        /// </summary>
        protected BaseEndpoint()
        {

        }

        /// <summary>
        /// Creates a new Base Endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        protected BaseEndpoint(Uri endpointUri)
        {
            Uri = endpointUri ?? throw new ArgumentNullException(nameof(endpointUri), "Endpoint URI cannot be null");
        }

        /// <summary>
        /// Gets the Endpoints URI
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets/Sets the HTTP authentication credentials to be used
        /// </summary>
        public NetworkCredential Credentials { get; set; }


        /// <summary>
        /// Gets/Sets the HTTP Mode used for requests
        /// </summary>
        /// <remarks>
        /// <para>This property defaults to the value AUTO. in AUTO mode GET will be used unless the total length of query parameters exceeeds 2048 characters
        /// or the query contains non-ASCII characters, and POST will be used for longer queries or where the query contains non-ASCII characters.</para>
        /// <para>
        /// Only AUTO, GET and POST are permitted - implementations may override this property if they wish to support more methods.
        /// </para>
        /// </remarks>
        public virtual string HttpMode
        {
            get => _httpMode;
            set
            {
                // Can only use GET/POST
                if (value.Equals("GET", StringComparison.OrdinalIgnoreCase) || value.Equals("POST", StringComparison.OrdinalIgnoreCase) || value.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
                {
                    _httpMode = value.ToUpper();
                }
                else
                {
                    throw new ArgumentException("HTTP Mode can only be AUTO/GET/POST, derived implementations should override this property if they wish to support more methods");
                }
            }
        }



        /// <summary>
        /// Gets/Sets the HTTP Timeouts used specified in milliseconds
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defaults to 30 Seconds (i.e. the default value is 30,000)
        /// </para>
        /// <para>
        /// It is important to understand that this timeout only applies to the HTTP request portions of any operation performed and that the timeout may apply more than once if a POST operation is used since the timeout applies separately to obtaining the request stream to POST the request and obtaining the response stream.  Also the timeout does not in any way apply to subsequent work that may be carried out before the operation can return so if you need a hard timeout you should manage that yourself.
        /// </para>
        /// <para>
        /// When set to a zero/negative value then the standard .Net timeout of 100 seconds will apply, use <see cref="int.MaxValue"/> if you want the maximum possible timeout i.e. if you expect to launch extremely long running operations.
        /// </para>
        /// <para>
        /// Not supported under Silverlight, Windows Phone and Portable Class Library builds
        /// </para>
        /// </remarks>
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (value >= 0)
                {
                    _timeout = value;
                }
            }
        }

        #region Credentials
        /// <summary>
        /// Sets the HTTP Digest authentication credentials to be used
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public void SetCredentials(string username, string password)
        {
            Credentials = new NetworkCredential(username, password);
        }

        /// <summary>
        /// Sets the HTTP Digest authentication credentials to be used
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="domain">Domain</param>
        public void SetCredentials(string username, string password, string domain)
        {
            Credentials = new NetworkCredential(username, password, domain);
        }


        /// <summary>
        /// Clears any in-use credentials so subsequent requests will not use HTTP authentication
        /// </summary>
        public void ClearCredentials()
        {
            Credentials = null;
        }
        #endregion

        /// <summary>
        /// Serializes the endpoints Credential and Proxy information
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (Credentials != null || Proxy != null)
            {
                INode endpoint = context.NextSubject;
                INode user = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));

                if (Credentials != null)
                {
                    context.Graph.Assert(new Triple(endpoint, user, context.Graph.CreateLiteralNode(Credentials.UserName)));
                    context.Graph.Assert(new Triple(endpoint, pwd, context.Graph.CreateLiteralNode(Credentials.Password)));
                }
                SerializeProxyConfiguration(context, endpoint, user, pwd);
            }
        }

        /// <summary>
        /// Method which may be overridden in derived classes to add any additional custom request options/headers to the request
        /// </summary>
        /// <param name="httpRequest">HTTP Request</param>
        /// <remarks>
        /// This is called at the end of <see cref="ApplyRequestOptions"/> so can also be used to override that methods default behaviour
        /// </remarks>
        protected virtual void ApplyCustomRequestOptions(HttpWebRequest httpRequest)
        {

        }
    }
}
