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
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
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
    public static JArray EnsureArray(JToken token)
    {
        if (token is JArray array) return array;
        return new JArray(token);
    }

    /// <summary>
    /// Determine if a JSON token represents a JSON object with no properties.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>True if <paramref name="token"/> represents a JSON object and has no child properties, false otherwise.</returns>
    public static bool IsEmptyMap(JToken token)
    {
        return token.Type == JTokenType.Object && !token.Children().Any();
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
    public static bool IsEmptyObject(JToken token)
    {
        return (token is JObject obj && obj.Count == 0);
    }

    /// <summary>
    /// Determine if the specified token is a JSON-LD default object.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is JSON object with an @default property, false otherwise.</returns>
    public static bool IsDefaultObject(JToken token)
    {
        return token is JObject o && o.ContainsKey("@default");
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
    /// Determine if a JSON token is a JSON-LD list object.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>True of <paramref name="token"/> is a <see cref="JObject"/> with a. <code>@list</code> property, false otherwise.</returns>
    public static bool IsListObject(JToken token)
    {
        return ((token as JObject)?.Property("@list")) != null;
    }

    /// <summary>
    /// Determine if a JSON token represents a JSON-LD node reference object.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is an object wth an @id property, false otherwise.</returns>
    public static bool IsNodeReference(JToken token)
    {
        return (token as JObject)?.Property("@id") != null;
    }

    /// <summary>
    /// Checks if a JSON token represents a subject.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is an object, is not an @vale, @set or @list, and either has more than one key or does not have an @id key.</returns>
    public static bool IsSubject(JToken token)
    {
        return token is JObject t &&
               !(t.ContainsKey("@value") || t.ContainsKey("@set") || t.ContainsKey("@list")) &&
               (t.Count > 1 || !t.ContainsKey("@id"));
    }

    /// <summary>
    /// Checks if a JSON token represents a subject reference.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is an object with a single @id property.</returns>
    public static bool IsSubjectReference(JToken token)
    {
        return token is JObject t && t.Count == 1 && t.ContainsKey("@id");
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
        return o.Properties().All(p => JsonLdKeywords.GraphObjectKeys.Contains(p.Name));
    }

    /// <summary>
    /// Determines if a JSON token is a JSON-LD simple graph object.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>True if <paramref name="token"/> is a JObject with an @graph property and optionally an @index property and no other properties; false otherwise.</returns>
    public static bool IsSimpleGraphObject(JToken token)
    {
        if (!(token is JObject o)) return false;
        if (!o.ContainsKey("@graph")) return false;
        return (o.Properties().All(p => p.Name == "@graph" || p.Name == "@index"));
    }

    /// <summary>
    /// Determine if a JSON token is an array, optionally testing each item in the array.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <param name="itemTest">The test to be applied to each child item of <paramref name="token"/>.</param>
    /// <returns>True if <paramref name="token"/> is a array and either <paramref name="itemTest"/> is null or returns true for all items in the array, false otherwise.</returns>
    public static bool IsArray(JToken token, Func<JToken, bool> itemTest = null)
    {
        if (token.Type != JTokenType.Array) return false;
        return itemTest == null || token.Children().All(itemTest);
    }

    /// <summary>
    /// Determine if the specified token is an empty array token.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is an array with no items, false otherwise.</returns>
    public static bool IsEmptyArray(JToken token)
    {
        return token is JArray array && array.Count == 0;
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
        var split = value.Split(new[] { '#' }, 2);
        return Uri.IsWellFormedUriString(split[0], UriKind.Relative) && FragmentRegex.IsMatch(split[1]);
    }

    /// <summary>
    /// Determine if the specified token is an IRI string.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is a string whose value is a valid IRI, false otherwise.</returns>
    public static bool IsIri(JToken token)
    {
        return (token.Type == JTokenType.String && IsIri(token.Value<string>()));
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

    public static bool IsScalar(JToken token)
    {
        return !(token == null || token.Type == JTokenType.Array || token.Type == JTokenType.Object);
    }

    /// <summary>
    /// Determine if a JSON token represents a string value.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>True if <paramref name="token"/> represents a string value, false otherwise.</returns>
    public static bool IsString(JToken token)
    {
        return token.Type == JTokenType.String;
    }

    public static bool IsValidBaseDirection(JToken token)
    {
        if (token.Type != JTokenType.String) return false;
        return token.Value<string>() == "ltr" || token.Value<string>() == "rtl";
    }


    /// <summary>
    /// Determine if a JSON token represents the null value.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>True if the token represents JSON null, false otherwise.</returns>
    public static bool IsNull(JToken token)
    {
        return token.Type == JTokenType.Null;
    }

    public static bool IsAbsoluteIri(JToken token)
    {
        if (!(token is JValue value)) return false;
        return value.Type == JTokenType.String && IsAbsoluteIri(value.Value<string>());
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
    /// <param name="token">The token to check.</param>
    /// <returns>True if <paramref name="token"/> is a string token and the value of the string can be parsed as a relative IRI.</returns>
    public static bool IsRelativeIri(JToken token)
    {
        if (!(token is JValue value)) return false;
        return value.Type == JTokenType.String && IsRelativeIri(value.Value<string>());
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
    /// <param name="token"></param>
    /// <param name="isTopmostMap"></param>
    /// <returns></returns>
    public static bool IsNodeObject(JToken token, bool isTopmostMap = false)
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

    /// <summary>
    /// Compare to value objects.
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>True if <paramref name="t1"/> and <paramref name="t2"/> are equal primitives;
    /// or are both value objects with matching @value, @type, @language and @index values;
    /// or are both subject or subject references with matching @id values.</returns>
    public static bool CompareValues(JToken t1, JToken t2)
    {
        if (t1.Equals(t2)) return true;
        if (t1 is JObject o1 && t2 is JObject o2)
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
    public static bool SafeEquals(JToken v1, JToken v2)
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
    public static void AddValue(JObject o, string entry, JToken value, bool asArray = false)
    {
        if (asArray)
        {
            // Ensure target property is an array
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
            // Call this method to add each individual item
            foreach (JToken item in valueArray)
            {
                AddValue(o, entry, item, asArray);
            }
        }
        else
        {
            // Adding a single item

            // If the property doesn't exist, add value as the single value of the property
            if (!o.ContainsKey(entry))
            {
                o[entry] = value;
            }
            else
            {
                // If property exists and its value is an array, append value to the array
                if (o[entry] is JArray entryArray)
                {
                    entryArray.Add(value);
                }
                else
                {
                    // Otherwise convert the target property value to an array and then append value
                    entryArray = new JArray(o[entry]);
                    entryArray.Add(value);
                    o[entry] = entryArray;
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
    public static void RemoveValue(JObject subject, string property, JToken value, bool propertyIsArray = false)
    {
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
                var array = new JArray();
                foreach (JToken v in values) array.Add(v);
                subject[property] = array;
                break;
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
    public static JArray ConcatenateValues(JToken token1, JToken token2)
    {
        JArray result = EnsureArray(token1);
        if (token2 is JArray)
        {
            foreach (JToken c in token2.Children()) result.Add(c);
        }
        else
        {
            result.Add(token2);
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
    public static JToken GetPropertyValue(JsonLdContext activeContext, JObject parent, string propertyName)
    {
        if (parent.TryGetValue(propertyName, out JToken ret)) return ret;
        foreach (var alias in activeContext.GetAliases(propertyName))
        {
            if (parent.TryGetValue(alias, out ret)) return ret;
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
    public static LanguageDirection ParseLanguageDirection(JToken value)
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

}
