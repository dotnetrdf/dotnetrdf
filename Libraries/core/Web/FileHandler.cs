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
using System.Configuration;
using System.IO;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Handler for serving individual RDF files at a fixed path e.g. a FOAF file
    /// </summary>
    /// <remarks>
    /// <para>
    /// The configuration for this Handler is very simple, simply create an entry in the  &lt;appSettings&gt; section of your Web.config file with the path you map the Handler to as the key and the path to the file as the value e.g.
    /// </para>
    /// <pre>
    /// &lt;add key="/directory/name" value="~/App_Data/data.rdf" /&gt;
    /// </pre>
    /// <para>
    /// <strong>Note:</strong> If the file you are serving is a Schema then we strongly recommend you use the <see cref="SchemaHandler">SchemaHandler</see> instead.
    /// </para>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseGraphHandler and its concrete implementation GraphHandler", true)]
    public class FileHandler : IHttpHandler
    {
        /// <summary>
        /// Gets that the Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            { 
                return true; 
            }
        }

        /// <summary>
        /// Processes the request for the File and returns a Graph
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.Buffer = true;

            //Get the File we are serving
            String path = context.Request.Path;
            if (context.Cache[path] != null)
            {
                Object cached = context.Cache[path];
                if (cached is Graph)
                {
                    Graph g = (Graph)cached;

                    //Output to the Client in a format suitable for them
                    String ctype;
                    IRdfWriter writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
                    context.Response.Clear();
                    context.Response.ContentType = ctype;
                    writer.Save(g, new StreamWriter(context.Response.OutputStream));
                }
            }

            if (ConfigurationManager.AppSettings[path] != null)
            {
                //Map the Path to an absolute path
                String file = ConfigurationManager.AppSettings[path];
                if (!Path.IsPathRooted(file))
                {
                    file = context.Server.MapPath(file);
                }

                //Load the Graph 
                Graph g = new Graph();
                FileLoader.Load(g, file);

                //Cache the Graph
                context.Cache.Add(path, g, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 15, 0), System.Web.Caching.CacheItemPriority.Normal, null);

                //Output to the Client in a format suitable for them
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
                context.Response.Clear();
                context.Response.ContentType = ctype;
                writer.Save(g, new StreamWriter(context.Response.OutputStream));
            }
            else
            {
                throw new RdfException("Required Configuration for FileHandler was not found - expected an AppSetting with the key '" + path + "' whose value is a Path to the file to be served");
            }
        }
    }
}

#endif