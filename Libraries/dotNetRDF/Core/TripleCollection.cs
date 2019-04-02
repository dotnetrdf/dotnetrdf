/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
//
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.Common.Collections;

namespace VDS.RDF
{
    /// <summary>
    /// Basic Triple Collection which is not indexed
    /// </summary>
    public class TripleCollection
        : BaseTripleCollection, IEnumerable<Triple>
    {
        /// <summary>
        /// Underlying Storage of the Triple Collection
        /// </summary>
        protected readonly MultiDictionary<Triple, object> _triples = new MultiDictionary<Triple, object>(new FullTripleComparer(new FastVirtualNodeComparer()));

        /// <summary>
        /// The triples binned by subject for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, HashSet<Triple>> _triplesSubjects = new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer());

        /// <summary>
        /// The triples binned by predicates for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, HashSet<Triple>> _triplesPredicates = new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer());

        /// <summary>
        /// The triples binned by objects for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, HashSet<Triple>> _triplesObjects = new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer());

        /// <summary>
        /// The triples binned by subjects and predicates for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>> _triplesSubjectsPredicates = new MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>>(new FastVirtualNodeComparer());

        /// <summary>
        /// The triples binned by predicates and objects for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>> _triplesPredicatesObjects = new MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>>(new FastVirtualNodeComparer());

        /// <summary>
        /// The triples binned by subjects and objects for easy retrieval
        /// </summary>
        protected readonly MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>> _triplesSubjectsObjects = new MultiDictionary<INode, MultiDictionary<INode, HashSet<Triple>>>(new FastVirtualNodeComparer());


        /// <summary>
        /// Creates a new Triple Collection
        /// </summary>
        public TripleCollection() { }

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public override bool Contains(Triple t)
        {
            return _triples.ContainsKey(t);
        }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override bool Add(Triple t)
        {
            if (!Contains(t))
            {
                if (!_triplesSubjects.ContainsKey(t.Subject))
                {
                    _triplesSubjects.Add(t.Subject, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesSubjects[t.Subject].Add(t);
                if (!_triplesPredicates.ContainsKey(t.Predicate))
                {
                    _triplesPredicates.Add(t.Predicate, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesPredicates[t.Predicate].Add(t);
                if (!_triplesObjects.ContainsKey(t.Object))
                {
                    _triplesObjects.Add(t.Object, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesObjects[t.Object].Add(t);

                if (!_triplesSubjectsPredicates.ContainsKey(t.Subject))
                {
                    _triplesSubjectsPredicates.Add(t.Subject, new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer()));
                }

                if (!_triplesSubjectsPredicates[t.Subject].ContainsKey(t.Predicate))
                {
                    _triplesSubjectsPredicates[t.Subject].Add(t.Predicate, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesSubjectsPredicates[t.Subject][t.Predicate].Add(t);

                if (!_triplesPredicatesObjects.ContainsKey(t.Predicate))
                {
                    _triplesPredicatesObjects.Add(t.Predicate, new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer()));
                }

                if (!_triplesPredicatesObjects[t.Predicate].ContainsKey(t.Object))
                {
                    _triplesPredicatesObjects[t.Predicate].Add(t.Object, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesPredicatesObjects[t.Predicate][t.Object].Add(t);

                if (!_triplesSubjectsObjects.ContainsKey(t.Subject))
                {
                    _triplesSubjectsObjects.Add(t.Subject, new MultiDictionary<INode, HashSet<Triple>>(new FastVirtualNodeComparer()));
                }

                if (!_triplesSubjectsObjects[t.Subject].ContainsKey(t.Object))
                {
                    _triplesSubjectsObjects[t.Subject].Add(t.Object, new HashSet<Triple>(new FullTripleComparer(new FastVirtualNodeComparer())));
                }

                _triplesSubjectsObjects[t.Subject][t.Object].Add(t);

                _triples.Add(t, null);
                RaiseTripleAdded(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a Triple from the Colleciton
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        protected internal override bool Delete(Triple t)
        {
            if (_triples.Remove(t))
            {
                if (_triplesSubjects.ContainsKey(t.Subject))
                {
                    _triplesSubjects[t.Subject].Remove(t);
                    if (_triplesSubjects[t.Subject].Count == 0)
                    {
                        _triplesSubjects.Remove(t.Subject);
                    }
                }

                if (_triplesObjects.ContainsKey(t.Object))
                {
                    _triplesObjects[t.Object].Remove(t);
                    if (_triplesObjects[t.Object].Count == 0)
                    {
                        _triplesObjects.Remove(t.Object);
                    }
                }

                if (_triplesPredicates.ContainsKey(t.Predicate))
                {
                    _triplesPredicates[t.Predicate].Remove(t);
                    if (_triplesPredicates[t.Predicate].Count == 0)
                    {
                        _triplesPredicates.Remove(t.Predicate);
                    }
                }

                if (_triplesSubjectsPredicates.ContainsKey(t.Subject))
                {
                    if (_triplesSubjectsPredicates[t.Subject].ContainsKey(t.Predicate))
                    {
                        _triplesSubjectsPredicates[t.Subject][t.Predicate].Remove(t);
                        if (_triplesSubjectsPredicates[t.Subject][t.Predicate].Count == 0)
                        {
                            _triplesSubjectsPredicates[t.Subject].Remove(t.Predicate);
                        }

                        if (_triplesSubjectsPredicates[t.Subject].Count == 0)
                        {
                            _triplesSubjectsPredicates.Remove(t.Subject);
                        }
                    }
                }

                if (_triplesPredicatesObjects.ContainsKey(t.Predicate))
                {
                    if (_triplesPredicatesObjects[t.Predicate].ContainsKey(t.Object))
                    {
                        _triplesPredicatesObjects[t.Predicate][t.Object].Remove(t);
                        if (_triplesPredicatesObjects[t.Predicate][t.Object].Count == 0)
                        {
                            _triplesPredicatesObjects[t.Predicate].Remove(t.Object);
                        }

                        if (_triplesPredicatesObjects[t.Predicate].Count == 0)
                        {
                            _triplesPredicatesObjects.Remove(t.Predicate);
                        }
                    }
                }

                if (_triplesSubjectsObjects.ContainsKey(t.Subject))
                {
                    if (_triplesSubjectsObjects[t.Subject].ContainsKey(t.Object))
                    {
                        _triplesSubjectsObjects[t.Subject][t.Object].Remove(t);
                        if (_triplesSubjectsObjects[t.Subject][t.Object].Count == 0)
                        {
                            _triplesSubjectsObjects[t.Subject].Remove(t.Object);
                        }

                        if (_triplesSubjectsObjects[t.Subject].Count == 0)
                        {
                            _triplesSubjectsObjects.Remove(t.Subject);
                        }
                    }
                }
                RaiseTripleRemoved(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public override int Count => _triples.Count;

        /// <summary>
        /// Gets the given Triple
        /// </summary>
        /// <param name="t">Triple to retrieve</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if the given Triple does not exist in the Triple Collection</exception>
        public override Triple this[Triple t]
        {
            get
            {
                if (_triples.TryGetKey(t, out Triple actual))
                {
                    return actual;
                }
                else
                {
                    throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                }
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> SubjectNodes => _triplesSubjects.Keys;//IEnumerable<INode> ns = from t in this//                        select t.Subject;//return ns.Distinct();

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes => _triplesPredicates.Keys;//IEnumerable<INode> ns = from t in this//                        select t.Predicate;//return ns.Distinct();

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collectio
        /// </summary>
        public override IEnumerable<INode> ObjectNodes => _triplesObjects.Keys;//IEnumerable<INode> ns = from t in this//                        select t.Object;//return ns.Distinct();

        /// <summary>
        /// Gets all the triples with the given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return _triplesSubjects.ContainsKey(subj) ? _triplesSubjects[subj] : (IEnumerable<Triple>)new List<Triple>();
            //return _triples.WithSubject(subj);
        }


        /// <summary>
        /// Gets all the triples with the given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return _triplesObjects.ContainsKey(obj) ? _triplesObjects[obj] : (IEnumerable<Triple>)new List<Triple>();
            //return _triples.WithObject(obj);
        }

        /// <summary>
        /// Gets all the triples with the given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return _triplesPredicates.ContainsKey(pred) ? _triplesPredicates[pred] : (IEnumerable<Triple>)new List<Triple>();
        }

        /// <summary>
        /// Gets all the triples with the given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return _triplesPredicatesObjects.ContainsKey(pred) ? _triplesPredicatesObjects[pred].ContainsKey(obj) ? _triplesPredicatesObjects[pred][obj] : (IEnumerable<Triple>)new List<Triple>() : (IEnumerable<Triple>)new List<Triple>();
        }



        /// <summary>
        /// Gets all the triples with the given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return _triplesSubjectsObjects.ContainsKey(subj) ? _triplesSubjectsObjects[subj].ContainsKey(obj) ? _triplesSubjectsObjects[subj][obj] : (IEnumerable<Triple>)new List<Triple>() : (IEnumerable<Triple>)new List<Triple>();
        }

        /// <summary>
        /// Gets all the triples with the given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return _triplesSubjectsPredicates.ContainsKey(subj) ? _triplesSubjectsObjects[subj].ContainsKey(pred) ? _triplesSubjectsObjects[subj][pred] : (IEnumerable<Triple>)new List<Triple>() : (IEnumerable<Triple>)new List<Triple>();
        }

        #region IEnumerable<Triple> Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return _triples.Keys.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Collection
        /// </summary>
        public override void Dispose()
        {
            _triples.Clear();
        }

        #endregion
    }

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
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
            _lockManager.EnterWriteLock();
        }

        /// <summary>
        /// Exists the write lock
        /// </summary>
        protected void ExitWriteLock()
        {
            _lockManager.ExitWriteLock();
        }

        /// <summary>
        /// Enters the read lock
        /// </summary>
        protected void EnterReadLock()
        {
            _lockManager.EnterReadLock();
        }

        /// <summary>
        /// Exists the read lock
        /// </summary>
        protected void ExitReadLock()
        {
            _lockManager.ExitReadLock();
        }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
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
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public override bool Contains(Triple t)
        {
            bool contains = false;
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

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public override int Count
        {
            get
            {
                int c = 0;
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

        /// <summary>
        /// Gets the original instance of a specific Triple from the Triple Collection
        /// </summary>
        /// <param name="t">Triple</param>
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
                if (temp == null)
                {
                    throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                }

                return temp;
            }
        }

        /// <summary>
        /// Deletes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
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
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            List<Triple> triples = new List<Triple>();
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
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                List<INode> nodes = new List<INode>();
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
        /// Gets all the Nodes which are Subjects of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                List<INode> nodes = new List<INode>();
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
        /// Gets all triples with the given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            List<Triple> triples = new List<Triple>();
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
        /// Gets all the triples with the given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            List<Triple> triples = new List<Triple>();
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
                EnterReadLock();
                triples = _triples.WithSubjectPredicate(subj, pred).ToList();
            }
            finally
            {
                ExitReadLock();
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
                EnterWriteLock();
                _triples.Dispose();
            }
            finally
            {
                ExitWriteLock();
            }
        }
    }
}
