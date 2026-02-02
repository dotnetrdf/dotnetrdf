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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.LDF.Hydra;
using VDS.RDF.Parsing;

namespace VDS.RDF.LDF.Client;

/// <summary>
/// A <see cref="IGraph">graph</see> that dispatches all operations to a Triple Pattern Fragments (TPF) endpoint.
/// </summary>
/// <remarks>
/// <para>Caution: All operations on this graph lead to (potentially numerous) network requests. This presents complexity characteristics that are very different from those you might expect from in-memory implementations of the <see cref="IGraph">interface</see>.</para>
/// <para>
/// This graph, like LDF itself, does not support
/// <list type="bullet">
/// <item>blank nodes,</item>
/// <item>quoted triples or</item>
/// <item>mutation.</item>
/// </list>
/// </para>
/// </remarks>
/// <exception cref="LdfException">Throw under various circumstances to represent operations that are illigal in the context of LDF or when this client is not compatible with the response from the LDF endpoint.</exception>
public class TpfLiveGraph : Graph
{
    private bool _disposed;
    private readonly TpfLoader _fragment;
    private readonly IriTemplate _search;

    /// <summary>
    /// Initializes a new instance of the <see cref="TpfLiveGraph"/> class.
    /// </summary>
    /// <param name="baseUri">The URI of the TPF endpoint used to obtain triples.</param>
    /// <param name="reader">(Optional) The reader to be used for parsing LDF responses (Turtle by default).</param>
    /// <param name="loader">(Optional) The loader to be used when sending LDF requests (<see cref="Loader"/> by default).</param>
    /// <exception cref="ArgumentNullException"><paramref name="baseUri"/> is <see langword="null"/>.</exception>
    /// <remarks>When this constructor is called, a network request will be sent to gather the LDF metadata.</remarks>
    public TpfLiveGraph(Uri baseUri, IRdfReader reader = null, Loader loader = null)
    {
        _fragment = new TpfLoader(baseUri ?? throw new ArgumentNullException(nameof(baseUri)), reader, loader);
        _search = _fragment.Metadata.Search;
        _triples = new TpfTripleCollection(_search, reader, loader);
    }

    /// <summary>
    /// Determines whether this graph is equal to the <paramref name="other"/> graph.
    /// </summary>
    /// <param name="other">Graph to test for equality.</param>
    /// <param name="mapping">Always <see langword="null"/> because Linked Data Fragments does not support blank nodes.</param>
    /// <returns>Whether this graph is equal to the <paramref name="other"/> graph.</returns>
    /// <remarks>
    /// <para>LDF graphs are equal to each other if their search templates are the same.</para>
    /// <para>An LDF graph might be equal to other graph types if they contain the same triples.</para>
    /// <para>Caution: Comparing LDF graphs to other types of graphs requires enumerating all statements in the LDF graph, which potentially involves numerous network requests.</para>
    /// </remarks>
    public override bool Equals(IGraph other, out Dictionary<INode, INode> mapping)
    {
        if (other is not TpfLiveGraph otherLdf)
        {
            return base.Equals(other, out mapping);
        }

        mapping = null; // No blanks in QPF
        return _search.Template == otherLdf._search.Template;
    }

    #region Mutation methods throw because this graph is read-only

    /// <summary>
    /// This graph cannot assert a triple because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Nothing.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override bool Assert(Triple _) => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot assert triples because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Nothing.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override bool Assert(IEnumerable<Triple> _) => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot be cleared because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void Clear() => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot merge another because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void Merge(IGraph _) => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot merge another because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <param name="__">Ignored.</param>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override void Merge(IGraph _, bool __) => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot retract a triple because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Nothing.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override bool Retract(Triple _) => throw new NotSupportedException("This graph is read-only.");

    /// <summary>
    /// This graph cannot retract triples because Linked Data Fragments does not support mutation.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Nothing.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public override bool Retract(IEnumerable<Triple> _) => throw new NotSupportedException("This graph is read-only.");

    #endregion

    #region Some methods and properties short-circuit to empty due to unsupported features in LDF

    /// <summary>
    /// This graph returns no quoted nodes because Linked Data Fragments does not support RDF*.
    /// </summary>
    public override IEnumerable<INode> AllQuotedNodes => [];

    /// <summary>
    /// This graph does not contain any quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>False.</returns>
    public override bool ContainsQuotedTriple(Triple _) => default;

    /// <summary>
    /// This graph returns no blank nodes because Linked Data Fragments does not support blank nodes.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Null.</returns>
    public override IBlankNode GetBlankNode(string _) => default;

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuoted(INode _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuoted(Uri _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithObject(Uri _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithObject(INode _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithPredicate(Uri _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithPredicate(INode _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithSubject(Uri _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithSubject(INode _) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <param name="__">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode _, INode __) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <param name="__">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithSubjectObject(INode _, INode __) => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    /// <param name="_">Ignored.</param>
    /// <param name="__">Ignored.</param>
    /// <returns>Empty.</returns>
    public override IEnumerable<Triple> GetQuotedWithPredicateObject(INode _, INode __) => [];

    /// <summary>
    /// This graph returns no quoted nodes because Linked Data Fragments does not support RDF*.
    /// </summary>
    public override IEnumerable<INode> QuotedNodes => [];

    /// <summary>
    /// This graph returns no quoted triples because Linked Data Fragments does not support RDF*.
    /// </summary>
    public override IEnumerable<Triple> QuotedTriples => [];

    #endregion

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                _fragment?.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
