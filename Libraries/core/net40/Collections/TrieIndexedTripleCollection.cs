using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using VDS.Common.Tries;
using VDS.RDF.Nodes;
using VDS.RDF.Graphs;

namespace VDS.RDF.Collections
{
    public class TrieIndexedTripleCollection
        : BaseTripleCollection
    {
        private static Func<Triple, IEnumerable<INode>> SpoMapper = (t => new INode[] { t.Subject, t.Predicate, t.Object });
        private static Func<Triple, IEnumerable<INode>> PosMapper = (t => new INode[] { t.Predicate, t.Object, t.Subject });
        private static Func<Triple, IEnumerable<INode>> OspMapper = (t => new INode[] { t.Object, t.Subject, t.Predicate });

        private ITrie<Triple, INode, Triple> _spo, _pos, _osp;
        private long _count = 0;

        public TrieIndexedTripleCollection()
        {
            this._spo = new Trie<Triple, INode, Triple>(SpoMapper);
            this._pos = new Trie<Triple, INode, Triple>(PosMapper);
            this._osp = new Trie<Triple, INode, Triple>(OspMapper);
        }

        public override bool Add(Triple t)
        {
            if (!this._spo.ContainsKey(t))
            {
                this._spo.Add(t, t);
                this._pos.Add(t, t);
                this._osp.Add(t, t);
                this._count++;
                return true;
            }
            return false;
        }

        public override bool Contains(Triple t)
        {
            return this._spo.ContainsKey(t);
        }

        public override long Count
        {
            get { return this._count; }
        }

        public override bool Remove(Triple t)
        {
            if (this._spo.ContainsKey(t))
            {
                this._spo.Remove(t);
                this._pos.Remove(t);
                this._osp.Remove(t);
                this._count--;
                return true;
            }
            return false;
        }

        public override void Clear()
        {
            this._spo.Clear();
            this._pos.Clear();
            this._osp.Clear();
            this.RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public override IEnumerable<INode> ObjectNodes
        {
            get { return this._osp.Root.Children.Select(n => n.KeyBit); }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get { return this._pos.Root.Children.Select(n => n.KeyBit); }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get { return this._spo.Root.Children.Select(n => n.KeyBit); }
        }

        protected IEnumerable<Triple> Find(ITrie<Triple, INode, Triple> index, params INode[] ns)
        {
            ITrieNode<INode, Triple> node = index.Root;
            if (node == null) return Enumerable.Empty<Triple>();
            foreach (INode n in ns)
            {
                ITrieNode<INode, Triple> temp;
                if (node.TryGetChild(n, out temp)) {
                    node = temp;
                } else {
                    return Enumerable.Empty<Triple>();
                }
            }
            return node.Values;
        }

        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return this.Find(this._osp, obj);
        }

        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return this.Find(this._pos, pred);
        }

        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return this.Find(this._pos, pred, obj);
        }

        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return this.Find(this._spo, subj);
        }

        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return this.Find(this._osp, obj, subj);
        }

        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return this.Find(this._spo, subj, pred);
        }

        public override void Dispose()
        {
            // No unmanged resources to dispose of
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._spo.Values.GetEnumerator();
        }
    }
}
