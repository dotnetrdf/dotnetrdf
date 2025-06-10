/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using VDS.RDF.Query;

namespace VDS.RDF.Storage;

/// <summary>
/// Static Helper for the Storage API.
/// </summary>
public static class StorageHelper
{
    /// <summary>
    /// Template for posting form data as part of a HTTP multipart request.
    /// </summary>
    public const string HttpMultipartContentTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

    /// <summary>
    /// Gets a new unique boundary for HTTP mutlipart requests.
    /// </summary>
    public static string HttpMultipartBoundary
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
    /// Handles HTTP Query Errors obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <param name="webEx">HTTP Error.</param>
    /// <returns></returns>
    public static RdfQueryException HandleHttpQueryError(WebException webEx)
    {
        return HandleHttpError<RdfQueryException>(webEx, "querying", (msg, ex) => new RdfQueryException(msg, ex));
    }

    /// <summary>
    /// Handles HTTP Query Errors obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <param name="responseMessage">HTTP response.</param>
    /// <returns></returns>
    public static RdfQueryException HandleHttpQueryError(HttpResponseMessage responseMessage)
    {
        return HandleHttpError(responseMessage, "querying", msg => new RdfQueryException(msg));
    }

    /// <summary>
    /// Handles HTTP Errors obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <param name="webEx">HTTP Error.</param>
    /// <param name="action">Action being performed.</param>
    /// <returns></returns>
    public static RdfStorageException HandleHttpError(WebException webEx, string action)
    {
        return HandleHttpError<RdfStorageException>(webEx, action, (msg, ex) => new RdfStorageException(msg, ex));
    }

    /// <summary>
    /// Handles HTTP errors, obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <param name="response">HTTP response.</param>
    /// <param name="action">Action being performed.</param>
    /// <returns>Exception that can be raised to notify this error.</returns>
    public static RdfStorageException HandleHttpError(HttpResponseMessage response, string action)
    {
        return HandleHttpError(response, action, (msg) => new RdfStorageException(msg));
    }

    /// <summary>
    /// Handles HTTP Errors obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <param name="webEx">HTTP Error.</param>
    /// <param name="action">Action being performed.</param>
    /// <param name="errorProvider">Function that generates the actual errors.</param>
    /// <remarks>
    /// Adapted from Ron Michael's Zettlemoyer's original patch for this in Stardog to use it across all operations as far as possible.
    /// </remarks>
    public static T HandleHttpError<T>(WebException webEx, string action, Func<string, Exception, T> errorProvider)
        where T : Exception
    {
        if (webEx.Response != null)
        {
            if (webEx.Response.ContentLength > 0)
            {
                var responseText = "";
                try
                {
                    responseText = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
                }
                catch
                {
                    return errorProvider("A HTTP error " + GetStatusLine(webEx) + " occurred while " + action + " the Store.\nError getting response, see aforementioned status line or inner exception for further details", webEx);
                }
                return errorProvider("A HTTP error " + GetStatusLine(webEx) + " occured while " + action + " the Store.\nStore returned the following error message: " + responseText + "\nSee aforementioned status line or inner exception for further details", webEx);
            }
            return errorProvider("A HTTP error " + GetStatusLine(webEx) + " occurred while " + action + " the Store.\nEmpty response body, see aformentioned status line or the inner exception for further details", webEx);
        }
        return errorProvider("A HTTP error " + GetStatusLine(webEx) + " occurred while " + action + " the Store.\nNo response, see aforementioned status line or inner exception for further details", webEx);
    }

    /// <summary>
    /// Handles HTTP errors obtaining additional information from the HTTP response if possible.
    /// </summary>
    /// <typeparam name="T">Type of exception to raise.</typeparam>
    /// <param name="responseMessage">HTTP response message to process.</param>
    /// <param name="action">Action being performed when the response message was received.</param>
    /// <param name="errorProvider">Function that generates the exception to be raised.</param>
    /// <returns></returns>
    public static T HandleHttpError<T>(HttpResponseMessage responseMessage, string action,
        Func<string, T> errorProvider) where T : Exception
    {
        if (responseMessage.Content.Headers.ContentLength.HasValue &&
            responseMessage.Content.Headers.ContentLength > 0)
        {
            string responseText;
            try
            {
                responseText = responseMessage.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                return errorProvider("A HTTP error " + GetStatusLine(responseMessage) + " occurred while " + action + " the Store.\nError getting response, see aforementioned status line or inner exception for further details");
            }
            return errorProvider("A HTTP error " + GetStatusLine(responseMessage ) + " occurred while " + action + " the Store.\nStore returned the following error message: " + responseText + "\nSee aforementioned status line or inner exception for further details");
        }
        return errorProvider("A HTTP error " + GetStatusLine(responseMessage) + " occurred while " + action + " the Store.\nEmpty response body, see aforementioned status line or the inner exception for further details");
    }

    /// <summary>
    /// Tries to get the status line for inclusion in the HTTP error message.
    /// </summary>
    /// <param name="webEx">Web exception.</param>
    /// <returns>Status line if available, empty string otherwise.</returns>
    private static string GetStatusLine(WebException webEx)
    {
        if (webEx.Response != null)
        {
            var httpResponse = webEx.Response as HttpWebResponse;
            if (httpResponse != null)
            {
                return "(HTTP " + (int)httpResponse.StatusCode + " " + httpResponse.StatusDescription + ")";
            }
            return webEx.Status.ToSafeString();
        }
        return string.Empty;
    }

    private static string GetStatusLine(HttpResponseMessage responseMessage)
    {
        return $"(HTTP {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase})";
    }

    /// <summary>
    /// Handles Query Errors.
    /// </summary>
    /// <param name="ex">Error.</param>
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
    /// Handles Errors.
    /// </summary>
    /// <param name="ex">Error.</param>
    /// <param name="action">Action being performed.</param>
    /// <returns></returns>
    public static RdfStorageException HandleError(Exception ex, string action)
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
    /// Handles Errors.
    /// </summary>
    /// <typeparam name="T">Error Type.</typeparam>
    /// <param name="ex">Error.</param>
    /// <param name="action">Action being performed.</param>
    /// <param name="errorProvider">Function that generates the actual errors.</param>
    /// <returns></returns>
    public static T HandleError<T>(Exception ex, string action, Func<string, Exception, T> errorProvider)
        where T : Exception
    {
        return errorProvider("An unexpected error occurred while " + action + " the Store. See inner exception for further details", ex);
    }
}
