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

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        public PelletServer(Uri serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            this._serverUri = serverUri.ToString();
            if (!this._serverUri.EndsWith("/")) this._serverUri += "/";

            this.Discover();
        }

        /// <summary>
        /// Creates a new connection to a Pellet Server
        /// </summary>
        /// <param name="serverUri">Server URI</param>
        public PelletServer(String serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri", "A Server URI must be specified in order to connect to a Pellet Server");
            if (serverUri.Equals(String.Empty)) throw new ArgumentException("Server URI cannot be the empty string", "serverUri");
            this._serverUri = serverUri;
            if (!this._serverUri.EndsWith("/")) this._serverUri += "/";

            this.Discover();
        }
#endif

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
            this._serverUri = serverUri.ToString();
            if (!this._serverUri.EndsWith("/")) this._serverUri += "/";

            this.Discover(callback, state);
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
            this._serverUri = serverUri;
            if (!this._serverUri.EndsWith("/")) this._serverUri += "/";

            this.Discover(callback, state);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Discovers the Knowledge Bases on a Server
        /// </summary>
        private void Discover()
        {
            try
            {
                //Make the request to the Server Root URL to get the JSON description of the server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._serverUri);
                request.Method = "GET";
                request.Accept = ServerDescriptionFormat;

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                //Get the response and parse the JSON
                String jsonText;
                JObject json;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //Get and parse the JSON
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();
                }

                JToken kbs = json.SelectToken("knowledge-bases");
                foreach (JToken kb in kbs.Children())
                {
                    this._kbs.Add(new KnowledgeBase(kb));
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
            catch (Exception)
            {
                throw new RdfReasoningException("Error while attempting to discover Knowledge Bases on a Pellet Server");
            }
        }

#endif
        /// <summary>
        /// Discovers the Knowledge Bases on a Server asynchronously
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state"></param>
        private void Discover(PelletServerReadyCallback callback, Object state)
        {
            //Make the request to the Server Root URL to get the JSON description of the server
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._serverUri);
            request.Method = "GET";
            request.Accept = ServerDescriptionFormat;


#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            //Get the response and parse the JSON
            String jsonText;
            JObject json;
            request.BeginGetResponse(result =>
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //Get and parse the JSON
                    jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    json = JObject.Parse(jsonText);

                    response.Close();

                    JToken kbs = json.SelectToken("knowledge-bases");
                    foreach (JToken kb in kbs.Children())
                    {
                        this._kbs.Add(new KnowledgeBase(kb));
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
                return this._kbs;
            }
        }

        /// <summary>
        /// Gets whether the Server has a Knowledge Base with the given Name
        /// </summary>
        /// <param name="name">Knowledge Base Name</param>
        /// <returns></returns>
        public bool HasKnowledgeBase(String name)
        {
            return this._kbs.Any(kb => kb.Name.Equals(name));
        }

        /// <summary>
        /// Gets whether the Server has a Knowledge Base which supports the given Service Type
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public bool HasKnowledgeBase(Type t)
        {
            return this._kbs.Any(kb => kb.SupportsService(t));
        }

        /// <summary>
        /// Gets the Knowledge Base with the given Name
        /// </summary>
        /// <param name="name">Knowledge Base Name</param>
        /// <returns>
        /// </returns>
        public KnowledgeBase GetKnowledgeBase(String name)
        {
            KnowledgeBase kb = this._kbs.FirstOrDefault(k => k.Name.Equals(name));
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
            return (from kb in this._kbs
                    where kb.SupportsService(t)
                    select kb);
        }

    }
}
