/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Implements the default remote JSON document loader logic for the JSON-LD processor.
    /// </summary>
    public static class DefaultDocumentLoader
    {
        public static RemoteDocument LoadJson(Uri remoteRef, JsonLdLoaderOptions loaderOptions)
        {
            var client = new RedirectingWebClient();
            client.Headers.Set(HttpRequestHeader.Accept,
                "application/ld+json;q=1.0, application/json;q=0.9, */*+json;q=0.8");

            // var responseString = client.DownloadString(remoteRef);
            var responseData = client.DownloadData(remoteRef);
            var contentType = client.ResponseHeaders.GetValues("Content-Type");
            bool matchesContentType =
                contentType != null &&
                contentType.Any(x => x.Contains("application/json") ||
                                     (x.Contains("application/ld+json") &&
                                      string.IsNullOrEmpty(loaderOptions.RequestProfile)) ||
                                     (x.Contains("application/ld+json") &&
                                      !string.IsNullOrEmpty(loaderOptions.RequestProfile) &&
                                      x.Contains(loaderOptions.RequestProfile)) ||
                                     x.Contains("+json"));
            var documentUrl = client.ResponseUri;
            string contextLink = null;
            if (!matchesContentType)
            {
                var contextLinks = ParseLinkHeaders(client.ResponseHeaders.GetValues("Link"));
                var alternateLink = contextLinks.FirstOrDefault(link =>
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
            if (!contentType.Any(x => x.Contains("application/ld+json")))
            {
                var contextLinks = ParseLinkHeaders(client.ResponseHeaders.GetValues("Link"))
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
                var ct = new ContentType(client.ResponseHeaders[HttpResponseHeader.ContentType]);
                if (!string.IsNullOrEmpty(ct.CharSet))
                {
                    responseEncoding = Encoding.GetEncoding(ct.CharSet);
                }
                var responseString = responseEncoding.GetString(responseData);
                var ret = new RemoteDocument
                {
                    ContextUrl = contextLink == null ? null : new Uri(contextLink),
                    DocumentUrl = client.ResponseUri,
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
            foreach (var linkHeaderValue in linkHeaderValues)
            {
                if (WebLink.TryParse(linkHeaderValue, out var link)) yield return link;
            }
        }

        
        private class RedirectingWebClient : WebClient
        {
            public Uri ResponseUri { get; private set; }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var webResponse = base.GetWebResponse(request);
                ResponseUri = webResponse?.ResponseUri;
                return webResponse;
            }
        }
    }
}
