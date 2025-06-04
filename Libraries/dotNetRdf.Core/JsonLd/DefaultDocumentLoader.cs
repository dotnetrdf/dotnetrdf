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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Implements the default remote JSON document loader logic for the JSON-LD processor.
/// </summary>
public static class DefaultDocumentLoader
{
    /// <summary>
    /// Get / Set the limit to the size (in bytes) of response content handled by the document loader.
    /// </summary>
    /// <remarks>This is initially set to 2097152 (2MB).</remarks>
    public static long MaxResponseContentBufferSize
    {
        get { return Client.MaxResponseContentBufferSize; }
        set { Client.MaxResponseContentBufferSize = value; }
    }

    /// <summary>
    /// Get / Set the maximum number of redirects to follow automatically.
    /// </summary>
    /// <remarks>This is initially set to 5. Setting to 0 disables auto redirect.</remarks>
    public static int MaxRedirects
    {
        get { return Handler.MaxAutomaticRedirections; }
        set
        {
            Handler.MaxAutomaticRedirections = value;
            Handler.AllowAutoRedirect = value > 0;
        }
    }

    private static readonly HttpClientHandler Handler = new()
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 5,
    };
    
    private static readonly HttpClient Client = new(Handler)
    {
        DefaultRequestHeaders =
        {
            Accept =
            {
                new MediaTypeWithQualityHeaderValue("application/ld+json", 1.0),
                new MediaTypeWithQualityHeaderValue("application/json", 0.9),
                new MediaTypeWithQualityHeaderValue("*/*+json", 0.8),
            },
        },
        MaxResponseContentBufferSize = 2 * 1024 * 1024,
    };


    /// <summary>
    /// Attempt to retrieve a JSON-LD document following the standard rules for following Link headers.
    /// </summary>
    /// <param name="remoteRef">The remote document reference to be resolved.</param>
    /// <param name="loaderOptions">Options to apply to the loading process.</param>
    /// <returns>The loaded remote document.</returns>
    /// <exception cref="JsonLdProcessorException">Raised if the remote reference could not be de-referenced to a processable JSON document.</exception>
    /// <exception cref="WebException">Raised if an error was encountered when retrieving content from the remote server.</exception>
    public static RemoteDocument LoadJson(Uri remoteRef, JsonLdLoaderOptions loaderOptions)
    {
        HttpResponseMessage responseMessage = Client.GetAsync(remoteRef).Result;
        var responseData = responseMessage.Content.ReadAsByteArrayAsync().Result;
        MediaTypeHeaderValue contentType = responseMessage.Content.Headers.ContentType;
        responseMessage.Content.Headers.TryGetValues("Link", out IEnumerable<string> linkHeaders);

        var matchesContentType =
            contentType != null &&
            (contentType.MediaType == "application/json" ||
            (contentType.MediaType == "application/ld+json" && string.IsNullOrEmpty(loaderOptions.RequestProfile)) ||
            (contentType.MediaType == "application/ld+json" && 
             !string.IsNullOrEmpty(loaderOptions.RequestProfile) &&
             contentType.Parameters.Any(p => 
                 p.Name == "profile" && p.Value.ToString() == loaderOptions.RequestProfile)) ||
            contentType.MediaType.Contains("+json"));
        string contextLink = null;
        if (!matchesContentType)
        {
            IEnumerable<WebLink> contextLinks = ParseLinkHeaders(linkHeaders);
            WebLink alternateLink = contextLinks.FirstOrDefault(link =>
                link.RelationTypes.Contains("alternate") && link.MediaTypes.Contains("application/ld+json"));
            if (alternateLink != null)
            {
                return LoadJson(new Uri(remoteRef, alternateLink.LinkValue), loaderOptions);
            }
            else
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed,
                    "Loading document failed. The server did not respond with a processable JSON document.");
            }
        }

        // If content type is application/ld+json the context link header is ignored
        if (contentType.MediaType != "application/ld+json")
        {
            var contextLinks = ParseLinkHeaders(linkHeaders)
                .Where(x => x.RelationTypes.Contains(JsonLdVocabulary.Context))
                .Select(x => x.LinkValue).ToList();
            if (contextLinks.Count > 1)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.MultipleContextLinkHeaders,
                    "Multiple context link headers");
            }

            contextLink = contextLinks.FirstOrDefault();
        }

        // Use the content of the response decoded according to the charset specified in the content-type header (or UTF-8 if not specified)
        Encoding responseEncoding = Encoding.UTF8;
        try
        {
            if (!string.IsNullOrEmpty(contentType.CharSet))
            {
                responseEncoding = Encoding.GetEncoding(contentType.CharSet);
            }
            var responseString = responseEncoding.GetString(responseData);
            var ret = new RemoteDocument
            {
                ContextUrl = contextLink == null ? null : new Uri(contextLink),
                DocumentUrl = responseMessage.RequestMessage.RequestUri,
                Document = JToken.Parse(responseString),
            };
            return ret;
        }
        catch (Exception ex)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed,
                "Loading document failed. Could not parse the content of the response as valid JSON.",
                ex);
        }
    }

    private static IEnumerable<WebLink> ParseLinkHeaders(IEnumerable<string> linkHeaderValues)
    {
        if (linkHeaderValues == null) yield break;
        foreach (var linkHeaderValue in linkHeaderValues)
        {
            if (WebLink.TryParse(linkHeaderValue, out WebLink link)) yield return link;
        }
    }
}
