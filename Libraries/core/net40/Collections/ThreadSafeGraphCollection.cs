/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Collections
{

    /// <summary>
    /// Thread Safe decorator around a Graph collection
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dependings on your platform this either provides MRSW concurrency via a <see cref="ReaderWriterLockSlim" /> or exclusive access concurrency via a <see cref="Monitor"/>
    /// </para>
    /// <para>
    /// Thread safety is only guaranteed for graph collection level operations, it is still possible for other code to modify the stored graphs in a non-thread safe way.  This can be avoided if all graphs in the collection are <see cref="ThreadSafeGraph"/> instances.
    /// </para>
    /// </remarks>
    public class ThreadSafeGraphCollection
        : WrapperGraphCollection
    {
#if !NO_RWLOCK
        private readonly ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
#endif

        /// <summary>
        /// Creates a new Thread Safe decorator around the default <see cref="GraphCollection"/>
        /// </summary>
        public ThreadSafeGraphCollection()
            : base(new GraphCollection()) { }

        /// <summary>
        /// Creates a new Thread Safe decorator around the supplied graph collection
        /// </summary>
        /// <param name="graphCollection">Graph Collection</param>
        public ThreadSafeGraphCollection(BaseGraphCollection graphCollection)
            : base(graphCollection) { }

        /// <summary>
        /// Enters the write lock
        /// </summary>
        protected void EnterWriteLock()
        {
#if !NO_RWLOCK
            this._lockManager.EnterWriteLock();
#else
            Monitor.Enter(this._graphs);
#endif
        }

        /// <summary>
        /// Exits the write lock
        /// </summary>
        protected void ExitWriteLock()
        {
#if !NO_RWLOCK
            this._lockManager.ExitWriteLock();
#else
            Monitor.Exit(this._graphs);
#endif
        }

        /// <summary>
        /// Enters the read lock
        /// </summary>
        protected void EnterReadLock()
        {
#if !NO_RWLOCK
            this._lockManager.EnterReadLock();
#else
            Monitor.Enter(this._graphs);
#endif
        }

        /// <summary>
        /// Exits the read lock
        /// </summary>
        protected void ExitReadLock()
        {
#if !NO_RWLOCK
            this._lockManager.ExitReadLock();
#else
            Monitor.Exit(this._graphs);
#endif
        }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphName">Graph name to test</param>
        /// <returns></returns>
        public override bool ContainsKey(INode graphName)
        {
            bool contains = false;

            try
            {
                this.EnterReadLock();
                contains = this._graphs.ContainsKey(graphName);
            }
            finally
            {
                this.ExitReadLock();
            }
            return contains;
        }

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="graphName">Name of the graph to add to</param>
        /// <param name="g">Graph to add</param>
        public override void Add(INode graphName, IGraph g)
        {
            try
            {
                this.EnterWriteLock();
                this._graphs.Add(graphName, g);
            }
            finally
            {
                this.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphName">Name of the Graph to remove</param>
        public override bool Remove(INode graphName)
        {
            try
            {
                this.EnterWriteLock();
                return this._graphs.Remove(graphName);
            }
            finally
            {
                this.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public override int Count
        {
            get
            {
                try
                {
                    this.EnterReadLock();
                    return this._graphs.Count;
                }
                finally
                {
                    this.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<KeyValuePair<INode, IGraph>> GetEnumerator()
        {
            try
            {
                this.EnterReadLock();
                return this._graphs.ToList().GetEnumerator();
            }
            finally
            {
                this.ExitReadLock();
            }
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public override ICollection<INode> Keys
        {
            get
            {
                try
                {
                    this.EnterReadLock();
                    return this._graphs.Keys.ToList();
                }
                finally
                {
                    this.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets a Graph from the Collection
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns></returns>
        public override IGraph this[INode graphName]
        {
            get
            {
                try
                {
                    this.EnterReadLock();
                    return this._graphs[graphName];
                }
                finally
                {
                    this.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    this.EnterWriteLock();
                    this.Add(graphName, value);
                }
                finally
                {
                    this.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <see cref="IGraph.Dispose">Dispose()</see> method of all Graphs contained in the Collection</remarks>
        public override void Dispose()
        {
            try
            {
                this.EnterWriteLock();
                this._graphs.Dispose();
            }
            finally
            {
                this.ExitWriteLock();
            }
        }
    }
}
