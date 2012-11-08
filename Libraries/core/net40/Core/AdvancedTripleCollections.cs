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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Common;

namespace VDS.RDF
{
    /// <summary>
    /// The Lazy Indexed Triple Collection is a variation on the <see cref="IndexedTripleCollection">IndexedTripleCollection</see> which only creates indexes based on the requests actually made
    /// </summary>
    /// <remarks>
    /// <para>
    /// For example if you were to ask for all Triples with the URI subject <strong>http://example.org/subject</strong> it would create a subject index only for that lookup, making another query would require a new subject index to be created (ie. in effect the indexes are just result caches for the queries you make).  This means that query requests over graphs using this triple collection can be quite slow but it means the graph uses less memory and is faster to load data as it does not need to create the index as data is entered.
    /// </para>
    /// <para>
    /// <strong>Warning: </strong> This is an experimental implementation and may be refined/removed in future releases, do not rely upon this!
    /// </para>
    /// </remarks>
    [Obsolete("This implementation is obsolete and will be removed in future releases", true)]
    public class LazyIndexedTripleCollection
        : BaseTripleCollection, IEnumerable<Triple>
    {
        /// <summary>
        /// Hash Table storage of Triples
        /// </summary>
        protected HashTable<int, Triple> _triples = new HashTable<int, Triple>(1);
        private HashTable<INode, Triple> _predIndex;
        private HashTable<INode, Triple> _subjIndex;
        private HashTable<INode, Triple> _objIndex;
        private HashTable<int, Triple> _subjPredIndex, _subjObjIndex, _predObjIndex;
        private bool _fullyIndexed = false;

        /// <summary>
        /// Creates a new Lazy Indexed Triple Collection
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
        public LazyIndexedTripleCollection(int capacity)
        {
            this._subjIndex = new HashTable<INode, Triple>(capacity, true);
            this._predIndex = new HashTable<INode, Triple>(capacity, true);
            this._objIndex = new HashTable<INode, Triple>(capacity, true);
            if (Options.FullTripleIndexing)
            {
                this._fullyIndexed = true;
                this._subjPredIndex = new HashTable<int, Triple>(capacity, true);
                this._subjObjIndex = new HashTable<int, Triple>(capacity, true);
                this._predObjIndex = new HashTable<int, Triple>(capacity, true);
            }
        }

        /// <summary>
        /// Creates a new Lazy Indexed Triple Collection
        /// </summary>
        /// <remarks>
        /// Uses a default Index Capacity of 10, see remarks on other constructor overload for details of this
        /// </remarks>
        public LazyIndexedTripleCollection()
            : this(10) { }

        /// <summary>
        /// Adds a Triple to the Collection if it doesn't already exist
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override bool Add(Triple t)
        {
            int hash = t.GetHashCode();
            if (!this._triples.ContainsKey(hash))
            {
                this._triples.Add(hash, t);
                this.Index(t);
                return true;
            }
            else
            {
                if (!this._triples.Contains(hash, t))
                {
                    this._triples.Add(hash, t);
                    this.Index(t);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Internal method for indexing Triples as they are asserted
        /// </summary>
        /// <param name="t">Triple to index</param>
        private void Index(Triple t)
        {
            //As we are lazy indexing we only add to indexes if a relevant index
            //already exists
            if (this._subjIndex.ContainsKey(t.Subject)) this._subjIndex.Add(t.Subject, t);
            if (this._predIndex.ContainsKey(t.Predicate)) this._predIndex.Add(t.Predicate, t);
            if (this._objIndex.ContainsKey(t.Object)) this._objIndex.Add(t.Object, t);

            //Build full indexes only if enabled
            if (this._fullyIndexed)
            {
                int sp = Tools.CombineHashCodes(t.Subject, t.Predicate);
                if (this._subjPredIndex.ContainsKey(sp)) this._subjPredIndex.Add(sp, t);
                int po = Tools.CombineHashCodes(t.Predicate, t.Object);
                if (this._predObjIndex.ContainsKey(po)) this._predObjIndex.Add(po, t);
                int so = Tools.CombineHashCodes(t.Subject, t.Object);
                if (this._subjObjIndex.ContainsKey(so)) this._subjObjIndex.Add(so, t);
            }
        }

        /// <summary>
        /// Internal method for unindexing Triples as they are retracted
        /// </summary>
        /// <param name="t">Triple to unindex</param>
        private void UnIndex(Triple t)
        {
            //Unindexing happens as usual
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
        /// <exception cref="KeyNotFoundException">Thrown if the given Triple does not exist in the Triple Collection</exception>
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
        protected internal override bool Delete(Triple t)
        {
            int hash = t.GetHashCode();
            if (this._triples.ContainsKey(hash))
            {
                if (this._triples.Remove(hash, t))
                {
                    this.UnIndex(t);
                    return true;
                }
            }
            return false;
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
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            if (!this._subjIndex.ContainsKey(subj))
            {
                //Try and build the Index
                this._subjIndex.AddEmpty(subj);
                foreach (Triple t in this._triples.Values)
                {
                    if (t.Subject.Equals(subj))
                    {
                        this._subjIndex.Add(subj, t);
                    }
                }
            }
            return this._subjIndex.GetValues(subj);

        }

        /// <summary>
        /// Gets all the Triples with a given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            if (!this._predIndex.ContainsKey(pred))
            {
                //Try and build the Index
                this._predIndex.AddEmpty(pred);
                foreach (Triple t in this._triples.Values)
                {
                    if (t.Predicate.Equals(pred))
                    {
                        this._predIndex.Add(pred, t);
                    }
                }
            }
            return this._predIndex.GetValues(pred);
        }

        /// <summary>
        /// Gets all the Triples with a given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            if (!this._objIndex.ContainsKey(obj))
            {
                //Try and build the Index
                this._objIndex.AddEmpty(obj);
                foreach (Triple t in this._triples.Values)
                {
                    if (t.Object.Equals(obj))
                    {
                        this._objIndex.Add(obj, t);
                    }
                }
            }
            return this._objIndex.GetValues(obj);
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
                if (!this._subjPredIndex.ContainsKey(key))
                {
                    //Try and build the Index
                    this._subjPredIndex.AddEmpty(key);
                    foreach (Triple t in this._triples.Values)
                    {
                        if (t.Subject.Equals(subj) && t.Predicate.Equals(pred))
                        {
                            this._subjPredIndex.Add(key, t);
                        }
                    }
                }
                return this._subjPredIndex.GetValues(key);
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
                if (!this._predObjIndex.ContainsKey(key))
                {
                    //Try and build the Index
                    this._predObjIndex.AddEmpty(key);
                    foreach (Triple t in this._triples.Values)
                    {
                        if (t.Predicate.Equals(pred) && t.Object.Equals(obj))
                        {
                            this._predObjIndex.Add(key, t);
                        }
                    }
                }
                return this._predObjIndex.GetValues(key);
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
                if (!this._subjObjIndex.ContainsKey(key))
                {
                    //Try and build the Index
                    this._subjObjIndex.AddEmpty(key);
                    foreach (Triple t in this._triples.Values)
                    {
                        if (t.Subject.Equals(subj) && t.Object.Equals(obj))
                        {
                            this._subjObjIndex.Add(key, t);
                        }
                    }
                }
                return this._subjObjIndex.GetValues(key);
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

    /// <summary>
    /// The List Indexed Triple Collection is a Triple Collection which uses sorted indices and binary search to return the answers to the queries.  This means that the indices require less memory but they are typically slower than the hash table based indices of the <see cref="IndexedTripleCollection">IndexedTripleCollection</see>
    /// </summary>
    /// <remarks>
    /// <para>
    /// List Indexes are created just-in-time but once a type of index has been created it can be used for all such lookups, this differs from the <see cref="LazyIndexedTripleCollection">LazyIndexedTripleCollection</see>
    /// </para>
    /// <para>
    /// <strong>Warning: </strong> This is an experimental implementation and may be refined/removed in future releases, do not rely upon this!
    /// </para>
    /// </remarks>
    [Obsolete("This implementation is obsolete and will be removed in future releases", true)]
    public class ListIndexedTripleCollection 
        : BaseTripleCollection, IEnumerable<Triple>
    {
        private HashTable<int, Triple> _triples;
        private List<Triple> _subjIndex, _predIndex, _objIndex;
        private bool _subjIndexReady = false, _predIndexReady = false, _objIndexReady = false;
        private bool _reindexSubjects = false, _reindexPredicates = false, _reindexObjects = false;
        private IComparer<Triple> _s = new SubjectComparer(),
                                  _p = new PredicateComparer(),
                                  _o = new ObjectComparer(),
                                  _sp = new SubjectPredicateComparer(), 
                                  _po = new PredicateObjectComparer(), 
                                  _os = new ObjectSubjectComparer();
        private VariableNode _varSubj = new VariableNode("s"),
                             _varPred = new VariableNode("p"),
                             _varObj = new VariableNode("o");

        /// <summary>
        /// Creates a new List Indexed Triple Collection
        /// </summary>
        public ListIndexedTripleCollection()
        {
            this._triples = new HashTable<int, Triple>();
        }

        private void Index(Triple t)
        {
            //Indexing should add to the Indexes if they have been created but mark them 
            //as not ready
            //Note: We mark the index as not ready before the Add to try and avoid any thread safety issues
            //though it is still theoretically possible that one thread can read a bad index
            if (this._subjIndex != null)
            {
                this._subjIndexReady = false;
                this._subjIndex.Add(t);
            }
            if (this._predIndex != null)
            {
                this._predIndexReady = false;
                this._predIndex.Add(t);
            }
            if (this._objIndex != null)
            {
                this._objIndexReady = false;
                this._objIndex.Add(t);
            }
        }

        private void UnIndex(Triple t)
        {
            //Unindexing should simply mark the Indexes as not ready and requiring reindexing
            this._reindexSubjects = true;
            this._subjIndexReady = false;
            this._reindexPredicates = true;
            this._predIndexReady = false;
            this._reindexObjects = true;
            this._objIndexReady = false;
        }

        private void PrepareSubjectIndex()
        {
            if (!this._subjIndexReady)
            {
                //Reindex if necessary
                if (this._reindexSubjects || this._subjIndex == null)
                {
                    this._subjIndex = new List<Triple>(this._triples.Values);
                }
                this._subjIndex.Sort(this._sp);
                this._subjIndexReady = true;
            }
        }

        private void PreparePredicateIndex()
        {
            if (!this._predIndexReady)
            {
                //Reindex if necessary
                if (this._reindexPredicates || this._predIndex == null)
                {
                    this._predIndex = new List<Triple>(this._triples.Values);
                }
                this._predIndex.Sort(this._po);
                this._predIndexReady = true;
            }
        }

        private void PrepareObjectIndex()
        {
            if (!this._objIndexReady)
            {
                //Reindex if necessary
                if (this._reindexObjects || this._objIndex == null)
                {
                    this._objIndex = new List<Triple>(this._triples.Values);
                }
                this._objIndex.Sort(this._os);
                this._objIndexReady = true;
            }
        }

        /// <summary>
        /// Adds a Triple to the Collection if it doesn't already exist
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override bool Add(Triple t)
        {
            int hash = t.GetHashCode();
            if (!this._triples.ContainsKey(hash))
            {
                this._triples.Add(hash, t);
                this.Index(t);
                return true;
            }
            else
            {
                if (!this._triples.Contains(hash, t))
                {
                    this._triples.Add(hash, t);
                    this.Index(t);
                    return true;
                }
            }
            return false;
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
        /// Deletes a Triple from the collection
        /// </summary>
        /// <param name="t">Triple to remove</param>
        protected internal override bool Delete(Triple t)
        {
            int hash = t.GetHashCode();
            if (this._triples.ContainsKey(hash))
            {
                if (this._triples.Remove(hash, t))
                {
                    this.UnIndex(t);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the given Triple from the Collection
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
        /// Gets all the Triples with a given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            this.PreparePredicateIndex();
            return this._predIndex.SearchIndex<Triple>(this._p, new Triple(this._varSubj.CopyNode(pred.Graph), pred, this._varObj.CopyNode(pred.Graph)));
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            this.PreparePredicateIndex();
            return this._predIndex.SearchIndex<Triple>(this._po, new Triple(this._varSubj.CopyNode(pred.Graph), pred, obj));
        }

        /// <summary>
        /// Gets all the Triples with a given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            this.PrepareObjectIndex();
            return this._objIndex.SearchIndex<Triple>(this._o, new Triple(this._varSubj.CopyNode(obj.Graph), this._varPred.CopyNode(obj.Graph), obj));
        }

        /// <summary>
        /// Gets all the Triples with a given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            this.PrepareSubjectIndex();
            return this._subjIndex.SearchIndex<Triple>(this._s, new Triple(subj, this._varPred.CopyNode(subj.Graph), this._varObj.CopyNode(subj.Graph)));
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            this.PrepareObjectIndex();
            return this._objIndex.SearchIndex<Triple>(this._os, new Triple(subj, this._varPred.CopyNode(subj.Graph), obj));
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            this.PrepareSubjectIndex();
            return this._subjIndex.SearchIndex<Triple>(this._sp, new Triple(subj, pred, this._varObj.CopyNode(subj.Graph)));
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
            }
        }

        /// <summary>
        /// Disposes of a Triple collection
        /// </summary>
        public override void Dispose()
        {
            this._triples.Clear();
            if (this._subjIndex != null) this._subjIndex.Clear();
            if (this._predIndex != null) this._predIndex.Clear();
            if (this._objIndex != null) this._objIndex.Clear();
        }

        /// <summary>
        /// Gets the enumerator of the Collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            //If any of the indexes is available use that as the enumerator as it'll be far faster
            if (this._subjIndex != null && !this._reindexSubjects)
            {
                return this._subjIndex.GetEnumerator();
            }
            else if (this._predIndex != null && !this._reindexPredicates)
            {
                return this._predIndex.GetEnumerator();
            }
            else if (this._objIndex != null && !this._reindexObjects)
            {
                return this._objIndex.GetEnumerator();
            }
            else
            {
                return (from Triple t in this._triples
                        select t).GetEnumerator();
            }
        }
    }

    /// <summary>
    /// An indexed Triple Collection which uses Tries for the indexes
    /// </summary>
    [Obsolete("This implementation is obsolete and will be removed in future releases", true)]
    public class TrieIndexedTripleCollection
        : BaseTripleCollection, IEnumerable<Triple>
    {
        private HashTable<int, Triple> _triples;
        private TripleTrie _spIndex = new TripleTrie(TripleIndexType.SubjectPredicate),
                           _poIndex = new TripleTrie(TripleIndexType.PredicateObject),
                           _osIndex = new TripleTrie(TripleTrie.KeyMapperOS),
                           _soIndex = new TripleTrie(TripleIndexType.SubjectObject);

        /// <summary>
        /// Creates a new Trie indexed triple collection
        /// </summary>
        public TrieIndexedTripleCollection()
        {
            this._triples = new HashTable<int, Triple>(1);
        }

        /// <summary>
        /// Adds a Triple to the Collection if it doesn't already exist
        /// </summary>
        /// <param name="t">Triple to add</param>
        protected internal override bool Add(Triple t)
        {
            int hash = t.GetHashCode();
            if (!this._triples.ContainsKey(hash))
            {
                this._triples.Add(hash, t);
                this.Index(t);
                return true;
            }
            else
            {
                if (!this._triples.Contains(hash, t))
                {
                    this._triples.Add(hash, t);
                    this.Index(t);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Internal method for indexing Triples as they are asserted
        /// </summary>
        /// <param name="t">Triple to index</param>
        private void Index(Triple t)
        {
            this.AddToIndex(t, this._spIndex);
            this.AddToIndex(t, this._poIndex);
            this.AddToIndex(t, this._osIndex);
            this.AddToIndex(t, this._soIndex);
        }

        private void AddToIndex(Triple t, TripleTrie index)
        {
            TrieNode<INode, List<Triple>> node = index.MoveToNode(t);
            if (node.Value == null) node.Value = new List<Triple>();
            node.Value.Add(t);
        }

        /// <summary>
        /// Internal method for unindexing Triples as they are retracted
        /// </summary>
        /// <param name="t">Triple to unindex</param>
        private void UnIndex(Triple t)
        {
            this.RemoveFromIndex(t, this._spIndex);
            this.RemoveFromIndex(t, this._poIndex);
            this.RemoveFromIndex(t, this._osIndex);
            this.RemoveFromIndex(t, this._soIndex);
        }

        private void RemoveFromIndex(Triple t, TripleTrie index)
        {
            TrieNode<INode, List<Triple>> node = index.Find(t);
            if (node == null) return;
            if (node.Value == null) return;
            node.Value.Remove(t);
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
        protected internal override bool Delete(Triple t)
        {
            int hash = t.GetHashCode();
            if (this._triples.ContainsKey(hash))
            {
                if (this._triples.Remove(hash, t))
                {
                    this.UnIndex(t);
                    return true;
                }
            }
            return false;
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
            }
        }

        /// <summary>
        /// Gets all the Triples with a given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            TrieNode<INode, List<Triple>> node = this._spIndex.Find(new INode[] { subj });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            TrieNode<INode, List<Triple>> node = this._poIndex.Find(new INode[] { pred });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            TrieNode<INode, List<Triple>> node = this._osIndex.Find(new INode[] { obj });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            TrieNode<INode, List<Triple>> node = this._spIndex.Find(new INode[] { subj, pred });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            TrieNode<INode, List<Triple>> node = this._poIndex.Find(new INode[] { pred, obj });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given Subject and Object
        /// </summary>
        /// <param name="subj"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            TrieNode<INode, List<Triple>> node = this._soIndex.Find(new INode[] { subj, obj });
            if (node == null) return Enumerable.Empty<Triple>();
            return (from ts in node.Values
                    from t in ts
                    select t);
        }

        /// <summary>
        /// Disposes of a Triple collection
        /// </summary>
        public override void Dispose()
        {
            this._triples.Clear();
            this._spIndex.Clear();
            this._poIndex.Clear();
            this._osIndex.Clear();
            this._soIndex.Clear();
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

    /// <summary>
    /// Trie for Triples
    /// </summary>
    class TripleTrie
        : Trie<Triple, INode, List<Triple>>
    {
        public TripleTrie(TripleIndexType type)
            : base(GetKeyMapper(type)) { }

        public TripleTrie(Func<Triple, IEnumerable<INode>> keyMapper)
            : base(keyMapper) { }

        static Func<Triple, IEnumerable<INode>> GetKeyMapper(TripleIndexType type)
        {
            switch (type)
            {
                case TripleIndexType.Object:
                    return KeyMapperO;
                case TripleIndexType.Predicate:
                    return KeyMapperP;
                case TripleIndexType.PredicateObject:
                    return KeyMapperPO;
                case TripleIndexType.Subject:
                    return KeyMapperS;
                case TripleIndexType.SubjectObject:
                    return KeyMapperOS;
                case TripleIndexType.SubjectPredicate:
                    return KeyMapperSP;
                default:
                    throw new ArgumentException("Not an index type supported by the TripleTrie");
            }
        }

        internal static IEnumerable<INode> KeyMapperS(Triple t)
        {
            return t.Subject.AsEnumerable();
        }

        internal static IEnumerable<INode> KeyMapperP(Triple t)
        {
            return t.Predicate.AsEnumerable();
        }

        internal static IEnumerable<INode> KeyMapperO(Triple t)
        {
            return t.Object.AsEnumerable();
        }

        internal static IEnumerable<INode> KeyMapperSP(Triple t)
        {
            return new INode[] { t.Subject, t.Predicate };
        }

        internal static IEnumerable<INode> KeyMapperSO(Triple t)
        {
            return new INode[] { t.Subject, t.Object };
        }

        internal static IEnumerable<INode> KeyMapperOS(Triple t)
        {
            return new INode[] { t.Object, t.Subject };
        }

        internal static IEnumerable<INode> KeyMapperPO(Triple t)
        {
            return new INode[] { t.Predicate, t.Object };
        }
    }
}
