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
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the SPARQL Query Service provided by a Pellet Server knowledge base
    /// </summary>
    public class QueryService : PelletService
    {
        private String _sparqlUri;

        /// <summary>
        /// Creates a new SPARQL Query Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal QueryService(String name, JObject obj)
            : base(name, obj)
        {
            this._sparqlUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf('{'));
        }

        /// <summary>
        /// Makes a SPARQL Query against the Knowledge Base
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
#if !SILVERLIGHT
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(this._sparqlUri));

            using (HttpWebResponse response = endpoint.QueryRaw(sparqlQuery))
            {
                try
                {
                    ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    SparqlResultSet results = new SparqlResultSet();
                    sparqlParser.Load(results, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return results;
                }
                catch (RdfParserSelectionException)
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return g;
                }
                response.Close();
            }
#else
            throw new PlatformNotSupportedException("Not currently supported on Silverlight/Windows Phone 7");
#endif
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
#if !SILVERLIGHT
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(this._sparqlUri));

            using (HttpWebResponse response = endpoint.QueryRaw(sparqlQuery))
            {
                try
                {
                    ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    sparqlParser.Load(resultsHandler, new StreamReader(response.GetResponseStream()));
                }
                catch (RdfParserSelectionException)
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(rdfHandler, new StreamReader(response.GetResponseStream()));
                }
                response.Close();
            }
#else
            throw new PlatformNotSupportedException("Not currently supported under Silverlight/Windows Phone 7");
#endif
        }
    }
}
