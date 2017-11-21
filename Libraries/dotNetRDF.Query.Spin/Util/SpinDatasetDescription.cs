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
using System.Runtime.CompilerServices;
using VDS.RDF.Storage;
using VDS.RDF.Query.Spin.LibraryOntology;

namespace VDS.RDF.Query.Spin.Util
{
    // TODO override the BaseUri to reflect the source dataset + add a "current Uri" property for modified datasets
    /// <summary>
    /// 
    /// </summary>
    internal class SpinDatasetDescription : ThreadSafeGraph
    {

        internal static Dictionary<Uri, IGraph> datasets = new Dictionary<Uri, IGraph>(RDFUtil.uriComparer);

        private Uri _sourceUri;

        private Dictionary<Uri, SpinWrappedGraph> _modificableGraphs = new Dictionary<Uri, SpinWrappedGraph>(RDFUtil.uriComparer);

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
            dataset = new SpinDatasetDescription(datasetUri);
            storage.LoadGraph(dataset, datasetUri);
            dataset.BaseUri = datasetUri;
            Triple isUpdateControlledDataset = dataset.GetTriplesWithPredicate(RDFRuntime.PropertyUpdatesDataset).FirstOrDefault();
            if (isUpdateControlledDataset!=null)
            {
                dataset._sourceUri = ((IUriNode)isUpdateControlledDataset.Object).Uri;
            }
            else
            {
                dataset.Assert(RDFUtil.CreateUriNode(datasetUri), RDF.PropertyType, SD.ClassDataset);
            }
            return dataset;
        }

        #endregion

        internal SpinDatasetDescription(Uri datasetUri)
        {
            BaseUri = datasetUri;
            _sourceUri = datasetUri;
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
            Retract(Triples.Where(t => t.Involves(graphUri)).ToList());
            return true;
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
        }

        public Uri SourceUri
        {
            get
            {
                return _sourceUri;
            }
        }

        #endregion

        #region Internal implementation

        // TODO change the name into something sensible
        internal void EnableUpdateControl()
        {
            if (!IsChanged)
            {
                IUriNode sourceDataset = RDFUtil.CreateUriNode(_sourceUri);
                BaseUri = RDFRuntime.NewTempDatasetUri();
                this.Retract(sourceDataset, RDF.PropertyType, SD.ClassDataset);
                this.Assert(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyUpdatesDataset, sourceDataset);
            }
        }

        internal void DisableUpdateControl() {
            BaseUri = _sourceUri;
            _additionGraphs.Clear();
            _removalGraphs.Clear();
            foreach (SpinWrappedGraph g in _modificableGraphs.Values) {
                g.Changed -= OnModificableGraphChange;
                g.Cleared -= OnModificableGraphCleared;
            }
            _modificableGraphs.Clear();
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

            IUriNode sourceGraph = RDFUtil.CreateUriNode(graphUri);
            IUriNode updateControlGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(graphUri, true));
            this.Retract(RDFUtil.CreateUriNode(BaseUri), RDFRuntime.PropertyRemovesGraph, sourceGraph);
            this.Assert(sourceGraph, RDF.PropertyType, SD.ClassGraph);
            this.Assert(updateControlGraph, RDF.PropertyType, RDFRuntime.ClassUpdateControlGraph);

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
            graph.Readonly = false;
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

        internal Uri GetUpdateControlUri(Uri graphUri, bool create=true)
        {

            Uri uri = GetTriplesWithPredicateObject(RDFRuntime.PropertyReplacesGraph, RDFUtil.CreateUriNode(graphUri))
                .Union(GetTriplesWithPredicateObject(RDFRuntime.PropertyUpdatesGraph, RDFUtil.CreateUriNode(graphUri)))
                .Select(t => ((IUriNode)t.Subject).Uri)
                .FirstOrDefault();
            if (uri == null && create)
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
            IUriNode monitoredGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(GetModifiableGraph(graphUri).BaseUri));
            Uri uri = RDFRuntime.NewTempGraphUri();
            _additionGraphs[graphUri] = uri;
            _additionGraphs[monitoredGraph.Uri] = uri;
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
            EnableUpdateControl();
            if (_removalGraphs.ContainsKey(graphUri))
            {
                return _removalGraphs[graphUri];
            }
            IUriNode monitoredGraph = RDFUtil.CreateUriNode(GetUpdateControlUri(GetModifiableGraph(graphUri).BaseUri));
            Uri uri = RDFRuntime.NewTempGraphUri();
            _removalGraphs[graphUri] = uri;
            _removalGraphs[monitoredGraph.Uri] = uri;
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

        internal bool IsGraphModified(Uri sourceGraph)
        {
            return IsGraphUpdated(sourceGraph) || IsGraphReplaced(sourceGraph);
        }

        #endregion

    }
}

