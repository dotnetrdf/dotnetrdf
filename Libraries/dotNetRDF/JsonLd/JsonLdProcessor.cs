/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using VDS.Common.Collections;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using Exception = System.Exception;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Implements the core JSON-LD processing. 
    /// </summary>
    public class JsonLdProcessor
    {
        private Uri _base;
        private readonly JsonLdProcessorOptions _options;
        private static readonly string[] JsonLdKeywords = {
            "@base",
            "@container",
            "@context",
            "@direction",
            "@graph",
            "@id",
            "@import",
            "@included",
            "@index",
            "@json",
            "@language",
            "@list",
            "@nest",
            "@none",
            "@prefix",
            "@propagate",
            "@protected",
            "@reverse",
            "@value",
            "@set",
            "@type",
            "@value",
            "@version",
            "@vocab",
        };

        private static readonly string[] JsonLdFramingKeywords = {
            "@default",
            "@embed",
            "@explicit",
            "@omitDefault",
            "@requireAll",
        };

        private static readonly string[] TermDefinitionKeys = {
            "@id",
            "@reverse",
            "@container",
            "@context",
            "@direction",
            "@index",
            "@language",
            "@nest",
            "@prefix",
            "@protected",
            "@type",
        };
        private static readonly string[] ValueObjectKeys = {
            "@direction",
            "@value",
            "@language",
            "@type",
            "@index",
        };

        private static readonly string[] GraphObjectKeys =
        {
            "@graph",
            "@id",
            "@index",
        };

        private static readonly string[] JsonLdContextKeywords = {
            "@base", 
            "@direction", 
            "@import", 
            "@language", 
            "@propagate",
            "@protected",
            "@version",
            "@vocab",
        };

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

        private readonly Dictionary<string, string> _identifierMap;
        private int _counter;
        private readonly Dictionary<Uri, JsonLdRemoteContext> _remoteContextCache;

        /// <summary>
        /// Create a new processor instance.
        /// </summary>
        /// <param name="options">JSON-LD processing options.</param>
        private JsonLdProcessor(JsonLdProcessorOptions options) {
            if (options == null) options = new JsonLdProcessorOptions();
            _options = options;
            ProcessingMode = _options.ProcessingMode;
            _identifierMap = new Dictionary<string, string>();
            _remoteContextCache = new Dictionary<Uri, JsonLdRemoteContext>();
            _counter = 0;
        }

        /// <summary>
        /// Get or set the base IRI for processing.
        /// </summary>
        /// <remarks>This value should be set to the IRI of the document being processed if available.
        /// </remarks>
        public Uri BaseIri
        {
            get => _options.Base ?? _base;
            set => _base = value;
        }

        /// <summary>
        /// Get or set the current processing mode.
        /// </summary>
        public JsonLdProcessingMode? ProcessingMode
        {
            get; set;
        }

        /// <summary>
        /// Get the warnings generated by the processor
        /// </summary>
        /// <remarks>May be an empty list</remarks>
        public List<JsonLdProcessorWarning> Warnings
        {
            get;
        } = new List<JsonLdProcessorWarning>();

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
            var result = activeContext.Clone();

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
            localContext = EnsureArray(localContext);

            // 5. For each item context in local context:
            foreach (var context in (JArray) localContext)
            {
                // 5.1 if context is null:
                if (context.Type == JTokenType.Null)
                {
                    // 5.1.1 If override protected is false and active context contains any protected term definitions,
                    // an invalid context nullification has been detected and processing is aborted.
                    if (!overrideProtected && activeContext.HasProtectedTerms())
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
                    var remoteUrl = baseUrl == null ? new Uri(contextStr) : new Uri(baseUrl, contextStr);
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
                    if (_options.RemoteContextLimit >= 0 &&  remoteContexts.Count >= _options.RemoteContextLimit)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.ContextOverflow, "Number of loaded remote context references exceeds the limit.");
                    }
                    if (!remoteContexts.Contains(remoteUrl)) remoteContexts.Add(remoteUrl);

                    // 5.2.4 If context was previously dereferenced, then the processor MUST NOT do a further dereference,
                    // and context is set to the previously established internal representation:
                    // set context document to the previously dereferenced document, and set loaded context to the value of the @context entry from the document in context document.
                    var loadedContext = GetRemoteContext(remoteUrl); // 5.2.4, 5.2.5
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
                var contextObject = context as JObject; // TODO: Rename local variable to contextDefinition

                // 5.5 - If context has an @version entry:
                var versionProperty = contextObject.Property("@version");
                if (versionProperty != null)
                {
                    // 5.5.1 - If the associated value is not 1.1, an invalid @version value has been detected, and processing is aborted.
                    var versionValue = versionProperty.Value.Value<string>();
                    if (!"1.1".Equals(versionValue))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVersionValue, $"Found invalid value for @version property: {versionValue}.");
                    }
                    // 5.5.2 - If processing mode is set to json-ld-1.0, a processing mode conflict error has been detected and processing is aborted.
                    if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.ProcessingModeConflict,
                            "Processing mode conflict. Processor options specify JSON-LD 1.0 processing mode, but encountered @version that requires JSON-LD 1.1 processing features");
                    }
                }

                // 5.6 - If context has an @import entry: 
                var importProperty = contextObject.Property("@import");
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
                    var remoteContext = GetRemoteContext(import); 
                    if (!(remoteContext.Context is JObject importContext))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                            "The value of the @context of the remote document referenced by @import must be a map.");
                    } else if (importContext.ContainsKey("@import"))
                    {
                        // 5.6.7 - If import context has a @import entry, an invalid context entry error has been detected and processing is aborted.
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContextEntry,
                            $"The remote context from {import} contains an @import property.");
                    }
                    // 5.6.8 - Set context to the result of merging context into import context, replacing common entries with those from context.
                    var tmp = new JObject(importContext);
                    tmp.Merge(contextObject);
                    contextObject = tmp;
                }

                // 5.7 - If context has an @base key and remote contexts is empty, i.e., the currently being processed context is not a remote context
                var baseProperty = contextObject.Property("@base");
                if (baseProperty != null && remoteContexts.Count == 0)
                {
                    // 5.7.1 - Initialize value to the value associated with the @base entry.
                    var value = baseProperty.Value;
                    // 5.7.2 - If value is null, remove the base IRI of result.
                    if (value.Type == JTokenType.Null)
                    {
                        result.RemoveBase();
                    }
                    // 5.7.3 - Otherwise, if value is an absolute IRI, the base IRI of result is set to value.
                    else if (IsAbsoluteIri(value))
                    {
                        result.Base = new Uri(value.Value<string>());
                    }
                    // 5.7.4 - Otherwise, if value is a relative IRI and the base IRI of result is not null, set the base IRI of result to the result of resolving
                    // value against the current base IRI of result.
                    else if (IsRelativeIri(value))
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
                var contextProperty = contextObject.Property("@vocab");
                if (contextProperty != null)
                {
                    // 5.8.1 - Initialize value to the value associated with the @vocab key.
                    var value = contextProperty.Value;
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
                        if (IsIri(str) || string.Empty.Equals(str) || IsBlankNodeIdentifier(str))
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
                var languageProperty = contextObject.Property("@language");
                if (languageProperty != null)
                {
                    // 5.9.1 - Initialize value to the value associated with the @language key.
                    var value = languageProperty.Value;
                    switch (value.Type)
                    {
                        case JTokenType.Null:
                            // 5.9.2 - If value is null, remove any default language from result.
                            result.Language = null;
                            break;
                        case JTokenType.String:
                            // 5.9.3 - Otherwise, if value is string, the default language of result is set to value.
                            result.Language = value.Value<string>().ToLowerInvariant(); // Processors MAY normalize language tags to lower case.
                            // TODO:  If value is not well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                            break;
                        default:
                            // 5.9.3 (cont) - If it is not a string, an invalid default language error has been detected and processing is aborted.
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidDefaultLanguage,
                                "@language property value must be a JSON string or null.");
                    }
                }

                // 5.10 - If context has an @direction entry
                var directionProperty = contextObject.Property("@direction");
                if (directionProperty != null)
                {
                    // 5.10.1 - If processing mode is json - ld - 1.0, an invalid context entry error has been detected and processing is aborted.
                    CheckProcessingMode("@direction");
                    // 5.10.2 - Initialize value to the value associated with the @direction entry.
                    // 5.10.3 - If value is null, remove any base direction from result.
                    // 5.10.4 - Otherwise, if value is a string, the base direction of result is set to value.
                    // If it is not null, "ltr", or "rtl", an invalid base direction error has been detected and processing is aborted.
                    result.BaseDirection = ParseLanguageDirection(directionProperty.Value);
                }

                // 5.11 - context has an @propagate entry:
                var propagateProperty = contextObject.Property("@propagate");
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
                var protectedProperty = contextObject.Property("@protected");
                if (protectedProperty != null)
                {
                    if (protectedProperty.Value.Type != JTokenType.Boolean)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidProtectedValue,
                            "The value of @protected must be a boolean");
                    }

                    @protected = protectedProperty.Value.Value<bool>();
                }
                foreach (var property in contextObject.Properties())
                {
                    var key = property.Name;
                    if (!JsonLdContextKeywords.Contains(key))
                    {
                        CreateTermDefinition(result, contextObject, key, defined, baseUrl, @protected, overrideProtected, new List<Uri>(remoteContexts) );
                    }
                }

            }
            return result;
        }

        private LanguageDirection ParseLanguageDirection(JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.Null:
                    return LanguageDirection.Unspecified;
                case JTokenType.String:
                    var directionStr = value.Value<string>();
                    switch (directionStr)
                    {
                        case "ltr":
                            return LanguageDirection.LeftToRight;
                        case "rtl":
                            return LanguageDirection.RightToLeft;
                        case null:
                            return LanguageDirection.Unspecified;
                        default:
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                                "The value of an @direction property must be 'ltr', 'rtl' or null.");
                    }
                default:
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                        "The value of an @direction property must be a string with value 'ltr' or 'rtl', or null.");
            }
        }

        private string SerializeLanguageDirection(LanguageDirection dir)
        {
            switch (dir)
            {
                case LanguageDirection.LeftToRight:
                    return "ltr";
                case LanguageDirection.RightToLeft:
                    return "rtl";
                default:
                    return null;
            }
        }

        /// <summary>
        /// If the processing mode is <see cref="JsonLdProcessingMode.JsonLd10"/>, throw a <see cref="JsonLdProcessorException"/> with error code
        /// <see cref="JsonLdErrorCode.ProcessingModeConflict"/>.
        /// </summary>
        /// <param name="keyword">The keyword that caused the check.</param>
        private void CheckProcessingMode(string keyword, JsonLdErrorCode errorCode = JsonLdErrorCode.ProcessingModeConflict)
        {
            if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
            {
                throw new JsonLdProcessorException(errorCode,
                    $"Processing mode conflict. Processor options specify JSON-LD 1.0 processing mode, but encountered {keyword} that requires JSON-LD 1.1 processing features");
            }
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
                foreach (var p in o.Properties())
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
            var protectedProperty = map.Property("@protected");
            if (protectedProperty == null) return defaultValue;
            if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
            var v = localContext[term].DeepClone();

            // 4 - If term is @type, and processing mode is json-ld-1.0, a keyword redefinition error has been detected and processing is aborted.
            if (term.Equals("@type"))
            {
                if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
            else if (IsKeyword(term))
            {
                // 5 Otherwise, since keywords cannot be overridden, term MUST NOT be a keyword and a keyword redefinition error has been detected and processing is aborted.
                throw new JsonLdProcessorException(JsonLdErrorCode.KeywordRedefinition,
                    $"Cannot redefine JSON-LD keyword {term}.");
            }
            else if (MatchesKeywordProduction(term))
            {
                // 5 (cont.) If term has the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), return; processors SHOULD generate a warning.
                Warn(JsonLdErrorCode.InvalidTermDefinition,
                    $"The term {term} has been ignored as it matches the pattern @[a-zA-Z] which is reserved for JSON-LD keywords.");
                return;
            }

            // 6 - Initialize previous definition to any existing term definition for term in active context, removing that term definition from active context.
            var previousDefinition = activeContext.RemoveTerm(term);
            var simpleTerm = false;

            // 7 - If value is null, convert it to a map consisting of a single entry whose key is @id and whose value is null.
            if (v == null || v.Type == JTokenType.Null)
            {
                v = new JObject(new JProperty("@id", null));
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
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, "A term definition must be a string, a map or null.");
            }
            var value = v as JObject;
            
            // 10 - Create a new term definition, definition, initializing prefix flag to false, protected to protected, and reverse property to false.
            var definition = new JsonLdTermDefinition {Prefix = false, Protected = @protected, Reverse = false};

            // 11 - if value has an @protected entry, set the protected flag in definition to the value of this entry. 
            definition.Protected = GetProtectedProperty(value, @protected);

            // 12 - If value contains the key @type:
            var typeValue = GetPropertyValue(activeContext, value, "@type");
            if (typeValue != null)
            {
                // 12.1 Initialize type to the value associated with the @type key, which must be a string. Otherwise, an invalid type mapping error has been detected and processing is aborted.
                if (typeValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping, $"Invalid type mapping for term {term}. The @type value must be a string, got {typeValue.Type}");
                }

                // 12.2 - Set type to the result of IRI expanding type, using local context, and defined.
                var type = ExpandIri(activeContext, typeValue.Value<string>(), true, false, localContext, defined);
                if ((type == "@json" || type == "@none") && _options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping, $"Invalid type mapping for term {term}. Unexpanded value was {typeValue.Value<string>()}, and the expanded type IRI is '{type}', but the JSON-LD Processing mode is set to 1.0 which does not support @none or @json types.");
                }
                if (type == "@id" || type == "@vocab" || type == "@json" || type == "@none" || IsAbsoluteIri(type))
                {
                    // 10.3 - Set the type mapping for definition to type.
                    definition.TypeMapping = type;
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping, $"Invalid type mapping for term {term}. Expected @type value to expand to @id, @vocab, @json, @none or an absolute IRI. Unexpanded value was {typeValue.Value<string>()}, expanded value was {type}.");
                }
            }

            var reverseValue = GetPropertyValue(activeContext, value, "@reverse");
            var containerValue = GetPropertyValue(activeContext, value, "@container");
            // 13 - If value contains the key @reverse:
            if (reverseValue != null)
            {
                // 13.1 - If value contains @id or @nest, members, an invalid reverse property error has been detected and processing is aborted.
                if (GetPropertyValue(activeContext, value, "@id") != null ||
                    GetPropertyValue(activeContext, value, "@nest") != null)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty, $"Invalid reverse property. The @reverse property cannot be combined with @id or @nest property on term {term}.");
                }

                // 13.2 - If the value associated with the @reverse key is not a string, an invalid IRI mapping error has been detected and processing is aborted.
                if (reverseValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"@reverse property value must be a string on term {term}");
                }

                // 13.3 - If the value associated with the @reverse entry is a string having the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), return; processors SHOULD generate a warning.
                if (MatchesKeywordProduction(reverseValue.Value<string>()))
                {
                    Warn(JsonLdErrorCode.InvalidIriMapping, "$@reverse property value on {term} matches the JSON-LD keyword production @[a-zA-Z]+.");
                    return;
                }

                // 13.4 - Otherwise, set the IRI mapping of definition to the result of IRI expanding the value associated with the @reverse entry, using local context,
                // and defined. If the result does not have the form of an IRI or a blank node identifier, an invalid IRI mapping error has been detected and processing
                // is aborted.
                var iriMapping = ExpandIri(activeContext, reverseValue.Value<string>(), true, false, localContext, defined);
                if (iriMapping == null || !(IsAbsoluteIri(iriMapping) || IsBlankNodeIdentifier(iriMapping)))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"@reverse property value must expand to an absolute IRI or blank node identifier. The @reverse property on term {term} expands to {iriMapping}.");
                }
                definition.IriMapping = iriMapping;

                // 13.5 - If value contains an @container entry, set the container mapping of definition to an array containing its value; if its value is neither @set, nor @index, nor null, an invalid reverse property error has been detected (reverse properties only support set- and index-containers) and processing is aborted.
                if (containerValue != null) {
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
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty, $"Invalid reverse property for term {term}. Reverse properties only support set and index container types. ");
                        }
                    }
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping, $"Invalid @container property for term {term}. Property value must be a string or null.");
                    }
                }
                // 13.6 - Set the reverse property flag of definition to true.
                definition.Reverse = true;

                // 13.7 - Set the term definition of term in active context to definition and the value associated with defined's key term to true and return.
                activeContext.SetTerm(term, definition);
                defined[term] = true;
                return;
            }

            // 14 - If value contains the key @id and its value does not equal term:
            var idValue = GetPropertyValue(activeContext, value, "@id");
            if (idValue != null && !term.Equals(idValue.Value<string>()))
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
                    if (!IsKeyword(idValueString) && MatchesKeywordProduction(idValueString))
                    {
                        Warn(JsonLdErrorCode.InvalidIdValue,
                            $"The value of the @id property of {term} matches the pattern @[a-zA-Z] which is reserved by the JSON-LD specification. This property will be ignored.");
                        return;
                    }
                    // 14.2.3 - Otherwise, set the IRI mapping of definition to the result of IRI expanding the value associated with the @id entry, using local context, and defined.
                    var iriMapping = ExpandIri(activeContext, idValue.Value<string>(), vocab:true, documentRelative:false, localContext:localContext, defined:defined);
                    
                    // If the resulting IRI mapping is neither a keyword, nor an IRI, nor a blank node identifier, an invalid IRI mapping error has been detected and processing is aborted;
                    if (!IsKeyword(iriMapping) && !IsAbsoluteIri(iriMapping) && !IsBlankNodeIdentifier(iriMapping))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping,
                            $"Invalid IRI Mapping. The value of the @id property of term '{term}' must be a keyword, an absolute IRI or a blank node identifier. Got value {iriMapping}.");
                    }

                    // if it equals @context, an invalid keyword alias error has been detected and processing is aborted.
                    if ("@context".Equals(iriMapping))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidKeywordAlias, $"Invalid keyword alias at term {term}.");
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
                         IsBlankNodeIdentifier(definition.IriMapping)))
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
                var prefixTermDefinition = activeContext.GetTerm(prefix);
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
                definition.IriMapping = ExpandIri(activeContext, term, vocab:true);
                // If the resulting IRI mapping is not an IRI, an invalid IRI mapping error has been detected and processing is aborted.
                if (!IsIri(definition.IriMapping))
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
                    } else if (!(definition.TypeMapping.Equals("@id") || definition.TypeMapping.Equals("@vocab")))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping,
                            $"Invalid Type Mapping. The definition of term '{term}' includes an @type container mapping, but the type mapping of the term is '{definition.TypeMapping}'. Expected either '@type' or '@vocab'.");
                    }
                }
            }
            // 20 - If value contains the entry @index: 
            var indexValue = GetPropertyValue(activeContext, value, "@index");
            if (indexValue != null)
            {
                // 20.1 - If processing mode is json-ld-1.0 or container mapping does not include @index, an invalid term definition has been detected and processing is aborted.
                if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
                index = ExpandIri(activeContext, index, vocab: true);
                if (!IsAbsoluteIri(index))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                        $"Invalid Term Definition. The expansion of the @index property of '{term}' resulted in a value ({index}) which is not an IRI.");
                }

                definition.IndexMapping = index;
            }

            // 21 - If value contains the entry @context: 
            var contextValue = GetPropertyValue(activeContext, value, "@context");
            if (contextValue != null)
            {
                // 21.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
                if (ProcessingMode == JsonLdProcessingMode.JsonLd10)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, 
                        $"Invalid Term Definition for term '{term}'. The @context property is not supported on a term definition when the processing mode is JSON-LD 1.0.");
                }
                // 21.2 - Initialize context to the value associated with the @context key, which is treated as a local context.
                var context = contextValue;

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
            var languageValue = GetPropertyValue(activeContext, value, "@language");
            if (languageValue != null && typeValue == null)
            {
                switch (languageValue.Type)
                {
                    // 22.1 - Initialize language to the value associated with the @language entry, which MUST be either null or a string.
                    // TODO: If language is not well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                    // Otherwise, an invalid language mapping error has been detected and processing is aborted.
                    // 22.2 - Set the language mapping of definition to language. 
                    case JTokenType.Null:
                        definition.LanguageMapping = null;
                        break;
                    case JTokenType.String:
                        definition.LanguageMapping = languageValue.Value<string>().ToLowerInvariant(); // Processors MAY normalize language tags to lower case.
                        break;
                    default:
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapping, 
                            $"Invalid Language Mapping on term '{term}'. The value of the @language property must be either null or a string");
                }
            }

            // 23 - If value contains the entry @direction and does not contain the entry @type:
            var directionValue = GetPropertyValue(activeContext, value, "@direction");
            if (directionValue != null && typeValue == null)
            {
                // 23.1 - Initialize direction to the value associated with the @direction entry, which MUST be either null, "ltr", or "rtl".Otherwise, an invalid base direction error has been detected and processing is aborted.
                // 23.2 - Set the direction mapping of definition to direction.
                definition.DirectionMapping = ParseLanguageDirection(directionValue);
            }

            // 24 - If value contains the key @nest:
            var nestValue = GetPropertyValue(activeContext, value, "@nest");
            if(nestValue != null)
            {
                // 24.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
                if (ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
                if (IsKeyword(nest) && !"@nest".Equals(nest))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue, 
                        $"Invalid Nest Value for term '{term}'. The value of the @nest property cannot be a JSON-LD keyword other than '@nest'");
                }
                definition.Nest = nest;
            }

            // 25 - If value contains the entry @prefix:
            var prefixValue = GetPropertyValue(activeContext, value, "@prefix");
            if (prefixValue != null)
            {
                // 25.1 - If processing mode is json - ld - 1.0, or if term contains a colon(:) or slash(/), an invalid term definition has been detected and processing is aborted.
                if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
                if (definition.Prefix && IsKeyword(definition.IriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition,
                        $"Invalid Term Definition. The term '{term}' is defined as a prefix, but its IRI mapping is a JSON-LD keyword.");
                }
            }

            // 26 - If the value contains any key other than @id, @reverse, @container, @context, @nest, or @type, an invalid term definition error has been detected and processing is aborted.
            var unrecognizedKeys = value.Properties().Select(prop => prop.Name).Where(x => !TermDefinitionKeys.Contains(x)).ToList();
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

        private void Warn(JsonLdErrorCode errorCode, string message)
        {
            Warnings.Add(new JsonLdProcessorWarning(errorCode, message));
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
        private string ExpandIri(JsonLdContext activeContext, string value, bool vocab = false, bool documentRelative = false, JObject localContext = null, Dictionary<string, bool> defined = null)
        {
            if (defined == null) defined = new Dictionary<string, bool>();

            // 1. If value is a keyword or null, return value as is.
            if (value == null || IsKeyword(value)) return value;

            // 2 - If value has the form of a keyword (i.e., it matches the ABNF rule "@"1*ALPHA from [RFC5234]), a processor SHOULD generate a warning and return null.
            if (MatchesKeywordProduction(value))
            {
                Warn(JsonLdErrorCode.InvalidTermDefinition,
                    $"The term {value} matches the production @[a-zA-Z] which is reserved by the JSON-LD specification.");
                return null;
            }

            // 3 -  If local context is not null, it contains an entry with a key that equals value, and the value of the entry for value in defined is not true,
            // invoke the Create Term Definition algorithm, passing active context, local context, value as term, and defined.
            // This will ensure that a term definition is created for value in active context during Context Processing. 
            if (localContext != null && localContext.ContainsKey(value) && defined.TryGetValue(value, out var isDefined) && !isDefined) {
                CreateTermDefinition(activeContext, localContext, value, defined);
            }

            var hasTerm = activeContext.TryGetTerm(value, out var termDefinition);

            // 4. If active context has a term definition for value, and the associated IRI mapping is a keyword, return that keyword.
            if (hasTerm && IsKeyword(termDefinition.IriMapping))
            {
                return termDefinition.IriMapping;
            }

            // 5. If vocab is true and the active context has a term definition for value, return the associated IRI mapping.
            if (vocab && hasTerm)
            {
                return termDefinition?.IriMapping;
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
                defined.TryGetValue(prefix, out bool prefixDefined);
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
                if (IsIri(value)) return value;
            }

            // 7 If vocab is true, and active context has a vocabulary mapping, return the result of concatenating the vocabulary mapping with value.
            if (vocab && activeContext.Vocab != null)
            {
                return activeContext.Vocab + value;
            }

            // 8 Otherwise, if document relative is true, set value to the result of resolving value against the base IRI. 
            // TODO: Only the basic algorithm in section 5.2 of [RFC3986] is used; neither Syntax-Based Normalization nor Scheme-Based Normalization are performed. Characters additionally allowed in IRI references are treated in the same way that unreserved characters are treated in URI references, per section 6.5 of [RFC3987].
            if (documentRelative && activeContext.HasBase)
            {
                var iri = new Uri(activeContext.Base, value);
                return iri.ToString();
            }

            // 9 Return value as is
            return value;
        }

        private static IEnumerable<WebLink> ParseLinkHeaders(IEnumerable<string> linkHeaderValues)
        {
            foreach(var linkHeaderValue in linkHeaderValues)
            {
                var fields = linkHeaderValue.Split(';').Select(x => x.Trim());
                var linkValue = fields.First().TrimStart('<').TrimEnd('>');
                var relTypes = new List<string>();
                var mediaTypes = new List<string>();
                foreach(var field in fields)
                {
                    var split = field.Split(new char[] { '=' }, 2);
                    if (split.Length == 2)
                    {
                        var key = split[0].Trim();
                        var value = split[1].Trim();
                        if (key.Equals("rel"))
                        {
                            value = value.Trim('"');
                            relTypes.AddRange(value.Split(' '));
                        }
                        else if (key.Equals("type"))
                        {
                            mediaTypes.Add(value);
                        }
                    }
                }

                yield return new WebLink {LinkValue = linkValue, RelationTypes = relTypes, MediaTypes = mediaTypes};
            }
        }

        internal class WebLink
        {
            public string LinkValue { get; set; }
            public List<string> RelationTypes { get; set; }
            public List<string> MediaTypes { get; set; }
        }

        /// <summary>
        /// Implementation of the Expansion Algorithm.
        /// </summary>
        /// <param name="activeContext"></param>
        /// <param name="activeProperty"></param>
        /// <param name="element"></param>
        /// <param name="baseUrl">The base URL assocaited with the document URL of the original document to expand</param>
        /// <param name="frameExpansion"></param>
        /// <param name="ordered"></param>
        /// <param name="fromMap"></param>
        /// <returns></returns>
        private JToken ExpandAlgorithm(JsonLdContext activeContext, string activeProperty, JToken element, Uri baseUrl, 
            bool frameExpansion = false, bool ordered = false, bool fromMap = false)
        {
            JToken result = null;

            // 1 - If element is null, return null.
            if (element.Type == JTokenType.Null)
            {
                return null;
            }

            // 2 - If active property is @default, initialize the frameExpansion flag to false.
            if ("@default".Equals(activeProperty)) frameExpansion = false;

            JsonLdTermDefinition activePropertyTermDefinition = null;
            var hasTermDefinition = activeProperty != null && activeContext.TryGetTerm(activeProperty,
                out activePropertyTermDefinition);
            // 3 - If active property has a term definition in active context with a local context, initialize property-scoped context to that local context.
            JToken propertyScopedContext = null;
            if (hasTermDefinition && activePropertyTermDefinition.LocalContext != null)
            {
                propertyScopedContext = activePropertyTermDefinition.LocalContext;
            }

            // 4 - If element is a scalar,
            if (IsScalar(element))
            {
                // 4.1 - If active property is null or @graph, drop the free-floating scalar by returning null.
                if (activeProperty == null || activeProperty == "@graph") return null;

                // 4.2 - If property-scoped context is defined, set active context to the result of the Context Processing algorithm, passing active context, property-scoped context as local context, and base URL from the term definition for active property in active context.
                if (propertyScopedContext != null)
                {
                    activeContext = ProcessContext(activeContext, propertyScopedContext,
                        activePropertyTermDefinition.BaseUrl);
                }

                // 4.3 - Return the result of the Value Expansion algorithm, passing the active context, active property, and element as value.
                return ExpandValue(activeContext, activeProperty, element);
            }

            // 5 - If element is an array,
            if (element.Type == JTokenType.Array)
            {
                // 5.1 - Initialize an empty array, result.
                result = new JArray();
                var resultArray = result as JArray;

                // 5.2 - For each item in element:
                foreach(var item in (element as JArray))
                {
                    // 5.2.1 - Initialize expanded item to the result of using this algorithm recursively, passing active context, active property, and item as element.
                    var expandedItem = ExpandAlgorithm(activeContext, activeProperty, item, baseUrl, frameExpansion, ordered, fromMap);
                    if (expandedItem != null)
                    {
                        // 5.2.2 - If the container mapping of active property includes @list, and expanded item is an array, set expanded item to a new map
                        // containing the entry @list where the value is the original expanded item.
                        if (hasTermDefinition &&
                            activePropertyTermDefinition.ContainerMapping.Contains(JsonLdContainer.List) &&
                            expandedItem.Type == JTokenType.Array)
                        {
                            expandedItem = new JObject(new JProperty("@list", expandedItem));
                        }
                        // 5.2.3 - If expanded item is an array, append each of its items to result. Otherwise, if expanded item is not null, append it to result.
                        if (expandedItem.Type == JTokenType.Array)
                        {
                            foreach (var arrayItem in expandedItem as JArray)
                            {
                                resultArray.Add(arrayItem);
                            }
                        }
                        else
                        {
                            resultArray.Add(expandedItem);
                        }
                    }
                }
                // 5.3 - Return result
                return result;
            }

            // 6 - Otherwise element is a map.
            var elementObject = element as JObject;

            // 7 - If active context has a previous context, the active context is not propagated.
            // If from map is undefined or false, and element does not contain an entry expanding to @value,
            // and element does not consist of a single entry expanding to @id (where entries are IRI expanded, set active context to previous context from active context, as the scope of a term-scoped context does not apply when processing new node objects.
            if (activeContext.PreviousContext != null && 
                !fromMap &&
                GetPropertyValue(activeContext, elementObject, "@value") == null &&
                !(elementObject.Count == 1 && GetPropertyValue(activeContext, elementObject, "@id") != null))
            {
                activeContext = activeContext.PreviousContext;
            }

            // 8 - If property-scoped context is defined, set active context to the result of the Context Processing algorithm, passing active context, property-scoped context as local context, base URL from the term definition for active property, in active context and true for override protected.
            if (propertyScopedContext != null)
            {
                activeContext = ProcessContext(activeContext, propertyScopedContext,
                    activePropertyTermDefinition.BaseUrl, overrideProtected: true);
            }

            // 9 - If element contains the key @context, set active context to the 
            // result of the Context Processing algorithm, passing active context 
            // and the value of the @context key as local context.
            var contextValue = GetPropertyValue(activeContext, elementObject, "@context");
            if (contextValue != null)
            {
                activeContext = ProcessContext(activeContext, contextValue, baseUrl);
            }

            // 10 - Initialize type-scoped context to active context. This is used for expanding values that may be relevant to any previous type-scoped context.
            var typeScopedContext = activeContext;

            // 11 - For each key/value pair in element ordered lexicographically by key where key expands to @type using the IRI Expansion algorithm, passing active context, key for value, and true for vocab:
            var typeProperties = elementObject.Properties().Where(property => "@type".Equals(ExpandIri(activeContext, property.Name, true))).OrderBy(p => p.Name).ToList();
            foreach(var property in typeProperties)
            {
                // 11.1 Convert value into an array, if necessary.
                var values = property.Value.Type == JTokenType.Array ? property.Value as JArray : new JArray(property.Value);
                // 11.2 For each term which is a value of value ordered lexicographically
                foreach (var term in values.OrderBy(v => v))
                {
                    // if term is a string, and term's term definition in type-scoped context has a local context, set active context to the result Context Processing algorithm, passing active context, the value of the term's local context as local context, base URL from the term definition for value in active context, and false for propagate.
                    if (term.Type == JTokenType.String &&
                        typeScopedContext.TryGetTerm(term.Value<string>(), out var termDefinition) &&
                        termDefinition.LocalContext != null)
                    {
                        activeContext = ProcessContext(activeContext, termDefinition.LocalContext,
                            termDefinition.BaseUrl, propagate: false);
                    }
                }
            }

            // 12 - Initialize two empty maps, result and nests. Initialize input type to expansion of the last value of the first entry in element expanding to @type (if any), ordering entries lexicographically by key. Both the key and value of the matched entry are IRI expanded. 
            // NOTE: nests is only used in steps 13/14 so is initialized in ExpandElement
            // TODO: Should inputType also be calculated in ExpandElement rather than here?
            result = new JObject();
            var resultObject = result as JObject;
            var firstTypeProperty = elementObject.Properties()
                .OrderBy(p => p.Name).FirstOrDefault(p => "@type".Equals(ExpandIri(activeContext, p.Name, true)));
            JToken inputType = null;
            if (firstTypeProperty != null)
            {
                inputType = firstTypeProperty.Value is JArray typeArray
                    ? typeArray[typeArray.Count - 1]
                    : firstTypeProperty.Value;
                inputType = ExpandIri(activeContext, inputType.Value<string>(), true);
            }

            // Implements 13 - 14
            ExpandElement(resultObject, inputType, activeContext, activeProperty, baseUrl, frameExpansion, ordered, elementObject, typeScopedContext);

            // 15 - If result contains the entry @value: 
            if (resultObject.ContainsKey("@value"))
            {
                // 15.1 - The result must not contain any entries other than @direction, @index, @language, @type, and @value. It must not contain an @type entry if it contains either @language or @direction entries. Otherwise, an invalid value object error has been detected and processing is aborted.
                if (resultObject.Properties().Any(p => !ValueObjectKeys.Contains(p.Name)))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                        $"Invalid value object. Expanding the value of {activeProperty} resulted in a value object with additional properties.");
                }
                if ((resultObject.ContainsKey("@language") || resultObject.ContainsKey("@direction")) && resultObject.ContainsKey("@type"))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                        $"Invalid value object. The expansion of {activeProperty} resulted in a value object with both @type and either @language or @direction.");
                }

                var resultType = resultObject.ContainsKey("@type") ? resultObject["@type"] : null;
                var resultValue = resultObject["@value"];
                // If the result's @type entry is @json, then the @value entry may contain any value, and is treated as a JSON literal.
                if (resultType != null && resultType.Value<string>().Equals("@json"))
                {
                    // No-op
                }
                // Otherwise, if the value of result's @value entry is null, or an empty array, return null.
                else if (resultValue == null || resultValue.Type == JTokenType.Null || resultValue is JArray resultArray && resultArray.Count == 0)
                {
                    return null;
                } else if (resultObject.ContainsKey("@language") && resultValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedValue,
                        $"Invalid langauge-tagged value. The expansion of {activeProperty} has an @language entry, but its @value entry is not a JSON string.");
                } else if (resultObject.ContainsKey("@type") && !IsAbsoluteIri(resultType))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypedValue, 
                        $"Invalid typed value. The expansion of {activeProperty} resulted in a value object with an @type entry whose value is not an IRI.");
                }
            }
            // 16 - Otherwise, if result contains the entry @type and its associated value is not an array, set it to an array containing only the associated value.
            else if (resultObject.ContainsKey("@type"))
            {
                resultObject["@type"] = EnsureArray(resultObject["@type"]);
            }
            // 17 - Otherwise, if result contains the entry @set or @list: 
            else if (resultObject.ContainsKey("@set"))
            {
                // 17.1 - The result must contain at most one other entry which must be @index. Otherwise, an invalid set or list object error has been detected and processing is aborted.
                if (resultObject.Properties().Any(p => p.Name != "@set" && p.Name != "@index"))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidSetOrListObject,
                        $"Invalid set or list object. The expansion of {activeProperty} resulted in a set object that has a property other than @set and @index.");
                }
                // 17.2 - If result contains the entry @set, then set result to the entry's associated value.
                result = resultObject["@set"];
            } else if (resultObject.ContainsKey("@list"))
            {
                // 17.1 - The result must contain at most one other entry which must be @index. Otherwise, an invalid set or list object error has been detected and processing is aborted.
                if (resultObject.Properties().Any(p => p.Name != "@list" && p.Name != "@index"))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidSetOrListObject,
                        $"Invalid set or list object. The expansion of {activeProperty} resulted in a list object that has a property other than @list and @index.");
                }
            }
            // 18 - If result is a map that contains only the entry @language, return null.
            if (result is JObject o && o.ContainsKey("@language") && o.Count == 1)
            {
                return null;
            }
            // 19 - If active property is null or @graph, drop free-floating values as follows:
            if (activeProperty == null || activeProperty.Equals("@graph"))
            {
                resultObject = result as JObject;
                if (resultObject != null)
                {
                    // 19.1 - If result is a map which is empty, or contains only the entries @value or @list, set result to null.
                    // KA: Confirmed that the filter should apply whenever resultObject contains @value or @list (not just when they are the only properties)
                    if (resultObject.Count == 0 ||
                        resultObject.Properties().Any(p => p.Name.Equals("@value") || p.Name.Equals("@list")))
                    {
                        result = null;
                    }
                    // 19.2 - Otherwise, if result is a map whose only entry is @id, set result to null.
                    // When the frameExpansion flag is set, a map containing only the @id entry is retained.
                    else if (resultObject.Count == 1 && resultObject.ContainsKey("@id") && !frameExpansion)
                    {
                        result = null;
                    }
                }
            }

            return result;
        }

        private void ExpandElement(JObject resultObject, JToken inputType, JsonLdContext activeContext, string activeProperty, Uri baseUrl, bool frameExpansion,
            bool ordered, JObject elementObject, JsonLdContext typeScopedContext)
        {
            var nests = new JObject();

            // 13 - For each key and value in element, ordered lexicographically by key if ordered is true: 
            var elementProperties = elementObject.Properties();
            if (ordered) elementProperties = elementProperties.OrderBy(p => p.Name);
            foreach (var p in elementProperties)
            {
                var key = p.Name;
                var value = p.Value;
                // 13.1 - If key is @context, continue to the next key.
                if (key.Equals("@context")) continue;
                // 13.2 - Initialize expanded property to the result of IRI expanding key.
                var expandedProperty = ExpandIri(activeContext, key, true);
                // 13.3 - If expanded property is null or it neither contains a colon (:) nor it is a keyword, drop key by continuing to the next key.
                if (expandedProperty == null) continue;
                if (!expandedProperty.Contains(':') && !IsKeyword(expandedProperty)) continue;
                JToken expandedValue = null;

                // 13.4 - If expanded property is a keyword: 
                if (IsKeyword(expandedProperty))
                {
                    // 13.4.1 - If active property equals @reverse, an invalid reverse property map error has been detected and processing is aborted.
                    if ("@reverse".Equals(activeProperty))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReversePropertyMap,
                            $"Invalid reverse property map. The term '{key}' on {activeProperty} expands to the @reverse keyword.");
                    }

                    // 13.4.2 - If result already has an expanded property entry, other than @included or @type (unless processing mode is json-ld-1.0),
                    // a colliding keywords error has been detected and processing is aborted.
                    if (resultObject.ContainsKey(expandedProperty) && expandedProperty != "@included" &&
                        expandedProperty != "@type")
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.CollidingKeywords,
                            $"Multiple entries in the value of {activeProperty} expand to the property {expandedProperty}. The second encountered one was {key}.");
                    }

                    // 13.4.3 - If expanded property is @id: 
                    if ("@id".Equals(expandedProperty))
                    {
                        // 13.4.3.1 - If value is not a string, an invalid @id value error has been detected and processing is aborted. When the frameExpansion flag is set, value MAY be an empty map, or an array of one or more strings.
                        // 13.4.3.2 - Otherwise, set expanded value to the result of IRI expanding value using true for document relative and false for vocab.
                        // When the frameExpansion flag is set, expanded value will be an array of one or more of the values, with string values expanded using the IRI Expansion algorithm as above.
                        switch (value.Type)
                        {
                            case JTokenType.String:
                                expandedValue = ExpandIri(activeContext, value.Value<string>(), false, true);
                                if (frameExpansion) expandedValue = new JArray(expandedValue);
                                break;

                            case JTokenType.Array when !frameExpansion:
                            case JTokenType.Object when !frameExpansion:
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                    $"Invalid @id value. The value of the property {key} in the value of {activeProperty} is not a string, but {key} expands to the keyword @id.");

                            case JTokenType.Array when value.Children().Any(c => c.Type != JTokenType.String):
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                    $"Invalid @id value. The property {key} in the value of {activeProperty} expands to the @id keyword, but its array value contains one or more non-string items.");

                            case JTokenType.Array:
                                var newArray = new JArray();
                                foreach (var child in value.Children())
                                    newArray.Add(ExpandIri(activeContext, child.Value<string>(), false, true));
                                expandedValue = newArray;
                                break;

                            case JTokenType.Object when value.Children().Any():
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                    $"Invalid @id value. The property {key} in the value of {activeProperty} expands to the @id keyword, but its map value contains one or more entries.");

                            case JTokenType.Object:
                                expandedValue = value;
                                break;

                            default:
                                if (frameExpansion)
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                        $"Invalid @id value. The property {key} in the value of {activeProperty} expands to the @id keyword. Its value must be a string, array of strings or an empty object.");
                                }

                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                    $"Invalid @id value. The value of the property {key} in the value of {activeProperty} is not a string, but {key} expands to the keyword @id.");
                        }
                    }

                    // 13.4.4 - If expanded property is @type: 
                    if ("@type" == expandedProperty)
                    {
                        switch (value.Type)
                        {
                            case JTokenType.String:
                                expandedValue = ExpandIri(typeScopedContext, value.Value<string>(), true, true);
                                break;
                            case JTokenType.Array when value.Children().Any(c => c.Type != JTokenType.String):
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue,
                                    $"Invalid @type value. The property {key} in the value of {activeProperty} expands to the @type keyword, but its array value contains one or more non-string items.");
                            case JTokenType.Array:
                                var expandedItems = new JArray();
                                foreach (var item in value.Children())
                                {
                                    expandedItems.Add(ExpandIri(typeScopedContext, item.Value<string>(), true, true));
                                }

                                expandedValue = expandedItems;
                                break;
                            case JTokenType.Object when frameExpansion && !value.HasValues:
                                expandedValue = value;
                                break;
                            case JTokenType.Object when frameExpansion && (value as JObject).ContainsKey("@default"):
                                var toExpand = (value as JObject)["@default"].Value<string>();
                                expandedValue = new JObject(new JProperty("@default"),
                                    ExpandIri(typeScopedContext, toExpand, true, true));
                                break;
                            default:
                                if (frameExpansion)
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue,
                                        $"Invalid @type value. The property {key} in the value of {activeProperty} expands to the @type keyword. Its value must be a string, array of strings, an empty object, or a default object.");
                                }

                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue,
                                    $"Invalid @type value. The property {key} in the value of {activeProperty} expands to the @type keyword. Its value must be a string or an array of strings.");
                        }

                        // 13.4.4.5 - If result already has an entry for @type, prepend the value of @type in result to expanded value, transforming it into an array, if necessary.
                        if (resultObject.ContainsKey("@type"))
                        {
                            expandedValue = ConcatenateValues(resultObject["@type"], expandedValue);
                        }
                    }

                    // 13.4.5 - If expanded property is @graph, set expanded value to the result of using this algorithm recursively passing active context, @graph for active property,
                    // value for element, base URL, and the frameExpansion and ordered flags, ensuring that expanded value is an array of one or more maps.
                    if ("@graph".Equals(expandedProperty))
                    {
                        expandedValue = ExpandAlgorithm(activeContext, "@graph", value, baseUrl, frameExpansion,
                            ordered);
                        if (expandedValue.Type == JTokenType.Object)
                        {
                            expandedValue = new JArray(expandedValue);
                        }
                    }

                    // 13.4.6 - If expanded property is @included: 
                    if ("@included".Equals(expandedProperty))
                    {
                        // 13.4.6.1 - If processing mode is json-ld-1.0, continue with the next key from element.
                        if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10) continue;
                        // 13.4.6.2 - Set expanded value to the result of using this algorithm recursively passing active context, null for active property, value for element, base URL, and the frameExpansion and ordered flags, ensuring that the result is an array.
                        expandedValue = EnsureArray(ExpandAlgorithm(activeContext, null, value, baseUrl, frameExpansion,
                            ordered));
                        // 13.4.6.3 - If any element of expanded value is not a node object, an invalid @included value error has been detected and processing is aborted.
                        if (expandedValue.Children().Any(c => !IsNodeObject(c)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIncludedValue,
                                $"Invalid @included value. The expanded value for the {key} property of {activeProperty} contains one or more values that are not valid node objects.");
                        }

                        if (resultObject.ContainsKey("@included"))
                        {
                            expandedValue = ConcatenateValues(resultObject["@included"], expandedValue);
                        }
                    }

                    // 13.4.7 - If expanded property is @value: 
                    if ("@value".Equals(expandedProperty))
                    {
                        // 13.4.7.1 - If input type is @json, set expanded value to value.
                        // If processing mode is json-ld-1.0, an invalid value object value error has been detected and processing is aborted.
                        if (inputType != null && inputType.Type == JTokenType.String && inputType.Value<string>().Equals("@json"))
                        {
                            if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObjectValue,
                                    $"Invalid value object value. The @value entry of {key} in {activeProperty} must be either scalar or null.");
                            }

                            expandedValue = value;
                        }
                        else if ((!frameExpansion && !(IsScalar(value) || IsNull(value))) ||
                                 (frameExpansion && !(IsScalar(value) || IsNull(value) || IsEmptyMap(value) ||
                                                      IsArray(value, IsScalar))))
                        {
                            // 13.4.7.2 - Otherwise, if value is not a scalar or null, an invalid value object value error has been detected and processing is aborted.
                            // When the frameExpansion flag is set, value MAY be an empty map or an array of scalar values.
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObjectValue,
                                $"Invalid value object value. The @value entry of {key} in {activeProperty} must be either scalar or null.");
                        }
                        else
                        {
                            // 13.4.7.3 - Otherwise, set expanded value to value. When the frameExpansion flag is set, expanded value will be an array of one or more string values or an array containing an empty map.
                            expandedValue = value;
                        }

                        // 13.4.7.4 - If expanded value is null, set the @value entry of result to null and continue with the next key from element.
                        // Null values need to be preserved in this case as the meaning of an @type entry depends on the existence of an @value entry.
                        if (IsNull(expandedValue))
                        {
                            resultObject["@value"] = null;
                            continue;
                        }
                    }

                    // 13.4.8 - If expanded property is @language:
                    if ("@language".Equals(expandedProperty))
                    {
                        // 13.4.8.1 - If value is not a string, an invalid language-tagged string error has been detected and processing is aborted.
                        // When the frameExpansion flag is set, value MAY be an empty map or an array of zero or more strings.
                        if (!frameExpansion && !IsString(value) ||
                            frameExpansion && !(IsString(value) || IsEmptyMap(value) || IsArray(value, IsString)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString,
                                $"Invalid language-tagged string. The expanded property {key} of {activeProperty} expands to an @language property but the value of the {key} is not a string.");
                        }

                        // 13.4.8.2 - Otherwise, set expanded value to value.
                        // TODO: If value is not well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                        // When the frameExpansion flag is set, expanded value will be an array of one or more string values or an array containing an empty map.
                        expandedValue = frameExpansion ? EnsureArray(value) : value;
                        // Processors MAY normalize language tags to lower case.
                        if (IsString(expandedValue))
                        {
                            expandedValue = new JValue(expandedValue.Value<string>().ToLower());
                        }
                    }

                    // 13.4.9 - If expanded property is @direction: 
                    if ("@direction".Equals(expandedProperty))
                    {
                        // 13.4.9.1 - If processing mode is json-ld-1.0, continue with the next key from element.
                        if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                        {
                            continue;
                        }

                        // 13.4.9.2 - If value is neither "ltr" nor "rtl", an invalid base direction error has been detected and processing is aborted.
                        // When the frameExpansion flag is set, value MAY be an empty map or an array of zero or more strings.
                        if (!frameExpansion && !IsValidBaseDirection(value) ||
                            frameExpansion && !(IsValidBaseDirection(value) || IsEmptyMap(value) ||
                                                IsArray(value, IsString)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                                $"Invalid base direction. The property {key} of {activeProperty} expands to an @direction property but the value of {key} is not a valid base direction string.");
                        }

                        // 13.4.9.3 - Otherwise, set expanded value to value.
                        // When the frameExpansion flag is set, expanded value will be an array of one or more string values or an array containing an empty map.
                        expandedValue = value;
                        if (frameExpansion) expandedValue = EnsureArray(expandedValue);
                        if (frameExpansion && !(IsArray(expandedValue, IsString) || IsArray(expandedValue, IsEmptyMap)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                                $"Invalid base direction. During frame expansion, the value of the property {key} of {activeProperty} does not expand to an array of strings or an array containing an empty map.");
                        }
                    }

                    // 13.4.10 - If expanded property is @index:
                    if ("@index".Equals(expandedProperty))
                    {
                        // 13.4.10.1 - If value is not a string, an invalid @index value error has been detected and processing is aborted.
                        if (!IsString(value))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIndexValue,
                                $"Invalid index value. The property {key} of {activeProperty} expands to an @index property but its value is not a string.");
                        }

                        // 13.4.10.2 - Otherwise, set expanded value to value.
                        expandedValue = value;
                    }

                    // 13.4.11 - If expanded property is @list: 
                    if ("@list".Equals(expandedProperty))
                    {
                        // 13.4.11.1 - If active property is null or @graph, continue with the next key from element to remove the free-floating list.
                        if (activeProperty == null || activeProperty.Equals("@graph"))
                        {
                            continue;
                        }

                        // 13.4.11.2 - Otherwise, initialize expanded value to the result of using this algorithm recursively passing active context, active property, value for element,
                        // base URL, and the frameExpansion and ordered flags, ensuring that the result is an array.
                        expandedValue = EnsureArray(
                            ExpandAlgorithm(activeContext, activeProperty, value, baseUrl, frameExpansion, ordered));
                    }

                    // 13.4.12 - If expanded property is @set, set expanded value to the result of using this algorithm recursively, passing active context, active property,
                    // value for element, base URL, and the frameExpansion and ordered flags.
                    if ("@set".Equals(expandedProperty))
                    {
                        expandedValue = ExpandAlgorithm(activeContext, activeProperty, value, baseUrl, frameExpansion,
                            ordered);
                    }

                    // 13.4.13 - If expanded property is @reverse: 
                    if ("@reverse".Equals(expandedProperty))
                    {
                        // 13.4.13.1 - If value is not a map, an invalid @reverse value error has been detected and processing is aborted.
                        if (value.Type != JTokenType.Object)
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseValue,
                                $"Invalid reverse value. The property {key} of {activeProperty} expands to an @reverse property but its value is not a JSON object.");
                        }

                        // 13.4.13.2 - Otherwise initialize expanded value to the result of using this algorithm recursively, passing active context, @reverse as active property, value as element, base URL, and the frameExpansion and ordered flags.
                        expandedValue = ExpandAlgorithm(activeContext, "@reverse", value, baseUrl, frameExpansion,
                            ordered);
                        var expandedValueObject = expandedValue as JObject;
                        if (expandedValueObject != null)
                        {
                            // 13.4.13.3 - If expanded value contains an @reverse entry, i.e., properties that are reversed twice, execute for each of its property and item the following steps: 
                            if (expandedValueObject.ContainsKey("@reverse"))
                            {
                                if (expandedValueObject["@reverse"] is JObject reverseObject)
                                {
                                    foreach (var property in reverseObject.Properties())
                                    {
                                        // 13.4.13.3.1 - Use add value to add item to the property entry in result using true for as array.
                                        AddValue(resultObject, property.Name, property.Value, true);
                                    }
                                }
                            }
                            // 13.4.13.4 - If expanded value contains an entry other than @reverse:
                            if (expandedValueObject.Properties().Any(x=>!x.Name.Equals("@reverse")))
                            {
                                // 13.4.13.4.1 - Set reverse map to the value of the @reverse entry in result, initializing it to an empty map, if necessary.
                                if (!resultObject.ContainsKey("@reverse")) resultObject["@reverse"] = new JObject();
                                var reverseMap = resultObject["@reverse"] as JObject;
                                // 13.4.13.4.2 - For each property and items in expanded value other than @reverse: 
                                foreach (var property in expandedValueObject)
                                {
                                    if (property.Key.Equals("@reverse")) continue;
                                    // 13.4.13.4.2.1 - For each item in items:
                                    // KA: Spec is not quite clear here but appears to indicate that the value of all properties should be an array or that array property values should be iterated over
                                    var items = property.Value is JArray itemsArray
                                        ? (IEnumerable<JToken>) itemsArray.Children()
                                        : new[] {property.Value};
                                    foreach (var item in items)
                                    {
                                        // 13.4.13.4.2.1.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                                        if (IsValueObject(item) || IsListObject(item))
                                        {
                                            throw new JsonLdProcessorException(
                                                JsonLdErrorCode.InvalidReversePropertyValue,
                                                $"Invalid reverse property value. Invalid value found in the expanion of {key} in {activeProperty}.");
                                        }

                                        // 13.4.13.4.2.1.2 - Use add value to add item to the property entry in reverse map using true for as array.
                                        AddValue(reverseMap, property.Key, item, true);
                                    }
                                }
                            }
                        }

                        // 13.4.13.5 - Continue with the next key from element.
                        continue;
                    }

                    // 13.4.14 - If expanded property is @nest, add key to nests, initializing it to an empty array, if necessary. Continue with the next key from element.
                    if ("@nest".Equals(expandedProperty))
                    {
                        if (!nests.ContainsKey(key))
                        {
                            nests.Add(key, new JArray());
                        }

                        continue;
                    }

                    // 13.4.15 - When the frameExpansion flag is set, if expanded property is any other framing keyword (@default, @embed, @explicit, @omitDefault, or @requireAll), set expanded value to the result of performing the Expansion Algorithm recursively, passing active context, active property, value for element, base URL, and the frameExpansion and ordered flags.
                    if (frameExpansion && !JsonLdFramingKeywords.Contains(expandedProperty))
                    {
                        expandedValue = ExpandAlgorithm(activeContext, activeProperty, value, baseUrl, frameExpansion,
                            ordered);
                    }

                    // 13.4.16 - Unless expanded value is null, expanded property is @value, and input type is not @json, set the expanded property entry of result to expanded value.
                    var inputTypeIsJson = inputType != null && inputType.Type == JTokenType.String &&
                                          inputType.Value<string>().Equals("@json");
                    if (!(expandedValue == null && "@value".Equals(expandedProperty) && !inputTypeIsJson))
                    {
                        resultObject[expandedProperty] = expandedValue;
                    }

                    // 13.4.17 - Continue with the next key from element.
                    continue;
                }

                // 13.5 - Initialize container mapping to key's container mapping in active context.
                var termDefinition = activeContext.GetTerm(key);
                var containerMapping = termDefinition?.ContainerMapping ?? new SortedSet<JsonLdContainer>();
                // 13.6 - If key's term definition in active context has a type mapping of @json, set expanded value to a new map, set the entry @value to value, and set the entry @type to @json.
                if ("@json".Equals(termDefinition?.TypeMapping))
                {
                    expandedValue = new JObject(new JProperty("@value", value), new JProperty("@type", "@json"));
                }
                // 13.7 - Otherwise, if container mapping includes @language and value is a map then value is expanded from a language map as follows: 
                else if (containerMapping.Contains(JsonLdContainer.Language) && value.Type == JTokenType.Object)
                {
                    // 13.7.1 - Initialize expanded value to an empty array.
                    var expandedValueArray = new JArray();
                    expandedValue = expandedValueArray;

                    // 13.7.2 - Initialize direction to the default base direction from active context.
                    // 13.7.3 - If key's term definition in active context has a direction mapping, update direction with that value.
                    var direction = termDefinition.DirectionMapping.HasValue
                        ? termDefinition.DirectionMapping
                        : activeContext.BaseDirection;

                    // 13.7.4 - For each key - value pair language-language value in value, ordered lexicographically by language if ordered is true: 
                    var properties = (value as JObject).Properties();
                    if (ordered) properties = properties.OrderBy(prop => prop.Name);
                    foreach (var langaugeMappingProperty in properties)
                    {
                        var language = langaugeMappingProperty.Name;
                        // 13.4.7.1 - If language value is not an array set language value to an array containing only language value.
                        var langaugeValue = EnsureArray(langaugeMappingProperty.Value);
                        // 13.4.7.2 - For each item in language value: 
                        foreach (var item in langaugeValue)
                        {
                            // 13.4.7.2.1 - If item is null, continue to the next entry in language value.
                            if (IsNull(item)) continue;
                            if (!IsString(item))
                            {
                                // 13.4.7.2.2 - item must be a string, otherwise an invalid language map value error has been detected and processing is aborted.
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapValue,
                                    $"Invalid language map value. An invalid langauge map was found for language {language} in the langauge map of the property {key} in {activeProperty}");
                            }

                            // 13.4.7.2.3 - Initialize a new map v consisting of two key-value pairs: (@value-item) and (@language-language).
                            // TODO: If item is neither @none nor well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                            var v = new JObject(new JProperty("@value", item),
                                new JProperty("@language",
                                    language
                                        .ToLowerInvariant())); // Processors MAY normalize language tags to lower case.

                            // 13.4.7.2.4 - If language is @none, or expands to @none, remove @language from v.
                            if (language.Equals("@none") || activeContext.GetAliases("@none").Contains(language))
                            {
                                v.Remove("@language");
                            }

                            // 13.4.7.2.5 -  If direction is not null, add an entry for @direction to v with direction.
                            if (direction.HasValue && direction != LanguageDirection.Unspecified)
                            {
                                v["@direction"] = SerializeLanguageDirection(direction.Value);
                            }

                            expandedValueArray.Add(v);
                        }
                    }
                }
                // 13.8 - Otherwise, if container mapping includes @index, @type, or @id and value is a map then value is expanded from an map as follows: 
                else if (value.Type == JTokenType.Object && 
                         (containerMapping.Contains(JsonLdContainer.Index) ||
                          containerMapping.Contains(JsonLdContainer.Type) ||
                          containerMapping.Contains(JsonLdContainer.Id)))
                {
                    // 13.8.1 Initialize expanded value to an empty array.
                    var expandedValueArray = new JArray();
                    expandedValue = expandedValueArray;

                    // 13.8.2 - Initialize index key to the key's index mapping in active context, or @index, if it does not exist.
                    var indexKey = termDefinition.IndexMapping ?? "@index";

                    // 13.8.3 - For each key - value pair index-index value in value, ordered lexicographically by index if ordered is true: 
                    var properties = (value as JObject).Properties();
                    if (ordered) properties = properties.OrderBy(prop => prop.Name);
                    foreach (var indexValueProperty in properties)
                    {
                        var index = indexValueProperty.Name;
                        var indexValue = EnsureArray(indexValueProperty.Value);

                        JsonLdContext mapContext = null;
                        // 13.8.3.1 - If container mapping includes @id or @type, initialize map context to the previous context from active context if it exists, otherwise, set map context to active context.
                        if (containerMapping.Contains(JsonLdContainer.Id) ||
                            containerMapping.Contains(JsonLdContainer.Type))
                        {
                            mapContext = activeContext.PreviousContext ?? activeContext;
                            // 13.8.3.2 - If container mapping includes @type and index's term definition in map context has a local context, update map context to the result of the Context Processing algorithm, passing map context as active context the value of the index's local context as local context and base URL from the term definition for index in map context.
                            if (containerMapping.Contains(JsonLdContainer.Type))
                            {
                                var indexTermDefinition = mapContext.GetTerm(index);
                                if (indexTermDefinition != null && indexTermDefinition.LocalContext != null)
                                {
                                    mapContext = ProcessContext(mapContext, indexTermDefinition.LocalContext,
                                        indexTermDefinition.BaseUrl);
                                }
                            }
                        }
                        else
                        {
                            // 13.8.3.3 - Otherwise, set map context to active context.
                            mapContext = activeContext;
                        }

                        // 13.8.3.4 - Initialize expanded index to the result of IRI expanding index.
                        var expandedIndex = ExpandIri(activeContext, index, true);

                        // 13.8.3.5 - If index value is not an array set index value to an array containing only index value.
                        // Already done at initialization

                        // 13.8.3.6 - Initialize index value to the result of using this algorithm recursively, passing map context as active context, key as active property, index value as element, base URL, true for from map, and the frameExpansion and ordered flags.
                        indexValue = EnsureArray(ExpandAlgorithm(mapContext, key, indexValue, baseUrl, frameExpansion,
                            ordered, true));

                        // 13.8.3.7 - For each item in index value: 
                        for (var ix = 0; ix < indexValue.Count; ix++)
                        {
                            var item = indexValue[ix] as JObject;
                            // 13.8.7.3.1 - If container mapping includes @graph, and item is not a graph object, set item to a new map containing the key - value pair @graph-item, ensuring that the value is represented using an array.
                            if (containerMapping.Contains(JsonLdContainer.Graph) && !IsGraphObject(item))
                            {
                                item = new JObject(new JProperty("@graph", EnsureArray(item)));
                            }

                            // 13.8.3.7.2 - If container mapping includes @index, index key is not @index, and expanded index is not @none: 
                            if (containerMapping.Contains(JsonLdContainer.Index) && !indexKey.Equals("@index") &&
                                !expandedIndex.Equals("@none"))
                            {
                                // 13.8.7.2.1 - Initialize re-expanded index to the result of calling the Value Expansion algorithm, passing the active context, index key as active property, and index as value.
                                var reExpandedIndex = ExpandValue(activeContext, indexKey, index);

                                // 13.8.7.2.2 - Initialize expanded index key to the result of IRI expanding index key.
                                var expandedIndexKey = ExpandIri(activeContext, indexKey, true);

                                // 13.8.7.2.3 - Initialize index property values to an array consisting of re-expanded index followed by the existing values of the concatenation of expanded index key in item, if any.
                                var indexPropertyValues = new JArray(reExpandedIndex);
                                if (item.ContainsKey(expandedIndexKey))
                                {
                                    indexPropertyValues =
                                        ConcatenateValues(indexPropertyValues, item[expandedIndexKey]);
                                }

                                // 13.8.7.2.4 - Add the key - value pair(expanded index key - index property values) to item.
                                item.Remove(expandedIndexKey); // Overwriting any existing value (which should have been appended to indexPropertyValues
                                item.Add(new JProperty(expandedIndexKey, indexPropertyValues));

                                // 13.8.7.2.5 - If item is a value object, it MUST NOT contain any extra properties; an invalid value object error has been detected and processing is aborted.
                                if (item.ContainsKey("@value") && item.Count > 1)
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                                        $"Invalid value object. Encountered an value object with properties other than @value while expanding the entry {key} of {activeProperty}.");
                                }
                            }
                            // 13.8.7.3 - Otherwise, if container mapping includes @index, item does not have an entry @index, and expanded index is not @none, add the key-value pair (@index-index) to item.
                            else if (containerMapping.Contains(JsonLdContainer.Index) && !item.ContainsKey("@index") &&
                                     !expandedIndex.Equals("@none"))
                            {
                                item.Add("@index", index);
                            }
                            // 13.8.7.4 - Otherwise, if container mapping includes @id item does not have the entry @id, and expanded index is not @none, add the key - value pair(@id - expanded index) to item, where expanded index is set to the result of IRI expandingindex using true for document relative and false for vocab.
                            else if (containerMapping.Contains(JsonLdContainer.Id) && !item.ContainsKey("@id") &&
                                     !expandedIndex.Equals("@none"))
                            {
                                expandedIndex = ExpandIri(activeContext, index, false, true);
                                item.Add("@id", expandedIndex);
                            }
                            // 13.8.7.5 - Otherwise, if container mapping includes @type and expanded index is not @none, initialize types to a new array consisting of expanded index followed by any existing values of @type in item.Add the key - value pair(@type - types) to item.
                            else if (containerMapping.Contains(JsonLdContainer.Type) && !expandedIndex.Equals("@none"))
                            {
                                var types = new JArray(expandedIndex);
                                if (item.ContainsKey("@type"))
                                {
                                    types = ConcatenateValues(types, item["@type"]);
                                }

                                item["@type"] = types;
                            }

                            // 13.8.7.6 - Append item to expanded value.
                            expandedValueArray.Add(item);
                        }
                    }
                }
                // 13.9 - Otherwise, initialize expanded value to the result of using this algorithm recursively, passing active context, key for active property, value for element, base URL, and the frameExpansion and ordered flags.
                else
                {
                    expandedValue = ExpandAlgorithm(activeContext, key, value, baseUrl, frameExpansion, ordered);
                }

                // 13.10 - If expanded value is null, ignore key by continuing to the next key from element.
                if (expandedValue == null) continue;
                // 13.11 If container mapping includes @list and expanded value is not already a list object, convert expanded value to a list object by first setting it to an array containing only expanded value if it is not already an array, and then by setting it to a map containing the key-value pair @list-expanded value.
                if (containerMapping.Contains(JsonLdContainer.List) && !IsListObject(expandedValue))
                {
                    expandedValue = new JObject(new JProperty("@list", EnsureArray(expandedValue)));
                }

                // 13.12 - If container mapping includes @graph, and includes neither @id nor @index, convert expanded value into an array, if necessary,
                // then convert each value ev in expanded value into a graph object:
                if (containerMapping.Contains(JsonLdContainer.Graph) &&
                    !containerMapping.Contains(JsonLdContainer.Id) && !containerMapping.Contains(JsonLdContainer.Index))
                {
                    var tmp = EnsureArray(expandedValue);
                    var expandedValueArray = new JArray();
                    foreach (var ev in tmp)
                    {
                        // 13.12.1 - Convert ev into a graph object by creating a map containing the key-value pair @graph-ev where ev is represented as an array.
                        // Note: This may lead to a graph object including another graph object, if ev was already in the form of a graph object.
                        expandedValueArray.Add(new JObject(new JProperty("@graph", EnsureArray(ev))));
                    }

                    expandedValue = expandedValueArray;
                }

                // 13.13 - If the term definition associated to key indicates that it is a reverse property
                if (termDefinition != null && termDefinition.Reverse)
                {
                    // 13.13.1 - If result has no @reverse entry, create one and initialize its value to an empty map.
                    // 13.13.2 - Reference the value of the @reverse entry in result using the variable reverse map.
                    if (!resultObject.ContainsKey("@reverse"))
                    {
                        resultObject.Add(new JProperty("@reverse", new JObject()));
                    }
                    var reverseMap = resultObject["@reverse"] as JObject;
                    // 13.13.3 - If expanded value is not an array, set it to an array containing expanded value.
                    expandedValue = EnsureArray(expandedValue);
                    // 13.13.4 - For each item in expanded value
                    foreach (var item in expandedValue.Children())
                    {
                        // 13.13.4.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                        if (IsValueObject(item) || IsListObject(item))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReversePropertyValue,
                                $"Invalid reverse property value. Invalid value found when expanding the property {key} of {activeProperty} - {key} is a reverse property but includes a value or list object amongst its value(s).");
                        }

                        // 13.13.4.2 - If reverse map has no expanded property entry, create one and initialize its value to an empty array.
                        if (!reverseMap.ContainsKey(expandedProperty))
                        {
                            reverseMap[expandedProperty] = new JArray();
                        }

                        // 13.13.4.3 - Use add value to add item to the expanded property entry in reverse map using true for as array.
                        AddValue(reverseMap, expandedProperty, item, true);
                    }
                }
                else
                {
                    AddValue(resultObject, expandedProperty, expandedValue, true);
                }
            }

            // 14 - For each key nesting-key in nests, ordered lexicographically if ordered is true: 
            var nestProperties = nests.Properties();
            if (ordered) nestProperties = nestProperties.OrderBy(p => p.Name);
            foreach (var nestingProperty in nestProperties)
            {
                var nestingKey = nestingProperty.Name;
                // 14.1 - Initialize nested values to the value of nesting-key in element, ensuring that it is an array.
                var nestedValues = EnsureArray(elementObject[nestingKey]);
                var expandedNestedValues = new JArray();
                // 14.2 - For each nested value in nested values: 
                foreach (var nestedValue in nestedValues)
                {
                    // 14.2.1 - If nested value is not a map, or any key within nested value expands to @value, an invalid @nest value error has been detected and processing is aborted.
                    var nestedValueObject = nestedValue as JObject;
                    if (nestedValueObject == null)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                            $"Invalid nest value. Invalid value found when expanding the nesting property {nestingKey} of {activeProperty}. The nested value is not a JSON object.");
                    }

                    if (nestedValueObject.Properties()
                        .Any(p => ExpandIri(activeContext, p.Name, true).Equals("@value")))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                            $"Invalid nest value. Invalid value found when expanding the nesting property {nestingKey} of {activeProperty}. The nested value contains a property which is, or which expands to @value.");
                    }

                    // 14.2.2 - Recursively repeat steps 13 and 14 using nested value for element. 
                    ExpandElement(resultObject, inputType, activeContext, activeProperty, baseUrl, frameExpansion, ordered, nestedValueObject, typeScopedContext);
                }

                nestingProperty.Value = expandedNestedValues;
            }
        }

        private static void AddValue(JObject o, string entry, JToken value, bool asArray = false)
        {
            if (asArray)
            {
                if (!o.ContainsKey(entry))
                {
                    o[entry] = new JArray();
                }
                else
                {
                    o[entry] = EnsureArray(o[entry]);
                }
            }

            if (value is JArray valueArray)
            {
                foreach (var item in valueArray)
                {
                    AddValue(o, entry, item, asArray);
                }
            }
            else
            {
                if (!o.ContainsKey(entry))
                {
                    o[entry] = value;
                }
                else
                {
                    if (o[entry] is JArray entryArray)
                    {
                        entryArray.Add(value);
                    }
                    else
                    {
                        entryArray = new JArray(o[entry]);
                        entryArray.Add(value);
                        o[entry] = entryArray;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new array which is a concatenation of the values of token1 and token2.
        /// </summary>
        /// <param name="token1"></param>
        /// <param name="token2"></param>
        /// <remarks>This method flattens any input arrays.</remarks>
        /// <returns></returns>
        private JArray ConcatenateValues(JToken token1, JToken token2)
        {
            var result = EnsureArray(token1);
            if (token2 is JArray)
            {
                foreach(var c in token2.Children()) result.Add(c);
            }
            else
            {
                result.Add(token2);
            }

            return result;
        }

        /// <summary>
        /// Ensure that <paramref name="token"/> is wrapped in an array unless it already is an array.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static JArray EnsureArray(JToken token)
        {
            if (token is JArray array) return array;
            return new JArray(token);
        }

        /*
        private void ExpandKeys(JsonLdContext activeContext, string activeProperty, JObject elementObject, JObject result, bool frameExpansion)
        {
            JArray nests = new JArray();
            // 8 - For each key and value in element, ordered lexicographically by key:
            foreach (var property in elementObject.Properties().OrderBy(p => p.Name))
            {
                var key = property.Name;
                var value = property.Value;
                JToken expandedValue = null;

                // 8.1 - If key is @context, continue to the next key.
                if (key.Equals("@context"))
                {
                    continue;
                }

                // 8.2 - Set expanded property to the result of using the IRI Expansion algorithm, passing active context, key for value, and true for vocab.
                var expandedProperty = ExpandIri(activeContext, key, true);

                // 8.3 - If expanded property is null or it neither contains a colon (:) nor it is a keyword, drop key by continuing to the next key.
                if (expandedProperty == null ||
                    !(expandedProperty.Contains(':') || IsKeyword(expandedProperty)))
                {
                    continue;
                }

                // 8.4 - If expanded property is a keyword:
                if (IsKeyword(expandedProperty))
                {
                    // 8.4.1 - If active property equals @reverse, an invalid reverse property map error has been detected and processing is aborted.
                    if ("@reverse".Equals(activeProperty))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReversePropertyMap, "Reverse property map cannot contain a keyword");
                    }

                    // 8.4.2 - If result has already an expanded property member, an colliding keywords error has been detected and processing is aborted.
                    if (result.Property(expandedProperty) != null)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.CollidingKeywords, $"Colliding Keywords: {expandedProperty}");
                    }

                    // 8.4.3 - If expanded property is @id and value is not a string, 
                    // an invalid @id value error has been detected and processing is aborted.
                    // Otherwise, set expanded value to the result of using the IRI Expansion algorithm, 
                    // passing active context, value, and true for document relative. 
                    // When the frame expansion flag is set, value may be an empty dictionary, 
                    // or an array of one or more strings. Expanded value will be an array of one 
                    // or more of these, with string values expanded using the IRI Expansion Algorithm.

                    if (expandedProperty.Equals("@id"))
                    {
                        switch (value.Type)
                        {
                            case JTokenType.Object:
                                if (frameExpansion && !(value as JObject).Properties().Any())
                                {
                                    expandedValue = new JArray(new JObject());
                                }
                                else
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                        "Invalid @id value");
                                }
                                break;
                            case JTokenType.Array:
                                if (frameExpansion)
                                {
                                    expandedValue = new JArray();
                                    foreach (var item in (value as JArray))
                                    {
                                        if (item.Type != JTokenType.String)
                                        {
                                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                                "Invalid @id value");
                                        }
                                        (expandedValue as JArray).Add(ExpandIri(activeContext, item.Value<string>(),
                                            documentRelative: true));
                                    }
                                }
                                else
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                        "Invalid @id value");
                                }
                                break;
                            case JTokenType.String:
                                if (frameExpansion)
                                {
                                    expandedValue = new JArray(ExpandIri(activeContext, value.Value<string>(),
                                        documentRelative: true));
                                }
                                else
                                {
                                    expandedValue = ExpandIri(activeContext, value.Value<string>(),
                                        documentRelative: true);
                                }
                                break;
                            default:
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                    "Invalid @id value");
                        }
                    }

                    // 8.4.4 - If expanded property is @type
                    if (expandedProperty.Equals("@type"))
                    {
                        if (value.Type == JTokenType.String)
                        {
                            expandedValue = ExpandIri(activeContext, value.Value<string>(), true, true);
                        }
                        else if (value.Type == JTokenType.Array)
                        {
                            var array = new JArray();
                            foreach (var item in (value as JArray))
                            {
                                if (item.Type != JTokenType.String)
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue, "The value of the @type property must be a string or an array of strings");
                                }
                                array.Add(ExpandIri(activeContext, item.Value<string>(), true, true));
                            }
                            expandedValue = array;
                        }
                        // When the frame expansion flag is set, value may also be an empty dictionary.
                        else if (value.Type == JTokenType.Object && frameExpansion &&
                                   !((value as JObject).Properties().Any()))
                        {
                            expandedValue = new JObject();
                        }
                        else
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue, "The value of the @type property must be a string or an array of strings");
                        }
                    }

                    // 8.4.5 - If expanded property is @graph,...
                    if (expandedProperty.Equals("@graph"))
                    {
                        // set expanded value to the result of using this algorithm recursively 
                        // passing active context, @graph for active property, and value for 
                        // element, ensuring that expanded value is an array of one or more dictionaries.
                        expandedValue = Expand(activeContext, "@graph", value);
                        // NOTE: The following lines are supposed to ensure the array contains at least one dictionary,
                        // but it causes a failure in the JSON-LD.org test suite on the test specifically for expanding an empty graph
                        // var array = expandedValue as JArray;
                        //if (array.Count == 0) { array.Add(new JObject());  }
                    }

                    // 8.4.6 - If expanded property is @value and value is not a scalar or null, an invalid value object value error has been detected and processing is aborted. Otherwise, set expanded value to value. 
                    // If expanded value is null, set the @value member of result to null and continue with the next key from element. 
                    // Null values need to be preserved in this case as the meaning of an @type member depends on the existence of an @value member. When the frame expansion flag is set, value may also be an empty dictionary or an array of scalar values. Expanded value will be null, or an array of one or more scalar values.
                    // When the frame expansion flag is set, value may also be an empty dictionary or an array of scalar values. 
                    // Expanded value will be null, or an array of one or more scalar values.
                    if (expandedProperty.Equals("@value"))
                    {
                        if (!(value.Type == JTokenType.Null || IsScalar(value) ||
                              (frameExpansion && value.Type == JTokenType.Array) ||
                              (frameExpansion && value.Type == JTokenType.Object)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                                "The expanded value of @value must be a scalar or null.");
                        }
                        expandedValue = value;
                        if (expandedValue.Type == JTokenType.Object)
                        {
                            if ((expandedValue as JObject).Properties().Any())
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                                    $"Expected an empty object when expanding @value at {value.Path}.");
                            }
                        }
                        else if (expandedValue.Type == JTokenType.Array)
                        {
                            if ((expandedValue as JArray).Any(item => !(item.Type == JTokenType.Null || IsScalar(item)))
                            )
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                                    $"Expected array of scalar values when expanding @value at {value.Path}");
                            }
                        }
                        if (expandedValue == null || expandedValue.Type == JTokenType.Null)
                        {
                            result["@value"] = null;
                            continue;
                        }
                    }

                    // 8.4.7 - If expanded property is @language and value is not a string, an invalid language-tagged string error has been detected and processing is aborted. Otherwise, set expanded value to lowercased value. When the frame expansion flag is set, value may also be an empty dictionary or an array of zero or strings. Expanded value will be an array of one or more string values converted to lower case.
                    if (expandedProperty.Equals("@language"))
                    {
                        if (value.Type == JTokenType.String)
                        {
                            expandedValue = value.Value<string>().ToLowerInvariant();
                        }
                        // When the frame expansion flag is set, value may also be an empty dictionary or an array of zero or strings. Expanded value will be an array of one or more string values converted to lower case.
                        else if (value.Type == JTokenType.Array && frameExpansion)
                        {
                            expandedValue = new JArray();
                            foreach (var item in value as JArray)
                            {
                                if (item.Type != JTokenType.String)
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString, $"Invalid value for @language property in {activeProperty}. Expected a JSON string, got {item.Type}.");
                                }
                                (expandedValue as JArray).Add(item.Value<string>().ToLowerInvariant());
                            }
                        }
                        else if (value.Type == JTokenType.Object && frameExpansion)
                        {
                            if ((value as JObject).Properties().Any())
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString, $"Invalid value for @language property in {activeProperty}. Expected an object value to have no properties.");
                            }
                            expandedValue = value;
                        }
                        else
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString, $"Invalid value for @language property in {activeProperty}. Expected a JSON string, got {value.Type}.");
                        }
                    }

                    // 8.4.8 - If expanded property is @index and value is not a string, an invalid @index value error has been detected and processing is aborted. Otherwise, set expanded value to value.
                    if (expandedProperty.Equals("@index"))
                    {
                        if (value.Type == JTokenType.String)
                        {
                            expandedValue = value;
                        }
                        else
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIndexValue, $"Invalid @index value. Expected a JSON string, got {value.Type}");
                        }
                    }

                    // 8.4.9 - If expanded property is @list:
                    if (expandedProperty.Equals("@list"))
                    {
                        // 8.4.9.1 - If active property is null or @graph, continue with the next key from element to remove the free-floating list.
                        if (activeProperty == null || activeProperty.Equals("@graph"))
                        {
                            continue;
                        }

                        // 8.4.9.2 - Otherwise, initialize expanded value to the result of using this algorithm recursively passing active context, active property, and value for element.
                        expandedValue = Expand(activeContext, activeProperty, value);

                        // 8.4.9.3 - If expanded value is a list object, a list of lists error has been detected and processing is aborted.
                        if (IsListObject(expandedValue))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.ListOfLists, "The expanded value of an @list property must not be a list object");
                        }
                    }

                    // 8.4.10 - If expanded property is @set, set expanded value to the result of using this algorithm recursively, passing active context, active property, and value for element.
                    if (expandedProperty.Equals("@set"))
                    {
                        expandedValue = ExpandAlgorithm(activeContext, activeProperty, value);
                    }

                    // 8.4.11 - If expanded property is @reverse and value is not a JSON object, an invalid @reverse value error has been detected and processing is aborted.
                    if (expandedProperty.Equals("@reverse"))
                    {
                        if (value.Type != JTokenType.Object)
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseValue, $"The value of an @reverse property must be a JSON object. Found {value.Type}.");
                        }

                        // 8.4.11.1 - Initialize expanded value to the result of using this algorithm recursively, passing active context, @reverse as active property, and value as element.
                        expandedValue = ExpandAlgorithm(activeContext, "@reverse", value);
                        
                        // 8.4.11.2 - If expanded value contains an @reverse member, i.e., properties that are reversed twice...
                        var nestedReverseProperty = (expandedValue as JObject).Property("@reverse");
                        if (nestedReverseProperty != null)
                        {
                            // ... execute for each of its property and item the following steps:
                            foreach (var nestedProperty in (nestedReverseProperty.Value as JObject).Properties())
                            {
                                // 8.4.11.2.1 - If result does not have a property member, create one and set its value to an empty array.
                                if (result.Property(nestedProperty.Name) == null)
                                {
                                    result.Add(new JProperty(nestedProperty.Name, new JArray()));
                                }
                                // 8.4.11.2.2 - Append item to the value of the property member of result.
                                var resultArray = result.Property(nestedProperty.Name).Value as JArray;
                                if (nestedProperty.Value is JArray)
                                {
                                    foreach (var item in (nestedProperty.Value as JArray))
                                    {
                                        resultArray.Add(item);
                                    }
                                }
                                else
                                {
                                    resultArray.Add(nestedProperty.Value);
                                }
                            }
                        }

                        // 8.4.11.3 - If expanded value contains members other than @reverse:
                        if ((expandedValue as JObject).Properties().Any(x => !x.Name.Equals("@reverse")))
                        {
                            // 8.4.11.3.1 - If result does not have an @reverse member, create one and set its value to an empty JSON object.
                            if (result.Property("@reverse") == null)
                            {
                                result.Add(new JProperty("@reverse", new JObject()));
                            }

                            // 8.4.11.3.2 - Reference the value of the @reverse member in result using the variable reverse map.
                            var reverseMap = result.Property("@reverse").Value as JObject;

                            // 8.4.11.3.3 - For each property and items in expanded value other than @reverse:
                            foreach (var entry in (expandedValue as JObject).Properties())
                            {
                                // 8.4.11.3.3.1 - For each item in items:
                                var items = entry.Value as JArray;
                                foreach (var item in items)
                                {
                                    // 8.4.11.3.3.1.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                                    if (IsValueObject(item) || IsListObject(item))
                                    {
                                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReversePropertyValue,
                                            "Reverse property value must not be a value object or list object.");
                                    }
                                    // 8.4.11.3.3.1.2 - If reverse map has no property member, create one and initialize its value to an empty array.
                                    if (reverseMap.Property(entry.Name) == null)
                                    {

                                        reverseMap[entry.Name] = new JArray();
                                    }
                                    // 8.4.11.3.3.1.3 - ppend item to the value of the property member in reverse map.
                                    (reverseMap[entry.Name] as JArray).Add(item);
                                }
                            }
                        }
                        // 8.4.11.4 - Continue with the next key from element.
                        continue;
                    }
    
                    // 8.4.12 - If expanded property is @nest, add key to nests, initializing it to an empty array, if necessary. Continue with the next key from element.
                    if (expandedProperty.Equals("@nest"))
                    {
                        if (nests == null)
                        {
                            nests = new JArray();
                        }
                        nests.Add(key);
                        continue;
                    }

                    // 8.4.13 - When the frame expansion flag is set, if expanded property is any other framing keyword (@explicit, @default, @embed, @explicit, @omitDefault, or @requireAll), set expanded value to the result of performing the Expansion Algorithm recursively, passing active context, active property, and value for element.
                    if (frameExpansion && JsonLdFramingKeywords.Contains(expandedProperty))
                    {
                        expandedValue = ExpandAlgorithm(activeContext, expandedProperty, value);
                    }
                    // 8.4.14 - Unless expanded value is null, set the expanded property member of result to expanded value.
                    if (expandedValue != null)
                    {
                        result.Add(new JProperty(expandedProperty, expandedValue));
                    }

                    // 8.4.15 - Continue with the next key from element.
                    continue;
                }

                // 8.5 - If key's term definition in active context has a local context, set term context to the result of the Context Processing algorithm, passing active context and the value of the key's local context as local context. Otherwise, set term context to active context.
                var termDefinition = activeContext.GetTerm(key);
                JsonLdContext termContext;
                if (termDefinition?.LocalContext != null)
                {
                    termContext = ProcessContext(activeContext, termDefinition.LocalContext);
                }
                else
                {
                    termContext = activeContext;
                }

                // 8.6 - If key's container mapping in term context is @language and value is a JSON object then value is expanded from a language map as follows:
                termDefinition = termContext.GetTerm(key);
                if (termDefinition?.ContainerMapping == JsonLdContainer.Language && value is JObject)
                {
                    // 8.6.1 - Initialize expanded value to an empty array.
                    expandedValue = new JArray();
                    // 8.6.2 - For each key-value pair language-language value in value, ordered lexicographically by language:
                    foreach (var p in (value as JObject).Properties().OrderBy(p => p.Name))
                    {
                        var language = p.Name;
                        // 8.6.2.1 - If language value is not an array set it to an array containing only language value.
                        var languageValue = p.Value.Type == JTokenType.Array ? p.Value as JArray : new JArray(p.Value);

                        // 8.6.2.2 - For each item in language value:
                        foreach (var item in languageValue)
                        {
                            // 8.6.2.2.1 - item must be a string or null, otherwise an invalid language map value error has been detected and processing is aborted.
                            if (!(item.Type == JTokenType.Null || item.Type == JTokenType.String))
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapValue, $"Invalid value for language map at {activeProperty}. Expected null or JSON string, got {item.Type}");
                            }
                            // 8.6.2.2.2 - Append a JSON object to expanded value that consists of two key-value pairs: (@value-item) and (@language-lowercased language), unless item is null.
                            if (item.Type != JTokenType.Null)
                            {
                                (expandedValue as JArray).Add(new JObject(
                                    new JProperty("@value", item),
                                    new JProperty("@language", language.ToLowerInvariant())));
                            }
                        }
                    }
                }
                // 8.7 - Otherwise, if key's container mapping in term context is @index, @type, or @id and value is a JSON object then value is expanded from an map as follows:                
                else if ((termDefinition?.ContainerMapping == JsonLdContainer.Index ||
                    termDefinition?.ContainerMapping == JsonLdContainer.Type ||
                    termDefinition?.ContainerMapping == JsonLdContainer.Id) &&
                    value is JObject)
                {
                    // 8.7.1 - Initialize expanded value to an empty array.
                    expandedValue = new JArray();
                    // 8.7.2 - For each key-value pair index-index value in value, ordered lexicographically by index:
                    foreach (var p in (value as JObject).Properties().OrderBy(p => p.Name))
                    {
                        var index = p.Name;
                        var indexValue = p.Value;
                        var indexTermDefinition = termContext.GetTerm(index);
                        // 8.7.2.1 - If container mapping is @type, and index's term definition in term context has a local context, set map context to the result of the Context Processing algorithm, passing term context as active context and the value of the index's local context as local context. Otherwise, set map context to term context.
                        var mapContext = 
                            (termDefinition.ContainerMapping == JsonLdContainer.Type && 
                             indexTermDefinition?.LocalContext != null) ? 
                             ProcessContext(activeContext, indexTermDefinition.LocalContext) : 
                             termContext;
                        // 8.7.2.2 - If index value is not an array set it to an array containing only index value.
                        if (!(indexValue is JArray)) indexValue = new JArray(indexValue);
                        // 8.7.2.3 - Initialize index value to the result of using this algorithm recursively, passing map context as active context, key as active property, and index value as element.
                        indexValue = Expand(mapContext, key, indexValue);
                        // 8.7.2.4 - For each item in index value:
                        foreach (var i in indexValue)
                        {
                            // 8.7.2.4.1 - If container mapping is @index and item does not have the key @index, add the key-value pair (@index-index) to item.
                            var item = i as JObject;
                            if (termDefinition.ContainerMapping == JsonLdContainer.Index && item.Property("@index") == null)
                            {
                                item.Add(new JProperty("@index", index));
                            }
                            // 8.7.2.4.2 - Otherwise, if container mapping is @id and item does not have the key @id, add the key-value pair (@id-expanded index) to item, where expanded index is set to the result of using the IRI Expansion algorithm, passing active context, index, and true for document relative.
                            else if (termDefinition.ContainerMapping == JsonLdContainer.Id && item.Property("@id") == null)
                            {
                                var expandedIndex = ExpandIri(activeContext, index, documentRelative: true);
                                item.Add(new JProperty("@id", expandedIndex));
                            }
                            // 8.7.2.4.3 - Otherwise, if container mapping is @type set types to the concatenation of expanded index with any existing values of @type in item, where expanded index is set to the result of using the IRI Expansion algorithm, passing active context, index, true for vocab, and true for document relative and add the key - value pair(@type - types) to item.
                            else if (termDefinition.ContainerMapping == JsonLdContainer.Type)
                            {
                                var expandedIndex = ExpandIri(activeContext, index, true, true);
                                var existingTypes = item.GetValue("@type") as JArray;
                                if (existingTypes != null)
                                {
                                    existingTypes.Insert(0, expandedIndex);
                                }
                                else
                                {
                                    item.Add(new JProperty("@type", new JArray(expandedIndex)));
                                }
                            }
                            // 8.7.2.4.4 - Append item to expanded value.
                            (expandedValue as JArray).Add(item);
                        }
                    }
                }
                // 8.8 - Otherwise, initialize expanded value to the result of using this algorithm recursively, passing term context as active context, key for active property, and value for element.
                else
                {
                    expandedValue = ExpandAlgorithm(termContext, key, value);
                }
                // 8.9 - If expanded value is null, ignore key by continuing to the next key from element.
                if (expandedValue == null)
                {
                    continue;
                }
                // 8.10 - If the container mapping associated to key in term context is @list and expanded value is not already a list object, convert expanded value to a list object by first setting it to an array containing only expanded value if it is not already an array, and then by setting it to a JSON object containing the key-value pair @list-expanded value.
                if (termDefinition?.ContainerMapping == JsonLdContainer.List && !IsListObject(value))
                {
                    expandedValue = expandedValue is JArray ? expandedValue : new JArray(expandedValue);
                    expandedValue = new JObject(new JProperty("@list", expandedValue));
                }
                // 8.11 - Otherwise, if the term definition associated to key indicates that it is a reverse property
                // NOTE: Although the spec says "Otherwise" here, making this an else if means that the expandedValue set above would never get added to the result object (and this results in some test suite failures), so I think the spec is in error here
                if (termDefinition != null &&  termDefinition.Reverse)
                {
                    // 8.11.1 - If result has no @reverse member, create one and initialize its value to an empty JSON object.
                    if (result.Property("@reverse") == null)
                    {
                        result.Add(new JProperty("@reverse", new JObject()));
                    }
                    // 8.11.2 - Reference the value of the @reverse member in result using the variable reverse map.
                    var reverseMap = result.Property("@reverse").Value as JObject;
                    // 8.11.3 - If expanded value is not an array, set it to an array containing expanded value.
                    expandedValue = (expandedValue is JArray) ? expandedValue : new JArray(expandedValue);
                    // 8.11.4 - For each item in expanded value
                    foreach (var item in expandedValue)
                    {
                        // 8.11.4.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                        if (IsValueObject(item) || IsListObject(item))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReversePropertyValue, "Reverse property value must not be a value object or list object.");
                        }
                        // 8.11.4.2 - If reverse map has no expanded property member, create one and initialize its value to an empty array.
                        if (reverseMap.Property(expandedProperty) == null)
                        {
                            reverseMap.Add(new JProperty(expandedProperty, new JArray()));
                        }
                        (reverseMap[expandedProperty] as JArray).Add(item);
                    }
                }
                // 8.12 - Otherwise, if key is not a reverse property:
                else if (termDefinition == null || !termDefinition.Reverse)
                {
                    // 8.12.1 - If result does not have an expanded property member, create one and initialize its value to an empty array.
                    if (result.Property(expandedProperty) == null)
                    {
                        result.Add(new JProperty(expandedProperty, new JArray()));
                    }
                    // 8.12.2 - Append expanded value to value of the expanded property member of result.
                    if (expandedValue is JArray)
                    {
                        foreach (var item in (expandedValue as JArray))
                        {
                            (result[expandedProperty] as JArray).Add(item);
                        }
                    }
                    else
                    {
                        (result[expandedProperty] as JArray).Add(expandedValue);
                    }
                }
            }
            // 8.13 - For each key nesting-key in nests
            foreach (var nestingKey in nests)
            {
                // 8.13.1 - Set nested values to the value of nesting-key in element, ensuring that it is an array.
                var nestedValues = elementObject.GetValue(nestingKey.Value<string>());
                if (!(nestedValues is JArray))
                {
                    nestedValues = new JArray(nestedValues);
                }

                // 8.13.2 - For each nested value in nested values:
                foreach (var nestedValue in nestedValues)
                {
                    // If nested value is not a JSON object, or any key within nested value expands to @value, an invalid @nest value error has been detected and processing is aborted.
                    if (!(nestedValue is JObject))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue, "Nested value must be a JSON object");
                    }
                    if ((nestedValue as JObject).Properties().Any(p => ExpandIri(activeContext, p.Name, true).Equals("@value")))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue, "Nested values may not contain keys that expand to @value");
                    }
                    // 8.13.2.2 - Recursively repeat step 7 using nested value for element.
                    ExpandKeys(activeContext, activeProperty, nestedValue as JObject, result, frameExpansion);
                }
            }
        }
*/

        /// <summary>
        /// Implementation of the Value Expansion algorithm
        /// </summary>
        /// <param name="activeContext"></param>
        /// <param name="activeProperty"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private JToken ExpandValue(JsonLdContext activeContext, string activeProperty, JToken value)
        {
            activeContext.TryGetTerm(activeProperty, out var activePropertyTermDefinition, true);
            var typeMapping = activePropertyTermDefinition?.TypeMapping;

            // 1 - If the active property has a type mapping in active context that is @id, and the value is a string, return a new map containing a single entry
            // where the key is @id and the value is the result IRI expanding value using true for document relative and false for vocab.
            if (typeMapping != null && typeMapping == "@id" && value.Type == JTokenType.String) {
                return new JObject(new JProperty("@id", ExpandIri(activeContext, value.Value<string>(), documentRelative: true, vocab:false)));
            }

            // 2 - If active property has a type mapping in active context that is @vocab, and the value is a string, return a new map containing a single entry where the key is @id and the value is the result of IRI expanding value using true for document relative.
            if (typeMapping != null && typeMapping == "@vocab" && value.Type == JTokenType.String)
            {
                return new JObject(new JProperty("@id", ExpandIri(activeContext, value.Value<string>(), vocab:true, documentRelative: true)));
            }

            // 3 - Otherwise, initialize result to a map with an @value entry whose value is set to value.
            var result = new JObject(new JProperty("@value", value));

            // 4 - If active property has a type mapping in active context, other than @id, @vocab, or @none, add @type to result and set its value to the value associated with the type mapping.
            if (typeMapping != null && typeMapping != "@id" && typeMapping != "@vocab" && typeMapping != "@none")
            {
                result.Add(new JProperty("@type", typeMapping));
            }
            // 5 - Otherwise, if value is a string:
            else if (value.Type == JTokenType.String)
            {
                // 5.1 - Initialize language to the language mapping for active property in active context, if any, otherwise to the default language of active context.
                var language =
                    activePropertyTermDefinition != null && 
                    activePropertyTermDefinition.HasLanguageMapping
                        ? activePropertyTermDefinition.LanguageMapping
                        : activeContext.Language;

                // 5.2 - Initialize direction to the direction mapping for active property in active context, if any, otherwise to the default base direction of active context.
                var direction = activePropertyTermDefinition?.DirectionMapping ?? (activeContext.BaseDirection ?? LanguageDirection.Unspecified);
                // 5.3 - If language is not null, add @language to result with the value language.
                if (language != null)
                {
                    result.Add(new JProperty("@language", language));
                }
                // 5.4 - If direction is not null, add @direction to result with the value direction.
                if (direction != LanguageDirection.Unspecified)
                {
                    result.Add(new JProperty("@direction", SerializeLanguageDirection(direction)));
                }
            }
            // 6 - Return result.
            return result;
        }

        private JsonLdContainer ParseContainerMapping(string term, string containerValue)
        {
            if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
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
                case JTokenType.Array when _options.ProcessingMode == JsonLdProcessingMode.JsonLd10:
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                        $"Invalid Container Mapping. The value of the @container property of term '{term}' is an array, but the processing mode is set to JSON-LD 1.0.");
                case JTokenType.Array:
                {
                    foreach (var entry in containerValue.Children())
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

        /// <summary>
        /// Run the Compaction algorithm.
        /// </summary>
        /// <param name="input">The JSON-LD data to be compacted. Expected to be a JObject or JArray of JObject or a JString whose value is the IRI reference to a JSON-LD document to be retrieved.</param>
        /// <param name="context">The context to use for the compaction process. May be a JObject, JArray of JObject, JString or JArray of JString. String values are treated as IRI references to context documents to be retrieved.</param>
        /// <param name="options">Additional processor options.</param>
        /// <returns></returns>
        public static JObject Compact(JToken input, JToken context, JsonLdProcessorOptions options)
        {
            var processor = new JsonLdProcessor(options);

            // Set expanded input to the result of using the expand method using input and options.
            JArray expandedInput = null;
            Uri remoteDocumentUrl = null;
            Uri contextBase = null;
            if (input.Type == JTokenType.String)
            {
                var remoteDocument = LoadJson(new Uri(input.Value<string>()),
                    new JsonLdLoaderOptions {ExtractAllScripts = options.ExtractAllScripts}, options);
                expandedInput = Expand(remoteDocument, remoteDocument.DocumentUrl,
                    new JsonLdLoaderOptions {ExtractAllScripts = false}, options);
                remoteDocumentUrl = contextBase = remoteDocument.DocumentUrl;
            }
            else
            {
                expandedInput = Expand(input, options);
            }

            if (contextBase == null) contextBase = options.Base;
            if (context is JObject contextObject && contextObject.ContainsKey("@context"))
            {
                context = contextObject["@context"];
            }

            var activeContext = processor.ProcessContext(new JsonLdContext(), context, contextBase);
            if (activeContext.Base == null)
            {
                activeContext.Base = options?.Base ?? (options.CompactToRelative ? remoteDocumentUrl : null);
            }

            var compactedOutput = processor.CompactAlgorithm(activeContext, null, expandedInput, options.CompactArrays,
                options.Ordered);
            if (IsEmptyArray(compactedOutput))
            {
                compactedOutput = new JObject();
            }
            else if (compactedOutput is JArray)
            {
                compactedOutput = new JObject(new JProperty(processor.CompactIri(activeContext, "@graph", vocab: true),
                    compactedOutput));
            }

            if (context != null && !IsEmptyObject(context))
            {
                (compactedOutput as JObject)["@context"] = context;
            }

            return compactedOutput as JObject;
        }

        private static bool IsEmptyObject(JToken token)
        {
            if (token.Type != JTokenType.Object) return false;
            return (token as JObject).Count == 0;
        }

        /*
        private JObject CompactWrapper(JToken context, JObject inverseContext, string activeProperty,
            JToken element, bool compactArrays = true)
        {
            var activeContext = ProcessContext(new JsonLdContext(), context);
            if (!inverseContext.Properties().Any()) inverseContext = CreateInverseContext(activeContext);
            var algorithmResult = CompactAlgorithm(activeContext, inverseContext, activeProperty, element,
                compactArrays);
            // If, after the algorithm outlined above is run, the result result is an array, 
            // replace it with a new dictionary with a single member whose key is the result of using the IRI Compaction algorithm, 
            // passing active context, inverse context, and @graph as iri and whose value is the array result. 
            // Finally, if a non-empty context has been passed, add an @context member to result and set its value to the passed context.
            if (algorithmResult is JArray)
            {
                if ((algorithmResult as JArray).Count > 0)
                {
                    algorithmResult = new JObject(
                        new JProperty(
                            CompactIri(activeContext, inverseContext, "@graph", vocab: true), algorithmResult));
                }
                else
                {
                    algorithmResult = new JObject();
                }
            }
            var contextArray = context as JArray;
            var contextObject = context as JObject;
            var contextString = context as JValue;
            if (contextArray != null && contextArray.Count > 0 ||
                contextObject != null && contextObject.Properties().Any() ||
                contextString != null)
            {
                (algorithmResult as JObject).Add("@context", context);
            }
            return algorithmResult as JObject;
        }
        */

        private JToken CompactAlgorithm(JsonLdContext activeContext, string activeProperty, JToken element, bool compactArrays = false, bool ordered = false)
        {
            JsonLdTermDefinition activeTermDefinition = null;
            if (activeProperty != null)
            {
                activeContext.TryGetTerm(activeProperty, out activeTermDefinition, true);
            }

            // 1 - Initialize type-scoped context to active context. This is used for compacting values that may be relevant to any previous type - scoped context.
            var typeScopedContext = activeContext;

            // 2 - If element is a scalar, it is already in its most compact form, so simply return element.
            if (IsScalar(element)) return element;

            // 3 - If element is an array: 
            if (element is JArray elementArray)
            {
                // 3.1 - Initialize result to an empty array.
                var arrayResult = new JArray();

                // 3.2 - For each item in element: 
                foreach (var item in element)
                {
                    // 3.2.1 - Initialize compacted item to the result of using this algorithm recursively, passing active context, active property, item for element, and the compactArrays and ordered flags.
                    var compactedItem = CompactAlgorithm(activeContext, activeProperty, item, compactArrays, ordered);
                    // 3.2.2 - If compacted item is not null, then append it to result.
                    if (compactedItem != null) arrayResult.Add(compactedItem);
                }
                // 3.3 - If result is empty or contains more than one value, or compactArrays is false, or active property is either @graph or @set, or container mapping for active property in active context includes either @list or @set, return result.
                if (arrayResult.Count != 1 || !compactArrays || "@graph".Equals(activeProperty) || "@set".Equals(activeProperty))
                {
                    return arrayResult;
                }
                if (activeTermDefinition != null && (activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.List) || 
                                                     activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.Set)))
                {
                    return arrayResult;
                }

                return arrayResult[0];
            }

            // 4 - Otherwise element is a map.
            var elementObject = element as JObject;

            // 5 - If active context has a previous context, the active context is not propagated.
            // If element does not contain an @value entry, and element does not consist of a single @id entry,
            // set active context to previous context from active context, as the scope of a term-scoped context does not apply when processing new node objects.
            if (activeContext.PreviousContext != null)
            {
                if (!elementObject.ContainsKey("@value") &&
                    (elementObject.Count > 1 || !elementObject.ContainsKey("@id")))
                {
                    activeContext = activeContext.PreviousContext;
                    // KA: Spec implies that step 6 uses the term definition in active context (after revert), but implementation uses typeScopedContext (before revert)
                    // Checking this at https://github.com/w3c/json-ld-api/issues/502
                    //activeTermDefinition = activeContext.GetTerm(activeProperty);
                }
            }

            // 6 - If the term definition for active property in active context has a local context:
            if (activeTermDefinition?.LocalContext != null)
            {
                // 6.1 - Set active context to the result of the Context Processing algorithm, passing active context, the value of the active property's
                // local context as local context, base URL from the term definition for active property in active context, and true for override protected.
                activeContext = ProcessContext(activeContext, activeTermDefinition.LocalContext,
                    activeTermDefinition.BaseUrl, overrideProtected: true);
                activeTermDefinition = activeContext.GetTerm(activeProperty);
            }

            // 7 - If element has an @value or @id entry and the result of using the Value Compaction algorithm, passing active context, active property,
            // and element as value is a scalar, or the term definition for active property has a type mapping of @json, return that result.
            if (elementObject.ContainsKey("@value") || elementObject.ContainsKey("@id"))
            {
                var compactValue = CompactValue(activeContext, activeProperty, elementObject);
                if (IsScalar(compactValue) ||
                    (activeTermDefinition != null && "@json".Equals(activeTermDefinition.TypeMapping)))
                {
                    return compactValue;
                }
            }
            // 8 - If element is a list object, and the container mapping for active property in active context includes @list, return the result of using this
            // algorithm recursively, passing active context, active property, value of @list in element for element, and the compactArrays and ordered flags.
            if (IsListObject(elementObject) && activeTermDefinition != null && activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.List))
            {
                return CompactAlgorithm(activeContext, activeProperty, elementObject["@list"], compactArrays, ordered);
            }
            // 9 - Initialize inside reverse to true if active property equals @reverse, otherwise to false.
            var insideReverse = "@reverse".Equals(activeProperty);
            // 10 - Initialize result to an empty map.
            var result = new JObject();
            // 11 - If element has an @type entry...
            if (elementObject.ContainsKey("@type"))
            {
                // ...create a new array compacted types initialized by transforming each expanded type of that entry into its
                // compacted form by IRI compacting expanded type. 
                var compactedTypes = new JArray();
                var expandedTypes = EnsureArray(elementObject["@type"]);
                foreach (var expandedType in expandedTypes)
                {
                    var compactedType = CompactIri(activeContext, expandedType.Value<string>(), vocab: true);
                    compactedTypes.Add(compactedType);
                }
                // Then, for each term in compacted types ordered lexicographically: 
                foreach (var term in compactedTypes.Select(t=>t.Value<string>()).OrderBy(x=>x))
                {
                    // 11.1 - If the term definition for term in type-scoped context has a local context set active context to the result of the
                    // Context Processing algorithm, passing active context and the value of term's local context in type-scoped context as
                    // local context base URL from the term definition for term in type-scoped context, and false for propagate. 
                    if (typeScopedContext.TryGetTerm(term, out var termDef) && termDef.LocalContext != null)
                    {
                        activeContext = ProcessContext(activeContext, termDef.LocalContext, termDef.BaseUrl,
                            propagate: false);
                    }
                }
            }
            // 12 - For each key expanded property and value expanded value in element, ordered lexicographically by expanded property if ordered is true: 
            var properties = elementObject.Properties();
            if (ordered) properties = properties.OrderBy(p => p.Name);
            foreach (var p in properties)
            {
                var expandedProperty = p.Name;
                var expandedValue = p.Value;
                // 12.1 - If expanded property is @id:
                if (expandedProperty.Equals("@id"))
                {
                    // 12.1.1 - If expanded value is a string, then initialize compacted value by IRI compacting expanded value with vocab set to false.
                    var compactedValue = CompactIri(activeContext, expandedValue.Value<string>(), vocab: false);
                    // 12.1.2 - Initialize alias by IRI compacting expanded property.
                    var alias = CompactIri(activeContext, expandedProperty, vocab: true);
                    // 12.1.3 - Add an entry alias to result whose value is set to compacted value and continue to the next expanded property.
                    result.Add(new JProperty(alias, compactedValue));
                    continue;
                }

                // 12.2 - If expanded property is @type:
                if (expandedProperty.Equals("@type"))
                {
                    JToken compactedValue;
                    // 12.2.1 - If expanded value is a string, then initialize compacted value by IRI compacting expanded value using type-scoped context for active context.
                    if (expandedValue.Type == JTokenType.String)
                    {
                        compactedValue = CompactIri(typeScopedContext, expandedValue.Value<string>(), vocab: true);
                    }
                    else
                    {
                        // 12.2.2 - Otherwise, expanded value must be a @type array:
                        // 12.2.2.1 - Initialize compacted value to an empty array.
                        var compactedValueArray  = new JArray();
                        compactedValue = compactedValueArray;
                        // 12.2.2.2 - For each item expanded type in expanded value:
                        foreach (var item in expandedValue.Children())
                        {
                            // 12.2.2.2.1 - Set term by IRI compacting expanded type using type-scoped context for active context.
                            var term = CompactIri(typeScopedContext, item.Value<string>(), vocab: true);
                            // 12.2.2.2.2 - Append term, to compacted value.
                            compactedValueArray.Add(term);
                        }
                    }

                    // 12.2.3 - Initialize alias by IRI compacting expanded property.
                    var alias = CompactIri(activeContext, expandedProperty, vocab: true);

                    // 12.2.4 - Initialize as array to true if processing mode is json - ld - 1.1 and the container mapping for alias in the active context includes @set, otherwise to the negation of compactArrays.
                    var aliasTermDefinition = activeContext.GetTerm(alias);
                    var asArray =
                        _options.ProcessingMode == JsonLdProcessingMode.JsonLd11 &&
                        aliasTermDefinition != null &&
                        aliasTermDefinition.ContainerMapping.Contains(JsonLdContainer.Set)
                            ? true
                            : !compactArrays;
                    // 12.2.5 - Use add value to add compacted value to the alias entry in result using as array.
                    AddValue(result, alias, compactedValue, asArray);

                    // 12.2.6 - Continue to the next expanded property.
                    continue;
                }
                // 12.3 - If expanded property is @reverse:
                if ("@reverse".Equals(expandedProperty))
                {
                    // 12.3.1 - Initialize compacted value to the result of using this algorithm recursively, passing active context, @reverse for active property, expanded value for element, and the compactArrays and ordered flags.
                    var compactedValue =
                        CompactAlgorithm(activeContext, "@reverse", expandedValue, compactArrays, ordered);
                    if (compactedValue is JObject compactedObject)
                    {
                        // 12.3.2 - For each property and value in compacted value:
                        foreach (var compactedObjectProperty in compactedObject.Properties().ToList())
                        {
                            // 12.3.1 - If the term definition for property in the active context indicates that property is a reverse property
                            var td = activeContext.GetTerm(compactedObjectProperty.Name, true);
                            if (td != null && td.Reverse)
                            {
                                // 12.3.2.1.1 - Initialize as array to true if the container mapping for property in the active context includes @set, otherwise the negation of compactArrays.
                                var asArray = td.ContainerMapping.Contains(JsonLdContainer.Set) ? true : !compactArrays;
                                // 12.3.2.1.2 - Use add value to add value to the property entry in result using as array.
                                AddValue(result, compactedObjectProperty.Name, compactedObjectProperty.Value, asArray);
                                // 12.3.2.1.3 - Remove the property entry from compacted value.
                                compactedObjectProperty.Remove();
                            }
                        }
                        // 12.3.3 - If compacted value has some remaining map entries, i.e., it is not an empty map:
                        if (compactedObject.HasValues)
                        {
                            var alias = CompactIri(activeContext, "@reverse", vocab: true);
                            result.Add(alias, compactedValue);
                        }
                    }
                    // 12.3.4 - Continue with the next expanded property from element.
                    continue;
                }
                // 12.4 - If expanded property is @preserve then:
                if ("@preserve".Equals(expandedProperty))
                {
                    if (!IsEmptyArray(expandedValue))
                    {
                        // 12.4.1 - Initialize compacted value to the result of using this algorithm recursively, passing
                        // active context, active property, expanded value for element, and the compactArrays and ordered flags.
                        var compactedValue = CompactAlgorithm(activeContext, activeProperty, expandedValue,
                            compactArrays,
                            ordered);
                        // 12.4.2 Add compacted value as the value of @preserve in result unless expanded value is an empty array.
                        result.Add("@preserve", compactedValue);
                    }
                }
                // 12.5 - If expanded property is @index and active property has a container mapping in active context that includes
                // @index, then the compacted result will be inside of an @index container, drop the @index entry by continuing to
                // the next expanded property.
                if ("@index".Equals(expandedProperty) && 
                    activeTermDefinition != null &&
                    activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.Index))
                {
                    continue;
                }
                // 12.6 - Otherwise, if expanded property is @direction, @index, @language, or @value: 
                else if ("@direction".Equals(expandedProperty) ||
                           "@index".Equals(expandedProperty) ||
                           "@language".Equals(expandedProperty) ||
                           "@value".Equals(expandedProperty))
                {
                    // 12.6.1 - Initialize alias by IRI compacting expanded property.
                    var alias = CompactIri(activeContext, expandedProperty, vocab: true);
                    // 12.6.2 - Add an entry alias to result whose value is set to expanded value and continue with the next expanded property.
                    result.Add(alias, expandedValue);
                    continue;
                }
                // 12.7 - If expanded value is an empty array: 
                if (IsEmptyArray(expandedValue))
                {
                    // 12.7.1 - Initialize item active property by IRI compacting expanded property using expanded value for value and inside reverse for reverse.
                    var itemActiveProperty =
                        CompactIri(activeContext, expandedProperty, expandedValue, true, insideReverse);
                    // 12.7.2 - If the term definition for item active property in the active context has a nest value entry(nest term): 
                    var td = activeContext.GetTerm(itemActiveProperty);
                    JObject nestResult = null;
                    if (td != null && td.Nest != null)
                    {

                        // 12.7.2.1 - If nest term is not @nest, or a term in the active context that expands to @nest, an invalid @nest value error has been detected, and processing is aborted.
                        var nestTerm = ExpandIri(activeContext, td.Nest, true);
                        if (!"@nest".Equals(nestTerm))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                                $"Invalid Nest Value. Error compacting property {expandedProperty} of {activeProperty}. The value of {expandedProperty} should be '@nest' or a term that expands to '@nest'.");
                        }
                        // 12.7.2.2 - If result does not have a nest term entry, initialize it to an empty map.
                        if (!result.ContainsKey("@nest"))
                        {
                            result.Add("@nest", new JObject());
                        }
                        // 12.7.2.3 - Initialize nest result to the value of nest term in result.
                        nestResult = result["@nest"] as JObject;
                    }
                    // 12.7.3 - Otherwise, initialize nest result to result.
                    else
                    {
                        nestResult = result;
                    }
                    // 12.7.4 - Use add value to add an empty array to the item active property entry in nest result using true for as array.
                    AddValue(nestResult, itemActiveProperty, new JArray(), true);
                }
                // 12.8 -  At this point, expanded value must be an array due to the Expansion algorithm. For each item expanded item in expanded value: 
                foreach (var expandedItem in expandedValue.Children())
                {
                    // 12.8.1 - Initialize item active property by IRI compacting expanded property using expanded item for value and inside reverse for reverse.
                    var itemActiveProperty =
                        CompactIri(activeContext, expandedProperty, expandedItem, true, insideReverse);
                    // 12.8.2 - If the term definition for item active property in the active context has a nest value entry (nest term):
                    var td = activeContext.GetTerm(itemActiveProperty);
                    JObject nestResult = null;
                    if (td != null && td.Nest != null)
                    {
                        // 12.8.2.1 - If nest term is not @nest, or a term in the active context that expands to @nest, an invalid @nest value error has been detected, and processing is aborted.
                        var nestTerm = ExpandIri(activeContext, td.Nest, true);
                        if (!"@nest".Equals(nestTerm))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                                $"Invalid Nest Value. Error compacting property {expandedProperty} of {activeProperty}. The value of {expandedProperty} should be '@nest' or a term that expands to '@nest'.");
                        }
                        // 12.8.2.2 - If result does not have a nest term entry, initialize it to an empty map.
                        if (!result.ContainsKey("@nest"))
                        {
                            result.Add("@nest", new JObject());
                        }
                        // 12.8.2.3 - Initialize nest result to the value of nest term in result.
                        nestResult = result["@nest"] as JObject;
                    }
                    // 12.8.3 - Otherwise, initialize nest result to result.
                    else
                    {
                        nestResult = result;
                    }

                    // 12.8.4 - Initialize container to container mapping for item active property in active context, or to a new empty array, if there is no such container mapping.
                    var container = (td != null && td.ContainerMapping != null)
                        ? td.ContainerMapping
                        : new HashSet<JsonLdContainer>();
                    // 12.8.5 - Initialize as array to true if container includes @set, or if item active property is @graph or @list, otherwise the negation of compactArrays.
                    var asArray =
                        (container.Contains(JsonLdContainer.Set) || "@graph".Equals(itemActiveProperty) ||
                         "@list".Equals(itemActiveProperty))
                            ? true
                            : !compactArrays;
                    // 12.8.6 - Initialize compacted item to the result of using this algorithm recursively, passing active context,
                    // item active property for active property, expanded item for element, along with the compactArrays and ordered flags.
                    // If expanded item is a list object or a graph object, use the value of the @list or @graph entries, respectively, for element instead of expanded item.
                    var elementToCompact = IsListObject(expandedItem) ? expandedItem["@list"] :
                        IsGraphObject(expandedItem) ? expandedItem["@graph"] : expandedItem;
                    var compactedItem = CompactAlgorithm(activeContext, itemActiveProperty, elementToCompact,
                        compactArrays, ordered);
                    // 12.8.7 - If expanded item is a list object: 
                    if (IsListObject(expandedItem))
                    {
                        // 12.8.7.1 - If compacted item is not an array, then set compacted item to an array containing only compacted item.
                        compactedItem = EnsureArray(compactedItem);
                        // 12.8.7.2 - If container does not include @list: 
                        if (!container.Contains(JsonLdContainer.List))
                        {
                            // 12.8.7.2.1 - Convert compacted item to a list object by setting it to a map containing an entry where the key is
                            // the result of IRI compacting @list and the value is the original compacted item.
                            compactedItem = new JObject(new JProperty(CompactIri(activeContext, "@list", vocab:true), compactedItem));

                            // 12.8.7.2.2 - If expanded item contains the entry @index - value, then add an entry to compacted item where the key is the result of IRI compacting @index and value is value.
                            if (expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@index"))
                            {
                                (compactedItem as JObject).Add(new JProperty(CompactIri(activeContext, "@index", vocab:true), expandedItemObject["@index"]));
                            }
                            // 12.8.7.2.3 - Use add value to add compacted item to the item active property entry in nest result using as array.
                            AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                        }
                        // 12.8.7.3 - Otherwise, set the value of the item active property entry in nest result to compacted item.
                        else
                        {
                            nestResult[itemActiveProperty] = compactedItem;
                        }
                    }
                    // 12.8.8 - If expanded item is a graph object: 
                    // KA: Although algorithm doesn't say "Otherwise", I think this is an else-if branch as nestResult gets updated in the preceding if branch.
                    else if (IsGraphObject(expandedItem)) 
                    {
                        // 12.8.8.1 - If container includes @graph and @id: 
                        if (container.Contains(JsonLdContainer.Graph) && container.Contains(JsonLdContainer.Id))
                        {
                            // 12.8.8.1.1 - Initialize map object to the value of item active property in nest result, initializing it to a new empty map, if necessary.
                            var mapObject = EnsureMapEntry(nestResult, itemActiveProperty);

                            // 12.8.8.1.2 - Initialize map key by IRI compacting the value of @id in expanded item or @none if no such value exists with vocab set to false if there is an @id entry in expanded item.
                            var mapKey =
                                (expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@id"))
                                    ? CompactIri(activeContext, expandedItemObject["@id"].Value<string>(),
                                        vocab: false)
                                    : CompactIri(activeContext, "@none", vocab: true);
                            // 12.8.8.1.3 - Use add value to add compacted item to the map key entry in map object using as array.
                            AddValue(mapObject, mapKey, compactedItem, asArray);
                        }
                        // 12.8.8.2 - Otherwise, if container includes @graph and @index and expanded item is a simple graph object: 
                        else if (IsSimpleGraphObject(expandedItem) && container.Contains(JsonLdContainer.Graph) &&
                                 container.Contains(JsonLdContainer.Index))
                        {
                            // 12.8.8.2.1 - Initialize map object to the value of item active property in nest result, initializing it to a new empty map, if necessary.
                            var mapObject = EnsureMapEntry(nestResult, itemActiveProperty);
                            // 12.8.8.2.2 - Initialize map key the value of @index in expanded item or @none, if no such value exists.
                            var mapKey =
                                expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@index")
                                    ? expandedItemObject["@index"].Value<string>()
                                    : "@none";
                            // 12.8.8.2.3 - Use add value to add compacted item to the map key entry in map object using as array.
                            AddValue(mapObject, mapKey, compactedItem, asArray);
                        }
                        // 12.8.8.3 - Otherwise, if container includes @graph and expanded item is a simple graph object the value cannot be represented as a map object. 
                        else if (IsSimpleGraphObject(expandedItem) && container.Contains(JsonLdContainer.Graph))
                        {
                            // 12.8.8.3.1 - If compacted item is an array with more than one value, it cannot be directly represented, as multiple objects would be interpreted as different named graphs.
                            if ((compactedItem is JArray compactedItemArray) && compactedItemArray.Count > 1)
                            {
                                // Set compacted item to a new map, containing the key from IRI compacting @included and the original compacted item as the value.
                                compactedItem =
                                    new JObject(new JProperty(CompactIri(activeContext, "@included", vocab: true),
                                        compactedItem));
                            }

                            // 12.8.8.3.2 - Use add value to add compacted item to the item active property entry in nest result using as array.
                            AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                        }
                        // 12.8.8.4 - Otherwise, container does not include @graph or otherwise does not match one of the previous cases.
                        else
                        {
                            // 12.8.8.4.1 - Set compacted item to a new map containing the key from IRI compacting @graph using the original compacted item as a value.
                            compactedItem = new JObject(new JProperty(CompactIri(activeContext, "@graph", vocab:true), compactedItem));
                            if (expandedItem is JObject expandedItemObject)
                            {
                                // 12.8.8.4.2 - If expanded item contains an @id entry, add an entry in compacted item using the key from IRI
                                // compacting @id using the value of IRI compacting the value of @id in expanded item using false for vocab.
                                if (expandedItemObject.ContainsKey("@id"))
                                {
                                    (compactedItem as JObject).Add(
                                        CompactIri(activeContext, "@id", vocab: true),
                                        CompactIri(activeContext, expandedItemObject["@id"].Value<string>(),
                                            vocab: false));
                                }
                                // 12.8.8.4.3 - If expanded item contains an @index entry, add an entry in compacted item using the key from IRI compacting @index and the value of @index in expanded item.
                                if (expandedItemObject.ContainsKey("@index"))
                                {
                                    (compactedItem as JObject).Add(
                                        CompactIri(activeContext, "@index", vocab: true),
                                        CompactIri(activeContext, expandedItemObject["@index"].Value<string>(),
                                            vocab: true));
                                }
                            }
                            // 12.8.8.4.4 - Use add value to add compacted item to the item active property entry in nest result using as array.
                            AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                        }
                    }
                    // 12.8.9 - Otherwise, if container includes @language, @index, @id, or @type and container does not include @graph: 
                    else if ((container.Contains(JsonLdContainer.Language) ||
                              container.Contains(JsonLdContainer.Index) ||
                              container.Contains(JsonLdContainer.Id) ||
                              container.Contains(JsonLdContainer.Type)) && !container.Contains(JsonLdContainer.Graph))
                    {
                        // 12.8.9.1 - Initialize map object to the value of item active property in nest result, initializing it to a new empty map, if necessary.
                        if (!nestResult.ContainsKey(itemActiveProperty))
                        {
                            nestResult[itemActiveProperty] = new JObject();
                        }
                        var mapObject = nestResult[itemActiveProperty] as JObject;

                        // 12.8.9.2 - Initialize container key by IRI compacting either @language, @index, @id, or @type based on the contents of container.
                        string expandedKey = null;
                        if (container.Contains(JsonLdContainer.Language)) expandedKey = "@langauge";
                        if (container.Contains(JsonLdContainer.Index)) expandedKey = "@index";
                        if (container.Contains(JsonLdContainer.Id)) expandedKey = "@id";
                        if (container.Contains(JsonLdContainer.Type)) expandedKey = "@type";
                        var containerKey = CompactIri(activeContext, expandedKey, vocab: true);

                        // 12.8.9.3 - Initialize index key to the value of index mapping in the term definition associated with item active property in active context, or @index, if no such value exists.
                        var indexKey = activeTermDefinition != null && activeTermDefinition.IndexMapping != null
                            ? activeTermDefinition.IndexMapping
                            : "@index";

                        // 12.8.9.4 - If container includes @language and expanded item contains a @value entry, then set compacted item to the value associated with its @value entry.
                        // Set map key to the value of @language in expanded item, if any.
                        string mapKey = null;
                        var expandedItemObject = expandedItem as JObject;
                        if (container.Contains(JsonLdContainer.Language) && expandedItemObject != null &&
                            expandedItemObject.ContainsKey("@value"))
                        {
                            compactedItem = expandedItemObject["@value"];
                            if (expandedItemObject.ContainsKey("@language"))
                            {
                                mapKey = expandedItemObject["@language"].Value<string>();
                            }
                        }
                        // 12.8.9.5 - Otherwise, if container includes @index and index key is @index, set map key to the value of @index in expanded item, if any.
                        else if (container.Contains(JsonLdContainer.Index) && "@index".Equals(indexKey))
                        {
                            if (expandedItemObject != null && expandedItemObject.ContainsKey("@index"))
                            {
                                mapKey = expandedItemObject["@index"].Value<string>();
                            }
                        }
                        // 12.8.9.6 - Otherwise, if container includes @index and index key is not @index: 
                        else if (container.Contains(JsonLdContainer.Index))
                        {
                            // 12.8.9.6.1 - Reinitialize container key by IRI compacting index key.
                            containerKey = CompactIri(activeContext, indexKey, vocab: true);
                            // 12.8.9.6.2 - Set map key to the first value of container key in compacted item, if any.
                            // 12.8.9.6.3 - If there are remaining values in compacted item for container key, use add value to add those remaining values to the container key in compacted item.
                            // Otherwise, remove that entry from compacted item.
                            var array = EnsureArray(compactedItem[containerKey]);
                            if (array.Count > 0)
                            {
                                mapKey = array[0].Value<string>();
                                if (array.Count > 1)
                                {
                                    array.RemoveAt(0);
                                }
                                else
                                {
                                    (compactedItem as JObject).Remove(containerKey);
                                }
                            }
                        }
                        // 12.8.9.7 - Otherwise, if container includes @id, set map key to the value of container key in compacted item and remove container key from compacted item.
                        else if (container.Contains(JsonLdContainer.Id))
                        {
                            mapKey = compactedItem[containerKey].Value<string>();
                            (compactedItem as JObject).Remove(containerKey);
                        }
                        // 12.8.9.8 - Otherwise, if container includes @type: 
                        else if (container.Contains(JsonLdContainer.Type))
                        {
                            // 12.8.9.8.1 - Set map key to the first value of container key in compacted item, if any.
                            // 12.8.9.8.2 - If there are remaining values in compacted item for container key, use add value to add those remaining values to the container key in compacted item.
                            // 12.8.9.8.3 - Otherwise, remove that entry from compacted item.
                            var array = EnsureArray(compactedItem[containerKey]);
                            if (array.Count > 0)
                            {
                                mapKey = array[0].Value<string>();
                                array.RemoveAt(0);
                                if (array.Count == 0) (compactedItem as JObject).Remove(containerKey);
                                else if (array.Count == 1) compactedItem[containerKey] = array[0];
                            }
                            // 12.8.9.8.4 - If compacted item contains a single entry with a key expanding to @id, set compacted item to the result of using this algorithm recursively, passing active context, item active property for active property, and a map composed of the single entry for @id from expanded item for element.
                            if ((compactedItem is JObject compactedItemObject) && compactedItemObject.Count == 1)
                            {
                                if (ExpandIri(activeContext, compactedItemObject.Properties().First().Name, vocab: true)
                                    .Equals("@id"))
                                {
                                    compactedItem = CompactAlgorithm(activeContext, itemActiveProperty,
                                        new JObject("@id", expandedItemObject["@id"]));
                                }
                            }
                        }
                        // 12.8.9.9 - If map key is null, set it to the result of IRI compacting @none.
                        if (mapKey == null) mapKey = CompactIri(activeContext, "@none", vocab: true);
                        // 12.8.9.10 - Use add value to add compacted item to the map key entry in map object using as array.
                        AddValue(mapObject, mapKey, compactedItem, asArray);
                    }
                    // 12.8.10 - Otherwise, use add value to add compacted item to the item active property entry in nest result using as array.
                    else
                    {
                        AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Ensure that a JObject has an entry for a given property, initializing it to an empty map if it does not exist
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static JObject EnsureMapEntry(JObject parent, string property)
        {
            if (!parent.ContainsKey(property))
            {
                parent[property] = new JObject();
            }
            return parent[property] as JObject;
        }

        /// <summary>
        /// Flattens the given input and compacts it using the passed context according to the steps in the JSON-LD Flattening algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JToken Flatten(JToken input, JToken context, JsonLdProcessorOptions options = null)
        {
            var processor = new JsonLdProcessor(options);
            // Set expanded input to the result of using the expand method using input and options.
            var expandedInput = Expand(input, options);
            // If context is a dictionary having an @context member, set context to that member's value, otherwise to context.
            var contextObject = context as JObject;
            if (contextObject?.Property("@context") != null)
            {
                context = contextObject["@context"];
            }
            return processor.FlattenWrapper(expandedInput, context, options?.CompactArrays ?? true);
        }

        private JToken FlattenWrapper(JToken element, JToken context, bool compactArrays = true)
        {
            // 1 = Initialize node map to a dictionary consisting of a single member whose key is @default and whose value is an empty dictionary.
            var nodeMap = new JObject(new JProperty("@default", new JObject()));
            // 2 - Perform the Node Map Generation algorithm, passing element and node map.
            GenerateNodeMap(element, nodeMap);
            // 3 - Initialize default graph to the value of the @default member of node map, which is a dictionary representing the default graph.
            var defaultGraph = nodeMap["@default"] as JObject;
            // 4 - For each key-value pair graph name-graph in node map where graph name is not @default, perform the following steps:
            foreach (var p in nodeMap.Properties().Where(p => !p.Name.Equals("@default")))
            {
                var graphName = p.Name;
                var graph = p.Value as JObject;
                // 4.1 - If default graph does not have a graph name member, create one and initialize its value to a dictionary consisting of an @id member whose value is set to graph name.
                if (defaultGraph.Property(graphName) == null)
                {
                    defaultGraph.Add(graphName, new JObject(new JProperty("@id", graphName)));
                }
                // 4.2 - Reference the value associated with the graph name member in default graph using the variable entry.
                var entry = defaultGraph[graphName] as JObject;
                // 4.3 - Add an @graph member to entry and set it to an empty array.
                entry["@graph"] = new JArray();
                // 4.4 - For each id-node pair in graph ordered by id, add node to the @graph member of entry, unless the only member of node is @id.
                foreach (var gp in graph.Properties().OrderBy(gp=>gp.Name))
                {
                    var node = gp.Value as JObject;
                    if (node.Properties().Any(x => !x.Name.Equals("@id")))
                    {
                        ((JArray)entry["@graph"]).Add(node);
                    }
                }
            }
            // 5 - Initialize an empty array flattened.
            var flattened = new JArray();
            // 6 - For each id-node pair in default graph ordered by id, add node to flattened, unless the only member of node is @id.
            foreach (var p in defaultGraph.Properties().OrderBy(p => p.Name))
            {
                var node = p.Value as JObject;
                if (node.Properties().Any(x => !x.Name.Equals("@id")))
                {
                    flattened.Add(node);
                }
            }
            // 7 - If context is null, return flattened.
            if (context == null) return flattened;
            
            // 8 - Otherwise, return the result of compacting flattened according the Compaction algorithm passing context 
            // ensuring that the compaction result has only the @graph keyword (or its alias) at the top-level other than @context, 
            // even if the context is empty or if there is only one element to put in the @graph array. This ensures that the returned document has a deterministic structure.
            var compactResult = Compact(flattened, context, _options);

            return compactResult;
        }

        /// <summary>
        /// Applies the Node Map Generation algorithm to the specified input.
        /// </summary>
        /// <param name="element">The element to be processed.</param>
        /// <param name="options">JSON-LD processor options.</param>
        /// <returns>The generated node map dictionary as a JObject instance.</returns>
        public static JObject GenerateNodeMap(JToken element, JsonLdProcessorOptions options = null)
        {
            var processor = new JsonLdProcessor(options);
            var nodeMap = new JObject(new JProperty("@default", new JObject()));
            processor.GenerateNodeMap(element, nodeMap);
            return nodeMap;
        }

        private void GenerateNodeMap(JToken element, JObject nodeMap,
            string activeGraph = "@default", JToken activeSubject = null,
            string activeProperty = null, JObject list = null)
        {
            // 1 - If element is an array, process each item in element as follows and then return:
            if (element is JArray)
            {
                foreach (var item in ((JArray) element))
                {
                    // 1.1 - Run this algorithm recursively by passing item for element, node map, active graph, active subject, active property, and list.
                    GenerateNodeMap(item, nodeMap, activeGraph, activeSubject, activeProperty, list);
                }
                return;
            }
            // 2 - Otherwise element is a dictionary. 
            // Reference the dictionary which is the value of the active graph member of node map using the variable graph. 
            // If the active subject is null, set node to null otherwise reference the active subject member of graph using 
            // the variable node.
            var elementObject = element as JObject;
            var graph = nodeMap[activeGraph] as JObject;
            JObject node = null, activeSubjectNode = null;
            if (activeSubject != null && activeSubject is JValue)
            {
                activeSubjectNode = node = graph[activeSubject.Value<string>()] as JObject;
            }
            // 3 - If element has an @type member, perform for each item the following steps:
            var typeProperty = elementObject.Property("@type");
            if (typeProperty != null)
            {
                var typeArray = typeProperty.Value as JArray;
                if (typeArray != null)
                {
                    // 3.1 - If item is a blank node identifier, replace it with a newly generated blank node identifier passing item for identifier.
                    for (var ix = 0; ix < typeArray.Count; ix++)
                    {
                        var typeId = typeArray[ix].Value<string>();
                        if (IsBlankNodeIdentifier(typeId))
                        {
                            typeArray[ix] = GenerateBlankNodeIdentifier(typeId);
                        }
                    }
                }
                else
                {
                    var typeId = typeProperty.Value.Value<string>();
                    if (IsBlankNodeIdentifier(typeId))
                    {
                        elementObject["@type"] = GenerateBlankNodeIdentifier(typeId);
                    }
                }
            }
            // 4 - If element has an @value member, perform the following steps:
            if (elementObject.Property("@value") != null)
            {
                // 4.1 - If list is null:
                if (list == null)
                {
                    // 4.1.1 - If node does not have an active property member, create one and initialize its value to an array containing element.
                    if (node.Property(activeProperty) == null)
                    {
                        node[activeProperty] = new JArray(element);
                    }
                    // 4.1.2 - Otherwise, compare element against every item in the array associated with the active property member of node. If there is no item equivalent to element, append element to the array. Two dictionaries are considered equal if they have equivalent key-value pairs.
                    var existingItems = node[activeProperty] as JArray;
                    if (!existingItems.Any(x => JToken.DeepEquals(x, element)))
                    {
                        existingItems.Add(element);
                    }
                }
                else
                {
                    // 4.2 - Otherwise, append element to the @list member of list.
                    var listArray = list["@list"] as JArray;
                    listArray.Add(element);
                }
            }
            // 5 - Otherwise, if element has an @list member, perform the following steps:
            else if (elementObject.Property("@list") != null)
            {
                // 5.1 - Initialize a new dictionary result consisting of a single member @list whose value is initialized to an empty array.
                var result = new JObject(new JProperty("@list", new JArray()));
                // 5.2 - Recursively call this algorithm passing the value of element's @list member for element, active graph, active subject, active property, and result for list.
                GenerateNodeMap(element["@list"], nodeMap, activeGraph, activeSubject, activeProperty, result);
                // 5.3 - Append result to the value of the active property member of node.
                (node[activeProperty] as JArray).Add(result);
            }
            // 6 - Otherwise element is a node object, perform the following steps:
            else
            {
                string id;
                // 6.1 - If element has an @id member, set id to its value and remove the member from element. If id is a blank node identifier, replace it with a newly generated blank node identifier passing id for identifier.
                if (elementObject.Property("@id") != null)
                {
                    id = elementObject.Property("@id").Value.Value<string>();
                    elementObject.Remove("@id");
                    if (IsBlankNodeIdentifier(id))
                    {
                        id = GenerateBlankNodeIdentifier(id);
                    }
                }
                // 6.2 - Otherwise, set id to the result of the Generate Blank Node Identifier algorithm passing null for identifier.
                else
                {
                    id = GenerateBlankNodeIdentifier(null);
                }
                // 6.3 - If graph does not contain a member id, create one and initialize its value to a dictionary consisting of a single member @id whose value is id.
                if (graph.Property(id) == null)
                {
                    graph[id] = new JObject(new JProperty("@id", id));
                }
                // 6.4 - Reference the value of the id member of graph using the variable node.
                node = graph[id] as JObject;
                // 6.5 - If active subject is a dictionary, a reverse property relationship is being processed. Perform the following steps:
                if (activeSubject is JObject)
                {
                    // 6.5.1 - If node does not have an active property member, create one and initialize its value to an array containing active subject.
                    if (node.Property(activeProperty) == null)
                    {
                        node[activeProperty] = new JArray(activeSubject);
                    }
                    // 6.5.2 - Otherwise, compare active subject against every item in the array associated with the active property member of node. If there is no item equivalent to active subject, append active subject to the array. Two dictionaries are considered equal if they have equivalent key-value pairs.
                    else
                    {
                        AppendUniqueElement(activeSubject, node[activeProperty] as JArray);
                    }
                }
                // 6.6 - Otherwise, if active property is not null, perform the following steps:
                else if (activeProperty != null)
                {
                    // 6.6.1 - Create a new dictionary reference consisting of a single member @id whose value is id.
                    var reference = new JObject(new JProperty("@id", id));
                    // 6.6.2 - If list is null:
                    if (list == null)
                    {
                        // 6.6.2.1 - If node does not have an active property member, create one and initialize its value to an array containing reference.
                        if (activeSubjectNode.Property(activeProperty) == null)
                        {
                            activeSubjectNode[activeProperty] = new JArray(reference);
                        }
                        // 6.6.2.2 - Otherwise, compare reference against every item in the array associated with the active property member of node. If there is no item equivalent to reference, append reference to the array. Two dictionaries are considered equal if they have equivalent key-value pairs.
                        AppendUniqueElement(reference, activeSubjectNode[activeProperty] as JArray);
                    }
                    else
                    {
                        // 6.6.3 - Otherwise, append element to the @list member of list.
                        var listArray = list["@list"] as JArray;
                        listArray.Add(reference); // KA: Should be reference rather than element (because reference has the @id member on it - confirmed in JSON-LD unit test flatten-0029)
                    }
                }
                // 6.7 - If element has an @type key, append each item of its associated array to the array associated with the @type key of node unless it is already in that array. Finally remove the @type member from element.
                if (elementObject.Property("@type") != null)
                {
                    if (node.Property("@type") == null)
                    {
                        node["@type"] = new JArray();
                    }
                    if (elementObject["@type"] is JArray)
                    {
                        foreach (var item in elementObject["@type"] as JArray)
                        {
                            AppendUniqueElement(item, node["@type"] as JArray);
                        }
                    }
                    else
                    {
                        AppendUniqueElement(elementObject["@type"], node["@type"] as JArray);
                    }
                    elementObject.Remove("@type");
                }
                // 6.8 - If element has an @index member, set the @index member of node to its value. If node has already an @index member with a different value, a conflicting indexes error has been detected and processing is aborted. Otherwise, continue by removing the @index member from element.
                if (elementObject.Property("@index") != null)
                {
                    if (node.Property("@index") != null && !JToken.DeepEquals(elementObject["@index"], node["@index"]))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.ConflictingIndexes,
                            $"Conflicting indexes for node with id {id}.");
                    }
                    node["@index"] = elementObject["@index"];
                    elementObject.Remove("@index");
                }
                // 6.9 - If element has an @reverse member:
                if (elementObject.Property("@reverse") != null)
                {
                    // 6.9.1 - Create a dictionary referenced node with a single member @id whose value is id.
                    var referencedNode = new JObject(new JProperty("@id", id));
                    // 6.9.2 - Set reverse map to the value of the @reverse member of element.
                    var reverseMap = elementObject["@reverse"] as JObject;
                    // 6.9.3 - For each key-value pair property-values in reverse map:
                    foreach (var p in reverseMap.Properties())
                    {
                        var property = p.Name;
                        var values = p.Value as JArray;
                        // 6.9.3.1 - For each value of values:
                        foreach (var value in values)
                        {
                            // 6.9.3.1.1 - Recursively invoke this algorithm passing value for element, node map, active graph, referenced node for active subject, and property for active property. Passing a dictionary for active subject indicates to the algorithm that a reverse property relationship is being processed.
                            GenerateNodeMap(value, nodeMap, activeGraph, referencedNode, property);
                        }
                    }
                    // 6.9.4 - Remove the @reverse member from element.
                    elementObject.Remove("@reverse");
                }
                // 6.10 - If element has an @graph member, recursively invoke this algorithm passing the value of the @graph member for element, node map, and id for active graph before removing the @graph member from element.
                if (elementObject.Property("@graph") != null)
                {
                    // KA: Ensure nodeMap contains a dictionary for the graph
                    if (nodeMap.Property(id) == null)
                    {
                        nodeMap.Add(id, new JObject());
                    }
                    GenerateNodeMap(elementObject["@graph"], nodeMap, id);
                    elementObject.Remove("@graph");
                }
                // 6.11 - Finally, for each key-value pair property-value in element ordered by property perform the following steps:
                foreach (var p in elementObject.Properties().OrderBy(p => p.Name).ToList())
                {
                    var property = p.Name;
                    var value = p.Value;
                    // 6.11.1 - If property is a blank node identifier, replace it with a newly generated blank node identifier passing property for identifier.
                    if (IsBlankNodeIdentifier(property))
                    {
                        property = GenerateBlankNodeIdentifier(property);
                    }
                    // 6.11.2 - If node does not have a property member, create one and initialize its value to an empty array.
                    if (node.Property(property) == null)
                    {
                        node[property] = new JArray();
                    }
                    // 6.11.3 - Recursively invoke this algorithm passing value for element, node map, active graph, id for active subject, and property for active property.
                    GenerateNodeMap(value, nodeMap, activeGraph, id, property);
                }
            }
        }

        private static JObject MergeNodeMaps(JObject graphMap)
        {
            var result = new JObject();
            foreach (var p in graphMap.Properties())
            {
                var graphName = p.Name;
                var nodeMap = p.Value as JObject;
                foreach (var np in nodeMap.Properties())
                {
                    var id = np.Name;
                    var node = np.Value as JObject;
                    var mergedNode = result[id] as JObject;
                    if (mergedNode == null)
                    {
                        result[id] = mergedNode = new JObject(new JProperty("@id", id));
                    }
                    foreach (var nodeProperty in node.Properties())
                    {
                        if (IsKeyword(nodeProperty.Name))
                        {
                            mergedNode[nodeProperty.Name] = nodeProperty.Value.DeepClone();
                        }
                        else
                        {

                            MergeValues(mergedNode, nodeProperty.Name, nodeProperty.Value);
                        }
                    }
                }

            }
            return result;
        }

        private static void MergeValues(JObject parent, string property, JToken values)
        {
            if (parent[property] == null)
            {
                parent[property] = new JArray();
            }
            var target = parent[property] as JArray;
            if (values is JArray)
            {
                foreach (var item in (values as JArray))
                {
                    target.Add(item);
                }
            }
            else
            {
                target.Add(values);
            }
        }
        private static void AppendUniqueElement(JToken element, JArray toArray)
        {
            if (!toArray.Any(x => JToken.DeepEquals(x, element)))
            {
                toArray.Add(element);
            }
        }

        private string GenerateBlankNodeIdentifier(string identifier)
        {
            string mappedIdentifier;
            // 1 - If identifier is not null and has an entry in the identifier map, return the mapped identifier.
            if (identifier != null && _identifierMap.TryGetValue(identifier, out mappedIdentifier))
            {
                return mappedIdentifier;
            }
            // 2 - Otherwise, generate a new blank node identifier by concatenating the string _:b and counter.
            // 3 - Increment counter by 1.
            mappedIdentifier = "_:b" + _counter++;
            // 4 - If identifier is not null, create a new entry for identifier in identifier map and set its value to the new blank node identifier.
            if (identifier != null)
            {
                _identifierMap[identifier] = mappedIdentifier;
            }
            // 5 - Return the new blank node identifier.
            return mappedIdentifier;
        }

        private static string ContainerAsString(JsonLdContainer container)
        {
            switch (container)
            {
                case JsonLdContainer.Graph:
                    return "@graph";
                case JsonLdContainer.Type:
                    return "@type";
                case JsonLdContainer.Id:
                    return "@id";
                case JsonLdContainer.Index:
                    return "@index";
                case JsonLdContainer.Language:
                    return "@language";
                case JsonLdContainer.List:
                    return "@list";
                case JsonLdContainer.Set:
                    return "@set";
                default:
                    return "@none";
            }
        }

        private JObject CreateInverseContext(JsonLdContext activeContext)
        {
            // 1. Initialize result to an empty map.
            var result = new JObject();

            // 2. Initialize default language to @none. If the active context has a default language, set default language to the default language from the active context normalized to lower case.
            var defaultLanguage = "@none";
            if (activeContext.Language != null) defaultLanguage = activeContext.Language.ToLowerInvariant();

            // 3. For each key term and value term definition in the active context, ordered by shortest term first (breaking ties by choosing the lexicographically least term):
            foreach (var term in activeContext.Terms.OrderBy(t => t.Length).ThenBy(t => t))
            {
                var termDefinition = activeContext.GetTerm(term);
                // 3.1 - If the term definition is null, term cannot be selected during compaction, so continue to the next term.
                // KA: Term defniitions with a null IriMapping are not used for IRI expansion (or compaction) so they should be skipped too.
                if (termDefinition == null || termDefinition.IriMapping == null)
                {
                    continue;
                }
                // 3.2 - Initialize container to @none. If there is a container mapping in term definition, set container to its associated value.
                var container = termDefinition.ContainerMapping.Any()
                    ? termDefinition.ContainerMapping.Select(ContainerAsString).OrderBy(s => s)
                        .Aggregate(string.Empty, (seed, add) => seed + add)
                    : "@none";

                // 3.3 - Initialize var to the value of the IRI mapping for the term definition.
                var iri = termDefinition.IriMapping;

                // 3.4 - If var is not an entry of result, add an entry where the key is var and the value is an empty map to result.
                if (result.Property(iri) == null)
                {
                    result.Add(iri, new JObject());
                }

                // 3.5 - Reference the value associated with the iri member in result using the variable container map.
                var containerMap = result[iri] as JObject;

                // 3.6 - If container map has no container entry, create one and set its value to a new map with three entries.
                // The first entry is @language and its value is a new empty map, the second entry is @type and its value is a new empty map,
                // and the third entry is @any and its value is a new map with the entry @none set to the term being processed.
                if (containerMap.Property(container) == null)
                {
                    containerMap.Add(container, new JObject(
                        new JProperty("@language", new JObject()),
                        new JProperty("@type", new JObject()),
                        new JProperty("@any", new JObject(new JProperty("@none", term)))));
                }

                // 3.7 - Reference the value associated with the container member in container map using the variable type/language map.
                var typeLanguageMap = containerMap[container] as JObject;

                // 3.8 - Reference the value associated with the @type member in type/language map using the variable type map.
                var typeMap = typeLanguageMap["@type"] as JObject;

                // 3.9 - Reference the value associated with the @language entry in type/language map using the variable language map.
                var languageMap = typeLanguageMap["@language"] as JObject;

                // 3.10 - If the term definition indicates that the term represents a reverse property:
                if (termDefinition.Reverse)
                {
                    // 3.10.1 - If type map does not have an @reverse entry, create one and set its value to the term being processed.
                    if (typeMap.Property("@reverse") == null)
                    {
                        typeMap.Add("@reverse", term);
                    }
                }
                // 3. 11 - Otherwise, if term definition has a type mapping which is @none: 
                else if ("@none".Equals(termDefinition.TypeMapping))
                {
                    // 3.11.1 - If language map does not have an @any entry, create one and set its value to the term being processed.
                    if (!languageMap.ContainsKey("@any"))
                    {
                        languageMap.Add(new JProperty("@any", term));
                    }
                    // 3.11.2 - If type map does not have an @any entry, create one and set its value to the term being processed.
                    if (!typeMap.ContainsKey("@any"))
                    {
                        typeMap.Add(new JProperty("@any", term));
                    }
                }
                // 3.12 - Otherwise, if term definition has a type mapping:
                else if (termDefinition.TypeMapping != null)
                {
                    // 3.12.1 - If type map does not have an entry corresponding to the type mapping in term definition, create one and set its value to the term being processed.
                    if (typeMap.Property(termDefinition.TypeMapping) == null)
                    {
                        typeMap.Add(termDefinition.TypeMapping, term);
                    }
                }
                // 3.13 - Otherwise, if term definition has both a language mapping and a direction mapping: 
                else if (termDefinition.HasLanguageMapping && termDefinition.DirectionMapping.HasValue)
                {
                    string langDir;
                    if (termDefinition.LanguageMapping != null && termDefinition.DirectionMapping != LanguageDirection.Unspecified)
                    {
                        langDir = termDefinition.LanguageMapping.ToLowerInvariant() + "_" +
                                  SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                    } else if (termDefinition.LanguageMapping != null)
                    {
                        langDir = termDefinition.LanguageMapping.ToLowerInvariant();
                    }else if (termDefinition.DirectionMapping != LanguageDirection.Unspecified)
                    {
                        langDir = "_" + SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                    }
                    else
                    {
                        langDir = "@null";
                    }

                    if (!languageMap.ContainsKey(langDir))
                    {
                        languageMap.Add(new JProperty(langDir, term));
                    }
                }
                // 3.14 - Otherwise, if term definition has a language mapping (might be null):
                else if (termDefinition.HasLanguageMapping)
                {
                    // 3.14.1 - If the language mapping equals null, set language to @null; otherwise set it to the language code in language mapping,  normalized to lower case.
                    var language = termDefinition.LanguageMapping?.ToLowerInvariant() ?? "@null";
                    // 3.14.2 - If language map does not have a language member, create one and set its value to the term being processed.
                    if (languageMap.Property(language) == null)
                    {
                        languageMap.Add(language, term);
                    }
                }
                // 3.15 - Otherwise, if term definition has a direction mapping (might be null): 
                else if (termDefinition.DirectionMapping.HasValue)
                {
                    // 3.15.1 If the direction mapping equals null, set direction to @none; otherwise to direction mapping preceded by an underscore ("_").
                    var direction = termDefinition.DirectionMapping.Value == LanguageDirection.Unspecified
                        ? "@none"
                        : "_" + SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                    if (!languageMap.ContainsKey(direction))
                    {
                        languageMap.Add(new JProperty(direction, term));
                    }
                }
                // 3.16 - Otherwise, if active context has a default base direction: 
                else if (activeContext.BaseDirection.HasValue)
                {
                    // 3.16.1 - Initialize a variable lang dir with the concatenation of default language and default base direction, separate by an underscore ("_"), normalized to lower case.
                    var langDir = (activeContext.Language?.ToLowerInvariant() ?? string.Empty) + "_" +
                                  SerializeLanguageDirection(activeContext.BaseDirection.Value);
                    // 3.16.2 - If language map does not have a lang dir entry, create one and set its value to the term being processed.
                    if (!languageMap.ContainsKey(langDir))
                    {
                        languageMap.Add(new JProperty(langDir, term));
                    }
                    // 3.16.3 - If language map does not have an @none entry, create one and set its value to the term being processed.
                    if (!languageMap.ContainsKey("@none"))
                    {
                        languageMap.Add(new JProperty("@none", term));
                    }
                    // 3.16.4 - If type map does not have an @none entry, create one and set its value to the term being processed.
                    if (!typeMap.ContainsKey("@none"))
                    {
                        typeMap.Add(new JProperty("@none", term));
                    }
                }
                // 3.17 - Otherwise
                else
                {
                    // 3.17.1 - If language map does not have a default language entry (after being normalized to lower case), create one and set its value to the term being processed.
                    if (!languageMap.ContainsKey(defaultLanguage.ToLowerInvariant()))
                    {
                        languageMap.Add(new JProperty(defaultLanguage.ToLowerInvariant(), term));
                    }

                    // 3.17.2 - If language map does not have an @none member, create one and set its value to the term being processed.
                    if (!languageMap.ContainsKey("@none"))
                    {
                        languageMap.Add(new JProperty("@none", term));
                    }

                    // 3.17.3 - If type map does not have an @none member, create one and set its value to the term being processed.
                    if (!typeMap.ContainsKey("@none"))
                    {
                        typeMap.Add(new JProperty("@none", term));
                    }
                }
            }
            return result;
        }

        private string CompactIri(JsonLdContext activeContext, string iri, JToken value = null,
            bool vocab = false, bool reverse = false)
        {
            // 1 - If var is null, return null.
            // KA - note local name for var is iri to avoid clash with C# keyword
            if (iri == null) return null;

            // 2 - If the active context has a null inverse context, set inverse context in active context to the result of calling the Inverse Context Creation algorithm using active context.
            if (activeContext.InverseContext == null)
            {
                activeContext.InverseContext = CreateInverseContext(activeContext);
            }

            // Initialize inverse context to the value of inverse context in active context.
            var inverseContext = activeContext.InverseContext;

            // 4 - If vocab is true and var is an entry of inverse context:
            if (vocab && inverseContext.ContainsKey(iri))
            {
                var defaultLanguage = "@none";
                // 4.1 - Initialize default language based on the active context's default language, normalized to lower case and default base direction: 
                // 4.1.1 - If the active context's default base direction is not null, to the concatenation of the active context's default language and default base direction, separated by an underscore("_"), normalized to lower case.
                // 4.1.2 - Otherwise, to the active context's default language, if it has one, normalized to lower case, otherwise to @none.
                if (activeContext.BaseDirection.HasValue &&
                    activeContext.BaseDirection != LanguageDirection.Unspecified)
                {
                    defaultLanguage =
                        (SerializeLanguageDirection(activeContext.BaseDirection.Value) + "_" + activeContext.Language)
                        .ToLowerInvariant();
                }
                else if (!string.IsNullOrEmpty(activeContext.Language))
                {
                    defaultLanguage = activeContext.Language.ToLowerInvariant();
                }

                // 4.2 - If value is a map containing an @preserve entry, use the first element from the value of @preserve as value.
                if ((value is JObject valueMap) && valueMap.ContainsKey("@preserve"))
                {
                    value = valueMap["@preserve"];
                    if (value is JArray valueArray)
                    {
                        value = valueArray[0];
                    }
                }

                // 4.3 - Initialize containers to an empty array. This array will be used to keep track of an ordered list of preferred container mapping for a term, based on what is compatible with value. 
                var containers = new List<string>();
                // 4.4 - Initialize type/language to @language, and type/language value to @null. These two variables will keep track of the preferred type mapping or language mapping for a term, based on what is compatible with value.
                var typeLanguage = "@language";
                var typeLanguageValue = "@null";
                // 4.5 - If value is a map containing an @index entry, and value is not a graph object then append the values @index and @index@set to containers.
                if (value is JObject && (value as JObject).ContainsKey("@index") && !IsGraphObject(value))
                {
                    containers.Add("@index");
                    containers.Add("@index@set");
                }

                // 4.6 - If reverse is true, set type/language to @type, type/language value to @reverse, and append @set to containers.
                if (reverse)
                {
                    typeLanguage = "@type";
                    typeLanguageValue = "@reverse";
                    containers.Add("@set");
                }
                else if (IsListObject(value))
                {
                    // 4.7 - Otherwise, if value is a list object, then set type/language and type/language value to the most specific values that work for all items in the list as follows: 
                    var valueObject = value as JObject;
                    // 4.7.1 - If @index is not an entry in value, then append @list to containers.
                    if (!valueObject.ContainsKey("@index")) containers.Add("@list");
                    // 4.7.2 - Initialize list to the array associated with the @list entry in value.
                    var list = EnsureArray(valueObject["@list"]);
                    // 4.7.3 - Initialize common type and common language to null.If list is empty, set common language to default language.
                    string commonType = null;
                    string commonLanguage = null;
                    if (list.Count == 0)
                    {
                        commonLanguage = defaultLanguage;
                    }

                    // 4.7.4 - For each item in list:
                    foreach (var item in list)
                    {
                        var itemObject = item as JObject;
                        // 4.7.4.1 - Initialize item language to @none and item type to @none.
                        var itemLanguage = "@none";
                        var itemType = "@none";
                        // 4.7.4.2 - If item contains an @value entry:
                        if (itemObject != null && itemObject.ContainsKey("@value"))
                        {
                            if (itemObject.ContainsKey("@direction"))
                            {
                                if (itemObject.ContainsKey("@language"))
                                {
                                    itemLanguage =
                                        (itemObject["@language"].Value<string>() + "_" +
                                         itemObject["@direction"].Value<string>()).ToLowerInvariant();
                                }
                                else
                                {
                                    itemLanguage = "_" + itemObject["@direction"].Value<string>().ToLowerInvariant();
                                }
                            }
                            else if (itemObject.ContainsKey("@language"))
                            {
                                itemLanguage = itemObject["@language"].Value<string>().ToLowerInvariant();
                            }
                            else if (itemObject.ContainsKey("@type"))
                            {
                                itemType = itemObject["@type"].Value<string>();
                            }
                            else
                            {
                                itemLanguage = "@null";
                            }
                        }
                        // 4.7.4.3 - Otherwise, set item type to @id.
                        else
                        {
                            itemType = "@id";
                        }

                        // 4.7.4.4 - If common language is null, set common language to item language.
                        if (commonLanguage == null)
                        {
                            commonLanguage = itemLanguage;
                        }
                        else
                        {
                            // 4.7.4.5 - Otherwise, if item language does not equal common language and item contains a @value entry, then set common language to @none because list items have conflicting languages.
                            if (!itemLanguage.Equals(commonLanguage) && itemObject.ContainsKey("@value"))
                            {
                                commonLanguage = "@none";
                            }
                        }

                        // 4.7.4.6 - If common type is null, set common type to item type.
                        if (commonType == null)
                        {
                            commonType = itemType;
                        }
                        else
                        {
                            // 4.7.4.7 - Otherwise, if item type does not equal common type, then set common type to @none because list items have conflicting types.
                            if (!itemType.Equals(commonType))
                            {
                                commonType = "@none";
                            }
                        }

                        // 4.7.4.8 - If common language is @none and common type is @none, then stop processing items in the list because it has been detected that there is no common language or type amongst the items.
                        if ("@none".Equals(commonLanguage) && "@none".Equals(commonType)) break;
                    }

                    // 4.7.5 - If common language is null, set common language to @none.
                    if (commonLanguage == null) commonLanguage = "@none";
                    // 4.7.6 - If common type is null, set common type to @none.
                    if (commonType == null) commonType = "@none";
                    // 4.7.7 - If common type is not @none then set type / language to @type and type/ language value to common type.
                    if (!"@none".Equals(commonType))
                    {
                        typeLanguage = "@type";
                        typeLanguageValue = commonType;
                    }
                    else
                    {
                        // 4.7.8 - Otherwise, set type/ language value to common language.
                        typeLanguageValue = commonLanguage;
                    }
                }
                else if (IsGraphObject(value))
                {
                    // 4.8 - Otherwise, if value is a graph object, prefer a mapping most appropriate for the particular value. 
                    var valueObject = value as JObject;
                    // 4.8.1 - If value contains an @index entry, append the values @graph@index and @graph@index@set to containers.
                    if (valueObject != null && valueObject.ContainsKey("@index"))
                    {
                        containers.Add("@graph@index");
                        containers.Add("@graph@index@set");
                    }

                    // 4.8.2 - If value contains an @id entry, append the values @graph @id and @graph@id @set to containers.
                    if (valueObject != null && valueObject.ContainsKey("@id"))
                    {
                        containers.Add("@graph@id");
                        containers.Add("@graph@id@set");
                    }

                    // 4.8.3 - Append the values @graph @graph @set, and @set to containers.
                    containers.Add("@graph");
                    containers.Add("@graph@set");
                    containers.Add("@set");
                    // 4.8.4 - If value does not contain an @index entry, append the values @graph @index and @graph@index @set to containers.
                    if (valueObject == null || !valueObject.ContainsKey("@index"))
                    {
                        containers.Add("@graph@index");
                        containers.Add("@graph@index@set");

                    }

                    // 4.8.5 - If the value does not contain an @id entry, append the values @graph@id and @graph @id@set to containers.
                    if (valueObject == null || !valueObject.ContainsKey("@id"))
                    {
                        containers.Add("@graph@id");
                        containers.Add("@graph@id@set");
                    }

                    // 4.8.6 - Append the values @index and @index@set to containers.
                    containers.Add("@index");
                    containers.Add("@index@set");
                    // 4.8.7 - Set type / language to @type and set type / language value to @id.
                    typeLanguage = "@type";
                    typeLanguageValue = "@id";
                }
                // 4.9 - Otherwise
                else
                {
                    // 4.9.1 - If value is a value object: 
                    if (IsValueObject(value))
                    {
                        var valueObject = value as JObject;
                        // 4.9.1.1 - If value contains an @direction entry and does not contain an @index entry, then set type/language value to the concatenation of the value's @language entry (if any) and the value's @direction entry, separated by an underscore ("_"), normalized to lower case. Append @language and @language@set to containers.
                        if (valueObject.ContainsKey("@direction") && !valueObject.ContainsKey("@index"))
                        {
                            if (valueObject.ContainsKey("@language"))
                            {
                                typeLanguageValue =
                                    (valueObject["@language"].Value<string>() + "_" +
                                     valueObject["@direction"].Value<string>()).ToLowerInvariant();
                            }
                            else
                            {
                                typeLanguageValue = ("_" + valueObject["direction"].Value<string>()).ToLowerInvariant();
                            }

                            containers.Add("@language");
                            containers.Add("@language@set");
                        }
                        // 4.9.1.2 - Otherwise, if value contains an @language entry and does not contain an @index entry, then set type/language value to the value of @language normalized to lower case, and append @language, and @language@set to containers.
                        else if (valueObject.ContainsKey("@language") && !valueObject.ContainsKey("@index"))
                        {
                            typeLanguageValue = valueObject["@language"].Value<string>().ToLowerInvariant();
                            containers.Add("@language");
                            containers.Add("@language@set");
                        }
                        // 4.9.1.3 - Otherwise, if value contains an @type entry, then set type/language value to its associated value and set type/language to @type.
                        else if (valueObject.ContainsKey("@type"))
                        {
                            typeLanguageValue = valueObject["@type"].Value<string>();
                            typeLanguage = "@type";
                        }

                    }
                    // 4.9.2 - Otherwise, set type/language to @type and set type/language value to @id, and append @id, @id@set, @type, and @set@type, to containers.
                    else
                    {
                        typeLanguage = "@type";
                        typeLanguageValue = "@id";
                        containers.Add("@id");
                        containers.Add("@id@set");
                        containers.Add("@type");
                        containers.Add("@set@type");
                    }

                    // 4.9.3 - Append @set to containers.
                    containers.Add("@set");
                }

                // 4.10 - Append @none to containers. This represents the non-existence of a container mapping, and it will be the last container mapping value to be checked as it is the most generic.
                containers.Add("@none");
                if (_options.ProcessingMode != JsonLdProcessingMode.JsonLd10)
                {
                    // 4.11 - If processing mode is not json-ld-1.0 and value is not a map or does not contain an @index entry, append @index and @index@set to containers.
                    if (value == null || value.Type != JTokenType.Object || (value as JObject).ContainsKey("@index"))
                    {
                        containers.Add("@index");
                        containers.Add("@index@set");
                    }

                    // 4.12 - If processing mode is not json - ld - 1.0 and value is a map containing only an @value entry, append @language and @language@set to containers.
                    if (value is JObject valueObject &&
                        valueObject.Count == 1 && 
                        valueObject.ContainsKey("@value"))
                    {
                        containers.Add("@langauge");
                        containers.Add("@langauge@set");
                    }
                }

                // 4.13 - If type / language value is null, set type/ language value to @null.This is the key under which null values are stored in the inverse context entry.
                if (typeLanguageValue == null) typeLanguageValue = "@null";
                // 4.14 - Initialize preferred values to an empty array.This array will indicate, in order, the preferred values for a term's type mapping or language mapping.
                var preferredValues = new List<string>();
                // 4.15 - If type / language value is @reverse, append @reverse to preferred values.
                if (typeLanguageValue.Equals("@reverse"))
                {
                    preferredValues.Add("@reverse");
                }

                // 4.16 - type/language value is @id or @reverse and value is a map containing an @id entry: 
                if ((typeLanguageValue.Equals("@id") || typeLanguageValue.Equals("@reverse")) && (value is JObject) &&
                    (value as JObject).ContainsKey("@id"))
                {
                    // 4.16.1 If the result of IRI compacting the value of the @id entry in value has a term definition in the active context with an IRI mapping that equals the value of the @id entry in value, then append @vocab, @id, and @none, in that order, to preferred values.
                    var idValue = value["@id"].Value<string>();
                    var compactedId = CompactIri(activeContext, idValue, vocab: true);
                    if (activeContext.TryGetTerm(compactedId, out var td2) && td2.IriMapping.Equals(idValue))
                    {
                        preferredValues.Add("@vocab");
                        preferredValues.Add("@id");
                        preferredValues.Add("@none");
                    }
                    else
                    {
                        // 4.16.2 - Otherwise, append @id, @vocab, and @none, in that order, to preferred values.
                        preferredValues.Add("@id");
                        preferredValues.Add("@vocab");
                        preferredValues.Add("@none");
                    }
                }
                else
                {
                    // 4.17 - Otherwise, append type/language value and @none, in that order, to preferred values.
                    // If value is a list object with an empty array as the value of @list, set type/language to @any.
                    preferredValues.Add(typeLanguageValue);
                    preferredValues.Add("@none");
                    if (IsListObject(value) && (value["@list"] as JArray).Count == 0)
                    {
                        typeLanguage = "@any";
                    }
                }

                // 4.18 - Append @any to preferred values.
                preferredValues.Add("@any");
                // 4.19 - If preferred values contains any entry having an underscore ("_"), append the substring of that entry from the underscore to the end of the string to preferred values.
                var toAppend = preferredValues.Where(pv => pv.Contains("_"))
                    .Select(pv => pv.Substring(pv.IndexOf("_"))).ToList();
                preferredValues.AddRange(toAppend);
                // 4.20 - Initialize term to the result of the Term Selection algorithm, passing var, containers, type/language, and preferred values.
                var term = SelectTerm(activeContext, iri, containers, typeLanguage, preferredValues);
                if (term != null) return term;
            }

            // 5 - At this point, there is no simple term that var can be compacted to. If vocab is true and active context has a vocabulary mapping: 
            if (vocab && !string.IsNullOrEmpty(activeContext.Vocab))
            {
                if (iri.StartsWith(activeContext.Vocab) && iri.Length > activeContext.Vocab.Length)
                {
                    var suffix = iri.Substring(activeContext.Vocab.Length);
                    if (!activeContext.TryGetTerm(suffix, out _)) return suffix;
                }
            }

            // 6 - The var could not be compacted using the active context's vocabulary mapping.
            // Try to create a compact IRI, starting by initializing compact IRI to null. This variable will be used to store the created compact IRI, if any.
            string compactIri = null;
            // 7 - For each term definition definition in active context:
            foreach (var definitionKey in activeContext.Terms)
            {
                var termDefinition = activeContext.GetTerm(definitionKey);
                // 7.1 - If the IRI mapping of definition is null, its IRI mapping equals var, its IRI mapping is not a substring at the beginning of var, or definition does not have a true prefix flag, definition's key cannot be used as a prefix. Continue with the next definition.
                if (termDefinition.IriMapping == null ||
                    termDefinition.IriMapping.Equals(iri) ||
                    !iri.StartsWith(termDefinition.IriMapping) ||
                    !termDefinition.Prefix) continue;
                // 7.2 - Initialize candidate by concatenating definition key, a colon(:), and the substring of var that follows after the value of the definition's IRI mapping.
                var candidate = definitionKey + ":" + iri.Substring(termDefinition.IriMapping.Length);
                // 7.1 - If either compact IRI is null, candidate is shorter or the same length but lexicographically less than compact IRI and candidate does not have a term definition in active context, or if that term definition has an IRI mapping that equals var and value is null, set compact IRI to candidate.
                if (!activeContext.HasTerm(candidate) && (compactIri == null || candidate.Length < compactIri.Length ||
                                                          candidate.CompareTo(compactIri) < 0))
                {
                    compactIri = candidate;
                }
                else if (activeContext.HasTerm(candidate))
                {
                    var candidateTermDef = activeContext.GetTerm(candidate);
                    if (candidateTermDef.IriMapping.Equals(iri) && value == null)
                    {
                        compactIri = candidate;
                    }
                }
            }

            // 8 - If compact IRI is not null, return compact IRI.
            if (compactIri != null)
            {
                return compactIri;
            }

            // 9 - To ensure that the IRI var is not confused with a compact IRI, if the IRI scheme of var matches any term in active context with prefix flag set to true, and var has no IRI authority (preceded by double-forward-slash (//), an IRI confused with prefix error has been detected, and processing is aborted.
            var ix = iri.IndexOf(':');
            if (ix > 0 && iri.IndexOf("://") != ix)
            {
                var scheme = iri.Substring(0, ix);
                if (activeContext.TryGetTerm(scheme, out var td) && td.Prefix)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.IriConfusedWithPrefix,
                        $"IRI confused with prefix. The {iri} has a scheme that is confusable with a term in the JSON-LD context and no authority to disabmiguate it from a JSON-LD term.");
                }
            }

            // 10 - If vocab is false, transform var to a relative IRI reference using the base IRI from active context, if it exists.
            if (!vocab && activeContext.HasBase)
            {
                if (iri.StartsWith("_:"))
                {
                    // Just return a blank node identifier unchanged
                    return iri;
                }
                var parsedIri = new Uri(iri);
                var relativeIri = activeContext.Base.MakeRelativeUri(parsedIri);
                // KA: If IRI is equivalent to base IRI just return last path segment rather than an empty string
                if (string.Empty.Equals(relativeIri.ToString()))
                {
                    var lastSlashIx = parsedIri.PathAndQuery.LastIndexOf('/');
                    return parsedIri.PathAndQuery.Substring(lastSlashIx + 1);
                }
                return relativeIri.ToString();
            }

            // 11 - Finally, return var as is.
            return iri;
        }

        /// <summary>
        /// Implementation of the Term Selection algorithm.
        /// </summary>
        /// <param name="activeContext"></param>
        /// <param name="iri"></param>
        /// <param name="containers"></param>
        /// <param name="typeLanguage"></param>
        /// <param name="preferredValues"></param>
        /// <returns></returns>
        private string SelectTerm(JsonLdContext  activeContext, string iri, List<string> containers, string typeLanguage, List<string> preferredValues)
        {
            // 1 - If the active context has a null inverse context, set inverse context in active context to the result of calling the Inverse Context Creation algorithm using active context.
            if (activeContext.InverseContext == null)
            {
                activeContext.InverseContext = CreateInverseContext(activeContext);
            }

            // 2 - Initialize inverse context to the value of inverse context in active context.
            var inverseContext = activeContext.InverseContext;

            // 3 - Initialize container map to the value associated with iri in the inverse context.
            var containerMap = inverseContext[iri] as JObject;

            // 4 - For each item container in containers:
            foreach (var container in containers)
            {
                // 4.1 - If container is not an entry of container map, then there is no term with a matching container mapping for it, so continue to the next container.
                if (!containerMap.ContainsKey(container)) continue;

                // 4.2 - Initialize type/language map to the value associated with the container entry in container map.
                var typeLanguageMap = containerMap[container];

                // 4.3 - Initialize value map to the value associated with type/language entry in type/language map.
                var valueMap = typeLanguageMap[typeLanguage] as JObject;

                // 4.4 - For each item in preferred values:
                foreach (var item in preferredValues)
                {
                    // 4.4.1 - If item is not an entry of value map, then there is no term with a matching type mapping or language mapping, so continue to the next item.
                    if (!valueMap.ContainsKey(item)) continue;
                    // 4.4.2 - Otherwise, a matching term has been found, return the value associated with the item member in value map.
                    return valueMap[item].Value<string>();
                }
            }
            // 5 - No matching term has been found. Return null.
            return null;
        }

        private JToken CompactValue(JsonLdContext activeContext, string activeProperty, JObject value)
        {
            var activeTermDefinition = activeProperty == null ? null : activeContext.GetTerm(activeProperty);

            // 1 - Initialize result to a copy of value.
            var result = value.DeepClone();

            // 2 - If the active context has a null inverse context, set inverse context in active context to the result of calling the
            // Inverse Context Creation algorithm using active context.
            if (activeContext.InverseContext == null)
            {
                activeContext.InverseContext = CreateInverseContext(activeContext);
            }

            // 3 - Initialize inverse context to the value of inverse context in active context.
            var inverseContext = activeContext.InverseContext;


            // 4 - Initialize language to the language mapping for active property in active context, if any, otherwise to the default language of active context.
            // 5 - Initialize direction to the direction mapping for active property in active context, if any, otherwise to the default base direction of active context.
            var languageMapping = activeContext.Language;
            var direction = activeContext.BaseDirection;
            if (activeTermDefinition != null)
            {
                if (activeTermDefinition.HasLanguageMapping) languageMapping = activeTermDefinition.LanguageMapping;
                if (activeTermDefinition.DirectionMapping.HasValue) direction = activeTermDefinition.DirectionMapping;
            }

            var activeTermDefinitionTypeMapping = activeTermDefinition?.TypeMapping ?? null;
            var valueHasType = value.ContainsKey("@type");
            var valueType = valueHasType && value["@type"].Type == JTokenType.String ? value["@type"].Value<string>() : null;
            
            // 6 - If value has an @id entry and has no other entries other than @index:
            if (value.ContainsKey("@id") &&
                value.Properties().All(p => p.Name.Equals("@id") || p.Name.Equals("@index")))
            {
                var typeMapping = activeTermDefinition != null ? activeTermDefinition.TypeMapping : null;
                if (typeMapping != null)
                {
                    // 6.1 - If the type mapping of active property is set to @id, set result to the result of IRI compacting the value associated with the @id entry using false for vocab.
                    // 6.2 - Otherwise, if the type mapping of active property is set to @vocab, set result to the result of IRI compacting the value associated with the @id entry.
                    if (typeMapping.Equals("@id") || typeMapping.Equals("@vocab"))
                    {
                        result = CompactIri(activeContext, value["@id"].Value<string>(),
                            vocab: typeMapping.Equals("@vocab"));
                    }
                }
            }
            // 7 - Otherwise, if value has an @type entry whose value matches the type mapping of active property, set result to the value associated with the @value entry of value.
            else if (valueType != null && valueType.Equals(activeTermDefinitionTypeMapping))
            {
                result = value["@value"];
            }
            // 8 - Otherwise, if the type mapping of active property is @none, or value has an @type entry, and the value of @type in value does not match the type mapping of active property, leave value as is, as value compaction is disabled.
            else if ("@none".Equals(activeTermDefinitionTypeMapping) || 
                     valueHasType && (valueType == null || valueType != activeTermDefinitionTypeMapping))
            {
                // 8.1 - Replace any value of @type in result with the result of IRI compacting the value of the @type entry.
                if (result is JObject resultObject && resultObject.ContainsKey("@type"))
                {
                    var typeValue = resultObject["@type"];
                    if (typeValue is JArray typeArray)
                    {
                        var newArray = new JArray();
                        foreach (var item in typeArray)
                        {
                            newArray.Add(CompactIri(activeContext, item.Value<string>(), vocab:true));
                        }

                        resultObject["@type"] = newArray;
                    }
                    else
                    {
                        resultObject["@type"] = CompactIri(activeContext, typeValue.Value<string>(), vocab: true);
                    }
                }
            }
            // 9 - Otherwise, if the value of the @value entry is not a string:
            else if (value.ContainsKey("@value") && value["@value"].Type != JTokenType.String)
            {
                // 9.1 - If value has an @index entry, and the container mapping associated to active property includes @index, or if value has no @index entry, set result to the value associated with the @value entry.
                if ((value.ContainsKey("@index") && activeTermDefinition != null &&
                     activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.Index)) ||
                    (!value.ContainsKey("@index")))
                {
                    result = value["@value"];
                }
            }
            // 10 - Otherwise, if value has an @language entry whose value exactly matches language, using a case-insensitive comparison if it is not null, or is not present, if language is null, and the value has an @direction entry whose value exactly matches direction, if it is not null, or is not present, if direction is null:
            else if (SafeEquals(languageMapping, value.ContainsKey("@language")? value["@language"]: null, StringComparison.OrdinalIgnoreCase) &&
                     SafeEquals(direction.HasValue ? SerializeLanguageDirection(direction.Value) : null,
                         value.ContainsKey("@direction") ? value["@direction"] : null, StringComparison.OrdinalIgnoreCase))
            {
                // 10.1 - If value has an @index entry, and the container mapping associated to active property includes @index, or value has no @index entry, set result to the value associated with the @value entry.
                if ((value.ContainsKey("@index") && activeTermDefinition != null &&
                     activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.Index)) ||
                    (!value.ContainsKey("@index")))
                {
                    result = value["@value"];
                }
            }
            // 11 - If result is a map, replace each key in result with the result of IRI compacting that key.
            if (result is JObject r)
            {
                foreach (var p in r.Properties().ToList())
                {
                    var compactKey = CompactIri(activeContext, p.Name, vocab: true);
                    if (!compactKey.Equals(p.Name))
                    {
                        r.Remove(p.Name);
                        r.Add(new JProperty(compactKey, p.Value));
                    }
                }
            }
            // 12 - Return result
            return result;
        }

        /// <summary>
        /// Compare a possibly null string and a possibly null JToken for equality
        /// </summary>
        /// <remarks>If <paramref name="str"/> is null, return true if <paramref name="t"/> is null and fale if it is not null.
        /// If <paramref name="str"/> is not null, return true if <paramref name="t"/> is a non-null token of type string and the string value of <paramref name="t"/>
        /// matches the value of <paramref name="str"/> using the comparison method specified by the <paramref name="comparisonOptions"/> parameter.</remarks>
        /// <param name="str"></param>
        /// <param name="t"></param>
        /// <param name="comparisonOptions"></param>
        /// <returns></returns>
        private static bool SafeEquals(string str, JToken t, StringComparison comparisonOptions)
        {
            if (str == null)
            {
                return t == null || t.Type == JTokenType.Null;
            }

            return t != null && t.Type == JTokenType.String && t.Value<string>().Equals(str, comparisonOptions);
        }

        /*
        /// <summary>
        /// Applies the JSON-LD Framing algorithm to the specified input JSON object.
        /// </summary>
        /// <param name="input">The RDF data to be framed as a JSON-LD document.</param>
        /// <param name="frame">The JSON-LD frame to be applied.</param>
        /// <param name="options">Processor options.</param>
        /// <returns>A JSON object representing the framed RDF data.</returns>
        public static JObject Frame(JToken input, JToken frame, JsonLdProcessorOptions options)
        {
            options.ProcessingMode = JsonLdProcessingMode.JsonLd11FrameExpansion;
            var expandedFrame = Expand(frame, options);
            options.ProcessingMode = JsonLdProcessingMode.JsonLd11;
            var expandedInput = Expand(input, options);
            var frameProcessor = new JsonLdProcessor(options);
            var context = frame["@context"] ?? new JObject();
            if (frame is JObject && frame["@graph"] != null) options.FrameDefault = true;
            return frameProcessor.FrameAlgorithm(expandedInput, expandedFrame, context);
        }

        private JObject FrameAlgorithm(JToken expandedInput, JToken expandedFrame, JToken context)
        {
            // If an error is detected in the expanded frame, a invalid frame error has been detected and processing is aborted. Need more specifics as to what constitutes a valid frame.
            ValidateFrame(expandedFrame);

            // Set graph map to the result of performing the Node Map Generation algorithm on expanded input.
            var graphMap = GenerateNodeMap(expandedInput, _options);

            // If the frameDefault option is present with the value true, set graph name to @default. 
            // Otherwise, create merged node map using the the Merge Node Maps algorithm with graph map and add merged node map as the value of @merged in graph map and set graph name to @merged.
            var graphName = _options.FrameDefault ? "@default" : "@merged";
            if (!_options.FrameDefault)
            {
                var mergedNodeMap = MergeNodeMaps(graphMap);
                graphMap["@merged"] = mergedNodeMap;
            }
            var framingState = new FramingState(_options, graphMap, graphName);
            var results = new JArray();
            ProcessFrame(framingState, framingState.Subjects.Properties().Select(p => p.Name).ToList(), expandedFrame,
                results, null);

            // If the pruneBlankNodeIdentifiers is true, remove the @id member of each node object where the member value is a blank node identifier which appears only once in any property value within result.
            if (_options.PruneBlankNodeIdentifiers)
            {
                PruneBlankNodeIdentifiers(results);
            }

            // Using result from the recursive algorithm, set compacted results to the result of using the compact method using results, context, and options.
            var compactedResults = CompactWrapper(context, new JObject(), null, results, _options.CompactArrays);
            //var compactedResults = Compact(results, context, _options);

            // If compacted results does not have a top-level @graph keyword, or if its value is not an array, modify compacted results to place the non @context properties of compacted results into a dictionary contained within the array value of @graph.
            if (compactedResults["@graph"] == null)
            {
                var compactedResultContext = compactedResults["@context"];
                compactedResults.Remove("@context");
                var updatedResults = new JObject(
                    new JProperty("@graph", 
                    compactedResults.Properties().Any() ? new JArray(compactedResults) : new JArray()));
                if (compactedResultContext != null)
                {
                    updatedResults.Add("@context", compactedResultContext);
                }
                compactedResults = updatedResults;
            }

            // Recursively, replace all key-value pairs in compacted results where the key is @preserve with the value from the key-pair. If the value from the key-pair is @null, replace the value with null. If, after replacement, an array contains only the value null remove the value, leaving an empty array.
            var jsonLdContext = ProcessContext(new JsonLdContext(), context);
            ReplacePreservedValues(compactedResults, jsonLdContext, _options.CompactArrays);
            if (compactedResults["@graph"].Type != JTokenType.Array)
            {
                compactedResults["@graph"] = new JArray(compactedResults["@graph"]);
            }
            return compactedResults;
        }
        */
        /*
        private void ReplacePreservedValues(JToken token, JsonLdContext context, bool compactArrays)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var o = token as JObject;
                    if (o["@preserve"] != null)
                    {
                        var parent = o.Parent;
                        var preserveValue = o["@preserve"];
                        if (preserveValue.Type == JTokenType.String && preserveValue.Value<string>().Equals("@null"))
                        {
                            // KA - although spec says only drop null if all elements are null, playground impl and unit test indicate otherwise...
                            // o.Replace(null);
                            //if (parent is JArray && parent.All(x => x.Type == JTokenType.Null))
                            //{
                            //    parent.Replace(new JArray());
                            //}
                            if (parent is JArray)
                            {
                                o.Remove();
                            }
                            else
                            {
                                o.Replace(null);
                            }
                        }
                        else
                        {
                            o.Replace(preserveValue);
                        }
                    }
                    foreach (var p in o)
                    {
                        ReplacePreservedValues(p.Value, context, compactArrays);
                    }
                    break;
                case JTokenType.Array:
                    var a = token as JArray;
                    foreach (var item in a.ToList())
                    {
                        ReplacePreservedValues(item, context, compactArrays);
                    }
                    
                    if (compactArrays && a.Count == 1)
                    {
                        var parentProperty = a.Parent as JProperty;
                        if (parentProperty != null)
                        {
                            var termDefinition = context.GetTerm(parentProperty.Name);
                            var expandedName = termDefinition?.TypeMapping ?? parentProperty.Name;
                            if (expandedName != "@graph" && 
                                expandedName != "@list" && 
                                (termDefinition == null || termDefinition.ContainerMapping == JsonLdContainer.Null))
                            {
                                a.Replace(a[0]);
                            }
                        }
                    }
                    break;
            }
        }
        */
        private void PruneBlankNodeIdentifiers(JToken token)
        {
            var objectMap = new Dictionary<string, BlankNodeMapEntry>();
            GenerateBlankNodeMap(objectMap, token, null);
            foreach (var mapEntry in objectMap)
            {
                if (!mapEntry.Value.IsReferenced)
                {
                    PruneBlankNodeIdentifier(mapEntry.Key, mapEntry.Value.IdProperty);
                }
            }
        }

        private static void PruneBlankNodeIdentifier(string id, JProperty toUpdate)
        {
            if (toUpdate.Value.Type == JTokenType.String)
            {
                toUpdate.Remove();
            }
            else if (toUpdate.Value is JArray)
            {
                foreach (var item in (JArray) toUpdate.Value)
                {
                    if (item.Value<string>().Equals(id))
                    {
                        item.Remove();
                        break;
                    }
                }
            }
        }
        private class BlankNodeMapEntry
        {
            public bool IsReferenced;
            public JProperty IdProperty;
        }

        private void GenerateBlankNodeMap(Dictionary<string, BlankNodeMapEntry> objectMap, JToken token, JProperty activeProperty)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    var str = token.Value<string>();
                    if (IsBlankNodeIdentifier(str))
                    {
                        BlankNodeMapEntry mapEntry;
                        if (!objectMap.TryGetValue(str, out mapEntry))
                        {
                            mapEntry = new BlankNodeMapEntry();
                            objectMap[str] = mapEntry;
                        }
                        if (activeProperty.Name == "@id" && mapEntry.IdProperty == null)
                        {
                            mapEntry.IdProperty = activeProperty;
                        }
                        else
                        {
                            mapEntry.IsReferenced = true;
                        }
                    }
                    break;
                case JTokenType.Array:
                    foreach (var item in (token as JArray))
                    {
                        GenerateBlankNodeMap(objectMap, item, activeProperty);
                    }
                    break;
                case JTokenType.Object:
                    foreach (var p in (token as JObject).Properties())
                    {
                        if (p.Name == "@value") continue;
                        GenerateBlankNodeMap(objectMap, p.Value, p);
                    }
                    break;
            }
        }

        /*
        private void ProcessFrame(FramingState state, List<string> subjects, JToken frame, JToken parent,
            string activeProperty, Stack<string> idStack = null)
        {
            // Stack to track circular references when processing embedded nodes
            if (idStack == null) idStack = new Stack<string>();

            // 1 - If frame is an array, set frame to the first member of the array, which must be a valid frame.
            var frameArray = frame as JArray;
            if (frameArray !=null)
            {
                frame = frameArray.Count == 0 ? new JObject() : frameArray[0];
                ValidateFrame(frame);
            }
            var frameObject = frame as JObject;

            // 2 - Initialize flags embed, explicit, and requireAll from object embed flag, explicit inclusion flag, and require all flag in state overriding from any property values for @embed, @explicit, and @requireAll in frame.
            var embed = GetEmbedOption(frameObject, state.Embed);
            var explicitFlag = GetBooleanOption(frameObject, "@explicit", state.ExplicitInclusion);
            var requireAll = GetBooleanOption(frameObject, "@requireAll", state.RequireAll);

            // 3 - Create a list of matched subjects by filtering subjects against frame using the Frame Matching algorithm with state, subjects, frame, and requireAll.
            var matchedSubjects = MatchFrame(state, subjects, frameObject, requireAll);

            // 4 - Set link the the value of link in state associated with graph name in state, creating a new empty dictionary, if necessary.
            if (state.Link[state.GraphName] == null)
            {
                state.Link[state.GraphName] = new JObject();
            }
            var link = state.Link[state.GraphName] as JObject;

            // 5 - For each id and associated node object node from the set of matched subjects, ordered by id:
            foreach (var match in matchedSubjects.OrderBy(x=>x.Key))
            {
                var id = match.Key;
                var node = match.Value;

                // 5.1 - Initialize output to a new dictionary with @id and id and add output to link associated with id.
                var output = new JObject(new JProperty("@id", id));
                link[id] = output;

                // 5.2 - If embed is @link and id is in link, node already exists in results. Add the associated node object from link to parent and do not perform additional processing for this node.
                if (embed == JsonLdEmbed.Link && link[id] != null)
                {
                    FramingAppend(parent, node, activeProperty);
                    continue;
                }
                // 5.3 - Otherwise, if embed is @never or if a circular reference would be created by an embed, add output to parent and do not perform additional processing for this node.
                else if (embed == JsonLdEmbed.Never || idStack.Contains(id))
                {
                    FramingAppend(parent, output, activeProperty);
                    continue;
                }
                // 5.4 - Otherwise, if embed is @last, remove any existing embedded node from parent accociate with graph name in state. Requires sorting of subjects. We could consider @sample, to embed just the first matched node. With sorting, we could also consider @first.
                else if (embed == JsonLdEmbed.Last)
                {
                    var parentObject = parent as JObject;
                    if (parentObject != null)
                    {
                        parentObject.Remove(state.GraphName);
                    }
                }
                // 5.5 - If embed is @last or @always
                if (embed == JsonLdEmbed.Last || embed == JsonLdEmbed.Always)
                {
                    idStack.Push(id);
                    // 5.5.1 - If graph map in state has an entry for id:
                    if (state.GraphMap[id] != null)
                    {
                        bool recurse = false;
                        JObject subframe = null;
                        // 5.5.1.1 - If frame does not have the key @graph, set recurse to true, unless graph name in state is @merged and set subframe to a new empty dictionary.
                        if (frame["@graph"] == null)
                        {
                            recurse = !state.GraphName.Equals("@merged");
                            subframe = new JObject();
                        }
                        // 5.5.1.2 - Otherwise, set subframe to the first entry for @graph in frame, or a new empty dictionary, if it does not exist, and set recurse to true, unless id is @merged or @default.
                        else
                        {
                            var graphEntry = (frame["@graph"] as JArray);
                            if (graphEntry == null)
                            {
                                graphEntry = new JArray(new JObject());
                                frame["@graph"] = graphEntry;
                            } else if (graphEntry.Count == 0)
                            {
                                graphEntry.Add(new JObject());
                            }
                            subframe = graphEntry[0] as JObject;
                            recurse = !(id.Equals("@merged") || id.Equals("@default"));
                        }
                        // 5.5.1.3 - If recurse is true:
                        if (recurse)
                        {
                            // 5.5.1.3.1 - Push graph name from state onto graph stack in state.
                            state.GraphStack.Push(state.GraphName);
                            // 5.5.1.3.2 - Set the value of graph name in state to id.
                            state.GraphName = id;
                            // 5.5.1.3.3 - Invoke the recursive algorithm using state, the keys from the graph map in state associated with id as subjects, subframe as frame, output as parent, and @graph as active property.
                            ProcessFrame(state,
                                (state.GraphMap[state.GraphName] as JObject).Properties().Select(p => p.Name).ToList(),
                                subframe, output, "@graph", idStack);
                            // 5.5.1.3.4 - Pop the value from graph stack in state and set graph name in state back to that value.
                            state.GraphName = state.GraphStack.Pop();
                        }
                    }

                    // 5.5.2 - For each property and objects in node, ordered by property:
                    foreach (var p in node.Properties().OrderBy(p=>p.Name))
                    {
                        var property = p.Name;
                        // Skip the @id property as it is already copied
                        if (property.Equals("@id")) continue;
                        var objects = p.Value as JArray;
                        
                        // 5.5.2.1 - If property is a keyword, add property and objects to output.
                        if (IsKeyword(property))
                        {
                            output[property] = objects;
                            continue;
                        }
                        // 5.5.2.2 - Otherwise, if property is not in frame, and explicit is true, processors must not add any values for property to output, and the following steps are skipped.
                        else if (frame[property] == null && explicitFlag)
                        {
                            continue;
                        }
                        // 5.5.2.3 - For each item in objects:
                        foreach (var item in objects)
                        {
                            // 5.5.2.3.1 - If item is a dictionary with the property @list, 
                            // then each listitem in the list is processed in sequence and 
                            // added to a new list dictionary in output:
                            if (IsListObject(item))
                            {
                                var list = new JObject();
                                output[property] = new JArray(list); // KA: Not sure what the correct key is for the list object
                                foreach (var listItem in item["@list"] as JArray)
                                {
                                    // If listitem is a node reference, invoke the recursive algorithm using state, 
                                    // the value of @id from listitem as the sole member of a new subjects array, 
                                    // the first value from @list in frame as frame, list as parent, 
                                    // and @list as active property. 
                                    // If frame does not exist, create a new frame using a new dictionary 
                                    // with properties for @embed, @explicit and @requireAll taken from embed, explicit and requireAll. 
                                    if (IsNodeReference(listItem))
                                    {
                                        var listFrames = frame["@list"] as JArray;
                                        var listFrame = listFrames?[0] as JObject;
                                        if (listFrame == null)
                                        {
                                            listFrame = MakeFrameObject(embed, explicitFlag, requireAll);
                                        }
                                        ProcessFrame(state,
                                            new List<string> {listItem["@id"].Value<string>()},
                                            listFrame,
                                            list,
                                            "@list",
                                            idStack);
                                    }
                                    // 5.5.2.3.1.2 - Otherwise, append a copy of listitem to @list in list.
                                    else
                                    {
                                        if (list["@list"] == null)
                                        {
                                            list["@list"] = new JArray();
                                        }
                                        (list["@list"] as JArray).Add(listItem.DeepClone());
                                    }
                                }
                            }
                            // 5.5.2.3.2 - If item is a node reference, invoke the recursive algorithm using state, 
                            // the value of @id from item as the sole member of a new subjects array, 
                            // the first value from property in frame as frame, output as parent, and property as active property. 
                            // If frame does not exist, create a new frame using a new dictionary with properties for @embed, @explicit and @requireAll taken from embed, explicit and requireAll.
                            else if (IsNodeReference(item))
                            {
                                var newFrame = ((frame[property] as JArray)?[0]) as JObject ??
                                               MakeFrameObject(embed, explicitFlag, requireAll);
                                ProcessFrame(state,
                                    new List<string> {item["@id"].Value<string>()},
                                    newFrame,
                                    output,
                                    property,
                                    idStack);
                            }
                            // 5.5.2.3.3 - Otherwise, append a copy of item to active property in output.
                            else
                            {
                                // KA - should only append if value pattern matches?
                                if (frame[property] == null || frame[property][0]["@value"] == null || ValuePatternMatch(frame[property], item))
                                {
                                    FramingAppend(output, item, property);
                                }
                            }
                        }
                    }

                    // 5.5.3 - For each non-keyword property and objects in frame that is not in output:
                    foreach (var frameProperty in frameObject.Properties())
                    {
                        var property = frameProperty.Name;
                        if (output[property] != null) continue;
                        if (IsKeyword(property) || IsFramingKeyword(property)) continue;
                        var objects = frameProperty.Value as JArray;
                        if (objects == null || objects.Count == 0)
                        {
                            // Initialise as an array containing an empty frame
                            objects = new JArray(new JObject());
                        }
                        // 5.5.3.1 - Let item be the first element in objects, which must be a frame object.
                        var item = objects[0];
                        ValidateFrame(item);
                        // 5.5.3.2 - Set property frame to the first item in objects or a newly created frame object if value is objects. property frame must be a dictionary.
                        var propertyFrame = objects[0] as JObject; // KA - ncomplete as I can't make sense of the spec algorithm here
                        // 5.5.3.3 - Skip property and property frame if property frame contains @omitDefault with a value of true, or does not contain @omitDefault and the value of the omit default flag is true.
                        var frameOmitDefault = GetBooleanOption(propertyFrame, "@omitDefault", state.OmitDefault);
                        if (frameOmitDefault)
                        {
                            continue;
                        }
                        // 5.5.3.4 - Add property to output with a new dictionary having a property @preserve and a value that is a copy of the value of @default in frame if it exists, or the string @null otherwise.
                        var defaultValue = propertyFrame["@default"] ?? new JArray("@null");
                        if (!(defaultValue is JArray)) defaultValue = new JArray(defaultValue);
                        FramingAppend(output, new JObject(new JProperty("@preserve", defaultValue)), property);
                        //output[property] = new JObject(new JProperty("@preserve", frame["@default"] ?? "@null"));
                    }

                    // 5.5.4 - If frame has the property @reverse, then for each reverse property and sub frame that are the values of @reverse in frame:
                    if (frame["@reverse"] != null)
                    {
                        foreach(var rp in (frame["@reverse"] as JObject).Properties())
                        {
                            var reverseProperty = rp.Name;
                            var subframe = rp.Value;
                            // 5.5.4.1 - Create a @reverse property in output with a new dictionary reverse dict as its value.
                            var reverseDict = new JObject();
                            output["@reverse"] = reverseDict;
                            // 5.5.4.2 - For each reverse id and node in the map of flattened subjects that has the property reverse property containing a node reference with an @id of id:
                            foreach (var p in state.Subjects.Properties())
                            {
                                var n = p.Value as JObject;
                                var reversePropertyValues = n[reverseProperty] as JArray;
                                if (reversePropertyValues == null) continue;
                                if (reversePropertyValues.Any(x => x["@id"]?.Value<string>().Equals(id) == true))
                                {
                                    // 5.5.4.2.1 - Add reverse property to reverse dict with a new empty array as its value.
                                    var reverseId = p.Name;
                                    if (reverseDict[reverseProperty] == null)
                                        reverseDict[reverseProperty] = new JArray();
                                    // 5.5.4.2.2 - Invoke the recursive algorithm using state, the reverse id as the sole member of a new subjects array, sub frame as frame, null as active property, and the array value of reverse property in reverse dict as parent.
                                    ProcessFrame(state, 
                                        new List<string> {reverseId}, 
                                        subframe,
                                        reverseDict[reverseProperty], 
                                        null,
                                        idStack);
                                }
                            }
                        }
                    }

                    // 5.5.5 - Once output has been set are required in the previous steps, add output to parent.
                    FramingAppend(parent, output, activeProperty);

                    idStack.Pop();
                }
            }
        }
        */

        private static JObject MakeFrameObject(JsonLdEmbed embed, bool explicitFlag, bool requireAll)
        {
            JObject listFrame;
            listFrame = new JObject(
                new JProperty("@embed", JsonLdEmbedAsString(embed)),
                new JProperty("@explicit", explicitFlag),
                new JProperty("@requireAll", requireAll));
            return listFrame;
        }

        private void FramingAppend(JToken parent, JToken child, string activeProperty)
        {
            if (parent is JArray)
            {
                if (activeProperty != null)
                {
                    throw new ArgumentException("activeProperty must be null when parent is an array");
                }
                (parent as JArray).Add(child);
            }
            else if (parent is JObject)
            {
                if (string.IsNullOrEmpty(activeProperty))
                {
                    throw new ArgumentException(
                        "activeproperty must be a non-null value when the parent is a JSON object",
                        nameof(activeProperty));
                }
                var array = parent[activeProperty] as JArray;
                if (array == null)
                {
                    parent[activeProperty] = (array = new JArray());
                }
                array.Add(child);
            }
        }

        private Dictionary<string, JObject> MatchFrame(FramingState state, IList<string> subjects, JObject frame,
            bool requireAll)
        {
            var matches = new Dictionary<string, JObject>();

            // Prepare frame lookups
            var idMatches = frame["@id"] as JArray;
            var typeMatches = frame["@type"] as JArray;
            var propertyMatches = new Dictionary<string, JToken>();
            foreach (var p in frame.Properties())
            {
                if (IsKeyword(p.Name)) continue;
                propertyMatches[p.Name] = p.Value as JArray;
            }

            // First match on the @id property if it is defined in frame
            if (idMatches != null)
            {
                foreach (var id in idMatches.Select(v=>v.Value<string>()))
                {
                    var match = state.Subjects[id] as JObject;
                    if (match != null) matches.Add(id, match);
                }
            }

            foreach (var subject in subjects)
            {
                if (matches.ContainsKey(subject))
                {
                    // Already matched on @id
                    continue;
                }
                var node = state.Subjects[subject] as JObject;
                if (node == null) continue;
                if (typeMatches != null)
                {
                    // Match on @type
                    var nodeTypes = node["@type"] as JArray;
                    if (nodeTypes != null)
                    {
                        if 
                        (IsWildcard(typeMatches) && nodeTypes.Any() ||
                         nodeTypes.Count == 0 && typeMatches.Count == 0 ||
                         nodeTypes.Any(x => typeMatches.Any(y => y.Value<string>().Equals(x.Value<string>()))))
                        {
                            // Matched on @type
                            matches.Add(subject, node);
                        }
                        continue;
                    }
                    if (typeMatches.Count == 0)
                    {
                        // Matched on @type = match none
                        matches.Add(subject, node);
                        continue;
                    }
                }
                if (propertyMatches.Count == 0 && idMatches == null && typeMatches == null)
                {
                    matches.Add(subject, node);
                    continue;
                }
                // If requireAll, start assuming that there is a match and break when disproven
                // If !requireAll, start assuming that there is no match and break when disproven
                bool match = requireAll;
                bool hasNonDefaultMatch = false;
                foreach (var pm in propertyMatches)
                {
                    var propertyMatch = MatchProperty(state, node, pm.Key, pm.Value, requireAll);
                    if (propertyMatch == MatchType.Abort)
                    {
                        match = false;
                        break;
                    }
                    if (propertyMatch == MatchType.NoMatch)
                    {
                        if (requireAll)
                        {
                            match = false;
                            break;
                        }
                    } else if (propertyMatch == MatchType.Match)
                    {
                        hasNonDefaultMatch = true;
                        if (!requireAll)
                        {
                            match = true;
                            break;
                        }
                    }
                    // A default match is inconclusive until the end of the loop
                }
                if (match && hasNonDefaultMatch)
                {
                    matches.Add(subject, node);
                }
            }
            return matches;
        }


        private MatchType MatchProperty(FramingState state, JObject node, string property, JToken frameValue, bool requireAll)
        {
            var nodeValues = node[property] as JArray;
            var frameArray = frameValue as JArray;
            bool isMatchNone = false, isWildcard = false, hasDefault = false;
            if (frameArray.Count == 0)
            {
                isMatchNone = true;
            } else if (frameArray.Count == 1 && IsWildcard(frameArray[0]))
            {
                isWildcard = true;
            }
            else if (frameArray.Count == 1 && frameArray[0]["@default"] != null)
            {
                hasDefault = true;
            }
            if (nodeValues == null || nodeValues.Count == 0)
            {
                if (hasDefault) return MatchType.DefaultMatch;
            }

            // Non-existant property cannot match frame
            if (nodeValues == null) return MatchType.NoMatch;

            // Frame specifies match none - nodeValues must be empty
            if (isMatchNone && nodeValues.Count != 0) return MatchType.Abort;

            // Frame specifies match wildcard - nodeValues must be non-empty
            if (isWildcard && nodeValues.Count > 0) return MatchType.Match;

            if (IsValueObject(frameArray[0]))
            {
                // frameArray is a value pattern array
                foreach (var valuePattern in frameArray)
                {
                    foreach (var value in nodeValues)
                    {
                        if (ValuePatternMatch(valuePattern, value))
                        {
                            return MatchType.Match;
                        }
                    }
                }
                return MatchType.NoMatch;
            }

            // frameArray is a node pattern array
            var valueSubjects = nodeValues.Where(x=>x["@id"]!=null).Select(x => x["@id"].Value<string>()).ToList();
            if (valueSubjects.Any())
            {
                foreach (var subframe in frameArray)
                {
                    var matchedSubjects = MatchFrame(state, valueSubjects, subframe as JObject, requireAll);
                    if (matchedSubjects.Any()) return MatchType.Match;
                }
            }

            return hasDefault ? MatchType.Match : MatchType.NoMatch;
        }

        private bool ValuePatternMatch(JToken valuePattern, JToken value)
        {
            if (valuePattern is JArray) valuePattern = ((JArray) valuePattern)[0];
            var valuePatternObject = valuePattern as JObject;
            var valueObject = value as JObject;
            if (valuePatternObject == null || valueObject == null) return false;
            if (valuePatternObject.Count == 0)
            {
                // Pattern is wildcard
                return true;
            }
            var v1 = valueObject["@value"];
            var t1 = valueObject["@type"];
            var l1 = valueObject["@language"];
            var v2 = valuePatternObject["@value"];
            var t2 = valuePatternObject["@type"];
            var l2 = valuePatternObject["@language"];
            return ValuePatternTokenMatch(v2, v1) && ValuePatternTokenMatch(t2, t1) && ValuePatternTokenMatch(l2, l1);
        }

        private bool ValuePatternTokenMatch(JToken patternToken, JToken valueToken)
        {
            if (patternToken == null && valueToken == null) return true;
            if (patternToken is JObject && ((JObject) patternToken).Count == 0)
            {
                // Pattern is a wildcard
                return valueToken != null;
            }
            var patternTokenArray = patternToken as JArray;
            if (patternTokenArray != null)
            {
                if (!patternTokenArray.Any())
                {
                    // Pattern is match none, value must be null
                    return valueToken == null;
                }
                // Otherwise the value token must be in the pattern token array
                return patternTokenArray.Any(x => JToken.DeepEquals(x, valueToken));
            }
            // Otherwise the pattern specifies a single value to match - just do a straight value match
            return JToken.DeepEquals(patternToken, valueToken);
        }

        // Property match enumeration
        private enum MatchType
        {
            // Basic non-match
            NoMatch,
            // Basic match
            Match,
            // Frame provides default value
            DefaultMatch,
            // No match and abort further matching for the node (when a frame specifies no match for a property and the node has some values)
            Abort,
        }

        private bool IsWildcard(JToken token)
        {
            if (token is JObject o) return o.Count == 0;
            if (token is JArray a) return a.Count == 1 && IsWildcard(a[0]);
            return false;
        }

        private static JsonLdEmbed GetEmbedOption(JObject frame, JsonLdEmbed defaultValue)
        {
            if (frame["@embed"] != null)
            {
                var embedString = frame["@embed"] is JObject
                    ? frame["@embed"]["@value"].Value<string>()
                    : frame["@embed"].Value<string>();
                switch (embedString.ToLowerInvariant())
                {
                    case "@always":
                        return JsonLdEmbed.Always;
                    case "true":
                    case "@last":
                        return JsonLdEmbed.Last;
                    case "@link":
                        return JsonLdEmbed.Link;
                    case "false":
                    case "@never":
                        return JsonLdEmbed.Never;
                    default:
                        throw new JsonLdFramingException(JsonLdFramingErrorCode.InvalidEmbedValue,
                            $"Invalid @embed value {embedString}");
                }
            }
            return defaultValue;
        }

        private static string JsonLdEmbedAsString(JsonLdEmbed embed)
        {
            switch (embed)
            {
                case JsonLdEmbed.Always:
                    return "@always";
                case JsonLdEmbed.Last:
                    return "@last";
                case JsonLdEmbed.Link:
                    return "@link";
                case JsonLdEmbed.Never:
                    return "@never";
                default:
                    return null;
            }
        }
        private bool GetBooleanOption(JObject frame, string property, bool defaultValue)
        {
            if (frame[property] != null)
            {
                var optValue = frame[property] as JValue;
                if (optValue != null) return optValue.Value<bool>();
                var optObject = frame[property] as JObject;
                if (optObject != null) return optObject["@value"].Value<bool>();
            }
            return defaultValue;
        }

        private void ValidateFrame(JToken expandedFrame)
        {
            // TODO: Implement frame validation (not currently defined in spec) - throw an invalid frame error if validation fails
            return;
        }

        /// <summary>
        /// Determine if a JSON token is a JSON-LD value object.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True of <paramref name="token"/> is a <see cref="JObject"/> with a. <code>@value</code> property, false otherwise.</returns>
        public static bool IsValueObject(JToken token)
        {
            return ((token as JObject)?.Property("@value")) != null;
        }

        /// <summary>
        /// Determine if a JSON token is a JSON-LD graph object.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if <paramref name="token"/> is a JObject with an @graph property and optionally @id and @index properties and no other properties; false otherwise.</returns>
        public static bool IsGraphObject(JToken token)
        {
            if (!(token is JObject o)) return false;
            if (!o.ContainsKey("@graph")) return false;
            return o.Properties().All(p => GraphObjectKeys.Contains(p.Name));
        }


        /// <summary>
        /// Determines if a JSON token is a JSON-LD simple graph object.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if <paramref name="token"/> is a JObject with an @graph property and optionally an @index property and no other properties; false otherwise.</returns>
        public static bool IsSimpleGraphObject(JToken token)
        {
            if (!(token is JObject o)) return false;
            if (!o.ContainsKey("@graph")) return false;
            return (o.Properties().All(p => p.Name == "@graph" || p.Name == "@index"));
        }

        /// <summary>
        /// Determine if a JSON token is a JSON-LD list object.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True of <paramref name="token"/> is a <see cref="JObject"/> with a. <code>@list</code> property, false otherwise.</returns>
        public static bool IsListObject(JToken token)
        {
            return ((token as JObject)?.Property("@list")) != null;
        }

        private bool IsScalar(JToken token)
        {
            return !(token == null || token.Type == JTokenType.Array || token.Type == JTokenType.Object);
        }

        /// <summary>
        /// Determine if a JSON token represents a string value
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if <paramref name="token"/> represents a string value, false otherwise.</returns>
        private bool IsString(JToken token)
        {
            return token.Type == JTokenType.String;
        }

        private bool IsValidBaseDirection(JToken token)
        {
            if (token.Type != JTokenType.String) return false;
            return token.Value<string>() == "ltr" || token.Value<string>() == "rtl";
        }

        /// <summary>
        /// Determine if a JSON token is an empty array.
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if <paramref name="token"/> is am empty array, false otherwise.</returns>
        private static bool IsEmptyArray(JToken token)
        {
            return (token.Type == JTokenType.Array) && ((token as JArray).Count == 0);
        }

        /// <summary>
        /// Determine if a JSON token is an array, optionally testing each item in the array
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <param name="itemTest">The test to be applied to each child item of <paramref name="token"/></param>
        /// <returns>True if <paramref name="token"/> is a array and either <paramref name="itemTest"/> is null or returns true for all items in the array, false otherwise.</returns>
        private static bool IsArray(JToken token, Func<JToken, bool> itemTest = null)
        {
            if (token.Type != JTokenType.Array) return false;
            return itemTest == null || token.Children().All(itemTest);
        }

        /// <summary>
        /// Determine if a JSON token represents the null value.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <returns>True if the token represents JSON null, false otherwise.</returns>
        private bool IsNull(JToken token)
        {
            return token.Type == JTokenType.Null;
        }

        /// <summary>
        /// Determine if a JSON token represents a JSON object with no properties
        /// </summary>
        /// <param name="token">The token to test</param>
        /// <returns>True if <paramref name="token"/> represents a JSON object and has no child properties, false otherwise.</returns>
        private bool IsEmptyMap(JToken token)
        {
            return token.Type == JTokenType.Object && !token.Children().Any();
        }

        private bool IsAbsoluteIri(JToken token)
        {
            if (!(token is JValue value)) return false;
            return value.Type == JTokenType.String && IsAbsoluteIri(value.Value<string>());
        }

        /// <summary>
        /// Determine if the specified string is an absolute IRI.
        /// </summary>
        /// <param name="value">The string value to be validated.</param>
        /// <returns>True if <paramref name="value"/> can be parsed as an absolute IRI, false otherwise.</returns>
        private static bool IsAbsoluteIri(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var _) && Uri.EscapeUriString(value).Equals(value);
        }

        private static bool IsRelativeIri(JToken token)
        {
            if (!(token is JValue value)) return false;
            return value.Type == JTokenType.String && IsRelativeIri(value.Value<string>());
        }

        /// <summary>
        /// Determine if the specified string is a relative IRI.
        /// </summary>
        /// <param name="value">The string value to be validated.</param>
        /// <returns>True if <paramref name="value"/> can be parsed as an absolute IRI, false otherwise.</returns>
        public static bool IsRelativeIri(string value)
        {
            return Uri.TryCreate(value, UriKind.Relative, out _) && Uri.EscapeUriString(value).Equals(value);
        }

        /// <summary>
        /// Determine if the specified string is an IRI.
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <returns>True if <paramref name="value"/> can be parsed as an IRI, false otherwise.</returns>
        private static bool IsIri(string value)
        {
            // The following would have been ideal, but returns false when the value contains a fragment identifier.
            //return Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute);

            return Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _) && 
                   Uri.EscapeUriString(value).Equals(value) && // Value must be fully escaped
                   !value.StartsWith("#");  // Value cannot be a fragment identifier on its own
        }

        /// <summary>
        /// Determine if the specified string is a JSON-LD keyword.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if <paramref name="value"/> is a JSON-LD keyword, false otherwise.</returns>
        public static bool IsKeyword(string value)
        {
            return JsonLdKeywords.Contains(value) || JsonLdFramingKeywords.Contains(value);
        }

        /// <summary>
        /// Determine if the specified string matches the JSON-LD reserved term production
        /// </summary>
        /// <param name="value">The value to be tested</param>
        /// <returns>True if <paramref name="value"/> matches the pattern for a reserved term, false otherwise.</returns>
        public bool MatchesKeywordProduction(string value)
        {
            return Regex.IsMatch(value, "^@[a-zA-Z]+$");
        }

        /// <summary>
        /// Determine if the specified string is a JSON-LD framing keyword.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFramingKeyword(string value)
        {
            return JsonLdFramingKeywords.Contains(value);
        }

        /// <summary>
        /// Determine if the specified string is a blank node identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsBlankNodeIdentifier(string value)
        {
            return value.StartsWith("_:");
        }

        private static bool IsNodeReference(JToken token)
        {
            return (token as JObject)?.Property("@id") != null;
        }

        /// <summary>
        /// Determines if a token represents a JSON-LD node object
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isTopmostMap"></param>
        /// <returns></returns>
        private static bool IsNodeObject(JToken token, bool isTopmostMap = false)
        {
            // A map is a node object if it exists outside of the JSON-LD context and:
            //   - it does not contain the @value, @list, or @set keywords, or
            //   - it is not the top - most map in the JSON-LD document consisting of no other entries than @graph and @context.
            if (!(token is JObject o)) return false;
            if (!(o.ContainsKey("@value") || o.ContainsKey("@list") || o.ContainsKey("@set"))) return true;
            if (!isTopmostMap)
            {
                if (o.ContainsKey("@graph") || o.ContainsKey("@set") && o.Count == 1) return true;
                if (o.ContainsKey("@graph") && o.ContainsKey("@set") && o.Count == 2) return true;
            }

            return false;
        }

        private JToken GetPropertyValue(JsonLdContext activeContext, JObject obj, string key)
        {
            JToken ret;
            if (obj.TryGetValue(key, out ret)) return ret;
            foreach (var alias in activeContext.GetAliases(key))
            {
                if (obj.TryGetValue(alias, out ret)) return ret;
            }
            return null;
        }

        /// <summary>
        /// Apply the JSON-LD context expansion algorithm to the context found at the specified URL.
        /// </summary>
        /// <param name="contextUrl">The URL to load the source context from.</param>
        /// <param name="options">Options to apply during the expansion processing.</param>
        /// <returns>The expanded JSON-LD contex.</returns>
        public static JArray Expand(Uri contextUrl, JsonLdProcessorOptions options = null)
        {
            var parsedJson = LoadJson(contextUrl, null, options);
            return Expand(parsedJson, contextUrl, null, options);
        }

        /// <summary>
        /// Apply the JSON-LD expansion algorithm to a context JSON object.
        /// </summary>
        /// <param name="input">The context JSON object to be expanded.</param>
        /// <param name="options">Options to apply during the expansion processing.</param>
        /// <returns>The expanded JSON-LD contex.</returns>
        public static JArray Expand(JToken input, JsonLdProcessorOptions options = null)
        {
            if (input is JValue && (input as JValue).Type == JTokenType.String)
            {
                return Expand((input as JValue).Value<string>(), options);
            }
            return Expand(new RemoteDocument { Document = input }, null, null, options);
        }

        private static JArray Expand(RemoteDocument doc, Uri documentLocation,
            JsonLdLoaderOptions loaderOptions = null,
            JsonLdProcessorOptions options = null)
        {
            if (doc.Document is string docContent)
            {
                try
                {
                    doc.Document = JToken.Parse(docContent);
                }
                catch (Exception ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed,
                        "Loading document failed. Error parsing document content as JSON: " + ex.Message);
                }
            }

            if (documentLocation == null) documentLocation = doc.DocumentUrl;
            var activeContext = new JsonLdContext(options.Base != null ? options.Base : documentLocation);
            var processor = new JsonLdProcessor(options);
            if (options?.ExpandContext != null)
            {
                var expandObject = options.ExpandContext as JObject;
                if (expandObject != null)
                {
                    var contextProperty = expandObject.Property("@context");
                    if (contextProperty != null)
                    {
                        activeContext = processor.ProcessContext(activeContext, contextProperty.Value, activeContext.OriginalBase);
                    }
                    else
                    {
                        activeContext = processor.ProcessContext(activeContext, expandObject, activeContext.OriginalBase);
                    }
                }
                else
                {
                    activeContext = processor.ProcessContext(activeContext, options.ExpandContext, activeContext.OriginalBase);
                }
            }
            if (doc.ContextUrl != null)
            {
                var contextDoc = LoadJson(doc.ContextUrl, loaderOptions, options);
                if (contextDoc.Document is string)
                {
                    contextDoc.Document = JToken.Parse(contextDoc.Document as string);
                }
                activeContext = processor.ProcessContext(activeContext, contextDoc.Document as JToken, doc.ContextUrl);
            }

            var expandedOutput =  processor.ExpandAlgorithm(activeContext, null, doc.Document as JToken,
                doc.DocumentUrl ?? options.Base, options.FrameExpansion, options.Ordered);
            if (expandedOutput is JObject expandedObject)
            {
                if (expandedObject.ContainsKey("@graph") && expandedObject.Count == 1)
                    expandedOutput = expandedObject["@graph"];
            }
            if (expandedOutput == null) expandedOutput = new JArray();
            expandedOutput = EnsureArray(expandedOutput);
            return expandedOutput as JArray;
        }

        private static RemoteDocument LoadJson(Uri remoteRef, JsonLdLoaderOptions loaderOptions, JsonLdProcessorOptions options)
        {
            if (options.DocumentLoader != null) return options.DocumentLoader(remoteRef, loaderOptions);

            var client = new RedirectingWebClient();
            client.Headers.Set(HttpRequestHeader.Accept, "application/ld+json;q=1.0, application/json;q=0.9, */*+json;q=0.8");

            var responseString = client.DownloadString(remoteRef);
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
                    return LoadJson(new Uri(remoteRef, alternateLink.LinkValue), loaderOptions, options);
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.LoadingDocumentFailed,
                        "Loading document failed. The server did not resopnd with a processable JSON document.");
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
                    throw new JsonLdProcessorException(JsonLdErrorCode.MultipleContextLinkHeaders, "Multiple context link headers");
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
            public Uri ResponseUri { get; private set; }

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                var webResponse = base.GetWebResponse(request);
                ResponseUri = webResponse?.ResponseUri;
                return webResponse;
            }
        }

        private JToken GetJsonRepresentation(RemoteDocument remoteDoc)
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
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext, "Could not parse remote content as a JSON document. ", ex);
                    }
                }
                default:
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext,
                        "Could not parse remote content as a JSON document.");
            }
        }
        
        private JsonLdRemoteContext GetRemoteContext(Uri reference)
        {
            if (_remoteContextCache.TryGetValue(reference, out var cachedContext)) return cachedContext;
            try
            {
                var remoteDoc = LoadJson(reference,
                    new JsonLdLoaderOptions
                        {Profile = JsonLdVocabulary.Context, RequestProfile = JsonLdVocabulary.Context}, _options);
                var jsonRepresentation = GetJsonRepresentation(remoteDoc);
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

                var remoteContext =new JsonLdRemoteContext(remoteDoc.DocumentUrl, remoteJsonObject["@context"]);
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
    }
}
