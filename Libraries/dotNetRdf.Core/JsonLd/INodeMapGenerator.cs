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

using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Processors;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Interface for the JSON-LD node map generator.
/// </summary>
public interface INodeMapGenerator
{
    /// <summary>
    /// Applies the Node Map Generation algorithm to the specified input.
    /// </summary>
    /// <param name="element">The element to be processed.</param>
    /// <param name="identifierGenerator">The identifier generator instance to use when creating new blank node identifiers. Defaults to a new instance of <see cref="BlankNodeGenerator"/>.</param>
    /// <returns>The generated node map dictionary as a JObject instance.</returns>
    JObject GenerateNodeMap(JToken element, IBlankNodeGenerator identifierGenerator = null);

    /// <summary>
    /// Creates a new node map object by merging the graph-level node maps contained in the input graph map object.
    /// </summary>
    /// <param name="graphMap">The input graph map to be merged.</param>
    /// <returns>The merged node map as a new object (the original node map is not modified).</returns>
    JObject GenerateMergedNodeMap(JObject graphMap);
}