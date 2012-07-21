/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Net;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// A Handler for generating directory listings
    /// </summary>
    public class DirectoryListingHandler
        : IHttpListenerHandler
    {
        /// <summary>
        /// Constant indicating that Directory Listing is Disabled
        /// </summary>
        public const String DirectoryListModeDisabled = "Disabled";
        /// <summary>
        /// Constant indicating that Directory Listing is set to Partial (default)
        /// </summary>
        public const String DirectoryListModePartial = "Partial";
        /// <summary>
        /// Constant indicating that Directory Listing is set to Full
        /// </summary>
        public const String DirectoryListModeFull = "Full";

        /// <summary>
        /// Indicates that the handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Processes a request
        /// </summary>
        /// <param name="context">Server Context</param>
        public void ProcessRequest(HttpServerContext context)
        {
            String path = context.Server.MapPath(context.Request.Url.AbsolutePath);
            if (path == null)
            {
                //Don't accept paths that are invalid
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            //Check State to see if DirectoryListingMode has been set
            String mode = DirectoryListModePartial;
            if (context.Server.State["DirectoryListingMode"] != null)
            {
                switch ((String)context.Server.State["DirectoryListingMode"])
                {
                    case DirectoryListModeDisabled:
                        mode = DirectoryListModeDisabled;
                        break;
                    case DirectoryListModeFull:
                        mode = DirectoryListModeFull;
                        break;
                    case DirectoryListModePartial:
                        mode = DirectoryListModePartial;
                        break;
                }
            }

            switch (mode)
            {
                case DirectoryListModeDisabled:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;

                case DirectoryListModeFull:
                case DirectoryListModePartial:
                    if (Directory.Exists(path))
                    {
                        context.Response.ContentType = "text/plain";

                        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                        {
                            foreach (String file in Directory.GetFiles(path))
                            {
                                String relPath = Path.GetFileName(file);
                                if (mode.Equals(DirectoryListModePartial))
                                {
                                    if (context.Server.GetMimeType(Path.GetExtension(relPath)) != null)
                                    {
                                        writer.WriteLine(relPath);
                                    }
                                }
                                else
                                {
                                    writer.WriteLine(relPath);
                                }
                            }
                            writer.Close();
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    break;
            }
        }
    }
}
