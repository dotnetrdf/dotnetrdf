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

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd.Processors;

/// <summary>
/// Implements the JSON-LD Flatten algorithm.
/// </summary>
internal class FlattenProcessor
{
    private readonly INodeMapGenerator _nodeMapGenerator;

    /// <summary>
    /// Create a new flatten processor instance.
    /// </summary>
    /// <param name="nodeMapGenerator">The node map generator to use for the initial node mapping step of the algorithm. Defaults to a new instance of <see cref="NodeMapGenerator"/>.</param>
    public FlattenProcessor(INodeMapGenerator nodeMapGenerator = null)
    {
        _nodeMapGenerator = nodeMapGenerator ?? new NodeMapGenerator();
    }

    /// <summary>
    /// Create a new flattened representation of the input element.
    /// </summary>
    /// <param name="element">The element to be processed.</param>
    /// <param name="ordered">True to process the properties of the element in lexicographical order.</param>
    /// <returns>A new token representing the flattened element.</returns>
    /// <remarks>This operation does not modify the input element.</remarks>
    public JToken FlattenElement(JToken element, bool ordered = false)
    {
        // 1 = Initialize node map to a map consisting of a single member whose key is @default
        // and whose value is an empty map.
        // 2 - Perform the Node Map Generation algorithm, passing element and node map.
        JObject nodeMap = _nodeMapGenerator.GenerateNodeMap(element);

        // 3 - Initialize default graph to the value of the @default member of node map,
        // which is a map representing the default graph.
        var defaultGraph = nodeMap["@default"] as JObject;

        // 4 - For each key-value pair graph name-graph in node map where graph name is not @default,
        // ordered lexicographically by graph name if ordered is true, perform the following steps: 
        IEnumerable<JProperty> properties = nodeMap.Properties().Where(p => !p.Name.Equals("@default"));
        if (ordered) properties = properties.OrderBy(p => p.Name);
        foreach (JProperty p in properties)
        {
            var graphName = p.Name;
            var graph = p.Value as JObject;

            // 4.1 - If default graph does not have a graph name entry, create one and initialize its
            // value to a map consisting of an @id entry whose value is set to graph name.
            if (!defaultGraph.ContainsKey(graphName))
            {
                defaultGraph.Add(graphName, new JObject(new JProperty("@id", graphName)));
            }

            // 4.2 - Reference the value associated with the graph name entry in default graph using the variable entry.
            var entry = defaultGraph[graphName] as JObject;

            // 4.3 - Add an @graph entry to entry and set it to an empty array.
            // 4.4 - For each id-node pair in graph ordered lexicographically by id if ordered is true,
            // add node to the @graph entry of entry, unless the only entry of node is @id.
            entry["@graph"] = FlattenGraph(graph, ordered);
        }

        // 5 - Initialize an empty array flattened.
        // 6 - For each id-node pair in default graph ordered lexicographically by id if ordered is true,
        // add node to flattened, unless the only entry of node is @id.
        JArray flattened = FlattenGraph(defaultGraph, ordered);


        // 7 - return flattened.
        return flattened;
    }

    private static JArray FlattenGraph(JObject graphObject, bool ordered)
    {
        var flattened = new JArray();
        IEnumerable<JProperty> graphProperties = graphObject.Properties();
        if (ordered) graphProperties = graphProperties.OrderBy(p => p.Name);
        foreach (JProperty p in graphProperties)
        {
            var node = p.Value as JObject;
            if (node.Count > 1 || node.Properties().Any(x => !x.Name.Equals("@id")))
            {
                flattened.Add(node);
            }
        }

        return flattened;
    }
}
