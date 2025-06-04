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
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;
using VDS.RDF.Parsing;

namespace VDS.RDF.JsonLd.Processors;

/// <summary>
/// Implements the JSON-LD context processing, term definition creation and IRI expansion algorithms.
/// </summary>
internal class ContextProcessor : ProcessorBase
{
    private static readonly char[] GenDelimChars =
    {
        ':',
        '/',
        '?',
        '#',
        '[',
        ']',
        '@',
    };

    private readonly IRemoteContextProvider _contextProvider;

    /// <summary>
    /// Create a new context processor instance.
    /// </summary>
    /// <param name="options">The processing options to use.</param>
    /// <param name="warnings">The list to add any warning messages to.</param>
    /// <param name="contextProvider">The provider for retrieving remote context documents.</param>
    public ContextProcessor(JsonLdProcessorOptions options, 
        IList<JsonLdProcessorWarning> warnings, 
        IRemoteContextProvider contextProvider = null) : base(options, warnings)
    {
        _contextProvider = contextProvider ?? new RemoteContextProvider(options);
    }

    /// <summary>
    /// Process a context in the scope of a current active context resulting in a new context.
    /// </summary>
    /// <param name="activeContext">The currently active context.</param>
    /// <param name="localContext">The context to be processed. May be a JSON object, string or array.</param>
    /// <param name="baseUrl">A base URL to use when resolving relative context URLs.</param>
    /// <param name="remoteContexts">The remote context's already processed. Used to detect circular references in the current context processing step.</param>
    /// <param name="overrideProtected">Boolean flag indicating whether or not to allow changes to propagated terms.</param>
    /// <param name="propagate">Boolean flag used to mark term definitions associated with non-propagated contexts.</param>
    /// <param name="validateScopedContext">Boolean flag used to limit recursion when validating possibly recursive scoped contexts.</param>
    /// <returns></returns>
    public JsonLdContext ProcessContext(JsonLdContext activeContext, JToken localContext, Uri baseUrl,
        List<Uri> remoteContexts = null, bool overrideProtected = false, bool propagate = true, bool validateScopedContext = true)
    {
        if (remoteContexts == null) remoteContexts = new List<Uri>();

        // 1. Initialize result to the result of cloning active context
        JsonLdContext result = activeContext.Clone();

        // 2. If local context is an object containing the member @propagate, its value MUST be boolean true or false, set propagate to that value. 
        if (localContext is JObject localContextObject)
        {
            if (localContextObject.ContainsKey("@propagate"))
            {
                if (localContextObject["@propagate"].Type == JTokenType.Boolean)
                {
                    propagate = localContextObject["@propagate"].Value<bool>();
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidPropagateValue, "The value of @propagate must be a boolean value.");
                }
            }
        }

        // 3. If propagate is false, and result does not have a previous context, set previous context in result to active context.
        if (!propagate && result.PreviousContext == null)
        {
            result.PreviousContext = activeContext;
        }

        // 4. If local context is not an array, set it to an array containing only local context.
        localContext = JsonLdUtils.EnsureArray(localContext);

        // 5. For each item context in local context:
        foreach (JToken context in (JArray)localContext)
        {
            // 5.1 if context is null:
            if (context.Type == JTokenType.Null)
            {
                // 5.1.1 If override protected is false and active context contains any protected term definitions,
                // an invalid context nullification has been detected and processing is aborted.
                if (!overrideProtected && result.HasProtectedTerms())
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContextNullification,
                        "Attempt to nullify a context containing one or more protected term definitions.");
                }
                // 5.1.2 Initialize result as a newly - initialized active context, setting both base IRI and original base
                // URL to the value of original base URL in active context, and, if propagate is false, previous context in
                // result to the previous value of result.
                var tmp = new JsonLdContext(activeContext.OriginalBase);
                if (!propagate)
                {
                    tmp.PreviousContext = result;
                }
                result = tmp;

