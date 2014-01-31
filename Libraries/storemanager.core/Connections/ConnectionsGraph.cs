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
using System.ComponentModel;
using System.Linq;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Manages a set of connections which are serialized to an underlying graph
    /// </summary>
    public class ConnectionsGraph
        : BaseConnectionsGraph
    {
        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        public ConnectionsGraph(IGraph g, String file)
            : base(g, file)
        {
        }
    }

    /// <summary>
    /// Manages a set of connections where a maximum number of connections are managed, if the number if exceeded then the least recent connection(s) are eliminated
    /// </summary>
    public class RecentConnectionsesGraph
        : ConnectionsGraph
    {
        private int _maxConnections;

        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        /// <param name="maxConnections">Maximum recent connections to manage</param>
        public RecentConnectionsesGraph(IGraph g, String file, int maxConnections)
            : base(g, file)
        {
            if (maxConnections < 1) throw new ArgumentException("Number of recent connections must be at least 1", "maxConnections");
            this.MaxConnections = maxConnections;
        }

        /// <summary>
        /// Gets/Sets the maximum recent connections to remember
        /// </summary>
        public int MaxConnections
        {
            get { return this._maxConnections; }
            set
            {
                if (value < 1) throw new ArgumentException("Number of recent connections must be at least 1");
                this._maxConnections = value;
                this.Save();
            }
        }

        /// <summary>
        /// Loads the connections from the underlying graph
        /// </summary>
        protected override void Load()
        {
            base.Load();
            // Doing the save will trim any excess connections
            this.Save();
        }

        /// <summary>
        /// Saves the connections to the underlying graph and file on disk
        /// </summary>
        protected override void Save()
        {
            if (this.MaxConnections > 0 && this._connections.Count > this.MaxConnections)
            {
                while (this._connections.Count > this.MaxConnections)
                {
                    Connection leastRecent = this._connections.OrderBy(c => c.LastOpened).ThenBy(c => c.LastModified).First();
                    this._connections.Remove(leastRecent);
                    this.RaiseRemoved(leastRecent);
                }
            }
            base.Save();
        }
    }

    /// <summary>
    /// Manages a set of connections where only active connections i.e. those that are open will be serialized to the underlying graph
    /// </summary>
    public class ActiveConnectionsGraph
        : ConnectionsGraph
    {
        private bool _firstRun = true;

        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        public ActiveConnectionsGraph(IGraph g, string file)
            : base(g, file)
        {
            IsClosed = false;
        }

        /// <summary>
        /// Handles property changed events on connections
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.IsClosed) return;
            if (e.PropertyName.Equals("IsOpen"))
            {
                // If the IsOpen property changed then a connection may now be in the closed state
                // Calling Save() causes closed connections to be culled
                this.Save();
            }
            // Even though we will have raised any appropriate Remove notifications we should still bubble up the Changed notification regardless
            this.RaiseChanged();
        }

        /// <summary>
        /// Loads the connections from the underlying graph
        /// </summary>
        protected override void Load()
        {
            base.Load();
            // Doing the save will trim any excess connections
            this.Save();
        }

        /// <summary>
        /// Saves the connections to the underlying graph and file on disk
        /// </summary>
        protected override void Save()
        {
            if (!this._firstRun && !this.IsClosed)
            {
                List<Connection> inactive = this._connections.Where(c => !c.IsOpen).ToList();
                this._connections.RemoveAll(c => !c.IsOpen);
                inactive.ForEach(c => this.RaiseRemoved(c));
            }
            base.Save();
            this._firstRun = false;
        }

        /// <summary>
        /// Gets that clear is required on save
        /// </summary>
        protected override bool RequiresClearOnSave
        {
            get { return true; }
            set { return; }
        }

        /// <summary>
        /// Gets whether the active connections graph is closed
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Closes the active connections graph
        /// </summary>
        public void Close()
        {
            this.Save();
            this.IsClosed = true;
        }
    }
}