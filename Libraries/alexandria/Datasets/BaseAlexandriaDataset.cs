using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Datasets;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Datasets
{
    public abstract class BaseAlexandriaDataset : BaseDataset
    {
        private BaseAlexandriaManager _manager;
        private TripleStore _modifiableGraphStore;

        public BaseAlexandriaDataset(BaseAlexandriaManager manager)
        {
            this._manager = manager;
        }

        public override void AddGraph(IGraph g)
        {
            this._manager.SaveGraph(g);
        }

        public override void RemoveGraph(Uri graphUri)
        {
            if (this._modifiableGraphStore != null)
            {
                //When we delete a Graph remove it from the Modifiable Graph Store as otherwise we'll
                //persist any changes to it which we don't want to do
                if (this._modifiableGraphStore.HasGraph(graphUri))
                {
                    this._modifiableGraphStore.Remove(graphUri);
                }
            }
            this._manager.DeleteGraph(graphUri);
        }

        public abstract override bool HasGraph(Uri graphUri);

        public abstract override IEnumerable<IGraph> Graphs
        {
            get;
        }

        public abstract override IEnumerable<Uri> GraphUris
        {
            get;
        }

        public abstract override IGraph this[Uri graphUri]
        {
            get;
        }

        public override IGraph GetModifiableGraph(Uri graphUri)
        {
            if (this._modifiableGraphStore == null) this._modifiableGraphStore = new TripleStore();
            if (this._modifiableGraphStore.HasGraph(graphUri))
            {
                return this._modifiableGraphStore.Graph(graphUri);
            }
            else
            {
                IGraph g = new ModifiableGraphWrapper(this[graphUri], this._manager);
                this._modifiableGraphStore.Add(g);
                return g;
            }
        }

        public override bool ContainsTriple(Triple t)
        {
            return this._manager.IndexManager.GetTriples(t).Any();
        }

        protected abstract override IEnumerable<Triple> GetAllTriples();

        public override IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return this._manager.IndexManager.GetTriplesWithSubject(subj);
        }

        public override IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return this._manager.IndexManager.GetTriplesWithPredicate(pred);
        }

        public override IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithObject(obj);
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return this._manager.IndexManager.GetTriplesWithSubjectPredicate(subj, pred);
        }

        public override IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithSubjectObject(subj, obj);
        }

        public override IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithPredicateObject(pred, obj);
        }

        public override void Flush()
        {
            if (this._modifiableGraphStore != null)
            {
                //Dispose of all the Graphs in the Modifiable Store
                //This causes them to flush their persistence actions to the manager
                //This results in the following Flush() call on the manager ensuring any updates are persisted to the underlying store
                foreach (IGraph g in this._modifiableGraphStore.Graphs)
                {
                    g.Dispose();
                }
            }
            this._manager.Flush();
        }
    }
}
