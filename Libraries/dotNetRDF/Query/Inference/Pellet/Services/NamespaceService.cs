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
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Namespace Service provided by a Pellet Server knowledge base
    /// </summary>
    public class NamespaceService
        : PelletService
    {
        /// <summary>
        /// Creates a new Namespace Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal NamespaceService(String name, JObject obj)
            : base(name, obj)
        {

        }

        /// <summary>
        /// Gets the Namespaces used in the Knowledge Base
        /// </summary>
        /// <returns></returns>
        public NamespaceMapper GetNamespaces()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Endpoint.Uri);
            request.Method = Endpoint.HttpMethods.First();
            request.Accept = "text/json";

            Tools.HttpDebugRequest(request);

            String jsonText;
            JObject json;
            try 
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();
                }

                // Parse the Response into a NamespaceMapper
                NamespaceMapper nsmap = new NamespaceMapper(true);
                foreach (JProperty nsDef in json.Properties())
                {
                    nsmap.AddNamespace(nsDef.Name, UriFactory.Create((String)nsDef.Value));
                }

                return nsmap;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
            catch (Exception ex)
            {
                throw new RdfReasoningException("Error occurred while parsing Namespace Service results", ex);
            }
        }

        /// <summary>
        /// Gets the Namespaces used in the Knowledge Base
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        /// <remarks>
        /// If the operation succeeds the callback will be invoked normally, if there is an error the callback will be invoked with a instance of <see cref="AsyncError"/> passed as the state which provides access to the error message and the original state passed in.
        /// </remarks>
        public void GetNamespaces(NamespaceCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Endpoint.Uri);
            request.Method = Endpoint.HttpMethods.First();
            request.Accept = "text/json";

            Tools.HttpDebugRequest(request);

            try
            {
                String jsonText;
                JObject json;
                request.BeginGetResponse(result =>
                    {
                        try
                        {
                            using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result))
                            {
                                Tools.HttpDebugResponse(response);
                                jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                                json = JObject.Parse(jsonText);

                                response.Close();
                            }

                            // Parse the Response into a NamespaceMapper
                            NamespaceMapper nsmap = new NamespaceMapper(true);
                            foreach (JProperty nsDef in json.Properties())
                            {
                                nsmap.AddNamespace(nsDef.Name, UriFactory.Create((String) nsDef.Value));
                            }

                            callback(nsmap, state);
                        }
                        catch (WebException webEx)
                        {
                            if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            callback(null, new AsyncError(new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server, see inner exception for details", webEx), state));
                        }
                        catch (Exception ex)
                        {
                            callback(null, new AsyncError(new RdfReasoningException("An unexpected error occurred while communicating with the Pellet Server, see inner exception for details", ex), state));
                        }
                    }, null);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                callback(null, new AsyncError(new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server, see inner exception for details", webEx), state));
            }
            catch (Exception ex)
            {
                callback(null, new AsyncError(new RdfReasoningException("An unexpected error occurred while communicating with the Pellet Server, see inner exception for details", ex), state));
            }
        }
    }
}
