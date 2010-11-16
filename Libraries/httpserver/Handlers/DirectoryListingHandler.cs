using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web.Handlers
{
    public class DirectoryListingHandler : IHttpListenerHandler
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

        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

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
