/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Provides a Read-Only wrapper that can be placed around another <see cref="IGenericIOManager">IGenericIOManager</see> instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance
    /// </para>
    /// </remarks>
    public class ReadOnlyConnector 
        : IGenericIOManager, IConfigurationSerializable
    {
        private IGenericIOManager _manager;

        /// <summary>
        /// Creates a new Read-Only connection which is a read-only wrapper around another store
        /// </summary>
        /// <param name="manager">Manager for the Store you want to wrap as read-only</param>
        public ReadOnlyConnector(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        #region IGenericIOManager Members

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, string graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this._manager.LoadGraph(handler, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the underlying Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            this._manager.LoadGraph(handler, graphUri);
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
                return (this._manager.IOBehaviour | IOBehaviour.IsReadOnly) & (IOBehaviour.HasDefaultGraph | IOBehaviour.HasDefaultGraph | IOBehaviour.IsQuadStore | IOBehaviour.IsTripleStore | IOBehaviour.IsReadOnly);
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
            return this._manager.ListGraphs();
        }

        /// <summary>
        /// Returns whether listing graphs is supported by the underlying store
        /// </summary>
        public virtual bool ListGraphsSupported
        {
            get
            {
                return this._manager.ListGraphsSupported;
            }
        }

        /// <summary>
        /// Returns whether the Store is ready
        /// </summary>
        public bool IsReady
        {
            get 
            {
                return this._manager.IsReady; 
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
            this._manager.Dispose();
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Manager
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Read Only]" + this._manager.ToString();
        }

        /// <summary>
        /// Serializes the Configuration of the Manager
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyGenericManager);

            context.Graph.Assert(manager, rdfType, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager));
            context.Graph.Assert(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().ToString()));
            context.Graph.Assert(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString()));

            if (this._manager is IConfigurationSerializable)
            {
                INode managerObj = context.Graph.CreateBlankNode();
                context.NextSubject = managerObj;
                ((IConfigurationSerializable)this._manager).SerializeConfiguration(context);
                context.Graph.Assert(manager, genericManager, managerObj);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration as the underlying IGenericIOManager is not serializable");
            }
        }
    }

    /// <summary>
    /// Provides a Read-Only wrapper that can be placed around another <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see> instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance
    /// </para>
    /// </remarks>
    public class QueryableReadOnlyConnector
        : ReadOnlyConnector, IQueryableGenericIOManager
    {
        private IQueryableGenericIOManager _queryManager;

        /// <summary>
        /// Creates a new Queryable Read-Only connection which is a read-only wrapper around another store
        /// </summary>
        /// <param name="manager">Manager for the Store you want to wrap as read-only</param>
        public QueryableReadOnlyConnector(IQueryableGenericIOManager manager)
            : base(manager)
        {
            this._queryManager = manager;
        }

        /// <summary>
        /// Executes a SPARQL Query on the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            return this._queryManager.Query(sparqlQuery);
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
            this._queryManager.Query(rdfHandler, resultsHandler, sparqlQuery);
        }

        /// <summary>
        /// Lists the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            if (base.ListGraphsSupported)
            {
                //Use the base classes ListGraphs method if it provides one
                return base.ListGraphs();
            }
            else
            {
                try
                {
                    Object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
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

#endif