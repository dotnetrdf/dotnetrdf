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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Static Helper for the Storage API
    /// </summary>
    public static class StorageHelper
    {
        /// <summary>
        /// Template for posting form data as part of a HTTP multipart request
        /// </summary>
        public const String HttpMultipartContentTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

        /// <summary>
        /// Gets a new unique boundary for HTTP mutlipart requests
        /// </summary>
        public static String HttpMultipartBoundary
        {
            get
            {
                Guid guid;
                do
                {
                    guid = Guid.NewGuid();
                } while (guid.Equals(Guid.Empty));
                return "--" + guid + "--";
            }
        }

        /// <summary>
        /// Handles HTTP Query Errors obtaining additional information from the HTTP response if possible
        /// </summary>
        /// <param name="webEx">HTTP Error</param>
        /// <returns></returns>
        public static RdfQueryException HandleHttpQueryError(WebException webEx)
        {
            return HandleHttpError<RdfQueryException>(webEx, "querying", (msg, ex) => new RdfQueryException(msg, ex));
        }

        /// <summary>
        /// Handles HTTP Errors obtaining additional information from the HTTP response if possible
        /// </summary>
        /// <param name="webEx">HTTP Error</param>
        /// <param name="action">Action being performed</param>
        /// <returns></returns>
        public static RdfStorageException HandleHttpError(WebException webEx, String action)
        {
            return HandleHttpError<RdfStorageException>(webEx, action, (msg, ex) => new RdfStorageException(msg, ex));
        }

        /// <summary>
        /// Handles HTTP Errors obtaining additional information from the HTTP response if possible
        /// </summary>
        /// <param name="webEx">HTTP Error</param>
        /// <param name="action">Action being performed</param>
        /// <param name="errorProvider">Function that generates the actual errors</param>
        /// <remarks>
        /// Adapted from Ron Michael's Zettlemoyer's original patch for this in Stardog to use it across all operations as far as possible
        /// </remarks>
        public static T HandleHttpError<T>(WebException webEx, String action, Func<String, Exception, T> errorProvider)
            where T : Exception
        {
            if (webEx.Response != null)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                if (webEx.Response.ContentLength > 0)
                {
                    String responseText = "";
                    try
                    {
                        responseText = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch
                    {
                        return errorProvider("A HTTP error occurred while " + action + " the Store. Error getting response, see inner exception for further details", webEx);
                    }
                    return errorProvider("A HTTP error occured while " + action + " the Store. Store returned the following error message: " + responseText + "\nSee inner exception for further details", webEx);
                }
                else
                {
                    return errorProvider("A HTTP error occurred while " + action + " the Store. Empty response, see inner exception for further details", webEx);
                }
            }
            else
            {
                return errorProvider("A HTTP error occurred while " + action + " the Store. No response, see inner exception for further details", webEx);
            }
        }

        /// <summary>
        /// Handles Query Errors
        /// </summary>
        /// <param name="ex">Error</param>
        /// <returns></returns>
        public static RdfQueryException HandleQueryError(Exception ex)
        {
            if (ex is WebException)
            {
                return HandleHttpQueryError((WebException)ex);
            }
            else
            {
                return HandleError<RdfQueryException>(ex, "querying", (msg, e) => new RdfQueryException(msg, e));
            }
        }

        /// <summary>
        /// Handles Errors
        /// </summary>
        /// <param name="ex">Error</param>
        /// <param name="action">Action being performed</param>
        /// <returns></returns>
        public static RdfStorageException HandleError(Exception ex, String action)
        {
            if (ex is WebException)
            {
                return HandleHttpError((WebException)ex, action);
            }
            else
            {
                return HandleError<RdfStorageException>(ex, action, (msg, e) => new RdfStorageException(msg, e));
            }
        }

        /// <summary>
        /// Handles Errors
        /// </summary>
        /// <typeparam name="T">Error Type</typeparam>
        /// <param name="ex">Error</param>
        /// <param name="action">Action being performed</param>
        /// <param name="errorProvider">Function that generates the actual errors</param>
        /// <returns></returns>
        public static T HandleError<T>(Exception ex, String action, Func<String, Exception, T> errorProvider)
            where T : Exception
        {
            return errorProvider("An unexpected error occurred while " + action + " the Store. See inner exception for further details", ex);
        }
    }
}
