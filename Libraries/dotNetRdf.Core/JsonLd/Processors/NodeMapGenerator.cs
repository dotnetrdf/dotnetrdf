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

using System.Linq;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd.Processors;

/// <summary>
/// An implementation of the JSON-LD node map generation algorithm.
/// </summary>
public class NodeMapGenerator : INodeMapGenerator
{
    private IBlankNodeGenerator _blankNodeGenerator;

    /// <summary>
    /// Applies the Node Map Generation algorithm to the specified input.
    /// </summary>
    /// <param name="element">The element to be processed.</param>
    /// <param name="identifierGenerator">The identifier generator instance to use when creating new blank node identifiers. Defaults to a new instance of <see cref="BlankNodeGenerator"/>.</param>
    /// <returns>The generated node map dictionary as a JObject instance.</returns>
    public JObject GenerateNodeMap(JToken element, IBlankNodeGenerator identifierGenerator = null)
    {
        _blankNodeGenerator = identifierGenerator ?? new BlankNodeGenerator();
        var nodeMap = new JObject(new JProperty("@default", new JObject()));
        GenerateNodeMapAlgorithm(element, nodeMap);
        return nodeMap;
    }

    private void GenerateNodeMapAlgorithm(JToken element, JObject nodeMap,
        string activeGraph = "@default", JToken activeSubject = null,
        string activeProperty = null, JObject list = null)
    {
        // 1 - If element is an array, process each item in element as follows and then return:
        if (element is JArray elementArray)
        {
            foreach (JToken item in elementArray)
            {
                // 1.1 - Run this algorithm recursively by passing item for element, node map, active graph, active subject, active property, and list.
                GenerateNodeMapAlgorithm(item, nodeMap, activeGraph, activeSubject, activeProperty, list);
            }
            return;
        }
        // 2 - Otherwise element is a map. 
        // Reference the map which is the value of the active graph entry of node map using the variable graph.
        // If the active subject is null, set node to null otherwise reference the active subject entry of graph
        // using the variable subject node.
        var elementObject = element as JObject;
        var graph = nodeMap[activeGraph] as JObject;
        JObject node = null, subjectNode = null;
        if (activeSubject != null && activeSubject is JValue)
        {
            subjectNode = node = graph[activeSubject.Value<string>()] as JObject;
        }

        // 3 - For each item in the @type entry of element, if any, or for the value of @type, if the value of @type exists and is not an array: 
        if (elementObject.ContainsKey("@type"))
        {
            // 3.1 - If item is a blank node identifier, replace it with a newly generated blank node identifier passing item for identifier.
            if (elementObject["@type"] is JArray typeArray)
            {
                for (var ix = 0; ix < typeArray.Count; ix++)
                {
                    var typeId = typeArray[ix].Value<string>();
                    if (JsonLdUtils.IsBlankNodeIdentifier(typeId))
                    {
                        typeArray[ix] = _blankNodeGenerator.GenerateBlankNodeIdentifier(typeId);
                    }
                }
            }
            else if (elementObject["@type"] is JValue)
            {
                var typeId = elementObject["@type"].Value<string>();
                if (JsonLdUtils.IsBlankNodeIdentifier(typeId))
                {
                    elementObject["@type"] = _blankNodeGenerator.GenerateBlankNodeIdentifier(typeId);
                }
            }
        }

        // 4 - If element has an @value entry, perform the following steps:
        if (elementObject.ContainsKey("@value"))
        {
            // 4.1 - If list is null:
            if (list == null)
            {
                // 4.1.1 - If subject node does not have an active property entry,
                // create one and initialize its value to an array containing element.
                if (!subjectNode.ContainsKey(activeProperty))
                {
                    subjectNode[activeProperty] = new JArray(element);
                }
                // 4.1.2 - Otherwise, compare element against every item in the array associated with the active property member of node. If there is no item equivalent to element, append element to the array. Two dictionaries are considered equal if they have equivalent key-value pairs.
                var existingItems = node[activeProperty] as JArray;
                if (!existingItems.Any(x => JToken.DeepEquals(x, element)))
                {
                    existingItems.Add(element);
                }
            }
            else
            {
                // 4.2 - Otherwise, append element to the @list member of list.
                var listArray = list["@list"] as JArray;
                listArray.Add(element);
            }
        }
        // 5 - Otherwise, if element has an @list member, perform the following steps:
        else if (elementObject.ContainsKey("@list"))
        {
            // 5.1 - Initialize a new map result consisting of a single entry @list whose value is initialized to an empty array.
            var result = new JObject(new JProperty("@list", new JArray()));

            // 5.2 - Recursively call this algorithm passing the value of element's @list entry for element,
            // node map, active graph, active subject, active property, and result for list.
            GenerateNodeMapAlgorithm(element["@list"], nodeMap, activeGraph, activeSubject, activeProperty, result);

            // 5.3 - If list is null, append result to the value of the active property entry of subject node.
            if (list == null)
            {
                (subjectNode[activeProperty] as JArray).Add(result);
            }
            else
            {
                // 5.4 - Otherwise, append result to the @list entry of list.
                (list["@list"] as JArray).Add(result);
            }
        }
        // 6 - Otherwise element is a node object, perform the following steps:
        else
        {
            string id;
            // 6.1 - If element has an @id member, set id to its value and remove the member from element. If id is a blank node identifier, replace it with a newly generated blank node identifier passing id for identifier.
            if (elementObject.ContainsKey("@id"))
            {
                id = elementObject["@id"]?.Value<string>();
                elementObject.Remove("@id");
                if (id == null) return; // Required to pass W3C test e122
                if (JsonLdUtils.IsBlankNodeIdentifier(id))
                {
                    id = _blankNodeGenerator.GenerateBlankNodeIdentifier(id);
                }
            }
            // 6.2 - Otherwise, set id to the result of the Generate Blank Node Identifier algorithm passing null for identifier.
            else
            {
                id = _blankNodeGenerator.GenerateBlankNodeIdentifier(null);
            }
            // 6.3 - If graph does not contain a member id, create one and initialize its value to a dictionary consisting of a single member @id whose value is id.
            if (!graph.ContainsKey(id))
            {
                graph[id] = new JObject(new JProperty("@id", id));
            }
            // 6.4 - Reference the value of the id member of graph using the variable node.
            node = graph[id] as JObject;
            // 6.5 - If active subject is a dictionary, a reverse property relationship is being processed. Perform the following steps:
            if (activeSubject is JObject)
            {
                // 6.5.1 - If node does not have an active property member, create one and initialize its value to an array containing active subject.
                if (!node.ContainsKey(activeProperty))
                {
                    node[activeProperty] = new JArray(activeSubject);
                }
                // 6.5.2 - Otherwise, compare active subject against every item in the array associated with the active property member of node.
                // If there is no item equivalent to active subject, append active subject to the array.
                // Two dictionaries are considered equal if they have equivalent key-value pairs.
                else
                {
                    AppendUniqueElement(activeSubject, node[activeProperty] as JArray);
                }
            }
            // 6.6 - Otherwise, if active property is not null, perform the following steps:
            else if (activeProperty != null)
            {
                // 6.6.1 - Create a new dictionary reference consisting of a single member @id whose value is id.
                var reference = new JObject(new JProperty("@id", id));
                // 6.6.2 - If list is null:
                if (list == null)
                {
                    // 6.6.2.1 - If subject node does not have an active property member, create one and initialize its value to an array containing reference.
                    if (!subjectNode.ContainsKey(activeProperty))
                    {
                        subjectNode[activeProperty] = new JArray(reference);
                    }
                    // 6.6.2.2 - Otherwise, compare reference against every item in the array associated with the active property member of node. If there is no item equivalent to reference, append reference to the array. Two dictionaries are considered equal if they have equivalent key-value pairs.
                    AppendUniqueElement(reference, subjectNode[activeProperty] as JArray);
                }
                else
                {
                    // 6.6.3 - Otherwise, append reference to the @list member of list.
                    var listArray = list["@list"] as JArray;
                    listArray.Add(reference);
                }
            }
            // 6.7 - If element has an @type entry, append each item of its associated array to the array associated with the @type entry of node unless it is already in that array.
            // Finally remove the @type entry from element.
            if (elementObject.ContainsKey("@type"))
            {
                if (node.Property("@type") == null)
                {
                    node["@type"] = new JArray();
                }
                foreach (JToken item in JsonLdUtils.EnsureArray(elementObject["@type"]))
                {
                    AppendUniqueElement(item, node["@type"] as JArray);
                }
                elementObject.Remove("@type");
            }

            // 6.8 - If element has an @index entry, set the @index entry of node to its value.
            // If node already has an @index entry with a different value, a conflicting indexes error has been detected and processing is aborted.
            // Otherwise, continue by removing the @index entry from element.
            if (elementObject.ContainsKey("@index"))
            {
                if (node.ContainsKey("@index") && !JToken.DeepEquals(elementObject["@index"], node["@index"]))
                {
                    throw new JsonLdProcessorException(JsonLdErrorCode.ConflictingIndexes,
                        $"Conflicting indexes for node with id {id}.");
                }
                node["@index"] = elementObject["@index"];
                elementObject.Remove("@index");
            }

            // 6.9 - If element has an @reverse entry:
            if (elementObject.ContainsKey("@reverse"))
            {
                // 6.9.1 - Create a map referenced node with a single entry @id whose value is id.
                var referencedNode = new JObject(new JProperty("@id", id));
                // 6.9.2 - Initialize reverse map to the value of the @reverse entry of element.
                var reverseMap = elementObject["@reverse"] as JObject;
                // 6.9.3 - For each key-value pair property-values in reverse map:
                foreach (JProperty p in reverseMap.Properties())
                {
                    var property = p.Name;
                    var values = p.Value as JArray;
                    // 6.9.3.1 - For each value of values:
                    foreach (JToken value in values)
                    {
                        // 6.9.3.1.1 - Recursively invoke this algorithm passing value for element, node map,
                        // active graph, referenced node for active subject, and property for active property.
                        // Passing a map for active subject indicates to the algorithm that a reverse property
                        // relationship is being processed.
                        GenerateNodeMapAlgorithm(value, nodeMap, activeGraph, referencedNode, property);
                    }
                }
                // 6.9.4 - Remove the @reverse entry from element.
                elementObject.Remove("@reverse");
            }
            // 6.10 - If element has an @graph entry, recursively invoke this algorithm passing the value of the
            // @graph entry for element, node map, and id for active graph before removing the @graph entry from element.
            if (elementObject.ContainsKey("@graph"))
            {
                // KA: Ensure nodeMap contains a dictionary for the graph
                if (!nodeMap.ContainsKey(id))
                {
                    nodeMap.Add(id, new JObject());
                }
                GenerateNodeMapAlgorithm(elementObject["@graph"], nodeMap, id);
                elementObject.Remove("@graph");
            }
            // 6.11 - If element has an @included entry, recursively invoke this algorithm passing the value of the
            // @included entry for element, node map, and active graph before removing the @included entry from element.
            if (elementObject.ContainsKey("@included"))
            {
                GenerateNodeMapAlgorithm(elementObject["@included"], nodeMap, activeGraph);
                elementObject.Remove("@included");
            }
            // 6.12 - Finally, for each key-value pair property-value in element ordered by property perform the following steps:
            foreach (JProperty p in elementObject.Properties().OrderBy(p => p.Name).ToList())
            {
                var property = p.Name;
                JToken value = p.Value;
                // 6.12.1 - If property is a blank node identifier, replace it with a newly generated blank node identifier passing property for identifier.
                if (JsonLdUtils.IsBlankNodeIdentifier(property))
                {
                    property = _blankNodeGenerator.GenerateBlankNodeIdentifier(property);
                }
                // 6.12.2 - If node does not have a property entry, create one and initialize its value to an empty array.
                if (!node.ContainsKey(property))
                {
                    node[property] = new JArray();
                }
                // 6.12.3 - Recursively invoke this algorithm passing value for element, node map, active graph, id for active subject, and property for active property.
                GenerateNodeMapAlgorithm(value, nodeMap, activeGraph, id, property);
            }
        }
    }

