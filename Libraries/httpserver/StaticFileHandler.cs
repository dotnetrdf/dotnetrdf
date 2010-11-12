using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web
{
    /// <summary>
    /// A HTTP Listener Handler which serves a variety of File Formats as Plain Text/Binary Data
    /// </summary>
    public class StaticFileHandler : IHttpListenerHandler
    {
        //TODO: Support a configuration file which determines which File Formats are valid
        List<MimeTypeMapping> _mappings = new List<MimeTypeMapping>()
        {
            new MimeTypeMapping(".html", "text/html"),
            new MimeTypeMapping(".htm", "text/html"),
            new MimeTypeMapping(".xhtml", "application/xhtml+xml"),
            new MimeTypeMapping(".xml", "application/xml"),
            new MimeTypeMapping(".css", "text/css"),
            new MimeTypeMapping(".js", "text/javascript"),
            new MimeTypeMapping(".txt", "text/plain"),
            new MimeTypeMapping(".jpg", "image/jpeg", true),
            new MimeTypeMapping(".jpeg", "image/jpeg", true),
            new MimeTypeMapping(".gif", "image/gif", true),
            new MimeTypeMapping(".png", "image/png", true),
            new MimeTypeMapping(".bmp", "image/bmp", true)
        };


        public bool IsReusable
        {
            get 
            {
                return true; 
            }
        }

        public void ProcessRequest(HttpListenerContext context, HttpServer server)
        {
            String baseDir = (String)server.State["BaseDirectory"];
            if (baseDir == null)
            {
                //If No Base Directory can't find anything
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            String path = context.Request.Url.AbsolutePath;
            if (path.Length > 1) path = path.Substring(1);

            if (path.Equals("/") || path.Equals(String.Empty))
            {
                //TODO: Directory Listing
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
            else
            {
                foreach (MimeTypeMapping mapping in this._mappings)
                {
                    if (path.EndsWith(mapping.Extension))
                    {
                        //Map Path to a File System Path
                        String actualPath = Path.Combine(baseDir, path.Replace('/', Path.DirectorySeparatorChar));
                        if (!actualPath.StartsWith(baseDir))
                        {
                            //Don't accept paths that try to go outside the base directory
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.Close();
                            return;
                        }
                        else if (!File.Exists(actualPath))
                        {
                            //If File doesn't exist then 404
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            context.Response.Close();
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
                                DateTime modified = File.GetLastWriteTimeUtc(actualPath);
                                if (modified <= dt)
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                                    context.Response.Close();
                                    return;
                                }
                            }
                        }

                        //Got here so Path is acceptable and file exists
                        //Return a 200 OK with appropriate MIME Type and Encoding
                        context.Response.ContentType = mapping.MimeType;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.AddHeader("Last-Modified", File.GetLastWriteTimeUtc(actualPath).ToRfc2822());

                        if (!context.Request.HttpMethod.Equals("HEAD"))
                        {
                            if (mapping.IsBinaryData)
                            {
                                using (StreamReader input = new StreamReader(actualPath))
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
                                using (StreamReader reader = new StreamReader(actualPath, true))
                                {
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

                        context.Response.Close();
                        return;
                    }
                }

                //If we get out of the foreach then the extension is not servable
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Close();
            }
        }
    }
}
