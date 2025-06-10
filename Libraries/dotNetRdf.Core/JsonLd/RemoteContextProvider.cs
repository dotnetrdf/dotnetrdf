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
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Default remote context provider implementation.
/// </summary>
public class RemoteContextProvider: IRemoteContextProvider
{
    private readonly JsonLdProcessorOptions _options;
    private readonly Dictionary<Uri, JsonLdRemoteContext> _remoteContextCache;

    /// <summary>
    /// Create a new provider instance configured using the specified options.
    /// </summary>
    /// <param name="options"></param>
    public RemoteContextProvider(JsonLdProcessorOptions options)
    {
        _options = options;
        _remoteContextCache = new Dictionary<Uri, JsonLdRemoteContext>();
    }

    /// <inheritdoc />
    public JsonLdRemoteContext GetRemoteContext(Uri reference)
    {
        if (_remoteContextCache.TryGetValue(reference, out JsonLdRemoteContext cachedContext)) return cachedContext;
        try
        {
            RemoteDocument remoteDoc = LoadJson(reference,
                new JsonLdLoaderOptions
                    { Profile = JsonLdVocabulary.Context, RequestProfile = JsonLdVocabulary.Context }, _options);
            JToken jsonRepresentation = GetJsonRepresentation(remoteDoc);
            if (!(jsonRepresentation is JObject remoteJsonObject))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                    $"Remote document at {reference} could not be parsed as a JSON object.");
            }

            if (!remoteJsonObject.ContainsKey("@context"))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                    $"Remote document at {reference} does not have an @context property");
            }

            var remoteContext = new JsonLdRemoteContext(remoteDoc.DocumentUrl, remoteJsonObject["@context"]);
            _remoteContextCache[reference] = remoteContext;
            return remoteContext;

        }
        catch (JsonLdProcessorException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed,
                $"Failed to load remote context from {reference}.", ex);
        }
    }

    private static RemoteDocument LoadJson(Uri remoteRef, JsonLdLoaderOptions loaderOptions,
        JsonLdProcessorOptions options)
    {
        return options.DocumentLoader != null
            ? options.DocumentLoader(remoteRef, loaderOptions)
            : DefaultDocumentLoader.LoadJson(remoteRef, loaderOptions);
    }

    private static JToken GetJsonRepresentation(RemoteDocument remoteDoc)
    {
        switch (remoteDoc.Document)
        {
            case JToken representation:
                return representation;
            case string docStr:
            {
                try
                {

                    return JToken.Parse(docStr);
                }
                catch (Exception ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                        "Could not parse remote content as a JSON document. ", ex);
                }
            }
            default:
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                    "Could not parse remote content as a JSON document.");
        }
    }
}