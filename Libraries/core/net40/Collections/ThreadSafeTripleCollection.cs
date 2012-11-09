using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// Thread Safe decorator for triple collections
    /// </summary>
    /// <remarks>
    /// Depending on the platform this either uses <see cref="ReaderWriterLockSlim"/> to provide MRSW concurrency or it uses <see cref="Monitor"/> to provide exclusive access concurrency, either way usage is thread safe
    /// </remarks>
    /// <threadsafety instance="true">This decorator provides thread safe access to any underlying triple collection</threadsafety>
    public class ThreadSafeTripleCollection
        : WrapperTripleCollection
    {
#if !NO_RWLOCK
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
#endif

        /// <summary>
        /// Creates a new thread safe triple collection which wraps a new instance of the default unindexed <see cref="TripleCollection"/>
        /// </summary>
        public ThreadSafeTripleCollection()
            : base(new TripleCollection()) { }

        /// <summary>
        /// Creates a new thread safe triple collection which wraps the provided triple collection
        /// </summary>
        /// <param name="tripleCollection">Triple Collection</param>
        public ThreadSafeTripleCollection(BaseTripleCollection tripleCollection)
            : base(tripleCollection) { }

        /// <summary>
        /// Enters the write lock
        /// </summary>
        protected void EnterWriteLock()
        {
#if !NO_RWLOCK
            this._lockManager.EnterWriteLock();
#else
            Monitor.Enter(this._triples);
#endif
        }

        /// <summary>
        /// Exists the write lock
        /// </summary>
        protected void ExitWriteLock()
        {
#if !NO_RWLOCK
            this._lockManager.ExitWriteLock();
#else
            Monitor.Exit(this._triples);
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
            Monitor.Enter(this._triples);
#endif
        }

        /// <summary>
        /// Exists the read lock
        /// </summary>
        protected void ExitReadLock()
        {
#if !NO_RWLOCK
            this._lockManager.ExitReadLock();
#else
            Monitor.Exit(this._triples);
#endif
        }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        public override bool Add(Triple t)
        {
            try
            {
                this.EnterWriteLock();
                return this._triples.Add(t); ;
            }
            finally
            {
                this.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public override bool Contains(Triple t)
        {
            bool contains = false;
            try
            {
                this.EnterReadLock();
                contains = this._triples.Contains(t);
            }
            finally
            {
                this.ExitReadLock();
            }
            return contains;
        }

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public override long Count
        {
            get
            {
                long c = 0;
                try
                {
                    this.EnterReadLock();
                    c = this._triples.Count;
                }
                finally
                {
                    this.ExitReadLock();
                }
                return c;
            }
        }

        /// <summary>
        /// Deletes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        public override bool Remove(Triple t)
        {
            try
            {
                this.EnterWriteLock();
                return this._triples.Remove(t);
            }
            finally
            {
                this.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples.GetEnumerator();
        }

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collectio
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get
            {
                List<INode> nodes = new List<INode>();
                try
                {
                    this.EnterReadLock();
                    nodes = this._triples.ObjectNodes.ToList();
                }
                finally
                {
                    this.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                List<INode> nodes = new List<INode>();
                try
                {
                    this.EnterReadLock();
                    nodes = this._triples.PredicateNodes.ToList();
                }
                finally
                {
                    this.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                List<INode> nodes = new List<INode>();
                try
                {
                    this.EnterReadLock();
                    nodes = this._triples.SubjectNodes.ToList();
                }
                finally
                {
                    this.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all triples with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithObject(obj).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all triples with the given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithPredicate(pred).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all triples with the given predicate object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithPredicateObject(pred, obj).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the triples with the given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithSubject(subj).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the triples with the given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithSubjectObject(subj, obj).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all triples with the given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this.EnterReadLock();
                triples = this._triples.WithSubjectPredicate(subj, pred).ToList();
            }
            finally
            {
                this.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Disposes of a Triple Collection
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this.EnterWriteLock();
                this._triples.Dispose();
            }
            finally
            {
                this.ExitWriteLock();
            }
        }
    }
}
