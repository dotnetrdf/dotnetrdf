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
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Implements the core JSON-LD processing 
    /// </summary>
    public partial class JsonLdProcessor
    {
        private Uri _base;
        private JsonLdProcessorOptions _options;
        private static readonly string[] JsonLdKeywords = new string[] {
            "@context",
            "@id",
            "@value",
            "@language",
            "@type",
            "@container",
            "@list",
            "@set",
            "@reverse",
            "@index",
            "@base",
            "@vocab",
            "@graph",
            "@nest",
            "@version",
        };

        private static readonly string[] JsonLdFramingKeywords = new string[]
        {
            "@default",
            "@embed",
            "@explicit",
            "@omitDefault",
            "@requireAll",
        };

        private static readonly string[] TermDefinitionKeys = new string[] {
            "@id",
            "@reverse",
            "@container",
            "@context",
            "@nest",
            "@type",
            "@language",
        };
        private static readonly string[] ValueObjectKeys = new string[]
        {
            "@value",
            "@language",
            "@type",
            "@index",
        };
        private Dictionary<string, string> _identifierMap;
        private int _counter;

        /// <summary>
        /// Create a new processor instance
        /// </summary>
        /// <param name="options">JSON-LD processing options</param>
        private JsonLdProcessor(JsonLdProcessorOptions options) {
            if (options == null) options = new JsonLdProcessorOptions();
            _options = options;
            ProcessingMode = _options.ProcessingMode;
            _identifierMap = new Dictionary<string, string>();
            _counter = 0;
        }

        /// <summary>
        /// Get or set the base IRI for processing
        /// </summary>
        /// <remarks>This value should be set to the IRI of the document being processed if available.
        /// </remarks>
        public Uri BaseIri
        {
            get
            {
                return _options.Base ?? _base;
            }
            set { _base = value; }
        }

        /// <summary>
        /// Get or set the current processing mode
        /// </summary>
        public JsonLdProcessingMode? ProcessingMode
        {
            get; set;
        }

        /// <summary>
        /// Process a context in the scope of a current active context resulting in a new context
        /// </summary>
        /// <param name="activeContext">The currently active context</param>
        /// <param name="localContext">The context to be processed. May be a JSON object, string or array.</param>
        /// <param name="remoteContexts">The remote context's already processed. Used to detect circular references in the current context processing step.</param>
        /// <returns></returns>
        public JsonLdContext ProcessContext(JsonLdContext activeContext, JToken localContext, List<Uri> remoteContexts = null)
        {
            if (remoteContexts == null) remoteContexts = new List<Uri>();

            // 1. Initialize result to the result of cloning active context
            var result = activeContext.Clone();

            // 2. If local context is not an array, set it to an array containing only local context.
            localContext = localContext as JArray ?? new JArray(localContext);

            // 3. For each item context in local context:
            foreach (var context in (localContext as JArray))
            {
                // 3.1 If context is null, set result to a newly-initialized active context and continue with the next context. 
                // The base IRI of the active context is set to the IRI of the currently being processed document 
                // (which might be different from the currently being processed context), if available; 
                // otherwise to null. If set, the base option of a JSON-LD API Implementation overrides the base IRI.
                if (context.Type == JTokenType.Null)
                {
                    result = new JsonLdContext
                    {
                        Base = BaseIri,
                    };
                    continue;
                }
                // 3.2 If context is a string
                if (context.Type == JTokenType.String)
                {
                    var contextStr = (context as JValue).Value<string>();
                    var remoteUrl = new Uri(contextStr);
                    if (remoteContexts.Contains(remoteUrl)) return activeContext;
                    var remoteContext = LoadReference(remoteUrl) as JObject;
                    remoteContexts.Add(remoteUrl);
                    if (remoteContext != null)
                    {
                        if (remoteContext.Property("@context") != null)
                        {
                            remoteContext = remoteContext["@context"] as JObject;
                        }

                        result = ProcessContext(result, remoteContext, remoteContexts);
                    }
                    continue;
                }
                // 3.3 - If context is not a JSON object, an invalid local context error has been detected and processing is aborted.
                if (context.Type != JTokenType.Object)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLocalContext, "Local context must be a string, array of strings or JSON object");
                }
                var contextObject = context as JObject;

                // 3.4 - If context has an @base key and remote contexts is empty, i.e., the currently being processed context is not a remote context
                var baseProperty = contextObject.Property("@base");
                if (baseProperty != null && remoteContexts.Count == 0)
                {
                    // 3.4.1 - If context has an @base key and remote contexts is empty, i.e., the currently being processed context is not a remote context:
                    var value = baseProperty.Value;
                    // 3.4.2 - If value is null, remove the base IRI of result.
                    if (value.Type == JTokenType.Null)
                    {
                        result.Base = null;
                    }
                    // 3.4.3 - Otherwise, if value is an absolute IRI, the base IRI of result is set to value.
                    else if (IsAbsoluteIri(value))
                    {
                        result.Base = new Uri(value.Value<string>());
                    }
                    // 3.4.4 - Otherwise, if value is a relative IRI and the base IRI of result is not null, set the base IRI of result to the result of resolving value against the current base IRI of result.
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
                    // 3.4.5 - Otherwise, an invalid base IRI error has been detected and processing is aborted.
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseIri, "The @base property must be an absolute IRI, a relative IRI or null");
                    }
                }

                // 3.5 - If context has an @version key:
                var versionProperty = contextObject.Property("@version");
                if (versionProperty != null)
                {
                    // 3.5.1 - If the associated value is not 1.1, an invalid @version value has been detected, and processing is aborted.
                    var versionValue = versionProperty.Value.Value<string>();
                    if (!"1.1".Equals(versionValue))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVersionValue, $"Found invalid value for @version property: {versionValue}.");
                    }
                    // 3.5.2: If processing mode is not set, and json-ld-1.1 is not a prefix of processing mode, a processing mode conflict error has been detected and processing is aborted.
                    if (_options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.ProcessingModeConflict,
                            "Processing mode conflict. Processor options specify JSON-LD 1.0 processing mode, but encountered @version that requires JSON-LD 1.1 processing features");
                    }
                    // 3.5.3: Set processing mode, to json-ld-1.1, if not already set.
                    if (!_options.ProcessingMode.HasValue)
                    {
                        _options.ProcessingMode = JsonLdProcessingMode.JsonLd11;
                    }
                }

                // 3.6 - If context has an @vocab key:
                var contextProperty = contextObject.Property("@vocab");
                if (contextProperty != null)
                {
                    // 3.6.1 - Initialize value to the value associated with the @vocab key.
                    var value = contextProperty.Value;
                    // 3.6.2 - If value is null, remove any vocabulary mapping from result.
                    if (value.Type == JTokenType.Null)
                    {
                        result.Vocab = null;
                    }
                    // 3.6.3 - Otherwise, if value is an absolute IRI or blank node identifier, the vocabulary mapping of result is set to value. If it is not an absolute IRI or blank node identifier, an invalid vocab mapping error has been detected and processing is aborted.
                    else
                    {
                        var str = value.Value<string>();
                        if (IsAbsoluteIri(str) || IsBlankNodeIdentifier(str))
                        {
                            result.Vocab = str;
                        }
                        else
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidVocabMapping, "The value of @vocab must be an absolute IRI or blank node identifier.");
                        }
                    }
                }

                // 3.7 - If context has an @language key
                var languageProperty = contextObject.Property("@language");
                if (languageProperty != null)
                {
                    // 3.7.1 - Initialize value to the value associated with the @language key.
                    var value = languageProperty.Value;
                    // 3.7.2 - If value is null, remove any default language from result.
                    if (value.Type == JTokenType.Null)
                    {
                        result.Language = null;
                    }
                    // 3.7.3 - Otherwise, if value is string, the default language of result is set to lowercased value. If it is not a string, an invalid default language error has been detected and processing is aborted.
                    else if (value.Type == JTokenType.String)
                    {
                        result.Language = value.Value<string>().ToLowerInvariant();
                    }
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidDefaultLanguage,
                            "@language property value must be a JSON string or null.");
                    }
                }
                // 3.8 - Set processing mode, to json-ld-1.0, if not already set.
                if (!ProcessingMode.HasValue)
                {
                    ProcessingMode = JsonLdProcessingMode.JsonLd10;
                }
                // 3.9 - Create a JSON object defined to use to keep track of whether or not a term has already been defined or currently being defined during recursion.
                var defined = new Dictionary<string, bool>();
                // 3.10 - For each key-value pair in context where key is not @base, @vocab, or @language, invoke the Create Term Definition algorithm, passing result for active context, context for local context, key, and defined.
                foreach (var property in contextObject.Properties())
                {
                    var key = property.Name;
                    if(!(key.Equals("@base") || key.Equals("@vocab") || key.Equals("@language")))
                    {
                        CreateTermDefinition(result, contextObject, key, defined);
                    }
                }
            }
            return result;
        }

        private void CreateTermDefinition(JsonLdContext activeContext, JObject localContext, string term, Dictionary<string, bool> defined = null)
        {
            if (defined == null) defined = new Dictionary<string, bool>();
            bool created;

            // 1 - If defined contains the key term and the associated value is true (indicating that the term definition has already been created), return. Otherwise, if the value is false, a cyclic IRI mapping error has been detected and processing is aborted.
            if (defined.TryGetValue(term, out created))
            {
                if (created) { return; }
                throw new JsonLdProcessorException(JsonLdErrorCode.CyclicIriMapping, $"Cyclic IRI mapping detected while processing term {term}");
            }

            // 2 - Set the value associated with defined's term key to false. This indicates that the term definition is now being created but is not yet complete.
            defined[term] = false;

            // 3 - Since keywords cannot be overridden, term must not be a keyword. Otherwise, a keyword redefinition error has been detected and processing is aborted.
            if (IsKeyword(term))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.KeywordRedefinition, $"Cannot redefine JSON-LD keyword {term}.");
            }

            // 4 - Remove any existing term definition for term in active context.
            activeContext.RemoveTerm(term);

            // 5 - Initialize value to a copy of the value associated with the key term in local context.
            var v = localContext[term];

            // 6 - If value is null or value is a JSON object containing the key-value pair @id-null, set the term definition in active context to null, set the value associated with defined's key term to true, and return.
            JObject value;
            var idValue = v is JObject ? GetPropertyValue(activeContext, v as JObject, "@id") : null;
            if (v == null || 
                (v is JValue && (v as JValue)?.Type == JTokenType.Null) ||
                (idValue != null && idValue.Type == JTokenType.Null))
            {
                activeContext.SetTerm(term, null);
                defined[term] = true;
                return;
            }
            // 7 - Otherwise, if value is a string, convert it to a JSON object consisting of a single member whose key is @id and whose value is value.
            else if (v.Type == JTokenType.String)
            {
                value = new JObject(new JProperty("@id", v.Value<string>()));
            }
            // 8 - Otherwise, value must be a JSON object, if not, an invalid term definition error has been detected and processing is aborted.
            else if (v.Type == JTokenType.Object)
            {
                value = v.DeepClone() as JObject;
            }
            else
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, $"Invalid term definition at {term}.");
            }

            // 9 - Create a new term definition, definition.
            var definition = new JsonLdTermDefinition();

            // 10 - If value contains the key @type:
            var typeValue = GetPropertyValue(activeContext, value, "@type");
            if (typeValue != null)
            {
                // 10.1 Initialize type to the value associated with the @type key, which must be a string. Otherwise, an invalid type mapping error has been detected and processing is aborted.
                if (typeValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping, $"Invalid type mapping for term {term}. The @type value must be a string, got {typeValue.Type}");
                }
                // 10.2 - Set type to the result of using the IRI Expansion algorithm, passing active context, type for value, true for vocab, false for document relative, local context, and defined. If the expanded type is neither @id, nor @vocab, nor an absolute IRI, an invalid type mapping error has been detected and processing is aborted.
                var type = ExpandIri(activeContext, typeValue.Value<string>(), true, false, localContext, defined);
                if (type == "@id" || type == "@vocab" || IsAbsoluteIri(type))
                {
                    // 10.3 - Set the type mapping for definition to type.
                    definition.TypeMapping = type;
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeMapping, $"Invalid type mapping for term {term}. Expected @type value to expand to @id, @vocab or an absolute IRI. Unexpanded value was {typeValue.Value<string>()}, expanded value was {type}.");
                }
            }

            var reverseValue = GetPropertyValue(activeContext, value, "@reverse");
            var containerValue = GetPropertyValue(activeContext, value, "@container");
            // 11 - If value contains the key @reverse:
            if (reverseValue != null)
            {
                // 11.1 - If value contains @id or @nest, members, an invalid reverse property error has been detected and processing is aborted.
                if (GetPropertyValue(activeContext, value, "@id") != null ||
                    GetPropertyValue(activeContext, value, "@nest") != null)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty, $"Invalid reverse property. The @reverse property cannot be combined with @id or @nest property on term {term}.");
                }

                // 11.2 - If the value associated with the @reverse key is not a string, an invalid IRI mapping error has been detected and processing is aborted.
                if (reverseValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"@reverse property value must be a string on term {term}");
                }

                // 11.3 - Otherwise, set the IRI mapping of definition to the result of using the IRI Expansion algorithm, passing active context, the value associated with the @reverse key for value, true for vocab, false for document relative, local context, and defined. If the result is neither an absolute IRI nor a blank node identifier, i.e., it contains no colon (:), an invalid IRI mapping error has been detected and processing is aborted.
                var iriMapping = ExpandIri(activeContext, reverseValue.Value<string>(), true, false, localContext, defined);
                if (!IsAbsoluteIri(iriMapping) && !IsBlankNodeIdentifier(iriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"@reverse property value must expand to an absolute IRI or blank node identifier. The @reverse property on term {term} expands to {iriMapping}.");
                }
                definition.IriMapping = iriMapping;

                // 11.4 - If value contains an @container member, set the container mapping of definition to its value; if its value is neither @set, nor @index, nor null, an invalid reverse property error has been detected (reverse properties only support set- and index-containers) and processing is aborted.
                if (containerValue != null) {
                    if (containerValue.Type == JTokenType.Null)
                    {
                        definition.ContainerMapping = JsonLdContainer.Null;
                    }
                    else if (containerValue.Type == JTokenType.String)
                    {
                        var containerMapping = containerValue.Value<string>();
                        if (containerMapping == "@set")
                        {
                            definition.ContainerMapping = JsonLdContainer.Set;
                        }
                        else if (containerMapping == "@index")
                        {
                            definition.ContainerMapping = JsonLdContainer.Index;
                        }
                        else
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidReverseProperty, $"Invalid reverse property for term {term}. Reverse properties only support set and index container types. ");
                        }
                    }
                    else
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping, $"Invalid @container property for term {term}. Property value must be a JSON string.");
                    }
                }
                // 11.5 - Set the reverse property flag of definition to true.
                definition.Reverse = true;
                // 11.6 - Set the term definition of term in active context to definition and the value associated with defined's key term to true and return.
                activeContext.SetTerm(term, definition);
                defined[term] = true;
                return;
            }

            // 12 - Set the reverse property flag of definition to false.
            definition.Reverse = false;

            // 13 - If value contains the key @id and its value does not equal term:
            idValue = GetPropertyValue(activeContext, value, "@id");
            if (idValue != null && !term.Equals(idValue.Value<string>()))
            {
                // 13.1 - If the value associated with the @id key is not a string, an invalid IRI mapping error has been detected and processing is aborted.
                if (idValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"Invalid IRI Mapping. The value of the @id property of term {term} must be a string.");
                }
                // 13.2 - Otherwise, set the IRI mapping of definition to the result of using the IRI Expansion algorithm, passing active context, the value associated with the @id key for value, true for vocab, false for document relative, local context, and defined. If the resulting IRI mapping is neither a keyword, nor an absolute IRI, nor a blank node identifier, an invalid IRI mapping error has been detected and processing is aborted; if it equals @context, an invalid keyword alias error has been detected and processing is aborted.
                var iriMapping = ExpandIri(activeContext, idValue.Value<string>(), true, false, localContext, defined);
                if (!IsKeyword(iriMapping) &&
                    !IsAbsoluteIri(iriMapping) &&
                    !IsBlankNodeIdentifier(iriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"Invalid IRI Mapping. The value of the @id property of term '{term}' must be a keyword, an absolute IRI or a blank node identifier. Got value {iriMapping}.");
                }
                if ("@context".Equals(iriMapping))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidKeywordAlias, $"Invalid keyword alias at term {term}.");
                }
                definition.IriMapping = iriMapping;
            }
            // 14 - Otherwise if the term contains a colon (:):
            else if (term.Contains(":"))
            {
                var ix = term.IndexOf(':');
                var prefix = term.Substring(0, ix);
                var rest = term.Substring(ix + 1);
                // 14.1 - If term is a compact IRI with a prefix that is a key in local context a dependency has been found. Use this algorithm recursively passing active context, local context, the prefix as term, and defined.
                if (localContext.Property(prefix) != null)
                {
                    CreateTermDefinition(activeContext, localContext, prefix, defined);
                }
                // 14.2 - If term's prefix has a term definition in active context, set the IRI mapping of definition to the result of concatenating the value associated with the prefix's IRI mapping and the term's suffix.
                var prefixTermDefinition = activeContext.GetTerm(prefix);
                if (prefixTermDefinition != null)
                {
                    definition.IriMapping = prefixTermDefinition.IriMapping + rest;
                }
                // 14.3 - Otherwise, term is an absolute IRI or blank node identifier. Set the IRI mapping of definition to term.
                else
                {
                    definition.IriMapping = term;
                }
            }
            // 15 - Otherwise, if active context has a vocabulary mapping, the IRI mapping of definition is set to the result of concatenating the value associated with the vocabulary mapping and term. If it does not have a vocabulary mapping, an invalid IRI mapping error been detected and processing is aborted.
            else if (activeContext.Vocab != null)
            {
                definition.IriMapping = activeContext.Vocab.ToString() + term;
            }
            else
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIriMapping, $"Invalid IRI Mapping. The term '{term}' could not be processed as an IRI mapping");
            }

            // 16 - if value contains the key @container
            if (containerValue != null)
            {
                definition.ContainerMapping = ValidateContainerMapping(term, containerValue);
                if (ProcessingMode == JsonLdProcessingMode.JsonLd10 &&
                    (definition.ContainerMapping == JsonLdContainer.Id ||
                    definition.ContainerMapping == JsonLdContainer.Type))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping, "Invalid Container Mapping. @id and @type containers are not supported when the processing mode is json-ld-1.0");
                }
            }

            // 17 - if value contains the key @context
            var contextValue = GetPropertyValue(activeContext, value, "@context");
            if (contextValue != null)
            {
                // 17.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
                if (ProcessingMode == JsonLdProcessingMode.JsonLd10)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, $"Invalid Term Definition for term '{term}'. The @context property is not supported on a term definition when the processing mode is json-ld-1.0");
                }
                // 17.2 - Initialize context to the value associated with the @context key, which is treated as a local context.
                var context = contextValue;

                // 17.3 - Invoke the Context Processing algorithm using the active context and context as local context. If any error is detected, an invalid scoped context error has been detected and processing is aborted.
                try
                {
                    ProcessContext(activeContext, context);
                }
                catch (JsonLdProcessorException ex)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidScopedContext, $"Invalid scoped context for term '{term}'. See inner exception for details of the scope processing error.", ex);
                }

                // 17.4 - Set the local context of definition to context.
                definition.LocalContext = context;
            }

            // 18 - if value contains the key @language and does not contain the key @type
            var languageValue = GetPropertyValue(activeContext, value, "@language");
            if (languageValue != null && typeValue == null)
            {
                // 18.1 Initialize language to the value associated with the @language key, which must be either null or a string.Otherwise, an invalid language mapping error has been detected and processing is aborted.
                // 18.2 If language is a string set it to lowercased language. Set the language mapping of definition to language.
                if (languageValue.Type == JTokenType.Null)
                {
                    definition.LanguageMapping = null;
                }
                else if (languageValue.Type == JTokenType.String)
                {
                    definition.LanguageMapping = languageValue.Value<string>().ToLowerInvariant();
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapping, $"Invalid Language Mapping on term '{term}'. The value of the @language property must be either null or a string");
                }
            }

            // 19 - If value contains the key @nest:
            var nestValue = GetPropertyValue(activeContext, value, "@nest");
            if(nestValue != null)
            {
                // 19.1 - If processingMode is json-ld-1.0, an invalid term definition has been detected and processing is aborted.
                if (ProcessingMode == JsonLdProcessingMode.JsonLd10)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, $"Invalid Term Definition for term '{term}. Term definitions may not contain the @nest property when the processing mode is json-ld-1.0");
                }
                // 19.2 - Initialize nest to the value associated with the @nest key, which must be a string and must not be a keyword other than @nest. Otherwise, an invalid @nest value error has been detected and processing is aborted.
                if (nestValue.Type != JTokenType.String)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue, $"Invalid Nest Value for term '{term}'. The value of the @nest property must be a string");
                }
                var nest = nestValue.Value<string>();
                if (IsKeyword(nest) && !"@nest".Equals(nest))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue, $"Invalid Nest Value for term '{term}'. The value of the @nest property cannot be a JSON-LD keyword other than '@nest'");
                }
                definition.Nest = nest;
            }

            // 20 - If the value contains any key other than @id, @reverse, @container, @context, @nest, or @type, an invalid term definition error has been detected and processing is aborted.
            var unrecognizedKeys = value.Properties().Select(prop => prop.Name).Where(x => !TermDefinitionKeys.Contains(x)).ToList();
            if (unrecognizedKeys.Any())
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTermDefinition, $"Invalid Term Definition for term '{term}'. Term definition contains unrecognised property key(s) {string.Join(", ", unrecognizedKeys)}");
            }

            // 21 - Set the term definition of term in active context to definition and set the value associated with defined's key term to true.
            activeContext.SetTerm(term, definition);
            defined[term] = true;
        }

        private string ExpandIri(JsonLdContext activeContext, string value, bool vocab = false, bool documentRelative = false, JObject localContext = null, Dictionary<string, bool> defined = null)
        {
            // 1. If value is a keyword or null, return value as is.
            if (value == null || IsKeyword(value)) return value;

            // 2. If local context is not null, it contains a key that equals value, 
            // and the value associated with the key that equals value in defined 
            // is not true, invoke the Create Term Definition algorithm, 
            // passing active context, local context, value as term, and defined. 
            // This will ensure that a term definition is created for value in active context during Context Processing.
            if (defined == null) defined = new Dictionary<string, bool>();
            var isDefined = false;
            defined.TryGetValue(value, out isDefined);
            if (localContext?.Property(value) != null && !isDefined) {
                CreateTermDefinition(activeContext, localContext, value, defined);
            }

            // 3. If vocab is true and the active context has a term definition for value, return the associated IRI mapping.
            if (vocab && activeContext.TryGetTerm(value, out JsonLdTermDefinition termDefinition))
            {
                return termDefinition?.IriMapping;
            }

            // 4. If value contains a colon (:), it is either an absolute IRI, a compact IRI, or a blank node identifier:
            var ix = value.IndexOf(':');
            if (ix > 0)
            {
                // 4.1 Split value into a prefix and suffix at the first occurrence of a colon (:).
                var prefix = value.Substring(0, ix);
                var suffix = value.Substring(ix + 1);

                // 4.2 If prefix is underscore (_) or suffix begins with double-forward-slash (//), 
                // return value as it is already an absolute IRI or a blank node identifier.
                if (prefix.Equals("_") || suffix.StartsWith("//"))
                {
                    return value;
                }

                // 4.3 If local context is not null, it contains a key that equals prefix, 
                // and the value associated with the key that equals prefix in defined is not true, 
                // invoke the Create Term Definition algorithm, passing active context, 
                // local context, prefix as term, and defined. This will ensure that a term definition 
                // is created for prefix in active context during Context Processing.
                defined.TryGetValue(prefix, out bool prefixDefined);
                if (localContext != null &&
                    localContext.Property(prefix) != null &&
                    !prefixDefined)
                {
                    CreateTermDefinition(activeContext, localContext, prefix, defined);
                }

                // 4.4 If active context contains a term definition for prefix, 
                // return the result of concatenating the IRI mapping associated with prefix and suffix.
                if (activeContext.TryGetTerm(prefix, out termDefinition))
                {
                    return termDefinition.IriMapping + suffix;
                }

                // 4.5 Return value as it is already an absolute IRI
                return value;
            }

            // 5 If vocab is true, and active context has a vocabulary mapping, return the result of concatenating the vocabulary mapping with value.
            if (vocab && activeContext.Vocab != null)
            {
                return activeContext.Vocab.ToString() + value;
            }
            // 6 Otherwise, if document relative is true, set value to the result of resolving value against the base IRI. 
            else if (documentRelative)
            {
                var baseIri = activeContext.HasBase ? activeContext.Base : BaseIri;
                if (baseIri != null)
                {
                    var iri = new Uri(activeContext.Base ?? BaseIri, value);
                    return iri.ToString();
                }
            }

            // 7 Return value as is
            return value;
        }

        private static IEnumerable<WebLink> ParseLinkHeaders(IEnumerable<string> linkHeaderValues)
        {
            foreach(var linkHeaderValue in linkHeaderValues)
            {
                var fields = linkHeaderValue.Split(';').Select(x => x.Trim());
                var linkValue = fields.First().TrimStart('<').TrimEnd('>');
                var relTypes = new List<string>();
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
                    }
                }
                yield return new WebLink { LinkValue = linkValue, RelationTypes = relTypes };
            }
        }

        internal class WebLink
        {
            public string LinkValue { get; set; }
            public List<string> RelationTypes { get; set; }
        }

        private JArray Expand(JsonLdContext activeContext, string activeProperty, JToken element)
        {
            var result = ExpandAlgorithm(activeContext, activeProperty, element);

            // If, after the above algorithm is run, the result is a JSON object that contains only an @graph key, 
            // set the result to the value of @graph's value. 
            // Otherwise, if the result is null, set it to an empty array. 
            // Finally, if the result is not an array, then set the result to an array containing only the result.

            var resultObject = result as JObject;
            if (resultObject != null && resultObject.Properties().Count() == 1 && resultObject.Property("@graph") != null)
            {
                result = resultObject["@graph"];
            }
            if (result == null || result.Type == JTokenType.Null)
            {
                result = new JArray();
            }
            if (result.Type != JTokenType.Array)
            {
                result = new JArray(result);
            }
            return result as JArray;
        }

        private JToken ExpandAlgorithm(JsonLdContext activeContext, string activeProperty, JToken element)
        {
            var frameExpansion = _options.ProcessingMode == JsonLdProcessingMode.JsonLd11FrameExpansion;

            JToken result = null;

            // 1 - If element is null, return null.
            if (element.Type == JTokenType.Null)
            {
                return null;
            }

            // 2 - If element is a scalar,
            if (IsScalar(element)) {
                // 2.1 - If active property is null or @graph, drop the free-floating scalar by returning null.
                if (activeProperty == null || activeProperty == "@graph") return null;

                // 2.2 - Return the result of the Value Expansion algorithm, passing the active context, active property, and element as value.
                return ExpandValue(activeContext, activeProperty, element);
            }

            JsonLdTermDefinition activePropertyTermDefinition = null;
            var hasTermDefinition = activeProperty != null && activeContext.TryGetTerm(activeProperty, 
                out activePropertyTermDefinition);
            // 3 - If element is an array,
            if (element.Type == JTokenType.Array)
            {
                // 3.1 - Initialize an empty array, result.
                result = new JArray();
                var resultArray = result as JArray;

                // 3.2 - For each item in element:
                foreach(var item in (element as JArray))
                {
                    // 3.2.1 - Initialize expanded item to the result of using this algorithm recursively, passing active context, active property, and item as element.
                    var expandedItem = ExpandAlgorithm(activeContext, activeProperty, item);
                    if (expandedItem != null)
                    {
                        // 3.2.2 - If the active property is @list or its container mapping is set to @list, 
                        // the expanded item must not be an array or a list object, 
                        // otherwise a list of lists error has been detected and processing is aborted.
                        if (activeProperty != null &&
                            (activeProperty.Equals("@list") ||
                            hasTermDefinition && activePropertyTermDefinition.ContainerMapping.Equals(JsonLdContainer.List)))
                        {
                            if (IsListObject(expandedItem) || expandedItem.Type == JTokenType.Array)
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.ListOfLists, $"List of lists error at property {activeProperty}");
                            }
                        }

                        // 3.2.2 - If expanded item is an array, append each of its items to result. Otherwise, if expanded item is not null, append it to result.
                        if (expandedItem != null && expandedItem.Type == JTokenType.Array)
                        {
                            foreach (var expandedItemItem in expandedItem as JArray)
                            {
                                resultArray.Add(expandedItemItem);
                            }
                        }
                        else
                        {
                            resultArray.Add(expandedItem);
                        }
                    }
                }

                // 3.3 - Return result
                return result;
            }

            // 4 - Otherwise element is a JSON object.
            var elementObject = element as JObject;

            // 5 - If element contains the key @context, set active context to the 
            // result of the Context Processing algorithm, passing active context 
            // and the value of the @context key as local context.
            var contextValue = GetPropertyValue(activeContext, elementObject, "@context");
            if (contextValue != null)
            {
                activeContext = ProcessContext(activeContext, contextValue);
            }

            // 6 - For each key/value pair in element ordered lexicographically by key where key expands to @type using the IRI Expansion algorithm, passing active context, key for value, and true for vocab:
            var typeProperties = elementObject.Properties().Where(property => "@type".Equals(ExpandIri(activeContext, property.Name, true))).OrderBy(p => p.Name).ToList();
            foreach(var property in typeProperties)
            {
                // For each term which is a value of value, 
                var values = property.Value.Type == JTokenType.Array ? property.Value as JArray : new JArray(property.Value);
                foreach(var value in values)
                {
                    if (value.Type == JTokenType.String) {
                        var term = value.Value<string>();
                        // if term's term definition in active context has a local context, 
                        if (activeContext.TryGetTerm(term, out JsonLdTermDefinition termDefinition) &&
                            termDefinition.LocalContext != null)
                        {
                            // set active context to the result to the result of 
                            // the Context Processing algorithm, passing active context 
                            // and the value of the term's local context as local context.
                            activeContext = ProcessContext(activeContext, termDefinition.LocalContext);
                        }
                    }
                }
            }

            // 7 - Initialize an empty JSON object, result.
            result = new JObject();
            var resultObject = result as JObject;

            // Implements step 8
            ExpandKeys(activeContext, activeProperty, elementObject, resultObject, frameExpansion);

            // 9 - If result contains the key @value:
            if (resultObject.Property("@value") != null) {
                // 9.1 - The result must not contain any keys other than @value, @language, @type, and @index. It must not contain both the @language key and the @type key. Otherwise, an invalid value object error has been detected and processing is aborted.
                if (resultObject.Properties().Any(p => !ValueObjectKeys.Contains(p.Name)))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject, "A value object may not contain properties other than @value, @language, @type and @index after expansion.");
                }
                if (resultObject.Property("@type")!=null && resultObject.Property("@language") != null)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject, "A value object may not contain both @type and @language properties after expansion.");
                }
                // 9.2 - If the value of result's @value key is null, then set result to null.
                var valueProperty = resultObject.Property("@value");
                var typeProperty = resultObject.Property("@type");
                if (valueProperty.Value.Type == JTokenType.Null)
                {
                    result = null;
                }
                // 9.3 - Otherwise, if the value of result's @value member is not a string and result contains the key @language, an invalid language-tagged value error has been detected (only strings can be language-tagged) and processing is aborted.
                else if (resultObject.Property("@language") != null &&
                         !(valueProperty.Value.Type == JTokenType.String ||
                           frameExpansion && (valueProperty.Value.Type == JTokenType.Array ||
                                              IsWildcard(valueProperty.Value))))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedValue,
                        $"A value object with an @language property must have a string value for the @value property. Found a {valueProperty.Value.Type}");
                }
                // 9.4 - Otherwise, if the result has an @type member and its value is not an IRI, an invalid typed value error has been detected and processing is aborted.
                else if (typeProperty != null)
                {
                    var typeValue = typeProperty.Value;
                    if (!((typeValue.Type == JTokenType.String && IsAbsoluteIri(typeValue.Value<string>())) ||
                          (typeValue.Type == JTokenType.Object && frameExpansion && !typeValue.Any()) ||
                          (typeValue.Type == JTokenType.Array && frameExpansion &&
                           typeValue.All(x => IsAbsoluteIri(x.Value<string>())))))
                        //if (typeValue.Type != JTokenType.String || !IsAbsoluteIri(typeValue.Value<string>()))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypedValue,
                            "The value of the @type property of a value object must be an IRI.");
                    }
                }
            }
            // 10 - Otherwise, if result contains the key @type and its associated value is not an array, set it to an array containing only the associated value.
            else if (resultObject.Property("@type") != null)
            {
                var typeProperty = resultObject.Property("@type");
                if (typeProperty.Value.Type != JTokenType.Array)
                {
                    typeProperty.Value = new JArray(typeProperty.Value);
                }
            }
            // 11 - Otherwise, if result contains the key @set or @list:
            else if (resultObject.Property("@set") != null || resultObject.Property("@list") != null)
            {
                // 11.1 - The result must contain at most one other key and that key must be @index. Otherwise, an invalid set or list object error has been detected and processing is aborted.
                var properties = resultObject.Properties().ToList();
                if (properties.Count > 2 || properties.Any(p => !(p.Name.Equals("@set") || p.Name.Equals("@list") || (p.Name.Equals("@index")))))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidSetOrListObject, "Set and list objects may only contain either an @set or @list property and an @index property.");
                }

                // 11.2 - If result contains the key @set, then set result to the key's associated value.
                var setProperty = resultObject.Property("@set");
                if (setProperty != null)
                {
                    result = setProperty.Value;
                }
            }
            // 12 - If result contains only the key @language, set result to null.
            if (resultObject.Properties().Count() == 1 && resultObject.Property("@language") != null)
            {
                result = null;
            }
            // 13 - If active property is null or @graph, drop free-floating values as follows:
            if (activeProperty == null || activeProperty.Equals("@graph"))
            {
                // 13.1 - If result is an empty JSON object or contains the keys @value or @list, set result to null.
                if (resultObject.Properties().Count() == 0 || resultObject.Property("@value")!=null || resultObject.Property("@list") != null)
                {
                    result = null;
                }
                // 13.2 - Otherwise, if result is a JSON object whose only key is @id, set result to null.
                // KA - but not for frame expansion?
                if (resultObject.Properties().Count() == 1 && resultObject.Property("@id") != null && !frameExpansion)
                {
                    result = null;
                }
            }
            // 14 - Return result
            return result;
        }

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
                        /* When the frame expansion flag is set, value may also be an empty dictionary. */
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

        private JToken ExpandValue(JsonLdContext activeContext, string activeProperty, JToken value)
        {
            var activePropertyTermDefinition = activeContext.GetTerm(activeProperty);
            var typeMapping = activePropertyTermDefinition?.TypeMapping;

            // 1 - If the active property has a type mapping in active context that is @id, return a new JSON object containing a single key-value pair where the key is @id and the value is the result of using the IRI Expansion algorithm, passing active context, value, and true for document relative.
            if (typeMapping != null && typeMapping == "@id") {
                return new JObject(new JProperty("@id", ExpandIri(activeContext, value.Value<string>(), documentRelative: true)));
            }
            // 2 - If active property has a type mapping in active context that is @vocab, return a new JSON object containing a single key-value pair where the key is @id and the value is the result of using the IRI Expansion algorithm, passing active context, value, true for vocab, and true for document relative.
            if (typeMapping != null && typeMapping == "@vocab")
            {
                return new JObject(new JProperty("@id", ExpandIri(activeContext, value.Value<string>(), vocab:true, documentRelative: true)));
            }
            // 3 - Otherwise, initialize result to a JSON object with an @value member whose value is set to value.
            var result = new JObject(new JProperty("@value", value));
            // 4 - If active property has a type mapping in active context, add an @type member to result and set its value to the value associated with the type mapping.
            if (typeMapping != null)
            {
                result.Add(new JProperty("@type", typeMapping));
            }
            // 5 - Otherwise, if value is a string:
            else if (value.Type == JTokenType.String)
            {
                // 5.1 - If a language mapping is associated with active property in active context, add an @language to result and set its value to the language code associated with the language mapping; unless the language mapping is set to null in which case no member is added.
                if (activePropertyTermDefinition!= null && activePropertyTermDefinition.HasLanguageMapping)
                {
                    if (activePropertyTermDefinition?.LanguageMapping != null)
                    {
                        result.Add(new JProperty("@language", activePropertyTermDefinition.LanguageMapping));
                    }
                }
                // 5.2 - Otherwise, if the active context has a default language, add an @language to result and set its value to the default language.
                else if (activeContext.Language != null)
                {
                    result.Add(new JProperty("@language", activeContext.Language));
                }
            }
            // 6 - Return result.
            return result;
        }

        private JsonLdContainer ValidateContainerMapping(string term, JToken containerValue)
        {
            if (containerValue.Type == JTokenType.String)
            {
                switch (containerValue.Value<string>())
                {
                    case "@list":
                        return JsonLdContainer.List;
                    case "@set":
                        return JsonLdContainer.Set;
                    case "@index":
                        return JsonLdContainer.Index;
                    case "@id":
                        return JsonLdContainer.Id;
                    case "@type":
                        return JsonLdContainer.Type;
                    case "@language":
                        return JsonLdContainer.Language;
                }
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping, $"Invalid Container Mapping. Unrecognised @container property value '{containerValue.Value<string>()}' for term '{term}'");
            }
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidContainerMapping,
                $"Invalid Container Mapping. The value of the @container property of term '{term}' must be a string.");
        }

        /// <summary>
        /// Run the Compaction algorithm
        /// </summary>
        /// <param name="input">The JSON-LD data to be compacted. Expected to be a JObject or JArray of JObject or a JString whose value is the IRI reference to a JSON-LD document to be retrieved</param>
        /// <param name="context">The context to use for the compaction process. May be a JObject, JArray of JObject, JString or JArray of JString. String values are treated as IRI references to context documents to be retrieved</param>
        /// <param name="options">Additional processor options</param>
        /// <returns></returns>
        public static JObject Compact(JToken input, JToken context, JsonLdProcessorOptions options)
        {
            var processor = new JsonLdProcessor(options);

            // Set expanded input to the result of using the expand method using input and options.
            var expandedInput = Expand(input, options);
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

        private JToken CompactAlgorithm(JsonLdContext activeContext, JObject inverseContext, string activeProperty,
            JToken element, bool compactArrays = true)
        {
            JsonLdTermDefinition termDefinition = null;

            // 1 - If the term definition for active property has a local context:
            if (activeProperty != null)
            {
                termDefinition = activeContext.GetTerm(activeProperty);
                if (termDefinition?.LocalContext != null)
                {
                    // 1.1 - Set active context to the result of the Context Processing algorithm, passing active context and the value of the active property's local context as local context.
                    activeContext = ProcessContext(activeContext, termDefinition.LocalContext);
                    // 1.2 - Set inverse context using the Inverse Context Creation algorithm using active context.
                    inverseContext = CreateInverseContext(activeContext);
                }
            }
            // 2 - If element is a scalar, it is already in its most compact form, so simply return element.
            if (IsScalar(element)) return element;
            // 3 - If element is an array:
            var elArray = element as JArray;
            JToken result;
            if (elArray != null)
            {
                // 3.1 - Initialize result to an empty array.
                result = new JArray();
                var resultArray = (JArray) result;
                // 3.2 - For each item in element:
                foreach (var item in elArray)
                {
                    // 3.2.1 - Initialize compacted item to the result of using this algorithm recursively, passing active context, inverse context, active property, and item for element.
                    var compactedItem = CompactAlgorithm(activeContext, inverseContext, activeProperty, item, compactArrays);
                    // 3.2.2 - If compacted item is not null, then append it to result.
                    if (compactedItem != null)
                    {
                        resultArray.Add(compactedItem);
                    }
                }
                // 3.3 - If result contains only one item (it has a length of 1), active property has no container mapping in active context, and compactArrays is true, set result to its only item.
                if (resultArray.Count == 1 &&
                    (termDefinition == null || termDefinition.ContainerMapping == JsonLdContainer.Null) &&
                    compactArrays)
                {
                    result = result[0];
                }
                return result;
            }
            // 4 - Otherwise element is a dictionary.
            var elObject = element as JObject;
            // 5 - If element has an @value or @id member and the result of using the Value Compaction algorithm, passing active context, inverse context, active property,and element as value is a scalar, return that result.
            if (elObject.Property("@value") != null || elObject.Property("@id") != null)
            {
                var compactedValue = CompactValue(activeContext, inverseContext, activeProperty, elObject);
                if (IsScalar(compactedValue)) return compactedValue;
            }
            // 6 - Initialize inside reverse to true if active property equals @reverse, otherwise to false.
            var insideReverse = "@reverse".Equals(activeProperty);
            // 7 - Initialize result to an empty dictionary.
            result = new JObject();
            var resultObject = (JObject) result;
            // 8 - For each key expanded property and value expanded value in element, ordered lexicographically by expanded property:
            foreach (var p in elObject.Properties().OrderBy(x=>x.Name))
            {
                var expandedProperty = p.Name;
                var expandedValue = p.Value;
                JToken compactedValue = null;
                // 8.1 - If expanded property is @id or @type:
                if (expandedProperty.Equals("@id") || expandedProperty.Equals("@type"))
                {
                    // 8.1.1 - If expanded value is a string, then initialize compacted value to the result of using the IRI Compaction algorithm, 
                    // passing active context, inverse context, expanded value for iri, and true for vocab if expanded property is @type, false otherwise.
                    if (expandedValue.Type == JTokenType.String)
                    {
                        compactedValue = new JValue(CompactIri(activeContext, inverseContext,
                            expandedValue.Value<string>(), vocab: expandedProperty.Equals("@type")));
                    }
                    else
                    // 8.1.2 - Otherwise, expanded value must be a @type array:
                    {
                        // 8.1.2.1 - Initialize compacted value to an empty array.
                        compactedValue = new JArray();
                        var valueArray = (JArray)compactedValue;
                        // 8.1.2.2 - For each item expanded type in expanded value:
                        foreach (var expandedType in (expandedValue as JArray))
                        {
                            // 8.1.2.2.1 - Set term to the result of of using the IRI Compaction algorithm, passing active context, inverse context, expanded type for iri, and true for vocab.
                            var term = CompactIri(activeContext, inverseContext, expandedType.Value<string>(),
                                vocab: true);
                            // 8.1.2.2.2 - If the term definition for term has a local context:
                            var td = activeContext.GetTerm(term);
                            if (td?.LocalContext != null)
                            {
                                // 8.1.2.2.2.1 - Set active context to the result of the Context Processing algorithm, passing active context and the value of term's local context as local context.
                                activeContext = ProcessContext(activeContext, td.LocalContext);
                                // 8.1.2.2.2.2 - Set inverse context using the Inverse Context Creation algorithm using active context.
                                inverseContext = CreateInverseContext(activeContext);
                            }
                            // 8.1.2.2.3 - Append term, to compacted value.
                            valueArray.Add(term);
                        }
                        // 8.1.2.3 - If compacted value contains only one item (it has a length of 1), then set compacted value to its only item.
                        if (valueArray.Count == 1)
                        {
                            compactedValue = valueArray[0];
                        }
                    }
                    // 8.1.3 - Initialize alias to the result of using the IRI Compaction algorithm, passing active context, inverse context, expanded property for iri, and true for vocab.
                    var alias = CompactIri(activeContext, inverseContext, expandedProperty, vocab: true);
                    // 8.1.4 - Add a member alias to result whose value is set to compacted value and continue to the next expanded property.
                    resultObject.Add(alias, compactedValue);
                    continue;
                }
                // 8.2 - If expanded property is @reverse:
                if (expandedProperty.Equals("@reverse"))
                {
                    // 8.2.1 - Initialize compacted value to the result of using this algorithm recursively, passing active context, inverse context, @reverse for active property, and expanded value for element.
                    compactedValue = CompactAlgorithm(activeContext, inverseContext, "@reverse", expandedValue);
                    // 8.2.2 - For each property and value in compacted value:
                    foreach (var rp in (compactedValue as JObject).Properties().ToList())
                    {
                        var property = rp.Name;
                        var value = rp.Value;
                        // 8.2.2.1 - If the term definition for property in the active context indicates that property is a reverse property
                        var td = activeContext.GetTerm(property);
                        if (td!=null && td.Reverse)
                        {
                            // 8.2.2.1.1 - If the term definition for property in the active context has a container mapping of @set or compactArrays is false, and value is not an array, set value to a new array containing only value.
                            if (td.ContainerMapping == JsonLdContainer.Set || !compactArrays)
                            {
                                if (value.Type != JTokenType.Array)
                                {
                                    value = new JArray(value);
                                }
                            }
                            // 8.2.2.1.2 - If property is not a member of result, add one and set its value to value.
                            if (resultObject.Property(property) == null)
                            {
                                resultObject.Add(property, value);
                            }
                            // 8.2.2.1.3 - Otherwise, if the value of the property member of result is not an array, set it to a new array containing only the value. Then append value to its value if value is not an array, otherwise append each of its items.
                            else
                            {
                                var propertyValue = resultObject[property];
                                if (propertyValue.Type != JTokenType.Array)
                                {
                                    propertyValue = new JArray(propertyValue);
                                }
                                if (value.Type == JTokenType.Array)
                                {
                                    foreach (var item in value)
                                    {
                                        (propertyValue as JArray).Add(item);
                                    }
                                }
                                else
                                {
                                    (propertyValue as JArray).Add(value);
                                }
                            }
                            // 8.2.2.1.4 - Remove the property member from compacted value.
                            (compactedValue as JObject).Remove(property);
                        }
                    }
                    // 8.2.3 - If compacted value has some remaining members, i.e., it is not an empty dictionary:
                    if ((compactedValue as JObject).Properties().Any())
                    {
                        // 8.2.3.1 - Initialize alias to the result of using the IRI Compaction algorithm, passing active context, inverse context, @reverse for iri, and true for vocab.
                        var alias = CompactIri(activeContext, inverseContext, "@reverse", vocab: true);
                        // 8.2.3.2 - Set the value of the alias member of result to compacted value.
                        resultObject.Add(alias, compactedValue);
                    }
                    // 8.2.4 - Continue with the next expanded property from element.
                    continue;
                }
                // 8.3 - If expanded property is @index and active property has a container mapping in active context that is @index, then the compacted result will be inside of an @index container, drop the @index property by continuing to the next expanded property.
                var activePropertyTermDefinition = activeProperty == null ? null : activeContext.GetTerm(activeProperty);
                if (expandedProperty.Equals("@index") &&
                    activePropertyTermDefinition != null &&
                    activePropertyTermDefinition.ContainerMapping == JsonLdContainer.Index)
                {
                    continue;
                }
                // 8.4 - Otherwise, if expanded property is @index, @value, or @language:
                else if (expandedProperty.Equals("@index") ||
                         expandedProperty.Equals("@value") ||
                         expandedProperty.Equals("@language"))
                {
                    // 8.4.1 - Initialize alias to the result of using the IRI Compaction algorithm, passing active context, inverse context, expanded property for iri, and true for vocab.
                    var alias = CompactIri(activeContext, inverseContext, expandedProperty, vocab: true);
                    // 8.4.2 - Add a member alias to result whose value is set to expanded value and continue with the next expanded property.
                    resultObject.Add(alias, expandedValue);
                    continue;
                }
                // 8.5 - If expanded value is an empty array:
                if (expandedValue is JArray && (expandedValue as JArray).Count == 0)
                {
                    // 8.5.1 - Initialize item active property to the result of using the IRI Compaction algorithm, passing active context, inverse context, expanded property for iri, expanded value for value, true for vocab, and inside reverse.
                    var itemActiveProperty = CompactIri(activeContext, inverseContext, expandedProperty, expandedValue,
                        true, insideReverse);
                    // 8.5.2 - If the term definition for item active property in the active context has a @nest member, that value (nest term) must be @nest, 
                    // or a term definition in the active context that expands to @nest, otherwise an invalid @nest value error has been detected, and processing is aborted. 
                    // If result does not have the key that equals nest term, initialize it to an empty JSON object (nest object). 
                    // If nest object does not have the key that equals item active property, set this key's value in nest object to an empty array.
                    // Otherwise, if the key's value is not an array, then set it to one containing only the value.
                    var itemActivePropertyTermDefinition = activeContext.GetTerm(itemActiveProperty);
                    if (itemActivePropertyTermDefinition != null && itemActivePropertyTermDefinition.Nest != null)
                    {
                        var nestTerm = ExpandIri(activeContext, itemActivePropertyTermDefinition.Nest);
                        if (!"@nest".Equals(nestTerm))
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                                $"The @nest member for the term {itemActiveProperty} does not expand to '@nest'");
                        if (resultObject.Property(nestTerm) == null)
                        {
                            resultObject.Add(nestTerm, new JObject());
                        }
                        var nestObject = resultObject[nestTerm] as JObject;
                        if (nestObject.Property(itemActiveProperty) == null)
                        {
                            nestObject.Add(itemActiveProperty, new JArray());
                        }
                        else
                        {
                            var v = nestObject[itemActiveProperty];
                            if (v.Type != JTokenType.Array)
                            {
                                nestObject[itemActiveProperty] = new JArray(v);
                            }
                        }
                    }
                    // 8.5.3 - Otherwise, if result does not have the key that equals item active property, set this key's value in result to an empty array. 
                    // Otherwise, if the key's value is not an array, then set it to one containing only the value.
                    else
                    {
                        if (resultObject.Property(itemActiveProperty) == null)
                        {
                            resultObject.Add(itemActiveProperty, new JArray());
                        }
                        else
                        {
                            var v = resultObject[itemActiveProperty];
                            if (v.Type != JTokenType.Array)
                            {
                                resultObject[itemActiveProperty] = new JArray(v);
                            }
                        }
                    }

                }
                // 8.6 - At this point, expanded value must be an array due to the Expansion algorithm. For each item expanded item in expanded value:
                foreach (var expandedItem in (expandedValue as JArray))
                {
                    // 8.6.1 - Initialize item active property to the result of using the IRI Compaction algorithm, passing active context, inverse context, 
                    // expanded property for iri, expanded item for value, true for vocab, and inside reverse.
                    var itemActiveProperty = CompactIri(activeContext, inverseContext, expandedProperty, expandedItem, true,
                        insideReverse);
                    JObject nestResult;
                    // 8.6.2 - If the term definition for item active property in the active context has a @nest member, 
                    // that value (nest term) must be @nest, or a term definition in the active context that expands to @nest, 
                    // otherwise an invalid @nest value error has been detected, and processing is aborted. 
                    // Set nest result to the value of nest term in result, initializing it to a new dictionary, if necessary; otherwise set nest result to result.
                    var itemActivePropertyTermDefinition = activeContext.GetTerm(itemActiveProperty);
                    if (itemActivePropertyTermDefinition?.Nest != null)
                    {
                        var nestTerm = itemActivePropertyTermDefinition.Nest;
                        var expandedNestTerm = ExpandIri(activeContext, itemActivePropertyTermDefinition.Nest, vocab:true);
                        if (!"@nest".Equals(expandedNestTerm))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                                $"The {nestTerm} member for the term {itemActiveProperty} does not expand to '@nest'");
                        }
                        if (resultObject.Property(nestTerm) == null)
                        {
                            resultObject.Add(nestTerm, new JObject());
                        }
                        nestResult = resultObject[nestTerm] as JObject;
                    }
                    else
                    {
                        nestResult = resultObject;
                    }
                    // 8.6.3 - Initialize container to null. If there is a container mapping for item active property in active context, set container to its value.
                    var container = itemActivePropertyTermDefinition == null
                        ? JsonLdContainer.Null
                        : itemActivePropertyTermDefinition.ContainerMapping;
                    // 8.6.4 - Initialize compacted item to the result of using this algorithm recursively, passing active context, inverse context, item active property for active property, expanded item for element if it does not contain the key @list, otherwise pass the key's associated value for element.
                    var listProperty = (expandedItem as JObject)?.Property("@list");
                    var compactedItem = CompactAlgorithm(activeContext, inverseContext, itemActiveProperty,
                        listProperty != null ? listProperty.Value : expandedItem);
                    // 8.6.5 - If expanded item is a list object:
                    if (IsListObject(expandedItem))
                    {
                        // 8.6.5.1 - If compacted item is not an array, then set it to an array containing only compacted item.
                        if (!(compactedItem is JArray))
                        {
                            compactedItem = new JArray(compactedItem);
                        }
                        // 8.6.5.2 - If container is not @list:
                        if (container != JsonLdContainer.List)
                        {
                            // 8.6.5.2.1 - Convert compacted item to a list object by setting it to a dictionary containing key-value pair where the key is the result of the IRI Compaction algorithm, passing active context, inverse context, @list for iri, and compacted item for value.
                            compactedItem = new JObject(
                                new JProperty(CompactIri(activeContext, inverseContext, "@list", vocab:true), compactedItem));
                            // 8.6.5.2.2 - If expanded item contains the key @index, then add a key-value pair to compacted item where the key is the result of the IRI Compaction algorithm, passing active context, inverse context, @index as iri, and the value associated with the @index key in expanded item as value.
                            var indexProperty = (expandedItem as JObject)?.Property("@index");
                            if (indexProperty != null)
                            {
                                (compactedItem as JObject).Add(CompactIri(activeContext, inverseContext, "@index", vocab:true),
                                    indexProperty.Value);
                            }
                        }
                        // 8.6.5.3 - Otherwise, item active property must not be a key in nest result because there cannot be two list objects associated with an active property that has a container mapping; a compaction to list of lists error has been detected and processing is aborted.
                        else
                        {
                            if (nestResult.Property(itemActiveProperty) != null)
                            {
                                throw new JsonLdProcessorException(JsonLdErrorCode.CompactionToListOfLists,
                                    $"Compaction to list of lists at property {activeProperty}.{itemActiveProperty}");
                            }
                        }
                    }
                    // 8.6.6 - If container is @language, @index, @id, or @type:
                    if (container == JsonLdContainer.Language ||
                        container == JsonLdContainer.Index ||
                        container == JsonLdContainer.Id ||
                        container == JsonLdContainer.Type)
                    {
                        // 8.6.6.1 - If item active property is not a key in nest result, initialize it to an empty dictionary. Initialize map object to the value of item active property in nest result.
                        if (nestResult.Property(itemActiveProperty) == null)
                        {
                            nestResult.Add(itemActiveProperty, new JObject());
                        }
                        var mapObject = nestResult[itemActiveProperty] as JObject;
                        // 8.6.6.2 - Set compacted container to the result of calling the IRI Compaction algorithm passing active context, container as iri, and true for vocab.
                        var compactedContainer = CompactIri(activeContext, inverseContext, ContainerAsString(container),
                            vocab: true);
                        // 8.6.6.3 - Initialize map key to the value associated with with the key that equals container in expanded item.
                        var mapKey = (expandedItem[ContainerAsString(container)] as JValue)?.Value<string>();
                        // 8.6.6.4 - If container is @language and compacted item contains the key @value, then set compacted item to the value associated with its @value key.
                        if (container == JsonLdContainer.Language &&
                            (compactedItem as JObject)?.Property("@value") != null)
                        {
                            compactedItem = (compactedItem as JObject)["@value"];
                        }
                        // 8.6.6.5 - If container is @id, set map key to the result of calling the IRI Compaction algorithm passing active context and the value associated with the key that equals compacted container in compacted item as iri.
                        if (container == JsonLdContainer.Id)
                        {
                            mapKey = CompactIri(activeContext, inverseContext,
                                compactedItem[compactedContainer].Value<string>());
                            // KA: Not in spec, but I think implied/required to pass unit tests.
                            (compactedItem as JObject).Remove(compactedContainer);
                        }
                        // 8.6.6.6 - If container is @type, set map key to the result of calling the IRI Compaction algorithm passing active context, 
                        // the the first value associated with the key that equals compacted container in compacted item as iri, and true for vocab. 
                        // If there are remaining values in compacted item for compacted container, set the value of compacted container in compacted 
                        // value to those remaining values. Otherwise, remove that key-value pair from compacted item.
                        if (container == JsonLdContainer.Type)
                        {
                            var types = compactedItem[compactedContainer];
                            var firstType = types is JArray
                                ? (types as JArray)[0].Value<string>()
                                : types.Value<string>();
                            mapKey = CompactIri(activeContext, inverseContext, firstType, vocab: true);

                            var containerValues = compactedItem[compactedContainer];
                            if (containerValues is JArray && (containerValues as JArray).Count > 1)
                            {
                                // TODO: Check - is this really compactedValue and not compactedItem that should be updated?
                                //if (compactedValue == null) compactedValue = new JObject();
                                //(compactedValue as JObject)[compactedContainer] =
                                //    new JArray((containerValues as JArray).Skip(1));
                                var remainingValues = new JArray((containerValues as JArray).Skip(1));
                                if (compactArrays && remainingValues.Count == 1)
                                {
                                    compactedItem[compactedContainer] = remainingValues[0];
                                }
                                else
                                {
                                    compactedItem[compactedContainer] = remainingValues;
                                }
                            }
                            else
                            {
                                (compactedItem as JObject).Remove(compactedContainer);
                            }
                        }

                        // 8.6.6.7 - If map key is not a key in map object, then set this key's value in map object to compacted item. 
                        // Otherwise, if the value is not an array, then set it to one containing only the value and then append compacted item to it.
                        if (mapObject.Property(mapKey) == null)
                        {
                            mapObject.Add(mapKey, compactedItem);
                        }
                        else
                        {
                            if (mapObject[mapKey] is JArray)
                            {
                                (mapObject[mapKey] as JArray).Add(compactedItem);
                            }
                            else
                            {
                                mapObject[mapKey] = new JArray(mapObject[mapKey], compactedItem);
                            }
                        }
                    }
                    // 8.6.7 - Otherwise
                    else
                    {
                        // 8.6.7.1 - If compactArrays is false, container is @set or @list, or expanded property is @list or @graph and compacted item is not an array, set it to a new array containing only compacted item.
                        if (compactArrays == false || container == JsonLdContainer.Set ||
                            container == JsonLdContainer.List ||
                            "@list".Equals(expandedProperty) || "@graph".Equals(expandedProperty))
                        {
                            if (!(compactedItem is JArray))
                            {
                                compactedItem = new JArray(compactedItem);
                            }
                        }
                        // 8.6.7.2 - If item active property is not a key in result then add the key - value pair, (item active property-compacted item), to nest result.
                        if (nestResult.Property(itemActiveProperty) == null)
                        {
                            nestResult.Add(itemActiveProperty, compactedItem);
                        }
                        // 8.6.7.3 - Otherwise, if the value associated with the key that equals item active property in nest result is not an array, 
                        // set it to a new array containing only the value. Then append compacted item to the value if compacted item is not an array, otherwise, concatenate it.
                        else
                        {
                            if (!(nestResult[itemActiveProperty] is JArray))
                            {
                                nestResult[itemActiveProperty] = new JArray(nestResult[itemActiveProperty]);
                            }
                            if (compactedItem is JArray)
                            {
                                foreach (var item in (compactedItem as JArray))
                                {
                                    (nestResult[itemActiveProperty] as JArray).Add(item);
                                }
                            }
                            else
                            {
                                (nestResult[itemActiveProperty] as JArray).Add(compactedItem);
                            }
                        }
                    }
                }
            }
            // 9 - Return result.
            return result;
        }

        /// <summary>
        /// Flattens the given input and compacts it using the passed context according to the steps in the JSON-LD Flattening algorithm
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
        /// <param name="element">The element to be processed</param>
        /// <param name="options">JSON-LD processor options</param>
        /// <returns>The generated node map dictionary as a JObject instance</returns>
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
            // 1. Initialize result to an empty dictionary.
            var result = new JObject();

            // 2. Initialize default language to @none. If the active context has a default language, set default language to it.
            var defaultLanguage = "@none";
            if (activeContext.Language != null) defaultLanguage = activeContext.Language;

            // 3. For each key term and value term definition in the active context, ordered by shortest term first (breaking ties by choosing the lexicographically least term):
            foreach (var term in activeContext.Terms.OrderBy(t => t.Length).ThenBy(t => t))
            {
                var termDefinition = activeContext.GetTerm(term);
                // 3.1 - If the term definition is null, term cannot be selected during compaction, so continue to the next term.
                if (termDefinition == null)
                {
                    continue;
                }
                // 3.2 - Initialize container to @none. If there is a container mapping in term definition, set container to its associated value.
                var container = ContainerAsString(termDefinition.ContainerMapping);
                // 3.3 - Initialize iri to the value of the IRI mapping for the term definition.
                var iri = termDefinition.IriMapping;
                // 3.4 - If iri is not a key in result, add a key-value pair where the key is iri and the value is an empty dictionary to result.
                if (result.Property(iri) == null)
                {
                    result.Add(iri, new JObject());
                }
                // 3.5 - Reference the value associated with the iri member in result using the variable container map.
                var containerMap = result[iri] as JObject;
                // 3.6 - If container map has no container member, create one and set its value to a new dictionary with two members. 
                // The first member is @language and its value is a new empty dictionary, 
                // the second member is @type and its value is a new empty dictionary.
                if (containerMap.Property(container) == null)
                {
                    containerMap.Add(container, new JObject(
                        new JProperty("@language", new JObject()),
                        new JProperty("@type", new JObject())));
                }
                // 3.7 - Reference the value associated with the container member in container map using the variable type/language map.
                var typeLanguageMap = containerMap[container] as JObject;
                
                // 3.8 - If the term definition indicates that the term represents a reverse property:
                if (termDefinition.Reverse)
                {
                    // 3.8.1 - Reference the value associated with the @type member in type/language map using the variable type map.
                    var typeMap = typeLanguageMap["@type"] as JObject;

                    // 3.8.2 - If type map does not have an @reverse member, create one and set its value to the term being processed.
                    if (typeMap.Property("@reverse") == null)
                    {
                        typeMap.Add("@reverse", term);
                    }
                }
                // 3.9 - Otherwise, if term definition has a type mapping:
                else if (termDefinition.TypeMapping != null)
                {
                    // 3.9.1 - Reference the value associated with the @type member in type/language map using the variable type map.
                    var typeMap = typeLanguageMap["@type"] as JObject;

                    // 3.9.2 - If type map does not have a member corresponding to the type mapping in term definition, create one and set its value to the term being processed.
                    if (typeMap.Property(termDefinition.TypeMapping) == null)
                    {
                        typeMap.Add(termDefinition.TypeMapping, term);
                    }
                }
                // 3.10 - Otherwise, if term definition has a language mapping (might be null):
                else if (termDefinition.HasLanguageMapping)
                {
                    // 3.10.1 - Reference the value associated with the @language member in type/language map using the variable language map.
                    var languageMap = typeLanguageMap["@language"] as JObject;
                    // 3.10.2 - If the language mapping equals null, set language to @null; otherwise set it to the language code in language mapping.
                    var language = termDefinition.LanguageMapping ?? "@null";
                    // 3.10.3 - If language map does not have a language member, create one and set its value to the term being processed.
                    if (languageMap.Property(language) == null)
                    {
                        languageMap.Add(language, term);
                    }
                }
                // 3.11 - Otherwise
                else
                {
                    // 3.11.1 - Reference the value associated with the @language member in type/language map using the variable language map.
                    var languageMap = typeLanguageMap["@language"] as JObject;

                    // 3.11.2 - If language map does not have a default language member, create one and set its value to the term being processed.
                    if (languageMap.Property(defaultLanguage) == null)
                    {
                        languageMap.Add(defaultLanguage, term);
                    }

                    // 3.11.3 - If language map does not have an @none member, create one and set its value to the term being processed.
                    if (languageMap.Property("@none") == null)
                    {
                        languageMap.Add("@none", term);
                    }

                    // 3.11.4 - Reference the value associated with the @type member in type / language map using the variable type map.
                    var typeMap = typeLanguageMap["@type"] as JObject;

                    // 3.11.5 - If type map does not have an @none member, create one and set its value to the term being processed.
                    if (typeMap.Property("@none") == null)
                    {
                        typeMap.Add("@none", term);
                    }
                }
            }
            return result;
        }

        private string CompactIri(JsonLdContext activeContext, JObject inverseContext, string iri, JToken value = null,
            bool vocab = false, bool reverse = false)
        {
            // 1 - If iri is null, return null.
            if (iri == null) return null;
            // 2 - If vocab is true and iri is a key in inverse context:
            if (vocab && inverseContext.Property(iri) != null)
            {
                // 2.1 - Initialize default language to active context's default language, if it has one, otherwise to @none.
                var defaultLanguage = activeContext.Language ?? "@none";

                // 2.2 - Initialize containers to an empty array. This array will be used to keep track of an ordered list of preferred container mapping for a term, based on what is compatible with value.
                var containers = new List<string>();

                // 2.3 - Initialize type / language to @language, and type / language value to @null.These two variables will keep track of the preferred type mapping or language mapping for a term, based on what is compatible with value.
                var typeLanguage = "@language";
                var typeLanguageValue = "@null";

                // 2.4 - If value is a dictionary, then for the keywords @index, @id, and @type, if value contains that keyword, append it to containers.
                var valueObject = value as JObject;
                if (valueObject != null)
                {
                    foreach (var keyword in new[] {"@index", "@id", "@type"})
                    {
                        if (valueObject.Property(keyword) != null)
                        {
                            containers.Add(keyword);
                        }
                    }
                }

                // 2.5 - If reverse is true, set type/ language to @type, type/ language value to @reverse, and append @set to containers.
                if (reverse)
                {
                    typeLanguage = "@type";
                    typeLanguageValue = "@reverse";
                    containers.Add("@set");
                }
                // 2.6 - Otherwise, if value is a list object, then set type/language and type/language value to the most specific values that work for all items in the list as follows:
                else if (IsListObject(value))
                {
                    // 2.6.1 - If @index is a not key in value, then append @list to containers.
                    if (valueObject.Property("@index") == null)
                    {
                        containers.Add("@list");
                    }
                    // 2.6.2 - Initialize list to the array associated with the key @list in value.
                    var list = valueObject["@list"] as JArray;
                    // 2.6.3 - Initialize common type and common language to null.If list is empty, set common language to default language.
                    string commonType = null, commonLanguage = null;
                    if (list.Count == 0) commonLanguage = defaultLanguage;
                    // 2.6.4 - For each item in list:
                    foreach (var item in list)
                    {
                        // 2.6.4.1 - Initialize item language to @none and item type to @none.
                        var itemLanguage = "@none";
                        var itemType = "@none";
                        // 2.6.4.2 - If item contains the key @value:
                        var itemObject = item as JObject;
                        if (itemObject?.Property("@value") != null)
                        {
                            // 2.6.4.2.1 - If item contains the key @language, then set item language to its associated value.
                            if (itemObject.Property("@language") != null)
                            {
                                itemLanguage = itemObject["@language"].Value<string>();
                            }
                            // 2.6.4.2.2 - Otherwise, if item contains the key @type, set item type to its associated value.
                            else if (itemObject.Property("@type") != null)
                            {
                                itemType = itemObject["@type"].Value<string>();
                            }
                            // 2.6.4.2.3 - Otherwise, set item language to @null.
                            else
                            {
                                itemLanguage = "@null";
                            }
                        }
                        // 2.6.4.3 - Otherwise, set item type to @id.
                        else
                        {
                            itemType = "@id";
                        }
                        // 2.6.4.4 - If common language is null, set it to item language.
                        if (commonLanguage == null)
                        {
                            commonLanguage = itemLanguage;
                        }
                        // 2.6.4.5 - Otherwise, if item language does not equal common language and item contains the key @value, then set common language to @none because list items have conflicting languages.
                        else if (!commonLanguage.Equals(itemLanguage) && itemObject?.Property("@value") != null)
                        {
                            commonLanguage = "@none";
                        }
                        // 2.6.4.6 - If common type is null, set it to item type.
                        if (commonType == null)
                        {
                            commonType = itemType;
                        }
                        // 2.6.4.7 - Otherwise, if item type does not equal common type, then set common type to @none because list items have conflicting types.
                        else if (!commonType.Equals(itemType))
                        {
                            commonType = "@none";
                        }
                        // 2.6.4.8 - If common language is @none and common type is @none, then stop processing items in the list because it has been detected that there is no common language or type amongst the items.
                        if (commonLanguage == "@none" && commonType == "@none")
                        {
                            break;
                        }
                    }
                    // 2.6.5 - If common language is null, set it to @none.
                    if (commonLanguage == null) commonLanguage = "@none";
                    // 2.6.6 - If common type is null, set it to @none.
                    if (commonType == null) commonType = "@none";
                    // 2.6.7 - If common type is not @none then set type/language to @type and type/language value to common type.
                    if (!commonType.Equals("@none"))
                    {
                        typeLanguage = "@type";
                        typeLanguageValue = commonType;
                    }
                    // 2.6.8 - Otherwise, set type/language value to common language.
                    else
                    {
                        typeLanguageValue = commonLanguage;
                    }
                }
                // 2.7 - Otherwise:
                else
                {
                    // 2.7.1 - If value is a value object:
                    if (IsValueObject(value))
                    {
                        // 2.7.1.1 - If value contains the key @language and does not contain the key @index, then set type/language value to its associated value and append @language to containers.
                        if (valueObject.Property("@language") != null && valueObject.Property("@index") == null)
                        {
                            typeLanguageValue = valueObject["@language"].Value<string>();
                            containers.Add("@language");
                        }
                        // 2.7.1.2 - Otherwise, if value contains the key @type, then set type/language value to its associated value and set type/language to @type.
                        else if (valueObject.Property("@type") != null)
                        {
                            typeLanguageValue = valueObject["@type"].Value<string>();
                            typeLanguage = "@type";
                        }
                    }
                    // 2.7.2 - Otherwise, set type/language to @type and set type/language value to @id.
                    else
                    {
                        typeLanguage = "@type";
                        typeLanguageValue = "@id";
                    }
                    // 2.7.3 - Append @set to containers.
                    containers.Add("@set");
                }
                // 2.8 - Append @none to containers. This represents the non-existence of a container mapping, and it will be the last container mapping value to be checked as it is the most generic.
                containers.Add("@none");
                // 2.9 - If type/language value is null, set it to @null. This is the key under which null values are stored in the inverse context entry.
                if (typeLanguageValue == null) typeLanguageValue = "@null";
                // 2.10 - Initialize preferred values to an empty array. This array will indicate, in order, the preferred values for a term's type mapping or language mapping.
                var preferredValues = new List<string>();
                // 2.11 - If type/language value is @reverse, append @reverse to preferred values.
                if (typeLanguageValue == "@reverse")
                {
                    preferredValues.Add("@reverse");
                }
                // 2.12 - If type/language value is @id or @reverse and value has an @id member:
                if ((typeLanguageValue == "@id" || typeLanguageValue == "@reverse") &&
                    valueObject?.Property("@id") != null)
                {
                    // 2.12.1 - If the result of using the IRI compaction algorithm, passing active context, inverse context, 
                    // the value associated with the @id key in value for iri, true for vocab, and true for document relative 
                    // has a term definition in the active context with an IRI mapping that equals the value associated with 
                    // the @id key in value, then append @vocab, @id, and @none, in that order, to preferred values.
                    var idValue = valueObject["@id"].Value<string>();
                    var compactIriResult = CompactIri(activeContext, inverseContext, idValue, vocab:true);
                    var termDefinition = activeContext.GetTerm(compactIriResult);
                    if (idValue.Equals(termDefinition?.IriMapping))
                    {
                        preferredValues.Add("@vocab");
                        preferredValues.Add("@id");
                        preferredValues.Add("@none");
                    }
                    // 2.12.2 - Otherwise, append @id, @vocab, and @none, in that order, to preferred values.
                    else
                    {
                        preferredValues.Add("@id");
                        preferredValues.Add("@vocab");
                        preferredValues.Add("@none");
                    }
                }
                // 2.13 - Otherwise, append type/language value and @none, in that order, to preferred values.
                else
                {
                    preferredValues.Add(typeLanguageValue);
                    preferredValues.Add("@none");
                }
                // 2.14 - Initialize term to the result of the Term Selection algorithm, passing inverse context, iri, containers, type/language, and preferred values.
                var term = SelectTerm(inverseContext, iri, containers, typeLanguage, preferredValues);
                // 2.15 - If term is not null, return term.
                if (term != null) return term;
            }
            // 3 - At this point, there is no simple term that iri can be compacted to. If vocab is true and active context has a vocabulary mapping:
            if (vocab && activeContext.Vocab != null)
            {
                // 3.1 - If iri begins with the vocabulary mapping's value but is longer, then initialize suffix to the substring of iri that does not match. 
                // If suffix does not have a term definition in active context, then return suffix.
                if (iri.StartsWith(activeContext.Vocab) && iri.Length > activeContext.Vocab.Length)
                {
                    var suffix = iri.Substring(activeContext.Vocab.Length);
                    if (activeContext.GetTerm(suffix) == null) return suffix;
                }
            }
            // 4 - The iri could not be compacted using the active context's vocabulary mapping. 
            // Try to create a compact IRI, starting by initializing compact IRI to null. 
            // This variable will be used to tore the created compact IRI, if any.
            string compactIri = null;
            // 5 - For each key term and value term definition in the active context:
            foreach (var term in activeContext.Terms)
            {
                // 5.1 - If the term contains a colon (:), then continue to the next term because terms with colons can't be used as prefixes.
                if (term.Contains(":")) continue;
                var termDefinition = activeContext.GetTerm(term);
                // 5.2 - If the term definition is null, its IRI mapping equals iri, or its IRI mapping is not a substring at the beginning of iri, the term cannot be used 
                // as a prefix because it is not a partial match with iri.Continue with the next term.
                if (termDefinition == null) continue;
                if (termDefinition.IriMapping == iri) continue;
                if (!iri.StartsWith(termDefinition.IriMapping)) continue;
                // 5.3 - Initialize candidate by concatenating term, a colon (:), and the substring of iri that follows after the value of the term definition's IRI mapping.
                var candidate = term + ":" + iri.Substring(termDefinition.IriMapping.Length);
                // 5.4 - If either compact IRI is null or candidate is shorter or the same length but lexicographically less than compact IRI 
                // and candidate does not have a term definition in active context or if the term definition has an IRI mapping that equals iri 
                // and value is null, set compact IRI to candidate.
                if (compactIri == null ||
                    compactIri.Length > candidate.Length ||
                    (compactIri.Length == candidate.Length && candidate.CompareTo(compactIri) < 0))
                {
                    termDefinition = activeContext.GetTerm(candidate);
                    if (termDefinition == null || (termDefinition.IriMapping.Equals(iri) && value == null))
                    {
                        compactIri = candidate;
                    }
                }
            }
            // 6 - If compact IRI is not null, return compact IRI.
            if (compactIri != null) return compactIri;
            // 7 - If vocab is false then transform iri to a relative IRI using the document's base IRI.
            if (!vocab)
            {
                var baseIri = activeContext.HasBase ? activeContext.Base : _options.Base;
                if (IsAbsoluteIri(iri) && baseIri != null)
                {
                    return baseIri.MakeRelativeUri(new Uri(iri)).ToString();
                }
            }
            // 8 - Finally, return iri as is.
            return iri;
        }

        private string SelectTerm(JObject inverseContext, string iri, List<string> containers, string typeLanguage, List<string> preferredValues)
        {
            // 1 - Initialize container map to the value associated with iri in the inverse context.
            var containerMap = inverseContext[iri] as JObject;
            // 2 - For each item container in containers:
            foreach (var container in containers)
            {
                // 2.1 - If container is not a key in container map, then there is no term with a matching container mapping for it, so continue to the next container.
                if (containerMap.Property(container) == null) continue;

                // 2.2 - Initialize type/ language map to the value associated with the container member in container map.
                var typeLanguageMap = containerMap[container];

                // 2.3 - Initialize value map to the value associated with type / language member in type / language map.
                var valueMap = typeLanguageMap[typeLanguage] as JObject;

                // 2.4 - For each item in preferred values:
                foreach (var item in preferredValues)
                {
                    // 2.4.1 - If item is not a key in value map, then there is no term with a matching type mapping or language mapping, so continue to the next item.
                    if (valueMap.Property(item) == null) continue;
                    // 2.4.2 - Otherwise, a matching term has been found, return the value associated with the item member in value map.
                    return valueMap[item].Value<string>();
                }
            }
            // 3 - No matching term has been found. Return null.
            return null;
        }

        private JToken CompactValue(JsonLdContext activeContext, JObject inverseContext, string activeProperty,
            JObject value)
        {
            var activeTermDefinition = activeProperty == null
                ? new JsonLdTermDefinition()
                : activeContext.GetTerm(activeProperty) ?? new JsonLdTermDefinition();
            var indexProperty = value.Property("@index");

            // 1 - Initialize number members to the number of members value contains.
            var numberMembers = value.Properties().Count();

            // 2 - If value has an @index member and the container mapping associated to active property is set to @index, decrease number members by 1.
            if (indexProperty != null && activeTermDefinition.ContainerMapping == JsonLdContainer.Index)
            {
                numberMembers -= 1;
            }
            // 3 - If number members is greater than 2, return value as it cannot be compacted.
            if (numberMembers > 2) return value;

            // 4 - If value has an @id member:
            var idProperty = value.Property("@id");
            if (idProperty != null)
            {
                // 4.1 - If number members is 1 and the type mapping of active property is set to @id, return the result of using the IRI compaction algorithm, passing active context, inverse context, and the value of the @id member for iri.
                if (numberMembers == 1 && "@id".Equals(activeTermDefinition.TypeMapping))
                {
                    return new JValue(CompactIri(activeContext, inverseContext, idProperty.Value.Value<string>()));
                }
                // 4.2 - Otherwise, if number members is 1 and the type mapping of active property is set to @vocab, return the result of using the IRI compaction algorithm, passing active context, inverse context, the value of the @id member for iri, and true for vocab.
                else if (numberMembers == 1 && "@vocab".Equals(activeTermDefinition.TypeMapping))
                {
                    return new JValue(CompactIri(activeContext, inverseContext, idProperty.Value.Value<string>(),
                        vocab: true));
                }
                // 4.3 - Otherwise, return value as is.
                else
                {
                    return value;
                }
            }
            // 5 - Otherwise, if value has an @type member whose value matches the type mapping of active property, return the value associated with the @value member of value.
            else if (value.Property("@type") != null &&
                     value["@type"].Value<string>().Equals(activeTermDefinition.TypeMapping))
            {
                return value["@value"];
            }
            // 6 - Otherwise, if value has an @language member whose value matches the language mapping of active property, return the value associated with the @value member of value.
            // KA: Spec is not specific about this, but from the unit tests it seems that the "language mapping of active property" should take into account the default language in the active context
            else if (value.Property("@language") != null &&
                     value["@language"].Value<string>().Equals(activeTermDefinition.HasLanguageMapping  ? activeTermDefinition.LanguageMapping : activeContext.Language))
            {
                return value["@value"];
            }
            // 7 - Otherwise, if number members equals 1 and either the value of the @value member is not a string, or the active context has no default language, or the language mapping of active property is set to null,, return the value associated with the @value member.
            else if (numberMembers == 1 &&
                     (value["@value"].Type != JTokenType.String ||
                      activeContext.Language == null ||
                      (activeTermDefinition.HasLanguageMapping && activeTermDefinition.LanguageMapping == null)))
            {
                return value["@value"];
            }
            // 8 - Otherwise, return value as is.
            return value;
        }

        /// <summary>
        /// Applies the JSON-LD Framing algorithm to the specified input JSON object
        /// </summary>
        /// <param name="input">The RDF data to be framed as a JSON-LD document</param>
        /// <param name="frame">The JSON-LD frame to be applied</param>
        /// <param name="options">Processor options</param>
        /// <returns>A JSON object representing the framed RDF data</returns>
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
            Abort
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
        /// Determine if a JSON token is a JSON-LD value object
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True of <paramref name="token"/> is a <see cref="JObject"/> with a <code>@value</code> property, false otherwise.</returns>
        public static bool IsValueObject(JToken token)
        {
            return ((token as JObject)?.Property("@value")) != null;
        }


        /// <summary>
        /// Determine if a JSON token is a JSON-LD list object
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True of <paramref name="token"/> is a <see cref="JObject"/> with a <code>@list</code> property, false otherwise.</returns>
        public static bool IsListObject(JToken token)
        {
            return ((token as JObject)?.Property("@list")) != null;
        }

        private bool IsScalar(JToken token)
        {
            return !(token.Type == JTokenType.Array || token.Type == JTokenType.Object);
        }

        private bool IsAbsoluteIri(JToken token)
        {
            var value = token as JValue;
            if (value == null) return false;
            if (value.Type != JTokenType.String) return false;
            return IsAbsoluteIri(value.Value<string>());
        }

        private bool IsAbsoluteIri(string value) {
            var ix = value.IndexOf(':');
            return ix > 0 && (ix == value.Length -1 || value.IndexOf("://", StringComparison.Ordinal) == ix);
        }

        private bool IsRelativeIri(JToken token)
        {
            var value = token as JValue;
            if (value == null) return false;
            if (value.Type != JTokenType.String) return false;
            return IsRelativeIri(value.Value<string>());
        }

        /// <summary>
        /// Determine if the specified string is a relative IRI
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsRelativeIri(string value)
        {
            return Uri.TryCreate(value, UriKind.Relative, out Uri result);
        }

        /// <summary>
        /// Determine if the specified string is a JSON-LD keyword
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if <paramref name="value"/> is a JSON-LD keyword, false otherwise</returns>
        public static bool IsKeyword(string value)
        {
            return JsonLdKeywords.Contains(value) || JsonLdFramingKeywords.Contains(value);
        }

        /// <summary>
        /// Determine if the specified string is a JSON-LD framing keyword
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFramingKeyword(string value)
        {
            return JsonLdFramingKeywords.Contains(value);
        }

        /// <summary>
        /// Determine if the specified string is a blank node identifier
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
    }
}
