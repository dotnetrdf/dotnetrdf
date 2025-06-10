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
/// A RDF Handler that rewrites the Graph URIs of Triples before passing them to an inner handler.
/// </summary>
public class GraphUriRewriteHandler
    : BaseRdfHandler, IWrappingRdfHandler
{
    private readonly IRdfHandler _handler;
    private readonly IRefNode _graphName;

    /// <summary>
    /// Creates a new Graph URI rewriting handler.
    /// </summary>
    /// <param name="handler">Handler to wrap.</param>
    /// <param name="graphUri">Graph URI to rewrite to.</param>
    public GraphUriRewriteHandler(IRdfHandler handler, Uri graphUri)
        : base()
    {
        _handler = handler;
        _graphName = new UriNode(graphUri);
    }

    /// <summary>
    /// Creates a new Graph URI rewriting handler.
    /// </summary>
    /// <param name="handler">Handler to wrap.</param>
    /// <param name="graphName">Graph name to rewrite to.</param>
    public GraphUriRewriteHandler(IRdfHandler handler, IRefNode graphName)
    {
        _handler = handler;
        _graphName = graphName;
    }

    /// <summary>
    /// Gets the Inner Handler.
    /// </summary>
    public IEnumerable<IRdfHandler> InnerHandlers
    {
        get
        {
            return _handler.AsEnumerable();
        }
    }

    /// <summary>
    /// Starts handling of RDF.
    /// </summary>
    protected override void StartRdfInternal()
    {
        base.StartRdfInternal();
        _handler.StartRdf();
    }

    /// <summary>
    /// Ends handling of RDF.
    /// </summary>
    /// <param name="ok">Whether parsing completed OK.</param>
    protected override void EndRdfInternal(bool ok)
    {
        _handler.EndRdf(ok);
        base.EndRdfInternal(ok);
    }

    /// <summary>
    /// Handles a Base URI declaration.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    /// <returns></returns>
    protected override bool HandleBaseUriInternal(Uri baseUri)
    {
        return _handler.HandleBaseUri(baseUri);
    }

    /// <summary>
    /// Handles a Namespace declaration.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="namespaceUri">Namespace URI.</param>
    /// <returns></returns>
    protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
    {
        return _handler.HandleNamespace(prefix, namespaceUri);
    }

    /// <summary>
    /// Handles a Triple by rewriting the Graph URI and passing it to the inner handler.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        return _handler.HandleQuad(t, _graphName);
    }

    /// <summary>
    /// Handles a quad by rewriting the graph name and passing it to the inner handler.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The name of the graph containing the triple.</param>
    /// <returns></returns>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        return _handler.HandleQuad(t, _graphName);
    }

    /// <summary>
    /// Returns true since this handler accepts all triples.
    /// </summary>
    public override bool AcceptsAll
    {
        get 
        {
            return true; 
        }
    }
}
