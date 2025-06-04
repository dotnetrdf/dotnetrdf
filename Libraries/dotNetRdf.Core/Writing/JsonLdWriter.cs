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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.JsonLd.Syntax;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing;

/// <summary>
/// Class for serializing a Triple Store in JSON-LD syntax.
/// </summary>
public class JsonLdWriter : BaseStoreWriter
{
    private readonly JsonLdWriterOptions _options;

    /// <summary>
    /// Create a new serializer with default serialization options.
    /// </summary>
    public JsonLdWriter()
    {
        _options = new JsonLdWriterOptions();
    }


    /// <summary>
    /// Create a new serializer with the specified serialization options.
    /// </summary>
    /// <param name="options"></param>
    public JsonLdWriter(JsonLdWriterOptions options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public override void Save(ITripleStore store, TextWriter output, bool leaveOpen)
    {
        if (store == null) throw new ArgumentNullException(nameof(store), "Cannot write a null store");
        if(output == null) throw new ArgumentNullException(nameof(output), "Cannot write to a null writer");
        JArray jsonArray = SerializeStore(store);
        output.Write(jsonArray.ToString(_options.JsonFormatting));
        output.Flush();
        if (!leaveOpen)
        {
            output.Close();
        }
    }

    /// <summary>
    /// Serialize a Triple Store to an expanded JSON-LD document.
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    public JArray SerializeStore(ITripleStore store)
    {
        // 1 - Initialize default graph to an empty dictionary.
        var defaultGraph = new JObject();
        // 2 - Initialize graph map to a dictionary consisting of a single member @default whose value references default graph.
        var graphMap = new JObject(new JProperty("@default", defaultGraph));
        // 3 - Initialize referenced once to an empty map.
        var referencedOnce =new Dictionary<string, Usage>();
        // 4 - Initialize compound literal subjects to an empty map.
        var compoundLiteralSubjects = new JObject();
        // 5 - For each graph in RDF dataset:
        foreach (IGraph graph in store.Graphs)
        {
            // 5.1 - If graph is the default graph, set name to @default, otherwise to the graph name associated with graph.
            var name = graph.Name == null ? "@default" : graph.Name.ToString();

            // 5.2 - If graph map has no name entry, create one and set its value to an empty map.
            if (!graphMap.ContainsKey(name))
            {
                graphMap.Add(name, new JObject());
            }

            // 5.3 - If compound literal subjects has no name entry, create one and set its value to an empty map.
            if (!compoundLiteralSubjects.ContainsKey(name))
            {
                compoundLiteralSubjects[name] = new JObject();
            }

            // 5.4 - If graph is not the default graph and default graph does not have a name entry,
            // create such an entry and initialize its value to a new map with a single entry @id whose value is name.
            if (name != "@default")
            {
                if (!defaultGraph.ContainsKey(name))
                {
                    defaultGraph.Add(name, new JObjectWithUsages(new JProperty("@id", name)));
                }
            }

            // 5.5 - Reference the value of the name entry in graph map using the variable node map.
            var nodeMap = graphMap[name] as JObject;

            // 5.6 - Reference the value of the name entry in compound literal subjects using the variable compound map.
            JToken compoundMap = compoundLiteralSubjects[name];

            // 5.7 - For each triple in graph consisting of subject, predicate, and object:
            foreach (Triple triple in graph.Triples)
            {
                var subject = MakeNodeString(triple.Subject);
                var predicate = MakeNodeString(triple.Predicate);
                var @object = triple.Object is IUriNode || triple.Object is IBlankNode ? MakeNodeString(triple.Object) : null;

                // 5.7.1 - If node map does not have a subject entry, create one and initialize its value to a new map consisting of a single entry @id whose value is set to subject.
                if (!nodeMap.ContainsKey(subject))
                {
                    nodeMap.Add(subject, new JObjectWithUsages(new JProperty("@id", subject)));
                }

                // 5.7.2 - Reference the value of the subject entry in node map using the variable node.
                var node = nodeMap[subject] as JObjectWithUsages;

                // 5.7.3 - If the rdfDirection option is compound-literal and predicate is rdf:direction, add an entry in compound map for subject with the value true.
                if (_options.RdfDirection.HasValue && _options.RdfDirection == JsonLdRdfDirectionMode.CompoundLiteral && RdfSpecsHelper.RdfDirection.Equals(predicate))
                {
                    compoundMap[subject] = true;
                }

                // 5.7.4 - If object is an IRI or blank node identifier, and node map does not have an object entry,
                // create one and initialize its value to a new map consisting of a single entry @id whose value is set
                // to object.
                if (triple.Object is IUriNode || triple.Object is IBlankNode)
                {
                    if (!nodeMap.ContainsKey(@object))
                    {
                        nodeMap.Add(@object, new JObjectWithUsages(new JProperty("@id", @object)));
                    }
                }

                // 5.7.5 - If predicate equals rdf:type, the useRdfType flag is not true, and object is an IRI or blank node identifier
                if (predicate.Equals(RdfSpecsHelper.RdfType) && !_options.UseRdfType &&
                    (triple.Object is IUriNode || triple.Object is IBlankNode))
                {
                    // Append object to the value of the @type entry of node; unless such an item already exists. 
                    if (node.ContainsKey("@type"))
                    {
                        AppendUniqueElement(@object, node["@type"] as JArray);
                    }
                    else
                    {
                        // If no such entry exists, create one and initialize it to an array whose only item is object.
                        node.Add("@type", new JArray(@object));
                    }

                    // Finally, continue to the next triple.
                    continue;
                }

                // 5.7.6 - Initialize value to the result of using the RDF to Object Conversion algorithm, passing object, rdfDirection, and useNativeTypes.
                JToken value = RdfToObject(triple.Object);

                // 5.7.7 -If node does not have a predicate entry, create one and initialize its value to an empty array.
                if (!node.ContainsKey(predicate))
                {
                    node[predicate] = new JArray();
                }

                // 5.7.8 - If there is no item equivalent to value in the array associated with the predicate entry of node, append a reference to value to the array. Two maps are considered equal if they have equivalent map entries.
                AppendUniqueElement(value, node[predicate] as JArray);

                // 5.7.9 - If object is rdf:nil, it represents the termination of an RDF collection:
                if (triple.Object is IUriNode u && u.Uri.ToString().Equals(RdfSpecsHelper.RdfListNil))
                {
                    // 5.7.9.1 - Reference the usages entry of the object entry of node map using the variable usages.
                    // 5.7.9.2 - Append a new map consisting of three entries, node, property, and value to the usages array. The node entry is set to a reference to node, property to predicate, and value to a reference to value.
                    var objectMap = nodeMap[@object] as JObjectWithUsages;
                    objectMap.Usages.Add(new Usage(node, predicate, value));
                }
                else if (@object != null && referencedOnce.ContainsKey(@object))
                {
                    // 5.7.10 - Otherwise, if referenced once has an entry for object, set the object entry of referenced once to false.
                    referencedOnce[@object] = null;
                }
                else if (triple.Object is IBlankNode)
                {
                    // 5.7.11 - Otherwise, if object is a blank node identifier, it might represent a list node:
                    // 5.7.11.1 - Set the object entry of referenced once to a new map consisting of three entries, node, property, and value to the usages array.
                    // The node entry is set to a reference to node, property to predicate, and value to a reference to value.
                    referencedOnce[@object] = new Usage(node, predicate, value);
                }
            }
        }

        // 6 - For each name and graph object in graph map:
        foreach (KeyValuePair<string, JToken> gp in graphMap)
        {
            var name = gp.Key;
            var graphObject = gp.Value as JObject;

            // 6.1 - If compound literal subjects has an entry for name, then for each cl which is a key in that entry:
            if (compoundLiteralSubjects.ContainsKey(name))
            {
                if (compoundLiteralSubjects[name] is JObject compoundMap)
                {
                    foreach (JProperty clProp in compoundMap.Properties())
                    {
                        var cl = clProp.Name;
                        // 6.1.1 - Initialize cl entry to the value of cl in referenced once, continuing to the next cl if cl entry is not a map.
                        Usage clEntry = referencedOnce[cl];
                        if (clEntry == null) continue;
                        // 6.1.2 - Initialize node to the value of node in cl entry.
                        // 6.1.3 - Initialize property to value of property in cl entry.
                        // 6.1.4 - Initialize value to value of value in cl entry.
                        JObjectWithUsages node = clEntry.Node;
                        var property = clEntry.Property;
                        JToken value = clEntry.Value;
                        // 6.1.5 - Initialize cl node to the value of cl in graph object, and remove that entry from graph object, continuing to the next cl if cl node is not a map.
                        var clNode = graphObject[cl] as JObject;
                        graphObject.Remove(cl);
                        if (clNode == null) continue;
                        // 6.1.6 - For each cl reference in the value of property in node where the value of @id in cl reference is cl:
                        foreach (JObject clReference in node[property].OfType<JObject>()
                            .Where(n => cl.Equals(n["@id"].Value<string>())))
                        {
                            // 6.1.6.1 - Delete the @id entry in cl reference.
                            clReference.Remove("@id");
                            // 6.1.6.2 - Add an entry to cl reference for @value with the value taken from the rdf:value entry in cl node.
                            clReference["@value"] = clNode[RdfSpecsHelper.RdfValue][0]["@value"];
                            // 6.1.6.3 - Add an entry to cl reference for @language with the value taken from the rdf:language entry in cl node, if any.
                            // If that value is not well-formed according to section 2.2.9 of [BCP47], an invalid language-tagged string error has been detected and processing is aborted.
                            if (clNode.ContainsKey(RdfSpecsHelper.RdfLanguage))
                            {
                                var language = clNode[RdfSpecsHelper.RdfLanguage][0]["@value"].Value<string>();
                                if (!LanguageTag.IsWellFormed(language))
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidLanguageTaggedString,
                                        $"Invalid Language-tagged string. Encountered a language tag ({language}) that is not well-formed according to BCP-47.");
                                }

                                clReference["@language"] = language;
                            }

                            // 6.1.6.4 - Add an entry to cl reference for @direction with the value taken from the rdf:direction entry in cl node, if any.
                            // If that value is not "ltr" or "rtl", an invalid base direction error has been detected and processing is aborted.
                            if (clNode.ContainsKey(RdfSpecsHelper.RdfDirection))
                            {
                                var direction = clNode[RdfSpecsHelper.RdfDirection][0]["@value"].Value<string>();
                                if (!("ltr".Equals(direction) || "rtl".Equals(direction)))
                                {
                                    throw new JsonLdProcessorException(JsonLdErrorCode.InvalidBaseDirection,
                                        $"Invalid base direction. Encountered a value for rdf:direction ({direction}) that is not allowed. Allowed values are 'rtl' or 'ltr'.");
                                }

                                clReference["@direction"] = direction;
                            }
                        }
                    }
                }
            }

            // 6.2 - If graph object has no rdf:nil entry, continue with the next name-graph object pair as the graph does not contain any lists that need to be converted.
            if (!graphObject.ContainsKey(RdfSpecsHelper.RdfListNil))
            {
                continue;
            }

            // 6.3 - Initialize nil to the value of the rdf:nil member of graph object.
            var nil = graphObject[RdfSpecsHelper.RdfListNil] as JObjectWithUsages;

            // 6.4 - For each item usage in the usages member of nil, perform the following steps:

            foreach (Usage usage in nil.Usages)
            {
                // 6.4.1 - Initialize node to the value of the value of the node entry of usage,
                // property to the value of the property entry of usage,
                // and head to the value of the value entry of usage.
                JObjectWithUsages node = usage.Node;
                var property = usage.Property;
                var head = usage.Value as JObject;
                // 6.4.2 - Initialize two empty arrays list and list nodes.
                var list = new JArray();
                var listNodes = new JArray();
                // 6.4.3 - While property equals rdf:rest, the value of the @id entry of node is a blank node identifier,
                // the value of the entry of referenced once associated with the @id entry of node is a map,
                // node has rdf:first and rdf:rest entries, both of which have as value an array consisting of a single element,
                // and node has no other entries apart from an optional @type entry whose value is an array with a single item equal to rdf:List,
                // node represents a well-formed list node.
                // Perform the following steps to traverse the list backwards towards its head:
                while (IsWellFormedListNode(node, property, referencedOnce))
                {
                    // 6.4.3.1 - Append the only item of rdf:first member of node to the list array.
                    list.Add((node[RdfSpecsHelper.RdfListFirst] as JArray)[0]);
                    // 6.4.3.2 - Append the value of the @id member of node to the list nodes array.
                    listNodes.Add(node["@id"]);
                    // 6.4.3.3 - Initialize node usage to the value of the entry of referenced once associated with the @id entry of node.
                    Usage nodeUsage = referencedOnce[node["@id"].Value<string>()];

                    // 6.4.3.4 - Set node to the value of the node entry of node usage,
                    // property to the value of the property entry of node usage,
                    // and head to the value of the value entry of node usage.
                    node = nodeUsage.Node;
                    property = nodeUsage.Property;
                    head = nodeUsage.Value as JObject;
                    // 6.4.3.5 - If the @id entry of node is an IRI instead of a blank node identifier, exit the while loop.
                    if (!JsonLdUtils.IsBlankNodeIdentifier(node["@id"].Value<string>())) break;
                }

                // 6.4.4 - Remove the @id entry from head.
                head.Remove("@id");
                // 6.4.5 - Reverse the order of the list array.
                list = new JArray(list.Reverse());
                // 6.4.6 - Add an @list entry to head and initialize its value to the list array.
                head["@list"] = list;
                // 6.5.7 - For each item node id in list nodes, remove the node id entry from graph object.
                foreach (var nodeId in listNodes.Select(item => item.Value<string>()))
                {
                    graphObject.Remove(nodeId);
                }
            }

        }

