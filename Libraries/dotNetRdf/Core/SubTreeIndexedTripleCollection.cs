/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;
using VDS.Common.Trees;

namespace VDS.RDF
{
    /// <summary>
    /// An indexed triple collection that uses our <see cref="MultiDictionary{TKey,TValue}"/> and <see cref="BinaryTree{TKey,TValue}"/> implementations under the hood for the index structures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A variation on <see cref="TreeIndexedTripleCollection"/> which structures the indexes slightly differently, this may give differing performance and reduced memory usage in some scenarios.
    /// </para>
    /// </remarks>
    public class SubTreeIndexedTripleCollection
        : AbstractIndexedTripleCollection
    {
        // Indexes
        private MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> _s = new MultiDictionary<INode,MultiDictionary<Triple,HashSet<Triple>>>(new FastVirtualNodeComparer()),
                                                                              _p = new MultiDictionary<INode,MultiDictionary<Triple,HashSet<Triple>>>(new FastVirtualNodeComparer()),
                                                                              _o = new MultiDictionary<INode,MultiDictionary<Triple,HashSet<Triple>>>(new FastVirtualNodeComparer());

        // Placeholder Variables for compound lookups
        private readonly VariableNode _subjVar = new VariableNode("s"),
                             _predVar = new VariableNode("p"),
                             _objVar = new VariableNode("o");

        // Hash Functions
        private readonly Func<Triple, int> _sHash = (t => Tools.CombineHashCodes(t.Subject, t.Predicate)),
                                  _pHash = (t => Tools.CombineHashCodes(t.Predicate, t.Object)),
                                  _oHash = (t => Tools.CombineHashCodes(t.Object, t.Subject));

        // Comparers
        private readonly IComparer<Triple> _sComparer = new SubjectPredicateComparer(new FastVirtualNodeComparer()),
                                  _pComparer = new PredicateObjectComparer(new FastVirtualNodeComparer()),
                                  _oComparer = new ObjectSubjectComparer(new FastVirtualNodeComparer());

        /// <summary>
        /// Indexes a Triple.
        /// </summary>
        /// <param name="t">Triple.</param>
        protected override void Index(Triple t)
        {
            Index(t.Subject, t, _s, _sHash, _sComparer);
            Index(t.Predicate, t, _p, _pHash, _pComparer);
            Index(t.Object, t, _o, _oHash, _oComparer);
        }

        /// <summary>
        /// Helper for indexing triples.
        /// </summary>
        /// <param name="n">Node to index by.</param>
        /// <param name="t">Triple.</param>
        /// <param name="index">Index to insert into.</param>
        /// <param name="comparer">Comparer for the Index.</param>
        /// <param name="hashFunc">Hash Function for the Index.</param>
        private void Index(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index, Func<Triple,int> hashFunc, IComparer<Triple> comparer)
        {
            if (index.TryGetValue(n, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                if (subtree.TryGetValue(t, out HashSet<Triple> ts))
                {
                    if (ts == null)
                    {
                        subtree[t] = new HashSet<Triple> { t };
                    }
                    else
                    {
                        ts.Add(t);
                    }
                }
                else
                {
                    subtree.Add(t, new HashSet<Triple> { t });
                }
            }
            else
            {
                subtree = new MultiDictionary<Triple, HashSet<Triple>>(hashFunc, false, comparer, MultiDictionaryMode.AVL);
                subtree.Add(t, new HashSet<Triple> { t });
                index.Add(n, subtree);
            }
        }

        /// <summary>
        /// Unindexes a triple.
        /// </summary>
        /// <param name="t">Triple.</param>
        protected override void Unindex(Triple t)
        {
            Unindex(t.Subject, t, _s);
            Unindex(t.Predicate, t, _p);
            Unindex(t.Object, t, _o);

        }

        /// <summary>
        /// Helper for unindexing triples.
        /// </summary>
        /// <param name="n">Node to index by.</param>
        /// <param name="t">Triple.</param>
        /// <param name="index">Index to remove from.</param>
        private void Unindex(INode n, Triple t, MultiDictionary<INode, MultiDictionary<Triple, HashSet<Triple>>> index)
        {
            if (index.TryGetValue(n, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                if (subtree.TryGetValue(t, out HashSet<Triple> ts))
                {
                    if (ts != null) ts.Remove(t);
                }
            }
        }


        /// <summary>
        /// Gets the specific instance of a Triple in the collection.
        /// </summary>
        /// <param name="t">Triple.</param>
        /// <returns></returns>
        public override Triple this[Triple t]
        {
            get
            {
                if (Triples.TryGetKey(t, out Triple actual))
                {
                    return actual;
                }
                throw new KeyNotFoundException("Given triple does not exist in this collection");
            }
        }

        /// <summary>
        /// Gets all the triples with a given object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithObject(INode obj)
        {
            if (_o.TryGetValue(obj, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets all the triples with a given predicate.
        /// </summary>
        /// <param name="pred">Predicate.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            if (_p.TryGetValue(pred, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets all the triples with a given subject.
        /// </summary>
        /// <param name="subj">Subject.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            if (_s.TryGetValue(subj, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return (from ts in subtree.Values
                        where ts != null
                        from t in ts
                        select t);
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets all the triples with a given predicate and object.
        /// </summary>
        /// <param name="pred">Predicate.</param>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            if (!_p.TryGetValue(obj, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return Enumerable.Empty<Triple>();
            }

            if (subtree.TryGetValue(new Triple(_subjVar, pred, obj), out HashSet<Triple> ts))
            {
                return ts ?? Enumerable.Empty<Triple>();
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets all the triples with a given subject and object.
        /// </summary>
        /// <param name="subj">Subject.</param>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            if (!_o.TryGetValue(obj, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return Enumerable.Empty<Triple>();
            }

            if (subtree.TryGetValue(new Triple(subj, _predVar, obj), out HashSet<Triple> ts))
            {
                return ts ?? Enumerable.Empty<Triple>();
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets all the triples with a given subject and predicate.
        /// </summary>
        /// <param name="subj">Subject.</param>
        /// <param name="pred">Predicate.</param>
        /// <returns></returns>
        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            if (!_s.TryGetValue(subj, out MultiDictionary<Triple, HashSet<Triple>> subtree))
            {
                return Enumerable.Empty<Triple>();
            }

            if (subtree.TryGetValue(new Triple(subj, pred, _objVar), out HashSet<Triple> ts))
            {
                return ts ?? Enumerable.Empty<Triple>();
            }
            return Enumerable.Empty<Triple>();
        }

        /// <summary>
        /// Gets the Object Nodes.
        /// </summary>
        public override IEnumerable<INode> ObjectNodes => _o.Keys;

        /// <summary>
        /// Gets the Predicate Nodes.
        /// </summary>
        public override IEnumerable<INode> PredicateNodes => _p.Keys;

        /// <summary>
        /// Gets the Subject Nodes.
        /// </summary>
        public override IEnumerable<INode> SubjectNodes => _s.Keys;

        /// <summary>
        /// Disposes of the collection.
        /// </summary>
        public override void Dispose()
        {
            Triples.Clear();
            _s.Clear();
            _p.Clear();
            _o.Clear();
        }

    }
}
