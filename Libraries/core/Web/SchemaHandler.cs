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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.IO;
using System.Net;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Handler for serving Schemas mapped to different URIs within your web application
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single instance of the SchemaHandler can be registered per application.  It's configuration specifies a number of RDF files which contain Schemas.  When a request comes in on a Uri which is mapped to the Handler the Handler will look for that Uri defined as having <em>rdf:type</em> of <em>rdfs:Class</em> or <em>rdf:Property</em> in any of the schemas.  If it finds the Uri in one of the schemas it serves that Graph in a format supported by the requesting client.
    /// </para>
    /// <para>
    /// You register one/more Schemas to the Handler using the following configuration variable.  The <strong>X</strong> in these examples indicate a 1 based index:
    /// <ul>
    ///     <li><strong>SchemaX</strong> - Registers the Schema contained in the file with the Handler</li>
    /// </ul>
    /// For example:
    /// <code>
    /// &lt;add key="Schema1" value="MySchema.rdf" /&gt;
    /// &lt;add key="Schema2" value="MyOtherSchema.ttl" /&gt;
    /// </code>
    /// Format of the RDF in each file is determined based upon the file extension.
    /// </para>
    /// </remarks>
    [Obsolete("This class is deprecated in favour of using a BaseGraphHandler and one of it's concrete implementations GraphHandler (for hash based schemas) or WildcardGraphHandler (for slash based schemas)", true)]
    public class SchemaHandler : IHttpHandler
    {
        private TripleStore _store = null;

        /// <summary>
        /// Returns that this Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Processes requests by looking for Schemas that either have the requested Uri as their Base Uri or that have a Class/Property defined in them with that Uri
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Buffer = true;

            if (this._store == null) this.LoadInternal(context);

            Uri u = context.Request.Url;
            UriNode subj = new UriNode(null, u);
            UriNode rdfType = new UriNode(null, new Uri(NamespaceMapper.RDF + "type"));
            UriNode rdfsClass = new UriNode(null, new Uri(NamespaceMapper.RDFS + "Class"));
            UriNode rdfProperty = new UriNode(null, new Uri(NamespaceMapper.RDF + "Property"));
            Triple classDef = new Triple(subj, rdfType, rdfsClass);
            Triple propDef = new Triple(subj, rdfType, rdfProperty);

            IGraph output = null;
            foreach (IGraph g in this._store.Graphs)
            {
                //See if the Uri matches the Base Uri
                if (g.BaseUri != null)
                {
                    if (g.BaseUri.ToString().Equals(subj.Uri.ToString(), StringComparison.Ordinal))
                    {
                        output = g;
                        break;
                    }
                    else if (!subj.Uri.ToString().Contains("#"))
                    {
                        String temp = subj.Uri.ToString() + "#";
                        if (g.BaseUri.ToString().Equals(temp, StringComparison.Ordinal))
                        {
                            output = g;
                            break;
                        }
                    }
                }

                //See if the Uri is of a Class/Property
                if (g.ContainsTriple(classDef))
                {
                    output = g;
                    break;
                }
                else if (g.ContainsTriple(propDef))
                {
                    output = g;
                    break;
                }
            }

            if (output != null)
            {
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
                writer.Save(output, context.Response.Output);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                //context.Response.Write("Couldn't find Uri '" + subj.ToString() + "' in a Schema");
            }
        }

        /// <summary>
        /// Loads the Schema files specified in the Configuration
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        private void LoadInternal(HttpContext context)
        {
            this._store = new TripleStore();
            int i = 1;
            while (ConfigurationManager.AppSettings["Schema" + i] != null)
            {
                try
                {
                    String path = ConfigurationManager.AppSettings["Schema" + i];
                    if (!Path.IsPathRooted(path))
                    {
                        path = context.Server.MapPath(path);
                    }
                    Graph g = new Graph();
                    FileLoader.Load(g, path);

                    this._store.Add(g);
                }
                catch (Exception ex)
                {
                    //Wrap the error
                    throw new RdfException("The Schema '" + ConfigurationManager.AppSettings["Schema" + i] + "' specified for the Schema Handler could not be loaded", ex);
                }

                i++;
            }
        }
    }
}

#endif