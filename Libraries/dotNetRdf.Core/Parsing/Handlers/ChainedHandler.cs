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
using System.Linq;

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A Handler which passes the RDF to be handled through a sequence of Handlers where Handling is terminated as soon as any Handler returns false.
/// </summary>
/// <remarks>
/// <para>
/// This differs from the <see cref="MultiHandler">MultiHandler</see> in that as soon as any Handler indicates that handling should stop by returning false handling is <strong>immediately</strong> terminated.  All Handlers will always have their StartRdf and EndRdf methods called.
/// </para>
/// </remarks>
public class ChainedHandler : BaseRdfHandler, IWrappingRdfHandler
{
    private readonly List<IRdfHandler> _handlers = new();

    /// <summary>
    /// Creates a new Chained Handler.
    /// </summary>
    /// <param name="handlers">Inner Handlers to use.</param>
    public ChainedHandler(IEnumerable<IRdfHandler> handlers)
    {
        if (handlers == null)
        {
            throw new ArgumentNullException(nameof(handlers), "Must be at least 1 Handler for use by the ChainedHandler");
        }

        _handlers.AddRange(handlers);
        if (!_handlers.Any())
        {
            throw new ArgumentException("Must be at least 1 Handler for use by the ChainedHandler", nameof(handlers));
        }

        // Check there are no identical handlers in the List
        for (var i = 0; i < _handlers.Count; i++)
        {
            for (var j = i + 1; j < _handlers.Count; j++)
            {
                if (ReferenceEquals(_handlers[i], _handlers[j]))
                {
                    throw new ArgumentException("All Handlers must be distinct IRdfHandler instances", "handlers");
                }
            }
        }
    }

    /// <summary>
    /// Gets the Inner Handlers used by this Handler.
    /// </summary>
    public IEnumerable<IRdfHandler> InnerHandlers
    {
        get
        {
            return _handlers;
        }
    }

    /// <summary>
    /// Starts the Handling of RDF for each inner handler.
    /// </summary>
    protected override void StartRdfInternal()
    {
        _handlers.ForEach(h => h.StartRdf());
    }

    /// <summary>
    /// Ends the Handling of RDF for each inner handler.
    /// </summary>
    /// <param name="ok">Whether parsing completed without errors.</param>
    protected override void EndRdfInternal(bool ok)
    {
        _handlers.ForEach(h => h.EndRdf(ok));
    }

    /// <summary>
    /// Handles Base URIs by getting each inner handler to attempt to handle it.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// Handling terminates at the first Handler which indicates handling should stop.
    /// </remarks>
    protected override bool HandleBaseUriInternal(Uri baseUri)
    {
        return _handlers.All(h => h.HandleBaseUri(baseUri));
    }

    /// <summary>
    /// Handles Namespaces by getting each inner handler to attempt to handle it.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="namespaceUri">Namespace URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// Handling terminates at the first Handler which indicates handling should stop.
    /// </remarks>
    protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
    {
        return _handlers.All(h => h.HandleNamespace(prefix, namespaceUri));
    }

    /// <summary>
    /// Handles Triples by getting each inner handler to attempt to handle it.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    /// <remarks>
    /// Handling terminates at the first Handler which indicates handling should stop.
    /// </remarks>
    protected override bool HandleTripleInternal(Triple t)
    {
        return _handlers.All(h => h.HandleTriple(t));
    }

    /// <summary>
    /// Handles Quads by getting each inner handler to attempt to handle it.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The name of the graph containing the triple.</param>
    /// <returns></returns>
    /// <remarks>
    /// Handling terminates at the first Handler which indicates handling should stop.
    /// </remarks>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        return _handlers.All(h => h.HandleQuad(t, graph));
    }

    /// <summary>
    /// Handles Comments by getting each inner handler to attempt to handle it.
    /// </summary>
    /// <param name="text">Comment text.</param>
    /// <returns></returns>
    /// <remarks>
    /// Handling terminates at the first Handler which indicates handling should stop.
    /// </remarks>
    protected override bool HandleCommentInternal(string text)
    {
        return _handlers.OfType<ICommentRdfHandler>().All(h => h.HandleComment(text));
    }

    /// <summary>
    /// Gets that this Handler accepts all Triples if all inner handlers do so.
    /// </summary>
    public override bool AcceptsAll
    {
        get
        {
            return _handlers.All(h => h.AcceptsAll);
        }
    }
}
