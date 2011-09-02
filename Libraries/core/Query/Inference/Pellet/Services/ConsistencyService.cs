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
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Consistency Service provided by a Pellet Server
    /// </summary>
    public class ConsistencyService 
        : PelletService
    {
        /// <summary>
        /// Creates a new Consistency Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ConsistencyService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT
        /// <summary>
        /// Returns whether the Knowledge Base is consistent
        /// </summary>
        public bool IsConsistent()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.HttpSparqlAcceptHeader;

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
                    ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    SparqlResultSet results = new SparqlResultSet();
                    parser.Load(results, new StreamReader(response.GetResponseStream()));

                    //Expect a boolean result set
                    return results.Result;
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
        /// Determines whether the Knowledge Base is consistent
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void IsConsistent(PelletConsistencyCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.HttpSparqlAcceptHeader;

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
                        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                        SparqlResultSet results = new SparqlResultSet();
                        parser.Load(results, new StreamReader(response.GetResponseStream()));

                        //Expect a boolean result set
                        callback(results.Result, state);
                    }
                }, null);
        }
    }
}
