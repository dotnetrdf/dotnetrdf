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
using System.Linq;
using VDS.Common.Collections;

namespace VDS.RDF;

/// <summary>
/// Basic Triple Collection which is not indexed.
/// </summary>
public class TripleCollection 
    : BaseTripleCollection, IEnumerable<Triple>
{
    /// <summary>
    /// Underlying Storage of the Triple Collection.
    /// </summary>
    protected readonly MultiDictionary<Triple, TripleRefs> Triples = new MultiDictionary<Triple, TripleRefs>(new FullTripleComparer(new FastVirtualNodeComparer()));

    private int _assertedCount, _quotedCount;

    /// <inheritdoc />
    public override bool Contains(Triple t)
    {
        return Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted;
    }

    /// <inheritdoc/>
    public override bool ContainsQuoted(Triple t)
    {
        return Triples.TryGetValue(t, out TripleRefs refs) && refs.QuoteCount > 0;
    }

    /// <inheritdoc />
    protected internal override bool Add(Triple t)
    {
        if (Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted)
        {
            return false;
        }

        if (refs == null)
        {
            // Triple has not been quoted or asserted
            refs = new TripleRefs { Asserted = true };
            Triples.Add(t, refs);
        }
        else
        {
            // Triple has been quoted before but hasn't been asserted
            refs.Asserted = true;
        }

        _assertedCount++;
        RaiseTripleAdded(t);
        if (t.Subject is ITripleNode stn) AddQuoted(stn);
        if (t.Object is ITripleNode otn) AddQuoted(otn);
        return true;
    }

    /// <summary>
    /// Adds a quotation of a triple to the collection.
    /// </summary>
    /// <param name="tripleNode">The triple node that quotes the triple to be added to the collection.</param>
    protected internal void AddQuoted(ITripleNode tripleNode)
    {
        if (Triples.TryGetValue(tripleNode.Triple, out TripleRefs refs))
        {
            refs.QuoteCount++;
        }
        else
        {
            Triples.Add(tripleNode.Triple, new TripleRefs{Asserted = false, QuoteCount = 1});
            _quotedCount++;
        }
        // Recursively process any nested quotations
        if (tripleNode.Triple.Subject is ITripleNode stn) AddQuoted(stn);
        if (tripleNode.Triple.Object is ITripleNode otn) AddQuoted(otn);
    }

    /// <summary>
    /// Deletes a Triple from the Collection.
    /// </summary>
    /// <param name="t">Triple to remove.</param>
    /// <remarks>
    /// Only asserted triples can be deleted. Deleting something that doesn't exist, or a triple that is only quoted has no effect and gives no error.
    /// </remarks>
    protected internal override bool Delete(Triple t)
    {
        if (Triples.TryGetValue(t, out TripleRefs refs) && refs.Asserted)
        {
            refs.Asserted = false;
            if (refs.QuoteCount == 0)
            {
                Triples.Remove(t);
            }

            _assertedCount--;
            RaiseTripleRemoved(t);
            if (t.Subject is ITripleNode stn) RemoveQuoted(stn);
            if (t.Object is ITripleNode otn) RemoveQuoted(otn);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a quote reference to a triple from the collection.
    /// </summary>
    /// <param name="tripleNode">The node containing the triple to be 'unquoted'.</param>
    /// <remarks>This method decreases the quote reference count for the triple. If the reference count drops to 0 and the triple is not also asserted in the graph, then it will be removed from the collection. If the triple itself quotes other triples, then this process is applied out recursively.</remarks>
    protected internal void RemoveQuoted(ITripleNode tripleNode)
    {
        if (Triples.TryGetValue(tripleNode.Triple, out TripleRefs refs))
        {
            refs.QuoteCount--;
            if (refs.QuoteCount == 0 && !refs.Asserted)
            {
                Triples.Remove(tripleNode.Triple);
                _quotedCount--;
            }
        }

        // Recursively remove any nested quotations
        if (tripleNode.Triple.Subject is ITripleNode stn) RemoveQuoted(stn);
        if (tripleNode.Triple.Object is ITripleNode otn) RemoveQuoted(otn);
    }


    /// <inheritdoc />
    public override int Count => _assertedCount;

    /// <inheritdoc />
    public override int QuotedCount => _quotedCount;

    /// <summary>
    /// Gets the given Triple.
    /// </summary>
    /// <param name="t">Triple to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given Triple does not exist in the Triple Collection.</exception>
    public override Triple this[Triple t]
    {
        get
        {
            if (Triples.TryGetKey(t, out Triple actual))
            {
                return actual;
            }

            throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
        }
    }

    /// <inheritdoc />
    public override IEnumerable<INode> ObjectNodes
    {
        get => Asserted.Select(t => t.Object).Distinct();
    }

    /// <inheritdoc />
    public override IEnumerable<INode> PredicateNodes
    {
        get => Asserted.Select(t => t.Predicate).Distinct();
    }

    /// <inheritdoc />
    public override IEnumerable<INode> SubjectNodes
    {
        get => Asserted.Select(t => t.Subject).Distinct();
    }

    /// <inheritdoc />
    public override IEnumerable<INode> QuotedObjectNodes
    {
        get => Quoted.Select(t => t.Object).Distinct();
    }

    /// <inheritdoc />
    public override IEnumerable<INode> QuotedPredicateNodes
    {
        get => Quoted.Select(t=>t.Predicate).Distinct();
    }

    /// <inheritdoc />
    public override IEnumerable<INode> QuotedSubjectNodes
    {
        get => Quoted.Select(t => t.Subject).Distinct();
    }

    #region IEnumerable<Triple> Members

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

    /// <inheritdoc />
    public override IEnumerable<Triple> Asserted {
        get
        {
            foreach (KeyValuePair<Triple, TripleRefs> x in Triples)
            {
                if (x.Value.Asserted) yield return x.Key;
            }
        }
    }

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<Triple> GetEnumerator()
    {
        return Asserted.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region IDisposable Members

    private bool _isDisposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (disposing)
            {
                Triples.Clear();
            }
        }
        base.Dispose(disposing);
    }

    #endregion
}
