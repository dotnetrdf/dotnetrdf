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
    /// Dependings on your platform this either provides MRSW concurrency via a <see cref="ReaderWriterLockSlim" /> or exclusive access concurrency via a <see cref="Monitor"/>
    /// </remarks>
    public class ThreadSafeGraphCollection
        : WrapperGraphCollection
    {
#if !NO_RWLOCK
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
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
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool ContainsKey(Uri graphUri)
        {
            bool contains = false;

            try
            {
                this.EnterReadLock();
                contains = this._graphs.ContainsKey(graphUri);
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
        /// <param name="g">Graph to add</param>
        public override void Add(Uri graphUri, IGraph g)
        {
            try
            {
                this.EnterWriteLock();
                return this._graphs.Add(g);
            }
            finally
            {
                this.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        public override bool Remove(Uri graphUri)
        {
            try
            {
                this.EnterWriteLock();
                return this._graphs.Remove(graphUri);
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
                int c = 0;
                try
                {
                    this.EnterReadLock();
                    c = this._graphs.Count;
                }
                finally
                {
                    this.ExitReadLock();
                }
                return c;
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<KeyValuePair<Uri, IGraph>> GetEnumerator()
        {
            List<KeyValuePair<Uri, IGraph>> graphs = new List<KeyValuePair<Uri, IGraph>>();
            try
            {
                this.EnterReadLock();
                graphs = this._graphs.ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return graphs.GetEnumerator();
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public override ICollection<Uri> Keys
        {
            get
            {
                List<Uri> uris = new List<Uri>();
                try
                {
                    this.EnterReadLock();
                    uris = this._graphs.Keys.ToList();
                }
                finally
                {
                    this.ExitReadLock();
                }
                return uris;
            }
        }

        /// <summary>
        /// Gets a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get
            {
                IGraph g = null;
                try
                {
                    this.EnterReadLock();
                    g = this._graphs[graphUri];
                }
                finally
                {
                    this.ExitReadLock();
                }
                return g;
            }
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <see cref="IGraph.Dipose">Dispose()</see> method of all Graphs contained in the Collection</remarks>
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
