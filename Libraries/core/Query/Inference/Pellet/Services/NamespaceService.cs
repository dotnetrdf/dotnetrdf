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

#if !SILVERLIGHT

        /// <summary>
        /// Gets the Namespaces used in the Knowledge Base
        /// </summary>
        /// <returns></returns>
        public NamespaceMapper GetNamespaces()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = "text/json";

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            String jsonText;
            JObject json;
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
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();
                }

                //Parse the Response into a NamespaceMapper
                NamespaceMapper nsmap = new NamespaceMapper(true);
                foreach (JProperty nsDef in json.Properties())
                {
                    nsmap.AddNamespace(nsDef.Name, new Uri((String)nsDef.Value));
                }

                return nsmap;
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
            catch (Exception ex)
            {
                throw new RdfReasoningException("Error occurred while parsing Namespace Service results", ex);
            }
        }

#endif

        /// <summary>
        /// Gets the Namespaces used in the Knowledge Base
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void GetNamespaces(NamespaceCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = "text/json";

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            String jsonText;
            JObject json;
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
                        jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        json = JObject.Parse(jsonText);

                        response.Close();
                    }

                    //Parse the Response into a NamespaceMapper
                    NamespaceMapper nsmap = new NamespaceMapper(true);
                    foreach (JProperty nsDef in json.Properties())
                    {
                        nsmap.AddNamespace(nsDef.Name, new Uri((String)nsDef.Value));
                    }

                    callback(nsmap, state);
                }, null);
        }
    }
}
