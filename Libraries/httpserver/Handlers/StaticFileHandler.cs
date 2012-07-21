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
using VDS.Web.Configuration;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// A HTTP Listener Handler which serves a variety of File Formats as Plain Text/Binary Data
    /// </summary>
    public class StaticFileHandler 
        : IHttpListenerHandler
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
        /// Processes requests for static files
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

            if (path.EndsWith(new String(new char[] { Path.DirectorySeparatorChar })) || path.Equals(String.Empty))
            {
                //Directory Listing
                context.Server.RemapHandler(context, typeof(DirectoryListingHandler));
                return;
            }
            else
            {
                MimeTypeMapping mapping = context.Server.GetMimeType(Path.GetExtension(path));
                if (mapping != null)
                {
                    //Check File actually exists
                    if (!File.Exists(path))
                    {
                        //If File doesn't exist then 404
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    //Check whether the User supplied a If-Modified-Since header
                    String ifModSince = context.Request.Headers["If-Modified-Since"];
                    if (ifModSince != null)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(ifModSince, out dt))
                        {
                            dt = dt.ToUniversalTime();
                            DateTime modified = File.GetLastWriteTimeUtc(path);
                            if (modified <= dt)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                                return;
                            }
                        }
                    }

                    //Got here so Path is acceptable and file exists
                    //Return a 200 OK with appropriate MIME Type and Encoding
                    context.Response.ContentType = mapping.MimeType;
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.AddHeader("Last-Modified", File.GetLastWriteTimeUtc(path).ToRfc2822());

                    if (!context.Request.HttpMethod.Equals("HEAD"))
                    {
                        if (mapping.IsBinaryData)
                        {
                            context.Response.ContentLength64 = new FileInfo(path).Length;
                            using (StreamReader input = new StreamReader(path))
                            {
                                using (Stream output = context.Response.OutputStream)
                                {
                                    Tools.CopyStream(input.BaseStream, output);
                                    output.Close();
                                }
                                input.Close();
                            }
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(path, true))
                            {
                                //context.Response.ContentLength64 = new FileInfo(actualPath).Length;
                                context.Response.ContentEncoding = reader.CurrentEncoding;
                                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream, reader.CurrentEncoding))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        writer.WriteLine(reader.ReadLine());
                                    }
                                    writer.Close();
                                }
                                reader.Close();
                            }
                        }
                    }
                    return;
                }

                //If we get here then the extension is not servable
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }
}
