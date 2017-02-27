using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.Constructors;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// Provides SPIN capabilities for a Dataset over any SPIN-unaware IUpdateableStorage (including the InMemoryManager).
    /// TODO decide how to handle the default unnamed graph case
    /// TODO design a concurrency management policy
    /// </summary>
    public class SpinWrappedDataset : ISparqlDataset
    {

        internal enum QueryMode
        {
            UserQuerying = 0,
            SpinInferencing = 1,
            SpinConstraintsChecking = 2
        }

        public List<String> commandCalls = new List<String>();

        internal bool _datasetDescriptionChanged;

        internal SpinDatasetDescription _configuration;

        internal IUpdateableStorage _storage;

        private SpinProcessor _spinProcessor = new SpinProcessor();

        private QueryMode _queryExecutionMode = QueryMode.UserQuerying;

        private HashSet<IGraph> _synchronizedGraphs = new HashSet<IGraph>();

        private HashSet<INode> _changedResources = new HashSet<INode>();

        #region Initialisation

        /// <summary>
        /// Inititalize a SpinWrapperDataset upon a storage engine using all the graphs in the store.
        /// </summary>
        /// <param name="storage"></param>
        internal SpinWrappedDataset(IUpdateableStorage storage)
        {
            _storage = storage;
            _configuration = SpinDatasetDescription.Load(_storage);
            Initialise();
        }

        /// <summary>
        /// Inititalize a SpinWrapperDataset upon a storage engine using a RDF SparqlDataset definition.
        /// </summary>
        /// <param name="datasetUri"></param>
        /// <param name="storage"></param>
        public SpinWrappedDataset(Uri datasetUri, IUpdateableStorage storage)
        {
            _storage = storage;
            _configuration = SpinDatasetDescription.Load(_storage, datasetUri);
            Initialise();
        }

        /// <summary>
        /// Inititalize a SPIN model upon a storage engine using a RDF SparqlDataset definition composed of the specified graphs.
        /// </summary>
        /// <param name="datasetUri"></param>
        /// <param name="storage"></param>
        /// <param name="graphUris"></param>
        public SpinWrappedDataset(Uri datasetUri, IUpdateableStorage storage, IEnumerable<Uri> graphUris)
        {
            _storage = storage;
            _configuration = SpinDatasetDescription.Load(_storage, datasetUri, graphUris);
            Initialise();
        }

        internal void Initialise()
        {
            IEnumerable<Uri> localGraphs = _storage.ListGraphs();
            foreach (IUriNode spinLibrary in _configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology).Select(t => t.Subject))
            {
                // TODO maybe clean this to use SPINImports instead
                if (localGraphs.Contains(spinLibrary.Uri))
                {
                    IGraph library = new ThreadSafeGraph();
                    library.BaseUri = spinLibrary.Uri;
                    _storage.LoadGraph(library, spinLibrary.Uri);
                    spinProcessor.Initialise(library);
                }
                else
                {
                    spinProcessor.Initialise(spinLibrary.Uri);
                }
            }
            // TODO explore the dataset to add spin:import triples
            _configuration.Changed += OnDatasetDescriptionChanged;
        }

        #endregion

        #region SpinWrapperDataset specifics

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

        internal Resource AsResource(INode node) {
            return Resource.Get(node, spinProcessor);
        }

        /// <summary>
        /// Event handler to monitor the current Dataset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void OnDatasetDescriptionChanged(object sender, GraphEventArgs args)
        {
            _datasetDescriptionChanged = true;
            if (!_configuration.IsChanged)
            {
                _configuration.Changed -= OnDatasetDescriptionChanged;
                _configuration.EnableUpdateControl();
                _configuration.Changed += OnDatasetDescriptionChanged;
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
            set
            {
                _queryExecutionMode = value;
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
                return _configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                    .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassEntailmentGraph))
                    .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology))
                    .Select(t => t.Subject)
                    .Where(g =>
                        !_configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, g)
                        .Union(_configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyRemovesGraph, g)).Any())
                    .Select(g => AsResource(g))
                    .ToList();
            }
            private set { }
        }

        internal IEnumerable<IResource> ActiveGraphs
        {
            get
            {
                List<Resource> graphs = _configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                    .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassEntailmentGraph))
                    .Select(t => t.Subject)
                    .Where(g =>
                        !_configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, g)
                        .Union(_configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyRemovesGraph, g)).Any())
                    .Select(g => AsResource(g))
                    .ToList();
                graphs.Add(AsResource(RDFUtil.CreateUriNode(Uri)));
                return graphs;
            }
            private set { }
        }

        /// <summary>
        /// Returns the current dataset Uri.
        /// If the dataset has been updated use SourceUri to get the orignal dataset Uri
        /// </summary>
        public Uri Uri
        {
            get
            {
                return _configuration.BaseUri;
            }
        }

        /// <summary>
        /// Returns the original dataset Uri
        /// </summary>
        public Uri SourceUri
        {
            get
            {
                return _configuration.SourceUri;
            }
        }

        internal void SaveConfiguration()
        {
            if (_datasetDescriptionChanged)
            {
                _storage.SaveGraph(_configuration);
                _datasetDescriptionChanged = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphUri"></param>
        public void ImportGraph(Uri graphUri)
        {
            IGraph graph = SPINImports.GetInstance().getImportedGraph(graphUri);
            ImportGraph(graph);
        }

        /// <summary>
        /// Imports a graph into the dataset and registers it as a SPIN.LibraryOntology for further SPIN processing.
        /// </summary>
        /// <param name="graph"></param>
        public void ImportGraph(IGraph graph)
        {
            // TODO forbid nulled URIed graphs or supply a composite URI ?
            // Since SPIN configuration should be processed at dataset load, we consider that subsequent imports should follow the normal SPIN processing pipeline.
            Uri graphUri = graph.BaseUri;
            // TODO handle the updates to the current SpinProcessor
            _configuration.ImportGraph(graphUri);
            AddGraph(graph);
        }

        private Uri _currentExecutionContext = null;
        internal Uri CurrentExecutionContext
        {
            get
            {
                return _currentExecutionContext;
            }
            private set
            {
                _currentExecutionContext = value;
            }
        }

        /// <summary>
        /// Builds a graph of rdf:type triples to restrict subsequent SPIN Constructors, Rules or Constraint checks evaluations
        /// </summary>
        /// <param name="resources"></param>
        internal Uri CreateExecutionContext(IEnumerable<INode> resources)
        {
            Uri executionContextUri = null;
            if (resources != null)
            {
                executionContextUri = RDFRuntime.NewTempGraphUri();
                SparqlParameterizedString restrictionQuery;
                IGraph resourceRestrictions = new ThreadSafeGraph();
                resourceRestrictions.BaseUri = executionContextUri;
                INode inputGraphNode = RDFUtil.CreateUriNode(executionContextUri);
                foreach (INode resource in resources)
                {
                    resourceRestrictions.Assert(inputGraphNode, RDFRuntime.PropertyExecutionRestrictedTo, resource);
                }
                _storage.SaveGraph(resourceRestrictions);
                restrictionQuery = new SparqlParameterizedString(SparqlTemplates.SetExecutionContext);

                restrictionQuery.SetUri("resourceRestriction", executionContextUri);
                StringBuilder sb = new StringBuilder();
                foreach (Resource graph in DefaultGraphs)
                {
                    sb.AppendLine("USING <" + graph.Uri.ToString() + ">");
                }
                restrictionQuery.CommandText = restrictionQuery.CommandText.Replace("@USING_DEFAULT", sb.ToString());
                _storage.Update(restrictionQuery.ToString());
            }
            return executionContextUri;
        }

        private IGraph _updatesMonitorGraph;
        private Uri _updatesMonitorGraphUri;
        /// <summary>
        /// Gets/sets a graph to monitor global changes to the dataset.
        /// Responsibility for the management of this graph is left to the caller.
        /// Changes will be notified only at the end of the update process to avoid inducing too much I/O with each partial result.
        /// </summary>
        public IGraph UpdatesMonitor
        {
            get
            {
                return _updatesMonitorGraph;
            }
            set
            {
                _updatesMonitorGraph = value;
            }
        }

        #endregion

        #region ISparqlDataset implementation

        private HashSet<Uri> _activeGraphs = new HashSet<Uri>(RDFUtil.uriComparer);
        private HashSet<Uri> _defaultGraphs = new HashSet<Uri>(RDFUtil.uriComparer); // do we really need this one ?

        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            foreach (Uri graphUri in graphUris)
            {
                SetActiveGraph(graphUri);
            }
        }

        public void SetActiveGraph(Uri graphUri)
        {
            _activeGraphs.Add(graphUri);
        }

        public void SetDefaultGraph(Uri graphUri)
        {
            _defaultGraphs.Add(graphUri);
        }

        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            foreach (Uri graphUri in graphUris)
            {
                SetDefaultGraph(graphUri);
            }
        }

        public void ResetActiveGraph()
        {
            _activeGraphs.Clear();
        }

        public void ResetDefaultGraph()
        {
            _defaultGraphs.Clear();
        }

        public IEnumerable<Uri> DefaultGraphUris
        {
            get
            {
                HashSet<Uri> graphUris = new HashSet<Uri>();
                graphUris.UnionWith(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Select(t => ((IUriNode)t.Subject).Uri));
                graphUris.UnionWith(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology)
                    .Select(t => ((IUriNode)t.Subject).Uri));
                return graphUris;
                //return _defaultGraphs.ToList();
            }
        }

        public IEnumerable<Uri> ActiveGraphUris
        {
            get
            {
                //HashSet<Uri> graphUris = new HashSet<Uri>(RDFUtil.uriComparer);
                //graphUris.UnionWith(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                //    .Select(t => ((IUriNode)t.Subject).Uri));
                //return graphUris;
                return _activeGraphs.ToList();
            }
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
            SpinWrappedGraph graph = (SpinWrappedGraph)GetModifiableGraph(g.BaseUri);
            graph.Reset();

            // Add graph handlers to monitor subsequent changes to the graph
            g.Changed += OnMonitoredGraphChanged;
            g.ClearRequested += OnMonitoredGraphClearRequested;
            g.Cleared += OnMonitoredGraphCleared;

            // Add the graph to the synchronized collection so remote updates are reflected locally on updates completion
            _synchronizedGraphs.Add(g);

            graph.Assert(g.Triples);
            ExecuteUpdate();

            return true;
        }

        /// <summary>
        /// Removes a graph from the Dataset.
        /// The underlying graph and it's entailments are not deleted from the store, however all pending changes on the graph are cancelled
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public bool RemoveGraph(Uri graphUri)
        {
            return _configuration.RemoveGraph(graphUri);
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            return _configuration.HasGraph(graphUri);
        }

        public IEnumerable<IGraph> Graphs
        {
            get
            {
                // TODO see whether we return a collection of SpinWrappedGraphs or if we still not support this feature
                throw new NotSupportedException();
            }
        }

        public IEnumerable<Uri> GraphUris
        {
            get
            {
                // TODO relocate this in the SpinDatasetDescription class
                return _configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Select(t => ((IUriNode)t.Subject).Uri);
            }
        }

        public IGraph this[Uri graphUri]
        {
            get
            {
                return _configuration[graphUri];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public IGraph GetModifiableGraph(Uri graphUri)
        {
            return _configuration.GetModifiableGraph(graphUri);
        }

        /// <summary>
        /// Executes a SPARQL query on the dataset
        /// </summary>
        /// <param name="sparqlQuery"></param>
        /// <returns></returns>
        public object ExecuteQuery(string sparqlQuery)
        {
            object result = ExecuteQuery(_spinProcessor.BuildQuery(sparqlQuery));
            ResetActiveGraph();
            return result;
        }


        /// <summary>
        /// Executes any pending SPARQL Update command from changes made to the dataset trough the API
        /// </summary>
        public void ExecuteUpdate()
        {
            ResetActiveGraph();
            SaveInMemoryChanges();
            if (_hasPendingChanges)
            {
                _hasPendingChanges = false; // for security only
                List<IUpdate> commands = new List<IUpdate>();
                commands.Add(new DeleteDataImpl(RDF.Nil, spinProcessor));
                commands.Add(new InsertDataImpl(RDF.Nil, spinProcessor));
                ExecuteUpdate(commands);
            }
            ResetActiveGraph();
        }

        /// <summary>
        /// Executes the SPARQL Update command on the dataset
        /// </summary>
        /// <param name="sparqlUpdateCommandSet"></param>
        public void ExecuteUpdate(string sparqlUpdateCommandSet)
        {
            ExecuteUpdate();
            ExecuteUpdate(_spinProcessor.BuildUpdate(sparqlUpdateCommandSet));
            ResetActiveGraph();
        }

        // TODO check whether transactions are supported by the storage provider to make those as atomical as possible
        /// <summary>
        /// Flushes changes to the dataset
        /// TODO handle dataset changes as updates instread of overwriting it to make it workable in a concurrent environment.
        /// </summary>
        public void Flush()
        {
            if (!_configuration.IsChanged)
            {
                return;
            }
            // TODO check if the updates did not raise any constraint violation, otherwise reject the Flush request
            // TODO related to the concurrency policy problem : handle any concurrent updates may have happened and succeded between the first modification and here
            SpinDatasetDescription updatedSourceDataset = new SpinDatasetDescription( _configuration.SourceUri);

            updatedSourceDataset.Assert(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology));
            updatedSourceDataset.Assert(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph));

            foreach (SpinWrappedGraph g in _configuration.ModificableGraphs)
            {
                Uri updatedGraphUri = _configuration.GetUpdateControlUri(g.BaseUri);
                Uri sourceGraph = g.BaseUri;

                if (_configuration.IsGraphReplaced(sourceGraph))
                {
                    _storage.Update("WITH <" + updatedGraphUri.ToString() + "> DELETE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p }"); // For safety only
                    _storage.Update("MOVE GRAPH <" + updatedGraphUri.ToString() + "> TO <" + sourceGraph.ToString() + ">");
                }
                else if (_configuration.IsGraphUpdated(sourceGraph))
                {
                    _storage.Update("DELETE { GRAPH <" + updatedGraphUri.ToString() + "> { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } . GRAPH <" + sourceGraph.ToString() + "> { ?s ?p ?o } } USING <" + updatedGraphUri.ToString() + "> USING NAMED <" + g.BaseUri.ToString() + "> WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p . GRAPH <" + g.BaseUri.ToString() + "> { ?s ?p ?o} }");
                    _storage.Update("ADD GRAPH <" + updatedGraphUri.ToString() + "> TO <" + sourceGraph.ToString() + ">");
                }
                else
                {
                    updatedSourceDataset.Retract(_configuration.GetTriplesWithSubject(RDFUtil.CreateUriNode(sourceGraph)));
                }
            }
            // TODO update the original dataset instead of overwriting it
            _storage.SaveGraph(updatedSourceDataset);
            DisposeUpdateControlledDataset();
        }

        public void Discard()
        {
            DisposeUpdateControlledDataset();
        }

        #endregion

        #region Internal query and commands processing implementation

        /// <summary>
        /// Resets internal registries
        /// </summary>
        internal void ResetExecutionContext()
        {
            if (_queryExecutionMode == QueryMode.UserQuerying)
            {
                _loopPreventionChecks.Clear();
                if (CurrentExecutionContext != null)
                {
                    _storage.DeleteGraph(CurrentExecutionContext);
                    CurrentExecutionContext = null;
                }
            }
        }

        internal object ExecuteQuery(IQuery spinQuery)
        {
            ExecuteUpdate();
            ISparqlPrinter sparqlFactory = new BaseSparqlPrinter(this);
            if (_queryExecutionMode != QueryMode.UserQuerying && spinQuery is IConstruct)
            {
                ExecuteUpdate((IConstruct)spinQuery);
                return null; // TODO is this correct or should we return the execution graph ?
            }
            SparqlParameterizedString commandText = sparqlFactory.GetCommandText(spinQuery);
            return _storage.Query(commandText.ToString());
        }

        internal void ExecuteUpdate(IConstruct spinUpdateCommandSet)
        {
        }

        private HashSet<Triple> _loopPreventionChecks = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

        // TODO Replace IEnumerable<IUpdate> with a SparqlUpdateCommandSet
        /// <summary>
        /// TODO find a way to compile the global changes so ExecutionContexts can be set globally for Rules processing or Constraints checking.
        /// </summary>
        /// <param name="spinUpdateCommandSet"></param>
        internal IGraph ExecuteUpdate(IEnumerable<IUpdate> spinUpdateCommandSet)
        {
            QueryMode currentQueryMode = QueryExecutionMode;
            Uri currentExecutionGraphUri = RDFRuntime.NewTempGraphUri();
            IGraph remoteChanges = new ThreadSafeGraph();
            remoteChanges.BaseUri = currentExecutionGraphUri;
            try
            {
                foreach (IUpdate update in spinUpdateCommandSet)
                {
                    if (update != null)
                    {
                        UpdateInternal(update, currentExecutionGraphUri);
                    }
                }
                _storage.LoadGraph(remoteChanges, currentExecutionGraphUri);

                List<Resource> newTypes = new List<Resource>();
                foreach (Triple t in remoteChanges.Triples)
                {
                    if (RDFUtil.sameTerm(RDF.PropertyType, t.Predicate))
                    {
                        // if remoteChanges contains an already checked rdf:type triple that means we have a infinite loop case in the SPIN processing pipeline so we stop execution
                        if (_loopPreventionChecks.Contains(t))
                        {
                            // TODO document better the exception cause
                            throw new SpinException("Infinite loop encountered. Execution is canceled");
                        }
                        _loopPreventionChecks.Add(t);
                        newTypes.Add(AsResource(t.Object));
                    }
                    else if (RDFUtil.sameTerm(RDFRuntime.PropertyHasChanged, t.Predicate)) 
                    {
                        // mark the resource as updated for the next global CheckConstraints or Apply rules
                        _changedResources.Add(t.Object);
                    }
                    else if (RDFUtil.sameTerm(RDFRuntime.PropertyResets, t.Predicate))
                    {
                        // TODO log the property resets for filtering pattern extensions in the subsequent SPARQL execution
                    }
                }
                CurrentExecutionContext = currentExecutionGraphUri;
                // run the constructors
                remoteChanges.Assert(this.RunConstructors(newTypes).Triples); 
            }
            catch (Exception any)
            {
                // for cleanliness sake on exception cases
                foreach (Uri graphUri in _configuration.GetTriplesRemovalsGraphs().Union(_configuration.GetTriplesAdditionsGraphs()))
                {
                    _storage.DeleteGraph(graphUri);
                }
                throw new Exception("", any);
            }
            finally
            {
                _storage.DeleteGraph(currentExecutionGraphUri);
                _queryExecutionMode = currentQueryMode;

                ResetExecutionContext();
            }
            return remoteChanges;
        }

        // TODO Replace IUpdate with a SparqlUpdateCommand
        private void UpdateInternal(IUpdate spinQuery, Uri outputGraphUri)
        {
            ISparqlPrinter sparqlFactory = new BaseSparqlPrinter(this);
            // TODO handle the current Execution Context while rewriting queries
            SparqlParameterizedString command = sparqlFactory.GetCommandText(spinQuery);
            command.SetUri("datasetUri", _configuration.BaseUri);
            command.SetUri("outputGraph", outputGraphUri);

            this.SaveConfiguration();
            if (!(spinQuery is IInsertData || spinQuery is IDeleteData))
            {
                commandCalls.Add(command.ToString());
                _storage.Update(command.ToString());
                UpdateInternal(new DeleteDataImpl(RDF.Nil, spinProcessor), outputGraphUri);
                UpdateInternal(new InsertDataImpl(RDF.Nil, spinProcessor), outputGraphUri);
                ResetActiveGraph();
                return;
            }
            SaveInMemoryChanges();

            HashSet<Uri> dataGraphs = new HashSet<Uri>(RDFUtil.uriComparer);
            dataGraphs.UnionWith(spinQuery is IDeleteData ? _configuration.GetTriplesRemovalsGraphs() : _configuration.GetTriplesAdditionsGraphs());
            try
            {
                _storage.Update(command.ToString());
                _ignoreMonitoredChangeEvents = true;
                // TODO if needed : postpone this until full update execution is done to alleviate I/O 
                foreach (IGraph synced in _synchronizedGraphs)
                {
                    Uri changesGraphUri = spinQuery is IDeleteData ? _configuration.GetTripleRemovalsMonitorUri(synced.BaseUri) : _configuration.GetTripleAdditionsMonitorUri(synced.BaseUri);
                    IGraph changes = new ThreadSafeGraph();
                    _storage.LoadGraph(changes, changesGraphUri);
                    if (spinQuery is IDeleteData)
                    {
                        synced.Retract(changes.Triples);
                    }
                    else {
                        synced.Assert(changes.Triples);
                    }
                }
                _ignoreMonitoredChangeEvents = false;
            }
            finally
            {
                foreach (Uri graphUri in dataGraphs)
                {
                    _storage.DeleteGraph(graphUri);
                }
            }
        }

