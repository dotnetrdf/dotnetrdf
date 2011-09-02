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
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Classify Service provided by a Pellet Server
    /// </summary>
    public class ClassifyService : PelletService
    {
        /// <summary>
        /// Creates a new Classify Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ClassifyService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT
        /// <summary>
        /// Extracts the Graph which comprises the class hierarchy
        /// </summary>
        public IGraph Classify()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif

                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return g;
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
                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
        }
#endif

        /// <summary>
        /// Extracts the Graph which comprises the class hierarchy
        /// </summary>
        /// <param name="callback">Callback for when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void Classify(GraphCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetResponse(result =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif

                        IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                        Graph g = new Graph();
                        parser.Load(g, new StreamReader(response.GetResponseStream()));

                        response.Close();
                        callback(g, state);
                    }                  
                }, null);
        }
    }
}
