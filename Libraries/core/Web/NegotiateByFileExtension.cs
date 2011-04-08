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
using System.IO;
using System.Linq;
using System.Web;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A HTTP Module that attempts to allow content negotiation by file extension wherever applicable
    /// </summary>
    public class NegotiateByFileExtension : IHttpModule
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
                String actualPath = context.Request.MapPath(context.Request.FilePath);
                if (!File.Exists(actualPath))
                {
                    //Get the File Extension and see if it is for an RDF format
                    String ext = context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf("."));
                    MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(ext)).FirstOrDefault();
                    if (def == null) return;

                    context.Request.Headers["Accept"] = def.CanonicalMimeType;
                    context.RewritePath(Path.GetFileNameWithoutExtension(actualPath), context.Request.PathInfo, context.Request.Url.Query, true);
                }
            }
        }
    }
}
