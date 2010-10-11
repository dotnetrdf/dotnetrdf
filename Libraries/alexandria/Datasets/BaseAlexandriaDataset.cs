using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Datasets;

namespace VDS.Alexandria.Datasets
{
    public abstract class BaseAlexandriaDataset : BaseDataset
    {
        private BaseAlexandriaManager _manager;

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
            this._manager.Flush();
        }
    }
}
