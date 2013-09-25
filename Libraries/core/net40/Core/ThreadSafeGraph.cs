/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !NO_RWLOCK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Collections;

namespace VDS.RDF
{
    /// <summary>
    /// A Thread Safe version of the <see cref="Graph">Graph</see> class
    /// </summary>
    /// <threadsafety instance="true">Should be safe for almost any concurrent read and write access scenario, internally managed using a <see cref="ReaderWriterLockSlim">ReaderWriterLockSlim</see>.  If you encounter any sort of Threading/Concurrency issue please report to the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">dotNetRDF Bugs Mailing List</a></threadsafety>
    /// <remarks>Performance will be marginally worse than a normal <see cref="Graph">Graph</see> but in multi-threaded scenarios this will likely be offset by the benefits of multi-threading.</remarks>
    public class ThreadSafeGraph
        : Graph
    {
        /// <summary>
        /// Locking Manager for the Graph
        /// </summary>
        protected ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Creates a new Thread Safe Graph
        /// </summary>
        public ThreadSafeGraph()
            : this(new ThreadSafeTripleCollection(new TreeIndexedTripleCollection())) { }

        /// <summary>
        /// Creates a new Thread Safe graph using the given Triple Collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public ThreadSafeGraph(BaseTripleCollection tripleCollection)
            : base(new ThreadSafeTripleCollection(tripleCollection)) { }

        /// <summary>
        /// Creates a new Thread Safe graph using a Thread Safe triple collection
        /// </summary>
        /// <param name="tripleCollection">Thread Safe triple collection</param>
        public ThreadSafeGraph(ThreadSafeTripleCollection tripleCollection)
            : base(tripleCollection) { }

        #region Triple Assertion and Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public override bool Assert(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                return base.Assert(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override bool Assert(IEnumerable<Triple> ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                return base.Assert(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public override bool Retract(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                return base.Retract(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override bool Retract(IEnumerable<Triple> ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                return base.Retract(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        #endregion

        /// <summary>
        /// Disposes of a Graph
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this._lockManager.Dispose();
        }

        public override IEnumerable<Triple> Find(INode s, INode p, INode o)
        {
            try
            {
                this._lockManager.EnterReadLock();
                return base.Find(s, p, o).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets the nodes that are used as vertices in the graph i.e. those which occur in the subject or object position of a triple
        /// </summary>
        public override IEnumerable<INode> Vertices
        {
            get
            {
                try
                {
                    this._lockManager.EnterReadLock();
                    return base.Vertices.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();                    
                }
            }
        }

        /// <summary>
        /// Gets the nodes that are used as edges in the graph i.e. those which occur in the predicate position of a triple
        /// </summary>
        public override IEnumerable<INode> Edges
        {
            get
            {
                try
                {
                    this._lockManager.EnterReadLock();
                    return base.Edges.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
            }
        }
    }

    /// <summary>
    /// A Thread Safe version of the <see cref="Graph">Graph</see> class
    /// </summary>
    /// <threadsafety instance="true">Should be safe for almost any concurrent read and write access scenario, internally managed using a <see cref="ReaderWriterLockSlim">ReaderWriterLockSlim</see>.  If you encounter any sort of Threading/Concurrency issue please report to the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">dotNetRDF Bugs Mailing List</a></threadsafety>
    /// <remarks>
    /// <para>
    /// Performance will be marginally worse than a normal <see cref="Graph">Graph</see> but in multi-threaded scenarios this will likely be offset by the benefits of multi-threading.
    /// </para>
    /// <para>
    /// Since this is a non-indexed version load performance will be better but query performance better
    /// </para>
    /// </remarks>
    public class NonIndexedThreadSafeGraph
        : ThreadSafeGraph
    {
        /// <summary>
        /// Creates a new non-indexed Thread Safe Graph
        /// </summary>
        public NonIndexedThreadSafeGraph()
            : base(new ThreadSafeTripleCollection()) { }
    }
}

#endif