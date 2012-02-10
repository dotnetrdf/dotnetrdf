/*

Copyright Robert Vesse 2009-12
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
using System.Net;
using VDS.RDF.Web.Configuration;
using VDS.Web;

namespace VDS.RDF.Utilities.Server
{
    /// <summary>
    /// Static Helper methods taken from the Handler Helper in the core library and modified to work with VDS.Web.Server as opposed to IIS
    /// </summary>
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
