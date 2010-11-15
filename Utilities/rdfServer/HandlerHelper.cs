using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Web.Configuration;
using VDS.Web;

namespace rdfServer
{
    public static class HandlerHelper
    {
        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        public static void HandleQueryErrors(HttpServerContext context, BaseHandlerConfiguration config, String title, String query, Exception ex)
        {
            HandleQueryErrors(context, config, title, query, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        public static void HandleQueryErrors(HttpServerContext context, BaseHandlerConfiguration config, String title, String query, Exception ex, int statusCode)
        {
            //Clear any existing Response and set our HTTP Status Code
            context.Response.StatusCode = statusCode;

            if (config != null)
            {
                //If not showing errors then we won't return our custom error description
                if (!config.ShowErrors) return;
            }

            //Set to Plain Text output and report the error
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";

            //Error Title
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(title + "\n");
                writer.Write(new String('-', title.Length) + "\n\n");

                //Output Query with Line Numbers
                if (query != null && !query.Equals(String.Empty))
                {
                    String[] lines = query.Split('\n');
                    for (int l = 0; l < lines.Length; l++)
                    {
                        writer.Write((l + 1) + ": " + lines[l] + "\n");
                    }
                    writer.Write("\n\n");
                }

                //Error Message
                writer.Write(ex.Message + "\n");

#if DEBUG
                //Stack Trace only when Debug build
                writer.Write(ex.StackTrace + "\n\n");
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    writer.Write(ex.Message + "\n");
                    writer.Write(ex.StackTrace + "\n\n");
                }
#endif
                writer.Close();
            }
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        public static void HandleUpdateErrors(HttpServerContext context, BaseHandlerConfiguration config, String title, String update, Exception ex)
        {
            HandleUpdateErrors(context, config, title, update, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        public static void HandleUpdateErrors(HttpServerContext context, BaseHandlerConfiguration config, String title, String update, Exception ex, int statusCode)
        {
            if (config != null)
            {
                if (!config.ShowErrors)
                {
                    context.Response.StatusCode = statusCode;
                    return;
                }
            }

            //Set to Plain Text output and report the error
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";

            //Error Title
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(title + "\n");
                writer.Write(new String('-', title.Length) + "\n\n");

                //Output Query with Line Numbers
                if (update != null && !update.Equals(String.Empty))
                {
                    String[] lines = update.Split('\n');
                    for (int l = 0; l < lines.Length; l++)
                    {
                        writer.Write((l + 1) + ": " + lines[l] + "\n");
                    }
                    writer.Write("\n\n");
                }

                //Error Message
                writer.Write(ex.Message + "\n");

#if DEBUG
                //Stack Trace only when Debug build
                writer.Write(ex.StackTrace + "\n\n");
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    writer.Write(ex.Message + "\n");
                    writer.Write(ex.StackTrace + "\n\n");
                }
#endif
                writer.Close();
            }
        }

    }
}
