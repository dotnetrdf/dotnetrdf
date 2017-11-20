/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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

        /// <summary>
        /// The SPARQL update command calls made by this wrapper
        /// </summary>
        public List<string> CommandCalls = new List<string>();

        internal bool DatasetDescriptionChanged;

        internal SpinDatasetDescription Configuration;

        internal IUpdateableStorage Storage;

        private SpinProcessor _spinProcessor = new SpinProcessor();

        private QueryMode _queryExecutionMode = QueryMode.UserQuerying;

        private readonly HashSet<IGraph> _synchronizedGraphs = new HashSet<IGraph>();

        private readonly HashSet<INode> _changedResources = new HashSet<INode>();

        #region Initialisation

        /// <summary>
        /// Inititalize a SpinWrapperDataset upon a storage engine using all the graphs in the store.
        /// </summary>
        /// <param name="storage"></param>
        internal SpinWrappedDataset(IUpdateableStorage storage)
        {
            Storage = storage;
            Configuration = SpinDatasetDescription.Load(Storage);
            Initialise();
        }

        /// <summary>
        /// Inititalize a SpinWrapperDataset upon a storage engine using a RDF SparqlDataset definition.
        /// </summary>
        /// <param name="datasetUri"></param>
        /// <param name="storage"></param>
        public SpinWrappedDataset(Uri datasetUri, IUpdateableStorage storage)
        {
            Storage = storage;
            Configuration = SpinDatasetDescription.Load(Storage, datasetUri);
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
            Storage = storage;
            Configuration = SpinDatasetDescription.Load(Storage, datasetUri, graphUris);
            Initialise();
        }

        internal void Initialise()
        {
            IEnumerable<Uri> localGraphs = Storage.ListGraphs();
            foreach (IUriNode spinLibrary in Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology).Select(t => t.Subject))
            {
                // TODO maybe clean this to use SPINImports instead
                if (localGraphs.Contains(spinLibrary.Uri))
                {
                    IGraph library = new ThreadSafeGraph();
                    library.BaseUri = spinLibrary.Uri;
                    Storage.LoadGraph(library, spinLibrary.Uri);
                    spinProcessor.Initialise(library);
                }
                else
                {
                    spinProcessor.Initialise(spinLibrary.Uri);
                }
            }
            // TODO explore the dataset to add spin:import triples
            Configuration.Changed += OnDatasetDescriptionChanged;
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
            DatasetDescriptionChanged = true;
            if (!Configuration.IsChanged)
            {
                Configuration.Changed -= OnDatasetDescriptionChanged;
                Configuration.EnableUpdateControl();
                Configuration.Changed += OnDatasetDescriptionChanged;
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
                return Storage;
            }
            private set { }
        }

        internal IEnumerable<IResource> DefaultGraphs
        {
            get
            {
                return Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                    .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassEntailmentGraph))
                    .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology))
                    .Select(t => t.Subject)
                    .Where(g =>
                        !Configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, g)
                        .Union(Configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyRemovesGraph, g)).Any())
                    .Select(g => AsResource(g))
                    .ToList();
            }
            private set { }
        }

        internal IEnumerable<IResource> ActiveGraphs
        {
            get
            {
                List<Resource> graphs = Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                    .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassEntailmentGraph))
                    .Select(t => t.Subject)
                    .Where(g =>
                        !Configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, g)
                        .Union(Configuration.GetTriplesWithPredicateObject(RDFRuntime.PropertyRemovesGraph, g)).Any())
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
                return Configuration.BaseUri;
            }
        }

        /// <summary>
        /// Returns the original dataset Uri
        /// </summary>
        public Uri SourceUri
        {
            get
            {
                return Configuration.SourceUri;
            }
        }

        internal void SaveConfiguration()
        {
            if (DatasetDescriptionChanged)
            {
                Storage.SaveGraph(Configuration);
                DatasetDescriptionChanged = false;
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
            Configuration.ImportGraph(graphUri);
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
                Storage.SaveGraph(resourceRestrictions);
                restrictionQuery = new SparqlParameterizedString(SparqlTemplates.SetExecutionContext);

                restrictionQuery.SetUri("resourceRestriction", executionContextUri);
                StringBuilder sb = new StringBuilder();
                foreach (Resource graph in DefaultGraphs)
                {
                    sb.AppendLine("USING <" + graph.Uri.ToString() + ">");
                }
                restrictionQuery.CommandText = restrictionQuery.CommandText.Replace("@USING_DEFAULT", sb.ToString());
                Storage.Update(restrictionQuery.ToString());
            }
            return executionContextUri;
        }

        private IGraph _updatesMonitorGraph;

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

        private readonly HashSet<Uri> _activeGraphs = new HashSet<Uri>(RDFUtil.uriComparer);
        private readonly HashSet<Uri> _defaultGraphs = new HashSet<Uri>(RDFUtil.uriComparer); // do we really need this one ?

        /// <inheritdoc />
        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            foreach (Uri graphUri in graphUris)
            {
                SetActiveGraph(graphUri);
            }
        }

        /// <inheritdoc />
        public void SetActiveGraph(Uri graphUri)
        {
            _activeGraphs.Add(graphUri);
        }

        /// <inheritdoc />
        public void SetDefaultGraph(Uri graphUri)
        {
            _defaultGraphs.Add(graphUri);
        }

        /// <inheritdoc />
        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            foreach (Uri graphUri in graphUris)
            {
                SetDefaultGraph(graphUri);
            }
        }

        /// <inheritdoc />
        public void ResetActiveGraph()
        {
            _activeGraphs.Clear();
        }

        /// <inheritdoc />
        public void ResetDefaultGraph()
        {
            _defaultGraphs.Clear();
        }

        /// <inheritdoc />
        public IEnumerable<Uri> DefaultGraphUris
        {
            get
            {
                HashSet<Uri> graphUris = new HashSet<Uri>();
                graphUris.UnionWith(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Select(t => ((IUriNode)t.Subject).Uri));
                graphUris.UnionWith(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology)
                    .Select(t => ((IUriNode)t.Subject).Uri));
                return graphUris;
                //return _defaultGraphs.ToList();
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool UsesUnionDefaultGraph => true;

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
            return Configuration.RemoveGraph(graphUri);
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            return Configuration.HasGraph(graphUri);
        }

        /// <inheritdoc />
        public IEnumerable<IGraph> Graphs
        {
            get
            {
                // TODO see whether we return a collection of SpinWrappedGraphs or if we still not support this feature
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public IEnumerable<Uri> GraphUris
        {
            get
            {
                // TODO relocate this in the SpinDatasetDescription class
                return Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph)
                    .Select(t => ((IUriNode)t.Subject).Uri);
            }
        }

        /// <inheritdoc />
        public IGraph this[Uri graphUri] => Configuration[graphUri];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public IGraph GetModifiableGraph(Uri graphUri)
        {
            return Configuration.GetModifiableGraph(graphUri);
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
            if (!Configuration.IsChanged)
            {
                return;
            }
            // TODO check if the updates did not raise any constraint violation, otherwise reject the Flush request
            // TODO related to the concurrency policy problem : handle any concurrent updates may have happened and succeded between the first modification and here
            SpinDatasetDescription updatedSourceDataset = new SpinDatasetDescription( Configuration.SourceUri);

            updatedSourceDataset.Assert(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SPIN.ClassLibraryOntology));
            updatedSourceDataset.Assert(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, SD.ClassGraph));

            foreach (SpinWrappedGraph g in Configuration.ModificableGraphs)
            {
                Uri updatedGraphUri = Configuration.GetUpdateControlUri(g.BaseUri);
                Uri sourceGraph = g.BaseUri;

                if (Configuration.IsGraphReplaced(sourceGraph))
                {
                    Storage.Update("WITH <" + updatedGraphUri.ToString() + "> DELETE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p }"); // For safety only
                    Storage.Update("MOVE GRAPH <" + updatedGraphUri.ToString() + "> TO <" + sourceGraph.ToString() + ">");
                }
                else if (Configuration.IsGraphUpdated(sourceGraph))
                {
                    Storage.Update("DELETE { GRAPH <" + updatedGraphUri.ToString() + "> { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p } . GRAPH <" + sourceGraph.ToString() + "> { ?s ?p ?o } } USING <" + updatedGraphUri.ToString() + "> USING NAMED <" + g.BaseUri.ToString() + "> WHERE { ?s <" + RDFRuntime.PropertyResets.Uri.ToString() + "> ?p . GRAPH <" + g.BaseUri.ToString() + "> { ?s ?p ?o} }");
                    Storage.Update("ADD GRAPH <" + updatedGraphUri.ToString() + "> TO <" + sourceGraph.ToString() + ">");
                }
                else
                {
                    updatedSourceDataset.Retract(Configuration.GetTriplesWithSubject(RDFUtil.CreateUriNode(sourceGraph)));
                }
            }
            // TODO update the original dataset instead of overwriting it
            Storage.SaveGraph(updatedSourceDataset);
            DisposeUpdateControlledDataset();
        }

        /// <inheritdoc />
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
                    Storage.DeleteGraph(CurrentExecutionContext);
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
            return Storage.Query(commandText.ToString());
        }

        internal void ExecuteUpdate(IConstruct spinUpdateCommandSet)
        {
        }

        private readonly HashSet<Triple> _loopPreventionChecks = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

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
                Storage.LoadGraph(remoteChanges, currentExecutionGraphUri);

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
                foreach (Uri graphUri in Configuration.GetTriplesRemovalsGraphs().Union(Configuration.GetTriplesAdditionsGraphs()))
                {
                    Storage.DeleteGraph(graphUri);
                }
                throw new Exception("", any);
            }
            finally
            {
                Storage.DeleteGraph(currentExecutionGraphUri);
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
            command.SetUri("datasetUri", Configuration.BaseUri);
            command.SetUri("outputGraph", outputGraphUri);

            this.SaveConfiguration();
            if (!(spinQuery is IInsertData || spinQuery is IDeleteData))
            {
                CommandCalls.Add(command.ToString());
                Storage.Update(command.ToString());
                UpdateInternal(new DeleteDataImpl(RDF.Nil, spinProcessor), outputGraphUri);
                UpdateInternal(new InsertDataImpl(RDF.Nil, spinProcessor), outputGraphUri);
                ResetActiveGraph();
                return;
            }
            SaveInMemoryChanges();

            HashSet<Uri> dataGraphs = new HashSet<Uri>(RDFUtil.uriComparer);
            dataGraphs.UnionWith(spinQuery is IDeleteData ? Configuration.GetTriplesRemovalsGraphs() : Configuration.GetTriplesAdditionsGraphs());
            try
            {
                Storage.Update(command.ToString());
                _ignoreMonitoredChangeEvents = true;
                // TODO if needed : postpone this until full update execution is done to alleviate I/O 
                foreach (IGraph synced in _synchronizedGraphs)
                {
                    Uri changesGraphUri = spinQuery is IDeleteData ? Configuration.GetTripleRemovalsMonitorUri(synced.BaseUri) : Configuration.GetTripleAdditionsMonitorUri(synced.BaseUri);
                    IGraph changes = new ThreadSafeGraph();
                    Storage.LoadGraph(changes, changesGraphUri);
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
                    Storage.DeleteGraph(graphUri);
                }
            }
        }

