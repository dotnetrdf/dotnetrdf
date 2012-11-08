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
