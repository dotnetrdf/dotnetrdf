/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
        protected Dictionary<int, Triple> _triples;
        /// <summary>
        /// Underlying Storage of the Triple Collection which handles the extra Triples that result from Hash Code collisions
        /// </summary>
        protected List<Triple> _collisionTriples;

        /// <summary>
        /// Creates a new Triple Collection
        /// </summary>
        public TripleCollection()
        {
            this._triples = new Dictionary<int, Triple>();
            this._collisionTriples = new List<Triple>();
        }

        /// <summary>
        /// Determines whether a given Triple is in the Triple Collection
        /// </summary>
        /// <param name="t">The Triple to test</param>
        /// <returns>True if the Triple already exists in the Triple Collection</returns>
        public override bool Contains(Triple t)
        {
            //Is the Hash Code in the Dictionary?
            if (this._triples.ContainsKey(t.GetHashCode()))
            {
                //Are the two Triples equal?
                if (this._triples[t.GetHashCode()].Equals(t))
                {
                    return true;
                }
                else
                {
                    //There's a Hash Code Collision
                    //Is the Triple in the Collision Triples list?
                    return this._collisionTriples.Contains(t);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override void Add(Triple t)
        {
            if (!this._triples.ContainsKey(t.GetHashCode()))
            {
                this._triples.Add(t.GetHashCode(), t);
            }
            else if (!this._triples[t.GetHashCode()].Equals(t))
            {
                //Hash Code Collision
                this._triples[t.GetHashCode()].Collides = true;
                t.Collides = true;

                //Add to Collision Triples list
                this._collisionTriples.Add(t);
            }
        }

        /// <summary>
        /// Deletes a Triple from the Colleciton
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        protected internal override void Delete(Triple t)
        {
            if (this._triples.ContainsKey(t.GetHashCode()))
            {
                if (this._triples[t.GetHashCode()].Equals(t))
                {
                    this._triples.Remove(t.GetHashCode());

                    //Were there any Collisions on this Hash Code?
                    if (this._collisionTriples.Any(tri => tri.GetHashCode().Equals(t.GetHashCode())))
                    {
                        //Move the first colliding Triple to the main Triple Collection
                        //and remove it from the Collision Triple list
                        Triple first = this._collisionTriples.First(tri => tri.GetHashCode().Equals(t.GetHashCode()));
                        this._triples.Add(first.GetHashCode(), first);
                        this._collisionTriples.Remove(first);
                    }
                }
                else
                {
                    //Hash Code Collision
                    //Remove from Collision Triples list instead
                    this._collisionTriples.Remove(t);
                }
            }
        }

        /// <summary>
        /// Gets the Number of Triples in the Triple Collection
        /// </summary>
        public override int Count
        {
            get
            {
                return (this._triples.Count + this._collisionTriples.Count);
            }
        }

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
                if (this.Contains(t))
                {
                    if (this._triples[t.GetHashCode()].Equals(t))
                    {
                        return this._triples[t.GetHashCode()];
                    }
                    else
                    {
#if SILVERLIGHT
                        for (int i = 0; i < this._collisionTriples.Count; i++)
                        {
                            if (this._collisionTriples[i].Equals(t)) return this._collisionTriples[i];
                        }
                        throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
#else
                        return this._collisionTriples.Find(x => x.Equals(t));
#endif
                    }
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
        public override IEnumerable<INode> SubjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Subject;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Predicates of Triples in the Triple Collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Predicate;

                return ns.Distinct();
            }
        }

        /// <summary>
        /// Gets all the Nodes which are Objects of Triples in the Triple Collectio
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get
            {
                IEnumerable<INode> ns = from t in this
                                        select t.Object;

                return ns.Distinct();
            }
        }

        #region IEnumerable<Triple> Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            if (this._collisionTriples.Count > 0)
            {
                //Some Collision Triples exist
                //Need to concatenate the main Triple Collection with the Collision Triples list
                return (from t in this._triples.Values
                        select t).Concat(this._collisionTriples).GetEnumerator();
            }
            else
            {
                //No Collision Triples exists
                //Just give back the enumerator for the main Triple Collection
                return this._triples.Values.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Collection
        /// </summary>
        public override void Dispose()
        {
            this._triples.Clear();
            this._collisionTriples.Clear();
        }

        #endregion
    }

#if !NO_RWLOCK

    /// <summary>
    /// Thread Safe Triple Collection
    /// </summary>
    public class ThreadSafeTripleCollection 
        : TripleCollection, IEnumerable<Triple>
    {
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override void Add(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Add(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
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
                this._lockManager.EnterReadLock();
                contains = base.Contains(t);
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    c = base.Count;
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    temp = base[t];
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                if (temp == null) throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                return temp;
            }
        }

        /// <summary>
        /// Deletes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        protected internal override void Delete(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Delete(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
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
                this._lockManager.EnterReadLock();

                if (this._collisionTriples.Count > 0)
                {
                    triples = (from t in this._triples.Values
                               select t).Concat(this._collisionTriples).ToList();
                }
                else
                {
                    triples = (from t in this._triples.Values
                               select t).ToList();
                }
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.ObjectNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.PredicateNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.SubjectNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Disposes of a Triple Collection
        /// </summary>
        public override void Dispose()
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Dispose();
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }
    }

