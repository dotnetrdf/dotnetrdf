/*

Copyright Robert Vesse 2009-10
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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Web.Configuration;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Static Helper class for HTTP Handlers
    /// </summary>
    public static class HandlerHelper
    {
        /// <summary>
        /// Checks whether a User is authenticated (or guests are permitted)
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="groups">User Groups to test against</param>
        /// <returns></returns>
        public static bool IsAuthenticated(HttpContext context, IEnumerable<UserGroup> groups)
        {
            if (groups.Any())
            {
                //Have we had credentials provided to us?
                if (context.Request.Headers["Authorization"] != null)
                {
                    String authDetails = context.Request.Headers["Authorization"];
                    if (authDetails.StartsWith("Basic"))
                    {
                        authDetails = authDetails.Substring(authDetails.IndexOf(' ') + 1);
                        authDetails = new String(Convert.FromBase64String(authDetails).Select(b => (char)b).ToArray());
                        String user = authDetails.Substring(0, authDetails.IndexOf(':'));
                        String pwd = authDetails.Substring(authDetails.IndexOf(':') + 1);

                        //Does any Group have this Member?
                        if (!groups.Any(g => g.HasMember(user, pwd)))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return false;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return false;
                    }
                }
                else if (!groups.Any(g => g.AllowGuests))
                {
                    //No Groups allow guests so we require authentication
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    try
                    {
                        context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"" + context.Request.Path + "\"");
                    }
                    catch (PlatformNotSupportedException)
                    {
                        context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"" + context.Request.Path + "\"");
                    }
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether a User is authenticated (or guests are permitted) and the given action is allowed
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="groups">User Groups to test against</param>
        /// <param name="action">Action to check for permission for</param>
        /// <returns></returns>
        public static bool IsAuthenticated(HttpContext context, IEnumerable<UserGroup> groups, String action)
        {
            if (groups.Any())
            {
                //Have we had credentials provided to us?
                if (context.Request.Headers["Authorization"] != null)
                {
                    String authDetails = context.Request.Headers["Authorization"];
                    if (authDetails.StartsWith("Basic"))
                    {
                        authDetails = authDetails.Substring(authDetails.IndexOf(' ') + 1);
                        authDetails = new String(Convert.FromBase64String(authDetails).Select(b => (char)b).ToArray());
                        String user = authDetails.Substring(0, authDetails.IndexOf(':'));
                        String pwd = authDetails.Substring(authDetails.IndexOf(':') + 1);

                        //Does any Group have this Member and allow this action?
                        if (!groups.Any(g => g.HasMember(user, pwd) && g.IsActionPermitted(context.Request.HttpMethod)))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return false;
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return false;
                    }
                }
                else if (!groups.Any(g => g.AllowGuests))
                {
                    //No Groups allow guests so we require authentication
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    try
                    {
                        context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"" + context.Request.Path + "\"");
                    }
                    catch (PlatformNotSupportedException)
                    {
                        context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"" + context.Request.Path + "\"");
                    }
                    return false;
                }
                else
                {
                    //No Autorization so does a Group that allows guests allow this action?
                    if (!groups.Any(g => g.AllowGuests && g.IsActionPermitted(context.Request.HttpMethod)))
                    {
                        //There are no Groups that allow guests and allow this action so this is forbidden
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        public static void HandleQueryErrors(HttpContext context, BaseHandlerConfiguration config, String title, String query, Exception ex)
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
        public static void HandleQueryErrors(HttpContext context, BaseHandlerConfiguration config, String title, String query, Exception ex, int statusCode)
        {
            //Clear any existing Response and set our HTTP Status Code
            context.Response.Clear();
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
            context.Response.Write(title + "\n");
            context.Response.Write(new String('-', title.Length) + "\n\n");

            //Output Query with Line Numbers
            if (query != null && !query.Equals(String.Empty))
            {
                String[] lines = query.Split('\n');
                for (int l = 0; l < lines.Length; l++)
                {
                    context.Response.Write((l + 1) + ": " + lines[l] + "\n");
                }
                context.Response.Write("\n\n");
            }

            //Error Message
            context.Response.Write(ex.Message + "\n");

#if DEBUG
            //Stack Trace only when Debug build
            context.Response.Write(ex.StackTrace + "\n\n");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                context.Response.Write(ex.Message + "\n");
                context.Response.Write(ex.StackTrace + "\n\n");
            }
#endif
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        public static void HandleUpdateErrors(HttpContext context, BaseHandlerConfiguration config, String title, String update, Exception ex)
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
        public static void HandleUpdateErrors(HttpContext context, BaseHandlerConfiguration config, String title, String update, Exception ex, int statusCode)
        {
            //Clear any existing Response
            context.Response.Clear();

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
            context.Response.Write(title + "\n");
            context.Response.Write(new String('-', title.Length) + "\n\n");

            //Output Query with Line Numbers
            if (update != null && !update.Equals(String.Empty))
            {
                String[] lines = update.Split('\n');
                for (int l = 0; l < lines.Length; l++)
                {
                    context.Response.Write((l + 1) + ": " + lines[l] + "\n");
                }
                context.Response.Write("\n\n");
            }

            //Error Message
            context.Response.Write(ex.Message + "\n");

#if DEBUG
            //Stack Trace only when Debug build
            context.Response.Write(ex.StackTrace + "\n\n");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                context.Response.Write(ex.Message + "\n");
                context.Response.Write(ex.StackTrace + "\n\n");
            }
#endif
        }
    }
}

#endif