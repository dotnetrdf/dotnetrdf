/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.Common;
using VDS.Common.Trees;

namespace VDS.RDF
{
    /// <summary>
    /// An indexed triple collection that uses our <see cref="MultiDictionary"/> and <see cref="BinaryTree"/> implementations under the hood for the index structures
    /// </summary>
    /// <remarks>
    /// <para>
    /// A variation on <see cref="TreeIndexedTripleCollection"/> which structures the indexes slightly differently to reduce memory usage.
    /// </para>
    /// <para>
    /// While the index structures may seem marginally more complex than our <see cref="HashTable"/> based <see cref="IndexedTripleCollection"/> in testing they actually perform faster, they also have the benefit of guaranteeing that index lookups never return irrelevant triples which is possible with the <see cref="IndexedTripleCollection"/> due to the way <see cref="HashTable"/> handles key collisions.
    /// </para>
    /// </remarks>
    public class SubTreeIndexedTripleCollection
        : BaseTripleCollection
    {
        //Main Storage
        private MultiDictionary<Triple, Object> _triples = new MultiDictionary<Triple, object>(new FullTripleComparer(new FastNodeComparer()));
        //Indexes
        private MultiDictionary<INode, MultiDictionary<Triple, List<Triple>>> _s = new MultiDictionary<INode,MultiDictionary<Triple,List<Triple>>>(new FastNodeComparer()),
                                                                              _p = new MultiDictionary<INode,MultiDictionary<Triple,List<Triple>>>(new FastNodeComparer()),
                                                                              _o = new MultiDictionary<INode,MultiDictionary<Triple,List<Triple>>>(new FastNodeComparer());

        //Placeholder Variables for compound lookups
        private VariableNode _subjVar = new VariableNode(null, "s"),
                             _predVar = new VariableNode(null, "p"),
                             _objVar = new VariableNode(null, "o");

        //Hash Functions
        private Func<Triple, int> _sHash = (t => Tools.CombineHashCodes(t.Subject, t.Predicate)),
                                  _pHash = (t => Tools.CombineHashCodes(t.Predicate, t.Object)),
                                  _oHash = (t => Tools.CombineHashCodes(t.Object, t.Subject));

        //Comparers
        private IComparer<Triple> _sComparer = new SubjectPredicateComparer(new FastNodeComparer()),
                                  _pComparer = new PredicateObjectComparer(new FastNodeComparer()),
                                  _oComparer = new ObjectSubjectComparer(new FastNodeComparer());

        private int _count = 0;

        /// <summary>
        /// Creates a new Tree Indexed triple collection
        /// </summary>
        public SubTreeIndexedTripleCollection()
        { }

        /// <summary>
        /// Indexes a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        private void Index(Triple t)
        {
            this.Index(t.Subject, t, this._s, this._sHash, this._sComparer);
            this.Index(t.Predicate, t, this._p, this._pHash, this._pComparer);
            this.Index(t.Object, t, this._o, this._oHash, this._oComparer);
        }

        /// <summary>
        /// Helper for indexing triples
        /// </summary>
        /// <param name="n">Node to index by</param>
        /// <param name="t">Triple</param>
        /// <param name="index">Index to insert into</param>
        /// <param name="comparer">Comparer for the Index</param>
        /// <param name="hashFunc">Hash Function for the Index</param>
        private void Index(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, List<Triple>>> index, Func<Triple,int> hashFunc, IComparer<Triple> comparer)
        {
            MultiDictionary<Triple, List<Triple>> subtree;
            if (index.TryGetValue(n, out subtree))
            {
                List<Triple> ts;
                if (subtree.TryGetValue(t, out ts))
                {
                    if (ts == null)
                    {
                        subtree[t] = new List<Triple> { t };
                    }
                    else
                    {
                        ts.Add(t);
                    }
                }
                else
                {
                    subtree.Add(t, new List<Triple> { t });
                }
            }
            else
            {
                subtree = new MultiDictionary<Triple, List<Triple>>(hashFunc, comparer, MultiDictionaryMode.AVL);
                subtree.Add(t, new List<Triple> { t });
                index.Add(n, subtree);
            }
        }

        /// <summary>
        /// Unindexes a triple
        /// </summary>
        /// <param name="t">Triple</param>
        private void Unindex(Triple t)
        {
            this.Unindex(t.Subject, t, this._s);
            this.Unindex(t.Predicate, t, this._p);
            this.Unindex(t.Object, t, this._o);

        }

        /// <summary>
        /// Helper for unindexing triples
        /// </summary>
        /// <param name="n">Node to index by</param>
        /// <param name="t">Triple</param>
        /// <param name="index">Index to remove from</param>
        private void Unindex(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, List<Triple>>> index)
        {
            MultiDictionary<Triple, List<Triple>> subtree;
            if (index.TryGetValue(n, out subtree))
            {
                List<Triple> ts;
                if (subtree.TryGetValue(t, out ts))
                {
                    if (ts != null) ts.Remove(t);
                }
            }
        }

        /// <summary>
        /// Adds a Triple to the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected internal override bool Add(Triple t)
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
        public override int Count
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
        protected internal override bool Delete(Triple t)
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
        /// Gets the specific instance of a Triple in the collection
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public override Triple this[Triple t]
        {
            get 
            {
                Triple actual;
                if (this._triples.TryGetKey(t, out actual))
                {
                    return actual;
                }
                else
                {
                    throw new KeyNotFoundException("Given triple does not exist in this collection");
                }
            }
        }

        /// <summary>
        /// Gets all the triples with a given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._o.TryGetValue(obj, out subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
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
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._p.TryGetValue(pred, out subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
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
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._s.TryGetValue(subj, out subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
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
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._p.TryGetValue(obj, out subtree))
            {
                List<Triple> ts;
                if (subtree.TryGetValue(new Triple(this._subjVar.CopyNode(pred.Graph), pred, obj.CopyNode(pred.Graph)), out ts))
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
                return Enumerable.Empty<Triple>();
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
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._o.TryGetValue(obj, out subtree))
            {
                List<Triple> ts;
                if (subtree.TryGetValue(new Triple(subj, this._predVar.CopyNode(subj.Graph), obj.CopyNode(subj.Graph)), out ts))
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
                return Enumerable.Empty<Triple>();
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
            MultiDictionary<Triple, List<Triple>> subtree;
            if (this._s.TryGetValue(subj, out subtree))
            {
                List<Triple> ts;
                if (subtree.TryGetValue(new Triple(subj, pred.CopyNode(subj.Graph), this._objVar.CopyNode(subj.Graph)), out ts))
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
                return Enumerable.Empty<Triple>();
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
            this._triples.Clear();
            this._s.Clear();
            this._p.Clear();
            this._o.Clear();
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
