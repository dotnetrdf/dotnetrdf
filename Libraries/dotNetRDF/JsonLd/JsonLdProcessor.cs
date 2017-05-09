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
        /// <summary>
        /// Create a new processor instance
        /// </summary>
        /// <param name="options">JSON-LD processing options</param>
        public JsonLdProcessor(JsonLdProcessorOptions options) {
            _options = options;
            ProcessingMode = _options.Syntax;
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

        public JsonLdSyntax? ProcessingMode
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
                    result = ProcessContext(activeContext, (context as JValue).Value<string>(), remoteContexts);
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
                    // TODO: Set processing mode
                    // 3.5.2: If processing mode is not set, and json-ld-1.1 is not a prefix of processing mode, a processing mode conflict error has been detected and processing is aborted.
                    // 3.5.3: Set processing mode, to json-ld-1.1, if not already set.
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
                if (this.ProcessingMode == null) this.ProcessingMode = JsonLdSyntax.JsonLd10;
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

        public void CreateTermDefinition(JsonLdContext activeContext, JObject localContext, string term, Dictionary<string, bool> defined = null)
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
                if (this.ProcessingMode == JsonLdSyntax.JsonLd10 &&
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
                if (this.ProcessingMode == JsonLdSyntax.JsonLd10)
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
                if (this.ProcessingMode == JsonLdSyntax.JsonLd10)
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



        private IEnumerable<WebLink> ParseLinkHeaders(IEnumerable<string> linkHeaderValues)
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

        public JArray Expand(JsonLdContext activeContext, string activeProperty, JToken element)
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
            ExpandKeys(activeContext, activeProperty, elementObject, resultObject);

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
                else if (valueProperty.Value.Type != JTokenType.String && resultObject.Property("@language") != null)
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedValue, $"A value object with an @language property must have a string value for the @value property. Found a {valueProperty.Value.Type}");
                }
                // 9.4 - Otherwise, if the result has an @type member and its value is not an IRI, an invalid typed value error has been detected and processing is aborted.
                else if (typeProperty != null)
                {
                    var typeValue = typeProperty.Value;
                    if (typeValue.Type != JTokenType.String || !IsAbsoluteIri(typeValue.Value<string>()))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypedValue, "The value of the @type property of a value object must be an IRI.");
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
                if (resultObject.Properties().Count() == 1 && resultObject.Property("@id") != null)
                {
                    result = null;
                }
            }
            // 14 - Return result
            return result;
        }

        private void ExpandKeys(JsonLdContext activeContext, string activeProperty, JObject elementObject, JObject result)
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

                    // 8.4.3 - If expanded property is @id...
                    if (expandedProperty.Equals("@id"))
                    {
                        // .. and value is not a string,...
                        if (value.Type != JTokenType.String)
                        {
                            // an invalid @id value error has been detected and processing is aborted.
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue, "Invalid @id value");
                        }
                        expandedValue = ExpandIri(activeContext, value.Value<string>(), documentRelative: true);
                        /* 
                         * TODO: When the frame expansion flag is set, value may be an empty dictionary, 
                         * or an array of one or more strings. Expanded value will be an 
                         * array of one or more of these, with string values expanded using 
                         * the IRI Expansion Algorithm. 
                         */
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
                        /* TODO: When the frame expansion flag is set, value may also be an empty dictionary. */
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
                        var array = expandedValue as JArray;
                        // NOTE: The following line is supposed to ensure the array contains at least one dictionary,
                        // but it causes a failure in the JSON-LD.org test suite on the test specifically for expanding an empty graph
                        //if (array.Count == 0) { array.Add(new JObject());  }
                    }

                    // 8.4.6 - If expanded property is @value and value is not a scalar or null, an invalid value object value error has been detected and processing is aborted. Otherwise, set expanded value to value. If expanded value is null, set the @value member of result to null and continue with the next key from element. Null values need to be preserved in this case as the meaning of an @type member depends on the existence of an @value member. When the frame expansion flag is set, value may also be an empty dictionary or an array of scalar values. Expanded value will be null, or an array of one or more scalar values.
                    if (expandedProperty.Equals("@value"))
                    {
                        if (!((value.Type == JTokenType.Null) || IsScalar(value)))
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject, "The expanded value of @value must be a scalar or null.");
                        }
                        expandedValue = value;
                        if (expandedValue == null || expandedValue.Type == JTokenType.Null)
                        {
                            result["@value"] = null;
                            continue;
                        }
                        // TODO:    When the frame expansion flag is set, value may also be an empty dictionary or an array of scalar values. Expanded value will be null, or an array of one or more scalar values.
                    }

                    // 8.4.7 - If expanded property is @language and value is not a string, an invalid language-tagged string error has been detected and processing is aborted. Otherwise, set expanded value to lowercased value. When the frame expansion flag is set, value may also be an empty dictionary or an array of zero or strings. Expanded value will be an array of one or more string values converted to lower case.
                    if (expandedProperty.Equals("@language"))
                    {
                        if (value.Type == JTokenType.String)
                        {
                            expandedValue = value.Value<string>().ToLowerInvariant();
                        }
                        // TODO: When the frame expansion flag is set, value may also be an empty dictionary or an array of zero or strings. Expanded value will be an array of one or more string values converted to lower case.
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

                    // TODO: 8.4.13 - When the frame expansion flag is set, if expanded property is any other framing keyword (@explicit, @default, @embed, @explicit, @omitDefault, or @requireAll), set expanded value to the result of performing the Expansion Algorithm recursively, passing active context, active property, and value for element.

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
                    ExpandKeys(activeContext, activeProperty, nestedValue as JObject, result);
                }
            }
        }

        private bool IsValueObject(JToken token)
        {
            return ((token as JObject)?.Property("@value")) != null;
        }

        private bool IsListObject(JToken token)
        {
            return ((token as JObject)?.Property("@list")) != null;
        }

        public JToken ExpandValue(JsonLdContext activeContext, string activeProperty, JToken value)
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

        private bool IsScalar(JToken token)
        {
            return !(token.Type == JTokenType.Array || token.Type == JTokenType.Object);
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

        private bool IsAbsoluteIri(JToken token)
        {
            var value = token as JValue;
            if (value == null) return false;
            if (value.Type != JTokenType.String) return false;
            return IsAbsoluteIri(value.Value<string>());
        }

        private bool IsAbsoluteIri(string value) {
            var ix = value.IndexOf(':');
            return ix > 0 && value.IndexOf("://") == ix;
        }

        private bool IsRelativeIri(JToken token)
        {
            var value = token as JValue;
            if (value == null) return false;
            if (value.Type != JTokenType.String) return false;
            return IsRelativeIri(value.Value<string>());
        }

        private bool IsRelativeIri(string value)
        {
            return Uri.TryCreate(value, UriKind.Relative, out Uri result);
        }

        private bool IsKeyword(string value)
        {
            return JsonLdKeywords.Contains(value);
        }

        private bool IsBlankNodeIdentifier(string value)
        {
            return value.StartsWith("_:");
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

        private JsonLdContext ProcessContext(JsonLdContext activeContext, JsonLdContext result, string contextRef, List<Uri> remoteContexts)
        {
            // Set context to the result of resolving value against the base IRI
            Uri contextIri;
            if (activeContext.Base != null)
            {
                contextIri = new Uri(activeContext.Base, contextRef);
            }
            else
            {
                contextIri = new Uri(contextRef, UriKind.Absolute);
            }

            if (remoteContexts.Contains(contextIri))
            {
                // If context is in the remote contexts array, a recursive context inclusion error has been detected and processing is aborted
                throw new JsonLdProcessorException(JsonLdErrorCode.RecursiveContextInclusion,
                    $"Recursive context reference found. Current context IRI is {contextIri}. Visited remote contexts are: {string.Join(", ", remoteContexts)}.");
            }
            // add context to remote contexts
            remoteContexts.Add(contextIri);

            // Dereference context
            var dereferencedContext = LoadReference(contextIri);
            if (dereferencedContext == null)
            {
                // If context cannot be dereferenced, a loading remote context failed error has been detected and processing is aborted.
                throw new JsonLdProcessorException(JsonLdErrorCode.LoadingRemoteContextFailed, $"Failed to load remote context {contextIri}.");
            }
            // If the dereferenced document has no top-level JSON object with an @context member, an invalid remote context has been detected and processing is aborted
            var dereferencedContextObject = dereferencedContext as JObject;
            if (dereferencedContextObject == null)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext, $"The remote resource at {contextIri} does not contain a top-level JSON object");
            }
            var contextProperty = dereferencedContextObject.Property("@context");
            if (contextProperty == null)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidRemoteContext, $"The remote resource at {contextIri} does not contain an @context property");
            }

            // Set result to the result of recursively calling this algorithm, passing result for active context, context for local context, and remote contexts.
            return ProcessContext(result, contextProperty.Value, remoteContexts);
        }

        
    }
}
