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
/// A RDF Handler which asserts Triples into a Graph.
/// </summary>
public class GraphHandler : BaseRdfHandler
{
    private IGraph _target;

    /// <summary>
    /// Creates a new Graph Handler.
    /// </summary>
    /// <param name="g">Graph.</param>
    public GraphHandler(IGraph g)
        : base(g)
    {
        Graph = g ?? throw new ArgumentNullException(nameof(g));
    }

    /// <summary>
    /// Gets the Graph that this handler wraps.
    /// </summary>
    protected IGraph Graph { get; }

    /// <summary>
    /// Starts Handling RDF ensuring that if the target Graph is non-empty RDF is handling into a temporary Graph until parsing completes successfully.
    /// </summary>
    protected override void StartRdfInternal()
    {
        if (Graph.IsEmpty)
        {
            _target = Graph;
        }
        else
        {
            _target = new Graph(true);
            _target.NamespaceMap.Import(Graph.NamespaceMap);
            _target.BaseUri = Graph.BaseUri;
        }
        NodeFactory = _target;
    }

    /// <summary>
    /// Ends Handling RDF discarding the handled Triples if parsing failed (indicated by false for the <paramref name="ok">ok</paramref> parameter) and otherwise merging the handled triples from the temporary graph into the target graph if necessary.
    /// </summary>
    /// <param name="ok">Indicates whether parsing completed OK.</param>
    protected override void EndRdfInternal(bool ok)
    {
        if (ok)
        {
            // If the Target Graph was different from the Destination Graph then do a Merge
            if (!ReferenceEquals(Graph, _target))
            {
                Graph.Merge(_target);
                Graph.NamespaceMap.Import(_target.NamespaceMap);
                if (Graph.BaseUri == null) Graph.BaseUri = _target.BaseUri;
            }
            else
            {
                // The Target was the Graph so we want to set our reference to it to be null so we don't
                // clear it in the remainder of our clean up step
                _target = null;
            }
        }
        else
        {
            // Discard the Parsed Triples if parsing failed
            if (ReferenceEquals(Graph, _target))
            {
                Graph.Clear();
                _target = null;
            }
        }

        // Always throw away the target afterwards if not already done so
        if (_target != null)
        {
            _target.Clear();
            _target = null;
        }
    }

    /// <summary>
    /// Handles Namespace Declarations by adding them to the Graphs Namespace Map.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="namespaceUri">Namespace URI.</param>
    /// <returns></returns>
    protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
    {
        _target.NamespaceMap.AddNamespace(prefix, namespaceUri);
        return true;
    }

    /// <summary>
    /// Handles Base URI Declarations by setting the Graphs Base URI.
    /// </summary>
    /// <param name="baseUri">Base URI.</param>
    /// <returns></returns>
    protected override bool HandleBaseUriInternal(Uri baseUri)
    {
        _target.BaseUri = baseUri;
        return true;
    }

    /// <summary>
    /// Handles Triples by asserting them in the Graph.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        _target.Assert(t);
        return true;
    }

    /// <summary>
    /// Handles Quads by asserting them in the target graph specified in the constructor of this handler.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="graph">The name of the graph containing the triple.</param>
    /// <returns></returns>
    /// <remarks>This handler asserts all triples received into the target graph, regardless of the graph name passed in <paramref name="graph"/>.</remarks>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        _target.Assert(t);
        return true;
    }

    /// <summary>
    /// Gets that this Handler accepts all Triples.
    /// </summary>
    public override bool AcceptsAll
    {
        get 
        {
            return true; 
        }
    }
}
