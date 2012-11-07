/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
