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
using System.Text;
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
    /// Useful if you want to test out some code using temporary in-memory data before you run the code against a real store or if you are using some code that requires an <see cref="IGenericIOManager">IGenericIOManager</see> interface but you need the results of that code to be available directly in-memory.
    /// </para>
    /// </remarks>
    public class InMemoryManager 
        : IUpdateableGenericIOManager, IConfigurationSerializable
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
            : this(new InMemoryDataset()) { }

        /// <summary>
        /// Creates a new In-Memory Manager which is a wrapper around an in-memory store
        /// </summary>
        /// <param name="store">Triple Store</param>
        public InMemoryManager(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new In-Memory Manager which is a wrapper around a SPARQL Dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public InMemoryManager(ISparqlDataset dataset)
        {
            this._dataset = dataset;
        }

        #region IGenericIOManager Members

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            IGraph g = null;
            if (this._dataset.HasGraph(graphUri))
            {
                g = this._dataset[graphUri];
            }
            handler.Apply(g);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI to load</param>
        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(handler, (Uri)null);
            }
            else
            {
                this.LoadGraph(handler, new Uri(graphUri));
            }
        }

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph</param>
        public void SaveGraph(IGraph g)
        {
            if (this._dataset.HasGraph(g.BaseUri))
            {
                this._dataset.RemoveGraph(g.BaseUri);
            }
            this._dataset.AddGraph(g);
            this._dataset.Flush();
        }

        /// <summary>
        /// Gets the IO Behaviour for In-Memory stores
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (!this._dataset.HasGraph(graphUri))
            {
                Graph temp = new Graph();
                temp.BaseUri = graphUri;
                this._dataset.AddGraph(temp);
            }

            if ((additions != null && additions.Any()) || (removals != null && removals.Any()))
            {
                IGraph g = this._dataset.GetModifiableGraph(graphUri);
                if (additions != null && additions.Any()) g.Assert(additions);
                if (removals != null && removals.Any()) g.Retract(removals);
            }

            this._dataset.Flush();
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null)
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else if (graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Returns that Triple level updates are supported
        /// </summary>
        public bool UpdateSupported
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
        public void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                IGraph g = this._dataset.GetModifiableGraph(graphUri);
                g.Clear();
                g.Dispose();
            }
            else
            {
                this._dataset.RemoveGraph(graphUri);
            }
            this._dataset.Flush();
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(string graphUri)
        {
            if (graphUri == null)
            {
                this.DeleteGraph((Uri)null);
            }
            else if (graphUri.Equals(String.Empty))
            {
                this.DeleteGraph((Uri)null);
            }
            else
            {
                this.DeleteGraph(new Uri(graphUri));
            }
        }

        /// <summary>
        /// Returns that Graph Deletion is supported
        /// </summary>
        public bool DeleteSupported
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
        public IEnumerable<Uri> ListGraphs()
        {
            return this._dataset.GraphUris;
        }

        /// <summary>
        /// Returns that listing graphs is supported
        /// </summary>
        public bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Store is ready
        /// </summary>
        public bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Returns that the Store is not read-only
        /// </summary>
        public bool IsReadOnly
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
            if (this._queryParser == null) this._queryParser = new SparqlQueryParser();
            SparqlQuery q = this._queryParser.ParseFromString(sparqlQuery);

            if (this._queryProcessor == null) this._queryProcessor = new LeviathanQueryProcessor(this._dataset);
            return this._queryProcessor.ProcessQuery(q);
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
            if (this._queryParser == null) this._queryParser = new SparqlQueryParser();
            SparqlQuery q = this._queryParser.ParseFromString(sparqlQuery);

            if (this._queryProcessor == null) this._queryProcessor = new LeviathanQueryProcessor(this._dataset);
            this._queryProcessor.ProcessQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Applies SPARQL Updates to the Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            if (this._updateParser == null) this._updateParser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(sparqlUpdate);

            if (this._updateProcessor == null) this._updateProcessor = new LeviathanUpdateProcessor(this._dataset);
            this._updateProcessor.ProcessCommandSet(cmds);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Manager
        /// </summary>
        public void Dispose()
        {
            this._dataset.Flush();
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
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);

            context.Graph.Assert(manager, rdfType, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager));
            context.Graph.Assert(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().ToString()));
            context.Graph.Assert(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString()));
        }

        #endregion
    }
}

#endif