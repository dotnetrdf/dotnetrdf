/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace VDS.RDF.JsonLd
{

    public partial class JsonLdProcessor
    {
        /// <summary>
        /// Apply the JSON-LD context expansion algorithm to the context found at the specified URL
        /// </summary>
        /// <param name="contextUrl">The URL to load the source context from</param>
        /// <param name="options">Options to apply during the expansion processing</param>
        /// <returns>The expanded JSON-LD contex</returns>
        public static JArray Expand(Uri contextUrl, JsonLdProcessorOptions options = null)
        {
            var parsedJson = LoadJson(contextUrl, options);
            return Expand(parsedJson, contextUrl, options);
        }

        /// <summary>
        /// Apply the JSON-LD expansion algorithm to a context JSON object
        /// </summary>
        /// <param name="input">The context JSON object to be expanded</param>
        /// <param name="options">Options to apply during the expansion processing</param>
        /// <returns>The expanded JSON-LD contex</returns>
        public static JArray Expand(JToken input, JsonLdProcessorOptions options = null)
        {
            if (input is JValue && (input as JValue).Type == JTokenType.String)
            {
                return Expand((input as JValue).Value<string>(), options);
            }
            return Expand(new RemoteDocument { Document = input }, null, options);
        }

        private static JArray Expand(RemoteDocument doc, Uri documentLocation,
            JsonLdProcessorOptions options = null)
        {
            var activeContext = new JsonLdContext { Base = documentLocation };
            if (options?.Base != null) activeContext.Base = options.Base;
            var processor = new JsonLdProcessor(options);
            if (options?.ExpandContext != null)
            {
                var expandObject = options.ExpandContext as JObject;
                if (expandObject != null)
                {
                    var contextProperty = expandObject.Property("@context");
                    if (contextProperty != null)
                    {
                        activeContext = processor.ProcessContext(activeContext, contextProperty.Value);
                    }
                    else
                    {
                        activeContext = processor.ProcessContext(activeContext, expandObject);
                    }
                }
                else
                {
                    activeContext = processor.ProcessContext(activeContext, options.ExpandContext);
                }
            }
            if (doc.ContextUrl != null)
            {
                var contextDoc = LoadJson(doc.ContextUrl, options);
                if (contextDoc.Document is string)
                {
                    contextDoc.Document = JToken.Parse(contextDoc.Document as string);
                }
                activeContext = processor.ProcessContext(activeContext, contextDoc.Document as JToken);
            }
            if (doc.Document is string)
            {
                doc.Document = JToken.Parse(doc.Document as string);
            }
            return processor.Expand(activeContext, null, doc.Document as JToken);
        }

        private static RemoteDocument LoadJson(Uri remoteRef, JsonLdProcessorOptions options)
        {
            if (options.Loader != null) return options.Loader(remoteRef);
            
            var client = new RedirectingWebClient();
            var responseString = client.DownloadString(remoteRef);
            var contentType = client.ResponseHeaders.GetValues("Content-Type");
            if (contentType == null ||
                !contentType.Any(x => x.Contains("application/json") ||
                                      x.Contains("application/ld+json") ||
                                      x.Contains("+json")))
            {
                throw new JsonLdProcessorException(
                    JsonLdErrorCode.LoadingDocumentFailed,
                    $"Loading document failed from {remoteRef} - retrieved content type ({contentType}) was not application/json, application/ld+json or */*+json.");
            }

            string contextLink = null;

            // If content type is application/ld+json the context link header is ignored
            if (!contentType.Any(x => x.Contains("application/ld+json")))
            {
                var contextLinks = ParseLinkHeaders(client.ResponseHeaders.GetValues("Link"))
                    .Where(x => x.RelationTypes.Contains("http://www.w3.org/ns/json-ld#context"))
                    .Select(x => x.LinkValue).ToList();
                if (contextLinks.Count > 1)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.MultipleContextLinkHeaders,  "Multiple context link headers");
                }
                contextLink = contextLinks.FirstOrDefault();
            }

            var ret = new RemoteDocument
            {
                ContextUrl = contextLink == null ? null : new Uri(contextLink),
                DocumentUrl = client.ResponseUri,
                Document = JToken.Parse(responseString),
            };
            return ret;
        }

        private class RedirectingWebClient : WebClient
        {
            public Uri  ResponseUri { get; private set; }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var webResponse = base.GetWebResponse(request);
                ResponseUri = webResponse?.ResponseUri;
                return webResponse;
            }
        }

        private JToken LoadReference(Uri reference)
        {
            if (_options.Loader != null)
            {
                try
                {
                    var remoteDoc = _options.Loader(reference);
                    if (remoteDoc.Document is JToken) return remoteDoc.Document as JToken;
                    if (remoteDoc.Document is string) return JToken.Parse(remoteDoc.Document as string);
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed, $"Loader returned an unrecognised type of Document ({remoteDoc.Document.GetType().FullName}). Expected either JToken or string.");
                }
                catch (Exception ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed, $"Could not load context from {reference}. Cause: {ex}", ex);
                }
            }
            else
            {
                // Invoke the default loader
                var remoteDoc = LoadJson(reference, _options);
                if (remoteDoc == null) throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed, $"Could not load JSON fomr {reference}.");
                return remoteDoc.Document as JToken;
            }
        }
    }
}