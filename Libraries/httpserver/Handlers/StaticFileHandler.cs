using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.Web.Configuration;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// A HTTP Listener Handler which serves a variety of File Formats as Plain Text/Binary Data
    /// </summary>
    public class StaticFileHandler : IHttpListenerHandler
    {
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

            if (path.EndsWith("/") || path.Equals(String.Empty))
            {
                //TODO: Directory Listing
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
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

                //If we get out of the foreach then the extension is not servable
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }
}
