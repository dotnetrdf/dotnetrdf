/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serializing a Triple Store in JSON-LD syntax
    /// </summary>
    public partial class JsonLdWriter : IStoreWriter
    {
        private readonly JsonLdWriterOptions _options;

        /// <summary>
        /// Create a new serializer with default serialization options
        /// </summary>
        public JsonLdWriter()
        {
            _options = new JsonLdWriterOptions();
        }


        /// <summary>
        /// Create a new serializer with the specified serialization options
        /// </summary>
        /// <param name="options"></param>
        public JsonLdWriter(JsonLdWriterOptions options)
        {
            _options = options;
        }

        /// <inheritdoc/>
        public void Save(ITripleStore store, string filename)
        {
            var jsonArray = SerializeStore(store);
            using (var writer = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write),
                Encoding.UTF8))
            {
                writer.Write(jsonArray);
            }
        }

        /// <inheritdoc/>
        public void Save(ITripleStore store, TextWriter output)
        {
            Save(store, output, false);
        }

        /// <inheritdoc/>
        public void Save(ITripleStore store, TextWriter output, bool leaveOpen)
        {
            var jsonArray = SerializeStore(store);
            output.Write(jsonArray.ToString(_options.JsonFormatting));
            output.Flush();
            if (!leaveOpen)
            {
                output.Close();
            }
        }

        /// <summary>
        /// Serialize a Triple Store to an expanded JSON-LD document
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public JArray SerializeStore(ITripleStore store)
        {
            // 1 - Initialize default graph to an empty dictionary.
            var defaultGraph = new Dictionary<string, JObjectWithUsages>();
            // 2 - Initialize graph map to a dictionary consisting of a single member @default whose value references default graph.
            var graphMap = new Dictionary<string, Dictionary<string, JObjectWithUsages>>{{"@default", defaultGraph}};
            // 3 - Initialize node usages map to an empty dictionary.
            var nodeUsagesMap = new Dictionary<string, JArray>();
            // 4 - For each graph in RDF dataset:
            foreach (var graph in store.Graphs)
            {
                // 4.1 - If graph is the default graph, set name to @default, otherwise to the graph name associated with graph.
                string name = graph.BaseUri == null ? "@default" : graph.BaseUri.ToString();

                // 4.2 - If graph map has no name member, create one and set its value to an empty dictionary.
                if (!graphMap.ContainsKey(name))
                {
                    graphMap.Add(name, new Dictionary<string, JObjectWithUsages>());
                }

                // 4.3 - If graph is not the default graph and default graph does not have a name member, create such a member and initialize its value to a new dictionary with a single member @id whose value is name.
                if (name != "@default")
                {
                    if (!defaultGraph.ContainsKey(name))
                    {
                        defaultGraph.Add(name, new JObjectWithUsages(new JProperty("@id", name)));
                    }
                }

                // 4.4 - Reference the value of the name member in graph map using the variable node map.
                var nodeMap = graphMap[name];

                // 4.5 - For each RDF triple in graph consisting of subject, predicate, and object:
                foreach (var triple in graph.Triples)
                {
                    var subject = MakeNodeString(triple.Subject);
                    var predicate = MakeNodeString(triple.Predicate);

                    // 4.5.1 - If node map does not have a subject member, create one and initialize its value to a new dictionary consisting of a single member @id whose value is set to subject.
                    if (!nodeMap.ContainsKey(subject))
                    {
                        nodeMap.Add(subject, new JObjectWithUsages(new JProperty("@id", subject)));
                    }

                    // 4.5.2 - Reference the value of the subject member in node map using the variable node.
                    var node = nodeMap[subject];

                    // 4.5.3 - If object is an IRI or blank node identifier, and node map does not have an object member, 
                    // create one and initialize its value to a new dictionary consisting of a single member @id whose value is set to object.
                    if (triple.Object is IUriNode || triple.Object is IBlankNode)
                    {
                        var obj = MakeNodeString(triple.Object);
                        if (!nodeMap.ContainsKey(obj))
                        {
                            nodeMap.Add(obj, new JObjectWithUsages(new JProperty("@id", obj)));
                        }
                    }

                    // 4.5.4 - If predicate equals rdf:type, the use rdf:type flag is not true, and object is an IRI or blank node identifier,
                    // append object to the value of the @type member of node; unless such an item already exists. 
                    // If no such member exists, create one and initialize it to an array whose only item is object. 
                    // Finally, continue to the next RDF triple.
                    if (predicate.Equals(RdfSpecsHelper.RdfType) && !_options.UseRdfType &&
                        (triple.Object is IUriNode || triple.Object is IBlankNode))
                    {
                        if (node.Property("@type") == null)
                        {
                            node.Add("@type", new JArray(MakeNodeString(triple.Object)));
                        }
                        else
                        {
                            (node["@type"] as JArray).Add(MakeNodeString(triple.Object));
                        }
                        continue;
                    }

                    // 4.5.5 - Set value to the result of using the RDF to Object Conversion algorithm, passing object and use native types.
                    var value = RdfToObject(triple.Object);

                    // 4.5.6 - If node does not have an predicate member, create one and initialize its value to an empty array.
                    if (node.Property(predicate) == null)
                    {
                        node.Add(predicate, new JArray());
                    }

                    // 4.5.7 - If there is no item equivalent to value in the array associated with the predicate member of node, append a reference to value to the array. Two JSON objects are considered equal if they have equivalent key-value pairs.
                    AppendUniqueElement(value, node[predicate] as JArray);

                    // 4.5.8 - If object is a blank node identifier or IRI, it might represent the list node:
                    if (triple.Object is IBlankNode || triple.Object is IUriNode)
                    {
                        // 4.5.8.1
                        var obj = MakeNodeString(triple.Object);
                        if (!nodeUsagesMap.ContainsKey(obj))
                        {
                            nodeUsagesMap.Add(obj, new JArray());
                        }
                        // 4.5.8.2
                        // AppendUniqueElement(node["@id"], nodeUsagesMap[obj] as JArray);
                        // KA - looks like a bug in the spec, if we don't add duplicate entries then this map does not correctly detect when a list node is referred to by the same subject in different statements
                        (nodeUsagesMap[obj]).Add(node["@id"]);
                        // 4.8.5.4
                        nodeMap[obj].Usages.Add(new Usage(node, predicate, value));
                    }
                }
            }

            // 5 - For each name and graph object in graph map:
            foreach (var gp in graphMap)
            {
                var graphObject = gp.Value;

                // 5.1 - If graph object has no rdf:nil member, continue with the next name-graph object pair as the graph does not contain any lists that need to be converted.
                if (!graphObject.ContainsKey(RdfSpecsHelper.RdfListNil))
                {
                    continue;
                }

                // 5.2 - Initialize nil to the value of the rdf:nil member of graph object.
                var nil = graphObject[RdfSpecsHelper.RdfListNil];

                // 5.3 - For each item usage in the usages member of nil, perform the following steps:
                var nilUsages = nil.Usages;
                if (nilUsages != null)
                {
                    foreach (var usage in nilUsages)
                    {
                        // 5.3.1 - Initialize node to the value of the value of the node member of usage, 
                        // property to the value of the property member of usage, and head to the value of the value member of usage.
                        var node = usage.Node;
                        var property = usage.Property;
                        var head = usage.Value as JObject;
                        // 5.3.2 - Initialize two empty arrays list and list nodes.
                        var list = new JArray();
                        var listNodes = new JArray();
                        // 5.3.3 - While property equals rdf:rest, the array value of the member of node usages map associated with the 
                        // @id member of node has only one member, the value associated to the usages member of node has exactly 1 entry, 
                        // node has a rdf:first and rdf:rest property, both of which have as value an array consisting of a single element, 
                        // and node has no other members apart from an optional @type member whose value is an array with a single item equal 
                        // to rdf:List, node represents a well-formed list node. 
                        // Perform the following steps to traverse the list backwards towards its head:
                        while (IsWellFormedListNode(node, property, nodeUsagesMap))
                        {
                            // 5.3.3.1 - Append the only item of rdf:first member of node to the list array.
                            list.Add((node[RdfSpecsHelper.RdfListFirst] as JArray)[0]);
                            // 5.3.3.2 - Append the value of the @id member of node to the list nodes array.
                            listNodes.Add(node["@id"]);
                            // 5.3.3.3 - Initialize node usage to the only item of the usages member of node.
                            var nodeUsage = node.Usages[0];
                            // 5.3.3.4 - Set node to the value of the node member of node usage, property to the value of the property member of node usage, and head to the value of the value member of node usage.
                            node = nodeUsage.Node;
                            property = nodeUsage.Property;
                            head = nodeUsage.Value as JObject;
                            // 5.3.3.5 - If the @id member of node is an IRI instead of a blank node identifier, exit the while loop.
                            if (!JsonLdProcessor.IsBlankNodeIdentifier(node["@id"].Value<string>())) break;
                        }
                        // 5.3.4 - If property equals rdf:first, i.e., the detected list is nested inside another list
                        if (property.Equals(RdfSpecsHelper.RdfListFirst))
                        {
                            // 5.3.4.1 - and the value of the @id of node equals rdf:nil, i.e., the detected list is empty, continue with the next usage item. The rdf:nil node cannot be converted to a list object as it would result in a list of lists, which isn't supported.
                            if (RdfSpecsHelper.RdfListNil.Equals(node["@id"].Value<string>()))
                            {
                                continue;
                            }
                            // 5.3.4.2 - Otherwise, the list consists of at least one item. We preserve the head node and transform the rest of the linked list to a list object.
                            // 5.3.4.3 - Set head id to the value of the @id member of head.
                            var headId = head["@id"].Value<string>();
                            // 5.3.4.4 - Set head to the value of the head id member of graph object so that all it's properties can be accessed.
                            head = graphObject[headId];
                            // 5.3.4.5 - Then, set head to the only item in the value of the rdf:rest member of head.
                            head = (head[RdfSpecsHelper.RdfListRest] as JArray)[0] as JObject;
                            // 5.3.4.6 - Finally, remove the last item of the list array and the last item of the list nodes array.
                            list.RemoveAt(list.Count - 1);
                            listNodes.RemoveAt(listNodes.Count - 1);
                        }
                        // 5.3.5 - Remove the @id member from head.
                        head.Remove("@id");
                        // 5.3.6 - Reverse the order of the list array.
                        list = new JArray(list.Reverse());
                        // 5.3.7 - Add an @list member to head and initialize its value to the list array.
                        head["@list"] = list;
                        // 5.3.8 - For each item node id in list nodes, remove the node id member from graph object.
                        foreach (var nodeId in listNodes)
                        {
                            graphObject.Remove(nodeId.Value<string>());
                        }
                    }
                }
            }
            // 6 - Initialize an empty array result.
            var result = new JArray();
            // 7 - For each subject and node in default graph ordered by subject:
            foreach (var dgp in defaultGraph.OrderBy(p => p.Key))
            {
                var subject = dgp.Key;
                var node = dgp.Value as JObject;
                // 7.1 - If graph map has a subject member:
                if (graphMap.ContainsKey(subject))
                {
                    // 7.1.1 - Add an @graph member to node and initialize its value to an empty array.
                    var graphArray = new JArray();
                    node["@graph"] = graphArray;
                    // 7.2.2 - For each key-value pair s-n in the subject member of graph map ordered by s, append n to the @graph member of node after removing its usages member, unless the only remaining member of n is @id.
                    foreach (var sp in graphMap[subject].OrderBy(sp => sp.Key))
                    {
                        var n = sp.Value as JObject;
                        n.Remove("usages");
                        if (n.Properties().Any(np => !np.Name.Equals("@id")))
                        {
                            graphArray.Add(n);
                        }
                    }
                }
                // 7.2 - Append node to result after removing its usages member, unless the only remaining member of node is @id.
                node.Remove("usages");
                if (node.Properties().Any(p => !p.Name.Equals("@id")))
                {
                    result.Add(node);
                }
            }
            // 8 - Return result.
            return result;
        }

        private static bool IsWellFormedListNode(JObjectWithUsages node, string property, Dictionary<string, JArray> nodeUsagesMap)
        {
            // While property equals rdf:rest, the array value of the member of node usages map associated with the 
            // @id member of node has only one member, the value associated to the usages member of node has exactly 1 entry, 
            // node has a rdf:first and rdf:rest property, both of which have as value an array consisting of a single element, 
            // and node has no other members apart from an optional @type member whose value is an array with a single item equal 
            // to rdf:List, node represents a well-formed list node. 

            if (!RdfSpecsHelper.RdfListRest.Equals(property)) return false;
            var nodeId = node["@id"].Value<string>();
            if (nodeId == null) return false;

            // Not mentioned in spec, but if node is not a blank node we should not merge it into a list array
            if (!JsonLdProcessor.IsBlankNodeIdentifier(nodeId)) return false;

            var mapEntry = nodeUsagesMap[nodeId] as JArray;
            if (mapEntry == null || mapEntry.Count != 1) return false;

            if (node.Usages.Count != 1) return false;

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
            // 1 - If value is an IRI or a blank node identifier, return a new dictionary consisting of a single member @id whose value is set to value.
            if (value is IUriNode uriNode)
            {
                return new JObject(new JProperty("@id", uriNode.Uri.OriginalString));
            }
            if (value is IBlankNode bNode)
            {
                return new JObject(new JProperty("@id", "_:" + bNode.InternalID));
            }
            // 2 - Otherwise value is an RDF literal:
            var literal = value as ILiteralNode;
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
                    } else if (literal.Value.Equals("false"))
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
            // 2.5 - Otherwise, if value is a language-tagged string add a member @language to result and set its value to the language tag of value.
            else if (!String.IsNullOrEmpty(literal.Language))
            {
                result["@language"] = literal.Language;
            }
            // 2.6 - Otherwise, set type to the datatype IRI of value, unless it equals xsd:string which is ignored.
            else
            {
                if (literal.DataType != null && !literal.DataType.ToString()
                        .Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    type = literal.DataType.ToString();
                }
            }
            // 2.7 - Add a member @value to result whose value is set to converted value.
            result["@value"] = convertedValue;
            // 2.8 - If type is not null, add a member @type to result whose value is set to type.
            if (type != null) result["@type"] = type;
            // 2.9 - Return result.
            return result;
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
            var uriNode = node as IUriNode;
            if (uriNode != null)
            {
                return uriNode.Uri.OriginalString;
            }
            var blankNode = node as IBlankNode;
            if (blankNode != null)
            {
                return "_:" + blankNode.InternalID;
            }
            throw new ArgumentException("Node must be a blank node or URI node", nameof(node));
        }

        /// <inheritdoc/>
        public event StoreWriterWarning Warning;

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
}
