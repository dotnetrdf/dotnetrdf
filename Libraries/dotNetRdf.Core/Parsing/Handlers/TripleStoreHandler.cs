/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
/// An  RDF Handler which asserts quads into a triple store.
/// </summary>
public class TripleStoreHandler : BaseRdfHandler
{
    private readonly ITripleStore _store;

    /// <summary>
    /// Creates a new Triple Store Handler.
    /// </summary>
    /// <param name="store">Triple Store to assert quads into.</param>
    /// <param name="defaultGraphName">The default graph name to use when asserting triples.</param>
    /// <param name="nodeFactory">Node Factory to use when creating the quads.</param>
    public TripleStoreHandler(ITripleStore store, IRefNode? defaultGraphName = null, INodeFactory? nodeFactory = null):base(nodeFactory ?? new NodeFactory())
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        DefaultGraphName = defaultGraphName;
    }

    /// <summary>
    /// The default graph name to use when asserting triples.
    /// </summary>
    /// <remarks>
    /// A value of <c>null</c> indicates that the unnamed graph of the triple store should be used.
    /// </remarks>
    public IRefNode? DefaultGraphName { get; }


    /// <summary>
    /// Handles a triple by asserting it into the default graph configured on this handler.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        _store.Assert(new Quad(t, DefaultGraphName));
        return true;
    }

    /// <inheritdoc/>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        _store.Assert(new Quad(t, graph));
        return true;
    }

    /// <inheritdoc/>
    public override bool AcceptsAll => true;
}