                // 5.1.3 Continue with the next context.
                continue;
            }

            // 5.2 If context is a string
            if (context.Type == JTokenType.String)
            {
                // 5.2.1 Initialize context to the result of resolving context against base URL.
                // If base URL is not a valid IRI, then context MUST be a valid IRI, otherwise a loading document failed error
                // has been detected and processing is aborted. 
                var contextStr = (context as JValue).Value<string>();
                Uri remoteUrl = baseUrl == null ? new Uri(contextStr) : new Uri(baseUrl, contextStr);
                if (!remoteUrl.IsAbsoluteUri)
                {
                    if (baseUrl == null)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed, "Could not resolve relative context URL");
                    }
                }

                // 5.2.2 If validate scoped context is false, and remote contexts already includes context do not process context further
                // and continue to any next context in local context.
                if (!validateScopedContext && remoteContexts.Contains(remoteUrl))
                {
                    continue;
                }
                // 5.2.3 If the number of entries in the remote contexts array exceeds a processor defined limit, a context overflow error has been
                // detected and processing is aborted; otherwise, add context to remote contexts.
                if (Options.RemoteContextLimit >= 0 && remoteContexts.Count >= Options.RemoteContextLimit)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.ContextOverflow, "Number of loaded remote context references exceeds the limit.");
                }
                remoteContexts.Add(remoteUrl);

                // 5.2.4 If context was previously dereferenced, then the processor MUST NOT do a further dereference,
                // and context is set to the previously established internal representation:
                // set context document to the previously dereferenced document, and set loaded context to the value of the @context entry from the document in context document.
                JsonLdRemoteContext loadedContext = _contextProvider.GetRemoteContext(remoteUrl); // 5.2.4, 5.2.5

                // 5.2.6 Set result to the result of recursively calling this algorithm, passing result for active context, loaded context for local context, the documentUrl of context document for base URL, a copy of remote contexts, and validate scoped context. 
                result = ProcessContext(result, loadedContext.Context, loadedContext.DocumentUrl,
                    remoteContexts, validateScopedContext: validateScopedContext);

                // 5.2.7 Continue with the next context.
                continue;
            }

            // 5.3 - If context is not a map, an invalid local context error has been detected and processing is aborted.
            if (context.Type != JTokenType.Object)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLocalContext, "Local context must be a string, array of strings or JSON object");
            }

            // 5.4 - Otherwise, context is a context definition.
            var contextDefinition = context as JObject;

            // 5.5 - If context has an @version entry:
            JProperty versionProperty = contextDefinition.Property("@version");
            if (versionProperty != null)
            {
                // 5.5.1 - If the associated value is not 1.1, an invalid @version value has been detected, and processing is aborted.
                var versionValue = versionProperty.Value.Value<string>();
                if (!"1.1".Equals(versionValue))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVersionValue, $"Found invalid value for @version property: {versionValue}.");
                }
                // 5.5.2 - If processing mode is set to json-ld-1.0, a processing mode conflict error has been detected and processing is aborted.
                if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.ProcessingModeConflict,
                        "Processing mode conflict. Processor options specify JSON-LD 1.0 processing mode, but encountered @version that requires JSON-LD 1.1 processing features");
                }
            }

            // 5.6 - If context has an @import entry: 
            JProperty importProperty = contextDefinition.Property("@import");
            if (importProperty != null)
            {
                // 5.6.1 - If processing mode is json-ld-1.0, an invalid context entry error has been detected and processing is aborted.
                CheckProcessingMode("@import", JsonLdErrorCode.InvalidContextEntry);

                // 5.6.2 - Otherwise, if the value of @import is not a string, an invalid @import value error has been detected and processing is aborted.
                if (importProperty.Value.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidImportValue,
                        "The value of an @import property must be a string");
                }

                // 5.6.3 - Initialize import to the result of resolving the value of @import against base URL.
                var import = new Uri(baseUrl, importProperty.Value.Value<string>());

                // Implements 5.6.4, 5.6.5, 5.6.6
                JsonLdRemoteContext remoteContext = _contextProvider.GetRemoteContext(import);
                if (!(remoteContext.Context is JObject importContext))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                        "The value of the @context of the remote document referenced by @import must be a map.");
                }
                else if (importContext.ContainsKey("@import"))
                {
                    // 5.6.7 - If import context has a @import entry, an invalid context entry error has been detected and processing is aborted.
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContextEntry,
                        $"The remote context from {import} contains an @import property.");
                }
                // 5.6.8 - Set context to the result of merging context into import context, replacing common entries with those from context.
                var tmp = new JObject(importContext);
                tmp.Merge(contextDefinition);
                contextDefinition = tmp;
            }

            // 5.7 - If context has an @base key and remote contexts is empty, i.e., the currently being processed context is not a remote context
            JProperty baseProperty = contextDefinition.Property("@base");
            if (baseProperty != null && remoteContexts.Count == 0)
            {
                // 5.7.1 - Initialize value to the value associated with the @base entry.
                JToken value = baseProperty.Value;
                // 5.7.2 - If value is null, remove the base IRI of result.
                if (value.Type == JTokenType.Null)
                {
                    result.RemoveBase();
                }
                // 5.7.3 - Otherwise, if value is an absolute IRI, the base IRI of result is set to value.
                else if (JsonLdUtils.IsAbsoluteIri(value))
                {
                    result.Base = new Uri(value.Value<string>());
                }
                // 5.7.4 - Otherwise, if value is a relative IRI and the base IRI of result is not null, set the base IRI of result to the result of resolving
                // value against the current base IRI of result.
                else if (JsonLdUtils.IsRelativeIri(value))
                {
                    if (result.Base != null)
                    {
                        result.Base = new Uri(result.Base, value.Value<string>());
                    }
                    else
                    {
                        // Otherwise, an invalid base IRI error has been detected and processing is aborted.
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseIri, "Unable to resolve relative @base IRI as there is no current base IRI.");
                    }
                }
                // 5.7.5 - Otherwise, an invalid base IRI error has been detected and processing is aborted.
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseIri, "The @base property must be an absolute IRI, a relative IRI or null");
                }
            }


            // 5.8 - If context has an @vocab key:
            JProperty contextProperty = contextDefinition.Property("@vocab");
            if (contextProperty != null)
            {
                // 5.8.1 - Initialize value to the value associated with the @vocab key.
                JToken value = contextProperty.Value;
                // 5.8.2 - If value is null, remove any vocabulary mapping from result.
                if (value.Type == JTokenType.Null)
                {
                    result.Vocab = null;
                }
                // 5.8.3 -Otherwise, if value is an IRI or blank node identifier, the vocabulary mapping of result is set to
                // the result of IRI expanding value using true for document relative . If it is not an IRI, or a blank node
                // identifier, an invalid vocab mapping error has been detected and processing is aborted. 
                else if (value.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVocabMapping,
                        "The value of @vocab must be a string.");
                }
                else
                {
                    var str = value.Value<string>();
                    if (JsonLdUtils.IsIri(str) || string.Empty.Equals(str) || JsonLdUtils.IsBlankNodeIdentifier(str))
                    {
                        // Expanding using result ensures that we expand relative to @base if it is specified
                        result.Vocab = ExpandIri(result, str, true, true);
                    }
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVocabMapping, "The value of @vocab must be an IRI or blank node identifier.");
                    }
                }
            }

            // 5.9 - If context has an @language key
            JProperty languageProperty = contextDefinition.Property("@language");
            if (languageProperty != null)
            {
                // 5.9.1 - Initialize value to the value associated with the @language key.
                JToken value = languageProperty.Value;
                switch (value.Type)
                {
                    case JTokenType.Null:
                        // 5.9.2 - If value is null, remove any default language from result.
                        result.Language = null;
                        break;
                    case JTokenType.String:
                        // 5.9.3 - Otherwise, if value is string, the default language of result is set to value.
                        result.Language = value.Value<string>().ToLowerInvariant(); // Processors MAY normalize language tags to lower case.
                        if (!LanguageTag.IsWellFormed(result.Language))
                        {
                            Warn(JsonLdErrorCode.MalformedLanguageTag,
                                $"The value of the @language property ({result.Language}) is not a well-formed BCP-47 language tag.");
                        }
                        break;
                    default:
                        // 5.9.3 (cont) - If it is not a string, an invalid default language error has been detected and processing is aborted.
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidDefaultLanguage,
                            "@language property value must be a JSON string or null.");
                }
            }

            // 5.10 - If context has an @direction entry
            JProperty directionProperty = contextDefinition.Property("@direction");
            if (directionProperty != null)
            {
                // 5.10.1 - If processing mode is json - ld - 1.0, an invalid context entry error has been detected and processing is aborted.
                CheckProcessingMode("@direction");
                // 5.10.2 - Initialize value to the value associated with the @direction entry.
                // 5.10.3 - If value is null, remove any base direction from result.
                // 5.10.4 - Otherwise, if value is a string, the base direction of result is set to value.
                // If it is not null, "ltr", or "rtl", an invalid base direction error has been detected and processing is aborted.
                result.BaseDirection = JsonLdUtils.ParseLanguageDirection(directionProperty.Value);
            }

            // 5.11 - context has an @propagate entry:
            JProperty propagateProperty = contextDefinition.Property("@propagate");
            if (propagateProperty != null)
            {
                // 5.11.1 - If processing mode is json - ld - 1.0, an invalid context entry error has been detected and processing is aborted.
                CheckProcessingMode("@propagate", JsonLdErrorCode.InvalidContextEntry);

                // 5.11.2 - Otherwise, if the value of @propagate is not boolean true or false, an invalid @propagate value error has been detected and processing is aborted.
                if (propagateProperty.Value.Type != JTokenType.Boolean)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidPropagateValue,
                        "The value of @propagate must be a boolean");
                }
            }

            // 5.12 - Create a JSON object defined to use to keep track of whether or not a term has already been defined or currently being defined during recursion.
            var defined = new Dictionary<string, bool>();

            // For each key-value pair in context where key is not @base, @direction, @import, @language, @propagate, @protected, @version, or @vocab, invoke the
            // Create Term Definition algorithm, passing result for active context, context for local context, key, defined, base URL, the value of the @protected
            // entry from context, if any, for protected, override protected, and a copy of remote contexts. 
            var @protected = false;
            JProperty protectedProperty = contextDefinition.Property("@protected");
            if (protectedProperty != null)
            {
                if (protectedProperty.Value.Type != JTokenType.Boolean)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidProtectedValue,
                        "The value of @protected must be a boolean");
                }

                @protected = protectedProperty.Value.Value<bool>();
            }
            foreach (JProperty property in contextDefinition.Properties())
            {
                var key = property.Name;
                if (!JsonLdKeywords.JsonLdContextKeywords.Contains(key))
                {
                    CreateTermDefinition(result, contextDefinition, key, defined, baseUrl, @protected, overrideProtected, new List<Uri>(remoteContexts));
                }
            }

        }
        return result;
    }

    private void CreateTermDefinition(JsonLdContext activeContext, JObject localContext, string term,
Dictionary<string, bool> defined = null,
Uri baseUrl = null, bool @protected = false, bool overrideProtected = false,
List<Uri> remoteContexts = null, bool validateScopedContexts = true)
    {
        if (defined == null) defined = new Dictionary<string, bool>();
        if (remoteContexts == null) remoteContexts = new List<Uri>();

        // 1 - If defined contains the entry term and the associated value is true (indicating that the term definition has already been created), return.
        // Otherwise, if the value is false, a cyclic IRI mapping error has been detected and processing is aborted.
        if (defined.TryGetValue(term, out var created))
        {
            if (created)
            {
                return;
            }

            throw new JsonLdProcessorException(JsonLdErrorCode.CyclicIriMapping,
                $"Cyclic IRI mapping detected while processing term {term}");
        }

        // 2 - If term is the empty string (""), an invalid term definition error has been detected and processing is aborted.
        // Otherwise, set the value associated with defined's term entry to false. This indicates that the term definition is now being created but is not yet complete.
        if (string.IsNullOrEmpty(term))
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                "Attempt to create a term with an empty string for a name.");
        }

        defined[term] = false;

        // 3 - Initialize value to a copy of the value associated with the entry term in local context.
        JToken v = localContext[term]?.DeepClone();

        // 4 - If term is @type, and processing mode is json-ld-1.0, a keyword redefinition error has been detected and processing is aborted.
        if (term.Equals("@type"))
        {
            if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.KeywordRedefinition,
                    "Cannot redefine the @type keyword under JSON-LD 1.0 processing rules.");
            }

            // At this point, value MUST be a map with only either or both of the following entries:
            //   An entry for @container with value @set.
            //   An entry for @protected.
            // Any other value means that a keyword redefinition error has been detected and processing is aborted.
            ValidateTypeRedefinition(v);
        }
        else if (JsonLdUtils.IsKeyword(term))
        {
            // 5 Otherwise, since keywords cannot be overridden, term MUST NOT be a keyword and a keyword redefinition error has been detected and processing is aborted.
            throw new JsonLdProcessorException(JsonLdErrorCode.KeywordRedefinition,
                $"Cannot redefine JSON-LD keyword {term}.");
        }
        else if (JsonLdUtils.MatchesKeywordProduction(term))
        {
            // 5 (cont.) If term has the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), return; processors SHOULD generate a warning.
            Warn(JsonLdErrorCode.InvalidTermDefinition,
                $"The term {term} has been ignored as it matches the pattern @[a-zA-Z] which is reserved for JSON-LD keywords.");
            return;
        }

        // 6 - Initialize previous definition to any existing term definition for term in active context, removing that term definition from active context.
        JsonLdTermDefinition previousDefinition = activeContext.RemoveTerm(term);
        var simpleTerm = false;

        // 7 - If value is null, convert it to a map consisting of a single entry whose key is @id and whose value is null.
        if (v == null || v.Type == JTokenType.Null)
        {
            v = new JObject(new JProperty("@id", JValue.CreateNull()));
        }

        // 8 - Otherwise, if value is a string, convert it to a map consisting of a single entry whose key is @id and whose value is value. Set simple term to true.
        if (v.Type == JTokenType.String)
        {
            v = new JObject(new JProperty("@id", v));
            simpleTerm = true;
        }

        // 9 - Otherwise, value MUST be a map, if not, an invalid term definition error has been detected and processing is aborted. Set simple term to false.
        if (v.Type != JTokenType.Object)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                "A term definition must be a string, a map or null.");
        }

        var value = v as JObject;

        // 10 - Create a new term definition, definition, initializing prefix flag to false, protected to protected, and reverse property to false.
        var definition = new JsonLdTermDefinition { Prefix = false, Protected = @protected, Reverse = false };

        // 11 - if value has an @protected entry, set the protected flag in definition to the value of this entry. 
        definition.Protected = GetProtectedProperty(value, @protected);

        // 12 - If value contains the key @type:
        JToken typeValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@type");
        if (typeValue != null)
        {
            // 12.1 Initialize type to the value associated with the @type key, which must be a string. Otherwise, an invalid type mapping error has been detected and processing is aborted.
            if (typeValue.Type != JTokenType.String)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping,
                    $"Invalid type mapping for term {term}. The @type value must be a string, got {typeValue.Type}");
            }

            // 12.2 - Set type to the result of IRI expanding type, using local context, and defined.
            var type = ExpandIri(activeContext, typeValue.Value<string>(), true, false, localContext, defined);
            if ((type == "@json" || type == "@none") && Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping,
                    $"Invalid type mapping for term {term}. Unexpanded value was {typeValue.Value<string>()}, and the expanded type IRI is '{type}', but the JSON-LD Processing mode is set to 1.0 which does not support @none or @json types.");
            }
            if (type == "@id" || type == "@vocab" || type == "@json" || type == "@none" || JsonLdUtils.IsAbsoluteIri(type))
            {
                // 10.3 - Set the type mapping for definition to type.
                definition.TypeMapping = type;
            }
            else
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping,
                    $"Invalid type mapping for term {term}. Expected @type value to expand to @id, @vocab, @json, @none or an absolute IRI. Unexpanded value was {typeValue.Value<string>()}, expanded value was {type}.");
            }
        }

        JToken reverseValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@reverse");
        JToken containerValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@container");
        // 13 - If value contains the key @reverse:
        if (reverseValue != null)
        {
            // 13.1 - If value contains @id or @nest, members, an invalid reverse property error has been detected and processing is aborted.
            if (JsonLdUtils.GetPropertyValue(activeContext, value, "@id") != null ||
                JsonLdUtils.GetPropertyValue(activeContext, value, "@nest") != null)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty,
                    $"Invalid reverse property. The @reverse property cannot be combined with @id or @nest property on term {term}.");
            }

            // 13.2 - If the value associated with the @reverse key is not a string, an invalid IRI mapping error has been detected and processing is aborted.
            if (reverseValue.Type != JTokenType.String)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                    $"@reverse property value must be a string on term {term}");
            }

            // 13.3 - If the value associated with the @reverse entry is a string having the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), return; processors SHOULD generate a warning.
            if (JsonLdUtils.MatchesKeywordProduction(reverseValue.Value<string>()))
            {
                Warn(JsonLdErrorCode.InvalidIriMapping,
                    "$@reverse property value on {term} matches the JSON-LD keyword production @[a-zA-Z]+.");
                return;
            }

            // 13.4 - Otherwise, set the IRI mapping of definition to the result of IRI expanding the value associated with the @reverse entry, using local context,
            // and defined. If the result does not have the form of an IRI or a blank node identifier, an invalid IRI mapping error has been detected and processing
            // is aborted.
            var iriMapping = ExpandIri(activeContext, reverseValue.Value<string>(), true, false, localContext, defined);
            if (iriMapping == null || !(JsonLdUtils.IsAbsoluteIri(iriMapping) || JsonLdUtils.IsBlankNodeIdentifier(iriMapping)))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                    $"@reverse property value must expand to an absolute IRI or blank node identifier. The @reverse property on term {term} expands to {iriMapping}.");
            }
            definition.IriMapping = iriMapping;

            // 13.5 - If value contains an @container entry, set the container mapping of definition to an array containing its value; if its value is neither @set, nor @index, nor null, an invalid reverse property error has been detected (reverse properties only support set- and index-containers) and processing is aborted.
            if (containerValue != null)
            {
                if (containerValue.Type == JTokenType.Null)
                {
                    definition.ContainerMapping.Clear();
                    definition.ContainerMapping.Add(JsonLdContainer.Null);
                }
                else if (containerValue.Type == JTokenType.String)
                {
                    var containerMapping = containerValue.Value<string>();
                    if (containerMapping == "@set")
                    {
                        definition.ContainerMapping.Clear();
                        definition.ContainerMapping.Add(JsonLdContainer.Set);
                    }
                    else if (containerMapping == "@index")
                    {
                        definition.ContainerMapping.Clear();
                        definition.ContainerMapping.Add(JsonLdContainer.Index);
                    }
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty,
                            $"Invalid reverse property for term {term}. Reverse properties only support set and index container types. ");
                    }
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                        $"Invalid @container property for term {term}. Property value must be a string or null.");
                }
            }
            // 13.6 - Set the reverse property flag of definition to true.
            definition.Reverse = true;

            // 13.7 - Set the term definition of term in active context to definition and the value associated with defined's key term to true.
            activeContext.SetTerm(term, definition);
            defined[term] = true;
        }
        // 14 - Otherwise, if value contains the key @id and its value does not equal term:
        else if (JsonLdUtils.GetPropertyValue(activeContext, value, "@id") is { } idValue && !term.Equals(idValue.Value<string>()))
        {
            // 14.1 - If the @id entry of value is null, the term is not used for IRI expansion, but is retained to be able to detect future redefinitions of this term.
            // 14.2 - Otherwise:
            if (idValue.Type != JTokenType.Null)
            {
                // 14.2.1 - If the value associated with the @id entry is not a string, an invalid IRI mapping error has been detected and processing is aborted.
                if (idValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                        $"Invalid IRI Mapping. The value of the @id property of term {term} must be a string.");
                }
                // 14.2.2 - If the value associated with the @id entry is not a keyword, but has the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), return;
                // processors SHOULD generate a warning.
                var idValueString = idValue.Value<string>();
                if (!JsonLdUtils.IsKeyword(idValueString) && JsonLdUtils.MatchesKeywordProduction(idValueString))
                {
                    Warn(JsonLdErrorCode.InvalidIdValue,
                        $"The value of the @id property of {term} matches the pattern @[a-zA-Z] which is reserved by the JSON-LD specification. This property will be ignored.");
                    return;
                }
                // 14.2.3 - Otherwise, set the IRI mapping of definition to the result of IRI expanding the value associated with the @id entry, using local context, and defined.
                var iriMapping = ExpandIri(activeContext, idValue.Value<string>(), vocab: true, documentRelative: false, localContext: localContext, defined: defined);

                // If the resulting IRI mapping is neither a keyword, nor an IRI, nor a blank node identifier, an invalid IRI mapping error has been detected and processing is aborted;
                if (!JsonLdUtils.IsKeyword(iriMapping) && !JsonLdUtils.IsAbsoluteIri(iriMapping) && !JsonLdUtils.IsBlankNodeIdentifier(iriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                        $"Invalid IRI Mapping. The value of the @id property of term '{term}' must be a keyword, an absolute IRI or a blank node identifier. Got value {iriMapping}.");
                }

                // if it equals @context, an invalid keyword alias error has been detected and processing is aborted.
                if ("@context".Equals(iriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidKeywordAlias,
                        $"Invalid keyword alias at term {term}.");
                }

                definition.IriMapping = iriMapping;

                // 14.2.4 - If the term contains a colon (:) anywhere but as the first or last character of term, or if it contains a slash (/) anywhere:
                var ix = term.IndexOf(':', 1);
                if ((ix > -1 && ix < term.Length - 1) || term.Contains('/'))
                {
                    // 14.2.4.1 - Set the value associated with defined's term entry to true.
                    defined[term] = true;
                    // 14.2.4.2 - If the result of IRI expanding term using local context, and defined, is not the same as the IRI mapping of definition,
                    // an invalid IRI mapping error has been detected and processing is aborted.
                    var termExpansion = ExpandIri(activeContext, term, localContext: localContext,
                        defined: defined, vocab: true);
                    if (!termExpansion.Equals(iriMapping))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                            $"The IRI generated by expanding the @id property of '{term}' ({iriMapping}) does not match that generated by expanding '{term} itself ({termExpansion}).");
                    }
                }

                // 14.2.5 - If term contains neither a colon (:) nor a slash (/), simple term is true, and if the IRI mapping of definition is either an IRI ending with a gen-delim character,
                // or a blank node identifier, set the prefix flag in definition to true.
                if (!term.Contains(':') && !term.Contains('/') && simpleTerm &&
                    (GenDelimChars.Contains(definition.IriMapping.Last()) ||
                     JsonLdUtils.IsBlankNodeIdentifier(definition.IriMapping)))
                {
                    definition.Prefix = true;
                }
            }
        }
        // 15 - Otherwise if the term contains a colon (:) anywhere after the first character
        else if (term.IndexOf(':', 1) > 0)
        {
            var ix = term.IndexOf(':');
            var prefix = term.Substring(0, ix);
            var rest = term.Substring(ix + 1);
            // 15.1 - If term is a compact IRI with a prefix that is an entry in local context a dependency has been found.
            // Use this algorithm recursively passing active context, local context, the prefix as term, and defined.
            if (localContext.Property(prefix) != null)
            {
                CreateTermDefinition(activeContext, localContext, prefix, defined);
            }
            // 15.2 - If term's prefix has a term definition in active context, set the IRI mapping of definition to the result of concatenating the value associated with the prefix's IRI mapping and the term's suffix.
            JsonLdTermDefinition prefixTermDefinition = activeContext.GetTerm(prefix);
            if (prefixTermDefinition != null)
            {
                definition.IriMapping = prefixTermDefinition.IriMapping + rest;
            }
            // 15.3 - Otherwise, term is an absolute IRI or blank node identifier. Set the IRI mapping of definition to term.
            else
            {
                definition.IriMapping = term;
            }
        }
        // 16 - Otherwise if the term contains a slash (/): 
        else if (term.Contains('/'))
        {
            // 16.1 - Term is a relative IRI reference.
            // 16.2 - Set the IRI mapping of definition to the result of IRI expanding term.
            definition.IriMapping = ExpandIri(activeContext, term, vocab: true);
            // If the resulting IRI mapping is not an IRI, an invalid IRI mapping error has been detected and processing is aborted.
            if (!JsonLdUtils.IsIri(definition.IriMapping))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                    $"The expansion of the term {term} resulted in the value {definition.IriMapping} which is not an IRI.");
            }
        }
        // 17 -  if term is @type, set the IRI mapping of definition to @type.
        else if (term.Equals("@type"))
        {
            definition.IriMapping = "@type";
        }
        // 18 - Otherwise, if active context has a vocabulary mapping, the IRI mapping of definition is set to the result of concatenating
        // the value associated with the vocabulary mapping and term. If it does not have a vocabulary mapping, an invalid IRI mapping error
        // been detected and processing is aborted.
        else if (activeContext.Vocab != null)
        {
            definition.IriMapping = activeContext.Vocab + term;
        }
        else
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                $"Invalid IRI Mapping. The term '{term}' could not be processed as an IRI mapping");
        }

        // 19 - if value contains the key @container
        if (containerValue != null)
        {
            // 19.1 Initialize container to the value associated with the @container entry, which MUST be either @graph, @id, @index, @language, @list, @set, @type, or an array containing exactly any one of those keywords, an array containing @graph and either @id or @index optionally including @set, or an array containing a combination of @set and any of @index, @graph, @id, @type, @language in any order . Otherwise, an invalid container mapping has been detected and processing is aborted.
            // 19.2 If the container value is @graph, @id, or @type, or is otherwise not a string, generate an invalid container mapping error and abort processing if processing mode is json - ld - 1.0.
            // 19.3 Set the container mapping of definition to container coercing to an array, if necessary.
            definition.ContainerMapping.Clear();
            definition.ContainerMapping.UnionWith(ValidateContainerMapping(term, containerValue));
            // 19.4 - If the container mapping of definition includes @type: 
            if (definition.ContainerMapping.Contains(JsonLdContainer.Type))
            {
                if (definition.TypeMapping == null)
                {
                    definition.TypeMapping = "@id";
                }
                else if (!(definition.TypeMapping.Equals("@id") || definition.TypeMapping.Equals("@vocab")))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping,
                        $"Invalid Type Mapping. The definition of term '{term}' includes an @type container mapping, but the type mapping of the term is '{definition.TypeMapping}'. Expected either '@type' or '@vocab'.");
                }
            }
        }
        // 20 - If value contains the entry @index: 
        JToken indexValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@index");
        if (indexValue != null)
        {
            // 20.1 - If processing mode is json-ld-1.0 or container mapping does not include @index, an invalid term definition has been detected and processing is aborted.
            if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition. The definition of term '{term}' includes an @index entry, which is not supported by the JSON-LD 1.0 processing mode.");
            }

            if (!definition.ContainerMapping.Contains(JsonLdContainer.Index))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition. The definition of term '{term}' includes an @index entry, but the container mapping for the term does not include @index.");
            }

            if (indexValue.Type != JTokenType.String)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition. The @index property on '{term}' must expand to an IRI.");
            }
            var index = indexValue.Value<string>();
            if (!JsonLdUtils.IsAbsoluteIri(ExpandIri(activeContext, index, vocab: true)))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition. The expansion of the @index property of '{term}' resulted in a value ({index}) which is not an IRI.");
            }

            definition.IndexMapping = index;
        }

        // 21 - If value contains the entry @context: 
        JToken contextValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@context");
        if (contextValue != null)
        {
            // 21.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
            if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition for term '{term}'. The @context property is not supported on a term definition when the processing mode is JSON-LD 1.0.");
            }
            // 21.2 - Initialize context to the value associated with the @context key, which is treated as a local context.
            JToken context = contextValue;

            // 21.3 - Invoke the Context Processing algorithm using the active context and context as local context. If any error is detected, an invalid scoped context error has been detected and processing is aborted.
            try
            {
                ProcessContext(activeContext, context, baseUrl, new List<Uri>(remoteContexts), true, true, false);
            }
            catch (JsonLdProcessorException ex)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidScopedContext,
                    $"Invalid scoped context for term '{term}'. See inner exception for details of the scope processing error.", ex);
            }

            // 21.4 - Set the local context of definition to context, and base URL to base URL.
            definition.LocalContext = context;
            definition.BaseUrl = baseUrl;
        }

        // 22 - if value contains the key @language and does not contain the key @type
        JToken languageValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@language");
        if (languageValue != null && typeValue == null)
        {
            switch (languageValue.Type)
            {
                // 22.1 - Initialize language to the value associated with the @language entry, which MUST be either null or a string.
                // If language is not well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                // Otherwise, an invalid language mapping error has been detected and processing is aborted.
                // 22.2 - Set the language mapping of definition to language. 
                case JTokenType.Null:
                    definition.LanguageMapping = null;
                    break;
                case JTokenType.String:
                    definition.LanguageMapping = languageValue.Value<string>().ToLowerInvariant(); // Processors MAY normalize language tags to lower case.
                    if (!LanguageTag.IsWellFormed(definition.LanguageMapping))
                    {
                        Warn(JsonLdErrorCode.MalformedLanguageTag,
                            $"The definition of term {term} includes an @language property whose value is not a well-formed BCP-47 language tag.");
                    }
                    break;
                default:
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapping,
                        $"Invalid Language Mapping on term '{term}'. The value of the @language property must be either null or a string");
            }
        }

        // 23 - If value contains the entry @direction and does not contain the entry @type:
        JToken directionValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@direction");
        if (directionValue != null && typeValue == null)
        {
            // 23.1 - Initialize direction to the value associated with the @direction entry, which MUST be either null, "ltr", or "rtl".Otherwise, an invalid base direction error has been detected and processing is aborted.
            // 23.2 - Set the direction mapping of definition to direction.
            definition.DirectionMapping = JsonLdUtils.ParseLanguageDirection(directionValue);
        }

        // 24 - If value contains the key @nest:
        JToken nestValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@nest");
        if (nestValue != null)
        {
            // 24.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
            if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition for term '{term}. Term definitions may not contain the @nest property when the processing mode is JSON-LD 1.0.");
            }

            // 24.2 - Initialize nest to the value associated with the @nest key, which must be a string and must not be a keyword other than @nest.
            // Otherwise, an invalid @nest value error has been detected and processing is aborted.
            if (nestValue.Type != JTokenType.String)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                    $"Invalid Nest Value for term '{term}'. The value of the @nest property must be a string.");
            }
            var nest = nestValue.Value<string>();
            if (JsonLdUtils.IsKeyword(nest) && !"@nest".Equals(nest))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                    $"Invalid Nest Value for term '{term}'. The value of the @nest property cannot be a JSON-LD keyword other than '@nest'");
            }
            definition.Nest = nest;
        }

        // 25 - If value contains the entry @prefix:
        JToken prefixValue = JsonLdUtils.GetPropertyValue(activeContext, value, "@prefix");
        if (prefixValue != null)
        {
            // 25.1 - If processing mode is json - ld - 1.0, or if term contains a colon(:) or slash(/), an invalid term definition has been detected and processing is aborted.
            if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition for term '{term}'. Term definitions may not contain the @prefix property when the processing mode is JSON-LD 1.0.");
            }

            if (term.Contains(':') || term.Contains('/'))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition for term '{term}'. A term containing a slash (/) or colon (:) character may not be defined as a prefix.");
            }

            // 25.2 - Set the prefix flag to the value associated with the @prefix entry, which MUST be a boolean. Otherwise, an invalid @prefix value error has been detected and processing is aborted.
            if (prefixValue.Type != JTokenType.Boolean)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidPrefixValue,
                    $"Invalid @prefix Value. The value of the @prefix property of the term '{term}' must be a boolean value.");
            }
            definition.Prefix = prefixValue.Value<bool>();

            // 25.3 - If the prefix flag of definition is set to true, and its IRI mapping is a keyword, an invalid term definition has been detected and processing is aborted.
            if (definition.Prefix && JsonLdUtils.IsKeyword(definition.IriMapping))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                    $"Invalid Term Definition. The term '{term}' is defined as a prefix, but its IRI mapping is a JSON-LD keyword.");
            }
        }

        // 26 - If the value contains any key other than @id, @reverse, @container, @context, @nest, or @type, an invalid term definition error has been detected and processing is aborted.
        var unrecognizedKeys = value.Properties().Select(prop => prop.Name).Where(x => !JsonLdKeywords.TermDefinitionKeys.Contains(x)).ToList();
        if (unrecognizedKeys.Any())
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, $"Invalid Term Definition for term '{term}'. Term definition contains unrecognised property key(s) {string.Join(", ", unrecognizedKeys)}");
        }

        // 27 - If override protected is false and previous definition exists and is protected; 
        if (!overrideProtected && previousDefinition != null && previousDefinition.Protected)
        {
            // 27.1 - If definition is not the same as previous definition (other than the value of protected), a protected term redefinition error has been detected, and processing is aborted.
            if (!definition.EquivalentTo(previousDefinition))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.ProtectedTermRedefinition,
                    $"Protected Term Redefinition. The term '{term}' attempts to change the previously encountered protected definition.");
            }

            // 27.2 - Set definition to previous definition to retain the value of protected.
            definition = previousDefinition;
        }
        // 28 - Set the term definition of term in active context to definition and set the value associated with defined's entry term to true.
        activeContext.SetTerm(term, definition);
        defined[term] = true;
    }

    /// <summary>
    /// Implementation of IRI Expansion algorithm.
    /// </summary>
    /// <param name="activeContext"></param>
    /// <param name="value"></param>
    /// <param name="vocab"></param>
    /// <param name="documentRelative"></param>
    /// <param name="localContext"></param>
    /// <param name="defined"></param>
    /// <returns></returns>
    public string ExpandIri(JsonLdContext activeContext, string value, bool vocab = false, bool documentRelative = false, JObject localContext = null, Dictionary<string, bool> defined = null)
    {
        if (defined == null) defined = new Dictionary<string, bool>();

        // 1. If value is a keyword or null, return value as is.
        if (value == null || JsonLdUtils.IsKeyword(value)) return value;

        // 2 - If value has the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), a processor SHOULD generate a warning and return null.
        if (JsonLdUtils.MatchesKeywordProduction(value))
        {
            Warn(JsonLdErrorCode.InvalidTermDefinition,
                $"The term {value} matches the production @[a-zA-Z] which is reserved by the JSON-LD specification.");
            return null;
        }

        // 3 -  If local context is not null, it contains an entry with a key that equals value, and the value of the entry for value in defined is not true,
        // invoke the Create Term Definition algorithm, passing active context, local context, value as term, and defined.
        // This will ensure that a term definition is created for value in active context during Context Processing. 
        if (localContext != null && localContext.ContainsKey(value) && defined.TryGetValue(value, out var isDefined) && !isDefined)
        {
            CreateTermDefinition(activeContext, localContext, value, defined);
        }

        var hasTerm = activeContext.TryGetTerm(value, out JsonLdTermDefinition termDefinition);

        // 4. If active context has a term definition for value, and the associated IRI mapping is a keyword, return that keyword.
        if (hasTerm && JsonLdUtils.IsKeyword(termDefinition.IriMapping))
        {
            return termDefinition.IriMapping;
        }

        // 5. If vocab is true and the active context has a term definition for value, return the associated IRI mapping.
        if (vocab && hasTerm)
        {
            return termDefinition.IriMapping;
        }

        // 6. If value contains a colon (:) anywhere after the first character, it is either an IRI, a compact IRI, or a blank node identifier:
        var ix = value.IndexOf(':');
        if (ix > 0)
        {
            // 6.1 Split value into a prefix and suffix at the first occurrence of a colon (:).
            var prefix = value.Substring(0, ix);
            var suffix = value.Substring(ix + 1);

            // 6.2 If prefix is underscore (_) or suffix begins with double-forward-slash (//), 
            // return value as it is already an absolute IRI or a blank node identifier.
            if (prefix.Equals("_") || suffix.StartsWith("//"))
            {
                return value;
            }

            // 6.3 If local context is not null, it contains a prefix entry, and the value of the prefix entry in defined is not true,
            // invoke the Create Term Definition algorithm, passing active context, local context, prefix as term, and defined.
            // This will ensure that a term definition is created for prefix in active context during Context Processing.
            defined.TryGetValue(prefix, out var prefixDefined);
            if (localContext?.Property(prefix) != null && !prefixDefined)
            {
                CreateTermDefinition(activeContext, localContext, prefix, defined);
            }

            // 6.4 If active context contains a term definition for prefix having a non-null IRI mapping and the prefix flag of the term
            // definition is true, return the result of concatenating the IRI mapping associated with prefix and suffix.
            if (activeContext.TryGetTerm(prefix, out termDefinition) && termDefinition.Prefix && termDefinition.IriMapping != null)
            {
                return termDefinition.IriMapping + suffix;
            }

            // 6.5 - If value has the form of an IRI, return value.
            if (JsonLdUtils.IsIri(value)) return value;
        }

        // 7 If vocab is true, and active context has a vocabulary mapping, return the result of concatenating the vocabulary mapping with value.
        if (vocab && activeContext.Vocab != null)
        {
            return activeContext.Vocab + value;
        }

        // 8 Otherwise, if document relative is true, set value to the result of resolving value against the base IRI. 
        // TODO: Only the basic algorithm in section 5.2 of [RFC3986] is used; neither Syntax-Based Normalization nor Scheme-Based Normalization are performed. Characters additionally allowed in IRI references are treated in the same way that unreserved characters are treated in URI references, per section 6.5 of [RFC3987].
        if (documentRelative && activeContext.Base != null)
        {
            var iri = new Uri(activeContext.Base, value);
            return iri.ToString();
        }

        // 9 Return value as is
        return value;
    }


    private static void ValidateTypeRedefinition(JToken value)
    {
        // At this point, value MUST be a map with only either or both of the following entries:
        //   An entry for @container with value @set.
        //   An entry for @protected.
        // Any other value means that a keyword redefinition error has been detected and processing is aborted.
        var isValid = value.Type == JTokenType.Object;
        if (isValid)
        {
            isValid = false;
            var o = value as JObject;
            foreach (JProperty p in o.Properties())
            {
                isValid = p.Name == "@container" ? "@set".Equals(p.Value.Value<string>()) : p.Name.Equals("@protected");
                if (!isValid) break;
            }
        }

        if (!isValid)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.KeywordRedefinition,
                "Attempt to redefine keyword @type.");
        }
    }

    private bool GetProtectedProperty(JObject map, bool defaultValue)
    {
        JProperty protectedProperty = map.Property("@protected");
        if (protectedProperty == null) return defaultValue;
        if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.ProcessingModeConflict,
                "Processing mode conflict. Processor options specify JSON-LD 1.0 processing mode, but encountered @protected that requires JSON-LD 1.1 processing features");
        }

        if (protectedProperty.Value.Type != JTokenType.Boolean)
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidProtectedValue,
                "The value of @protected must be a boolean true or false.");
        }

        return protectedProperty.Value.Value<bool>();
    }

    private ISet<JsonLdContainer> ValidateContainerMapping(string term, JToken containerValue)
    {
        // 19.1 Initialize container to the value associated with the @container entry, which MUST be either @graph, @id, @index, @language, @list, @set, @type,
        // or an array containing exactly any one of those keywords, an array containing @graph and either @id or @index optionally including @set,
        // or an array containing a combination of @set and any of @index, @graph, @id, @type, @language in any order.
        // Otherwise, an invalid container mapping has been detected and processing is aborted.
        var containerMapping = new HashSet<JsonLdContainer>();
        switch (containerValue.Type)
        {
            case JTokenType.String:
                containerMapping.Add(ParseContainerMapping(term, containerValue.Value<string>()));
                break;
            case JTokenType.Array when Options.ProcessingMode == JsonLdProcessingMode.JsonLd10:
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                    $"Invalid Container Mapping. The value of the @container property of term '{term}' is an array, but the processing mode is set to JSON-LD 1.0.");
            case JTokenType.Array:
                {
                    foreach (JToken entry in containerValue.Children())
                    {
                        if (entry.Type != JTokenType.String)
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                                $"Invalid Container Mapping. The value of the @container property of term '{term}' is an array containing non-string entries.");
                        }

                        containerMapping.Add(ParseContainerMapping(term, entry.Value<string>()));
                    }

                    break;
                }
            default:
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                    $"Invalid Container Mapping. The value of the @container property of term '{term}' must be a string or an array of strings.");
        }

        if (containerMapping.Count == 1) return containerMapping;
        if (containerMapping.Contains(JsonLdContainer.Graph) &&
            containerMapping.Contains(JsonLdContainer.Id) || containerMapping.Contains(JsonLdContainer.Index))
        {
            switch (containerMapping.Count)
            {
                case 2:
                    return containerMapping;
                case 3 when containerMapping.Contains(JsonLdContainer.Set):
                    return containerMapping;
            }
        }

        if (containerMapping.Contains(JsonLdContainer.Set) && !containerMapping.Contains(JsonLdContainer.List))
            return containerMapping;

        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
            $"Invalid Container Mapping. The value of the @container property of term '{term}' contains an invalid combination of container keywords.");
    }

    private JsonLdContainer ParseContainerMapping(string term, string containerValue)
    {
        if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
        {
            switch (containerValue)
            {
                case "@graph":
                case "@id":
                case "@type":
                    // 19.2 - If the container value is @graph, @id, or @type, or is otherwise not a string, generate an invalid container mapping error and abort processing if processing mode is json-ld-1.0.
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                        $"The term {term} specifies a container mapping of {containerValue} which is not supported under the JSON-LD 1.0 processing model.");
                case "@index":
                    return JsonLdContainer.Index;
                case "@language":
                    return JsonLdContainer.Language;
                case "@list":
                    return JsonLdContainer.List;
                case "@set":
                    return JsonLdContainer.Set;
            }

        }
        switch (containerValue)
        {
            case "@graph":
                return JsonLdContainer.Graph;
            case "@id":
                return JsonLdContainer.Id;
            case "@index":
                return JsonLdContainer.Index;
            case "@language":
                return JsonLdContainer.Language;
            case "@list":
                return JsonLdContainer.List;
            case "@set":
                return JsonLdContainer.Set;
            case "@type":
                return JsonLdContainer.Type;
        }
        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
            $"The term {term} specifies an unrecognized container mapping of {containerValue}.");
    }
}
