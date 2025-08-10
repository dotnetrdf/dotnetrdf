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

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for Triple Collections.
/// </summary>
/// <remarks>
/// Designed to allow the underlying storage of a Triple Collection to be changed at a later date without affecting classes that use it.
/// </remarks>
public abstract class BaseTripleCollection
    : IEnumerable<Triple>, IDisposable
{
    private bool _isDisposed = false;
    
    /// <summary>
    /// Adds a Triple to the Collection.
    /// </summary>
    /// <param name="t">Triple to add.</param>
    /// <remarks>Adding a Triple that already exists should be permitted though it is not necessary to persist the duplicate to underlying storage.</remarks>
    protected abstract internal bool Add(Triple t);

    /// <summary>
    /// Determines whether a given Triple is asserted in the Triple Collection.
    /// </summary>
    /// <param name="t">The Triple to test.</param>
    /// <returns>True if the triple exists as an asserted triple in the collection.</returns>
    public abstract bool Contains(Triple t);

    /// <summary>
    /// Determines whether a given triple is quoted by a triple node of a triple in the collection.
    /// </summary>
    /// <param name="t">The triple to test.</param>
    /// <returns>True if the triple is quoted in the triple collection, false otherwise.</returns>
    public abstract bool ContainsQuoted(Triple t);

    /// <summary>
    /// Gets the number of asserted triples in the triple collection.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the number of quoted triples in the triple collection.
    /// </summary>
    public abstract int QuotedCount { get; }

    /// <summary>
    /// Deletes an asserted triple from the collection.
    /// </summary>
    /// <param name="t">Triple to remove.</param>
    /// <returns>True if the operation removed an asserted triple from the collection, false otherwise.</returns>
    /// <remarks>
    /// Deleting a triple that is not asserted in the collection should have no effect and give no error.
    /// Quoted triples cannot be removed from the collection via this method - instead they should be automatically removed when
    /// all asserted triples that reference them are removed.
    /// Deleting a triple that is both asserted and quoted will remove the assertion of the triple (and return true), but
    /// the triple will remain in the collection as a quoted triples.
    /// </remarks>
    protected abstract internal bool Delete(Triple t);

    /// <summary>
    /// Gets the given triple.
    /// </summary>
    /// <param name="t">Triple to retrieve.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given Triple doesn't exist.</exception>
    public abstract Triple this[Triple t] { get; }

    /// <summary>
    /// Gets the triples that match the provided pattern.
    /// </summary>
    /// <param name="pattern">A tuple of subject, predicate and object node. Each item in the tuple is nullable, with a null value indicating a wildcard for matching in that position.</param>
    public virtual IEnumerable<Triple> this[(INode s, INode p, INode o) pattern]
    {
        get
        {
            if (pattern.s == null)
            {
                if (pattern.p == null)
                {
                    return pattern.o == null ? this : this.WithObject(pattern.o);
                }

                return pattern.o == null
                    ? this.WithPredicate(pattern.p)
                    : this.WithPredicateObject(pattern.p, pattern.o);
            }

            if (pattern.p == null)
            {
                return pattern.o == null
                    ? this.WithSubject(pattern.s)
                    : this.WithSubjectObject(pattern.s, pattern.o);
            }

            if (pattern.o == null) return this.WithSubjectPredicate(pattern.s, pattern.p);
            try
            {
                Triple t = this[new Triple(pattern.s, pattern.p, pattern.o)];
                return t.AsEnumerable();
            }
            catch (KeyNotFoundException)
            {
                return Enumerable.Empty<Triple>();
            }
        }
    }

    /// <summary>
    /// Gets all the nodes which are objects of asserted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> ObjectNodes { get; }

    /// <summary>
    /// Gets all the nodes which are predicates of asserted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> PredicateNodes { get; }

    /// <summary>
    /// Gets all the nodes which are subjects of asserted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> SubjectNodes { get; }

    /// <summary>
    /// Gets all the nodes which are subjects of quoted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> QuotedObjectNodes { get; }
    /// <summary>
    /// Gets all the nodes which are predicates of quoted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> QuotedPredicateNodes { get; }
    /// <summary>
    /// Gets all the nodes which are objects of quoted triples in the triple collection.
    /// </summary>
    public abstract IEnumerable<INode> QuotedSubjectNodes { get; }

    /// <summary>
    /// Gets all the asserted triples with the given subject.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithSubject(INode subj)
    {
        return this.Where(t => t.Subject.Equals(subj));
    }

    /// <summary>
    /// Gets all quoted triples with the given subject.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithSubject(INode subj)
    {
        return Quoted.Where(t => t.Subject.Equals(subj));
    }

    /// <summary>
    /// Gets all the asserted triples with the given predicate.
    /// </summary>
    /// <param name="pred">Predicate to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithPredicate(INode pred)
    {
        return this.Where(t => t.Predicate.Equals(pred));
    }

    /// <summary>
    /// Gets all the quoted triples with the given Predicate.
    /// </summary>
    /// <param name="pred">Predicate to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithPredicate(INode pred)
    {
        return Quoted.Where(t => t.Predicate.Equals(pred));
    }

    /// <summary>
    /// Gets all the asserted triples with the given object.
    /// </summary>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithObject(INode obj)
    {
        return this.Where(t => t.Object.Equals(obj));
    }

    /// <summary>
    /// Gets all the quoted triples with the given Object.
    /// </summary>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithObject(INode obj)
    {
        return Quoted.Where(t => t.Object.Equals(obj));
    }

    /// <summary>
    /// Gets all the asserted triples with the given subject/predicate pair.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <param name="pred">Predicate to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
    {
        return this.Where(t => t.Subject.Equals(subj) && t.Predicate.Equals(pred));
    }


    /// <summary>
    /// Gets all the quoted triples with the given Subject Predicate pair.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <param name="pred">Predicate to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return Quoted.Where(t => t.Subject.Equals(subj) && t.Predicate.Equals(pred));
    }

    /// <summary>
    /// Gets all the Triples with the given Predicate Object pair.
    /// </summary>
    /// <param name="pred">Predicate to lookup.</param>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
    {
        return this.Where(t => t.Object.Equals(obj) && t.Predicate.Equals(pred));
    }

    /// <summary>
    /// Gets all the quoted triples with the given Predicate Object pair.
    /// </summary>
    /// <param name="pred">Predicate to lookup.</param>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithPredicateObject(INode pred, INode obj)
    {
        return Quoted.Where(t => t.Object.Equals(obj) && t.Predicate.Equals(pred));
    }

    /// <summary>
    /// Gets all the Triples with the given Subject Object pair.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
    {
        return this.Where(t => t.Subject.Equals(subj) && t.Object.Equals(obj));
    }

    /// <summary>
    /// Gets all the quoted triples with the given Subject Object pair.
    /// </summary>
    /// <param name="subj">Subject to lookup.</param>
    /// <param name="obj">Object to lookup.</param>
    /// <returns></returns>
    public virtual IEnumerable<Triple> QuotedWithSubjectObject(INode subj, INode obj)
    {
        return Quoted.Where(t => t.Subject.Equals(subj) && t.Object.Equals(obj));
    }

    /// <summary>
    /// Disposes of a Triple Collection.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of a Triple Collection.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Gets the typed Enumerator for the Triple Collection.
    /// </summary>
    /// <returns>An enumerator over the asserted triples in the triple collection.</returns>
    public abstract IEnumerator<Triple> GetEnumerator();

    /// <summary>
    /// Gets the triples that are asserted in the collection.
    /// </summary>
    /// <remarks>This returns the same set of triples as the enumerator on this class, but as an <see cref="IEnumerable{Triple}"/> instance.</remarks>
    public abstract IEnumerable<Triple> Asserted { get; }

    /// <summary>
    /// Gets the triples that are quoted in the collection.
    /// </summary>
    public abstract IEnumerable<Triple> Quoted { get; }

    /// <summary>
    /// Gets the non-generic Enumerator for the Triple Collection.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return Enumerable.Empty<Triple>().GetEnumerator();
    }

    /// <summary>
    /// Event which occurs when a triple is added to the collection as an asserted triple.
    /// </summary>
    public event TripleEventHandler TripleAdded;

    /// <summary>
    /// Event which occurs when a triple is un-asserted from the collection.
    /// </summary>
    public event TripleEventHandler TripleRemoved;

    /// <summary>
    /// Helper method for raising the <see cref="TripleAdded">Triple Added</see> event.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected void RaiseTripleAdded(Triple t)
    {
        TripleAdded?.Invoke(this, new TripleEventArgs(t, null));
    }

    /// <summary>
    /// Helper method for raising the <see cref="TripleRemoved">Triple Removed</see> event.
    /// </summary>
    /// <param name="t">Triple.</param>
    protected void RaiseTripleRemoved(Triple t)
    {
        TripleRemoved?.Invoke(this, new TripleEventArgs(t, null, false));
    }
}
