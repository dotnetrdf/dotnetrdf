using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.LibraryOntology;

namespace VDS.RDF.Query.Spin.Util
{
    // TODO override the BaseUri to reflect the source dataset + add a "current Uri" property for modified datasets
    /// <summary>
    /// 
    /// </summary>
    public class SpinDatasetDescription : ThreadSafeGraph
    {

        internal static Dictionary<Uri, IGraph> datasets = new Dictionary<Uri, IGraph>(RDFUtil.uriComparer);

        private Dictionary<Uri, SpinWrappedGraph> _modificableGraphs = new Dictionary<Uri, SpinWrappedGraph>(RDFUtil.uriComparer);

        private bool _isDirty = false;

        #region Static methods

        internal static SpinDatasetDescription Load(IUpdateableStorage storage, Uri datasetUri = null, IEnumerable<Uri> graphsUri = null)
        {
            SpinDatasetDescription dataset;
            if (datasetUri == null)
            {
                SparqlResultSet datasetDiscovery = (SparqlResultSet)storage.Query("SELECT ?dataset WHERE {?dataset a <" + SD.ClassDataset.Uri.ToString() + ">}");
                int datasetCount = datasetDiscovery.Results.Count;
                if (datasetCount > 1)
                {
                    throw new Exception("More than one dataset has been found in the current storage provider. Please specify which to use through the datasetUri parameter.");
                }
                else if (datasetCount == 1)
                {
                    datasetUri = ((IUriNode)datasetDiscovery.Results.FirstOrDefault().Value("dataset")).Uri;
                }
                else
                {
                    datasetUri = UriFactory.Create(RDFRuntime.DATASET_NS_URI + Guid.NewGuid().ToString());
                }
            }
            /* Do not cache yet
            if (datasets.ContainsKey(datasetUri))
            {
                dataset = datasets[datasetUri];
                if (dataset.GetTriplesWithPredicateObject(RDF.type, SD.ClassGraph).Any())
                {
                    return dataset;
                }
            }
            */
            dataset = new SpinDatasetDescription();
            dataset.BaseUri = datasetUri;
            storage.LoadGraph(dataset, datasetUri);
            dataset.Assert(RDFUtil.CreateUriNode(datasetUri), RDF.PropertyType, SD.ClassDataset);

            /* Do not cache yet
            datasets[datasetUri] = dataset;
            */
            return dataset;
        }

        #endregion

        internal SpinDatasetDescription()
        {
        }

        #region Public methods

        public bool RemoveGraph(Uri graphUri)
        {
            if (HasGraph(graphUri))
            {
                IUriNode sourceGraph = RDFUtil.CreateUriNode(graphUri);
                this.Retract(sourceGraph, RDF.PropertyType, SD.ClassGraph);
                this.Assert(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyRemovesGraph, sourceGraph);
                return true;
            }
            else
            {
                Retract(Triples.Where(t => t.Involves(graphUri)).ToList());
                return true;
            }
            return false;
        }

        public bool ImportGraph(Uri graphUri)
        {
            INode graph = RDFUtil.CreateUriNode(graphUri);
            this.Assert(graph, RDF.PropertyType, SPIN.ClassLibraryOntology);
            return true;
        }

        public bool HasGraph(Uri graphUri)
        {
            return ContainsTriple(new Triple(RDFUtil.CreateUriNode(graphUri), RDF.PropertyType, SD.ClassGraph));
        }

        public IEnumerable<SpinWrappedGraph> ModificableGraphs
        {
            get
            {
                return _modificableGraphs.Values.ToList();
            }
        }

        public IGraph this[Uri graphUri]
        {
            get
            {
                IGraph graph = new SpinWrappedGraph();
                graph.BaseUri = graphUri;
                return graph;
            }
        }

        public IGraph GetModifiableGraph(Uri graphUri)
        {
            return GetModifiableGraph(graphUri, RDFRuntime.PropertyUpdatesGraph);
        }

        public bool IsChanged
        {
            get
            {
                return !RDFUtil.sameTerm(BaseUri, SourceUri);
            }
            private set { }
        }

        public Uri SourceUri
        {
            get
            {
                IUriNode sourceDataset = GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesDataset).Select(t => (IUriNode)t.Object).FirstOrDefault();
                return sourceDataset == null ? BaseUri : sourceDataset.Uri;
            }
        }

        #endregion

        #region Internal implementation

