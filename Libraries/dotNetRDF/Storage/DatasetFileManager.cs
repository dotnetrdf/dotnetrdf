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
using System.ComponentModel;
using System.IO;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Allows you to treat an RDF Dataset File - NQuads, TriG or TriX - as a read-only generic store
    /// </summary>
    public class DatasetFileManager 
        : BaseAsyncSafeConnector, IQueryableStorage, IConfigurationSerializable
    {
        private TripleStore _store = new TripleStore();
        private bool _ready = false;
        private String _filename;

        /// <summary>
        /// Creates a new Dataset File Manager
        /// </summary>
        /// <param name="filename">File to load from</param>
        /// <param name="isAsync">Whether to load asynchronously</param>
        public DatasetFileManager(string filename, bool isAsync)
        {
            if (!File.Exists(filename)) throw new RdfStorageException("Cannot connect to a Dataset File that doesn't exist");
            _filename = filename;

            if (isAsync)
            {
                var asyncLoader =
                    new Thread(new ThreadStart(delegate { Initialise(filename); })) {IsBackground = true};
                asyncLoader.Start();
            }
            else
            {
                Initialise(filename);
            }
        }

        /// <summary>
        /// Internal helper method for loading the data
        /// </summary>
        /// <param name="filename">File to load from</param>
        private void Initialise(String filename)
        {
            try
            {
                IStoreReader reader = MimeTypesHelper.GetStoreParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(filename));
                reader.Load(_store, filename);

                _ready = true;
            }
            catch (RdfException rdfEx)
            {
                throw new RdfStorageException("An Error occurred while trying to read the Dataset File", rdfEx);
            }
        }

        /// <summary>
        /// Makes a query against the in-memory copy of the Stores data
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object Query(string sparqlQuery)
        {
            var queryProcessor = new LeviathanQueryProcessor(_store);
            var queryParser = new SparqlQueryParser();
            var query = queryParser.ParseFromString(sparqlQuery);
            return queryProcessor.ProcessQuery(query);
        }

        /// <summary>
        /// Makes a query against the in-memory copy of the Stores data processing the results with one of the given handlers
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            var queryProcessor = new LeviathanQueryProcessor(_store);
            var queryParser = new SparqlQueryParser();
            var query = queryParser.ParseFromString(sparqlQuery);
            queryProcessor.ProcessQuery(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Loads a Graph from the Dataset
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public override void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Dataset with the given Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public override void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            IGraph g = null;
            if (graphUri == null)
            {
                if (_store.HasGraph(graphUri))
                {
                    g = _store[graphUri];
                }
            }
            else if (_store.HasGraph(graphUri))
            {
                g = _store[graphUri];
            }

            if (g == null) return;
            handler.Apply(g);
        }

        /// <summary>
        /// Loads a Graph from the Dataset
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public override void LoadGraph(IGraph g, String graphUri)
        {
            if (graphUri.Equals(String.Empty))
            {
                LoadGraph(g, (Uri)null);
            }
            else
            {
                LoadGraph(g, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Dataset with the given Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public override void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (graphUri.Equals(String.Empty))
            {
                LoadGraph(handler, (Uri)null);
            }
            else
            {
                LoadGraph(handler, UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <exception cref="RdfStorageException">Always thrown since this Manager provides a read-only connection</exception>
        public override void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Gets the Save Behaviour of the Store
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.ReadOnlyGraphStore;
            }
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Returns that Updates are not supported since this is a read-only connection
        /// </summary>
        public override bool UpdateSupported
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Throws an error since this connection is read-only
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph from a read-only connection</exception>
        public override void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this connection is read-only
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph from a read-only connection</exception>
        public override void DeleteGraph(String graphUri)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Returns that deleting graphs is not supported
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that the Manager is ready if the underlying file has been loaded
        /// </summary>
        public override bool IsReady
        {
            get
            {
                return _ready;
            }
        }

        /// <summary>
        /// Returns that the Manager is read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the list of URIs of Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            return _store.Graphs.GraphUris;
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
        /// Gets the Source File this manager represents a read-only view of
        /// </summary>
        [Description("The Source File from which the dataset originates")]
        public String SourceFile
        {
            get
            {
                return _filename;
            }
        }

        /// <summary>
        /// Gets the String representation of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Dataset File] " + _filename;
        }


        /// <summary>
        /// Disposes of the Manager
        /// </summary>
        public override void Dispose()
        {
            _store.Dispose();
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode file = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, file, context.Graph.CreateLiteralNode(_filename)));
        }
    }
}