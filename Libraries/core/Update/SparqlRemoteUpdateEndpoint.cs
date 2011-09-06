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
#if !NO_WEB
using System.Web;
#endif

namespace VDS.RDF.Update
{
    /// <summary>
    /// A Class for connecting to a remote SPARQL Update endpoint and executing Updates against it
    /// </summary>
    public class SparqlRemoteUpdateEndpoint : BaseEndpoint
    {
        const int LongUpdateLength = 2048;

        /// <summary>
        /// Creates a new SPARQL Update Endpoint for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public SparqlRemoteUpdateEndpoint(Uri endpointUri)
            : base(endpointUri) 
        {
            this.HttpMode = "POST";
        }

        /// <summary>
        /// Creates a new SPARQL Update Endpoint for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public SparqlRemoteUpdateEndpoint(String endpointUri)
            : this(new Uri(endpointUri)) { }

#if !SILVERLIGHT

        /// <summary>
        /// Makes an update request to the remote endpoint
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            try
            {
                //Build the Request URI and POST Data
                StringBuilder requestUri = new StringBuilder();
                requestUri.Append(this.Uri.ToString());
                StringBuilder postData = new StringBuilder();
                bool longUpdate = false;
                if (!this.HttpMode.Equals("POST") && sparqlUpdate.Length <= LongUpdateLength)
                {
                    if (!this.Uri.Query.Equals(String.Empty))
                    {
                        requestUri.Append("&update=");
                    }
                    else
                    {
                        requestUri.Append("?update=");
                    }
                    requestUri.Append(HttpUtility.UrlEncode(sparqlUpdate));
                }
                else
                {
                    longUpdate = true;
                    postData.Append("update=");
                    postData.Append(Uri.EscapeDataString(sparqlUpdate));
                }

                //Make the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());

                //Apply Credentials to request if necessary
                if (this.Credentials != null)
                {
                    request.Credentials = this.Credentials;
                }

#if !NO_PROXY
                //Use a Proxy if required
                if (this.Proxy != null)
                {
                    request.Proxy = this.Proxy;
                    if (this.UseCredentialsForProxy)
                    {
                        request.Proxy.Credentials = this.Credentials;
                    }
                }
#endif

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                if (longUpdate)
                {
                    request.Method = "POST";
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(postData);
                        writer.Close();
                    }
                }
                else
                {
                    request.Method = this.HttpMode;
                }
                request.Accept = MimeTypesHelper.Any;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we don't get an error then we should be fine
                    response.Close();
                }

            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new SparqlUpdateException("A HTTP Error occurred when trying to make the SPARQL Update", webEx);
            }
        }

#endif

        /// <summary>
        /// Makes an update request asynchronously to the remote endpoint
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <param name="callback">Callback to invoke when the update completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Update(String sparqlUpdate, UpdateCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.Method = "POST";
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            request.Accept = MimeTypesHelper.Any;

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetRequestStream(result =>
                {
                    Stream stream = request.EndGetRequestStream(result);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write("update=");
                        writer.Write(HttpUtility.UrlEncode(sparqlUpdate));

                        writer.Close();
                    }

                    request.BeginGetResponse(innerResult =>
                        {
                            using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(innerResult))
                            {
#if DEBUG
                                if (Options.HttpDebugging)
                                {
                                    Tools.HttpDebugResponse(response);
                                }
#endif

                                response.Close();
                                callback(state);
                            }
                        }, null);
                }, null);
        }
    }
}
