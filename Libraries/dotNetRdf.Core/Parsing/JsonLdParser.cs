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
using System.Text.Json;
using System.Text.Json.Nodes;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.JsonLd;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.JsonLd.Syntax;
using System.Collections.Generic;

namespace VDS.RDF.Parsing;

/// <summary>
/// Parser for JSON-LD 1.0/1.1.
/// </summary>
public class JsonLdParser : IStoreReader
{
    /// <inheritdoc/>
    /// <remarks>This class does not raise this event.</remarks>
#pragma warning disable CS0067
    public event StoreReaderWarning Warning; // TODO: Enable passing up of JsonLdProcessor warnings through this event
#pragma warning restore CS0067

    /// <summary>
    /// Get the current parser options.
    /// </summary>
    public JsonLdProcessorOptions ParserOptions { get; }

    /// <summary>
    /// Create an instance of the parser configured to parser JSON-LD 1.1 with no pre-defined context.
    /// </summary>
    public JsonLdParser() : this(new JsonLdProcessorOptions { ProcessingMode = JsonLdProcessingMode.JsonLd11}) { }

    /// <summary>
    /// Create an instance of the parser configured with the provided parser options.
    /// </summary>
    /// <param name="parserOptions"></param>
    public JsonLdParser(JsonLdProcessorOptions parserOptions)
    {
        ParserOptions = parserOptions;
    }

