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
        public static JArray Expand(Uri inputUrl, JsonLdProcessorOptions options = null)
        {
            return ExpandAsync(inputUrl, options).Result;
        }

        /// <summary>
        /// Return the result of expanding a parsed JSON document
        /// </summary>
        /// <param name="input">The parsed JSON to be expanded</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JArray Expand(JToken input, JsonLdProcessorOptions options = null)
        {
            return ExpandAsync(input, options).Result;
        }

        /// <summary>
        /// Return the result of expanding the JSON document retrieved from the specified URL
        /// </summary>
        /// <param name="inputUrl"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<JArray> ExpandAsync(Uri inputUrl, JsonLdProcessorOptions options = null)
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
        public static async Task<JArray> ExpandAsync(JToken input, JsonLdProcessorOptions options = null)
        {
            if (input is JValue && (input as JValue).Type == JTokenType.String)
            {
                return await ExpandAsync((input as JValue).Value<string>(), options);
            }
            return await ExpandAsync(new RemoteDocument {Document = input}, null, options);
        }

        /// <summary>
        /// Run the Compaction algorithm asynchronously
        /// </summary>
        /// <param name="input">The JSON-LD data to be compacted. Expected to be a JObject or JArray of JObject or a JString whose value is the IRI reference to a JSON-LD document to be retrieved</param>
        /// <param name="context">The context to use for the compaction process. May be a JObject, JArray of JObject, JString or JArray of JString. String values are treated as IRI references to context documents to be retrieved</param>
        /// <param name="options">Additional processor options</param>
        /// <returns></returns>
        public static async Task<JObject> CompactAsync(JToken input, JToken context, JsonLdProcessorOptions options)
        {
            var processor = new JsonLdProcessor(options);

            // Set expanded input to the result of using the expand method using input and options.
            var expandedInput = await ExpandAsync(input, options);
            // If context is a dictionary having an @context member, set context to that member's value, otherwise to context.
            var contextProperty = (context as JObject)?.Property("@context");
            if (contextProperty != null)
            {
                context = contextProperty.Value;
            }
            // Set compacted output to the result of using the Compaction algorithm, passing context as active context, an empty dictionary as inverse context, null as property, expanded input as element, and if passed, the compactArrays flag in options.
            var compactResult = processor.CompactWrapper(context, new JObject(), null, expandedInput, options.CompactArrays);
            return compactResult;
        }

        /// <summary>
        /// Flattens the given input and compacts it using the passed context according to the steps in the JSON-LD Flattening algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<JToken> FlattenAsync(JToken input, JToken context, JsonLdProcessorOptions options = null)
        {
            var processor = new JsonLdProcessor(options);

            // Set expanded input to the result of using the expand method using input and options.
            var expandedInput = await ExpandAsync(input, options);
            // If context is a dictionary having an @context member, set context to that member's value, otherwise to context.
            var contextObject = context as JObject;
            if (contextObject?.Property("@context") != null)
            {
                context = contextObject["@context"];
            }
            return processor.FlattenWrapper(expandedInput, context, options?.CompactArrays ?? true);
        }

        private static async Task<JArray> ExpandAsync(RemoteDocument doc, Uri documentLocation,
            JsonLdProcessorOptions options = null)
        {
            var activeContext = new JsonLdContext {Base = documentLocation};
            if (options.Base != null) activeContext.Base = options.Base;
            var processor = new JsonLdProcessor(options);
            if (options.ExpandContext != null)
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
                var contextDoc = await LoadJsonAsync(doc.ContextUrl, options);
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

        private static async Task<RemoteDocument> LoadJsonAsync(Uri remoteRef, JsonLdProcessorOptions options)
        {
            if (options.Loader != null) return options.Loader(remoteRef);
            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(Options.UriLoaderTimeout),
            };
            var response = await client.GetAsync(remoteRef).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var mediaType = response.Content.Headers.ContentType.MediaType;
            if (!(mediaType.Equals("application/json") || mediaType.EndsWith("+json")))
            {
                throw new JsonLdProcessorException(
                    JsonLdErrorCode.LoadingDocumentFailed, 
                    $"Loading document failed from {remoteRef} - retrieved content type ({mediaType}) was not application/json, application/ld+json or */*+json.");
            }
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string contextLink = null;

            // If content type is application/ld+json the context link header is ignored
            if (!mediaType.Equals("application/ld+json"))
            {
                var contextLinks = ParseLinkHeaders(response.Headers.GetValues("Link"))
                    .Where(x => x.RelationTypes.Contains("http://www.w3.org/ns/json-ld#context"))
                    .Select(x => x.LinkValue).ToList();
                if (contextLinks.Count > 1)
                {
                    throw new JsonLdProcessorException(
                        JsonLdErrorCode.MultipleContextLinkHeaders, 
                        "Multiple context link headers");
                }
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