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
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd.Processors;

internal static class FramingProcessor
{
    /// <summary>
    /// Implements the JSON-LD Framing Algorithm.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="subjects"></param>
    /// <param name="frameObjectOrArray"></param>
    /// <param name="parent"></param>
    /// <param name="activeProperty"></param>
    /// <param name="ordered"></param>
    /// <param name="idStack"></param>
    /// <param name="processingMode"></param>
    public static void ProcessFrame(FramingState state, List<string> subjects, JToken frameObjectOrArray,
        JToken parent,
        string activeProperty, bool ordered = false, Stack<string> idStack = null, 
        JsonLdProcessingMode processingMode = JsonLdProcessingMode.JsonLd11)
    {
        // Stack to track circular references when processing embedded nodes
        if (idStack == null) idStack = new Stack<string>();

        // 1 - If frame is an array, set frame to the first member of the array, which must be a valid frame.
        if (frameObjectOrArray is JArray frameArray)
        {
            frameObjectOrArray = frameArray.Count == 0 ? new JObject() : frameArray[0];
            ValidateFrame(frameObjectOrArray);
        }

        var frame = frameObjectOrArray as JObject;

        // 2 - Initialize flags embed, explicit, and requireAll from object embed flag, explicit inclusion flag, and require all flag in state overriding from any property values for @embed, @explicit, and @requireAll in frame.
        JsonLdEmbed embed = GetEmbedOption(frame, state.Embed, processingMode);
        var explicitFlag = GetBooleanOption(frame, "@explicit", state.ExplicitInclusion);
        var requireAll = GetBooleanOption(frame, "@requireAll", state.RequireAll);

        // 3 - Create a list of matched subjects by filtering subjects against frame using the Frame Matching algorithm with state, subjects, frame, and requireAll.
        Dictionary<string, JObject> matchedSubjects = MatchFrame(state, subjects, frame, requireAll);

        // 5 - For each id and associated node object node from the set of matched subjects, ordered lexicographically by id if the optional ordered flag is true:
        var matches = (IEnumerable<KeyValuePair<string, JObject>>) matchedSubjects;
        if (ordered) matches = matches.OrderBy(x => x.Key, StringComparer.Ordinal);
        foreach (KeyValuePair<string, JObject> match in matches)
        {
            var id = match.Key;
            JObject node = match.Value;

            // Set up tracking for embedded nodes, clearing the value for each top-level property
            state.TrackEmbeddedNodes(activeProperty == null);

            // 4.1 - Initialize output to a new dictionary with @id and id.
            var output = new JObject(new JProperty("@id", id));

            // 4.2 - If the embedded flag in state is false and there is an existing embedded node in parent associated with graph name and id in state, do not perform additional processing for this node. 
            if (!state.Embedded && state.HasEmbeddedNode(id))
            {
                continue;
            }

            // 4.3 - Otherwise, if the embedded flag in state is true and either embed is @never or if a circular reference would be created by an embed, add output to parent and do not perform additional processing for this node.
            if (state.Embedded && (embed == JsonLdEmbed.Never || idStack.Contains(id)))
            {
                FramingAppend(parent, output, activeProperty);
                continue;
            }

            // 4.4 - Otherwise, if the embedded flag in state is true, embed is @once, and there is an existing embedded node in parent associated with graph name and id in state, add output to parent and do not perform additional processing for this node.
            if (state.Embedded && (embed == JsonLdEmbed.First || embed == JsonLdEmbed.Once) && state.HasEmbeddedNode(id))
            {
                FramingAppend(parent, output, activeProperty);
                continue;
            }

            if (embed == JsonLdEmbed.Last)
            {
                if (state.HasEmbeddedNode(id))
                {
                    RemoveEmbed(state, id);
                }
            }

            state.AddEmbeddedNode(id, parent, activeProperty);

            idStack.Push(id);
            // 4.5 - If graph map in state has an entry for id:
            if (state.GraphMap.ContainsKey(id))
            {
                var recurse = false;
                JObject subframe = null;
                // 4.5.1 - If frame does not have the key @graph, set recurse to true, unless graph name in state is @merged and set subframe to a new empty dictionary.
                if (!frame.ContainsKey("@graph"))
                {
                    recurse = !state.GraphName.Equals("@merged");
                    subframe = new JObject();
                }
                // 4.5.2 - Otherwise, set subframe to the first entry for @graph in frame, or a new empty dictionary, if it does not exist, and set recurse to true, unless id is @merged or @default.
                else
                {
                    //var graphEntry = (frame["@graph"] as JArray);
                    //if (graphEntry == null)
                    //{
                    //    graphEntry = new JArray(new JObject());
                    //    frameObjectOrArray["@graph"] = graphEntry;
                    //}
                    //else if (graphEntry.Count == 0)
                    //{
                    //    graphEntry.Add(new JObject());
                    //}
                    // subframe = graphEntry[0] as JObject;
                    if (frame.ContainsKey("@graph"))
                    {
                        subframe = frame["@graph"][0] as JObject;
                    }
                    else
                    {
                        subframe = new JObject();
                    }

                    recurse = !(id.Equals("@merged") || id.Equals("@default"));
                }

                // 4.5.3 - If recurse is true:
                if (recurse)
                {
                    // 4.5.3.1 - Set the value of graph name in state to id.
                    state.GraphStack.Push(state
                        .GraphName); // KA: Using stack to track current graph instead of copying state
                    state.GraphName = id;
                    // 4.5.3.2 - Set the value of embedded flag in state to false.
                    state.Embedded = false;
                    // 4.5.3.3 - Invoke the algorithm using a copy of state with the value of graph name set to id and the value of embedded flag set to false,
                    // the keys from the graph map in state associated with id as subjects, subframe as frame, output as parent, and @graph as active property. 
                    ProcessFrame(state,
                        state.Subjects.Properties().Select(p => p.Name).ToList(),
                        subframe, output, "@graph", ordered, idStack);
                    // Pop the value from graph stack in state and set graph name in state back to that value.
                    state.GraphName = state.GraphStack.Pop();
                }
            }

            // 4.6  - If frame has an @included entry, invoke the algorithm using a copy of state with the value of embedded flag set to false, subjects, frame, output as parent, and @included as active property. 
            if (frame.ContainsKey("@included"))
            {
                var oldEmbedded = state.Embedded;
                state.Embedded = false;
                JToken includeFrame = frame["@included"];
                ProcessFrame(state, subjects, includeFrame, output, "@included", ordered, idStack);
                state.Embedded = oldEmbedded;
            }

            // 4.7 - For each property and objects in node, ordered by property:
            IEnumerable<JProperty> nodeProperties = node.Properties();
            if (ordered) nodeProperties = nodeProperties.OrderBy(p => p.Name, StringComparer.Ordinal);
            foreach (JProperty p in nodeProperties)
            {
                var property = p.Name;
                var objects =p.Value as JArray;

                // 4.7.1 - If property is a keyword, add property and objects to output.
                if (JsonLdUtils.IsKeyword(property))
                {
                    output[property] = p.Value;
                    continue;
                }

                // 4.7.2 - Otherwise, if property is not in frame, and explicit is true, processors must not add any values for property to output, and the following steps are skipped.
                if (!frame.ContainsKey(property) && explicitFlag)
                {
                    continue;
                }

                // 4.7.3 - For each item in objects:
                foreach (JToken item in objects)
                {
                    // 4.7.3.1 - If item is a dictionary with the property @list, 
                    // then each listitem in the list is processed in sequence and 
                    // added to a new list dictionary in output:
                    if (JsonLdUtils.IsListObject(item))
                    {
                        var list = new JObject();
                        output[property] =
                            new JArray(list); // KA: Not sure what the correct key is for the list object
                        foreach (JToken listItem in item["@list"] as JArray)
                        {
                            // 4.7.3.1.1 - If listitem is a node reference, invoke the recursive algorithm using state, 
                            // the value of @id from listitem as the sole member of a new subjects array, 
                            // the first value from @list in frame as frame, list as parent, 
                            // and @list as active property. 
                            // If frame does not exist, create a new frame using a new dictionary 
                            // with properties for @embed, @explicit and @requireAll taken from embed, explicit and requireAll. 
                            if (JsonLdUtils.IsNodeReference(listItem))
                            {
                                JToken listFrame = null;
                                if (frame.ContainsKey(property) && frame[property] is JArray fpArray &&
                                    fpArray.Count > 0 && fpArray[0] is JObject subframe &&
                                    subframe.ContainsKey("@list"))
                                {
                                    listFrame = subframe["@list"] as JArray;
                                }
                                if (listFrame == null) listFrame = MakeFrameObject(embed, explicitFlag, requireAll);
                                var oldEmbedded = state.Embedded;
                                state.Embedded = true;
                                ProcessFrame(state,
                                    new List<string> {listItem["@id"].Value<string>()},
                                    listFrame,
                                    list,
                                    "@list",
                                    ordered,
                                    idStack);
                                state.Embedded = oldEmbedded;
                            }
                            // 4.7.3.1.2 - Otherwise, append a copy of listitem to @list in list.
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
                    // 4.7.3.2 - If item is a node reference, invoke the algorithm using
                    // a copy of state with the value of embedded flag set to true,
                    // the value of @id from item as the sole item in a new subjects array,
                    // the first value from property in frame as frame,
                    // output as parent, and property as active property.
                    // If frame does not exist, create a new frame using a new map with properties for @embed, @explicit and @requireAll taken from embed, explicit and requireAll.
                    else if (JsonLdUtils.IsNodeReference(item))
                    {
                        JObject newFrame = ((frameObjectOrArray[property] as JArray)?[0]) as JObject ??
                                           MakeFrameObject(embed, explicitFlag, requireAll);
                        var oldEmbedded = state.Embedded;
                        state.Embedded = true;
                        ProcessFrame(state,
                            new List<string> {item["@id"].Value<string>()},
                            newFrame,
                            output,
                            property,
                            ordered,
                            idStack, 
                            processingMode);
                        state.Embedded = oldEmbedded;
                    }
                    // 4.7.3.3 - Otherwise, append a copy of item to active property in output.
                    else
                    {
                        // KA - should only append if value pattern matches?
                        if (frameObjectOrArray[property] == null ||
                            frameObjectOrArray[property][0]["@value"] == null ||
                            ValuePatternMatch(frameObjectOrArray[property], item))
                        {
                            FramingAppend(output, item, property);
                        }
                    }
                }
            }

            // 4.7.4 - For each non-keyword property and objects in frame (other than `@type) that is not in output: 
            foreach (JProperty frameProperty in frame.Properties())
            {
                var property = frameProperty.Name;
                if (property.Equals("@type"))
                {
                    if (!IsDefaultObjectArray(frameProperty.Value)) continue;
                }
                else
                {
                    if (output.ContainsKey(property) || JsonLdUtils.IsKeyword(property)) continue;
                }

                var objects = frameProperty.Value as JArray;
                if (objects == null || objects.Count == 0)
                {
                    // KA - Check that this is still needed?
                    // Initialise as an array containing an empty frame
                    objects = new JArray(new JObject());
                }

                // 4.7.4.1 - Let item be the first element in objects, which must be a frame object.
                JToken item = objects[0];
                ValidateFrame(item);

                // 4.7.4.2 - Set property frame to the first item in objects or a newly created frame object if value is objects. property frame must be a dictionary.
                JObject propertyFrame = objects[0] as JObject ?? MakeFrameObject(embed, explicitFlag, requireAll); // KA - incomplete as I can't make sense of the spec algorithm here
                // 4.7.4.3 - Skip property and property frame if property frame contains @omitDefault with a value of true, or does not contain @omitDefault and the value of the omit default flag is true.
                var frameOmitDefault = GetBooleanOption(propertyFrame, "@omitDefault", state.OmitDefault);
                if (frameOmitDefault)
                {
                    continue;
                }

                // 4.7.4.4 - Add property to output with a new dictionary having a property @preserve and a value that is a copy of the value of @default in frame if it exists, or the string @null otherwise.
                JArray defaultValue = JsonLdUtils.EnsureArray(propertyFrame["@default"]) ?? new JArray("@null");
                output[property] = new JObject(new JProperty("@preserve", defaultValue));
                //if (!(defaultValue is JArray)) defaultValue = new JArray(defaultValue);
                //FramingAppend(output, new JObject(new JProperty("@preserve", defaultValue)), property);
                // // output[property] = new JObject(new JProperty("@preserve", frame["@default"] ?? "@null"));
            }

            // 4.7.5 - If frame has the property @reverse, then for each reverse property and sub frame that are the values of @reverse in frame:
            if (frame.ContainsKey("@reverse"))
            {
                foreach (JProperty rp in (frame["@reverse"] as JObject).Properties())
                {
                    var reverseProperty = rp.Name;
                    JToken subFrame = rp.Value;
                    // 4.7.5.1 - Create a @reverse property in output with a new dictionary reverse dict as its value.
                    output["@reverse"] ??= new JObject();
                    JToken reverseDict = output["@reverse"];

                    // 4.7.5.2 - For each reverse id and node in the map of flattened subjects that has the property reverse property containing a node reference with an @id of id:
                    foreach (JProperty p in state.Subjects.Properties())
                    {
                        var n = p.Value as JObject;
                        if (n[reverseProperty] is not JArray reversePropertyValues) continue;
                        if (reversePropertyValues.Any(x => x["@id"]?.Value<string>().Equals(id) == true))
                        {
                            // 4.7.5.2.1 - Add reverse property to reverse dict with a new empty array as its value.
                            var reverseId = p.Name;
                            reverseDict[reverseProperty] ??= new JArray();
                            // 4.7.5.2.2 - Invoke the recursive algorithm using state, the reverse id as the sole member of a new subjects array, sub frame as frame, null as active property, and the array value of reverse property in reverse dict as parent.
                            var oldEmbedded = state.Embedded;
                            state.Embedded = true;
                            ProcessFrame(state,
                                new List<string> {reverseId},
                                subFrame,
                                reverseDict[reverseProperty],
                                null,
                                ordered,
                                idStack);
                            state.Embedded = oldEmbedded;
                        }
                    }
                }
            }

            // 4.7.6 - Once output has been set are required in the previous steps, add output to parent.
            FramingAppend(parent, output, activeProperty);
            idStack.Pop();
        }
    }

    private static JsonLdEmbed GetEmbedOption(JObject frame, JsonLdEmbed defaultValue, JsonLdProcessingMode processingMode)
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
                case "@once":
                    return JsonLdEmbed.Once;
                case "@first" when processingMode == JsonLdProcessingMode.JsonLd10:
                    return JsonLdEmbed.First;
                case "@last" when processingMode == JsonLdProcessingMode.JsonLd10:
                    return JsonLdEmbed.Last;
                case "@link" when processingMode == JsonLdProcessingMode.JsonLd10:
                    return JsonLdEmbed.Link;
                case "@first":
                case "@last":
                case "@link":
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidEmbedValue,
                        $"Invalid @embed value {embedString}. This value is only valid if the processing mode is JSON-LD 1.0.");
                case "false":
                case "@never":
                    return JsonLdEmbed.Never;
                default:
                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidEmbedValue,
                        $"Invalid @embed value {embedString}");
            }
        }
        return defaultValue;
    }

    private static JObject MakeFrameObject(JsonLdEmbed embed, bool explicitFlag, bool requireAll)
    {
        var ret = new JObject(
            new JProperty("@embed", JsonLdEmbedAsString(embed)),
            new JProperty("@explicit", explicitFlag),
            new JProperty("@requireAll", requireAll));
        return ret;
    }

    private static string JsonLdEmbedAsString(JsonLdEmbed embed)
    {
        switch (embed)
        {
            case JsonLdEmbed.Always:
                return "@always";
            case JsonLdEmbed.Once:
                return "@once";
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

    private static bool GetBooleanOption(JObject frame, string property, bool defaultValue)
    {
        if (frame[property] != null)
        {
            switch (frame[property])
            {
                case JValue optValue:
                    return optValue.Type == JTokenType.Null ? defaultValue :optValue.Value<bool>();
                case JObject optObject:
                    return optObject.ContainsKey("@value") ? optObject["@value"].Value<bool>() : defaultValue;
            }
        }
        return defaultValue;
    }


    private static Dictionary<string, JObject> MatchFrame(FramingState state, IEnumerable<string> subjects,
        JObject frame, bool requireAll)
    {
        var matches = new Dictionary<string, JObject>();
        JArray idMatches = frame.ContainsKey("@id") ? JsonLdUtils.EnsureArray(frame["@id"]) : null;
        JArray typeMatches = frame.ContainsKey("@type") ? JsonLdUtils.EnsureArray(frame["@type"]) : null;
        var propertyMatches = new Dictionary<string, JArray>();
        foreach (KeyValuePair<string, JToken> p in frame)
        {
            if (JsonLdUtils.IsKeyword(p.Key)) continue;
            propertyMatches[p.Key] = JsonLdUtils.EnsureArray(p.Value);
        }
        foreach (var subject in subjects)
        {
            var node = state.Subjects[subject] as JObject;
            // Check @id and @type first
            if (idMatches != null)
            {
                if (!(IsWildcard(idMatches) || IsMatchNone(idMatches) || idMatches.Any(v=>v.Value<string>().Equals(subject))))
                {
                    // No match on id. Continue to next subject
                    continue;
                }

                if (!requireAll)
                {
                    matches.Add(subject, node);
                    continue;
                }
            }
            if (typeMatches != null)
            {
                JArray nodeTypes = node.ContainsKey("@type") ? JsonLdUtils.EnsureArray(node["@type"]) : null;
                var hasTypeMatch =
                    IsMatchNone(typeMatches) && (nodeTypes == null || nodeTypes.Count == 0) ||
                    IsWildcard(typeMatches) && nodeTypes != null && nodeTypes.Count > 0 ||
                    IsDefaultObjectArray(typeMatches) ||
                    nodeTypes != null && typeMatches.Any(t => nodeTypes.Any(nt => nt.Value<string>().Equals(t.Value<string>())));
                if (hasTypeMatch)
                {
                    if (!requireAll)
                    {
                        matches.Add(subject, node);
                        continue;
                    }
                }
                else
                {
                    // Failed to match on type - always fails the node
                    continue;
                }
                //if (IsMatchNone(typeMatches) && nodeTypes != null && nodeTypes.Count > 0) continue;
                //if (IsWildcard(typeMatches) && (nodeTypes == null || nodeTypes.Count == 0)) continue;
                //if (nodeTypes == null) continue;
                //if (!typeMatches.Any(t => nodeTypes.Any(nt=>nt.Value<string>().Equals(t.Value<string>())))) continue;
                //if (!requireAll)
                //{
                //    matches.Add(subject, node);
                //    continue;
                //}
            }


            if (propertyMatches.Count == 0)
            {
                // No additional properties to match
                matches.Add(subject, node);
                continue;
            }

            // If requireAll, assume there is a match and break when disproven
            // If !requireAll, assume there is no match and break when disproven
            var match = requireAll;
            var hasNonDefaultMatch = false;
            foreach (KeyValuePair<string, JArray> pm in propertyMatches)
            {
                MatchType propertyMatch = MatchProperty(state, node, pm.Key, pm.Value, requireAll);
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
                }
                else if (propertyMatch == MatchType.Match)
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

    private static MatchType MatchProperty(FramingState state, JObject node, string property, JToken frameValue, bool requireAll)
    {
        var nodeValues = node[property] as JArray;
        var frameArray = frameValue as JArray;
        bool isMatchNone = false, isWildcard = false, hasDefault = false;
        if (frameArray.Count == 0)
        {
            isMatchNone = true;
        }
        else if (frameArray.Count == 1 && JsonLdUtils.IsDefaultObject(frameArray[0]))
        {
            hasDefault = true;
        }
        else if (frameArray.Count == 1 && IsWildcard(frameArray[0]))
        {
            isWildcard = true;
        }
        
        if (nodeValues == null || nodeValues.Count == 0)
        {
            if (hasDefault)
            {
                return MatchType.DefaultMatch;
            }
        }

        // Non-existant property cannot match frame
        if (nodeValues == null) return MatchType.NoMatch;

        // Frame specifies match none - nodeValues must be empty
        if (isMatchNone && nodeValues.Count != 0) return MatchType.Abort;

        // Frame specifies match wildcard - nodeValues must be non-empty
        if (isWildcard && nodeValues.Count > 0) return MatchType.Match;

        if (JsonLdUtils.IsValueObject(frameArray[0]))
        {
            // frameArray is a value pattern array
            foreach (JToken valuePattern in frameArray)
            {
                foreach (JToken value in nodeValues)
                {
                    if (ValuePatternMatch(valuePattern, value))
                    {
                        return MatchType.Match;
                    }
                }
            }
            return MatchType.NoMatch;
        }

        if (JsonLdUtils.IsListObject(frameArray[0]))
        {
            JToken frameListValue = frameArray[0]["@list"][0];
            if (JsonLdUtils.IsListObject(nodeValues[0]))
            {
                JToken nodeListValues = nodeValues[0]["@list"];
                if (JsonLdUtils.IsValueObject(frameListValue))
                {
                    if (nodeListValues.Any(v => ValuePatternMatch(frameListValue, v)))
                    {
                        return MatchType.Match;
                    }
                } else if (JsonLdUtils.IsSubject(frameListValue) || JsonLdUtils.IsSubjectReference(frameListValue))
                {
                    if (nodeListValues.Any(v => NodePatternMatch(state, frameListValue as JObject, v, requireAll)))
                    {
                        return MatchType.Match;
                    }
                }
            }
        }

        // frameArray is a node pattern array
        var valueSubjects = nodeValues.Where(x => x["@id"] != null).Select(x => x["@id"].Value<string>()).ToList();
        if (valueSubjects.Any())
        {
            foreach (JToken subframe in frameArray)
            {
                Dictionary<string, JObject> matchedSubjects = MatchFrame(state, valueSubjects, subframe as JObject, requireAll);
                if (matchedSubjects.Any()) return MatchType.Match;
            }
        }

        return hasDefault ? MatchType.Match : MatchType.NoMatch;
    }

    private static bool NodePatternMatch(FramingState state, JObject frame, JToken value, bool requireAll)
    {
        if (!(value is JObject valueObject) || !valueObject.ContainsKey("@id")) return false;
        Dictionary<string, JObject> matches = MatchFrame(state, new[] {valueObject["@id"].Value<string>()}, frame, requireAll);
        return matches.Any();

    }

    private static bool ValuePatternMatch(JToken valuePattern, JToken value)
    {
        if (valuePattern is JArray) valuePattern = ((JArray)valuePattern)[0];
        var valuePatternObject = valuePattern as JObject;
        var valueObject = value as JObject;
        if (valuePatternObject == null || valueObject == null) return false;
        if (valuePatternObject.Count == 0)
        {
            // Pattern is wildcard
            return true;
        }
        JToken v1 = valueObject["@value"];
        JToken t1 = valueObject["@type"];
        JToken l1 = valueObject["@language"];
        JToken v2 = valuePatternObject["@value"];
        JToken t2 = valuePatternObject["@type"];
        JToken l2 = valuePatternObject["@language"];
        return ValuePatternTokenMatch(v2, v1) && ValuePatternTokenMatch(t2, t1) && ValuePatternTokenMatch(l2, l1, true);
    }

    private static bool ValuePatternTokenMatch(JToken patternToken, JToken valueToken, bool normalizeCase = false)
    {
        if (patternToken == null && valueToken == null) return true;
        if (IsWildcard(patternToken))
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
            if (normalizeCase) valueToken = Lowercase(valueToken);
            return normalizeCase ? 
                patternTokenArray.Any(x=>JToken.DeepEquals(Lowercase(x), valueToken)) :
                patternTokenArray.Any(x => JToken.DeepEquals(x, valueToken));
        }
        // Otherwise the pattern specifies a single value to match - just do a straight value match
        return normalizeCase
            ? JToken.DeepEquals(Lowercase(patternToken), Lowercase(valueToken))
            : JToken.DeepEquals(patternToken, valueToken);
    }

    private static JToken Lowercase(JToken token)
    {
        if (token == null) return null;
        if (token.Type == JTokenType.String){
            return new JValue(token.Value<string>().ToLower(CultureInfo.InvariantCulture));
        }

        return token;
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

    private static bool IsWildcard(JToken token)
    {
        if (token == null) return false;
        switch (token.Type)
        {
            case JTokenType.Array:
                return (token as JArray).Any(IsWildcard);
            case JTokenType.Object:
                return (token as JObject).Properties()
                    .All(p => JsonLdKeywords.FramingKeywords.Contains(p.Name));
            default:
                return false;
        }
    }

    private static bool IsMatchNone(JToken token)
    {
        return JsonLdUtils.IsEmptyArray(token);
    }

    private static bool IsEmptyMapArray(JToken token)
    {
        return (token is JArray array) && array.Count == 1 && JsonLdUtils.IsEmptyMap(array[0]);
    }

    /// <summary>
    /// Determine if the specified token is a valid frame object.
    /// </summary>
    /// <param name="value">The token to validate.</param>
    /// <exception cref="JsonLdProcessorException">Raised if <paramref name="value"/> is not a valid frame object.</exception>
    private static void ValidateFrame(JToken value)
    {
        // 1.1 - Frame MUST be a map.
        if (!(value is JObject obj))
        {
            throw new JsonLdProcessorException(JsonLdErrorCode.InvalidFrame,
                $"Invalid frame. The frame must be a map.");

        }

        // 1.2 - If frame has an @id entry, its value MUST be either an array containing a single empty map as a
        // value, a valid IRI or an array where all values are valid IRIs.
        if (obj.ContainsKey("@id"))
        {
            JToken idValue = obj["@id"];
            if (!(IsEmptyMapArray(idValue) || JsonLdUtils.IsIri(idValue) || JsonLdUtils.IsArray(idValue, JsonLdUtils.IsIri)))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidFrame,
                    "Invalid frame. The value of the @id property must be either " +
                    "an array containing a single empty map; an IRI string or an array of IRI strings.");
            }
        }

        // 1.3 - If frame has a @type entry, its value MUST be either an array containing a single empty map as a
        // value, an array containing a map with a entry whose key is @default, a valid IRI or an array where all
        // values are valid IRIs.
        if (obj.ContainsKey("@type"))
        {
            JToken typeValue = obj["@type"];
            if (!(IsEmptyMapArray(typeValue) || IsDefaultObjectArray(typeValue) || JsonLdUtils.IsIri(typeValue) ||
                  JsonLdUtils.IsArray(typeValue, JsonLdUtils.IsIri)))
            {
                throw new JsonLdProcessorException(JsonLdErrorCode.InvalidFrame,
                    "Invalid frame. The value of the @type property must be either " +
                    "an array containing a single empty map, " +
                    "an array containing a single map with an @default key, an IRI string, " +
                    "or an array of IRI strings.");
            }
        }
    }

    private static void FramingAppend(JToken parent, JToken child, string activeProperty)
    {
        if (parent is JArray parentArray)
        {
            if (activeProperty != null)
            {
                throw new ArgumentException("activeProperty must be null when parent is an array");
            }
            parentArray.Add(child);
        }
        else if (parent is JObject parentObject)
        {
            if (string.IsNullOrEmpty(activeProperty))
            {
                throw new ArgumentException(
                    "Active property must be a non-null value when the parent is a JSON object",
                    nameof(activeProperty));
            }
            var array = parentObject[activeProperty] as JArray;
            if (array == null)
            {
                parent[activeProperty] = array = new JArray();
            }
            array.Add(child);
        }
    }

    private static bool IsDefaultObjectArray(JToken token)
    {
        return token is JArray array &&
               array.Count == 1 &&
               array[0] is JObject o &&
               o.Count == 1 &&
               o.ContainsKey("@default");
    }

    private static void RemoveEmbed(FramingState state, string id)
    {
        Tuple<JToken, string> embed = state.GetEmbeddedNode(id);
        if (embed == null) return;
        JToken parent = embed.Item1;
        var property = embed.Item2;
        var subject = new JObject(new JProperty("@id", id));
        if (parent is JArray parentArray)
        {
            for(var i = 0; i < parentArray.Count; i++)
            {
                JToken item = parentArray[i];
                if (item is JObject itemObject && itemObject.ContainsKey("@id") && itemObject["@id"].Value<string>().Equals(id))
                {
                    parent[i] = subject;
                    break;
                }
            }
        }
        else
        {
            var useArray = JsonLdUtils.IsArray(parent[property]);
            JsonLdUtils.RemoveValue(parent as JObject, property, subject, useArray);
            JsonLdUtils.AddValue(parent as JObject, property, subject, useArray);
        }
    }
}
