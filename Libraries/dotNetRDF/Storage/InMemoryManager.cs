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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Provides a wrapper around an in-memory store
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful if you want to test out some code using temporary in-memory data before you run the code against a real store or if you are using some code that requires an <see cref="IStorageProvider">IStorageProvider</see> interface but you need the results of that code to be available directly in-memory.
    /// </para>
    /// </remarks>
    public class InMemoryManager 
        : BaseAsyncSafeConnector, IUpdateableStorage, IAsyncUpdateableStorage, IConfigurationSerializable
    {
        private ISparqlDataset _dataset;
        private SparqlQueryParser _queryParser;
        private SparqlUpdateParser _updateParser;
        private LeviathanQueryProcessor _queryProcessor;
        private LeviathanUpdateProcessor _updateProcessor;

        /// <summary>
        /// Creates a new In-Memory Manager which is a wrapper around a new empty in-memory store
        /// </summary>
        public InMemoryManager()
            : this(new InMemoryQuadDataset(false)) { }

        /// <summary>
        /// Creates a new In-Memory Manager which is a wrapper around an in-memory store
        /// </summary>
        /// <param name="store">Triple Store</param>
        public InMemoryManager(IInMemoryQueryableStore store)
            : this(new InMemoryQuadDataset(store, false)) { }

        /// <summary>
        /// Creates a new In-Memory Manager which is a wrapper around a SPARQL Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public InMemoryManager(ISparqlDataset dataset)
        {
            _dataset = dataset;
        }

        #region IStorageProvider Members

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI to load</param>
        public override void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI to load</param>
        public override void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            IGraph g = null;
            if (_dataset.HasGraph(graphUri))
            {
                g = _dataset[graphUri];
            }
            handler.Apply(g);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI to load</param>
        public override void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                LoadGraph(g, (Uri)null);
            }
            else
            {
                LoadGraph(g, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI to load</param>
        public override void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                LoadGraph(handler, (Uri)null);
            }
            else
            {
                LoadGraph(handler, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph</param>
        public override void SaveGraph(IGraph g)
        {
            if (_dataset.HasGraph(g.BaseUri))
            {
                _dataset.RemoveGraph(g.BaseUri);
            }
            _dataset.AddGraph(g);
            _dataset.Flush();
        }

        /// <summary>
        /// Gets the IO Behaviour for In-Memory stores
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples | IOBehaviour.ExplicitEmptyGraphs;
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (!_dataset.HasGraph(graphUri))
            {
                Graph temp = new Graph();
                temp.BaseUri = graphUri;
                _dataset.AddGraph(temp);
            }

            if ((additions != null && additions.Any()) || (removals != null && removals.Any()))
            {
                IGraph g = _dataset.GetModifiableGraph(graphUri);
                if (additions != null && additions.Any()) g.Assert(additions.ToList());
                if (removals != null && removals.Any()) g.Retract(removals.ToList());
            }

            _dataset.Flush();
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null)
            {
                UpdateGraph((Uri)null, additions, removals);
            }
            else if (graphUri.Equals(String.Empty))
            {
                UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                UpdateGraph(UriFactory.Create(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Returns that Triple level updates are supported
        /// </summary>
        public override bool UpdateSupported
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public override void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                IGraph g = _dataset.GetModifiableGraph(graphUri);
                g.Clear();
                g.Dispose();
            }
            else
            {
                _dataset.RemoveGraph(graphUri);
            }
            _dataset.Flush();
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public override void DeleteGraph(string graphUri)
        {
            if (graphUri == null)
            {
                DeleteGraph((Uri)null);
            }
            else if (graphUri.Equals(String.Empty))
            {
                DeleteGraph((Uri)null);
            }
            else
            {
                DeleteGraph(UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Returns that Graph Deletion is supported
        /// </summary>
        public override bool DeleteSupported
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Lists the URIs of Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            return _dataset.GraphUris;
        }

        /// <summary>
        /// Returns that listing graphs is supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Store is ready
        /// </summary>
        public override bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Returns that the Store is not read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            if (_queryParser == null) _queryParser = new SparqlQueryParser();
            SparqlQuery q = _queryParser.ParseFromString(sparqlQuery);

            if (_queryProcessor == null) _queryProcessor = new LeviathanQueryProcessor(_dataset);
            return _queryProcessor.ProcessQuery(q);
        }

        /// <summary>
        /// Makes a SPARQL Query against the Store processing the results with the appropriate processor from those given
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            if (_queryParser == null) _queryParser = new SparqlQueryParser();
            SparqlQuery q = _queryParser.ParseFromString(sparqlQuery);

            if (_queryProcessor == null) _queryProcessor = new LeviathanQueryProcessor(_dataset);
            _queryProcessor.ProcessQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Applies SPARQL Updates to the Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            if (_updateParser == null) _updateParser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = _updateParser.ParseFromString(sparqlUpdate);

            if (_updateProcessor == null) _updateProcessor = new LeviathanUpdateProcessor(_dataset);
            _updateProcessor.ProcessCommandSet(cmds);
        }

        #endregion

        #region Async Members

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            this.AsyncQuery(sparqlQuery, callback, state);
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            this.AsyncQueryHandlers(sparqlQuery, rdfHandler, resultsHandler, callback, state);
        }

        /// <summary>
        /// Updates the store asynchronously
        /// </summary>
        /// <param name="sparqlUpdates">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
        {
            this.AsyncUpdate(sparqlUpdates, callback, state);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Manager
        /// </summary>
        public override void Dispose()
        {
            _dataset.Flush();
        }

        #endregion

        /// <summary>
        /// Gets a String representation of the Manager
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[In-Memory]";
        }

        #region IConfigurationSerializable Members

        /// <summary>
        /// Serializes the Configuration of the Manager
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));

            context.Graph.Assert(manager, rdfType, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));
            context.Graph.Assert(manager, dnrType, context.Graph.CreateLiteralNode(GetType().ToString()));
            context.Graph.Assert(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString()));
        }

        #endregion
    }
}