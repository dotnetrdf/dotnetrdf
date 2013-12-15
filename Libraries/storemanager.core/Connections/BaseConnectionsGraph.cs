/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
        /// <summary>
        /// List of connections
        /// </summary>
        protected readonly List<Connection> _connections = new List<Connection>();

        /// <summary>
        /// Property changed event handler
        /// </summary>
        protected readonly PropertyChangedEventHandler _handler;

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
            this._handler = this.HandlePropertyChanged;

            // Load in connections
            this.Load();
        }

        /// <summary>
        /// Handles property changed events on connections
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaiseChanged();
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
        public IEnumerable<Connection> Connections
        {
            get { return this._connections; }
        }

        /// <summary>
        /// Gets the number of connections
        /// </summary>
        public int Count
        {
            get { return this._connections.Count; }
        }

        /// <summary>
        /// Adds a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        public virtual void Add(Connection connection)
        {
            if (this._connections.Contains(connection)) return;
            if (!ConnectionInstancePool.Contains(connection))
            {
                ConnectionInstancePool.Add(connection);
            }
            this._connections.Add(connection);
            connection.PropertyChanged += this._handler;
            this.RaiseAdded(connection);
            this.Save();
        }

        /// <summary>
        /// Removes a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        public virtual void Remove(Connection connection)
        {
            if (!this._connections.Contains(connection)) return;
            this._connections.Remove(connection);
            connection.PropertyChanged -= this._handler;
            this.RaiseRemoved(connection);
            this.Save();
        }

        /// <summary>
        /// Clears all connections
        /// </summary>
        public virtual void Clear()
        {
            this._connections.ForEach(c => c.PropertyChanged -= this._handler);
            this._connections.Clear();
            this.RaiseCleared();
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

                Connection connection;
                if (!ConnectionInstancePool.TryGetInstance(connectionUri, out connection))
                {
                    connection = new Connection(this.Graph, connectionUri);
                    ConnectionInstancePool.Add(connection);
                }
                this._connections.Add(connection);
            }
        }

        /// <summary>
        /// Saves the connections to the underlying graph and file on disk
        /// </summary>
        protected virtual void Save()
        {
            // Only clear the graph if there are no connections
            if (this._connections.Count == 0) this.Graph.Clear();
            foreach (Connection connection in this._connections)
            {
                connection.SaveConfiguration(this.Graph);
            }
            this.Graph.SaveToFile(this.File);
        }

        /// <summary>
        /// Raises the appropriate collection changed event to indicate a connection was added
        /// </summary>
        /// <param name="connection">Connection</param>
        protected void RaiseAdded(Connection connection)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null) d(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, connection));
        }

        /// <summary>
        /// Raises the appropriate collection changed event to indicate a connection was removed
        /// </summary>
        /// <param name="connection">Connection</param>
        protected void RaiseRemoved(Connection connection)
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null) d(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, connection));
        }

        /// <summary>
        /// Raises the appropriate collection changed event
        /// </summary>
        protected void RaiseChanged()
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null) d(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raises the appropriate collection changed event to indicate the collection was cleared
        /// </summary>
        protected void RaiseCleared()
        {
            NotifyCollectionChangedEventHandler d = this.CollectionChanged;
            if (d != null) d(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Event which is raised when the set of collections changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}