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
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VDS.RDF.JsonLd
{

    public partial class JsonLdProcessor
    {
        /// <summary>
        /// Return the result of expanding the JSON-LD document retrieved from the specified URL
        /// </summary>
        /// <param name="inputUrl"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public JArray Expand(Uri inputUrl, JsonLdProcessorOptions options = null)
        {
            return ExpandAsync(inputUrl, options).Result;
        }

        /// <summary>
        /// Return the result of expanding a parsed JSON document
        /// </summary>
        /// <param name="input">The parsed JSON to be expanded</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public JArray Expand(JToken input, JsonLdProcessorOptions options = null)
        {
            return ExpandAsync(input, options).Result;
        }

        /// <summary>
        /// Return the result of expanding the JSON document retrieved from the specified URL
        /// </summary>
        /// <param name="inputUrl"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<JArray> ExpandAsync(Uri inputUrl, JsonLdProcessorOptions options = null)
        {
            var parsedJson = await LoadJsonAsync(inputUrl, options);
            return await ExpandAsync(parsedJson, inputUrl, options);
        }


        /// <summary>
        /// Return the result of expanding a parsed JSON document
        /// </summary>
        /// <param name="input"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <remarks>If <paramref name="input"/> is a string JValue, it will be treated as specifying the IRI of a JSON-LD resource to be retrieved and expanded. Otherwise the input should be either a JArray or a JObject.</remarks>
        public async Task<JArray> ExpandAsync(JToken input, JsonLdProcessorOptions options = null)
        {
            if (input is JValue && (input as JValue).Type == JTokenType.String)
            {
                return await ExpandAsync((input as JValue).Value<string>(), options);
            }
            return await ExpandAsync(new RemoteDocument {Document = input}, null, options);
        }

        private async Task<JArray> ExpandAsync(RemoteDocument doc, Uri documentLocation,
            JsonLdProcessorOptions options = null)
        {
            var activeContext = new JsonLdContext {Base = documentLocation};
            if (options.Base != null) activeContext.Base = options.Base;
            if (options.ExpandContext != null)
            {
                var expandObject = options.ExpandContext as JObject;
                if (expandObject != null)
                {
                    var contextProperty = expandObject.Property("@context");
                    if (contextProperty != null)
                    {
                        activeContext = ProcessContext(activeContext, contextProperty.Value);
                    }
                    else
                    {
                        activeContext = ProcessContext(activeContext, expandObject);
                    }
                }
                else
                {
                    activeContext = ProcessContext(activeContext, options.ExpandContext);
                }
            }
            if (doc.ContextUrl != null)
            {
                var contextDoc = await LoadJsonAsync(doc.ContextUrl, options);
                if (contextDoc.Document is string)
                {
                    contextDoc.Document = JToken.Parse(contextDoc.Document as string);
                }
                activeContext = ProcessContext(activeContext, contextDoc.Document as JToken);
            }
            if (doc.Document is string)
            {
                doc.Document = JToken.Parse(doc.Document as string);
            }
            return Expand(activeContext, null, doc.Document as JToken);
        }

        private async Task<RemoteDocument> LoadJsonAsync(Uri remoteRef, JsonLdProcessorOptions options)
        {
            if (options.Loader != null) return options.Loader(remoteRef);
            var client = new HttpClient();
            var response = await client.GetAsync(remoteRef);
            response.EnsureSuccessStatusCode();
            if (response.Headers.GetValues("Content-Type")
                .Any(x => x.Contains("application/json") || x.Contains("application/ld+json") || x.Contains("+json")))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed, "Loading document failed from {remoteRef} - retrieved content type was not application/json, application/ld+json or */*+json.");
            }
            var responseString = await response.Content.ReadAsStringAsync();
            string contextLink = null;

            // If content type is application/ld+json the context link header is ignored
            if (!response.Headers.GetValues("Content-Type").Any(x => x.Contains("application/ld+json")))
            {
                var contextLinks = ParseLinkHeaders(response.Headers.GetValues("Link"))
                    .Where(x => x.RelationTypes.Contains("http://www.w3.org/ns/json-ld#context"))
                    .Select(x => x.LinkValue).ToList();
                if (contextLinks.Count > 1) throw new JsonLdProcessorException(JsonLdErrorCode.MultipleContextLinkHeaders, "Multiple context link headers");
                contextLink = contextLinks.FirstOrDefault();
            }

            var ret = new RemoteDocument
            {
                ContextUrl = contextLink == null ? null : new Uri(contextLink),
                DocumentUrl = response.RequestMessage.RequestUri,
                Document = JToken.Parse(responseString),
            };
            return ret;
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
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed,  $"Loader returned an unrecognised type of Document ({remoteDoc.Document.GetType().FullName}). Expected either JToken or string.");
                }
                catch (Exception ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed, $"Could not load context from {reference}. Cause: {ex}", ex);
                }
            }
            else
            {
                // Invoke the default loader
                var loaderTask = LoadJsonAsync(reference, this._options);
                var remoteDoc = loaderTask.Result;
                if (remoteDoc == null)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed,
                        $"Could not load JSON from {reference}.");
                }
                return remoteDoc.Document as JToken;
            }
        }
    }
}