        // 7 - Initialize an empty array result.
        var result = new JArray();
        // 8 - For each subject and node in default graph ordered lexicographically by subject if ordered is true:
        IEnumerable<JProperty> defaultGraphProperties = defaultGraph.Properties();
        if (_options.Ordered) defaultGraphProperties = defaultGraphProperties.OrderBy(x => x.Name, StringComparer.Ordinal);
        foreach (JProperty defaultGraphProperty in defaultGraphProperties)
        {
            var subject = defaultGraphProperty.Name;
            var node = defaultGraphProperty.Value as JObject;
            // 8.1 - If graph map has a subject member:
            if (graphMap.ContainsKey(subject))
            {
                // 8.1.1 - Add an @graph member to node and initialize its value to an empty array.
                var graphArray = new JArray();
                node["@graph"] = graphArray;
                // 8.1.2 - For each key-value pair s-n in the subject entry of graph map ordered lexicographically by s if ordered is true,
                // append n to the @graph entry of node after removing its usages entry, unless the only remaining entry of n is @id.
                IEnumerable<JProperty> subjectMapProperties = (graphMap[subject] as JObject).Properties();
                if (_options.Ordered)
                {
                    subjectMapProperties = subjectMapProperties.OrderBy(x => x.Name, StringComparer.Ordinal);
                }
                foreach (JProperty subjectMapProperty in subjectMapProperties)
                {
                    var s = subjectMapProperty.Name;
                    var n = subjectMapProperty.Value as JObject;
                    n.Remove("usages");
                    if (n.Properties().Any(np => !np.Name.Equals("@id")))
                    {
                        graphArray.Add(n);
                    }
                }
            }
            // 8.2 - Append node to result after removing its usages member, unless the only remaining member of node is @id.
            node.Remove("usages");
            if (node.Properties().Any(p => !p.Name.Equals("@id")))
            {
                result.Add(node);
            }
        }
        // 9 - Return result.
        return result;
    }

    private static bool IsWellFormedListNode(JObject node, string property, Dictionary<string, Usage> nodeUsagesMap)
    {
        // If property equals rdf:rest, the value of the @id entry of node is a blank node identifier,
        // the value of the entry of referenced once associated with the @id entry of node is a map,
        // node has rdf: first and rdf: rest entries, both of which have as value an array consisting of a single element,
        // and node has no other entries apart from an optional @type entry whose value is an array with a single item equal to rdf: List,
        // node represents a well-formed list node. 
        if (!RdfSpecsHelper.RdfListRest.Equals(property)) return false;
        var nodeId = node["@id"].Value<string>();
        if (nodeId == null || !JsonLdUtils.IsBlankNodeIdentifier(nodeId)) return false;
        if (!nodeUsagesMap.TryGetValue(nodeId, out Usage usage)) return false;
        if (usage == null) return false;

        var first = node[RdfSpecsHelper.RdfListFirst] as JArray;
        var rest = node[RdfSpecsHelper.RdfListRest] as JArray;
        if (first == null || rest == null) return false;
        if (first.Count != 1 || rest.Count != 1) return false;

        var type = node["@type"] as JArray;
        if (type != null && (type.Count != 1 ||
                             type.Count == 1 && !type[0].Value<string>().Equals(RdfSpecsHelper.RdfList)))
            return false;
        var propCount = node.Properties().Count();
        if (type == null && propCount != 3 || type != null && propCount != 4) return false;
        return true;
    }

    private JToken RdfToObject(INode value)
    {
        switch (value)
        {
            // 1 - If value is an IRI or a blank node identifier, return a new dictionary consisting of a single member @id whose value is set to value.
            case IUriNode uriNode:
                return new JObject(new JProperty("@id", uriNode.Uri.OriginalString));
            case IBlankNode bNode:
                return new JObject(new JProperty("@id", "_:" + bNode.InternalID));
            case ILiteralNode literal:
                // 2 - Otherwise value is an RDF literal:
                // 2.1 - Initialize a new empty dictionary result.
                var result = new JObject();
                // 2.2 - Initialize converted value to value.
                JToken convertedValue = new JValue(literal.Value);
                // 2.3 - Initialize type to null
                string type = null;
                // 2.4 - If use native types is true
                if (_options.UseNativeTypes && literal.DataType != null)
                {
                    // 2.4.1 - If the datatype IRI of value equals xsd:string, set converted value to the lexical form of value.
                    if (literal.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        convertedValue = new JValue(literal.Value);
                    }
                    // 2.4.2 - Otherwise, if the datatype IRI of value equals xsd:boolean, set converted value to true if the lexical form of value matches true, or false if it matches false. If it matches neither, set type to xsd:boolean.
                    else if (literal.DataType.ToString()
                                 .Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                    {
                        if (literal.Value.Equals("true"))
                        {
                            convertedValue = new JValue(true);
                        }
                        else if (literal.Value.Equals("false"))
                        {
                            convertedValue = new JValue(false);
                        }
                        else
                        {
                            type = XmlSpecsHelper.XmlSchemaDataTypeBoolean;
                        }
                    }
                    // 2.4.3 - Otherwise, if the datatype IRI of value equals xsd:integer or xsd:double and its lexical form is a valid xsd:integer or xsd:double according [XMLSCHEMA11-2], set converted value to the result of converting the lexical form to a JSON number.
                    else if (literal.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger))
                    {
                        if (IsWellFormedInteger(literal.Value))
                        {
                            convertedValue = new JValue(long.Parse(literal.Value));
                        }
                    }
                    else if (literal.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
                    {
                        if (IsWellFormedDouble(literal.Value))
                        {
                            convertedValue = new JValue(double.Parse(literal.Value));
                        }
                    }
                    // KA: Step missing from spec - otherwise set type to the datatype IRI
                    else
                    {
                        type = literal.DataType.ToString();
                    }
                }
                // 2.5 - Otherwise, if processing mode is not json-ld-1.0, and value is a JSON literal, set converted value to the result of turning the lexical value of value into the JSON-LD internal representation, and set type to @json. If the lexical value of value is not valid JSON according to the JSON Grammar [RFC8259], an invalid JSON literal error has been detected and processing is aborted.
                else if (_options.ProcessingMode != JsonLdProcessingMode.JsonLd10 &&
                         RdfSpecsHelper.RdfJson.Equals(literal.DataType?.ToString()))
                {
                    try
                    {
                        convertedValue = JToken.Parse(literal.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonLdProcessorException(JsonLdErrorCode.InvalidJsonLiteral,
                            "Invalid JSON literal. " + ex.Message);
                    }

                    type = "@json";
                }
                // 2.6 - Otherwise, if the datatype IRI of value starts with https://www.w3.org/ns/i18n#, and rdfDirection is i18n-datatype:
                else if (_options.RdfDirection == JsonLdRdfDirectionMode.I18NDatatype && literal.DataType != null &&
                         literal.DataType.ToString().StartsWith("https://www.w3.org/ns/i18n#"))
                {
                    var fragment = literal.DataType.Fragment.TrimStart('#');
                    if (!string.IsNullOrEmpty(literal.DataType.Fragment) && fragment.Contains("_"))
                    {
                        convertedValue = literal.Value;
                        var sepIx = fragment.IndexOf("_", StringComparison.Ordinal);
                        if (sepIx > 0)
                        {
                            result["@language"] = fragment.Substring(0, sepIx);
                        }
                        result["@direction"] = fragment.Substring(sepIx + 1);
                    }
                }
                // 2.7 - Otherwise, if value is a language-tagged string add a member @language to result and set its value to the language tag of value.
                else if (!string.IsNullOrEmpty(literal.Language))
                {
                    result["@language"] = literal.Language;
                }
                // 2.8 - Otherwise, set type to the datatype IRI of value, unless it equals xsd:string which is ignored.
                else
                {
                    if (literal.DataType != null && !literal.DataType.ToString()
                            .Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        type = literal.DataType.ToString();
                    }
                }
                // 2.9 - Add a member @value to result whose value is set to converted value.
                result["@value"] = convertedValue;
                // 2.10 - If type is not null, add a member @type to result whose value is set to type.
                if (type != null) result["@type"] = type;
                // 2.11 - Return result.
                return result;
        }

        return null;
    }

    private static readonly Regex IntegerLexicalRepresentation = new Regex(@"^(\+|\-)?\d+$");
    private static bool IsWellFormedInteger(string literal)
    {
        return IntegerLexicalRepresentation.IsMatch(literal);
    }

    private static readonly Regex DoubleLexicalRepresentation = new Regex(@"^((\+|-)?([0-9]+(\.[0-9]*)?|\.[0-9]+)([Ee](\+|-)?[0-9]+)?|(\+|-)?INF|NaN)$");

    private static bool IsWellFormedDouble(string literal)
    {
        return DoubleLexicalRepresentation.IsMatch(literal);
    }

    private static void AppendUniqueElement(JToken element, JArray toArray)
    {
        if (!toArray.Any(x => JToken.DeepEquals(x, element)))
        {
            toArray.Add(element);
        }
    }

    private static string MakeNodeString(INode node)
    {
        return node switch
        {
            IUriNode uriNode => uriNode.Uri.OriginalString,
            IBlankNode blankNode => "_:" + blankNode.InternalID,
            _ => throw new ArgumentException("Node must be a blank node or URI node", nameof(node))
        };
    }

    /// <inheritdoc/>
    /// <remarks>This class does not raise this event.</remarks>
#pragma warning disable CS0067
    public override event StoreWriterWarning Warning;
#pragma warning restore CS0067

    private class JObjectWithUsages : JObject
    {
        public readonly List<Usage> Usages = new List<Usage>();
        public JObjectWithUsages(params object[] content) : base(content) { }
    }

    private class Usage
    {
        public Usage(JObjectWithUsages node, string property, JToken value)
        {
            Node = node;
            Property = property;
            Value = value;
        }
        public JObjectWithUsages Node { get; }
        public string Property { get; }
        public JToken Value { get; }
    }
}