        // TODO change the name into something sensible
        private void MakeWritable()
        {
            if (!GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesDataset).Any())
            {
                IUriNode sourceDataset = RDFUtil.CreateUriNode(BaseUri);
                BaseUri = RDFRuntime.NewTempDatasetUri();
                this.Retract(sourceDataset, RDF.PropertyType, SD.ClassDataset);
                this.Assert(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyUpdatesDataset, sourceDataset);
            }
        }

        // TODO change the name into something sensible
        internal void MakeReadOnly()
        {
            IUriNode sourceDataset = GetTriplesWithSubjectPredicate(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyUpdatesDataset).Select(t => (IUriNode)t.Object).FirstOrDefault();
            BaseUri = sourceDataset.Uri;
        }

        private IGraph GetModifiableGraph(Uri graphUri, INode modificationMode)
        {
            if (_modificableGraphs.ContainsKey(graphUri))
            {
                return _modificableGraphs[graphUri];
            }
            if (ContainsTriple(new Triple(RDFUtil.CreateUriNode(graphUri), RDF.PropertyType, RDFRuntime.ClassReadOnlyGraph)))
            {
                throw new SpinException("The graph " + graphUri.ToString() + " is marked as Readonly for the current dataset");
            }

            MakeWritable();

            IUriNode sourceGraph = RDFUtil.CreateUriNode(graphUri);
            IUriNode updateControlGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(graphUri));
            this.Retract(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyRemovesGraph, sourceGraph);
            this.Assert(sourceGraph, RDF.PropertyType, SD.ClassGraph);

            if (!RDFUtil.sameTerm(sourceGraph, updateControlGraph))
            {
                this.Assert(updateControlGraph, modificationMode, sourceGraph);
                this.Assert(updateControlGraph, RDFRuntime.PropertyUpdatesGraph, updateControlGraph);
            }
            else
            {
                this.Assert(updateControlGraph, RDFRuntime.PropertyUpdatesGraph, sourceGraph);
            }

            SpinWrappedGraph graph = (SpinWrappedGraph)this[graphUri];
            graph.BaseUri = graphUri;
            graph._readonly = false;
            graph.Changed += OnModificableGraphChange;
            graph.Cleared += OnModificableGraphCleared;
            _modificableGraphs[graphUri] = graph;
            return graph;
        }

        internal IEnumerable<Uri> GetTriplesAdditionsGraphs()
        {
            return GetTriplesWithPredicate(RDFRuntime.PropertyAddTriplesTo)
                .Select(t => ((IUriNode)t.Subject).Uri);
        }

        internal IEnumerable<Uri> GetTriplesRemovalsGraphs()
        {
            return GetTriplesWithPredicate(RDFRuntime.PropertyDeleteTriplesFrom)
                .Select(t => ((IUriNode)t.Subject).Uri);
        }

        private void OnModificableGraphChange(object sender, GraphEventArgs e)
        {
            if (e.TripleEvent != null)
            {
                Uri graphUri = e.TripleEvent.GraphUri;
                IUriNode graphNode = RDFUtil.CreateUriNode(GetUpdateControlUri(graphUri));
                if (e.TripleEvent.WasAsserted) {
                    this.Assert(RDFUtil.CreateUriNode(GetTripleAdditionsMonitorUri(graphUri)), RDFRuntime.PropertyAddTriplesTo, graphNode);
                } else {
                    this.Assert(RDFUtil.CreateUriNode(GetTripleRemovalsMonitorUri(graphUri)), RDFRuntime.PropertyDeleteTriplesFrom, graphNode);
                }
            }
        }

        private void OnModificableGraphCleared(object sender, GraphEventArgs e)
        {
            SpinWrappedGraph graph = (SpinWrappedGraph)e.Graph;
            graph.Reset();

            IUriNode sourceGraph = RDFUtil.CreateUriNode(graph.BaseUri);
            IUriNode graphNode = RDFUtil.CreateUriNode(GetUpdateControlUri(graph.BaseUri));
            this.Retract(graphNode, RDFRuntime.PropertyUpdatesGraph, sourceGraph);
            this.Assert(graphNode, RDFRuntime.PropertyReplacesGraph, sourceGraph);
        }

        internal Uri GetUpdateControlUri(Uri graphUri)
        {

            Uri uri = GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, RDFUtil.CreateUriNode(graphUri))
                .Union(GetTriplesWithPredicateObject(RDFRuntime.PropertyUpdatesGraph, RDFUtil.CreateUriNode(graphUri)))
                .Select(t => ((IUriNode)t.Subject).Uri)
                .FirstOrDefault();
            if (uri == null)
            {
                uri = RDFRuntime.NewUpdateControlGraphUri();
            }
            return uri;
        }

        internal Uri GetTripleAdditionsMonitorUri(SpinWrappedGraph graph)
        {
            return GetTripleAdditionsMonitorUri(GetUpdateControlUri(graph.BaseUri));
        }

        private Dictionary<Uri, Uri> _additionGraphs = new Dictionary<Uri, Uri>(RDFUtil.uriComparer);
        internal Uri GetTripleAdditionsMonitorUri(Uri graphUri)
        {
            if (_additionGraphs.ContainsKey(graphUri))
            {
                return _additionGraphs[graphUri];
            }
            INode monitoredGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(graphUri));
            Uri uri = RDFRuntime.NewTempGraphUri();
            _additionGraphs[graphUri] = uri;
            this.Assert(RDFUtil.CreateUriNode(uri), RDFRuntime.PropertyAddTriplesTo, monitoredGraph);
            return uri;
        }

        internal Uri GetTripleRemovalsMonitorUri(SpinWrappedGraph graph)
        {
            return GetTripleRemovalsMonitorUri(GetUpdateControlUri(graph.BaseUri));
        }

        private Dictionary<Uri, Uri> _removalGraphs = new Dictionary<Uri, Uri>(RDFUtil.uriComparer);
        internal Uri GetTripleRemovalsMonitorUri(Uri graphUri)
        {
            if (_removalGraphs.ContainsKey(graphUri))
            {
                return _removalGraphs[graphUri];
            }
            INode monitoredGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(graphUri));
            Uri uri = RDFRuntime.NewTempGraphUri();
            _removalGraphs[graphUri] = uri;
            this.Assert(RDFUtil.CreateUriNode(uri), RDFRuntime.PropertyDeleteTriplesFrom, monitoredGraph);
            return uri;
        }

        internal bool IsGraphReplaced(Uri sourceGraph)
        {
            return GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, RDFUtil.CreateUriNode(sourceGraph)).Any();
        }

        internal bool IsGraphUpdated(Uri sourceGraph)
        {
            return GetTriplesWithPredicateObject(RDFRuntime.PropertyUpdatesGraph, RDFUtil.CreateUriNode(sourceGraph)).Any();
        }

        #endregion

    }
}

