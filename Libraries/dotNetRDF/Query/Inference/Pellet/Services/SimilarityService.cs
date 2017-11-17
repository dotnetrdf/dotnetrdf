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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Similarity Service provided by a Pellet Knowledge Base
    /// </summary>
    public class SimilarityService
        : PelletService
    {
        private String _similarityUri;

        /// <summary>
        /// Creates a new Similarity Service for a Pellet Knowledge Base
        /// </summary>
        /// <param name="serviceName">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal SimilarityService(String serviceName, JObject obj)
            : base(serviceName, obj)
        {
            if (!Endpoint.Uri.EndsWith("similarity/"))
            {
                _similarityUri = Endpoint.Uri.Substring(0, Endpoint.Uri.IndexOf("similarity/") + 11);
            }
            else
            {
                _similarityUri = Endpoint.Uri;
            }
        }

        /// <summary>
        /// Gets a list of key value pairs listing Similar Individuals and their Similarity scores
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <returns></returns>
        public List<KeyValuePair<INode, double>> Similarity(int number, String individual)
        {
            IGraph g = SimilarityRaw(number, individual);

            List<KeyValuePair<INode, double>> similarities = new List<KeyValuePair<INode, double>>();

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = g.NamespaceMap;
            query.CommandText = "SELECT ?ind ?similarity WHERE { ?s cp:isSimilarTo ?ind ; cp:similarityValue ?similarity }";

            try
            {
                Object results = g.ExecuteQuery(query);
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in (SparqlResultSet)results)
                    {
                        if (r["similarity"].NodeType == NodeType.Literal)
                        {
                            similarities.Add(new KeyValuePair<INode, double>(r["ind"], Convert.ToDouble(((ILiteralNode)r["similarity"]).Value, CultureInfo.InvariantCulture)));
                        }
                    }
                }
                else
                {
                    throw new RdfReasoningException("Unable to extract the Similarity Information from the Similarity Graph returned by Pellet Server");
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                throw new RdfReasoningException("A HTTP error occurred while communicating with Pellet Server", webEx);
            }
            catch (Exception ex)
            {
                throw new RdfReasoningException("Unable to extract the Similarity Information from the Similarity Graph returned by Pellet Server", ex);
            }

            return similarities;
        }

        /// <summary>
        /// Gets the raw Similarity Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <returns></returns>
        public IGraph SimilarityRaw(int number, String individual)
        {
            if (number < 1) throw new RdfReasoningException("Pellet Server requires the number of Similar Individuals to be at least 1");

            String requestUri = _similarityUri + number + "/" + individual;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

            Tools.HttpDebugRequest(request);

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

        /// <summary>
        /// Gets a list of key value pairs listing Similar Individuals and their Similarity scores
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the operation succeeds the callback will be invoked normally, if there is an error the callback will be invoked with a instance of <see cref="AsyncError"/> passed as the state which provides access to the error message and the original state passed in.
        /// </remarks>
        public void Similarity(int number, String individual, PelletSimilarityServiceCallback callback, Object state)
        {
            SimilarityRaw(number, individual, (g, s) =>
                {
                    if (s is AsyncError)
                    {
                        callback(null, s);
                    }
                    else
                    {
                        List<KeyValuePair<INode, double>> similarities = new List<KeyValuePair<INode, double>>();

                        SparqlParameterizedString query = new SparqlParameterizedString();
                        query.Namespaces = g.NamespaceMap;
                        query.CommandText = "SELECT ?ind ?similarity WHERE { ?s cp:isSimilarTo ?ind ; cp:similarityValue ?similarity }";

                        Object results = g.ExecuteQuery(query);
                        if (results is SparqlResultSet)
                        {
                            foreach (SparqlResult r in (SparqlResultSet) results)
                            {
                                if (r["similarity"].NodeType == NodeType.Literal)
                                {
                                    similarities.Add(new KeyValuePair<INode, double>(r["ind"], Convert.ToDouble(((ILiteralNode) r["similarity"]).Value, CultureInfo.InvariantCulture)));
                                }
                            }

                            callback(similarities, state);
                        }
                        else
                        {
                            throw new RdfReasoningException("Unable to extract the Similarity Information from the Similarity Graph returned by Pellet Server");
                        }
                    }
                }, null);
        }

        /// <summary>
        /// Gets the raw Similarity Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the operation succeeds the callback will be invoked normally, if there is an error the callback will be invoked with a instance of <see cref="AsyncError"/> passed as the state which provides access to the error message and the original state passed in.
        /// </remarks>
        public void SimilarityRaw(int number, String individual, GraphCallback callback, Object state)
        {
            if (number < 1) throw new RdfReasoningException("Pellet Server requires the number of Similar Individuals to be at least 1");

            String requestUri = _similarityUri + number + "/" + individual;

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
