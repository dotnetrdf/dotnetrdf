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
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
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

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

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
