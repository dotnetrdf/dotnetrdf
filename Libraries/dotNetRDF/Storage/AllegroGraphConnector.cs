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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management;
#if !NO_WEB
using System.Web;
#endif

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to an AllegroGraph Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// Connection to AllegroGraph is based on their new HTTP Protocol which is an extension of the <a href="http://www.openrdf.org/doc/sesame2/system/ch08.html">Sesame 2.0 HTTP Protocol</a>.  The specification for the AllegroGraph protocol can be found <a href="http://www.franz.com/agraph/support/documentation/current/new-http-server.html">here</a>
    /// </para>
    /// <para>
    /// If you wish to use a Store which is part of the Root Catalog on an AllegroGraph 4.x and higher server you can either use the constructor overloads that omit the <strong>catalogID</strong> parameter or pass in null as the value for that parameter
    /// </para>
    /// </remarks>
    public class AllegroGraphConnector
        : BaseSesameHttpProtocolConnector, IAsyncUpdateableStorage
#if !NO_SYNC_HTTP
        , IUpdateableStorage
#endif
    {
        private String _agraphBase;
        private readonly String _catalog;
         
        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base URI for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="storeID">Store ID</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID)
            : this(baseUri, catalogID, storeID, (String)null, (String)null) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="storeID">Store ID</param>
        public AllegroGraphConnector(String baseUri, String storeID)
            : this(baseUri, null, storeID) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID, String username, String password)
            : base(baseUri, storeID, username, password)
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
#if NETCORE
            this._agraphBase = this._baseUri.Copy();
#else
            this._agraphBase = String.Copy(this._baseUri);
#endif
            if (catalogID != null)
            {
                this._baseUri += "catalogs/" + catalogID + "/";
            }
            this._store = storeID;
            this._catalog = catalogID;
            this._updatePath = String.Empty;

            this._server = new AllegroGraphServer(this._baseUri, this._catalog);
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        public AllegroGraphConnector(String baseUri, String storeID, String username, String password)
            : this(baseUri, null, storeID, username, password) { }

#if !NO_PROXY

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID, WebProxy proxy)
            : this(baseUri, catalogID, storeID, null, null, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphConnector(String baseUri, String storeID, WebProxy proxy)
            : this(baseUri, null, storeID, proxy) { }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="catalogID">Catalog ID</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphConnector(String baseUri, String catalogID, String storeID, String username, String password, WebProxy proxy)
            : this(baseUri, catalogID, storeID, username, password)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Creates a new Connection to an AllegroGraph store in the Root Catalog (AllegroGraph 4.x and higher)
        /// </summary>
        /// <param name="baseUri">Base Uri for the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username for connecting to the Store</param>
        /// <param name="password">Password for connecting to the Store</param>
        /// <param name="proxy">Proxy Server</param>
        public AllegroGraphConnector(String baseUri, String storeID, String username, String password, WebProxy proxy)
            : this(baseUri, null, storeID, username, password, proxy) { }

#endif

        /// <summary>
        /// Gets the Catalog under which the repository you are connected to is located
        /// </summary>
        [Description("The Catalog under which the repository is located.  If using the Root Catalog on AllegroGrah 4+ <ROOT> will be displayed.")]
        public String Catalog
        {
            get
            {
                return (this._catalog != null ? this._catalog : "<ROOT>");
            }
        }

#if !NO_SYNC_HTTP

        /// <summary>
        /// Makes a SPARQL Update request to the Allegro Graph server
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public virtual void Update(string sparqlUpdate)
        {
            try
            {
                HttpWebRequest request;

                // Create the Request
                request = this.CreateRequest(this._repositoriesPrefix + this._store + this._updatePath, MimeTypesHelper.Any, "POST", new Dictionary<String, String>());

                // Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                StringBuilder postData = new StringBuilder();
                postData.Append("query=");
                postData.Append(HttpUtility.UrlEncode(EscapeQuery(sparqlUpdate)));
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(Options.UseBomForUtf8)))
                {
                    writer.Write(postData);
                    writer.Close();
                }

                Tools.HttpDebugRequest(request);

                // Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here it completed OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "updating");
            }
        }

#endif

        /// <summary>
        /// Makes a SPARQL Update request to the Allegro Graph server
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Update(string sparqlUpdate, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request;

                // Create the Request
                request = this.CreateRequest(this._repositoriesPrefix + this._store + this._updatePath, MimeTypesHelper.Any, "POST", new Dictionary<String, String>());

                // Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                StringBuilder postData = new StringBuilder();
                postData.Append("query=");
                postData.Append(HttpUtility.UrlEncode(EscapeQuery(sparqlUpdate)));

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(r);
                        using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                        {
                            writer.Write(postData);
                            writer.Close();
                        }

                        Tools.HttpDebugRequest(request);

                        // Get the Response and process based on the Content Type
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                Tools.HttpDebugResponse(response);
                                // If we get here it completed OK
                                response.Close();
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
            }
        }

        /// <summary>
        /// Does nothing as AllegroGraph does not require the same query escaping that Sesame does
        /// </summary>
        /// <param name="query">Query to escape</param>
        /// <returns></returns>
        protected override string EscapeQuery(string query)
        {
            return query;
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected override HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> queryParams)
        {
            // Remove JSON Mime Types from supported Accept types
            // This is a compatability issue with Allegro having a weird custom JSON serialisation
            if (accept.Contains("application/json"))
            {
                accept = accept.Replace("application/json,", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains("text/json"))
            {
                accept = accept.Replace("text/json", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains(",;")) accept = accept.Replace(",;", ",");

            return base.CreateRequest(servicePath, accept, method, queryParams);
        }

        protected override string GetSaveContentType()
        {
            // AllegroGraph rejects application/n-triples as it still expects to get text/plain so have to use that instead
            return "text/plain";
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._catalog != null)
            {
                return "[AllegroGraph] Store '" + this._store + "' in Catalog '" + this._catalog + "' on Server '" + this._baseUri.Substring(0, this._baseUri.IndexOf("catalogs/")) + "'";
            }
            return "[AllegroGraph] Store '" + this._store + "' in Root Catalog on Server '" + this._baseUri + "'";
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode catalog = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCatalog));
            INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            if (this._catalog != null)
            {
                context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri.Substring(0, this._baseUri.IndexOf("catalogs/")))));
                context.Graph.Assert(new Triple(manager, catalog, context.Graph.CreateLiteralNode(this._catalog)));
            }
            else
            {
                context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            }
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._store)));
            
            if (this._username != null && this._pwd != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
            }

            base.SerializeStandardConfig(manager, context);
        }
    }
}
