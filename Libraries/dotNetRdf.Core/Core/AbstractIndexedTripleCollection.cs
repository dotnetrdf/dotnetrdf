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

using System.Collections.Generic;
using VDS.Common.Collections;

namespace VDS.RDF;

/// <summary>
/// An abstract base class for triple collections with a lookup index.
/// </summary>
public abstract class AbstractIndexedTripleCollection : BaseTripleCollection
{
    /// <summary>
    /// The triples asserted and quoted in the triple collection.
    /// </summary>
    protected readonly MultiDictionary<Triple, TripleRefs> Triples = new MultiDictionary<Triple, TripleRefs>(new FullTripleComparer(new FastVirtualNodeComparer()));

    private int _assertedCount, _quotedCount;

    /// <inheritdoc />
    public override int Count => _assertedCount;

    /// <inheritdoc />
    public override int QuotedCount => _quotedCount;

    /// <inheritdoc />
    public override bool Contains(Triple t)
    {
        return Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted;
    }

    /// <inheritdoc />
    public override bool ContainsQuoted(Triple t)
    {
        return Triples.TryGetValue(t, out TripleRefs refs) && refs.QuoteCount > 0;
    }

    /// <inheritdoc />
    public override IEnumerator<Triple> GetEnumerator()
    {
        return Asserted.GetEnumerator();
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> Asserted
    {
        get
        {
            foreach (KeyValuePair<Triple, TripleRefs> x in Triples)
            {
                if (x.Value.Asserted) yield return x.Key;
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> Quoted
    {
        get
        {
            foreach (KeyValuePair<Triple, TripleRefs> x in Triples)
            {
                if (x.Value.QuoteCount > 0) yield return x.Key;
            }
        }
    }

    /// <summary>
    /// Adds a Triple to the collection.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected internal override bool Add(Triple t)
    {
        if (Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted)
        {
            return false;
        }

        if (refs == null)
        {
            refs = new TripleRefs { Asserted = true };
            Triples.Add(t, refs);
        }
        else
        {
            refs.Asserted = true;
        }

        _assertedCount++;
        IndexAsserted(t);
        if (t.Subject is ITripleNode stn) AddQuoted(stn);
        if (t.Object is ITripleNode otn) AddQuoted(otn);
        return true;
    }

    /// <summary>
    /// Override in derived classes to add an asserted triple to the index.
    /// </summary>
    /// <param name="t">Triple to add.</param>
    protected abstract void IndexAsserted(Triple t);

    /// <summary>
    /// Override in derived classes to remove an asserted triple from the index.
    /// </summary>
    /// <param name="t">Triple to remove.</param>
    protected abstract void UnindexAsserted(Triple t);

    /// <summary>
    /// Override in derived classes to add a quoted triple to the index.
    /// </summary>
    /// <param name="t">Triple to add.</param>
    protected abstract void IndexQuoted(Triple t);

    /// <summary>
    /// Override in derived classes to remove a quoted triple from the index.
    /// </summary>
    /// <param name="t"></param>
    protected abstract void UnindexQuoted(Triple t);

    /// <summary>
    /// Deletes a triple from the collection.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected internal override bool Delete(Triple t)
    {
        if (Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted)
        {
            refs.Asserted = false;
            if (refs.QuoteCount == 0)
            {
                // If removed then decrement count
                Triples.Remove(t);
            }

            _assertedCount--;
            UnindexAsserted(t);
            // TripleRemoved event is raised when the triple is retracted from the graph. It may still be quoted.
            RaiseTripleRemoved(t);

            if (t.Subject is ITripleNode stn) RemoveQuoted(stn);
            if (t.Object is ITripleNode otn) RemoveQuoted(otn);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Adds a quotation of a triple to the collection.
    /// </summary>
    /// <param name="tripleNode">The triple node that quotes the triple to be added to the collection.</param>
    protected internal void AddQuoted(ITripleNode tripleNode)
    {
        if (Triples.TryGetValue(tripleNode.Triple, out TripleRefs refs))
        {
            if (refs.QuoteCount == 0)
            {
                // Triple was previously asserted but not quoted so add it to the quoted triples index.
                IndexQuoted(tripleNode.Triple);
                _quotedCount++;
            }
            refs.QuoteCount++;
            return;
        }
        // New (not previously asserted or quoted) triple
        Triples.Add(tripleNode.Triple, new TripleRefs { Asserted = false, QuoteCount = 1 });
        IndexQuoted(tripleNode.Triple);
        _quotedCount++;

        // Recursively process any nested quotations
        if (tripleNode.Triple.Subject is ITripleNode stn) AddQuoted(stn);
        if (tripleNode.Triple.Object is ITripleNode otn) AddQuoted(otn);
    }

    /// <summary>
    /// Removes a quote reference to a triple from the collection.
    /// </summary>
    /// <param name="tripleNode">The node containing the triple to be 'unquoted'.</param>
    /// <remarks>This method decreases the quote reference count for the triple.
    /// If the reference count drops to 0, the triple is removed from the quoted triples index.
    /// If the reference count drops to 0 and the triple is not asserted in the graph, it will be removed from the collection count.
    /// If the triple itself quotes other triples, then this process is applied out recursively.
    /// </remarks>
    protected internal void RemoveQuoted(ITripleNode tripleNode)
    {
        if (Triples.TryGetValue(tripleNode.Triple, out TripleRefs refs) && refs.QuoteCount > 0)
        {
            refs.QuoteCount--;
            if (refs.QuoteCount == 0)
            {
                _quotedCount--;
                UnindexQuoted(tripleNode.Triple);
            }
        }
        // Recursively remove any nested quotations
        if (tripleNode.Triple.Subject is ITripleNode stn) RemoveQuoted(stn);
        if (tripleNode.Triple.Object is ITripleNode otn) RemoveQuoted(otn);
    }

}