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
    /// While the index structures may seem marginally more complex than our <see cref="HashTable"/> based <see cref="IndexedTripleCollection"/> in testing they actually perform faster, they also have the benefit of guaranteeing that index lookups never return irrelevant triples which is possible with the <see cref="IndexedTripleCollection"/> due to the way <see cref="HashTable"/> handles key collisions.
    /// </para>
    /// </remarks>
    public class TreeIndexedTripleCollection
        : BaseTripleCollection
    {
        //Main Storage
        private MultiDictionary<Triple, Object> _triples = new MultiDictionary<Triple, object>();
        //Simple Indexes
        private MultiDictionary<INode, List<Triple>> _s = new MultiDictionary<INode, List<Triple>>(MultiDictionaryMode.AVL),
                                                     _p = new MultiDictionary<INode, List<Triple>>(MultiDictionaryMode.AVL),
                                                     _o = new MultiDictionary<INode, List<Triple>>(MultiDictionaryMode.AVL);
        //Compound Indexes
        private MultiDictionary<Triple, List<Triple>> _sp, _so, _po;

        //Placeholder Variables for compound lookups
        private VariableNode _subjVar = new VariableNode(null, "s"),
                             _predVar = new VariableNode(null, "p"),
                             _objVar = new VariableNode(null, "o");

        private bool _fullIndexing = false;
        private int _count = 0;

        public TreeIndexedTripleCollection()
            : this(MultiDictionaryMode.Unbalanced) { }

        public TreeIndexedTripleCollection(MultiDictionaryMode compoundIndexMode)
        {
            if (Options.FullTripleIndexing)
            {
                this._fullIndexing = true;
                this._sp = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Subject, t.Predicate), new SPComparer(), compoundIndexMode);
                this._so = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Subject, t.Object), new SOComparer(), compoundIndexMode);
                this._po = new MultiDictionary<Triple, List<Triple>>(t => Tools.CombineHashCodes(t.Predicate, t.Object), new POComparer(), compoundIndexMode);
            }
        }

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

        private void UnindexSimple(INode n, Triple t, MultiDictionary<INode, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(n, out ts))
            {
                if (ts != null) ts.Remove(t);
            }
        }

        private void UnindexCompound(Triple t, MultiDictionary<Triple, List<Triple>> index)
        {
            List<Triple> ts;
            if (index.TryGetValue(t, out ts))
            {
                if (ts != null) ts.Remove(t);
            }
        }

        protected internal override bool Add(Triple t)
        {
            if (!this._triples.ContainsKey(t))
            {
                this._triples.Add(t, null);
                this._count++;
                return true;
            }
            return false;
        }

        public override bool Contains(Triple t)
        {
            return this._triples.ContainsKey(t);
        }

        public override int Count
        {
            get 
            {
                //Note we maintain the count manually as traversing the entire tree every time we want to count would get very expensive
                return this._count;
            }
        }

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

        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return this._o.Keys;
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get
            {
                return this._p.Keys;
            }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return this._s.Keys;
            }
        }

        public override void Dispose()
        {
            this._triples.Clear();
            this._s.Clear();
            this._p.Clear();
            this._o.Clear();

            if (this._fullIndexing)
            {
                this._so.Clear();
                this._sp.Clear();
                this._po.Clear();
            }
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.Keys.GetEnumerator();
        }
    }
}
