using System;
using System.Collections.Generic;
using System.Linq;
using org.topbraid.spin.model;
using org.topbraid.spin.model.update;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// Provides SPIN capabilities for a Dataset over a StorageProvider that is not SPIN-aware.
    /// Do we really need to miplement the ISparqlDataset interface since we do not really want to use it anyway ?
    /// TODO creates a schema for RDFModel
    /// TODO determine whether the graphs modifications should be handled by the External dataset or here. methinks it should be the responsibility of the external dataset...
    ///     if this is the case, do we really need this class ?
    /// </summary>
    public class SpinWrappedDataset : ISparqlDataset
    {

        internal enum QueryMode
        {
            UserQuerying = 0,
            SpinInferencing = 1,
            SpinConstraintsChecking = 2
        }

        // TODO add event handlers on the _modelDataset
        // TODO rename the private field into something more understandable
        internal INode _datasetNode;

        internal bool _datasetChanged;

        internal IGraph _underlyingRDFDataset;

        internal IUpdateableStorage _storage;

        private SpinProcessor _spinProcessor = new SpinProcessor();

        private bool _useNativeTransactions = false;

        private QueryMode _queryExecutionMode = QueryMode.UserQuerying;

        #region Initialisation

        /// <summary>
        /// Inititalize a SpinWrapperDataset upon a storage engine using all the graphs in the store.
        /// </summary>
        /// <param name="underlyingStorage"></param>
        internal SpinWrappedDataset(IUpdateableStorage storage)
        {
            _storage = storage;
            _underlyingRDFDataset = DatasetUtil.LoadDataset(_storage);
            Initialise();
        }

        /// <summary>
        /// </summary>
        /// <param name="storage"></param>
        public SpinWrappedDataset(Uri datasetUri, IUpdateableStorage storage)
        {
            _storage = storage;
            _underlyingRDFDataset = DatasetUtil.LoadDataset(_storage, datasetUri);
            Initialise();
        }

        /// <summary>
        /// Inititalize a SPIN model upon a storage engine using a dataset composed of the specified graphs.
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="dataset"></param>
        public SpinWrappedDataset(Uri datasetUri, IUpdateableStorage storage, IEnumerable<Uri> dataset)
        {
            _storage = storage;
            _underlyingRDFDataset = DatasetUtil.LoadDataset(_storage, datasetUri, dataset);
            Initialise();
        }

        internal void Initialise()
        {
            if (_storage is ITransactionalStorage || _storage is IAsyncTransactionalStorage)
            {
                _useNativeTransactions = true;
            }
            _datasetNode = _underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassDataset)
                .Union(_underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SPINRuntime.ClassUpdateControlledDataset))
                .Select(t => t.Subject).FirstOrDefault();
            HasDatasetChanged = true;
            _underlyingRDFDataset.Changed += OnDatasetChanged;
        }

        #endregion

        #region SpinWrapperDataset specifics

        protected void OnDatasetChanged(object sender, GraphEventArgs args)
        {
            HasDatasetChanged = true;
        }

        /// <summary>
        /// Gets/sets the SpinProcessor that is assigned to this Dataset
        /// </summary>
        internal SpinProcessor spinProcessor
        {
            get
            {
                return _spinProcessor;
            }
            set
            {
                _spinProcessor = value;
            }
        }

        /// <summary>
        /// Gets whether the underlying storage supports transactions or not.
        /// This parameter is used to know to which extend update requests should be rewritten to not interfer with normal storage usage.
        /// </summary>
        internal bool UseNativeTransactions
        {
            get
            {
                return _useNativeTransactions;
            }
        }

        /// <summary>
        /// Gets whether the Dataset in-memory RDF representation has changed and need to be persisted in the underlying storage on next request.
        /// </summary>
        internal bool HasDatasetChanged
        {
            get
            {
                return _datasetChanged;
            }
            set
            {
                _datasetChanged = value;
            }
        }

        /// <summary>
        /// Gets whether queries should be simply executed or monitored for SpinInference.
        /// TODO find some sexier name
        /// </summary>
        internal QueryMode QueryExecutionMode
        {
            get
            {
                return _queryExecutionMode;
            }
        }

        /// <summary>
        /// Gets the Dataset namespace prefixes map if supported.
        /// </summary>
        public INamespaceMapper Namespaces
        {
            get
            {
                return new NamespaceMapper();
            }
            private set { }
        }

        internal IUpdateableStorage UnderlyingStorage
        {
            get
            {
                return _storage;
            }
            private set { }
        }

        internal IEnumerable<IResource> DefaultGraphs
        {
            get
            {
                return _underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph)
                    .Union(_underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SPIN.ClassLibraryOntology))
                    .Select(t => _underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, t.Subject).Any() ? null : Resource.Get(t.Subject, _spinProcessor));
            }
            private set { }
        }

        internal IEnumerable<IResource> ActiveGraphs
        {
            get
            {
                List<Resource> graphs = _underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph)
                    .Select(t => _underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, t.Subject).Any() ? null : Resource.Get(t.Subject, _spinProcessor))
                   .ToList();
                graphs.Add(Resource.Get(_datasetNode, _spinProcessor));
                return graphs;
            }
            private set { }
        }


        #endregion

        #region ISparqlDataset specific overrides implementation

        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public void SetActiveGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(Uri graphUri)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public void ResetActiveGraph()
        {
            throw new NotImplementedException();
        }

        public void ResetDefaultGraph()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<Uri> DefaultGraphUris
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Uri> ActiveGraphUris
        {
            get { throw new NotImplementedException(); }
        }

        public bool UsesUnionDefaultGraph
        {
            get { return true; }
        }

        /// <summary>
        /// Adds a graph in the dataset and submits it to SPIN processing.
        /// If the graph already exists in the dataset, this will lead its replacement by the new graph.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public bool AddGraph(IGraph g)
        {
            // TODO handle the default graph Uri;
            Uri graphUri = g.BaseUri;
            INode graphNode = RDFUtil.CreateUriNode(graphUri);

            Triple t = _underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyUpdatesGraph, graphNode).Union(_underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, graphNode)).FirstOrDefault();
            if (t != null)
            {
                g.BaseUri = ((IUriNode)t.Subject).Uri;
                _storage.SaveGraph(g);
                g.BaseUri = graphUri;
            }
            else
            {
                _storage.SaveGraph(g);
                _underlyingRDFDataset.Assert(graphNode, RDF.type, SD.ClassGraph);
                _underlyingRDFDataset.Assert(graphNode, SPINRuntime.PropertyReplacesGraph, graphNode); // TODO will this work ?
            }
            return true;
        }

        /// <summary>
        /// Removes a graph from the Dataset.
        /// The underlying graph is not deleted from the store, however all pending changes on the graph are cancelled
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public bool RemoveGraph(Uri graphUri)
        {
            INode graphNode = RDFUtil.CreateUriNode(graphUri);
            // TODO handle this case on transactional storage
            if (_underlyingRDFDataset.ContainsTriple(new Triple(graphNode, RDF.type, SD.ClassGraph)))
            {
                _underlyingRDFDataset.Retract(graphNode, RDF.type, SD.ClassGraph);
                Triple t = _underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyUpdatesGraph, graphNode).Union(_underlyingRDFDataset.GetTriplesWithPredicateObject(SPINRuntime.PropertyReplacesGraph, graphNode)).FirstOrDefault();
                if (t != null)
                {
                    _underlyingRDFDataset.Retract(t);
                    _storage.DeleteGraph(((IUriNode)t.Subject).Uri);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            return _underlyingRDFDataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(graphUri), RDF.type, SD.ClassGraph));
        }

        public IEnumerable<IGraph> Graphs
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerable<Uri> GraphUris
        {
            get
            {
                return _underlyingRDFDataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph).Select(t => ((IUriNode)t.Subject).Uri);
            }
        }

        public IGraph this[Uri graphUri]
        {
            get
            {
                IGraph graph = new OnDemandGraph(_storage);
                graph.BaseUri = graphUri;
                return graph;
            }
        }

        public IGraph GetModifiableGraph(Uri graphUri)
        {
            IGraph graph = this[graphUri];
            // TODO add handlers to monitor changes to the graph;
            if (!_underlyingRDFDataset.ContainsTriple(new Triple(RDFUtil.CreateUriNode(graphUri), RDF.type, SD.ClassGraph)))
            {
                _underlyingRDFDataset.Assert(RDFUtil.CreateUriNode(graphUri), RDF.type, SD.ClassGraph);
            }
            return graph;
        }

        public object ExecuteQuery(string sparqlQuery)
        {
            return ExecuteQuery(_spinProcessor.BuildQuery(sparqlQuery));
        }

        internal object ExecuteQuery(IQuery spinQuery)
        {
            this.PersistDataset();
            IContextualSparqlPrinter sparqlFactory = new StringContextualSparqlPrinter(this);
            spinQuery.print(sparqlFactory);
            if (_queryExecutionMode == QueryMode.SpinInferencing && spinQuery is IConstruct)
            {
                ExecuteUpdate("");//TODO if the query is a CONSTRUCT query wrap it in a corresponding INSERT else return the query result normally
                return null; // TODO is this correct or should we return the execution graph ?
            }
            return _storage.Query(sparqlFactory.ToString());
        }

        public void ExecuteUpdate(string sparqlUpdateCommandSet)
        {
            foreach (IUpdate update in _spinProcessor.BuildUpdate(sparqlUpdateCommandSet))
            {
                ExecuteUpdate(update);
            }
        }

        internal void ExecuteUpdate(IUpdate spinQuery)
        {
            this.CreateUpdateControlledDataset(); // TODO ?remove this, it should be finally handled directly through the IContextualSparqlPrinter usage?
            this.PersistDataset();
            // TODO create a temporary SpinEval.ExecutionGraph in the dataset to monitor property deletions and new RDF.type triples...
            StringContextualSparqlPrinter sparqlFactory = new StringContextualSparqlPrinter(this);
            spinQuery.print(sparqlFactory);
            _storage.Update(sparqlFactory.getString());
        }

#if DEBUG
        public String getUpdateQuery(String spinQuery)
        {
            this.CreateUpdateControlledDataset(); // TODO ?remove this, it should be finally handled directly through the IContextualSparqlPrinter usage?
            foreach (IUpdate update in _spinProcessor.BuildUpdate(spinQuery))
            {
                this.PersistDataset();
                StringContextualSparqlPrinter sparqlFactory = new StringContextualSparqlPrinter(this);
                update.print(sparqlFactory);
                return sparqlFactory.getString();
            }
            return "";
        }
#endif

        public void Flush()
        {
            throw new NotImplementedException("TODO");
        }

        public void Discard()
        {
            throw new NotImplementedException("TODO");
        }

        #endregion


        #region ISparqlDataset not supported ?yet? methods

        public bool HasTriples
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> Triples
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
