using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Datasets;
using VDS.Alexandria.Utilities;

namespace VDS.Alexandria.Datasets
{
    public abstract class BaseAlexandriaDataset : BaseTransactionalDataset
    {
        private BaseAlexandriaManager _manager;

        public BaseAlexandriaDataset(BaseAlexandriaManager manager)
        {
            this._manager = manager;
        }

        protected sealed override void AddGraphInternal(IGraph g)
        {
            this._manager.SaveGraph(g);
        }

        protected sealed override void RemoveGraphInternal(Uri graphUri)
        {
            this._manager.DeleteGraph(graphUri);
        }

        protected sealed override ITransactionalGraph GetModifiableGraphInternal(Uri graphUri)
        {
            return new StoreGraphPersistenceWrapper(this._manager, this.GetGraphInternal(graphUri));
        }

        protected sealed override bool ContainsTripleInternal(Triple t)
        {
            return this._manager.IndexManager.GetTriples(t).Any();
        }

        protected abstract override IEnumerable<Triple> GetAllTriples();

        protected override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            return this._manager.IndexManager.GetTriplesWithSubject(subj);
        }

        protected override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            return this._manager.IndexManager.GetTriplesWithPredicate(pred);
        }

        protected override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithObject(obj);
        }

        protected override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            return this._manager.IndexManager.GetTriplesWithSubjectPredicate(subj, pred);
        }

        protected override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithSubjectObject(subj, obj);
        }

        protected override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            return this._manager.IndexManager.GetTriplesWithPredicateObject(pred, obj);
        }

        protected override void FlushInternal()
        {
            this._manager.Flush();
        }
    }
}
