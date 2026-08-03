/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd.Processors;

/// <summary>
/// Various utility methods used by the JSON-LD processor and algorithm implementations.
/// </summary>
internal class JsonLdUtils
{
    /// <summary>
    /// Provides a hashed index of JSON-LD keywords for faster resolution in the parser.
    /// </summary>
    internal static HashSet<string> KeywordSet = new HashSet<string>(JsonLdKeywords.CoreKeywords.Union(JsonLdKeywords.FramingKeywords));


    /// <summary>
    /// Ensure that <paramref name="token"/> is wrapped in an array unless it already is an array.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static JsonArray EnsureArray(JsonNode token)
    {
        if (token is JsonArray array) return array;
        return new JsonArray(token.DetachedClone());
    }

    /// <summary>
    /// Determine if a JSON token represents a JSON object with no properties.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>True if <paramref name="token"/> represents a JSON object and has no child properties, false otherwise.</returns>
    [Obsolete("Use IsEmptyObject(JsonNode) instead.")]
    public static bool IsEmptyMap(JsonNode token)
    {
        return IsEmptyObject(token);
    }

    /// <summary>
    /// Determine if the specified string is a JSON-LD keyword (either API or Framing).
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if <paramref name="value"/> is a JSON-LD keyword, false otherwise.</returns>
    public static bool IsKeyword(string value)
    {
        return KeywordSet.Contains(value);
    }

    /// <summary>
    /// Determine if the token is an object with no properties.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if the token represents a JSON object with no properties, false otherwise.</returns>
    public static bool IsEmptyObject(JsonNode token)
    {
        return token is JsonObject obj && obj.Count == 0;
    }

    /// <summary>
    /// Determine if the specified token is a JSON-LD default object.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is JSON object with an @default property, false otherwise.</returns>
    public static bool IsDefaultObject(JsonNode node)
    {
        return node is JsonObject obj && obj.ContainsKey("@default");
    }

    public static bool HasNonNullProperty(JsonObject obj, string propertyName)
    {
        return obj.TryGetPropertyValue(propertyName, out JsonNode value) && value != null;
    }

    /// <summary>
    /// Determine if a JSON token is a JSON-LD value object.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True of <paramref name="node"/> is a <see cref="JsonObject"/> with a non-null @value property, false otherwise.</returns>
    public static bool IsValueObject(JsonNode node)
    {
        return node is JsonObject obj && HasNonNullProperty(obj, "@value");
    }


    /// <summary>
    /// Determine if a JSON node is a JSON-LD list object.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>True of <paramref name="node"/> is a <see cref="JsonObject"/> with a non-null @list property, false otherwise.</returns>
    public static bool IsListObject(JsonNode node)
    {
        return node is JsonObject obj && HasNonNullProperty(obj, "@list");
    }

    /// <summary>
    /// Determine if a JSON node represents a JSON-LD node reference object.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is an object with a non-null @id property, false otherwise.</returns>
    public static bool IsNodeReference(JsonNode node)
    {
        return node is JsonObject obj && HasNonNullProperty(obj, "@id");
    }

    /// <summary>
    /// Checks if a JSON node represents a subject.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is an object, is not an @value, @set or @list, and either has more than one key or does not have an @id key.</returns>
    public static bool IsSubject(JsonNode node)
    {
        return node is JsonObject obj &&
               !(HasNonNullProperty(obj, "@value") || HasNonNullProperty(obj, "@set") || HasNonNullProperty(obj, "@list")) &&
               (obj.Count > 1 || !HasNonNullProperty(obj, "@id"));
    }

    /// <summary>
    /// Checks if a JSON node represents a subject reference.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is an object with a single @id property.</returns>
    public static bool IsSubjectReference(JsonNode node)
    {
        return node is JsonObject obj && obj.Count == 1 && HasNonNullProperty(obj, "@id");
    }

    /// <summary>
    /// Determine if a JSON node is a JSON-LD graph object.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>True if <paramref name="node"/> is a JsonObject with an @graph property and optionally @id and @index properties and no other properties; false otherwise.</returns>
    public static bool IsGraphObject(JsonNode node)
    {
        if (node is not JsonObject o) return false;
        if (o.Count > 3) return false;
        if (!o.ContainsKey("@graph")) return false;
        return o.All(p => JsonLdKeywords.GraphObjectKeys.Contains(p.Key));
    }

    /// <summary>
    /// Determines if a JSON node is a JSON-LD simple graph object.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <returns>True if <paramref name="node"/> is a JsonObject with an @graph property and optionally an @index property and no other properties; false otherwise.</returns>
    public static bool IsSimpleGraphObject(JsonNode node)
    {
        if (node is not JsonObject o) return false;
        if (o.Count > 2) return false;
        if (!o.ContainsKey("@graph")) return false;
        return o.All(p => p.Key == "@graph" || p.Key == "@index");
    }