#if DEBUG
        /// <summary>
        /// Process a SPIN query and return the SPARQL Update commands that it compiles to
        /// </summary>
        /// <param name="spinQuery"></param>
        /// <returns></returns>
        public String GetUpdateQuery(String spinQuery)
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
            return Configuration.IsGraphModified(graphUri);
        }

        private void SaveInMemoryChanges()
        {
            foreach (SpinWrappedGraph g in Configuration.ModificableGraphs)
            {
                if (!g.IsChanged)
                {
                    continue;
                }
                SetActiveGraph(g.BaseUri);
                _hasPendingChanges = true;
                Storage.UpdateGraph(Configuration.GetTripleAdditionsMonitorUri(g), g.Additions, null);
                Storage.UpdateGraph(Configuration.GetTripleRemovalsMonitorUri(g), g.Removals, null);
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
            Storage.DeleteGraph(g.BaseUri);
            _ignoreMonitoredChangeEvents = false;
        }

        internal void DisposeUpdateControlledDataset()
        {
            if (!Configuration.IsChanged)
            {
                return;
            }
            // TODO simplify this
            IEnumerable<String> disposableGraphs = Configuration.GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesGraph)
                        .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassExecutionGraph))
                        .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassFunctionEvalResultSet))
                        .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph))
                        .Union(Configuration.GetTriplesWithPredicateObject(RDF.PropertyType, RDFRuntime.ClassUpdateControlledDataset))
                        .Select(t => ((IUriNode)t.Subject).Uri.ToString());
            foreach (String graphUri in disposableGraphs)
            {
                Storage.DeleteGraph(graphUri);
            }
            Configuration.Changed -= OnDatasetDescriptionChanged;
            Storage.DeleteGraph(Configuration.BaseUri);

            // Reloads the original dataset from the storage
            Uri sourceUri = Configuration.SourceUri;
            Configuration.Clear();
            Storage.LoadGraph(Configuration, Configuration.SourceUri);
            Configuration.DisableUpdateControl();
            Initialise();
        }


        #endregion

        #region ISparqlDataset not supported ?yet? methods

        /// <inheritdoc />
        public bool HasTriples
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> Triples
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
