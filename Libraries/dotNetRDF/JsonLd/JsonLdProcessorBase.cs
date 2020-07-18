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

using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Base class for the API and Framing JSON-LD processors.
    /// </summary>
    public class JsonLdProcessorBase
    {
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
        /// The list of JSON-LD keywords added by the Framing specification.
        /// </summary>
        public static readonly string[] JsonLdFramingKeywords =
        {
            "@default",
            "@embed",
            "@explicit",
            "@omitDefault",
            "@requireAll",
        };

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
        /// The list of JSON-LD keywords defined by the API and Processing specification.
        /// </summary>
        public static readonly string[] JsonLdKeywords =
        {
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

        /// <summary>
        /// Determine if the specified string is a JSON-LD keyword (either API or Framing).
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if <paramref name="value"/> is a JSON-LD keyword, false otherwise.</returns>
        public static bool IsKeyword(string value)
        {
            return JsonLdProcessorBase.JsonLdKeywords.Contains(value) ||
                   JsonLdProcessorBase.JsonLdFramingKeywords.Contains(value);
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

        /// <summary>
        /// Determine if the specified string is an IRI.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns>True if <paramref name="value"/> can be parsed as an IRI, false otherwise.</returns>
        public static bool IsIri(string value)
        {
            // The following would have been ideal, but returns false when the value contains a fragment identifier.
            //return Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute);

            return !IsBlankNodeIdentifier(value) &&
                   Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _) &&
                   Uri.EscapeUriString(value).Equals(value) && // Value must be fully escaped
                   !value.StartsWith("#"); // Value cannot be a fragment identifier on its own
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

        /// <summary>
        /// Determine i the specified token is a blank node identifier string.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>True if <paramref name="token"/> is a string whose value is a valid IRI, false otherwise.</returns>
        public static bool IsBlankNodeIdentifier(JToken token)
        {
            return token.Type == JTokenType.String && IsBlankNodeIdentifier(token.Value<string>());
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
                    foreach (var v in values) array.Add(v);
                    subject[property] = array;
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a value to a subject. If the value is a array, all values in the array will be added.
        /// </summary>
        /// <param name="subject">The subject to add the value to.</param>
        /// <param name="property">The property that relates the value to the subject.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="propertyIsArray">True if the property is always an array. Defaults to false.</param>
        /// <param name="valueIsArray">True if the value to be added should be preserved as an array. Defaults to false.</param>
        /// <param name="allowDuplicate">True to allow duplicates. Defaults to true.</param>
        /// <param name="prependValue">True to prepend <paramref name="value"/> to any existing values, false to append. Defaults to false.</param>
        public static void AddValue(JObject subject, string property, JToken value,
            bool propertyIsArray = false, bool valueIsArray = false, bool allowDuplicate = true,
            bool prependValue = false)
        {
            if (valueIsArray)
            {
                subject[property] = value;
            }
            else if (value is JArray valueArray)
            {
                if (valueArray.Count == 0 && propertyIsArray && !subject.ContainsKey(property))
                {
                    subject[property] = new JArray();
                }

                if (prependValue)
                {
                    var newArray = new JArray(valueArray);
                    foreach (var item in EnsureArray(subject[property]))
                    {
                        newArray.Add(item);
                    }

                    subject[property] = newArray;
                }
                else
                {
                    var newArray = new JArray();
                    foreach (var item in EnsureArray(subject[property])) newArray.Add(item);
                    foreach (var item in valueArray) newArray.Add(item);
                    subject[property] = newArray;
                }
            }
            else if (subject.ContainsKey(property))
            {
                var hasValue = !allowDuplicate && subject.ContainsKey(property) &&
                               (subject[property] is JArray exArray && exArray.Any(t => SafeEquals(t, value)) ||
                                SafeEquals(subject[property], value));
                if (!hasValue || propertyIsArray)
                {
                    subject[property] = EnsureArray(subject[property]);
                }

                if (!hasValue)
                {
                    var array = subject[property] as JArray;
                    if (prependValue)
                    {
                        array.Insert(0, value);
                    }
                    else
                    {
                        array.Add(value);
                    }
                }
            }
            else
            {
                subject[property] = propertyIsArray ? new JArray(value) : value;
            }
        }
    }
}