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
            //TODO: Work out the logic regarding mapping to the default graph for this method and implement

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

            if (this._defaultGraph != null && (graphUri == null || graphUri.ToString().Equals(GraphCollection.DefaultGraphUri)))
            {
                if (this._defaultGraph is ModifiableGraphWrapper)
                {
                    return this._defaultGraph;
                }
                else if (this._modifiableGraphStore.HasGraph(this._defaultGraph.BaseUri))
                {
                    return this._modifiableGraphStore.Graph(this._defaultGraph.BaseUri);
                }
                else
                {
                    this._defaultGraph = new ModifiableGraphWrapper(this._defaultGraph, this._manager);
                    this._modifiableGraphStore.Add(this._defaultGraph);
                    return this._defaultGraph;
                }
            }
            else
            {
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
        }

        public override bool ContainsTriple(Triple t)
        {
            if (this._activeGraph != null)
            {
                return this._activeGraph.ContainsTriple(t);
            }
            else if (this._defaultGraph != null)
            {
                return this._defaultGraph.ContainsTriple(t);
            }
            else
            {
                return this._manager.IndexManager.GetTriples(t).Any();
            }
        }

        protected abstract override IEnumerable<Triple> GetAllTriples();

        public override IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            if (this._activeGraph == null)
            {
                if (this._defaultGraph == null)
                {
                    return this._manager.IndexManager.GetTriplesWithSubject(subj);
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
                    return this._manager.IndexManager.GetTriplesWithPredicate(pred);
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
                    return this._manager.IndexManager.GetTriplesWithObject(obj);
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
                    return this._manager.IndexManager.GetTriplesWithSubjectPredicate(subj, pred);
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
                    return this._manager.IndexManager.GetTriplesWithSubjectObject(subj, obj);
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
                    return this._manager.IndexManager.GetTriplesWithPredicateObject(pred, obj);
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
