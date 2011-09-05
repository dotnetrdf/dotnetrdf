/*

Copyright Robert Vesse 2009-11
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
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Predict Service of a Pellet Knowledge Base
    /// </summary>
    public class PredictService
        : PelletService
    {

        private String _predictUri;

        /// <summary>
        /// Creates a new Predict Service for a Pellet Knowledge Base
        /// </summary>
        /// <param name="serviceName">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal PredictService(String serviceName, JObject obj)
            : base(serviceName, obj)
        {
            if (!this.Endpoint.Uri.EndsWith("predict/"))
            {
                this._predictUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf("predict/") + 8);
            }
            else
            {
                this._predictUri = this.Endpoint.Uri;
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets the list of Predictions for the given Individual and Property
        /// </summary>
        /// <param name="individual">QName of an Inidividual</param>
        /// <param name="property">QName of a Property</param>
        /// <returns></returns>
        public List<INode> Predict(String individual, String property)
        {
            IGraph g = this.PredictRaw(individual, property);

            List<INode> predictions = (from t in g.Triples
                                       select t.Object).Distinct().ToList();

            return predictions;
        }

        /// <summary>
        /// Gets the Raw Predictions Graph from the Knowledge Base
        /// </summary>
        /// <param name="individual">QName of an Individual</param>
        /// <param name="property">QName of a Property</param>
        /// <returns></returns>
        public IGraph PredictRaw(String individual, String property)
        {
            String requestUri = this._predictUri + individual + "/" + property;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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
        /// Gets the list of Predictions for the given Individual and Property
        /// </summary>
        /// <param name="individual">QName of an Inidividual</param>
        /// <param name="property">QName of a Property</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Predict(String individual, String property, NodeListCallback callback, Object state)
        {
            this.PredictRaw(individual, property, (g, s) =>
                {
                    List<INode> predictions = (from t in g.Triples
                                               select t.Object).Distinct().ToList();

                    callback(predictions, state);
                }, state);
        }

        /// <summary>
        /// Gets the Raw Predictions Graph from the Knowledge Base
        /// </summary>
        /// <param name="individual">QName of an Individual</param>
        /// <param name="property">QName of a Property</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void PredictRaw(String individual, String property, GraphCallback callback, Object state)
        {
            String requestUri = this._predictUri + individual + "/" + property;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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
