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
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any dataset that can be exposed via Fuseki
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses all three Services provided by a Fuseki instance - Query, Update and HTTP Update
    /// </para>
    /// </remarks>
    public class FusekiConnector : SparqlHttpProtocolConnector, IQueryableGenericIOManager
    {
        private SparqlFormatter _formatter = new SparqlFormatter();
        private String _updateUri;
        private String _queryUri;

        public FusekiConnector(Uri serviceUri)
            : this(serviceUri.ToSafeString()) { }

        public FusekiConnector(String serviceUri)
            : base(serviceUri) 
        {
            if (!serviceUri.ToString().EndsWith("/data")) throw new ArgumentException("This does not appear to be a valid Fuseki Server URI, you must provide the URI that ends with /data", "serviceUri");

            this._updateUri = serviceUri.Substring(0, serviceUri.Length - 4) + "update";
            this._queryUri = serviceUri.Substring(0, serviceUri.Length - 4) + "query";
        }

        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                String graph = (graphUri != null && !graphUri.Equals(String.Empty)) ? "GRAPH <" + this._formatter.FormatUri(graphUri) + "> {" : String.Empty;
                StringBuilder update = new StringBuilder();

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        update.AppendLine("INSERT DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in additions)
                        {
                            update.AppendLine(this._formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        if (update.Length > 0) update.AppendLine(";");

                        update.AppendLine("DELETE DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in removals)
                        {
                            update.AppendLine(this._formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (update.Length > 0)
                {
                    //Make the SPARQL Update Request
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._updateUri);
                    request.Method = "POST";
                    request.ContentType = "application/sparql-update";

                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(update.ToString());
                    writer.Close();

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        //If we get here without erroring then the request was OK
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw new RdfStorageException("A HTTP error occurred while communicating with the Fuseki Server", webEx);
            }
        }

        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }
    }
}
