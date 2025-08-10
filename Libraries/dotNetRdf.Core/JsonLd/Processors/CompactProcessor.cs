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

namespace VDS.RDF.JsonLd.Processors;

internal class CompactProcessor : ProcessorBase
{
    private readonly ContextProcessor _contextProcessor;

    public CompactProcessor(JsonLdProcessorOptions options,
        ContextProcessor contextProcessor,
        IList<JsonLdProcessorWarning> warnings) : base(options, warnings)
    {
        _contextProcessor = contextProcessor;
    }

    /// <summary>
    /// Implementation of the JSON-LD Compact Algorithm.
    /// </summary>
    /// <param name="activeContext"></param>
    /// <param name="activeProperty"></param>
    /// <param name="element"></param>
    /// <param name="compactArrays"></param>
    /// <param name="ordered"></param>
    /// <returns></returns>
    public JToken CompactElement(JsonLdContext activeContext, string activeProperty, JToken element, bool compactArrays = false, bool ordered = false)
    {
        JsonLdTermDefinition activeTermDefinition = null;
        if (activeProperty != null)
        {
            activeContext.TryGetTerm(activeProperty, out activeTermDefinition, true);
        }

        // 1 - Initialize type-scoped context to active context. This is used for compacting values that may be relevant to any previous type - scoped context.
        JsonLdContext typeScopedContext = activeContext;

        // 2 - If element is a scalar, it is already in its most compact form, so simply return element.
        if (JsonLdUtils.IsScalar(element)) return element;

        // 3 - If element is an array: 
        if (element is JArray elementArray)
        {
            // 3.1 - Initialize result to an empty array.
            var arrayResult = new JArray();

            // 3.2 - For each item in element: 
            foreach (JToken item in elementArray)
            {
                // 3.2.1 - Initialize compacted item to the result of using this algorithm recursively, passing active context, active property, item for element, and the compactArrays and ordered flags.
                JToken compactedItem = CompactElement(activeContext, activeProperty, item, compactArrays, ordered);
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
            activeContext = _contextProcessor.ProcessContext(activeContext, activeTermDefinition.LocalContext,
                activeTermDefinition.BaseUrl, overrideProtected: true);
            activeTermDefinition = activeContext.GetTerm(activeProperty);
        }

        // 7 - If element has an @value or @id entry and the result of using the Value Compaction algorithm, passing active context, active property,
        // and element as value is a scalar, or the term definition for active property has a type mapping of @json, return that result.
        if (elementObject.ContainsKey("@value") || elementObject.ContainsKey("@id"))
        {
            JToken compactValue = CompactValue(activeContext, activeProperty, elementObject);
            if (JsonLdUtils.IsScalar(compactValue) ||
                (activeTermDefinition != null && "@json".Equals(activeTermDefinition.TypeMapping)))
            {
                return compactValue;
            }
        }
        // 8 - If element is a list object, and the container mapping for active property in active context includes @list, return the result of using this
        // algorithm recursively, passing active context, active property, value of @list in element for element, and the compactArrays and ordered flags.
        if (JsonLdUtils.IsListObject(elementObject) && activeTermDefinition != null && activeTermDefinition.ContainerMapping.Contains(JsonLdContainer.List))
        {
            return CompactElement(activeContext, activeProperty, elementObject["@list"], compactArrays, ordered);
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
            JArray expandedTypes = JsonLdUtils.EnsureArray(elementObject["@type"]);
            foreach (JToken expandedType in expandedTypes)
            {
                var compactedType = CompactIri(activeContext, expandedType.Value<string>(), vocab: true);
                compactedTypes.Add(compactedType);
            }
            // Then, for each term in compacted types ordered lexicographically: 
            foreach (var term in compactedTypes.Select(t => t.Value<string>()).OrderBy(x => x))
            {
                // 11.1 - If the term definition for term in type-scoped context has a local context set active context to the result of the
                // Context Processing algorithm, passing active context and the value of term's local context in type-scoped context as
                // local context base URL from the term definition for term in type-scoped context, and false for propagate. 
                if (typeScopedContext.TryGetTerm(term, out JsonLdTermDefinition termDef) && termDef.LocalContext != null)
                {
                    activeContext = _contextProcessor.ProcessContext(activeContext, termDef.LocalContext, termDef.BaseUrl,
                        propagate: false);
                }
            }
        }
        // 12 - For each key expanded property and value expanded value in element, ordered lexicographically by expanded property if ordered is true: 
        IEnumerable<JProperty> properties = elementObject.Properties();
        if (ordered) properties = properties.OrderBy(p => p.Name);
        foreach (JProperty p in properties)
        {
            var expandedProperty = p.Name;
            JToken expandedValue = p.Value;
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
                    var compactedValueArray = new JArray();
                    compactedValue = compactedValueArray;
                    // 12.2.2.2 - For each item expanded type in expanded value:
                    foreach (JToken item in expandedValue.Children())
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
                JsonLdTermDefinition aliasTermDefinition = activeContext.GetTerm(alias);
                var asArray =
                    Options.ProcessingMode == JsonLdProcessingMode.JsonLd11 &&
                    aliasTermDefinition != null &&
                    aliasTermDefinition.ContainerMapping.Contains(JsonLdContainer.Set) || !compactArrays;
                // 12.2.5 - Use add value to add compacted value to the alias entry in result using as array.
                JsonLdUtils.AddValue(result, alias, compactedValue, asArray);

                // 12.2.6 - Continue to the next expanded property.
                continue;
            }
            // 12.3 - If expanded property is @reverse:
            if ("@reverse".Equals(expandedProperty))
            {
                // 12.3.1 - Initialize compacted value to the result of using this algorithm recursively, passing active context, @reverse for active property, expanded value for element, and the compactArrays and ordered flags.
                JToken compactedValue =
                    CompactElement(activeContext, "@reverse", expandedValue, compactArrays, ordered);
                if (compactedValue is JObject compactedObject)
                {
                    // 12.3.2 - For each property and value in compacted value:
                    foreach (JProperty compactedObjectProperty in compactedObject.Properties().ToList())
                    {
                        // 12.3.1 - If the term definition for property in the active context indicates that property is a reverse property
                        JsonLdTermDefinition td = activeContext.GetTerm(compactedObjectProperty.Name, true);
                        if (td != null && td.Reverse)
                        {
                            // 12.3.2.1.1 - Initialize as array to true if the container mapping for property in the active context includes @set, otherwise the negation of compactArrays.
                            var asArray = td.ContainerMapping.Contains(JsonLdContainer.Set) || !compactArrays;
                            // 12.3.2.1.2 - Use add value to add value to the property entry in result using as array.
                            JsonLdUtils.AddValue(result, compactedObjectProperty.Name, compactedObjectProperty.Value, asArray);
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
                if (!JsonLdUtils.IsEmptyArray(expandedValue))
                {
                    // 12.4.1 - Initialize compacted value to the result of using this algorithm recursively, passing
                    // active context, active property, expanded value for element, and the compactArrays and ordered flags.
                    JToken compactedValue = CompactElement(activeContext, activeProperty, expandedValue,
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
            if (JsonLdUtils.IsEmptyArray(expandedValue))
            {
                // 12.7.1 - Initialize item active property by IRI compacting expanded property using expanded value for value and inside reverse for reverse.
                var itemActiveProperty =
                    CompactIri(activeContext, expandedProperty, expandedValue, true, insideReverse);
                // 12.7.2 - If the term definition for item active property in the active context has a nest value entry(nest term): 
                JsonLdTermDefinition td = activeContext.GetTerm(itemActiveProperty);
                JObject nestResult = null;
                if (td != null && td.Nest != null)
                {

                    // 12.7.2.1 - If nest term is not @nest, or a term in the active context that expands to @nest, an invalid @nest value error has been detected, and processing is aborted.
                    var nestTerm = _contextProcessor.ExpandIri(activeContext, td.Nest, true);
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
                JsonLdUtils.AddValue(nestResult, itemActiveProperty, new JArray(), true);
            }
            // 12.8 -  At this point, expanded value must be an array due to the Expansion algorithm. For each item expanded item in expanded value: 
            foreach (JToken expandedItem in expandedValue.Children())
            {
                // 12.8.1 - Initialize item active property by IRI compacting expanded property using expanded item for value and inside reverse for reverse.
                var itemActiveProperty =
                    CompactIri(activeContext, expandedProperty, expandedItem, true, insideReverse);
                // 12.8.2 - If the term definition for item active property in the active context has a nest value entry (nest term):
                JsonLdTermDefinition itemActiveTermDefinition = activeContext.GetTerm(itemActiveProperty);
                JObject nestResult = null;
                if (itemActiveTermDefinition != null && itemActiveTermDefinition.Nest != null)
                {
                    // 12.8.2.1 - If nest term is not @nest, or a term in the active context that expands to @nest, an invalid @nest value error has been detected, and processing is aborted.
                    var nestTerm = itemActiveTermDefinition.Nest;
                    if (!"@nest".Equals(_contextProcessor.ExpandIri(activeContext, nestTerm, true)))
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidNestValue,
                            $"Invalid Nest Value. Error compacting property {expandedProperty} of {activeProperty}. The value of {expandedProperty} should be '@nest' or a term that expands to '@nest'.");
                    }
                    // 12.8.2.2 - If result does not have a nest term entry, initialize it to an empty map.
                    // 12.8.2.3 - Initialize nest result to the value of nest term in result.
                    nestResult = EnsureMapEntry(result, nestTerm);
                }
                // 12.8.3 - Otherwise, initialize nest result to result.
                else
                {
                    nestResult = result;
                }

                // 12.8.4 - Initialize container to container mapping for item active property in active context, or to a new empty array, if there is no such container mapping.
                ISet<JsonLdContainer> container = (itemActiveTermDefinition != null && itemActiveTermDefinition.ContainerMapping != null)
                    ? itemActiveTermDefinition.ContainerMapping
                    : new HashSet<JsonLdContainer>();
                // 12.8.5 - Initialize as array to true if container includes @set, or if item active property is @graph or @list, otherwise the negation of compactArrays.
                var asArray = container.Contains(JsonLdContainer.Set) ||
                              "@graph".Equals(itemActiveProperty) ||
                               "@list".Equals(itemActiveProperty) ||
                              !compactArrays;
                // 12.8.6 - Initialize compacted item to the result of using this algorithm recursively, passing active context,
                // item active property for active property, expanded item for element, along with the compactArrays and ordered flags.
                // If expanded item is a list object or a graph object, use the value of the @list or @graph entries, respectively, for element instead of expanded item.
                JToken elementToCompact = JsonLdUtils.IsListObject(expandedItem) ? expandedItem["@list"] :
                    JsonLdUtils.IsGraphObject(expandedItem) ? expandedItem["@graph"] : expandedItem;
                JToken compactedItem = CompactElement(activeContext, itemActiveProperty, elementToCompact,
                    compactArrays, ordered);
                // 12.8.7 - If expanded item is a list object: 
                if (JsonLdUtils.IsListObject(expandedItem))
                {
                    // 12.8.7.1 - If compacted item is not an array, then set compacted item to an array containing only compacted item.
                    compactedItem = JsonLdUtils.EnsureArray(compactedItem);
                    // 12.8.7.2 - If container does not include @list: 
                    if (!container.Contains(JsonLdContainer.List))
                    {
                        // 12.8.7.2.1 - Convert compacted item to a list object by setting it to a map containing an entry where the key is
                        // the result of IRI compacting @list and the value is the original compacted item.
                        compactedItem = new JObject(new JProperty(CompactIri(activeContext, "@list", vocab: true), compactedItem));

                        // 12.8.7.2.2 - If expanded item contains the entry @index - value, then add an entry to compacted item where the key is the result of IRI compacting @index and value is value.
                        if (expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@index"))
                        {
                            (compactedItem as JObject).Add(new JProperty(CompactIri(activeContext, "@index", vocab: true), expandedItemObject["@index"]));
                        }
                        // 12.8.7.2.3 - Use add value to add compacted item to the item active property entry in nest result using as array.
                        JsonLdUtils.AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                    }
                    // 12.8.7.3 - Otherwise, set the value of the item active property entry in nest result to compacted item.
                    else
                    {
                        nestResult[itemActiveProperty] = compactedItem;
                    }
                }
                // 12.8.8 - If expanded item is a graph object: 
                // KA: Although algorithm doesn't say "Otherwise", I think this is an else-if branch as nestResult gets updated in the preceding if branch.
                else if (JsonLdUtils.IsGraphObject(expandedItem))
                {
                    // 12.8.8.1 - If container includes @graph and @id: 
                    if (container.Contains(JsonLdContainer.Graph) && container.Contains(JsonLdContainer.Id))
                    {
                        // 12.8.8.1.1 - Initialize map object to the value of item active property in nest result, initializing it to a new empty map, if necessary.
                        JObject mapObject = EnsureMapEntry(nestResult, itemActiveProperty);

                        // 12.8.8.1.2 - Initialize map key by IRI compacting the value of @id in expanded item or @none if no such value exists with vocab set to false if there is an @id entry in expanded item.
                        var mapKey =
                            (expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@id"))
                                ? CompactIri(activeContext, expandedItemObject["@id"].Value<string>(),
                                    vocab: false)
                                : CompactIri(activeContext, "@none", vocab: true);
                        // 12.8.8.1.3 - Use add value to add compacted item to the map key entry in map object using as array.
                        JsonLdUtils.AddValue(mapObject, mapKey, compactedItem, asArray);
                    }
                    // 12.8.8.2 - Otherwise, if container includes @graph and @index and expanded item is a simple graph object: 
                    else if (JsonLdUtils.IsSimpleGraphObject(expandedItem) && container.Contains(JsonLdContainer.Graph) &&
                             container.Contains(JsonLdContainer.Index))
                    {
                        // 12.8.8.2.1 - Initialize map object to the value of item active property in nest result, initializing it to a new empty map, if necessary.
                        JObject mapObject = EnsureMapEntry(nestResult, itemActiveProperty);
                        // 12.8.8.2.2 - Initialize map key the value of @index in expanded item or @none, if no such value exists.
                        var mapKey =
                            expandedItem is JObject expandedItemObject && expandedItemObject.ContainsKey("@index")
                                ? expandedItemObject["@index"].Value<string>()
                                : "@none";
                        // 12.8.8.2.3 - Use add value to add compacted item to the map key entry in map object using as array.
                        JsonLdUtils.AddValue(mapObject, mapKey, compactedItem, asArray);
                    }
                    // 12.8.8.3 - Otherwise, if container includes @graph and expanded item is a simple graph object the value cannot be represented as a map object. 
                    else if (JsonLdUtils.IsSimpleGraphObject(expandedItem) && container.Contains(JsonLdContainer.Graph))
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
                        JsonLdUtils.AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                    }
                    // 12.8.8.4 - Otherwise, container does not include @graph or otherwise does not match one of the previous cases.
                    else
                    {
                        // 12.8.8.4.1 - Set compacted item to a new map containing the key from IRI compacting @graph using the original compacted item as a value.
                        compactedItem = new JObject(new JProperty(CompactIri(activeContext, "@graph", vocab: true), compactedItem));
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
                        JsonLdUtils.AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
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
                    if (container.Contains(JsonLdContainer.Language)) expandedKey = "@language";
                    if (container.Contains(JsonLdContainer.Index)) expandedKey = "@index";
                    if (container.Contains(JsonLdContainer.Id)) expandedKey = "@id";
                    if (container.Contains(JsonLdContainer.Type)) expandedKey = "@type";
                    var containerKey = CompactIri(activeContext, expandedKey, vocab: true);

                    // 12.8.9.3 - Initialize index key to the value of index mapping in the term definition associated with item active property in active context, or @index, if no such value exists.
                    var indexKey = itemActiveTermDefinition != null && itemActiveTermDefinition.IndexMapping != null
                        ? itemActiveTermDefinition.IndexMapping
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
                    else if (container.Contains(JsonLdContainer.Index) && compactedItem is JObject)
                    {
                        // 12.8.9.6.1 - Reinitialize container key by IRI compacting index key after first IRI expanding it.
                        var expandedIndexKey = _contextProcessor.ExpandIri(activeContext, indexKey);
                        containerKey = CompactIri(activeContext, expandedIndexKey, vocab: true);
                        // 12.8.9.6.2 - Set map key to the first value of container key in compacted item, if any.
                        // 12.8.9.6.3 - If there are remaining values in compacted item for container key, use add value to add those remaining values to the container key in compacted item.
                        // Otherwise, remove that entry from compacted item.
                        JArray array = JsonLdUtils.EnsureArray(compactedItem[containerKey]);

                        (compactedItem as JObject).Remove(containerKey);
                        foreach (JToken item in array)
                        {
                            if (mapKey == null && item.Type == JTokenType.String)
                            {
                                mapKey = item.Value<string>();
                            }
                            else
                            {
                                JsonLdUtils.AddValue(compactedItem as JObject, containerKey, item);
                            }
                        }
                    }
                    // 12.8.9.7 - Otherwise, if container includes @id, set map key to the value of container key in compacted item and remove container key from compacted item.
                    else if (container.Contains(JsonLdContainer.Id))
                    {
                        if (compactedItem is JObject compactedItemObject &&
                            compactedItemObject.ContainsKey(containerKey))
                        {
                            mapKey = compactedItemObject[containerKey].Value<string>();
                            compactedItemObject.Remove(containerKey);
                        }
                    }
                    // 12.8.9.8 - Otherwise, if container includes @type: 
                    else if (container.Contains(JsonLdContainer.Type))
                    {
                        // 12.8.9.8.1 - Set map key to the first value of container key in compacted item, if any.
                        // 12.8.9.8.2 - If there are remaining values in compacted item for container key, use add value to add those remaining values to the container key in compacted item.
                        // 12.8.9.8.3 - Otherwise, remove that entry from compacted item.
                        JArray array = compactedItem.HasValues
                            ? JsonLdUtils.EnsureArray(compactedItem[containerKey])
                            : [];

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
                            if (_contextProcessor.ExpandIri(activeContext, compactedItemObject.Properties().First().Name, vocab: true)
                                .Equals("@id"))
                            {
                                compactedItem = CompactElement(activeContext, itemActiveProperty,
                                    new JObject(new JProperty("@id", expandedItemObject["@id"])));
                            }
                        }
                    }
                    // 12.8.9.9 - If map key is null, set it to the result of IRI compacting @none.
                    if (mapKey == null) mapKey = CompactIri(activeContext, "@none", vocab: true);
                    // 12.8.9.10 - Use add value to add compacted item to the map key entry in map object using as array.
                    JsonLdUtils.AddValue(mapObject, mapKey, compactedItem, asArray);
                }
                // 12.8.10 - Otherwise, use add value to add compacted item to the item active property entry in nest result using as array.
                else
                {
                    JsonLdUtils.AddValue(nestResult, itemActiveProperty, compactedItem, asArray);
                }
            }
        }

        return result;
    }

    public string CompactIri(JsonLdContext activeContext, string iri, JToken value = null,
bool vocab = false, bool reverse = false)
    {
        // 1 - If var is null, return null.
        // KA - note local name for var is iri to avoid clash with C# keyword
        if (iri == null) return null;

        // 2 - If the active context has a null inverse context, set inverse context in active context to the result of calling the Inverse Context Creation algorithm using active context.
        // Initialize inverse context to the value of inverse context in active context.
        JObject inverseContext = activeContext.InverseContext;

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
                    (JsonLdUtils.SerializeLanguageDirection(activeContext.BaseDirection.Value) + "_" + activeContext.Language)
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
            if (value is JObject && (value as JObject).ContainsKey("@index") && !JsonLdUtils.IsGraphObject(value))
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
            else if (JsonLdUtils.IsListObject(value))
            {
                // 4.7 - Otherwise, if value is a list object, then set type/language and type/language value to the most specific values that work for all items in the list as follows: 
                var valueObject = value as JObject;
                // 4.7.1 - If @index is not an entry in value, then append @list to containers.
                if (!valueObject.ContainsKey("@index")) containers.Add("@list");
                // 4.7.2 - Initialize list to the array associated with the @list entry in value.
                JArray list = JsonLdUtils.EnsureArray(valueObject["@list"]);
                // 4.7.3 - Initialize common type and common language to null.If list is empty, set common language to default language.
                string commonType = null;
                string commonLanguage = null;
                if (list.Count == 0)
                {
                    commonLanguage = defaultLanguage;
                }

                // 4.7.4 - For each item in list:
                foreach (JToken item in list)
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
            else if (JsonLdUtils.IsGraphObject(value))
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
                if (JsonLdUtils.IsValueObject(value))
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
            if (Options.ProcessingMode != JsonLdProcessingMode.JsonLd10)
            {
                // 4.11 - If processing mode is not json-ld-1.0 and value is not a map or does not contain an @index entry, append @index and @index@set to containers.
                if (value == null || value.Type != JTokenType.Object || !(value as JObject).ContainsKey("@index"))
                {
                    containers.Add("@index");
                    containers.Add("@index@set");
                }

                // 4.12 - If processing mode is not json - ld - 1.0 and value is a map containing only an @value entry, append @language and @language@set to containers.
                if (value is JObject valueObject &&
                    valueObject.Count == 1 &&
                    valueObject.ContainsKey("@value"))
                {
                    containers.Add("@language");
                    containers.Add("@language@set");
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
                if (activeContext.TryGetTerm(compactedId, out JsonLdTermDefinition td2) && td2.IriMapping.Equals(idValue))
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
                if (JsonLdUtils.IsListObject(value) && (value["@list"] as JArray).Count == 0)
                {
                    typeLanguage = "@any";
                }
            }

            // 4.18 - Append @any to preferred values.
            preferredValues.Add("@any");
            
            // 4.19 - If preferred values contains any entry having an underscore ("_"), append the substring of that entry from the underscore to the end of the string to preferred values.
            var toAppend = preferredValues
                .Where(pv => pv.Contains("_"))
                .Select(pv => pv.Substring(pv.IndexOf("_", StringComparison.Ordinal)))
                .ToList();
            preferredValues.AddRange(toAppend);

            // 4.20 - Initialize term to the result of the Term Selection algorithm, passing var, containers, type/language, and preferred values.
            var term = activeContext.SelectTerm(iri, containers, typeLanguage, preferredValues);
            if (term != null)
            {
                return term;
            }
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
            JsonLdTermDefinition termDefinition = activeContext.GetTerm(definitionKey);
            // 7.1 - If the IRI mapping of definition is null, its IRI mapping equals var, its IRI mapping is not a substring at the beginning of var, or definition does not have a true prefix flag, definition's key cannot be used as a prefix. Continue with the next definition.
            if (termDefinition.IriMapping == null ||
                termDefinition.IriMapping.Equals(iri) ||
                !iri.StartsWith(termDefinition.IriMapping) ||
                !termDefinition.Prefix) continue;
            // 7.2 - Initialize candidate by concatenating definition key, a colon(:), and the substring of var that follows after the value of the definition's IRI mapping.
            var candidate = definitionKey + ":" + iri.Substring(termDefinition.IriMapping.Length);
            // 7.1 - If either compact IRI is null, candidate is shorter or the same length but lexicographically less than compact IRI and candidate does not have a term definition in active context, or if that term definition has an IRI mapping that equals var and value is null, set compact IRI to candidate.
            if (!activeContext.HasTerm(candidate) && (compactIri == null || candidate.Length < compactIri.Length ||
                                                      string.Compare(candidate, compactIri, StringComparison.Ordinal) < 0))
            {
                compactIri = candidate;
            }
            else if (activeContext.HasTerm(candidate))
            {
                JsonLdTermDefinition candidateTermDef = activeContext.GetTerm(candidate);
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
        if (ix > 0 && iri.IndexOf("://", StringComparison.Ordinal) != ix)
        {
            var scheme = iri.Substring(0, ix);
            if (activeContext.TryGetTerm(scheme, out JsonLdTermDefinition td) && td.Prefix)
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.IriConfusedWithPrefix,
                    $"IRI confused with prefix. The {iri} has a scheme that is confusable with a term in the JSON-LD context and no authority to disabmiguate it from a JSON-LD term.");
            }
        }

        // 10 - If vocab is false, transform var to a relative IRI reference using the base IRI from active context, if it exists.
        if (!vocab && activeContext.Base != null)
        {
            if (iri.StartsWith("_:"))
            {
                // Just return a blank node identifier unchanged
                return iri;
            }
            var parsedIri = new Uri(iri);
            Uri relativeIri = activeContext.Base.MakeRelativeUri(parsedIri);

            var relativeIriString = relativeIri.ToString();

            // KA: If IRI is equivalent to base IRI just return last path segment rather than an empty string
            if (string.Empty.Equals(relativeIriString))
            {
                var lastSlashIx = parsedIri.PathAndQuery.LastIndexOf('/');
                relativeIriString = parsedIri.PathAndQuery.Substring(lastSlashIx + 1);
            }

            // To avoid confusion with a keyword, if var has the form of a keyword, prepend to it a period followed by a a slash (./).
            if (JsonLdUtils.MatchesKeywordProduction(relativeIriString))
                relativeIriString = "./" + relativeIriString;

            return relativeIriString;
        }

        // 11 - Finally, return var as is.
        return iri;
    }


    

    private JToken CompactValue(JsonLdContext activeContext, string activeProperty, JObject value)
    {
        JsonLdTermDefinition activeTermDefinition = activeProperty == null ? null : activeContext.GetTerm(activeProperty);

        // 1 - Initialize result to a copy of value.
        JToken result = value.DeepClone();

        // 2 - If the active context has a null inverse context, set inverse context in active context to the result of calling the
        // Inverse Context Creation algorithm using active context.
        // 3 - Initialize inverse context to the value of inverse context in active context.
        // var inverseContext = activeContext.InverseContext; -- Not used?


        // 4 - Initialize language to the language mapping for active property in active context, if any, otherwise to the default language of active context.
        // 5 - Initialize direction to the direction mapping for active property in active context, if any, otherwise to the default base direction of active context.
        var languageMapping = activeContext.Language;
        LanguageDirection? direction = activeContext.BaseDirection;
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
            var typeMapping = activeTermDefinition?.TypeMapping;
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
                JToken typeValue = resultObject["@type"];
                if (typeValue is JArray typeArray)
                {
                    var newArray = new JArray();
                    foreach (JToken item in typeArray)
                    {
                        newArray.Add(CompactIri(activeContext, item.Value<string>(), vocab: true));
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
        else if (SafeEquals(languageMapping, value.ContainsKey("@language") ? value["@language"] : null, StringComparison.OrdinalIgnoreCase) &&
                 SafeEquals(direction.HasValue ? JsonLdUtils.SerializeLanguageDirection(direction.Value) : null,
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
            foreach (JProperty p in r.Properties().ToList())
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
    /// Compare a possibly null string and a possibly null JToken for equality.
    /// </summary>
    /// <remarks>If <paramref name="str"/> is null, return true if <paramref name="t"/> is null and false if it is not null.
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

    

    /// <summary>
    /// Ensure that a JObject has an entry for a given property, initializing it to an empty map if it does not exist.
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


}