    /// <inheritdoc />
    public JObject GenerateMergedNodeMap(JObject graphMap)
    {
        var result = new JObject();
        foreach (JProperty p in graphMap.Properties())
        {
            var graphName = p.Name;
            var nodeMap = p.Value as JObject;
            foreach (JProperty np in nodeMap.Properties())
            {
                var id = np.Name;
                var node = np.Value as JObject;
                if (!(result[id] is JObject mergedNode))
                {
                    result[id] = mergedNode = new JObject(new JProperty("@id", id));
                }
                foreach (JProperty nodeProperty in node.Properties())
                {
                    if (!JsonLdUtils.IsKeyword(nodeProperty.Name) || nodeProperty.Name.Equals("@type"))
                    {
                        MergeValues(mergedNode, nodeProperty.Name, nodeProperty.Value);
                    }
                    else
                    {
                        mergedNode[nodeProperty.Name] = nodeProperty.Value.DeepClone();
                    }
                }
            }

        }
        return result;
    }

    private static void MergeValues(JObject parent, string property, JToken values)
    {
        if (parent[property] == null)
        {
            parent[property] = new JArray();
        }
        var target = parent[property] as JArray;
        if (values is JArray)
        {
            foreach (JToken item in (values as JArray))
            {
                target.Add(item);
            }
        }
        else
        {
            target.Add(values);
        }
    }

    private static void AppendUniqueElement(JToken element, JArray toArray)
    {
        if (!toArray.Any(x => JToken.DeepEquals(x, element)))
        {
            toArray.Add(element);
        }
    }
}
