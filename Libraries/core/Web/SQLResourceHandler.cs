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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_WEB && !NO_ASP && !NO_DATA && !NO_STORAGE

using System;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Web.Configuration.Resource;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A SQL Resource Handler attempts to retrieve any Uri it is invoked at as a Graph from the underlying Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// This Handler supports only one handler of this type being registered (multiple Handlers can be registered but their configuration will be shared if they get it from the same Web.config file)
    /// </para>
    /// <para>
    /// Each Handler registered in Web.config may have a prefix for their Configuration variables set by adding a AppSetting key using the type of the handler like so:
    /// <code>&lt;add key="VDS.RDF.Web.SqlResourceHandler" value="ABC" /&gt;</code>
    /// Then when the Handler at that path is invoked it will look for Configuration variables prefixed with that name.
    /// </para>
    /// <para>
    /// The Handler supports the same Database Configuration variables as the <see cref="SparqlHandler">SparqlHandler</see> and supports the following Configuration variables for itself:
    /// </para>
    /// <ul>
    /// <li><strong>LookupMode</strong> (<em>Optional</em>) - Sets how the Handler looks up URIs it is invoked with.  This should be one of the values from the <see cref="SQLResourceLookupMode">SQLResourceLookupMode</see> enumeration, see the enumeration documentation for how each mode behaves.  Default is <see cref="SQLResourceLookupMode.Graph">Graph</see></li>
    /// </ul>
    /// </remarks>
    [Obsolete("This class is considered obsolete - use a ProtocolHandler instead to achieve similar functionality", true)]
    public class SqlResourceHandler : BaseResourceHandler
    {
        private String _cacheKey = String.Empty;
        private SqlResourceHandlerConfiguration _config;

        /// <summary>
        /// Handles a Request for a Resource by retrieving it from the underlying SQL Store
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        public override void ProcessRequest(HttpContext context)
        {
            //Turn on Response Buffering
            context.Response.Buffer = true;

            //Add our Custom Headers
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            this.LoadConfig(context);

            SqlReader sqlreader;
            sqlreader = new SqlReader(this._config.Manager);

            try
            {
                //Initialise Variables
                Graph g = new Graph();
                String uri = this.RewriteURI(context.Request.Url.ToString());

                if (this._config.LookupMode == SQLResourceLookupMode.Graph || (this._config.LookupMode == SQLResourceLookupMode.GraphOrDescribe && this._config.Manager.Exists(new Uri(uri))))
                {
                    g = sqlreader.Load(uri);
                }
                else if (this._config.LookupMode == SQLResourceLookupMode.Describe || this._config.LookupMode == SQLResourceLookupMode.GraphOrDescribe)
                {
                    //Try to get all Triples with this Subject
                    try
                    {
                        this._config.Manager.Open(true);
                        String lookup = "SELECT * FROM TRIPLES T INNER JOIN NODES N ON T.tripleSubject=N.nodeID WHERE nodeType=" + (int)NodeType.Uri + " AND nodeValue=N'" + this._config.Manager.EscapeString(uri) + "'";
                        DataTable data = this._config.Manager.ExecuteQuery(lookup);

                        if (data.Rows.Count == 0)
                        {
                            this._config.Manager.Close(true);
                            context.Response.Clear();
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return;
                        }
                        else
                        {
                            //Load into the Graph
                            foreach (DataRow row in data.Rows)
                            {
                                INode s, p, o;
                                s = this._config.Manager.LoadNode(g, row["tripleSubject"].ToString());
                                p = this._config.Manager.LoadNode(g, row["triplePredicate"].ToString());
                                o = this._config.Manager.LoadNode(g, row["tripleObject"].ToString());

                                //Exclude Triples with BNode Objects
                                if (o.NodeType != NodeType.Blank)
                                {
                                    g.Assert(new Triple(s, p, o));
                                }
                            }
                        }
                    }
                    finally
                    {
                        this._config.Manager.Close(true);
                    }
                }
                else
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                //Get the appropriate Writer
                String ctype;
                IRdfWriter writer;
                if (context.Request.AcceptTypes != null)
                {
                    writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
                }
                else
                {
                    //Default To RDF/XML if no accept header
                    writer = new FastRdfXmlWriter();
                    ctype = MimeTypesHelper.RdfXml[0];
                }
                context.Response.ContentType = ctype;

                //Clear any existing Response
                context.Response.Clear();

                //Write the Graph to the Client
                writer.Save(g, new StreamWriter(context.Response.OutputStream));
            }
            catch
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
        }

        /// <summary>
        /// Loads the Handler specific Configuration for this Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="cacheKey">Cache Key</param>
        /// <param name="prefix">Config Variable Prefix</param>
        protected override void LoadConfigInternal(HttpContext context, String cacheKey, String prefix)
        {
            if (context.Cache[cacheKey] == null)
            {
                //No Config Cached so create a new Config object which will load the Config and cache the Manager
                this._config = new SqlResourceHandlerConfiguration(context, cacheKey, prefix);
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 15, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Retrieve from Cache
                this._config = (SqlResourceHandlerConfiguration)context.Cache[cacheKey];
            }
        }
    }
}

#endif