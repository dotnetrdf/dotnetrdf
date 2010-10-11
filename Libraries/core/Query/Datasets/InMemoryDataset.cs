using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    public class InMemoryDataset : BaseDataset
    {
        private IInMemoryQueryableStore _store;

        public InMemoryDataset(IInMemoryQueryableStore store)
        {
            this._store = store;
        }

        #region Graph Existence and Retrieval

        public override void AddGraph(IGraph g)
        {
            this._store.Add(g, true);
        }

        public override void RemoveGraph(Uri graphUri)
        {
            this._store.Remove(graphUri);
        }

        public override bool HasGraph(Uri graphUri)
        {
            return this._store.HasGraph(graphUri);
        }

        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return this._store.Graphs; 
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return (from g in this._store.Graphs
                        select g.BaseUri);
            }
        }

        public override IGraph this[Uri graphUri]
        {
            get 
            {
                return this._store.Graph(graphUri); 
            }
        }

        #endregion

        #region Triple Existence and Retrieval

        public override bool ContainsTriple(Triple t)
        {
            return this._store.Contains(t);
        }

        protected override IEnumerable<Triple> GetAllTriples()
        {
            return this._store.Triples;
        }

        public override IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubject(subj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubject(subj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubject(subj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithPredicate(pred)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithPredicate(pred);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithPredicate(pred);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithObject(obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithObject(obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithObject(obj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubjectPredicate(subj, pred)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubjectPredicate(subj, pred);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubjectPredicate(subj, pred);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithSubjectObject(subj, obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithSubjectObject(subj, obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithSubjectObject(subj, obj);
            }
        }

        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return (from g in this.Graphs
                            from t in g.GetTriplesWithPredicateObject(pred, obj)
                            select t);
                }
                else
                {
                    return this._defaultGraph.GetTriplesWithPredicateObject(pred, obj);
                }
            }
            else
            {
                return this._activeGraph.GetTriplesWithPredicateObject(pred, obj);
            }
        }

        #endregion

        public override void Flush()
        {
            if (this._store is IFlushableStore)
            {
                ((IFlushableStore)this._store).Flush();
            }
        }
    }
}
