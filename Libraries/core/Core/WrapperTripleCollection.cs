using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Decorator for Triple Collections to make it easier to add additional functionality to existing collections
    /// </summary>
    public class WrapperTripleCollection
        : BaseTripleCollection
    {
        protected BaseTripleCollection _triples;

        public WrapperTripleCollection(BaseTripleCollection tripleCollection)
        {
            if (tripleCollection == null) throw new ArgumentNullException("tripleCollection");
            this._triples = tripleCollection;
            this._triples.TripleAdded += this.HandleTripleAdded;
            this._triples.TripleRemoved += this.HandleTripleRemoved;
        }

        private void HandleTripleAdded(Object sender, TripleEventArgs args)
        {
            this.RaiseTripleAdded(args.Triple);
        }

        private void HandleTripleRemoved(Object sender, TripleEventArgs args)
        {
            this.RaiseTripleRemoved(args.Triple);
        }

        protected internal override void Add(Triple t)
        {
            this._triples.Add(t);
        }

        public override bool Contains(Triple t)
        {
            return this._triples.Contains(t);
        }

        public override int Count
        {
            get
            {
                return this._triples.Count; 
            }
        }

        protected internal override void Delete(Triple t)
        {
            this._triples.Delete(t);
        }

        public override Triple this[Triple t]
        {
            get
            {
                return this._triples[t];
            }
        }

        public override IEnumerable<INode> ObjectNodes
        {
            get 
            {
                return this._triples.ObjectNodes;
            }
        }

        public override IEnumerable<INode> PredicateNodes
        {
            get 
            {
                return this._triples.PredicateNodes; 
            }
        }

        public override IEnumerable<INode> SubjectNodes
        {
            get 
            {
                return this._triples.SubjectNodes;
            }
        }

        public override void Dispose()
        {
            this._triples.Dispose();
        }

        public override IEnumerator<Triple> GetEnumerator()
        {
            return this._triples.GetEnumerator();
        }

        public override IEnumerable<Triple> WithObject(INode obj)
        {
            return this._triples.WithObject(obj);
        }

        public override IEnumerable<Triple> WithPredicate(INode pred)
        {
            return this._triples.WithPredicate(pred);
        }

        public override IEnumerable<Triple> WithPredicateObject(INode pred, INode obj)
        {
            return this._triples.WithPredicateObject(pred, obj);
        }

        public override IEnumerable<Triple> WithSubject(INode subj)
        {
            return this._triples.WithSubject(subj);
        }

        public override IEnumerable<Triple> WithSubjectObject(INode subj, INode obj)
        {
            return this._triples.WithSubjectObject(subj, obj);
        }

        public override IEnumerable<Triple> WithSubjectPredicate(INode subj, INode pred)
        {
            return this._triples.WithSubjectPredicate(subj, pred);
        }
    }
}
