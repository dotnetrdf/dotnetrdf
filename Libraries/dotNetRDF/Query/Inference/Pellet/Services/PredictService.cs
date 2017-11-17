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
            if (!Endpoint.Uri.EndsWith("predict/"))
            {
                _predictUri = Endpoint.Uri.Substring(0, Endpoint.Uri.IndexOf("predict/") + 8);
            }
            else
            {
                _predictUri = Endpoint.Uri;
            }
        }

        /// <summary>
        /// Gets the list of Predictions for the given Individual and Property
        /// </summary>
        /// <param name="individual">QName of an Inidividual</param>
        /// <param name="property">QName of a Property</param>
        /// <returns></returns>
        public List<INode> Predict(String individual, String property)
        {
            IGraph g = PredictRaw(individual, property);

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
            String requestUri = _predictUri + individual + "/" + property;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

            Tools.HttpDebugRequest(request);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return g;
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
        }

        /// <summary>
        /// Gets the list of Predictions for the given Individual and Property
        /// </summary>
        /// <param name="individual">QName of an Inidividual</param>
        /// <param name="property">QName of a Property</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the operation succeeds the callback will be invoked normally, if there is an error the callback will be invoked with a instance of <see cref="AsyncError"/> passed as the state which provides access to the error message and the original state passed in.
        /// </remarks>
        public void Predict(String individual, String property, NodeListCallback callback, Object state)
        {
            PredictRaw(individual, property, (g, s) =>
                {
                    if (s is AsyncError)
                    {
                        callback(null, s);
                    }
                    else
                    {
                        List<INode> predictions = (from t in g.Triples
                                                   select t.Object).Distinct().ToList();

                        callback(predictions, state);
                    }
                }, state);
        }

        /// <summary>
        /// Gets the Raw Predictions Graph from the Knowledge Base
        /// </summary>
        /// <param name="individual">QName of an Individual</param>
        /// <param name="property">QName of a Property</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the operation succeeds the callback will be invoked normally, if there is an error the callback will be invoked with a instance of <see cref="AsyncError"/> passed as the state which provides access to the error message and the original state passed in.
        /// </remarks>
        public void PredictRaw(String individual, String property, GraphCallback callback, Object state)
        {
            String requestUri = _predictUri + individual + "/" + property;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

            Tools.HttpDebugRequest(request);

            try
            {
                request.BeginGetResponse(result =>
                    {
                        try
                        {
                            using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(result))
                            {
                                Tools.HttpDebugResponse(response);

                                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                                Graph g = new Graph();
                                parser.Load(g, new StreamReader(response.GetResponseStream()));

                                response.Close();
                                callback(g, state);
                            }
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
