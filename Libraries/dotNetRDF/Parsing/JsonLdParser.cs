/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using Newtonsoft.Json;
using VDS.RDF.Parsing.Handlers;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for JSON-LD 1.0/1.1.
    /// </summary>
    public class JsonLdParser : IStoreReader
    {
        /// <inheritdoc/>
        public event StoreReaderWarning Warning;

        private readonly JsonSerializer _jsonSerializer;

        /// <summary>
        /// Get the current parser options.
        /// </summary>
        public JsonLdProcessorOptions ParserOptions { get; private set; }

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
            using (var reader = File.OpenText(filename))
            {
                Load(new StoreHandler(store), reader);
            }
        }

        /// <inheritdoc/>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (input == null) throw new ArgumentNullException(nameof(input));
            Load(new StoreHandler(store), input);
        }

        /// <inheritdoc/>
        public void Load(IRdfHandler handler, string filename)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            using (var reader = File.OpenText(filename))
            {
                Load(handler, reader);
            }
        }

        /// <inheritdoc/>
        public void Load(IRdfHandler handler, TextReader input)
        {
            handler.StartRdf();
            var rdfTypeNode = handler.CreateUriNode(new Uri(RdfNs + "type"));
            try
            {
                JToken element;
                using (var reader = new JsonTextReader(input) { DateParseHandling = DateParseHandling.None })
                {
                    element = JToken.ReadFrom(reader);
                }
                var expandedElement = JsonLdProcessor.Expand(element, ParserOptions);
                var nodeMapGenerator = new NodeMapGenerator();
                var nodeMap = nodeMapGenerator.GenerateNodeMap(expandedElement);
                foreach (var p in nodeMap.Properties())
                {
                    var graphName = p.Name;
                    var graph = p.Value as JObject;
                    if (graph == null) continue;
                    Uri graphIri;
                    if (graphName == "@default")
                    {
                        graphIri = null;
                    }
                    else
                    {
                        if (IsBlankNodeIdentifier(graphName))
                        {
                            graphIri = new Uri("nquads:bnode:" + graphName.Substring(2));
                        } 
                        else if (!(Uri.TryCreate(graphName, UriKind.Absolute, out graphIri) && graphIri.IsWellFormedOriginalString()))
                        {
                            continue;
                        }
                    }
                    foreach (var gp in graph.Properties())
                    {
                        var subject = gp.Name;
                        var node = gp.Value as JObject;
                        INode subjectNode;
                        if (IsBlankNodeIdentifier(subject))
                        {
                            subjectNode = handler.CreateBlankNode(subject.Substring(2));
                        }
                        else
                        {
                            if (!(Uri.TryCreate(subject, UriKind.Absolute, out var subjectIri) && subjectIri.IsWellFormedOriginalString())) continue;
                            subjectNode = handler.CreateUriNode(subjectIri);
                        }
                        foreach (var np in node.Properties())
                        {
                            var property = np.Name;
                            var values = np.Value as JArray;
                            if (property.Equals("@type"))
                            {
                                foreach (var type in values)
                                {
                                    var typeNode = MakeNode(handler, type, graphIri);
                                    if (typeNode != null)
                                    {
                                        handler.HandleTriple(new Triple(subjectNode, rdfTypeNode, typeNode, graphIri));
                                    }
                                }
                            }
                            else if (JsonLdUtils.IsBlankNodeIdentifier(property) && ParserOptions.ProduceGeneralizedRdf ||
                                     Uri.IsWellFormedUriString(property, UriKind.Absolute))
                            {
                                foreach (var item in values)
                                {
                                    var predicateNode = MakeNode(handler, property, graphIri);
                                    var objectNode = MakeNode(handler, item, graphIri);
                                    if (predicateNode != null && objectNode != null)
                                    {
                                        handler.HandleTriple(new Triple(subjectNode, predicateNode, objectNode,
                                            graphIri));
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

        private INode MakeNode(IRdfHandler handler, JToken token, Uri graphIri, bool allowRelativeIri = false)
        {
            if (token is JValue)
            {
                var stringValue = token.Value<string>();
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

            if (JsonLdUtils.IsValueObject(token))
            {
                string literalValue = null;
                var valueObject = token as JObject;
                var value = valueObject["@value"];
                var datatype = valueObject.Property("@type")?.Value.Value<string>();
                var language = valueObject.Property("@language")?.Value.Value<string>();
                if (datatype == "@json")
                {
                    datatype = RdfNs + "JSON";
                    var serializer = new JsonLiteralSerializer();
                    literalValue = serializer.Serialize(value);
                }
                else if (value.Type == JTokenType.Boolean)
                {
                    literalValue = value.Value<bool>() ? "true" : "false";
                    if (datatype == null) datatype = XsdNs + "boolean";
                }
                else if (value.Type == JTokenType.Float ||
                         value.Type == JTokenType.Integer && datatype != null && datatype.Equals(XsdNs + "double"))
                {
                    var doubleValue = value.Value<double>();
                    var roundedValue = Math.Round(doubleValue);
                    if (doubleValue.Equals(roundedValue) && doubleValue < 1000000000000000000000.0 && datatype == null)
                    {
                        // Integer values up to 10^21 should be rendered as an integer rather than a float
                        literalValue = roundedValue.ToString("F0");
                        // The JSON-LD test suite requires no leading minus sign when the value is 0
                        if (literalValue.Equals("-0")) literalValue = "0";
                        datatype = XsdNs + "integer";
                    }
                    else
                    {
                        literalValue = value.Value<double>().ToString("E15", CultureInfo.InvariantCulture);
                        literalValue = ExponentialFormatMatcher.Replace(literalValue, "$1E");
                        if (literalValue.EndsWith("E")) literalValue = literalValue + "0";
                        if (datatype == null) datatype = XsdNs + "double";
                    }
                }
                else if (value.Type == JTokenType.Integer ||
                         value.Type == JTokenType.Float && datatype != null && datatype.Equals(XsdNs + "integer"))
                {
                    literalValue = value.Value<long>().ToString("D", CultureInfo.InvariantCulture);
                    if (datatype == null) datatype = XsdNs + "integer";
                }
                else if (valueObject.ContainsKey("@direction") && ParserOptions.RdfDirection.HasValue)
                {
                    literalValue = value.Value<string>();
                    var direction = valueObject["@direction"].Value<string>();
                    language = valueObject.ContainsKey("@language")
                        ? valueObject["@language"].Value<string>().ToLowerInvariant()
                        : string.Empty;
                    if (ParserOptions.RdfDirection == JsonLdRdfDirectionMode.I18NDatatype)
                    {
                        datatype = "https://www.w3.org/ns/i18n#" + language + "_" + direction;
                        return handler.CreateLiteralNode(literalValue, new Uri(datatype));
                    }
                    // Otherwise direction mode is CompoundLiteral
                    var literalNode = handler.CreateBlankNode();
                    var xsdString =
                        UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
                    handler.HandleTriple(new Triple(
                        literalNode,
                        handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfValue)),
                        handler.CreateLiteralNode(literalValue, xsdString),
                        graphIri));
                    if (!string.IsNullOrEmpty(language))
                    {
                        handler.HandleTriple(new Triple(
                            literalNode,
                            handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfLanguage)),
                            handler.CreateLiteralNode(language, xsdString),
                            graphIri));
                    }

                    handler.HandleTriple(new Triple(
                        literalNode,
                        handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfDirection)),
                        handler.CreateLiteralNode(direction, xsdString),
                        graphIri));

                    return literalNode;
                }
                else
                {
                    literalValue = value.Value<string>();
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
                var listArray = token["@list"] as JArray;
                return MakeRdfList(handler, listArray, graphIri);
            }

            if((token as JObject)?.Property("@id")!=null)
            {
                // Must be a node object
                var nodeObject = (JObject) token;
                return MakeNode(handler, nodeObject["@id"], graphIri);
            }

            return null;
        }

        private INode MakeRdfList(IRdfHandler handler, JArray list, Uri graphIri)
        {
            var rdfFirst = handler.CreateUriNode(new Uri(RdfNs + "first"));
            var rdfRest = handler.CreateUriNode(new Uri(RdfNs + "rest"));
            var rdfNil = handler.CreateUriNode(new Uri(RdfNs + "nil"));
            if (list == null || list.Count == 0) return rdfNil;
            var bNodes = list.Select(x => handler.CreateBlankNode()).ToList();
            for(int ix = 0; ix < list.Count; ix++)
            {
                var subject = bNodes[ix];
                var obj = MakeNode(handler, list[ix], graphIri);
                if (obj != null)
                {
                    handler.HandleTriple(new Triple(subject, rdfFirst, obj, graphIri));
                }
                var rest = (ix + 1 < list.Count) ? bNodes[ix + 1] : (INode)rdfNil;
                handler.HandleTriple(new Triple(subject, rdfRest, rest, graphIri));
            }
            return bNodes[0];
        }

        private static bool IsBlankNodeIdentifier(string id)
        {
            return id.StartsWith("_:");
        }
    }
}