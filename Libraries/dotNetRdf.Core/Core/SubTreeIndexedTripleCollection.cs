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
using VDS.Common.Collections;
using VDS.Common.Trees;

namespace VDS.RDF;

/// <summary>
/// An indexed triple collection that uses our <see cref="MultiDictionary{TKey,TValue}"/> and <see cref="BinaryTree{TKey,TValue}"/> implementations under the hood for the index structures.
/// </summary>
/// <remarks>
/// <para>
/// A variation on <see cref="TreeIndexedTripleCollection"/> which structures the indexes slightly differently, this may give differing performance and reduced memory usage in some scenarios.
/// </para>
/// </remarks>
public class SubTreeIndexedTripleCollection
    : AbstractIndexedTripleCollection
{
    // Indexes
    private readonly MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>>
        _sp = new(new FastVirtualNodeComparer()),
        _po = new(new FastVirtualNodeComparer()),
        _os = new(new FastVirtualNodeComparer()),
        _qsp = new(new FastVirtualNodeComparer()),
        _qpo = new(new FastVirtualNodeComparer()),
        _qos = new(new FastVirtualNodeComparer());

    // Placeholder Variables for compound lookups
    private readonly VariableNode _subjVar = new("s"),
                         _predVar = new("p"),
                         _objVar = new("o");

    // Hash Functions
    private readonly Func<Triple, int> _sHash = t => Tools.CombineHashCodes(t.Subject, t.Predicate),
                              _pHash = t => Tools.CombineHashCodes(t.Predicate, t.Object),
                              _oHash = t => Tools.CombineHashCodes(t.Object, t.Subject);

    // Comparers
    private readonly BaseTripleComparer _spComparer = new SubjectPredicateComparer(new FastVirtualNodeComparer()),
        _poComparer = new PredicateObjectComparer(new FastVirtualNodeComparer()),
        _osComparer = new ObjectSubjectComparer(new FastVirtualNodeComparer());

    /// <summary>
    /// Indexes a Triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected override void IndexAsserted(Triple t)
    {
        Index(t.Subject, t, _sp, _sHash, _spComparer);
        Index(t.Predicate, t, _po, _pHash, _poComparer);
        Index(t.Object, t, _os, _oHash, _osComparer);
    }

    /// <inheritdoc />
    protected override void IndexQuoted(Triple t)
    {
        Index(t.Subject, t, _qsp, _sHash, _spComparer);
        Index(t.Predicate, t, _qpo, _pHash, _poComparer);
        Index(t.Object, t, _qos, _oHash, _osComparer);
    }

    /// <summary>
    /// Helper for indexing triples.
    /// </summary>
    /// <param name="n">Node to index by.</param>
    /// <param name="t">Triple.</param>
    /// <param name="index">Index to insert into.</param>
    /// <param name="comparer">Comparer for the Index.</param>
    /// <param name="hashFunc">Hash Function for the Index.</param>
    private void Index(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index, Func<Triple,int> hashFunc, IComparer<Triple> comparer)
    {
        if (index.TryGetValue(n, out MultiDictionary<Triple, HashSet<Triple>> subtree))
        {
            if (subtree.TryGetValue(t, out HashSet<Triple> ts))
            {
                if (ts == null)
                {
                    subtree[t] = new HashSet<Triple> { t };
                }
                else
                {
                    ts.Add(t);
                }
            }
            else
            {
                subtree.Add(t, new HashSet<Triple> { t });
            }
        }
        else
        {
            subtree = new MultiDictionary<Triple, HashSet<Triple>>(hashFunc, false, comparer, MultiDictionaryMode.Avl);
            subtree.Add(t, new HashSet<Triple> { t });
            index.Add(n, subtree);
        }
    }

    /// <summary>
    /// Unindexes a triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected override void UnindexAsserted(Triple t)
    {
        Unindex(t.Subject, t, _sp);
        Unindex(t.Predicate, t, _po);
        Unindex(t.Object, t, _os);

    }

    /// <inheritdoc/>
    protected override void UnindexQuoted(Triple t)
    {
        Unindex(t.Subject, t, _qsp);
        Unindex(t.Predicate, t, _qpo);
        Unindex(t.Object, t, _qos);
    }

    /// <summary>
    /// Helper for unindexing triples.
    /// </summary>
    /// <param name="n">Node to index by.</param>
    /// <param name="t">Triple.</param>
    /// <param name="index">Index to remove from.</param>
    private void Unindex(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index)
    {
        if (index.TryGetValue(n, out MultiDictionary<Triple, HashSet<Triple>> subtree))
        {
            if (subtree.TryGetValue(t, out HashSet<Triple> ts))
            {
                if (ts != null) ts.Remove(t);
            }
        }
    }


    /// <summary>
    /// Gets the specific instance of a Triple in the collection.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public override Triple this[Triple t]
    {
        get
        {
            if (Triples.TryGetKey(t, out Triple actual))
            {
                return actual;
            }
            throw new KeyNotFoundException("Given triple does not exist in this collection");
        }
    }

    private IEnumerable<Triple> WithNode(INode key, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index)
    {
        if (index.TryGetValue(key, out MultiDictionary<Triple, HashSet<Triple>> subtree))
        {
            return (from ts in subtree.Values
                    where ts != null
                    from t in ts
                    select t);
        }
        return Enumerable.Empty<Triple>();
    }

    private IEnumerable<Triple> WithNodeAndTriple(INode key, Triple subkey, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index)
    {
        if (index.TryGetValue(key, out MultiDictionary<Triple, HashSet<Triple>> subtree))
        {
            if (subtree.TryGetValue(subkey, out HashSet<Triple> triples))
            {
                return triples ?? Enumerable.Empty<Triple>();
            }
        }
        return Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all the triples with a given object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithObject(INode obj)
    {
        return WithNode(obj, _os);
    }

    /// <summary>
    /// Gets all the triples with a given predicate.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithPredicate(INode pred)
    {
        return WithNode(pred, _po);
    }

    /// <summary>
    /// Gets all the triples with a given subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubject(INode subj)
    {
        return WithNode(subj, _sp);
    }

    /// <summary>
    /// Gets all the triples with a given predicate and object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
    {
        return WithNodeAndTriple(pred, new Triple(_subjVar, pred, obj), _po);
    }

    /// <summary>
    /// Gets all the triples with a given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
    {
        return WithNodeAndTriple(obj, new Triple(subj, _predVar, obj), _os);
    }

    /// <summary>
    /// Gets all the triples with a given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
    {
        return WithNodeAndTriple(subj, new Triple(subj, pred, _objVar), _sp);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithObject(INode obj)
    {
        return WithNode(obj, _qos);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithPredicate(INode pred)
    {
        return WithNode(pred, _qpo);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithSubject(INode subj)
    {
        return WithNode(subj, _qsp);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithPredicateObject(INode pred, INode obj)
    {
        return WithNodeAndTriple(pred, new Triple(_subjVar, pred, obj), _qpo);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithSubjectObject(INode subj, INode obj)
    {
        return WithNodeAndTriple(obj, new Triple(subj, _predVar, obj), _qos);
    }

    /// <inheritdoc/>
    public override IEnumerable<Triple> QuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return WithNodeAndTriple(subj, new Triple(subj, pred, _objVar), _qsp);
    }

    /// <inheritdoc/>
    public override IEnumerable<INode> ObjectNodes => _os.Keys;

    /// <inheritdoc/>
    public override IEnumerable<INode> PredicateNodes => _po.Keys;
    
    /// <inheritdoc/>
    public override IEnumerable<INode> SubjectNodes => _sp.Keys;
    
    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedObjectNodes => _qos.Keys;
    
    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedPredicateNodes => _qpo.Keys;
    
    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedSubjectNodes => _qsp.Keys;

    private bool _isDisposed;

    /// <summary>
    /// Disposes of the collection.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (disposing)
            {
                Triples.Clear();
                _sp.Clear();
                _po.Clear();
                _os.Clear();
                _qos.Clear();
                _qpo.Clear();
                _qsp.Clear();
            }
        }

        base.Dispose(disposing);
    }
}
