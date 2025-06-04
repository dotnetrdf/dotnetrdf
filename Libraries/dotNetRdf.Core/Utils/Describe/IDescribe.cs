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

namespace VDS.RDF.Utils.Describe;

/// <summary>
/// Interface for a class of algorithms that emit a sub-graph of a dataset from a list of starting nodes.
/// </summary>
public interface IDescribeAlgorithm
{
    /// <summary>
    /// Generates a Graph which is the description of the resources represented by the provided nodes.
    /// </summary>
    /// <param name="dataset">The dataset to extract descriptions from.</param>
    /// <param name="nodes">The nodes to be described.</param>
    /// <returns></returns>
    IGraph Describe(ITripleIndex dataset, IEnumerable<INode> nodes);

    /// <summary>
    /// Generates a graph which is the description of the resources represented by the provided nodes.
    /// </summary>
    /// <param name="handler">The handler to receive the triples that provide the description.</param>
    /// <param name="dataset">The dataset to extract descriptions from.</param>
    /// <param name="nodes">The nodes to be described.</param>
    /// <param name="baseUri">An optional base URI to pass through to the RDF handler.</param>
    /// <param name="namespaceMap">An optional namespace map to pass through to the RDF handler.</param>
    void Describe(IRdfHandler handler, ITripleIndex dataset, IEnumerable<INode> nodes, Uri baseUri = null, INamespaceMapper namespaceMap = null);
}