    /// <summary>
    /// Determine if a JSON node is an array, optionally testing each item in the array.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <param name="itemTest">The test to be applied to each child item of <paramref name="node"/>.</param>
    /// <returns>True if <paramref name="node"/> is a array and either <paramref name="node"/> is null or returns true for all items in the array, false otherwise.</returns>
    public static bool IsArray(JsonNode node, Func<JsonNode, bool> itemTest = null)
    {
        if (node is not JsonArray array) return false;
        return itemTest == null || array.All(itemTest);
    }

    /// <summary>
    /// Determine if the specified node is an empty array node.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is an array with no items, false otherwise.</returns>
    public static bool IsEmptyArray(JsonNode node)
    {
        return node is JsonArray array && array.Count == 0;
    }

    private static readonly Regex FragmentRegex = new Regex("^([a-zA-Z0-9-._~!$&'()*+,;=:@/?]|%[0-9A-Fa-f]{2})*$");

    /// <summary>
    /// Determine if the specified string is an IRI.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    /// <returns>True if <paramref name="value"/> can be parsed as an IRI, false otherwise.</returns>
    public static bool IsIri(string value)
    {
        // The following would have been ideal, but returns false when the value is a relative IRI that contains a fragment identifier.
        //return Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute);
        if (IsBlankNodeIdentifier(value)) { return false; }
        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri parsed)) { return false; }
        if (parsed.IsAbsoluteUri) { return parsed.IsWellFormedOriginalString(); }
        if (!value.Contains('#')) { return parsed.IsWellFormedOriginalString() || Uri.EscapeUriString(value).Equals(value); }
        if (value.StartsWith("#")) return false;
        var split = value.Split(['#'], 2);
        return Uri.IsWellFormedUriString(split[0], UriKind.Relative) && FragmentRegex.IsMatch(split[1]);
    }

    /// <summary>
    /// Determine if the specified node is an IRI string.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is a string whose value is a valid IRI, false otherwise.</returns>
    public static bool IsIri(JsonNode node)
    {
        return node is JsonValue value &&
            value.SafeValueKind() == JsonValueKind.String &&
            value.GetValue<string>() is string s &&
            IsIri(s);
    }

    /// <summary>
    /// Determine if the specified string is a blank node identifier.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsBlankNodeIdentifier(string value)
    {
        return value != null && value.StartsWith("_:");
    }

    public static bool IsScalar(JsonNode node)
    {
        return !(node == null || node is JsonArray || node is JsonObject);
    }

    public static bool IsScalarOrNull(JsonNode node)
    {
        return node == null || IsScalar(node);
    }

    /// <summary>
    /// Determine if a JSON node represents a string value.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <returns>True if <paramref name="node"/> represents a string value, false otherwise.</returns>
    public static bool IsString(JsonNode node)
    {
        return node.SafeValueKind() == JsonValueKind.String;
    }

    public static bool IsValidBaseDirection(JsonNode token)
    {
        if (token.SafeValueKind() != JsonValueKind.String) return false;
        var value = token.GetValue<string>();
        return value == "ltr" || value == "rtl";
    }


    /// <summary>
    /// Determine if a JSON node represents the null value.
    /// </summary>
    /// <param name="node">The node to test.</param>
    /// <returns>True if the token represents JSON null, false otherwise.</returns>
    public static bool IsNull(JsonNode node)
    {
        return node.SafeValueKind() == JsonValueKind.Null;
    }

    /// <summary>
    /// Determine if a JSON node is a string whose value can be parsed as an absolute IRI.
    /// </summary>
    /// <param name="node">The node to tests.</param>
    /// <returns>True if <paramref name="node"/> represents a JSON string and the value of the string can be parsed as an absolute IRI, false otherwise.</returns>
    public static bool IsAbsoluteIri(JsonNode node)
    {
        if (node.SafeValueKind() != JsonValueKind.String) return false;
        var value = node.GetValue<string>();
        return IsAbsoluteIri(value);
    }

    /// <summary>
    /// Determine if the specified string is an absolute IRI.
    /// </summary>
    /// <param name="value">The string value to be validated.</param>
    /// <returns>True if <paramref name="value"/> can be parsed as an absolute IRI, false otherwise.</returns>
    public static bool IsAbsoluteIri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var u)
               && (
                   // Trying to determine if the TryCreate constructor performed some unwanted escaping
                   // IsWellFormedOriginalString() works most of the time but fails for some of the JSON-LD tests - in particular where the path contains [ or ]
                   u.IsWellFormedOriginalString() || 
                   // This check sees if escaping the original string changes it - this unfortunately fails for IRIs because .NET 
                   Uri.EscapeUriString(value).Equals(value));
    }

    /// <summary>
    /// Determine if a JSON token is a string whose value can be parsed as a relative IRI.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if <paramref name="node"/> is a string node and the value of the string can be parsed as a relative IRI.</returns>
    public static bool IsRelativeIri(JsonNode node)
    {
        if (node.SafeValueKind() != JsonValueKind.String) return false;
        var value = node.GetValue<string>();
        return IsRelativeIri(value);
    }

    /// <summary>
    /// Determine if the specified string is a relative IRI.
    /// </summary>
    /// <param name="value">The string value to be validated.</param>
    /// <returns>True if <paramref name="value"/> can be parsed as a relative IRI, false otherwise.</returns>
    public static bool IsRelativeIri(string value)
    {
        return Uri.TryCreate(value, UriKind.Relative, out _) && Uri.EscapeUriString(value).Equals(value);
    }

    /// <summary>
    /// Determine if the specified string matches the JSON-LD reserved term production.
    /// </summary>
    /// <param name="value">The value to be tested.</param>
    /// <returns>True if <paramref name="value"/> matches the pattern for a reserved term, false otherwise.</returns>
    public static bool MatchesKeywordProduction(string value)
    {
        return Regex.IsMatch(value, "^@[a-zA-Z]+$");
    }

    /// <summary>
    /// Determines if a token represents a JSON-LD node object.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="isTopmostMap"></param>
    /// <returns></returns>
    public static bool IsNodeObject(JsonNode node, bool isTopmostMap = false)
    {
        // A map is a node object if it exists outside of the JSON-LD context and:
        //   - it does not contain the @value, @list, or @set keywords, or
        //   - it is not the top - most map in the JSON-LD document consisting of no other entries than @graph and @context.
        if (node is not JsonObject o) return false;
        if (!(o.ContainsKey("@value") || o.ContainsKey("@list") || o.ContainsKey("@set"))) return true;
        if (!isTopmostMap)
        {
            if (o.ContainsKey("@graph") || o.ContainsKey("@set") && o.Count == 1) return true;
            if (o.ContainsKey("@graph") && o.ContainsKey("@set") && o.Count == 2) return true;
        }

        return false;
    }

    /// <summary>
    /// Compare to value objects.
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>True if <paramref name="t1"/> and <paramref name="t2"/> are equal primitives;
    /// or are both value objects with matching @value, @type, @language and @index values;
    /// or are both subject or subject references with matching @id values.</returns>
    public static bool CompareValues(JsonNode t1, JsonNode t2)
    {
        if (JsonNode.DeepEquals(t1, t2)) return true;
        if (t1 is JsonObject o1 && t2 is JsonObject o2)
        {
            if (IsValueObject(o1) && IsValueObject(o2))
            {
                if (SafeEquals(o1["@value"], o2["@value"]) &&
                    SafeEquals(o1["@type"], o2["@type"]) &&
                    SafeEquals(o1["language"], o2["@language"]) &&
                    SafeEquals(o1["@index"], o2["@index"]))
                {
                    return true;
                }
            }

            return SafeEquals(o1["@id"], o2["@id"]);
        }

        return false;
    }

    /// <summary>
    /// Compare two values for equality safely.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool SafeEquals(JsonNode v1, JsonNode v2)
    {
        if (v1 == null) return v2 == null;
        return CompareValues(v1, v2);
    }

    /// <summary>
    /// Add a value to a subject.
    /// </summary>
    /// <param name="o">The subject to add a value to.</param>
    /// <param name="entry">The name of the property to receive the value.</param>
    /// <param name="value">The value to be added.</param>
    /// <param name="asArray">If true, the property created on the subject is always an array. If false the property created on the subject will be an array only if required to hold mutiple values.</param>
    public static void AddValue(JsonObject o, string entry, JsonNode value, bool asArray = false)
    {
        if (asArray)
        {
            // Ensure target property is an array
            if (!o.ContainsKey(entry))
            {
                o[entry] = new JsonArray();
            }
            else
            {
                o[entry] = EnsureArray(o[entry]);
            }
        }

        if (value is JsonArray valueArray)
        {
            // Call this method to add each individual item
            foreach (JsonNode item in valueArray)
            {
                AddValue(o, entry, item, asArray);
            }
        }
        else
        {
            // Adding a single item
            value = value.DetachedClone();

            // If the property doesn't exist, add value as the single value of the property
            if (!o.ContainsKey(entry))
            {
                o[entry] = value;
            }
            else
            {
                // If property exists and its value is an array, append value to the array
                if (o[entry] is JsonArray entryArray)
                {
                    entryArray.Add(value);
                }
                else
                {
                    // Otherwise convert the target property value to an array and then append value
                    JsonNode existingValue = o[entry];
                    o.Remove(entry);
                    o[entry] = new JsonArray
                    {
                        existingValue,
                        value,
                    };
                }
            }
        }
    }

    /// <summary>
    /// Removes a value from a subject.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="property">The property that relates the value to the subject.</param>
    /// <param name="value">The value to remove.</param>
    /// <param name="propertyIsArray">True if the value of the property is always an array.</param>
    public static void RemoveValue(JsonObject subject, string property, JsonNode value, bool propertyIsArray = false)
    {
        if (!subject.ContainsKey(property)) return;
        var values = EnsureArray(subject[property]).Where(t => !CompareValues(t, value)).ToList();
        switch (values.Count)
        {
            case 0:
                subject.Remove(property);
                break;
            case 1 when !propertyIsArray:
                subject[property] = values[0];
                break;
            default:
            {
                var newArray = new JsonArray();
                foreach (JsonNode v in values) newArray.Add(v.DetachedClone());
                subject[property] = newArray;
                break;
            }
        }
    }

    /// <summary>
    /// Creates a new array which is a concatenation of the provided inputs.
    /// </summary>
    /// <param name="node1">The first input.</param>
    /// <param name="node2">The second input.</param>
    /// <remarks>This method flattens any input arrays.</remarks>
    /// <returns>An array consisting of the (flattened) concatenation of <paramref name="node1"/> and <paramref name="node2"/>.</returns>
    public static JsonArray ConcatenateValues(JsonNode node1, JsonNode node2)
    {
        JsonArray result = EnsureArray(node1);
        if (node2 is JsonArray array)
        {
            foreach (JsonNode c in array) result.Add(c.DeepClone());
        }
        else
        {
            result.Add(node2);
        }

        return result;
    }


    /// <summary>
    /// Gets the value of a property from a subject node, taking ito account possibly aliases defined in the active context.
    /// </summary>
    /// <param name="activeContext">The context to use.</param>
    /// <param name="parent">The subject node to retrieve a property from.</param>
    /// <param name="propertyName">The name of the property whose value is to be retrieved.</param>
    /// <returns>The property value if found, null otherwise.</returns>
    public static JsonNode GetPropertyValue(JsonLdContext activeContext, JsonObject parent, string propertyName)
    {
        if (parent.TryGetPropertyValue(propertyName, out JsonNode ret)) return ret;
        foreach (var alias in activeContext.GetAliases(propertyName))
        {
            if (parent.TryGetPropertyValue(alias, out ret)) return ret;
        }
        return null;
    }

    /// <summary>
    /// Attempt to interpret a JSON token as a language direction value.
    /// </summary>
    /// <param name="value">The token to be parsed.</param>
    /// <returns><see cref="LanguageDirection.Unspecified"/> if <paramref name="value"/> is a JSON null token,
    /// <see cref="LanguageDirection.LeftToRight"/> or <see cref="LanguageDirection.RightToLeft"/> if
    /// <paramref name="value"/> is a string token with the value 'ltr' or 'rtl' respectively.</returns>
    /// <exception cref="JsonLdProcessorException"> raised if <paramref name="value"/> is not a JSON string or null token,
    /// or if <paramref name="value"/> is a string but its value is neither 'ltr' nor 'rtl'.</exception>
    public static LanguageDirection ParseLanguageDirection(JsonNode value)
    {
        switch (value.SafeValueKind())
        {
            case JsonValueKind.Null:
                return LanguageDirection.Unspecified;
            case JsonValueKind.String:
                var directionStr = value.GetValue<string>();
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

    public static string SerializeLanguageDirection(LanguageDirection dir)
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

    public static bool IsBooleanNode(JsonNode value)
    {
        return value.SafeValueKind() == JsonValueKind.True || value.SafeValueKind() == JsonValueKind.False;
    }

    /// <summary>
    /// Merges two JSON objects into a new object, using properties from <paramref name="obj1"/> as the base.
    /// </summary>
    /// <param name="obj1">The first JSON object.</param>
    /// <param name="obj2">The second JSON object.</param>
    /// <returns>A new JSON object containing merged properties from both input objects. Properties from <paramref name="obj2"/> will not overwrite those from <paramref name="obj1"/>.</returns>
    public static JsonObject MergeObjects(JsonObject obj1, JsonObject obj2)
    {
        var result = obj1.DeepClone() as JsonObject;
        foreach (KeyValuePair<string, JsonNode> kvp in obj2)
        {
            result.TryAdd(kvp.Key, kvp.Value.DeepClone());
        }
        return result;
    }

    public static void ReplaceInParent(JsonNode node, JsonNode newValue)
    {
        if (node.Parent is JsonArray array)
        {
            int index = array.IndexOf(node);
            if (index >= 0)
            {
                array[index] = newValue.DetachedClone();
            }
        }
        else if (node.Parent is JsonObject obj)
        {
            var property = obj.FirstOrDefault(p => p.Value == node);
            if (property.Key != null)
            {
                obj[property.Key] = newValue.DetachedClone();
            }
        }
    }

}