#endif

    /// <summary>
    /// A Triple Collection which trades a little load performance for improved lookup performance
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the libaries <see cref="HashTable&lt;TKey,TValue&gt;">HashTable</see> class for storage
    /// </para>
    /// <para>
    /// Indexes on Subjects, Predicates and Objects and on Subject-Predicate, Subject-Object and Predicate-Object pairs.
    /// </para>
    /// <para>
    /// In cases where you require minimal indexing and want to reduce memory usage you can set the <see cref="Options.FullTripleIndexing">Options.FullTripleIndexing</see> property to be false which disables the paired indices.  Once this is disabled any instance of this class instantiated when the option is disabled will only create basic indexes.
    /// </para>
    /// <para>
    /// Note: This is a change from the 0.1.x API behaviour where the <see cref="Options.FullTripleIndexing">FullTripleIndexing</see> option was disabled by default, from the 0.2.0 release onwards this is enabled by default.
    /// </para>
    /// </remarks>
    public class IndexedTripleCollection 
        : BaseTripleCollection, IEnumerable<Triple>
    {
        /// <summary>
        /// Hash Table storage of Triples
        /// </summary>
        protected HashTable<int, Triple> _triples = new HashTable<int, Triple>();
        private HashTable<INode, Triple> _predIndex;
        private HashTable<INode, Triple> _subjIndex;
        private HashTable<INode, Triple> _objIndex;
        private HashTable<int, Triple> _subjPredIndex, _subjObjIndex, _predObjIndex;
        private bool _fullyIndexed = false;

        /// <summary>
        /// Creates a new Indexed Triple Collection
        /// </summary>
        /// <param name="capacity">Initial Capacity of Index Slots</param>
        /// <remarks>
        /// <para>
        /// Indexes are stored using our <see cref="HashTable">HashTable</see> structure which is in effect a Dictionary where each Key can have multiple values.  The capacity is the initial number of value slots assigned to each Key, value slots grow automatically as desired but setting an appropriate capacity will allow you to tailor memory usage to your data and may make it possible to store data which would cause <see cref="OutOfMemoryException">OutOfMemoryException</see>'s with the default settings.
        /// </para>
        /// <para>
        /// For example if you have 1 million triples where no Triples share any Nodes in common then the memory needing to be allocated would be 1,000,000 * 6 * 10 * 4 bytes just for the object references without taking into account all the overhead for the data structures themselves.  Whereas if you set the capacity to 1 you reduce your memory requirements by a factor of 10 instantly.
        /// </para>
        /// <para>
        /// <strong>Note:</strong> Always remember that if you have large quantities of triples (1 million plus) then you are often better not loading them directly into memory and there many be better ways of processing the RDF depending on your application.  For example if you just want to do simple things with the data like validate it, count it etc you may be better using a <see cref="IRdfHandler">IRdfHandler</see> to process your data.
        /// </para>
        /// </remarks>
        public IndexedTripleCollection(int capacity)
        {
            this._subjIndex = new HashTable<INode, Triple>(capacity);
            this._predIndex = new HashTable<INode, Triple>(capacity);
            this._objIndex = new HashTable<INode, Triple>(capacity);
            if (Options.FullTripleIndexing)
            {
                this._fullyIndexed = true;
                this._subjPredIndex = new HashTable<int, Triple>(capacity);
                this._subjObjIndex = new HashTable<int, Triple>(capacity);
                this._predObjIndex = new HashTable<int, Triple>(capacity);
            }
        }

        /// <summary>
        /// Creates a new Indexed Triple Collection
        /// </summary>
        /// <remarks>
        /// Uses a default Index Capacity of 10, see remarks on other constructor overload for details of this
        /// </remarks>
        public IndexedTripleCollection()
            : this(10) { }

        /// <summary>
        /// Adds a Triple to the Collection if it doesn't already exist
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override void Add(Triple t)
        {
            int hash = t.GetHashCode();
            if (!this._triples.ContainsKey(hash))
            {
                this._triples.Add(hash, t);
                this.Index(t);
            }
            else
            {
                if (!this._triples.Contains(hash, t))
                {
                    this._triples.Add(hash, t);
                    t.Collides = true;
                    this.Index(t);
                }
            }
        }

        /// <summary>
        /// Internal method for indexing Triples as they are asserted
        /// </summary>
        /// <param name="t">Triple to index</param>
        private void Index(Triple t)
        {
            this._subjIndex.Add(t.Subject, t);
            this._predIndex.Add(t.Predicate, t);
            this._objIndex.Add(t.Object, t);

            //Build full indexes only if enabled
            if (this._fullyIndexed)
            {
                this._subjPredIndex.Add(Tools.CombineHashCodes(t.Subject, t.Predicate), t);
                this._predObjIndex.Add(Tools.CombineHashCodes(t.Predicate, t.Object), t);
                this._subjObjIndex.Add(Tools.CombineHashCodes(t.Subject, t.Object), t);
            }
        }

        /// <summary>
        /// Internal method for unindexing Triples as they are retracted
        /// </summary>
        /// <param name="t">Triple to unindex</param>
        private void UnIndex(Triple t)
        {
            this._subjIndex.Remove(t.Subject, t);
            this._predIndex.Remove(t.Predicate, t);
            this._objIndex.Remove(t.Object, t);

            if (this._fullyIndexed)
            {
                this._subjPredIndex.Remove(Tools.CombineHashCodes(t.Subject, t.Predicate), t);
                this._predObjIndex.Remove(Tools.CombineHashCodes(t.Predicate, t.Object), t);
                this._subjObjIndex.Remove(Tools.CombineHashCodes(t.Subject, t.Object), t);
            }
        }

        /// <summary>
        /// Gets whether a given Triple is contained in the collection
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns></returns>
        public override bool Contains(Triple t)
        {
            return this._triples.Contains(t.GetHashCode(), t);
        }

        /// <summary>
        /// Gets the number of Triples in the collection
        /// </summary>
        public override int Count
        {
            get 
            {
                return this._triples.Count;
            }
        }

        /// <summary>
        /// Gets the given Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to retrieve</param>
        /// <returns></returns>
        /// <exception cref="KeyNoutFoundException">Thrown if the given Triple does not exist in the Triple Collection</exception>
        public override Triple this[Triple t]
        {
            get 
            {
                if (this.Contains(t))
                {
                    Triple temp;
                    if (this._triples.TryGetValue(t.GetHashCode(), t, out temp))
                    {
                        if (temp == null) throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                        return temp;
                    }
                    else
                    {
                        throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                    }
                }
                else
                {
                    throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                }
            }
        }

        /// <summary>
        /// Deletes a Triple from the collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        protected internal override void Delete(Triple t)
        {
            int hash = t.GetHashCode();
            if (this._triples.ContainsKey(hash))
            {
                if (this._triples.Remove(hash, t))
                {
                    this.UnIndex(t);
                }
            }
        }

        /// <summary>
        /// Gets the Object Nodes from the collection
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return (from Triple t in this._triples
                        select t.Object).Distinct();
            }
        }

        /// <summary>
        /// Gets the Predicate Nodes from the collection
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return (from Triple t in this._triples
                        select t.Predicate).Distinct();
            }
        }

        /// <summary>
        /// Gets the Subject Nodes from the collection
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return (from Triple t in this._triples
                        select t.Subject).Distinct();
                //return (from n in this._subjIndex.Keys
                //            select n);
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            if (this._subjIndex.ContainsKey(subj))
            {
                return this._subjIndex.GetValues(subj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            if (this._predIndex.ContainsKey(pred))
            {
                return this._predIndex.GetValues(pred);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            if (this._objIndex.ContainsKey(obj))
            {
                return this._objIndex.GetValues(obj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            if (this._fullyIndexed)
            {
                int key = Tools.CombineHashCodes(subj, pred);
                if (this._subjPredIndex.ContainsKey(key))
                {
                    return this._subjPredIndex.GetValues(key);
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return base.WithSubjectPredicate(subj, pred);
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            if (this._fullyIndexed)
            {
                int key = Tools.CombineHashCodes(pred, obj);
                if (this._predObjIndex.ContainsKey(key))
                {
                    return this._predObjIndex.GetValues(key);
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return base.WithPredicateObject(pred, obj);
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Object
        /// </summary>
        /// <param name="subj"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            if (this._fullyIndexed)
            {
                int key = Tools.CombineHashCodes(subj, obj);
                if (this._subjObjIndex.ContainsKey(key))
                {
                    return this._subjObjIndex.GetValues(key);
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return base.WithSubjectObject(subj, obj);
            }
        }

        /// <summary>
        /// Disposes of a Triple collection
        /// </summary>
        public override void Dispose()
        {
            this._triples.Clear();
            this._subjIndex.Clear();
            this._predIndex.Clear();
            this._objIndex.Clear();

            //If full indexing is on then we need to clear the compound indices
            //Note that while the default value for _fullyIndexed is false the constructor
            //will alter this based on the Options.FullTripleIndexing property which defaults
            //to true so unless the user explicitly disables this prior to instantiating the triple
            //collection we will almost certainly be using compound indices
            if (this._fullyIndexed)
            {
                this._subjPredIndex.Clear();
                this._predObjIndex.Clear();
                this._subjObjIndex.Clear();
            }
        }

        /// <summary>
        /// Gets the enumerator of the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return (from Triple t in this._triples
                    select t).GetEnumerator();
        }
    }

#if !NO_RWLOCK

    /// <summary>
    /// Thread Safe Triple Collection which is also Indexed
    /// </summary>
    /// <remarks>
    /// Using the indexed Triple Collection requires more memory but is considerably faster for a lot of the lookup operations you would typically want to do - in essence we trade some memory consumption for performance.
    /// </remarks>
    public class IndexedThreadSafeTripleCollection 
        : IndexedTripleCollection, IEnumerable<Triple>
    {
        private ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Creates a new Indexed Thread Safe Triple Collection
        /// </summary>
        public IndexedThreadSafeTripleCollection()
            : base() { }

        /// <summary>
        /// Creates a new Indexed Thread Safe Triple Collection with the given index slot capacity
        /// </summary>
        /// <param name="capacity">Index Slot Capacity</param>
        public IndexedThreadSafeTripleCollection(int capacity)
            : base(capacity) { }

        /// <summary>
        /// Adds a Triple to the Collection
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override void Add(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Add(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();    
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
                this._lockManager.EnterReadLock();
                contains = base.Contains(t);
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();           
                    c = base.Count;
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return c;
            }
        }

        /// <summary>
        /// Deletes a Triple from the Collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <remarks>Deleting something that doesn't exist has no effect and gives no error</remarks>
        protected internal override void Delete(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Delete(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the given Triple from the Triple Collection
        /// </summary>
        /// <param name="t">Triple to retrieve</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if the given Triple does not exist in the Collection</exception>
        public override Triple this[Triple t]
        {
            get
            {
                Triple temp;
                try
                {
                    this._lockManager.EnterReadLock();
                    temp = base[t];
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                if (temp == null) throw new KeyNotFoundException("The given Triple does not exist in the Triple Collection");
                return temp;
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
                this._lockManager.EnterReadLock();
                triples = (from Triple t in this._triples
                           select t).ToList();                
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.ObjectNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.PredicateNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
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
                    this._lockManager.EnterReadLock();
                    nodes = base.SubjectNodes.ToList();
                }
                finally
                {
                    this._lockManager.ExitReadLock();
                }
                return nodes;
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithSubject(subj).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithPredicate(pred).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithObject(obj).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithSubjectPredicate(subj, pred).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithPredicateObject(pred, obj).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Object
        /// </summary>
        /// <param name="subj"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.WithSubjectObject(subj, obj).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
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
                this._lockManager.EnterWriteLock();
                base.Dispose();
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }
    }

#endif

}
