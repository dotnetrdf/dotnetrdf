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

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Represents a Connection to a Pellet Server
    /// </summary>
    public class PelletServer
    {
        private String _serverUri;
        private List<KnowledgeBase> _kbs = new List<KnowledgeBase>();

        /// <summary>
        /// Preferred MIME Type for the format to retrieve the Server Description in
        /// </summary>
        private const String ServerDescriptionFormat = "text/json";

        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        public PelletServer(Uri serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            _serverUri = serverUri.AbsoluteUri;
            if (!_serverUri.EndsWith("/")) _serverUri += "/";

            Discover();
        }

        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        public PelletServer(String serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            if (serverUri.Equals(String.Empty)) throw new ArgumentException("Server URI cannot be the empty string", "serverUri");
            _serverUri = serverUri;
            if (!_serverUri.EndsWith("/")) _serverUri += "/";

            Discover();
        }

        /// <summary>
        /// Connects to a Pellet Server instance asynchronously invoking the callback when the connection is ready
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        /// <param name="callback">Callback to invoke when the connection is ready</param>
        /// <param name="state">State to pass to the callback</param>
        public static void Connect(Uri serverUri, PelletServerReadyCallback callback, Object state)
        {
            new PelletServer(serverUri, callback, state);
        }

        /// <summary>
        /// Connects to a Pellet Server instance asynchronously invoking the callback when the connection is ready
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        /// <param name="callback">Callback to invoke when the connection is ready</param>
        /// <param name="state">State to pass to the callback</param>
        public static void Connect(String serverUri, PelletServerReadyCallback callback, Object state)
        {
            new PelletServer(serverUri, callback, state);
        }

        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        /// <param name="callback">Callback to invoke when the connection is ready</param>
        /// <param name="state">State to pass to the callback</param>
        private PelletServer(Uri serverUri, PelletServerReadyCallback callback, Object state)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            _serverUri = serverUri.AbsoluteUri;
            if (!_serverUri.EndsWith("/")) _serverUri += "/";

            Discover(callback, state);
        }

        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        /// <param name="callback">Callback to invoke when the connection is ready</param>
        /// <param name="state">State to pass to the callback</param>
        private PelletServer(String serverUri, PelletServerReadyCallback callback, Object state)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            if (serverUri.Equals(String.Empty)) throw new ArgumentException("Server URI cannot be the empty string", "serverUri");
            _serverUri = serverUri;
            if (!_serverUri.EndsWith("/")) _serverUri += "/";

            Discover(callback, state);
        }

        /// <summary>
        /// Discovers the Knowledge Bases on a Server
        /// </summary>
        private void Discover()
        {
            try
            {
                // Make the request to the Server Root URL to get the JSON description of the server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serverUri);
                request.Method = "GET";
                request.Accept = ServerDescriptionFormat;

                Tools.HttpDebugRequest(request);

                // Get the response and parse the JSON
                String jsonText;
                JObject json;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    
                    // Get and parse the JSON
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();
                }

                JToken kbs = json.SelectToken("knowledge-bases");
                foreach (JToken kb in kbs.Children())
                {
                    _kbs.Add(new KnowledgeBase(kb));
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);

                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
            catch (Exception)
            {
                throw new RdfReasoningException("Error while attempting to discover Knowledge Bases on a Pellet Server");
            }
        }

        /// <summary>
        /// Discovers the Knowledge Bases on a Server asynchronously
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state"></param>
        private void Discover(PelletServerReadyCallback callback, Object state)
        {
            // Make the request to the Server Root URL to get the JSON description of the server
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_serverUri);
            request.Method = "GET";
            request.Accept = ServerDescriptionFormat;

            Tools.HttpDebugRequest(request);

            // Get the response and parse the JSON
            String jsonText;
            JObject json;
            request.BeginGetResponse(result =>
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                    
                    Tools.HttpDebugResponse(response);
                    
                    // Get and parse the JSON
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();

                    JToken kbs = json.SelectToken("knowledge-bases");
                    foreach (JToken kb in kbs.Children())
                    {
                        _kbs.Add(new KnowledgeBase(kb));
                    }

                    callback(this, state);
                }, null);
        }

        /// <summary>
        /// Gets the Knowledge Bases available from this Pellet Server
        /// </summary>
        public IEnumerable<KnowledgeBase> KnowledgeBases
        {
            get
            {
                return _kbs;
            }
        }

        /// <summary>
        /// Gets whether the Server has a Knowledge Base with the given Name
        /// </summary>
        /// <param name="name">Knowledge Base Name</param>
        /// <returns></returns>
        public bool HasKnowledgeBase(String name)
        {
            return _kbs.Any(kb => kb.Name.Equals(name));
        }

        /// <summary>
        /// Gets whether the Server has a Knowledge Base which supports the given Service Type
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public bool HasKnowledgeBase(Type t)
        {
            return _kbs.Any(kb => kb.SupportsService(t));
        }

        /// <summary>
        /// Gets the Knowledge Base with the given Name
        /// </summary>
        /// <param name="name">Knowledge Base Name</param>
        /// <returns>
        /// </returns>
        public KnowledgeBase GetKnowledgeBase(String name)
        {
            KnowledgeBase kb = _kbs.FirstOrDefault(k => k.Name.Equals(name));
            if (kb != null) return kb;
            throw new RdfReasoningException("This Pellet Server does not contain a Knowledge Base named '" + name + "'");
        }

        /// <summary>
        /// Gets all the Knowledge Bases which support a given Server
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public IEnumerable<KnowledgeBase> GetKnowledgeBases(Type t)
        {
            return (from kb in _kbs
                    where kb.SupportsService(t)
                    select kb);
        }

    }
}
