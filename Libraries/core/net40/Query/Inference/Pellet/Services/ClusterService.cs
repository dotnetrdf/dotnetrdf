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
    /// Represents the Cluster Service provided by a Pellet Knowledge Base
    /// </summary>
    public class ClusterService : PelletService
    {
        private String _clusterUri;

        internal ClusterService(String serviceName, JObject obj)
            : base(serviceName, obj)
        {
            if (!this.Endpoint.Uri.EndsWith("cluster/"))
            {
                this._clusterUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf("cluster/") + 8);
            }
            else
            {
                this._clusterUri = this.Endpoint.Uri;
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a list of lists expressing clusters within the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <returns></returns>
        public List<List<INode>> Cluster(int number)
        {
            IGraph g = this.ClusterRaw(number); 

            //Build the List of Lists
            List<List<INode>> clusters = new List<List<INode>>();
            foreach (INode clusterNode in g.Triples.SubjectNodes.Distinct())
            {
                List<INode> cluster = new List<INode>();
                foreach (Triple t in g.GetTriplesWithSubject(clusterNode))
                {
                    cluster.Add(t.Object);
                }
                cluster = cluster.Distinct().ToList();
                clusters.Add(cluster);
            }
            return clusters;
        }

        /// <summary>
        /// Gets a list of lists expressing clusters within the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="type">QName of a Type to cluster around</param>
        /// <returns></returns>
        public List<List<INode>> Cluster(int number, String type)
        {
            IGraph g = this.ClusterRaw(number, type);

            //Build the List of Lists
            List<List<INode>> clusters = new List<List<INode>>();
            foreach (INode clusterNode in g.Triples.SubjectNodes.Distinct())
            {
                List<INode> cluster = new List<INode>();
                foreach (Triple t in g.GetTriplesWithSubject(clusterNode))
                {
                    cluster.Add(t.Object);
                }
                cluster = cluster.Distinct().ToList();
                clusters.Add(cluster);
            }
            return clusters;
        }

        /// <summary>
        /// Gets the raw Cluster Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <returns></returns>
        public IGraph ClusterRaw(int number)
        {
            if (number < 2) throw new RdfReasoningException("Pellet Server requires the number of Clusters to be at least 2");

            String requestUri = this._clusterUri + number + "/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(type => !type.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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
        /// Gets the raw Cluster Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="type">QName of a Type to Cluster around</param>
        /// <returns></returns>
        public IGraph ClusterRaw(int number, String type)
        {
            if (number < 2) throw new RdfReasoningException("Pellet Server requires the number of Clusters to be at least 2");

            String requestUri = this._clusterUri + number + "/" + type;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(t => !t.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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

#endif

        /// <summary>
        /// Gets a list of lists expressing clusters within the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="callback">Callback to be invoked when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void Cluster(int number, PelletClusterServiceCallback callback, Object state)
        {
            this.ClusterRaw(number, (g, s) =>
                {
                    //Build the List of Lists
                    List<List<INode>> clusters = new List<List<INode>>();
                    foreach (INode clusterNode in g.Triples.SubjectNodes.Distinct())
                    {
                        List<INode> cluster = new List<INode>();
                        foreach (Triple t in g.GetTriplesWithSubject(clusterNode))
                        {
                            cluster.Add(t.Object);
                        }
                        cluster = cluster.Distinct().ToList();
                        clusters.Add(cluster);
                    }

                    callback(clusters, s);
                }, state);
        }

        /// <summary>
        /// Gets a list of lists expressing clusters within the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="type">QName of a Type to cluster around</param>
        /// <param name="callback">Callback to be invoked when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void Cluster(int number, String type, PelletClusterServiceCallback callback, Object state)
        {
            this.ClusterRaw(number, type, (g, s) =>
                {
                    //Build the List of Lists
                    List<List<INode>> clusters = new List<List<INode>>();
                    foreach (INode clusterNode in g.Triples.SubjectNodes.Distinct())
                    {
                        List<INode> cluster = new List<INode>();
                        foreach (Triple t in g.GetTriplesWithSubject(clusterNode))
                        {
                            cluster.Add(t.Object);
                        }
                        cluster = cluster.Distinct().ToList();
                        clusters.Add(cluster);
                    }

                    callback(clusters, s);
                }, state);
        }

        /// <summary>
        /// Gets the raw Cluster Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="callback">Callback to be invoked when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void ClusterRaw(int number, GraphCallback callback, Object state)
        {
            if (number < 2) throw new RdfReasoningException("Pellet Server requires the number of Clusters to be at least 2");

            String requestUri = this._clusterUri + number + "/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes.Where(type => !type.Equals("text/json")), MimeTypesHelper.SupportedRdfMimeTypes);

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

        /// <summary>
        /// Gets the raw Cluster Graph for the Knowledge Base
        /// </summary>
        /// <param name="number">Number of Clusters</param>
        /// <param name="type">QName of a Type to Cluster around</param>
        /// <param name="callback">Callback to be invoked when the operation completes</param>
        /// <param name="state">State to be passed to the callback</param>
        public void ClusterRaw(int number, String type, GraphCallback callback, Object state)
        {
            if (number < 2) throw new RdfReasoningException("Pellet Server requires the number of Clusters to be at least 2");

            String requestUri = this._clusterUri + number + "/" + type;

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
