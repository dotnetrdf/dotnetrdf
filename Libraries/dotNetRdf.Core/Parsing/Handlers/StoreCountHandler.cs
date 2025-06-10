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

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A RDF Handler which simply counts the Triples and Graphs.
/// </summary>
public class StoreCountHandler : BaseRdfHandler
{
    private HashSet<string> _graphs;

    /// <summary>
    /// Creates a new Store Count Handler.
    /// </summary>
    public StoreCountHandler()
        : base(new NodeFactory(new NodeFactoryOptions())) { }

    /// <summary>
    /// Starts RDF Handling by resetting the counters.
    /// </summary>
    protected override void StartRdfInternal()
    {
        TripleCount = 0;
        _graphs = new HashSet<string>();
    }

    /// <summary>
    /// Handles Triples/Quads by counting the Triples and distinct Graph URIs.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        TripleCount++;
        _graphs.Add(String.Empty);
        return true;
    }

    /// <summary>
    /// Handles Triples/Quads by counting the Triples and distinct Graph URIs.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The graph containing the triple.</param>
    /// <returns></returns>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        TripleCount++;
        _graphs.Add(graph.ToSafeString());
        return true;
    }

    /// <summary>
    /// Gets the count of Triples.
    /// </summary>
    public int TripleCount { get; private set; }

    /// <summary>
    /// Gets the count of distinct Graph URIs.
    /// </summary>
    public int GraphCount => _graphs.Count;

    /// <summary>
    /// Gets that this Handler accepts all Triples.
    /// </summary>
    public override bool AcceptsAll => true;
}
