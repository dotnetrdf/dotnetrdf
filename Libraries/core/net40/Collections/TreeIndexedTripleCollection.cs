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
using System.Text;
using VDS.Common;
using VDS.Common.Trees;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// An indexed triple collection that uses our <see cref="MultiDictionary"/> and <see cref="BinaryTree"/> implementations under the hood for the index structures
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the index structures may seem marginally more complex than our <see cref="HashTable"/> based <see cref="IndexedTripleCollection"/> in testing they actually perform faster, they also have the benefit of guaranteeing that index lookups never return irrelevant triples which is possible with the <see cref="IndexedTripleCollection"/> due to the way <see cref="HashTable"/> handles key collisions.
    /// </para>
    /// </remarks>
    public class TreeIndexedTripleCollection
        : BaseTripleCollection
    {
        //Main Storage
        private MultiDictionary<Triple, Object> _triples = new MultiDictionary<Triple, object>(new FullTripleComparer(new FastNodeComparer()));
        //Simple Indexes
        private MultiDictionary<INode, List<Triple>> _s = new MultiDictionary<INode, List<Triple>>(new FastNodeComparer(), MultiDictionaryMode.AVL),
                                                     _p = new MultiDictionary<INode, List<Triple>>(new FastNodeComparer(), MultiDictionaryMode.AVL),
                                                     _o = new MultiDictionary<INode, List<Triple>>(new FastNodeComparer(), MultiDictionaryMode.AVL);
        //Compound Indexes
        private MultiDictionary<Triple, List<Triple>> _sp, _so, _po;

        //Placeholder Variables for compound lookups
        private VariableNode _subjVar = new VariableNode("s"),
                             _predVar = new VariableNode("p"),
                             _objVar = new VariableNode("o");

        private bool _fullIndexing = false;
        private long _count = 0;

        /// <summary>
        /// Creates a new Tree Indexed triple collection
        /// </summary>
        public TreeIndexedTripleCollection()
            : this(MultiDictionaryMode.Unbalanced) { }

        /// <summary>
        /// Creates a new Tree Indexed triple collection
        /// </summary>
        /// <param name="compoundIndexMode">Mode to use for compound indexes</param>
        public TreeIndexedTripleCollection(MultiDictionaryMode compoundIndexMode)
        {
            if (Options.FullTripleIndexing)
            {
                this._fullIndexing = true;
                this._sp = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Subject, t.Predicate), new SubjectPredicateComparer(new FastNodeComparer()), compoundIndexMode);
                this._so = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Subject, t.Object), new SubjectObjectComparer(new FastNodeComparer()), compoundIndexMode);
                this._po = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Predicate, t.Object), new PredicateObjectComparer(new FastNodeComparer()), compoundIndexMode);
            }
        }

        /// <summary>
        /// Indexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        private void Index(Triple t)
        {
            this.IndexSimple(t.Subject, t, this._s);
            this.IndexSimple(t.Predicate, t, this._p);
            this.IndexSimple(t.Object, t, this._o);

            if (this._fullIndexing)
            {
                this.IndexCompound(t, this._sp);
                this.IndexCompound(t, this._so);
                this.IndexCompound(t, this._po);
            }
        }

        /// <summary>
        /// Helper for indexing triples
        /// </summary>
        /// <param name="n">Node to index by</param>
        /// <param name="t">Triple</param>
        /// <param name="index">Index to insert into</param>
        private void IndexSimple(INode n, Triple t, MultiDictionary<INode, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(n, out ts))
            {
                if (ts == null)
                {
                    index[n] = new List<Triple> { t };
                }
                else
                {
                    ts.Add(t);
                }
            }
            else
            {
                index.Add(n, new List<Triple> { t });
            }
        }

        /// <summary>
        /// Helper for indexing triples
        /// </summary>
        /// <param name="t">Triple to index by</param>
        /// <param name="index">Index to insert into</param>
        private void IndexCompound(Triple t, MultiDictionary<Triple, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(t, out ts))
            {
                if (ts == null)
                {
                    index[t] = new List<Triple> { t };
                }
                else
                {
                    ts.Add(t);
                }
            }
            else
            {
                index.Add(t, new List<Triple> { t });
            }
        }

        /// <summary>
        /// Unindexes a triple
        /// </summary>
        /// <param name="t">Triple</param>
        private void Unindex(Triple t)
        {
            this.UnindexSimple(t.Subject, t, this._s);
            this.UnindexSimple(t.Predicate, t, this._p);
            this.UnindexSimple(t.Object, t, this._o);

            if (this._fullIndexing)
            {
                this.UnindexCompound(t, this._sp);
                this.UnindexCompound(t, this._so);
                this.UnindexCompound(t, this._po);
            }
        }

        /// <summary>
        /// Helper for unindexing triples
        /// </summary>
        /// <param name="n">Node to index by</param>
        /// <param name="t">Triple</param>
        /// <param name="index">Index to remove from</param>
        private void UnindexSimple(INode n, Triple t, MultiDictionary<INode, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(n, out ts))
            {
                if (ts != null) ts.Remove(t);
            }
        }

        /// <summary>
        /// Helper for unindexing triples
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="index">Index to remove from</param>
        private void UnindexCompound(Triple t, MultiDictionary<Triple, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(t, out ts))
            {
                if (ts != null) ts.Remove(t);
            }
        }

        /// <summary>
        /// Adds a Triple to the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override bool Add(Triple t)
        {
            if (!this.Contains(t))
            {
                this._triples.Add(t, null);
                this.Index(t);
                this._count++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the collection contains a given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override bool Contains(Triple t)
        {
            return this._triples.ContainsKey(t);
        }

        /// <summary>
        /// Gets the count of triples in the collection
        /// </summary>
        public override long Count
        {
            get 
            {
                //Note we maintain the count manually as traversing the entire tree every time we want to count would get very expensive
                return this._count;
            }
        }

        /// <summary>
        /// Deletes a triple from the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override bool Remove(Triple t)
        {
            if (this._triples.Remove(t))
            {
                //If removed then unindex
                this.Unindex(t);
                this.RaiseTripleRemoved(t);
                this._count--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all the triples with a given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            List<Triple> ts;
            if (this._o.TryGetValue(obj, out ts))
            {
                return (ts != null ? ts : Enumerable.Empty<Triple>());
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the triples with a given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            List<Triple> ts;
            if (this._p.TryGetValue(pred, out ts))
            {
                return (ts != null ? ts : Enumerable.Empty<Triple>());
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the triples with a given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            List<Triple> ts;
            if (this._s.TryGetValue(subj, out ts))
            {
                return (ts != null ? ts : Enumerable.Empty<Triple>());
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all the triples with a given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            if (this._fullIndexing)
            {
                List<Triple> ts;
                if (this._po.TryGetValue(new Triple(this._subjVar, pred, obj), out ts))
                {
                    return (ts != null ? ts : Enumerable.Empty<Triple>());
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return this.WithPredicate(pred).Where(t => t.Object.Equals(obj));
            }
        }

        /// <summary>
        /// Gets all the triples with a given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            if (this._fullIndexing)
            {
                List<Triple> ts;
                if (this._so.TryGetValue(new Triple(subj, this._predVar, obj), out ts))
                {
                    return (ts != null ? ts : Enumerable.Empty<Triple>());
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return this.WithSubject(subj).Where(t => t.Object.Equals(obj));
            }
        }

        /// <summary>
        /// Gets all the triples with a given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            if (this._fullIndexing)
            {
                List<Triple> ts;
                if (this._sp.TryGetValue(new Triple(subj, pred, this._objVar), out ts))
                {
                    return (ts != null ? ts : Enumerable.Empty<Triple>());
                }
                else
                {
                    return Enumerable.Empty<Triple>();
                }
            }
            else
            {
                return this.WithSubject(subj).Where(t => t.Predicate.Equals(pred));
            }
        }

        /// <summary>
        /// Gets the Object Nodes
        /// </summary>
        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return this._o.Keys;
            }
        }

        /// <summary>
        /// Gets the Predicate Nodes
        /// </summary>
        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                return this._p.Keys;
            }
        }

        /// <summary>
        /// Gets the Subject Nodes
        /// </summary>
        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return this._s.Keys;
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        public override void Dispose()
        {
            //No unmanaged resources to dispose of
        }

        /// <summary>
        /// Gets the enumerator for the collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.Keys.GetEnumerator();
        }
    }
}