    /// <summary>
    /// Read JSON-LD from the specified file and add the RDF quads found in the JSON-LD to the specified store.
    /// </summary>
    /// <param name="store">The store to add the parsed RDF quads to.</param>
    /// <param name="filename">The path to the JSON file to be parsed.</param>
    public void Load(ITripleStore store, string filename)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (filename == null) throw new ArgumentNullException(nameof(filename));
        using var reader = File.OpenText(filename);
        Load(new StoreHandler(store), reader, store.UriFactory);
    }
    
    /// <summary>
    /// Adds the RDF quads found in the JSON-LD to the specified store.
    /// </summary>
    /// <param name="store">The store to add the parsed RDF quads to.</param>
    /// <param name="input">The expanded JSON-LD document.</param>
    internal void Load(ITripleStore store, JsonArray input)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (input == null) throw new ArgumentNullException(nameof(input));
        Load(new StoreHandler(store), input, store.UriFactory);
    }

    /// <inheritdoc/>
    public void Load(ITripleStore store, TextReader input)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (input == null) throw new ArgumentNullException(nameof(input));
        Load(new StoreHandler(store), input, store.UriFactory);
    }

    /// <inheritdoc/>
    public void Load(IRdfHandler handler, string filename)
    {
        Load(handler, filename, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, string filename, IUriFactory uriFactory)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (filename == null) throw new ArgumentNullException(nameof(filename));
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

        using var reader = File.OpenText(filename);
        Load(handler, reader, uriFactory);
    }

    /// <inheritdoc/>
    public void Load(IRdfHandler handler, TextReader input)
    {
        Load(handler, input, UriFactory.Root);
    }

    /// <inheritdoc />
    public void Load(IRdfHandler handler, TextReader input, IUriFactory uriFactory) {
        // System.Text.Json does not support parsing from a TextReader and in any case the whole DOM is needed for JSON-LD processing
        // so we read the whole input into a string and parse that into a JsonNode.
        string jsonString;
        try {
            jsonString = input.ReadToEnd();
        } finally {
            input.Close();
        }
        
        var element = JsonNode.Parse(jsonString);
        var warnings = new List<JsonLdProcessorWarning>();
        JsonArray expandedElement = JsonLdProcessor.Expand(element, ParserOptions, warnings);
        if (warnings.Any())
        {
            foreach (JsonLdProcessorWarning warning in warnings)
            {
                RaiseWarning(warning.Message);
            }
        }
        Load(handler, expandedElement, uriFactory);
    }
    
    private void Load(IRdfHandler handler, JsonArray input, IUriFactory uriFactory) {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

        handler.StartRdf();
        IUriNode rdfTypeNode = handler.CreateUriNode(uriFactory.Create(RdfNs + "type"));
        try
        {
            var nodeMapGenerator = new NodeMapGenerator();
            JsonObject nodeMap = nodeMapGenerator.GenerateNodeMap(input);
            foreach (KeyValuePair<string, JsonNode> p in nodeMap)
            {
                var graphName = p.Key;
                var graph = p.Value as JsonObject;
                if (graph == null) continue;
                IRefNode graphNode;
                if (graphName == "@default")
                {
                    graphNode = null;
                }
                else
                {
                    if (IsBlankNodeIdentifier(graphName))
                    {
                        graphNode = handler.CreateBlankNode(graphName.Substring(2));
                    } 
                    else if (Uri.TryCreate(graphName, UriKind.Absolute, out Uri graphIri) && graphIri.IsWellFormedOriginalString())
                    {
                        graphNode = handler.CreateUriNode(graphIri);
                    }
                    else
                    {
                        continue;
                    }
                }
                foreach (KeyValuePair<string, JsonNode> gp in graph)
                {
                    var subject = gp.Key;
                    var node = gp.Value as JsonObject;
                    IRefNode subjectNode;
                    if (IsBlankNodeIdentifier(subject))
                    {
                        subjectNode = handler.CreateBlankNode(subject.Substring(2));
                    }
                    else
                    {
                        if (!(Uri.TryCreate(subject, UriKind.Absolute, out Uri subjectIri) &&
                              subjectIri.IsWellFormedOriginalString()))
                        {
                            RaiseWarning(
                                $"Unable to generate a well-formed absolute IRI for subject `{subjectIri}`. This subject will be ignored.");
                            continue;
                        }
                        subjectNode = handler.CreateUriNode(subjectIri);
                    }
                    foreach (KeyValuePair<string, JsonNode> np in node)
                    {
                        var property = np.Key;
                        var values = np.Value as JsonArray;
                        if (property.Equals("@type"))
                        {
                            foreach (JsonNode type in values)
                            {
                                INode typeNode = MakeNode(handler, type, graphNode);
                                if (typeNode is null)
                                {
                                    if (ParserOptions.SafeMode)
                                    {
                                        RaiseWarning(
                                            $"Unable to generate a well-formed absolute IRI for type `{type}`. This type will be ignored.");
                                    }
                                    continue;
                                }
                                handler.HandleQuad(new Triple(subjectNode, rdfTypeNode, typeNode), graphNode);
                            }
                        }
                        else if ((JsonLdUtils.IsBlankNodeIdentifier(property) && ParserOptions.ProduceGeneralizedRdf) ||
                                 Uri.IsWellFormedUriString(property, UriKind.Absolute))
                        {
                            foreach (JsonNode item in values)
                            {
                                var predicateNode = MakeNode(handler, property, graphNode) as IRefNode;
                                if (ParserOptions.SafeMode && predicateNode is null)
                                {
                                    RaiseWarning(
                                        $"Unable to generate a predicate node for property `{property}`. This property will be ignored.");
                                    continue;
                                }
                                INode objectNode = MakeNode(handler, item, graphNode);
                                if (ParserOptions.SafeMode && objectNode is null)
                                {
                                    RaiseWarning(
                                        $"Unable to generate an object node for value `{item}` of property `{property}`. This value will be ignored.");
                                    continue;
                                }
                                if (predicateNode != null && objectNode != null)
                                {
                                    handler.HandleQuad(new Triple(subjectNode, predicateNode, objectNode), graphNode);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            handler.EndRdf(false);
            throw;
        }
        handler.EndRdf(true);
    }
    
    private const string XsdNs = "http://www.w3.org/2001/XMLSchema#";
    private const string RdfNs = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string RdfValue = RdfNs + "value";
    private static readonly Regex ExponentialFormatMatcher = new Regex(@"(\d)0*E\+?0*");

    private static bool IsInteger(JsonNode node)
    {
        if (node is JsonValue valueNode && valueNode.GetValueKind() == JsonValueKind.Number)
        {
            var doubleValue = valueNode.GetValue<double>();
            var roundedValue = Math.Round(doubleValue);
            return doubleValue.Equals(roundedValue) && doubleValue < 1e21;
        }
        return false;
    }
    private INode MakeNode(IRdfHandler handler, JsonNode token, IRefNode graphName, bool allowRelativeIri = false)
    {
        if (token is JsonValue)
        {
            var stringValue = token.GetValue<string>();
            if (JsonLdUtils.IsBlankNodeIdentifier(stringValue))
            {
                return handler.CreateBlankNode(stringValue.Substring(2));
            }
            if (Uri.TryCreate(stringValue, allowRelativeIri ? UriKind.RelativeOrAbsolute : UriKind.Absolute, out Uri iri))
            {
                if (!Uri.IsWellFormedUriString(stringValue, allowRelativeIri ? UriKind.RelativeOrAbsolute : UriKind.Absolute)) return null;
                return handler.CreateUriNode(iri);
            }
            return null;
        }

        if (JsonLdUtils.IsValueObject(token) && token is JsonObject valueObject)
        {
            string literalValue;
            JsonNode value = valueObject["@value"];
            var datatype = valueObject.ContainsKey("@type") ? valueObject["@type"].GetValue<string>() : null;
            var language = valueObject.ContainsKey("@language") ? valueObject["@language"].GetValue<string>() : null;
            if (datatype == "@json")
            {
                datatype = RdfNs + "JSON";
                var serializer = new JsonLiteralSerializer();
                // The "unsafe" JSON Encoder escapes more characters than necessary for JSON-LD, so we need to unescape some of them to conform to the JSON-LD 1.1 specification.
                literalValue = Regex.Replace(serializer.Serialize(value), @"\\u([0-9a-fA-F]{4})", match =>
                {
                    var unicodeChar = (char)Convert.ToInt32(match.Groups[1].Value, 16);
                    return 0x00 <= unicodeChar && unicodeChar <= 0x1F ? match.Value : unicodeChar.ToString();
                });
            }
            else if (value.GetValueKind() == JsonValueKind.True || value.GetValueKind() == JsonValueKind.False)
            {
                literalValue = value.GetValue<bool>() ? "true" : "false";
                datatype ??= XsdNs + "boolean";
            }
            else if (
                value.GetValueKind() == JsonValueKind.Number &&
                (!IsInteger(value) ||
                     (IsInteger(value) && datatype != null && datatype.Equals(XsdNs + "double"))))
            {
                var doubleValue = value.GetValue<double>();
                var roundedValue = Math.Round(doubleValue);
                if (doubleValue.Equals(roundedValue) && doubleValue < 1e21 && datatype == null)
                {
                    // Integer values up to 10^21 should be rendered as an integer rather than a float
                    literalValue = roundedValue.ToString("F0");
                    // The JSON-LD test suite requires no leading minus sign when the value is 0
                    if (literalValue.Equals("-0")) literalValue = "0";
                    datatype = XsdNs + "integer";
                }
                else
                {
                    literalValue = value.GetValue<double>().ToString("E15", CultureInfo.InvariantCulture);
                    literalValue = ExponentialFormatMatcher.Replace(literalValue, "$1E");
                    if (literalValue.EndsWith("E")) literalValue = literalValue + "0";
                    datatype ??= XsdNs + "double";
                }
            }
            else if (value.GetValueKind() == JsonValueKind.Number &&
                (IsInteger(value) ||
                     (!IsInteger(value) && datatype != null && datatype.Equals(XsdNs + "integer"))))
            {
                var decimalValue = value.GetValue<decimal>();
                literalValue = decimalValue.ToString("F0", CultureInfo.InvariantCulture);
                datatype ??= XsdNs + "integer";
            }
            else if (valueObject.ContainsKey("@direction") && ParserOptions.RdfDirection.HasValue)
            {
                literalValue = value.GetValue<string>();
                var direction = valueObject["@direction"].GetValue<string>();
                language = valueObject.ContainsKey("@language")
                    ? valueObject["@language"].GetValue<string>().ToLowerInvariant()
                    : string.Empty;
                if (ParserOptions.RdfDirection == JsonLdRdfDirectionMode.I18NDatatype)
                {
                    datatype = "https://www.w3.org/ns/i18n#" + language + "_" + direction;
                    return handler.CreateLiteralNode(literalValue, new Uri(datatype));
                }
                // Otherwise direction mode is CompoundLiteral
                IBlankNode literalNode = handler.CreateBlankNode();
                Uri xsdString =
                    UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
                handler.HandleQuad(new Triple(
                    literalNode,
                    handler.CreateUriNode(UriFactory.Root.Create(RdfSpecsHelper.RdfValue)),
                    handler.CreateLiteralNode(literalValue, xsdString)),
                    graphName);
                if (!string.IsNullOrEmpty(language))
                {
                    handler.HandleQuad(new Triple(
                        literalNode,
                        handler.CreateUriNode(UriFactory.Root.Create(RdfSpecsHelper.RdfLanguage)),
                        handler.CreateLiteralNode(language, xsdString)),
                        graphName);
                }

                handler.HandleQuad(new Triple(
                    literalNode,
                    handler.CreateUriNode(UriFactory.Root.Create(RdfSpecsHelper.RdfDirection)),
                    handler.CreateLiteralNode(direction, xsdString)),
                    graphName);

                return literalNode;
            }
            else
            {
                literalValue = value.GetValue<string>();
                if (datatype == null && language == null)
                {
                    datatype = XsdNs + "string";
                }
            }

            if (language != null)
            {
                return LanguageTag.IsWellFormed(language) ? handler.CreateLiteralNode(literalValue, language) : null;
            } 
            return handler.CreateLiteralNode(literalValue, new Uri(datatype));
        }
        if (JsonLdUtils.IsListObject(token))
        {
            var listArray = token["@list"] as JsonArray;
            return MakeRdfList(handler, listArray, graphName);
        }

        if((token as JsonObject)?.TryGetPropertyValue("@id", out var idProperty) == true)
        {
            return MakeNode(handler, idProperty, graphName);
        }

        return null;
    }

    private INode MakeRdfList(IRdfHandler handler, JsonArray list, IRefNode graphName)
    {
        IUriNode rdfFirst = handler.CreateUriNode(new Uri(RdfNs + "first"));
        IUriNode rdfRest = handler.CreateUriNode(new Uri(RdfNs + "rest"));
        IUriNode rdfNil = handler.CreateUriNode(new Uri(RdfNs + "nil"));
        if (list == null || list.Count == 0) return rdfNil;
        var bNodes = list.Select(x => handler.CreateBlankNode()).ToList();
        for(var ix = 0; ix < list.Count; ix++)
        {
            IBlankNode subject = bNodes[ix];
            INode obj = MakeNode(handler, list[ix], graphName);
            if (obj != null)
            {
                handler.HandleQuad(new Triple(subject, rdfFirst, obj), graphName);
            }
            INode rest = (ix + 1 < list.Count) ? bNodes[ix + 1] : (INode)rdfNil;
            handler.HandleQuad(new Triple(subject, rdfRest, rest), graphName);
        }
        return bNodes[0];
    }

    private static bool IsBlankNodeIdentifier(string id)
    {
        return id.StartsWith("_:");
    }

    private void RaiseWarning(string message)
    {
        Warning?.Invoke(message);
    }
}