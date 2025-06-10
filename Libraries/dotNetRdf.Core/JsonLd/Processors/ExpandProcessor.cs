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

internal class ExpandProcessor : ProcessorBase
{
    private readonly ContextProcessor _contextProcessor;

    public ExpandProcessor(JsonLdProcessorOptions options, 
        ContextProcessor contextProcessor,
        List<JsonLdProcessorWarning> warnings) : base(options, warnings)
    {
        _contextProcessor = contextProcessor;
    }

    /// <summary>
    /// Implementation of the Expansion Algorithm.
    /// </summary>
    /// <param name="activeContext"></param>
    /// <param name="activeProperty"></param>
    /// <param name="element"></param>
    /// <param name="baseUrl">The base URL associated with the document URL of the original document to expand.</param>
    /// <param name="frameExpansion"></param>
    /// <param name="ordered"></param>
    /// <param name="fromMap"></param>
    /// <returns></returns>
    public JToken ExpandElement(JsonLdContext activeContext, string activeProperty, JToken element, Uri baseUrl,
        bool frameExpansion = false, bool ordered = false, bool fromMap = false)
    {
        JToken result;

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
        if (JsonLdUtils.IsScalar(element))
        {
            // 4.1 - If active property is null or @graph, drop the free-floating scalar by returning null.
            if (activeProperty == null || activeProperty == "@graph") return null;

            // 4.2 - If property-scoped context is defined, set active context to the result of the Context Processing algorithm, passing active context, property-scoped context as local context, and base URL from the term definition for active property in active context.
            if (propertyScopedContext != null)
            {
                activeContext = _contextProcessor.ProcessContext(activeContext, propertyScopedContext,
                    activePropertyTermDefinition.BaseUrl);
            }

            // 4.3 - Return the result of the Value Expansion algorithm, passing the active context, active property, and element as value.
            return ExpandValue(activeContext, activeProperty, element);
        }

        // 5 - If element is an array,
        if (element is JArray elementArray)
        {
            // 5.1 - Initialize an empty array, result.
            result = new JArray();
            var resultArray = (JArray)result;

            // 5.2 - For each item in element:
            foreach (JToken item in elementArray)
            {
                // 5.2.1 - Initialize expanded item to the result of using this algorithm recursively, passing active context, active property, and item as element.
                JToken expandedItem = ExpandElement(activeContext, activeProperty, item, baseUrl, frameExpansion, ordered, fromMap);
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
                    if (expandedItem is JArray expandedItemArray)
                    {
                        foreach (JToken arrayItem in expandedItemArray)
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
            JsonLdUtils.GetPropertyValue(activeContext, elementObject, "@value") == null &&
            !(elementObject.Count == 1 && JsonLdUtils.GetPropertyValue(activeContext, elementObject, "@id") != null))
        {
            activeContext = activeContext.PreviousContext;
        }

        // 8 - If property-scoped context is defined, set active context to the result of the Context Processing algorithm, passing active context, property-scoped context as local context, base URL from the term definition for active property, in active context and true for override protected.
        if (propertyScopedContext != null)
        {
            activeContext = _contextProcessor.ProcessContext(activeContext, propertyScopedContext,
                activePropertyTermDefinition.BaseUrl, overrideProtected: true);
        }

        // 9 - If element contains the key @context, set active context to the 
        // result of the Context Processing algorithm, passing active context 
        // and the value of the @context key as local context.
        JToken contextValue = JsonLdUtils.GetPropertyValue(activeContext, elementObject, "@context");
        if (contextValue != null)
        {
            activeContext = _contextProcessor.ProcessContext(activeContext, contextValue, baseUrl);
        }

        // 10 - Initialize type-scoped context to active context. This is used for expanding values that may be relevant to any previous type-scoped context.
        JsonLdContext typeScopedContext = activeContext;

        // 11 - For each key/value pair in element ordered lexicographically by key where key expands to @type using the IRI Expansion algorithm, passing active context, key for value, and true for vocab:
        var typeProperties = elementObject.Properties().Where(property => "@type".Equals(_contextProcessor.ExpandIri(activeContext, property.Name, true))).OrderBy(p => p.Name).ToList();
        foreach (JProperty property in typeProperties)
        {
            // 11.1 Convert value into an array, if necessary.
            JArray values = property.Value.Type == JTokenType.Array ? property.Value as JArray : new JArray(property.Value);
            // 11.2 For each term which is a value of value ordered lexicographically
            foreach (JToken term in values.OrderBy(v => v))
            {
                // if term is a string, and term's term definition in type-scoped context has a local context, set active context to the result Context Processing algorithm, passing active context, the value of the term's local context as local context, base URL from the term definition for value in active context, and false for propagate.
                if (term.Type == JTokenType.String &&
                    typeScopedContext.TryGetTerm(term.Value<string>(), out JsonLdTermDefinition termDefinition) &&
                    termDefinition.LocalContext != null)
                {
                    activeContext = _contextProcessor.ProcessContext(activeContext, termDefinition.LocalContext,
                        termDefinition.BaseUrl, propagate: false);
                }
            }
        }

        // 12 - Initialize two empty maps, result and nests. Initialize input type to expansion of the last value of the first entry in element expanding to @type (if any), ordering entries lexicographically by key. Both the key and value of the matched entry are IRI expanded. 
        // NOTE: nests is only used in steps 13/14 so is initialized in ExpandElement
        result = new JObject();
        var resultObject = result as JObject;
        JProperty firstTypeProperty = elementObject.Properties()
            .OrderBy(p => p.Name).FirstOrDefault(p => "@type".Equals(_contextProcessor.ExpandIri(activeContext, p.Name, true)));
        JToken inputType = null;
        if (firstTypeProperty != null)
        {
            inputType = firstTypeProperty.Value is JArray typeArray
                ? (typeArray.Count > 0 ? typeArray[typeArray.Count - 1] : null)
                : firstTypeProperty.Value;
            if (inputType is JValue) // Don't try to expand a framing wildcard
            {
                inputType = _contextProcessor.ExpandIri(activeContext, inputType.Value<string>(), true);
            }
        }

        // Implements 13 - 14
        ExpandElement(resultObject, inputType, activeContext, activeProperty, baseUrl, frameExpansion, ordered, elementObject, typeScopedContext);

        // 15 - If result contains the entry @value: 
        if (resultObject.ContainsKey("@value"))
        {
            // 15.1 - The result must not contain any entries other than @direction, @index, @language, @type, and @value. It must not contain an @type entry if it contains either @language or @direction entries. Otherwise, an invalid value object error has been detected and processing is aborted.
            if (resultObject.Properties().Any(p => !JsonLdKeywords.ValueObjectKeys.Contains(p.Name)))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                    $"Invalid value object. Expanding the value of {activeProperty} resulted in a value object with additional properties.");
            }
            if ((resultObject.ContainsKey("@language") || resultObject.ContainsKey("@direction")) && resultObject.ContainsKey("@type"))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObject,
                    $"Invalid value object. The expansion of {activeProperty} resulted in a value object with both @type and either @language or @direction.");
            }

