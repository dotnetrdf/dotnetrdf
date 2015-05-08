/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
        private static readonly Func<Triple, IEnumerable<INode>> _spoMapper = (t => new INode[] { t.Subject, t.Predicate, t.Object });
        private static readonly Func<Triple, IEnumerable<INode>> _posMapper = (t => new INode[] { t.Predicate, t.Object, t.Subject });
        private static readonly Func<Triple, IEnumerable<INode>> _ospMapper = (t => new INode[] { t.Object, t.Subject, t.Predicate });

        private readonly ITrie<Triple, INode, Triple> _spo, _pos, _osp;
        private long _count = 0;

        public TrieIndexedTripleCollection()
        {
            this._spo = new Trie<Triple, INode, Triple>(_spoMapper);
            this._pos = new Trie<Triple, INode, Triple>(_posMapper);
            this._osp = new Trie<Triple, INode, Triple>(_ospMapper);
        }

        public override bool CanModifyDuringIteration
        {
            get { return false; }
        }

        public override bool HasIndexes
        {
            get { return true; }
        }

        public override bool Add(Triple t)
        {
            if (this._spo.ContainsKey(t)) return false;

            this._spo.Add(t, t);
            this._pos.Add(t, t);
            this._osp.Add(t, t);
            this._count++;
            this.RaiseTripleAdded(t);

            return true;
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
            if (!this._spo.ContainsKey(t)) return false;

            this._spo.Remove(t);
            this._pos.Remove(t);
            this._osp.Remove(t);
            this._count--;
            this.RaiseTripleRemoved(t);

            return true;
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
