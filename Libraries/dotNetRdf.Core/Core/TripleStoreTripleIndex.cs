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

namespace VDS.RDF;

/// <summary>
/// Provides an implementation of the <see cref="ITripleIndex"/> interface over a subset of the graphs contained
/// in a <see cref="ITripleStore"/>.
/// </summary>
public class TripleStoreTripleIndex : ITripleIndex
{
    private readonly IEnumerable<IGraph> _graphs;

    /// <summary>
    /// Create a triple index over the default graph of the specified store.
    /// </summary>
    /// <param name="store">The store to expose as a triple index.</param>
    /// <param name="unionDefaultGraph">Whether the default graph is the union of all graphs in the store (true) or just the unnamed graph (false).</param>
    public TripleStoreTripleIndex(ITripleStore store, bool unionDefaultGraph)
    {
        _graphs = unionDefaultGraph ? store.Graphs : new[] {store[(IRefNode)null]};
    }

    /// <summary>
    /// Create a triple index over the specified graph of the specified store.
    /// </summary>
    /// <param name="store">The store to expose as a triple index.</param>
    /// <param name="defaultGraph">The name of the graph to expose in the index.</param>
    /// <exception cref="ArgumentException">Raised if <paramref name="defaultGraph"/> is not the name of any existing graph in <paramref name="store"/>.</exception>
    public TripleStoreTripleIndex(ITripleStore store, IRefNode defaultGraph)
    {
        if (!store.HasGraph(defaultGraph))
        {
            throw new ArgumentException("The specified default graph does not exist in the store.", nameof(defaultGraph));
        }
        _graphs = [store[defaultGraph]];
    }

    private IEnumerable<Triple> GetTriples(Func<IGraph, IEnumerable<Triple>> selector)
    {
        return _graphs.SelectMany(selector);
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(Uri uri)
    {
        return GetTriples(new UriNode(uri));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(INode n)
    {
        foreach (IGraph g in _graphs)
        {
            foreach (Triple t in g.GetTriplesWithSubject(n).Union(g.GetTriplesWithPredicate(n))
                         .Union(g.GetTriplesWithObject(n)))
            {
                yield return t;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithObject(Uri u)
    {
        return GetTriplesWithObject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithObject(INode n)
    {
        return GetTriples(g => g.GetTriplesWithSubject(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
    {
        return GetTriples(g => g.GetTriplesWithPredicate(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
    {
        return GetTriplesWithPredicate(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubject(INode n)
    {
        return GetTriples(g=>g.GetTriplesWithSubject(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
    {
        return GetTriplesWithSubject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
    {
        return GetTriples(g=>g.GetTriplesWithSubjectPredicate(subj, pred));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
    {
        return GetTriples(g=>g.GetTriplesWithSubjectObject(subj, obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
    {
        return GetTriples(g=>g.GetTriplesWithPredicateObject(pred, obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuoted(Uri uri)
    {
        return GetTriples(g => g.GetQuoted(uri));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuoted(INode n)
    {
        return GetTriples(g => g.GetQuoted(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithObject(Uri u)
    {
        return GetQuotedWithObject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithObject(INode n)
    {
        return GetTriples(g => g.GetQuotedWithObject(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicate(INode n)
    {
        return GetTriples(g => g.GetQuotedWithPredicate(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicate(Uri u)
    {
        return GetQuotedWithPredicate(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubject(INode n)
    {
        return GetTriples(g => g.GetQuotedWithSubject(n));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubject(Uri u)
    {
        return GetQuotedWithSubject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return GetTriples(g=>g.GetQuotedWithSubjectPredicate(subj, pred));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj)
    {
        return GetTriples(g=>g.GetQuotedWithSubjectObject(subj, obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj)
    {
        return GetTriples(g=>g.GetQuotedWithPredicateObject(pred, obj));
    }
}