            JToken resultType = resultObject.ContainsKey("@type") ? resultObject["@type"] : null;
            JToken resultValue = resultObject["@value"];
            // If the result's @type entry is @json, then the @value entry may contain any value, and is treated as a JSON literal.
            if (resultType != null && resultType.Type == JTokenType.String && resultType.Value<string>().Equals("@json"))
            {
                // No-op
            }
            // Otherwise, if the value of result's @value entry is null, or an empty array, return null.
            else if (resultValue == null || resultValue.Type == JTokenType.Null || resultValue is JArray resultArray && resultArray.Count == 0)
            {
                return null;
            }
            else if (resultObject.ContainsKey("@language") && resultValue.Type != JTokenType.String && !frameExpansion) // KA: in frame expansion @language could be an empty object or array
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedValue,
                    $"Invalid language-tagged value. The expansion of {activeProperty} has an @language entry, but its @value entry is not a JSON string.");
            }
            else if (resultObject.ContainsKey("@type") && !JsonLdUtils.IsAbsoluteIri(resultType) && !frameExpansion)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypedValue,
                    $"Invalid typed value. The expansion of {activeProperty} resulted in a value object with an @type entry whose value is not an IRI.");
            }
        }
        // 16 - Otherwise, if result contains the entry @type and its associated value is not an array, set it to an array containing only the associated value.
        else if (resultObject.ContainsKey("@type"))
        {
            resultObject["@type"] = JsonLdUtils.EnsureArray(resultObject["@type"]);
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
        }
        else if (resultObject.ContainsKey("@list"))
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
                    if (!frameExpansion)
                    {
                        result = null;
                    }
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

    private void ExpandNestedElement(
        JObject resultObject, JToken inputType, JsonLdContext activeContext, string activeProperty, Uri baseUrl,
        bool frameExpansion,
        bool ordered, JObject elementObject, JsonLdContext typeScopedContext
    )
    {
        // 3 - If active property has a term definition in active context with a local context, initialize property-scoped context to that local context.
        JsonLdTermDefinition activePropertyTermDefinition = null;
        var hasTermDefinition = activeProperty != null && activeContext.TryGetTerm(activeProperty,
            out activePropertyTermDefinition);
        JToken propertyScopedContext = null;
        if (hasTermDefinition && activePropertyTermDefinition.LocalContext != null)
        {
            propertyScopedContext = activePropertyTermDefinition.LocalContext;
        }

        // 8 - If property-scoped context is defined, set active context to the result of the Context Processing algorithm, passing active context, property-scoped context as local context, base URL from the term definition for active property, in active context and true for override protected.
        if (propertyScopedContext != null)
        {
            activeContext = _contextProcessor.ProcessContext(activeContext, propertyScopedContext, baseUrl, overrideProtected: true);
        }

        ExpandElement(resultObject, inputType, activeContext, activeProperty, baseUrl, frameExpansion, ordered, elementObject, typeScopedContext);
    }

    private void ExpandElement(JObject resultObject, JToken inputType, JsonLdContext activeContext, string activeProperty, Uri baseUrl, bool frameExpansion,
        bool ordered, JObject elementObject, JsonLdContext typeScopedContext)
    {
        var nests = new JObject();

        // 13 - For each key and value in element, ordered lexicographically by key if ordered is true: 
        IEnumerable<JProperty> elementProperties = elementObject.Properties();
        if (ordered) elementProperties = elementProperties.OrderBy(p => p.Name);
        foreach (JProperty p in elementProperties)
        {
            var key = p.Name;
            JToken value = p.Value;
            // 13.1 - If key is @context, continue to the next key.
            if (key.Equals("@context")) continue;
            // 13.2 - Initialize expanded property to the result of IRI expanding key.
            var expandedProperty = _contextProcessor.ExpandIri(activeContext, key, true);
            // 13.3 - If expanded property is null or it neither contains a colon (:) nor it is a keyword, drop key by continuing to the next key.
            if (expandedProperty == null)
            {
                if (Options.SafeMode)
                {
                    Warn(JsonLdErrorCode.InvalidIriMapping, $"Unable to generate a well-formed absolute IRI for predicate `{key}`. This property will be ignored.");
                }
                continue;
            }
            if (!expandedProperty.Contains(':') && !JsonLdUtils.IsKeyword(expandedProperty))
            {
                if (Options.SafeMode)
                {
                    Warn(JsonLdErrorCode.InvalidIriMapping, $"Unable to generate a well-formed absolute IRI for predicate `{key}`. This property will be ignored.");
                }
                continue;
            }
            JToken expandedValue = null;

            // 13.4 - If expanded property is a keyword: 
            if (JsonLdUtils.IsKeyword(expandedProperty))
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
                            expandedValue = _contextProcessor.ExpandIri(activeContext, value.Value<string>(), false, true);
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
                            foreach (JToken child in value.Children())
                                newArray.Add(_contextProcessor.ExpandIri(activeContext, child.Value<string>(), false, true));
                            expandedValue = newArray;
                            break;

                        case JTokenType.Object when value.Children().Any():
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIdValue,
                                $"Invalid @id value. The property {key} in the value of {activeProperty} expands to the @id keyword, but its map value contains one or more entries.");

                        case JTokenType.Object:
                            expandedValue = frameExpansion ? new JArray(value) : value;
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
                            expandedValue = _contextProcessor.ExpandIri(typeScopedContext, value.Value<string>(), true, true);
                            break;
                        case JTokenType.Array when value.Children().Any(c => c.Type != JTokenType.String):
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidTypeValue,
                                $"Invalid @type value. The property {key} in the value of {activeProperty} expands to the @type keyword, but its array value contains one or more non-string items.");
                        case JTokenType.Array:
                            var expandedItems = new JArray();
                            foreach (JToken item in value.Children())
                            {
                                expandedItems.Add(_contextProcessor.ExpandIri(typeScopedContext, item.Value<string>(), true, true));
                            }

                            expandedValue = expandedItems;
                            break;
                        case JTokenType.Object when frameExpansion && !value.HasValues:
                            expandedValue = value;
                            break;
                        case JTokenType.Object when frameExpansion && (value as JObject).ContainsKey("@default"):
                            var toExpand = (value as JObject)["@default"].Value<string>();
                            expandedValue = new JObject(new JProperty("@default",
                                _contextProcessor.ExpandIri(typeScopedContext, toExpand, true, true)));
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
                        expandedValue = JsonLdUtils.ConcatenateValues(resultObject["@type"], expandedValue);
                    }
                }

                // 13.4.5 - If expanded property is @graph, set expanded value to the result of using this algorithm recursively passing active context, @graph for active property,
                // value for element, base URL, and the frameExpansion and ordered flags, ensuring that expanded value is an array of one or more maps.
                if ("@graph".Equals(expandedProperty))
                {
                    expandedValue = ExpandElement(activeContext, "@graph", value, baseUrl, frameExpansion,
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
                    if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10) continue;
                    // 13.4.6.2 - Set expanded value to the result of using this algorithm recursively passing active context, null for active property, value for element, base URL, and the frameExpansion and ordered flags, ensuring that the result is an array.
                    expandedValue = JsonLdUtils.EnsureArray(ExpandElement(activeContext, null, value, baseUrl, frameExpansion,
                        ordered));
                    // 13.4.6.3 - If any element of expanded value is not a node object, an invalid @included value error has been detected and processing is aborted.
                    if (expandedValue.Children().Any(c => !JsonLdUtils.IsNodeObject(c)))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidIncludedValue,
                            $"Invalid @included value. The expanded value for the {key} property of {activeProperty} contains one or more values that are not valid node objects.");
                    }

                    if (resultObject.ContainsKey("@included"))
                    {
                        expandedValue = JsonLdUtils.ConcatenateValues(resultObject["@included"], expandedValue);
                    }
                }

                // 13.4.7 - If expanded property is @value: 
                if ("@value".Equals(expandedProperty))
                {
                    // 13.4.7.1 - If input type is @json, set expanded value to value.
                    // If processing mode is json-ld-1.0, an invalid value object value error has been detected and processing is aborted.
                    if (inputType != null && inputType.Type == JTokenType.String && inputType.Value<string>().Equals("@json"))
                    {
                        if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                        {
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidValueObjectValue,
                                $"Invalid value object value. The @value entry of {key} in {activeProperty} must be either scalar or null.");
                        }

                        expandedValue = value;
                    }
                    else if ((!frameExpansion && !(JsonLdUtils.IsScalar(value) || JsonLdUtils.IsNull(value))) ||
                             (frameExpansion && !(JsonLdUtils.IsScalar(value) || JsonLdUtils.IsNull(value) || JsonLdUtils.IsEmptyMap(value) ||
                                                  JsonLdUtils.IsArray(value, JsonLdUtils.IsScalar))))
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
                    if (JsonLdUtils.IsNull(expandedValue))
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
                    if (!frameExpansion && !JsonLdUtils.IsString(value) ||
                        frameExpansion && !(JsonLdUtils.IsString(value) || 
                                            JsonLdUtils.IsEmptyMap(value) || 
                                            JsonLdUtils.IsArray(value, JsonLdUtils.IsString)))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString,
                            $"Invalid language-tagged string. The expanded property {key} of {activeProperty} expands to an @language property but the value of the {key} is not a string.");
                    }

                    // 13.4.8.2 - Otherwise, set expanded value to value.
                    // If value is not well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                    // When the frameExpansion flag is set, expanded value will be an array of one or more string values or an array containing an empty map.
                    expandedValue = frameExpansion ? JsonLdUtils.EnsureArray(value) : value;
                    // Processors MAY normalize language tags to lower case.
                    if (JsonLdUtils.IsString(expandedValue))
                    {
                        var languageTag = expandedValue.Value<string>().ToLowerInvariant();
                        if (!LanguageTag.IsWellFormed(languageTag))
                        {
                            Warn(JsonLdErrorCode.MalformedLanguageTag,
                                $"The @language value of {activeProperty} is not a well-formed BCP-47 language tag.");
                        }

                        expandedValue = new JValue(languageTag);
                    }
                }

                // 13.4.9 - If expanded property is @direction: 
                if ("@direction".Equals(expandedProperty))
                {
                    // 13.4.9.1 - If processing mode is json-ld-1.0, continue with the next key from element.
                    if (Options.ProcessingMode == JsonLdProcessingMode.JsonLd10)
                    {
                        continue;
                    }

                    // 13.4.9.2 - If value is neither "ltr" nor "rtl", an invalid base direction error has been detected and processing is aborted.
                    // When the frameExpansion flag is set, value MAY be an empty map or an array of zero or more strings.
                    if (!frameExpansion && !JsonLdUtils.IsValidBaseDirection(value) ||
                        frameExpansion && !(JsonLdUtils.IsValidBaseDirection(value) || JsonLdUtils.IsEmptyMap(value) ||
                                            JsonLdUtils.IsArray(value, JsonLdUtils.IsString)))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                            $"Invalid base direction. The property {key} of {activeProperty} expands to an @direction property but the value of {key} is not a valid base direction string.");
                    }

                    // 13.4.9.3 - Otherwise, set expanded value to value.
                    // When the frameExpansion flag is set, expanded value will be an array of one or more string values or an array containing an empty map.
                    expandedValue = value;
                    if (frameExpansion) expandedValue = JsonLdUtils.EnsureArray(expandedValue);
                    if (frameExpansion && !(JsonLdUtils.IsArray(expandedValue, JsonLdUtils.IsString) || JsonLdUtils.IsArray(expandedValue, JsonLdUtils.IsEmptyMap)))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                            $"Invalid base direction. During frame expansion, the value of the property {key} of {activeProperty} does not expand to an array of strings or an array containing an empty map.");
                    }
                }

                // 13.4.10 - If expanded property is @index:
                if ("@index".Equals(expandedProperty))
                {
                    // 13.4.10.1 - If value is not a string, an invalid @index value error has been detected and processing is aborted.
                    if (!JsonLdUtils.IsString(value))
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
                    expandedValue = JsonLdUtils.EnsureArray(
                        ExpandElement(activeContext, activeProperty, value, baseUrl, frameExpansion, ordered));
                }

                // 13.4.12 - If expanded property is @set, set expanded value to the result of using this algorithm recursively, passing active context, active property,
                // value for element, base URL, and the frameExpansion and ordered flags.
                if ("@set".Equals(expandedProperty))
                {
                    expandedValue = ExpandElement(activeContext, activeProperty, value, baseUrl, frameExpansion,
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
                    expandedValue = ExpandElement(activeContext, "@reverse", value, baseUrl, frameExpansion,
                        ordered);
                    if (expandedValue is JObject expandedValueObject)
                    {
                        // 13.4.13.3 - If expanded value contains an @reverse entry, i.e., properties that are reversed twice, execute for each of its property and item the following steps: 
                        if (expandedValueObject.ContainsKey("@reverse"))
                        {
                            if (expandedValueObject["@reverse"] is JObject reverseObject)
                            {
                                foreach (JProperty property in reverseObject.Properties())
                                {
                                    // 13.4.13.3.1 - Use add value to add item to the property entry in result using true for as array.
                                    JsonLdUtils.AddValue(resultObject, property.Name, property.Value, true);
                                }
                            }
                        }
                        // 13.4.13.4 - If expanded value contains an entry other than @reverse:
                        if (expandedValueObject.Properties().Any(x => !x.Name.Equals("@reverse")))
                        {
                            // 13.4.13.4.1 - Set reverse map to the value of the @reverse entry in result, initializing it to an empty map, if necessary.
                            if (!resultObject.ContainsKey("@reverse")) resultObject["@reverse"] = new JObject();
                            var reverseMap = resultObject["@reverse"] as JObject;
                            // 13.4.13.4.2 - For each property and items in expanded value other than @reverse: 
                            foreach (KeyValuePair<string, JToken> property in expandedValueObject)
                            {
                                if (property.Key.Equals("@reverse")) continue;
                                // 13.4.13.4.2.1 - For each item in items:
                                // KA: Spec is not quite clear here but appears to indicate that the value of all properties should be an array or that array property values should be iterated over
                                IEnumerable<JToken> items = property.Value is JArray itemsArray
                                    ? (IEnumerable<JToken>)itemsArray.Children()
                                    : new[] { property.Value };
                                foreach (JToken item in items)
                                {
                                    // 13.4.13.4.2.1.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                                    if (JsonLdUtils.IsValueObject(item) || JsonLdUtils.IsListObject(item))
                                    {
                                        throw new JsonLdProcessorException(
                                            JsonLdErrorCode.InvalidReversePropertyValue,
                                            $"Invalid reverse property value. Invalid value found in the expanion of {key} in {activeProperty}.");
                                    }

                                    // 13.4.13.4.2.1.2 - Use add value to add item to the property entry in reverse map using true for as array.
                                    JsonLdUtils.AddValue(reverseMap, property.Key, item, true);
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
                if (frameExpansion && JsonLdKeywords.FramingKeywords.Contains(expandedProperty))
                {
                    expandedValue = ExpandElement(activeContext, expandedProperty, value, baseUrl, frameExpansion,
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
            JsonLdTermDefinition termDefinition = activeContext.GetTerm(key);
            ISet<JsonLdContainer> containerMapping = termDefinition?.ContainerMapping ?? new SortedSet<JsonLdContainer>();
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
                LanguageDirection? direction = termDefinition.DirectionMapping ?? activeContext.BaseDirection;

                // 13.7.4 - For each key - value pair language-language value in value, ordered lexicographically by language if ordered is true: 
                IEnumerable<JProperty> properties = (value as JObject).Properties();
                if (ordered) properties = properties.OrderBy(prop => prop.Name);
                foreach (JProperty languageMappingProperty in properties)
                {
                    var language = languageMappingProperty.Name;
                    // 13.4.7.1 - If language value is not an array set language value to an array containing only language value.
                    JArray languageValue = JsonLdUtils.EnsureArray(languageMappingProperty.Value);
                    // 13.4.7.2 - For each item in language value: 
                    foreach (JToken item in languageValue)
                    {
                        // 13.4.7.2.1 - If item is null, continue to the next entry in language value.
                        if (JsonLdUtils.IsNull(item)) continue;
                        if (!JsonLdUtils.IsString(item))
                        {
                            // 13.4.7.2.2 - item must be a string, otherwise an invalid language map value error has been detected and processing is aborted.
                            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageMapValue,
                                $"Invalid language map value. An invalid language map was found for language {language} in the language map of the property {key} in {activeProperty}");
                        }

                        // 13.4.7.2.3 - Initialize a new map v consisting of two key-value pairs: (@value-item) and (@language-language).
                        // If item is neither @none nor well-formed according to section 2.2.9 of [BCP47], processors SHOULD issue a warning.
                        language = language.ToLowerInvariant();
                        if (!("@none".Equals(language) || LanguageTag.IsWellFormed(language)))
                        {
                            Warn(JsonLdErrorCode.MalformedLanguageTag,
                                $"The property {languageMappingProperty} in {activeProperty} must be either a " +
                                $"well-formed BCP-47 language tag or '@none'.");
                        }
                        var v = new JObject(
                            new JProperty("@value", item),
                            new JProperty("@language", language.ToLowerInvariant())); // Processors MAY normalize language tags to lower case.

                        // 13.4.7.2.4 - If language is @none, or expands to @none, remove @language from v.
                        if (language.Equals("@none") || activeContext.GetAliases("@none").Contains(language))
                        {
                            v.Remove("@language");
                        }

                        // 13.4.7.2.5 -  If direction is not null, add an entry for @direction to v with direction.
                        if (direction.HasValue && direction != LanguageDirection.Unspecified)
                        {
                            v["@direction"] = JsonLdUtils.SerializeLanguageDirection(direction.Value);
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
                IEnumerable<JProperty> properties = (value as JObject).Properties();
                if (ordered) properties = properties.OrderBy(prop => prop.Name);
                foreach (JProperty indexValueProperty in properties)
                {
                    var index = indexValueProperty.Name;
                    JArray indexValue = JsonLdUtils.EnsureArray(indexValueProperty.Value);

                    JsonLdContext mapContext = null;
                    // 13.8.3.1 - If container mapping includes @id or @type, initialize map context to the previous context from active context if it exists, otherwise, set map context to active context.
                    if (containerMapping.Contains(JsonLdContainer.Id) ||
                        containerMapping.Contains(JsonLdContainer.Type))
                    {
                        mapContext = activeContext.PreviousContext ?? activeContext;
                        // 13.8.3.2 - If container mapping includes @type and index's term definition in map context has a local context, update map context to the result of the Context Processing algorithm, passing map context as active context the value of the index's local context as local context and base URL from the term definition for index in map context.
                        if (containerMapping.Contains(JsonLdContainer.Type))
                        {
                            JsonLdTermDefinition indexTermDefinition = mapContext.GetTerm(index);
                            if (indexTermDefinition?.LocalContext != null)
                            {
                                mapContext = _contextProcessor.ProcessContext(mapContext, indexTermDefinition.LocalContext,
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
                    var expandedIndex = _contextProcessor.ExpandIri(activeContext, index, true);

                    // 13.8.3.5 - If index value is not an array set index value to an array containing only index value.
                    // Already done at initialization

                    // 13.8.3.6 - Initialize index value to the result of using this algorithm recursively, passing map context as active context, key as active property, index value as element, base URL, true for from map, and the frameExpansion and ordered flags.
                    indexValue = JsonLdUtils.EnsureArray(ExpandElement(mapContext, key, indexValue, baseUrl, frameExpansion,
                        ordered, true));

                    // 13.8.3.7 - For each item in index value: 
                    for (var ix = 0; ix < indexValue.Count; ix++)
                    {
                        var item = indexValue[ix] as JObject;
                        // 13.8.7.3.1 - If container mapping includes @graph, and item is not a graph object, set item to a new map containing the key - value pair @graph-item, ensuring that the value is represented using an array.
                        if (containerMapping.Contains(JsonLdContainer.Graph) && !JsonLdUtils.IsGraphObject(item))
                        {
                            item = new JObject(new JProperty("@graph", JsonLdUtils.EnsureArray(item)));
                        }

                        // 13.8.3.7.2 - If container mapping includes @index, index key is not @index, and expanded index is not @none: 
                        if (containerMapping.Contains(JsonLdContainer.Index) && !indexKey.Equals("@index") &&
                            !expandedIndex.Equals("@none"))
                        {
                            // 13.8.3.7.2.1 - Initialize re-expanded index to the result of calling the Value Expansion algorithm, passing the active context, index key as active property, and index as value.
                            JToken reExpandedIndex = ExpandValue(activeContext, indexKey, index);

                            // 13.8.3.7.2.2 - Initialize expanded index key to the result of IRI expanding index key.
                            var expandedIndexKey = _contextProcessor.ExpandIri(activeContext, indexKey, true);

                            // 13.8.3.7.2.3 - Initialize index property values to an array consisting of re-expanded index followed by the existing values of the concatenation of expanded index key in item, if any.
                            var indexPropertyValues = new JArray(reExpandedIndex);
                            if (item.ContainsKey(expandedIndexKey))
                            {
                                indexPropertyValues =
                                    JsonLdUtils.ConcatenateValues(indexPropertyValues, item[expandedIndexKey]);
                            }

                            // 13.8.3.7.2.4 - Add the key - value pair(expanded index key - index property values) to item.
                            item.Remove(expandedIndexKey); // Overwriting any existing value (which should have been appended to indexPropertyValues
                            item.Add(new JProperty(expandedIndexKey, indexPropertyValues));

                            // 13.8.3.7.2.5 - If item is a value object, it MUST NOT contain any extra properties; an invalid value object error has been detected and processing is aborted.
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
                            expandedIndex = _contextProcessor.ExpandIri(activeContext, index, false, true);
                            item.Add("@id", expandedIndex);
                        }
                        // 13.8.7.5 - Otherwise, if container mapping includes @type and expanded index is not @none, initialize types to a new array consisting of expanded index followed by any existing values of @type in item.Add the key - value pair(@type - types) to item.
                        else if (containerMapping.Contains(JsonLdContainer.Type) && !expandedIndex.Equals("@none"))
                        {
                            var types = new JArray(expandedIndex);
                            if (item.ContainsKey("@type"))
                            {
                                types = JsonLdUtils.ConcatenateValues(types, item["@type"]);
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
                expandedValue = ExpandElement(activeContext, key, value, baseUrl, frameExpansion, ordered);
            }

            // 13.10 - If expanded value is null, ignore key by continuing to the next key from element.
            if (expandedValue == null) continue;
            // 13.11 If container mapping includes @list and expanded value is not already a list object, convert expanded value to a list object by first setting it to an array containing only expanded value if it is not already an array, and then by setting it to a map containing the key-value pair @list-expanded value.
            if (containerMapping.Contains(JsonLdContainer.List) && !JsonLdUtils.IsListObject(expandedValue))
            {
                expandedValue = new JObject(new JProperty("@list", JsonLdUtils.EnsureArray(expandedValue)));
            }

            // 13.12 - If container mapping includes @graph, and includes neither @id nor @index, convert expanded value into an array, if necessary,
            // then convert each value ev in expanded value into a graph object:
            if (containerMapping.Contains(JsonLdContainer.Graph) &&
                !containerMapping.Contains(JsonLdContainer.Id) && !containerMapping.Contains(JsonLdContainer.Index))
            {
                JArray tmp = JsonLdUtils.EnsureArray(expandedValue);
                var expandedValueArray = new JArray();
                foreach (JToken ev in tmp)
                {
                    // 13.12.1 - Convert ev into a graph object by creating a map containing the key-value pair @graph-ev where ev is represented as an array.
                    // Note: This may lead to a graph object including another graph object, if ev was already in the form of a graph object.
                    expandedValueArray.Add(new JObject(new JProperty("@graph", JsonLdUtils.EnsureArray(ev))));
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
                expandedValue = JsonLdUtils.EnsureArray(expandedValue);
                // 13.13.4 - For each item in expanded value
                foreach (JToken item in expandedValue.Children())
                {
                    // 13.13.4.1 - If item is a value object or list object, an invalid reverse property value has been detected and processing is aborted.
                    if (JsonLdUtils.IsValueObject(item) || JsonLdUtils.IsListObject(item))
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
                    JsonLdUtils.AddValue(reverseMap, expandedProperty, item, true);
                }
            }
            else
            {
                JsonLdUtils.AddValue(resultObject, expandedProperty, expandedValue, true);
            }
        }

        // 14 - For each key nesting-key in nests, ordered lexicographically if ordered is true: 
        IEnumerable<JProperty> nestProperties = nests.Properties();
        if (ordered) nestProperties = nestProperties.OrderBy(p => p.Name);
        foreach (JProperty nestingProperty in nestProperties)
        {
            var nestingKey = nestingProperty.Name;
            // 14.1 - Initialize nested values to the value of nesting-key in element, ensuring that it is an array.
            JArray nestedValues = JsonLdUtils.EnsureArray(elementObject[nestingKey]);
            var expandedNestedValues = new JArray();
            // 14.2 - For each nested value in nested values: 
            foreach (JToken nestedValue in nestedValues)
            {
                // 14.2.1 - If nested value is not a map, or any key within nested value expands to @value, an invalid @nest value error has been detected and processing is aborted.
                // 14.2.2 - Recursively repeat steps 3, 8, 13 and 14 using nesting-key for active property, and nested value for element.
                if (nestedValue is JObject nestedValueObject)
                {
                    if (nestedValueObject.Properties()
                        .Any(p => _contextProcessor.ExpandIri(activeContext, p.Name, true).Equals("@value")))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                            $"Invalid nest value. Invalid value found when expanding the nesting property {nestingKey} of {activeProperty}. The nested value contains a property which is, or which expands to @value.");
                    }

                    ExpandNestedElement(resultObject, inputType, activeContext, nestingKey, baseUrl, frameExpansion, ordered, nestedValueObject, typeScopedContext);
                }
                else
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                        $"Invalid nest value. Invalid value found when expanding the nesting property {nestingKey} of {activeProperty}. The nested value is not a JSON object.");
                }
            }
            nestingProperty.Value = expandedNestedValues;
        }
    }


    /// <summary>
    /// Implementation of the Value Expansion algorithm.
    /// </summary>
    /// <param name="activeContext"></param>
    /// <param name="activeProperty"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private JToken ExpandValue(JsonLdContext activeContext, string activeProperty, JToken value)
    {
        activeContext.TryGetTerm(activeProperty, out JsonLdTermDefinition activePropertyTermDefinition, true);
        var typeMapping = activePropertyTermDefinition?.TypeMapping;

        // 1 - If the active property has a type mapping in active context that is @id, and the value is a string, return a new map containing a single entry
        // where the key is @id and the value is the result IRI expanding value using true for document relative and false for vocab.
        if (typeMapping != null && typeMapping == "@id" && value.Type == JTokenType.String)
        {
            return new JObject(new JProperty("@id", _contextProcessor.ExpandIri(activeContext, value.Value<string>(), documentRelative: true, vocab: false)));
        }

        // 2 - If active property has a type mapping in active context that is @vocab, and the value is a string, return a new map containing a single entry where the key is @id and the value is the result of IRI expanding value using true for document relative.
        if (typeMapping != null && typeMapping == "@vocab" && value.Type == JTokenType.String)
        {
            return new JObject(new JProperty("@id", _contextProcessor.ExpandIri(activeContext, value.Value<string>(), vocab: true, documentRelative: true)));
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
            LanguageDirection direction = activePropertyTermDefinition?.DirectionMapping ?? (activeContext.BaseDirection ?? LanguageDirection.Unspecified);
            // 5.3 - If language is not null, add @language to result with the value language.
            if (language != null)
            {
                result.Add(new JProperty("@language", language));
            }
            // 5.4 - If direction is not null, add @direction to result with the value direction.
            if (direction != LanguageDirection.Unspecified)
            {
                result.Add(new JProperty("@direction", JsonLdUtils.SerializeLanguageDirection(direction)));
            }
        }
        // 6 - Return result.
        return result;
    }

}
