/*

Copyright Robert Vesse 2009-10
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Storage
{
    public class DydraConnector : SesameHttpProtocolConnector
    {
        private const String DydraBaseUri = "http://dydra.com/";
        private const String DydraApiKeyPassword = "X";
        private String _apiKey;

        public DydraConnector(String accountID, String repositoryID)
            : base(DydraBaseUri + accountID + "/", repositoryID)
        {
            this._repositoriesPrefix = String.Empty;
            this._queryPath = "/sparql";
            this._fullContextEncoding = false;
        }

        public DydraConnector(String accountID, String repositoryID, String apiKey)
            : this(accountID, repositoryID)
        {
            this._apiKey = apiKey;
            this._username = this._apiKey;
            this._pwd = DydraApiKeyPassword;
            this._hasCredentials = true;
        }

        public DydraConnector(String accountID, String repositoryID, String username, String password)
            : this(accountID, repositoryID)
        {
            this._username = username;
            this._pwd = password;
            this._hasCredentials = true;
        }

        public override IEnumerable<Uri> ListGraphs()
        {
            try
            {
                //Use the /contexts method to get the Graph URIs
                HttpWebRequest request = this.CreateRequest("/contexts", MimeTypesHelper.HttpSparqlAcceptHeader, "GET", new Dictionary<string, string>());
                SparqlResultSet results = new SparqlResultSet();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    parser.Load(results, new StreamReader(response.GetResponseStream()));
                    response.Close();
                }

                List<Uri> graphUris = new List<Uri>();
                foreach (SparqlResult r in results)
                {
                    if (r.HasValue("contextID"))
                    {
                        INode value = r["contextID"];
                        if (value.NodeType == NodeType.Uri)
                        {
                            graphUris.Add(((IUriNode)value).Uri);
                        }
                        else if (value.NodeType == NodeType.Blank)
                        {
                            //Dydra allows BNode Graph URIs
                            graphUris.Add(new Uri("dydra:bnode:" + ((IBlankNode)value).InternalID));
                        }
                    }
                }
                return graphUris;
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("An error occurred while attempting to retrieve the Graph List from the Store, see inner exception for details", ex);
            }
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected override HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
        {
            //Build the Request Uri
            String requestUri = this._baseUri + servicePath;//this.GetCredentialedUri() + servicePath;
            if (this._apiKey != null)
            {
                requestUri += "?auth_token=" + Uri.EscapeDataString(this._apiKey);
            }
            if (queryParams.Count > 0)
            {
                if (requestUri.Contains("?"))
                {
                    if (!requestUri.EndsWith("&")) requestUri += "&";
                }
                else
                {
                    requestUri += "?";
                }
                foreach (String p in queryParams.Keys)
                {
                    requestUri += p + "=" + Uri.EscapeDataString(queryParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;

            ////Add Credentials if needed
            //if (this._hasCredentials)
            //{
            //    NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
            //    request.Credentials = credentials;
            //}

            return request;
        }

        protected override string EscapeQuery(string query)
        {
            return query;
        }

        private String GetCredentialedUri()
        {
            if (this._hasCredentials)
            {
                if (this._apiKey != null)
                {
                    return this._baseUri.Substring(0, 7) + Uri.EscapeUriString(this._apiKey) + "@" + this._baseUri.Substring(7);
                }
                else
                {
                    return this._baseUri.Substring(0, 7) + Uri.EscapeUriString(this._username) + ":" + Uri.EscapeUriString(this._pwd) + "@" + this._baseUri.Substring(7);
                }
            }
            else
            {
                return this._baseUri;
            }
        }
    }
}