#if DEBUG
        public String getUpdateQuery(String spinQuery)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IUpdate update in _spinProcessor.BuildUpdate(spinQuery))
            {
                this.SaveConfiguration();
                BaseSparqlPrinter sparqlFactory = new BaseSparqlPrinter(this);
                sb.Append(sparqlFactory.GetCommandText(update));
                sb.AppendLine();
            }
            return sb.ToString();
        }
#endif

        #endregion

        #region Internal graph handling

        private bool _hasPendingChanges = false;

        internal bool IsGraphModified(Uri graphUri) {
            return _configuration.IsGraphModified(graphUri);
        }

        private void SaveInMemoryChanges()
        {
            foreach (SpinWrappedGraph g in _configuration.ModificableGraphs)
            {
                if (!g.IsChanged)
                {
                    continue;
                }
                SetActiveGraph(g.BaseUri);
                _hasPendingChanges = true;
                _storage.UpdateGraph(_configuration.GetTripleAdditionsMonitorUri(g), g.additions, null);
                _storage.UpdateGraph(_configuration.GetTripleRemovalsMonitorUri(g), g.removals, null);
                g.Reset();
            }
        }

        private bool _ignoreMonitoredChangeEvents = false;

        internal void OnMonitoredGraphChanged(object sender, GraphEventArgs e)
        {
            if (_ignoreMonitoredChangeEvents) return;
            Uri graphUri = e.Graph.BaseUri;
            SpinWrappedGraph g = (SpinWrappedGraph)GetModifiableGraph(graphUri);
            if (e.TripleEvent != null)
            {
                if (e.TripleEvent.WasAsserted)
                {
                    g.Assert(e.TripleEvent.Triple);
                }
                else
                {
                    g.Retract(e.TripleEvent.Triple);
                }
            }
        }

        internal void OnMonitoredGraphClearRequested(object sender, GraphEventArgs e)
        {
            _ignoreMonitoredChangeEvents = true;
        }

        internal void OnMonitoredGraphCleared(object sender, GraphEventArgs e)
        {
            SpinWrappedGraph g = (SpinWrappedGraph)GetModifiableGraph(e.Graph.BaseUri);
            g.Clear();
            _storage.DeleteGraph(g.BaseUri);
            _ignoreMonitoredChangeEvents = false;
        }

        internal void DisposeUpdateControlledDataset()
        {
            if (!_configuration.IsChanged)
            {
                return;
            }
            // TODO simplify this
            IEnumerable<String> disposableGraphs = _configuration.GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesGraph)
                        .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassExecutionGraph))
                        .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassFunctionEvalResultSet))
                        .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                        .Union(_configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset))
                        .Select(t => ((IUriNode)t.Subject).Uri.ToString());
            foreach (String graphUri in disposableGraphs)
            {
                _storage.DeleteGraph(graphUri);
            }
            _configuration.Changed -= OnDatasetDescriptionChanged;
            _storage.DeleteGraph(_configuration.BaseUri);

            // Reloads the original dataset from the storage
            Uri sourceUri = _configuration.SourceUri;
            _configuration.Clear();
            _storage.LoadGraph(_configuration, _configuration.SourceUri);
            _configuration.DisableUpdateControl();
            Initialise();
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
