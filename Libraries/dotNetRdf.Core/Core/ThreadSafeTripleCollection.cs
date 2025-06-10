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
using System.Threading;

namespace VDS.RDF;

/// <summary>
/// Thread Safe decorator for triple collections.
/// </summary>
/// <remarks>
/// Depending on the platform this either uses <see cref="ReaderWriterLockSlim"/> to provide MRSW concurrency or it uses <see cref="Monitor"/> to provide exclusive access concurrency, either way usage is thread safe.
/// </remarks>
/// <threadsafety instance="true">This decorator provides thread safe access to any underlying triple collection.</threadsafety>
public class ThreadSafeTripleCollection 
    : WrapperTripleCollection
{
    private readonly ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Creates a new thread safe triple collection which wraps a new instance of the default unindexed <see cref="TripleCollection"/>.
    /// </summary>
    public ThreadSafeTripleCollection()
        : base(new TripleCollection()) { }

    /// <summary>
    /// Creates a new thread safe triple collection which wraps the provided triple collection.
    /// </summary>
    /// <param name="tripleCollection">Triple Collection.</param>
    public ThreadSafeTripleCollection(BaseTripleCollection tripleCollection)
        : base(tripleCollection) { }

    /// <summary>
    /// Enters the write lock.
    /// </summary>
    protected void EnterWriteLock()
    {
        _lockManager.EnterWriteLock();
    }

    /// <summary>
    /// Exists the write lock.
    /// </summary>
    protected void ExitWriteLock()
    {
        _lockManager.ExitWriteLock();
    }

    /// <summary>
    /// Enters the read lock.
    /// </summary>
    protected void EnterReadLock()
    {
        _lockManager.EnterReadLock();
    }

    /// <summary>
    /// Exists the read lock.
    /// </summary>
    protected void ExitReadLock()
    {
        _lockManager.ExitReadLock();
    }

    /// <summary>
    /// Adds a Triple to the Collection.
    /// </summary>
    /// <param name="t">Triple to add.</param>
    protected internal override bool Add(Triple t)
    {
        try
        {
            EnterWriteLock();
            return _triples.Add(t); ;
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <summary>
    /// Determines whether a given Triple is in the Triple Collection.
    /// </summary>
    /// <param name="t">The Triple to test.</param>
    /// <returns>True if the Triple already exists in the Triple Collection.</returns>
    public override bool Contains(Triple t)
    {
        var contains = false;
        try
        {
            EnterReadLock();
            contains = _triples.Contains(t);
        }
        finally
        {
            ExitReadLock();
        }
        return contains;
    }

   /// <inheritdoc />
    public override bool ContainsQuoted(Triple t)
    {
        try
        {
            EnterReadLock();
            return _triples.ContainsQuoted(t);
        }
        finally
        {
            ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the Number of Triples in the Triple Collection.
    /// </summary>
    public override int Count
    {
        get
        {
            var c = 0;
            try
            {
                EnterReadLock();
                c = _triples.Count;
            }
            finally
            {
                ExitReadLock();
            }
            return c;
        }
    }

    /// <inheritdoc />
    public override int QuotedCount
    {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.QuotedCount;
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets the original instance of a specific Triple from the Triple Collection.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public override Triple this[Triple t]
    {
        get
        {
            Triple temp;
            try
            {
                EnterReadLock();
                temp = _triples[t];
            }
            finally
            {
                ExitReadLock();
            }
            if (temp == null) throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
            return temp;
        }
    }

    /// <summary>
    /// Deletes a Triple from the Collection.
    /// </summary>
    /// <param name="t">Triple to remove.</param>
    /// <remarks>Deleting something that doesn't exist has no effect and gives no error.</remarks>
    protected internal override bool Delete(Triple t)
    {
        try
        {
            EnterWriteLock();
            return _triples.Delete(t);
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<Triple> GetEnumerator()
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples.GetEnumerator();
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> Asserted
    {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.Asserted.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<Triple> Quoted
    {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.Quoted.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }


    /// <summary>
    /// Gets all the Nodes which are Objects of Triples in the Triple Collectio.
    /// </summary>
    public override IEnumerable<INode> ObjectNodes
    {
        get
        {
            var nodes = new List<INode>();
            try
            {
                EnterReadLock();
                nodes = _triples.ObjectNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
            return nodes;
        }
    }

    /// <summary>
    /// Gets all the Nodes which are Predicates of Triples in the Triple Collection.
    /// </summary>
    public override IEnumerable<INode> PredicateNodes
    {
        get
        {
            var nodes = new List<INode>();
            try
            {
                EnterReadLock();
                nodes = _triples.PredicateNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
            return nodes;
        }
    }

    /// <summary>
    /// Gets all the Nodes which are Subjects of Triples in the Triple Collection.
    /// </summary>
    public override IEnumerable<INode> SubjectNodes
    {
        get
        {
            var nodes = new List<INode>();
            try
            {
                EnterReadLock();
                nodes = _triples.SubjectNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
            return nodes;
        }
    }


    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedObjectNodes {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.QuotedObjectNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedPredicateNodes {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.QuotedPredicateNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerable<INode> QuotedSubjectNodes
    {
        get
        {
            try
            {
                EnterReadLock();
                return _triples.QuotedSubjectNodes.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets all triples with the given Object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithObject(INode obj)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithObject(obj).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    /// <summary>
    /// Gets all triples with the given predicate.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithPredicate(INode pred)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithPredicate(pred).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    /// <summary>
    /// Gets all triples with the given predicate object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithPredicateObject(pred, obj).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    /// <summary>
    /// Gets all the triples with the given subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubject(INode subj)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithSubject(subj).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    /// <summary>
    /// Gets all the triples with the given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithSubjectObject(subj, obj).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    /// <summary>
    /// Gets all triples with the given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
    {
        var triples = new List<Triple>();
        try
        {
            EnterReadLock();
            triples = _triples.WithSubjectPredicate(subj, pred).ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return triples;
    }

    private bool _isDisposed;
    /// <summary>
    /// Disposes of a Triple Collection.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (disposing)
            {
                try
                {
                    EnterWriteLock();
                    _triples.Dispose();
                }
                finally
                {
                    ExitWriteLock();
                }
            }
        }
        base.Dispose(disposing);
    }
}