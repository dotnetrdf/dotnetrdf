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

namespace VDS.RDF.Dynamic
{
    using System;

    /// <summary>
    /// Contains helper extension methods for dynamic graphs and nodes.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Dynamically wraps a graph.
        /// </summary>
        /// <param name="graph">The graph to wrap dynamically.</param>
        /// <param name="subjectBaseUri">The Uri to use for resolving relative subject references.</param>
        /// <param name="predicateBaseUri">The Uri used to resolve relative predicate references.</param>
        /// <returns>A dynamic graph that wrappes <paramref name="graph"/>.</returns>
        public static dynamic AsDynamic(this IGraph graph, Uri subjectBaseUri = null, Uri predicateBaseUri = null)
        {
            return new DynamicGraph(graph, subjectBaseUri, predicateBaseUri);
        }

        /// <summary>
        /// Dynamically wraps a node.
        /// </summary>
        /// <param name="node">The node to wrap dynamically.</param>
        /// <param name="baseUri">The Uri to use for resolving relative predicate references.</param>
        /// <returns>A dynamic node that wraps <paramref name="node"/>.</returns>
        public static dynamic AsDynamic(this INode node, Uri baseUri = null)
        {
            return new DynamicNode(node, baseUri);
        }
    }
}
