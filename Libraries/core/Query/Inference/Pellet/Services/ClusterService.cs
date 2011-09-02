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
