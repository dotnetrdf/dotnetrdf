using System;
using System.Collections.Generic;
using VDS.RDF.Configuration;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Abstract implementation of a connections graph
    /// </summary>
    public abstract class BaseConnectionsGraph
        : IConnectionsGraph
    {
        protected readonly List<Connection> _connections = new List<Connection>();

        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        protected BaseConnectionsGraph(IGraph g, String file)
        {
            if (ReferenceEquals(g, null)) throw new ArgumentNullException("g");
            if (ReferenceEquals(file, null)) throw new ArgumentNullException("file");

            this.File = file;
            this.Graph = g;

            // Load in connections
            this.Load();
        }

        /// <summary>
        /// Gets the file on disk where this graph is saved
        /// </summary>
        public String File { get; private set; }

        /// <summary>
        /// Gets the underlying graph
        /// </summary>
        public IGraph Graph { get; private set; }

        /// <summary>
        /// Gets the managed connections
        /// </summary>
        public IEnumerable<Connection> Connections { get { return this._connections; } }

        /// <summary>
        /// Adds a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        public virtual void Add(Connection connection)
        {
            this._connections.Add(connection);
            this.Save();
        }

        /// <summary>
        /// Removes a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        public virtual void Remove(Connection connection)
        {
            this._connections.Remove(connection);
            this.Save();
        }

        /// <summary>
        /// Clears all connections
        /// </summary>
        public virtual void Clear()
        {
            this._connections.Clear();
            this.Save();
        }

        /// <summary>
        /// Loads the connections from the underlying graph
        /// </summary>
        protected virtual void Load()
        {
            this._connections.Clear();
            if (this.Graph.Triples.Count == 0) return;

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));

            query.CommandText = "SELECT * WHERE { ?connection a @type . }";

            Graph g = new Graph();
            query.SetParameter("type", g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));

            SparqlResultSet results = this.Graph.ExecuteQuery(query) as SparqlResultSet;
            if (results == null) return;
            foreach (SparqlResult r in results)
            {
                INode connectionNode = r["connection"];
                if (connectionNode.NodeType != NodeType.Uri) continue;
                Uri connectionUri = ((IUriNode) connectionNode).Uri;

                Connection connection = new Connection(this.Graph, connectionUri);
                this._connections.Add(connection);
            }
        }

        /// <summary>
        /// Saves the connections to the underlying graph and file on disk
        /// </summary>
        protected virtual void Save()
        {
            this.Graph.Clear();
            foreach (Connection connection in this._connections)
            {
                connection.SaveConfiguration(this.Graph);
            }
            this.Graph.SaveToFile(this.File);
        }
    }
}