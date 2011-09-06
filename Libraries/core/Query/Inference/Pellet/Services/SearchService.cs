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
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Search Service provided by a Pellet Server
    /// </summary>
    public class SearchService
        : PelletService
    {
        private String _searchUri;

        /// <summary>
        /// Creates a new Search Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal SearchService(String name, JObject obj)
            : base(name, obj) 
        {
            this._searchUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf('{'));
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets the list of Search Results which match the given search term
        /// </summary>
        /// <param name="text">Search Term</param>
        /// <returns>A list of Search Results representing Nodes in the Knowledge Base that match the search term</returns>
        public List<SearchServiceResult> Search(String text)
        {
            String search = this._searchUri + "?search=" + Uri.EscapeDataString(text);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(search);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = "text/json";

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            String jsonText;
            JArray json;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugResponse(response);
                }
#endif
                jsonText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                json = JArray.Parse(jsonText);

                response.Close();
            }

            //Parse the Response into Search Results
            try
            {
                List<SearchServiceResult> results = new List<SearchServiceResult>();

                foreach (JToken result in json.Children())
                {
                    JToken hit = result.SelectToken("hit");
                    String type = (String)hit.SelectToken("type");
                    INode node;
                    if (type.ToLower().Equals("uri"))
                    {
                        node = new UriNode(null, new Uri((String)hit.SelectToken("value")));
                    }
                    else
                    {
                        node = new BlankNode(null, (String)hit.SelectToken("value"));
                    }
                    double score = (double)result.SelectToken("score");

                    results.Add(new SearchServiceResult(node, score));
                }

                return results;
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
                throw new RdfReasoningException("Error occurred while parsing Search Results from the Search Service", ex);
            }
        }

#endif

        /// <summary>
        /// Gets the list of Search Results which match the given search term
        /// </summary>
        /// <param name="text">Search Term</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Search(String text, PelletSearchServiceCallback callback, Object state)
        {
            String search = this._searchUri + "?search=" + Uri.EscapeDataString(text);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(search);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = "text/json";

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            String jsonText;
            JArray json;
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
                        json = JArray.Parse(jsonText);

                        response.Close();

                        //Parse the Response into Search Results

                        List<SearchServiceResult> results = new List<SearchServiceResult>();

                        foreach (JToken res in json.Children())
                        {
                            JToken hit = res.SelectToken("hit");
                            String type = (String)hit.SelectToken("type");
                            INode node;
                            if (type.ToLower().Equals("uri"))
                            {
                                node = new UriNode(null, new Uri((String)hit.SelectToken("value")));
                            }
                            else
                            {
                                node = new BlankNode(null, (String)hit.SelectToken("value"));
                            }
                            double score = (double)res.SelectToken("score");

                            results.Add(new SearchServiceResult(node, score));
                        }

                        callback(results, state);
                    }
                }, null);
        }
    }

    /// <summary>
    /// Represents a Search Result returned from the
    /// </summary>
    public class SearchServiceResult
    {
        private INode _n;
        private double _score;

        /// <summary>
        /// Creates a new Search Service Result
        /// </summary>
        /// <param name="node">Result Node</param>
        /// <param name="score">Result Score</param>
        internal SearchServiceResult(INode node, double score)
        {
            this._n = node;
            this._score = score;
        }

        /// <summary>
        /// Gets the Node for this Result
        /// </summary>
        public INode Node
        {
            get
            {
                return this._n;
            }
        }

        /// <summary>
        /// Gets the Score for this Result
        /// </summary>
        public double Score
        {
            get
            {
                return this._score;
            }
        }

        /// <summary>
        /// Gets the String representation of the Result
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._n.ToString() + ": " + this._score;
        }
    }
}
