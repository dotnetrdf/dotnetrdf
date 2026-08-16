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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Globalization;
namespace VDS.RDF.JsonLd;

/// <summary>
/// Overrides some of the default Newtonsoft.Json JSON value formatting so that
/// the output of the JSON-LD writer is better conforming to the JSON-LD 1.1 specification.
/// </summary>
internal class JsonLiteralSerializer
{
    /// <summary>
    /// Return a string serialization of the provided token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public string Serialize(JsonNode token)
    {
        var memoryStream = new MemoryStream();
        
        using (var writer = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            Serialize(writer, token);
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    private static void Serialize(Utf8JsonWriter writer, JsonNode token)
    {
        if (token == null)
        {
            writer.WriteRawValue("null");
            return;
        }
        switch (token.GetValueKind())
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in (token as JsonObject).OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Key);
                    Serialize(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (JsonNode item in (token as JsonArray))
                {
                    Serialize(writer, item);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.Number:

                var doubleValue = token.GetValue<double>();
                switch (doubleValue)
                {
                    case double.NaN:
                        writer.WriteRawValue("NaN");
                        break;
                    case double.NegativeInfinity:
                        writer.WriteRawValue("-Infinity");
                        break;
                    case double.PositiveInfinity:
                        writer.WriteRawValue("Infinity");
                        break;
                    default:
                    {
                        var v = token.GetValue<double>().ToString("G", CultureInfo.InvariantCulture);
                        if (v.EndsWith(".0"))
                        {
                            v = v.Substring(0, v.Length - 2);
                        }
                        writer.WriteRawValue(v);
                        break;
                    }
                };
                break;
            case JsonValueKind.True:
                writer.WriteRawValue("true");
                break;
            case JsonValueKind.False:
                writer.WriteRawValue("false");
                break;
            case JsonValueKind.Null:
                writer.WriteRawValue("null");
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(token.GetValue<string>());
                break;
            default:
                writer.WriteRawValue(token.GetValue<string>());
                break;
        }
    }
}
