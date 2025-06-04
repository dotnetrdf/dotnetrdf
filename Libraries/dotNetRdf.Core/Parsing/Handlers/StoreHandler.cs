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

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A RDF Handler that loads Quads into a <see cref="ITripleStore">ITripleStore</see> instance.
/// </summary>
public class StoreHandler
    : BaseRdfHandler
{
    private readonly INamespaceMapper _nsmap = new NamespaceMapper();

    /// <summary>
    /// Creates a new Store Handler.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    public StoreHandler(ITripleStore store)
    {
        Store = store ?? throw new ArgumentNullException(nameof(store));
    }

    /// <summary>
    /// Gets the Triple Store that this Handler is populating.
    /// </summary>
    protected ITripleStore Store { get; }

    #region IRdfHandler Members

    /// <summary>
    /// Handles namespaces by adding them to each graph.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="namespaceUri">Namespace URI.</param>
    /// <returns></returns>
    protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
    {
        _nsmap.AddNamespace(prefix, namespaceUri);
        return true;
    }

    /// <summary>
    /// Handles Triples by asserting them into the un-named graph creating the graph if necessary.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        if (!Store.HasGraph((IRefNode)null))
        {
            var g = new Graph();
            Store.Add(g);
        }
        IGraph target = Store[(IRefNode)null];
        target.Assert(t);
        return true;
    }

    /// <summary>
    /// Handles Triples by asserting them into the appropriate graph creating the graph if necessary.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The graph containing the triple.</param>
    /// <returns></returns>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        if (!Store.HasGraph(graph))
        {
            var g = new Graph(graph);
            Store.Add(g);
        }
        IGraph target = Store[graph];
        target.Assert(t);
        return true;
    }

    /// <summary>
    /// Starts handling RDF.
    /// </summary>
    protected override void StartRdfInternal()
    {
        _nsmap.Clear();
    }

    /// <summary>
    /// Ends RDF handling and propagates all discovered namespaces to all discovered graphs.
    /// </summary>
    /// <param name="ok">Whether parsing completed successfully.</param>
    protected override void EndRdfInternal(bool ok)
    {
        // Propogate discovered namespaces to all graphs
        foreach (IGraph g in Store.Graphs)
        {
            g.NamespaceMap.Import(_nsmap);
        }
    }

    /// <summary>
    /// Gets that the Handler accepts all Triples.
    /// </summary>
    public override bool AcceptsAll
    {
        get
        {
            return true;
        }
    }

    #endregion
}
