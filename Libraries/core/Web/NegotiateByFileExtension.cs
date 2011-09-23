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

#if !NO_ASP && !NO_WEB

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using VDS.RDF.Parsing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Module that attempts to allow content negotiation by file extension wherever applicable
    /// </summary>
    public class NegotiateByFileExtension 
        : IHttpModule
    {
        /// <summary>
        /// Disposes of the Module
        /// </summary>
        public void Dispose()
        {
            //clean-up code here.
        }

        /// <summary>
        /// Intialises the Module
        /// </summary>
        /// <param name="context">HTTP Application</param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        /// <summary>
        /// Handles the start of requests by doing conneg wherever applicable
        /// </summary>
        /// <param name="sender">Sender of the Event</param>
        /// <param name="e">Event Arguments</param>
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app == null) return;
            HttpContext context = app.Context;
            if (context == null) return;

            if (context.Request.Url.AbsolutePath.Contains("."))
            {
                String actualPath = context.Request.MapPath(context.Request.Path);
                if (!File.Exists(actualPath))
                {
                    //Get the File Extension and see if it is for an RDF format
                    String ext = context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf("."));
                    switch (ext)
                    {
                        case ".aspx":
                        case ".asmx":
                        case ".ashx":
                        case ".axd":
                            //The above file extensions are special to ASP.Net and so may not actually exist as files
                            //so we need to ignore them for the purposes of negotiating by file extension
                            return;
                    }

                    try
                    {
                        List<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(ext)).ToList();
                        if (defs.Count == 0) return;

                        context.Request.Headers["Accept"] = String.Join(",", defs.Select(d => d.CanonicalMimeType).ToArray());
                        String filePath = Path.GetFileNameWithoutExtension(actualPath);
                        if (filePath == null || filePath.Equals(String.Empty))
                        {
                            if (context.Request.Url.AbsolutePath.EndsWith(ext)) filePath = context.Request.Url.AbsolutePath.Substring(0, context.Request.Url.AbsolutePath.Length - ext.Length);
                        }
                        String query = context.Request.Url.Query;
                        if (query.StartsWith("?")) query = query.Substring(1);
                        context.RewritePath(filePath, String.Empty, query, true);
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a RdfParserSelectionException we shouldn't do anything, this fixes bug CORE-94
                    }
                }
            }
        }
    }
}

#endif