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
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Provides a Read-Only wrapper that can be placed around another <see cref="IStorageProvider">IStorageProvider</see> instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance
    /// </para>
    /// </remarks>
    public class ReadOnlyConnector 
        : IStorageProvider, IConfigurationSerializable
    {
        private IStorageProvider _manager;

        /// <summary>
        /// Creates a new Read-Only connection which is a read-only wrapper around another store
        /// </summary>
        /// <param name="manager">Manager for the Store you want to wrap as read-only</param>
        public ReadOnlyConnector(IStorageProvider manager)
        {
            _manager = manager;
        }

        #region IStorageProvider Members

        /// <summary>
        /// Gets the parent server (if any)
        /// </summary>
        public virtual IStorageServer ParentServer
        {
            get
            {
                return _manager.ParentServer;
            }
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            _manager.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, string graphUri)
        {
            _manager.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            _manager.LoadGraph(handler, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            _manager.LoadGraph(handler, graphUri);
        }

        /// <summary>
        /// Throws an exception since you cannot save a Graph using a read-only connection
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot save a Graph using a read-only connection</exception>
        public void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        /// <summary>
        /// Gets the IO Behaviour of the read-only connection taking into account the IO Behaviour of the underlying store
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return (_manager.IOBehaviour | IOBehaviour.IsReadOnly) & (IOBehaviour.HasDefaultGraph | IOBehaviour.HasDefaultGraph | IOBehaviour.IsQuadStore | IOBehaviour.IsTripleStore | IOBehaviour.IsReadOnly);
            }
        }

        /// <summary>
        /// Throws an exception since you cannot update a Graph using a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of the Graph</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot update a Graph using a read-only connection</exception>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        /// <summary>
        /// Throws an exception since you cannot update a Graph using a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of the Graph</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot update a Graph using a read-only connection</exception>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        /// <summary>
        /// Returns that Update is not supported
        /// </summary>
        public bool UpdateSupported
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Throws an exception as you cannot delete a Graph using a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph using a read-only connection</exception>
        public void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        /// <summary>
        /// Throws an exception as you cannot delete a Graph using a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph using a read-only connection</exception>
        public void DeleteGraph(string graphUri)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        /// <summary>
        /// Returns that deleting graphs is not supported
        /// </summary>
        public bool DeleteSupported
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Gets the list of graphs in the underlying store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            return _manager.ListGraphs();
        }

        /// <summary>
        /// Returns whether listing graphs is supported by the underlying store
        /// </summary>
        public virtual bool ListGraphsSupported
        {
            get
            {
                return _manager.ListGraphsSupported;
            }
        }

        /// <summary>
        /// Returns whether the Store is ready
        /// </summary>
        public bool IsReady
        {
            get 
            {
                return _manager.IsReady; 
            }

        }

        /// <summary>
        /// Returns that the Store is read-only
        /// </summary>
        public bool IsReadOnly
        {
            get 
            {
                return true; 
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Store
        /// </summary>
        public void Dispose()
        {
            _manager.Dispose();
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Manager
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Read Only]" + _manager.ToString();
        }

        /// <summary>
        /// Serializes the Configuration of the Manager
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode storageProvider = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            context.Graph.Assert(manager, rdfType, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));
            context.Graph.Assert(manager, dnrType, context.Graph.CreateLiteralNode(GetType().ToString()));
            context.Graph.Assert(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString()));

            if (_manager is IConfigurationSerializable)
            {
                INode managerObj = context.Graph.CreateBlankNode();
                context.NextSubject = managerObj;
                ((IConfigurationSerializable)_manager).SerializeConfiguration(context);
                context.Graph.Assert(manager, storageProvider, managerObj);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration as the underlying IStorageProvider is not serializable");
            }
        }
    }

    /// <summary>
    /// Provides a Read-Only wrapper that can be placed around another <see cref="IQueryableStorage">IQueryableStorage</see> instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance
    /// </para>
    /// </remarks>
    public class QueryableReadOnlyConnector
        : ReadOnlyConnector, IQueryableStorage
    {
        private IQueryableStorage _queryManager;

        /// <summary>
        /// Creates a new Queryable Read-Only connection which is a read-only wrapper around another store
        /// </summary>
        /// <param name="manager">Manager for the Store you want to wrap as read-only</param>
        public QueryableReadOnlyConnector(IQueryableStorage manager)
            : base(manager)
        {
            _queryManager = manager;
        }

        /// <summary>
        /// Executes a SPARQL Query on the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            return _queryManager.Query(sparqlQuery);
        }

        /// <summary>
        /// Executes a SPARQL Query on the underlying Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            _queryManager.Query(rdfHandler, resultsHandler, sparqlQuery);
        }

        /// <summary>
        /// Lists the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            if (base.ListGraphsSupported)
            {
                // Use the base classes ListGraphs method if it provides one
                return base.ListGraphs();
            }
            else
            {
                try
                {
                    Object results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                    if (results is SparqlResultSet)
                    {
                        List<Uri> graphs = new List<Uri>();
                        foreach (SparqlResult r in ((SparqlResultSet)results))
                        {
                            if (r.HasValue("g"))
                            {
                                INode temp = r["g"];
                                if (temp.NodeType == NodeType.Uri)
                                {
                                    graphs.Add(((IUriNode)temp).Uri);
                                }
                            }
                        }
                        return graphs;
                    }
                    else
                    {
                        return Enumerable.Empty<Uri>();
                    }
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
                }
            }
        }

        /// <summary>
        /// Returns that listing Graphs is supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }
    }
}