/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
            if (!this.Endpoint.Uri.EndsWith("similarity/"))
            {
                this._similarityUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf("similarity/") + 11);
            }
            else
            {
                this._similarityUri = this.Endpoint.Uri;
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a list of key value pairs listing Similar Individuals and their Similarity scores
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <returns></returns>
        public List<KeyValuePair<INode, double>> Similarity(int number, String individual)
        {
            IGraph g = this.SimilarityRaw(number, individual);

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
                            similarities.Add(new KeyValuePair<INode, double>(r["ind"], Convert.ToDouble(((ILiteralNode)r["similarity"]).Value)));
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

            String requestUri = this._similarityUri + number + "/" + individual;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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

#endif

        /// <summary>
        /// Gets a list of key value pairs listing Similar Individuals and their Similarity scores
        /// </summary>
        /// <param name="number">Number of Similar Individuals</param>
        /// <param name="individual">QName of a Individual to find Similar Individuals to</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Similarity(int number, String individual, PelletSimilarityServiceCallback callback, Object state)
        {
            this.SimilarityRaw(number, individual, (g, _) =>
                {
                    List<KeyValuePair<INode, double>> similarities = new List<KeyValuePair<INode, double>>();

                    SparqlParameterizedString query = new SparqlParameterizedString();
                    query.Namespaces = g.NamespaceMap;
                    query.CommandText = "SELECT ?ind ?similarity WHERE { ?s cp:isSimilarTo ?ind ; cp:similarityValue ?similarity }";

                    Object results = g.ExecuteQuery(query);
                    if (results is SparqlResultSet)
                    {
                        foreach (SparqlResult r in (SparqlResultSet)results)
                        {
                            if (r["similarity"].NodeType == NodeType.Literal)
                            {
                                similarities.Add(new KeyValuePair<INode, double>(r["ind"], Convert.ToDouble(((ILiteralNode)r["similarity"]).Value)));
                            }
                        }

                        callback(similarities, state);
                    }
                    else
                    {
                        throw new RdfReasoningException("Unable to extract the Similarity Information from the Similarity Graph returned by Pellet Server");
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
        public void SimilarityRaw(int number, String individual, GraphCallback callback, Object state)
        {
            if (number < 1) throw new RdfReasoningException("Pellet Server requires the number of Similar Individuals to be at least 1");

            String requestUri = this._similarityUri + number + "/" + individual;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

            Tools.HttpDebugRequest(request);

            request.BeginGetResponse(result =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                    {
                        Tools.HttpDebugResponse(response);
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
