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
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    /// <summary>
    /// A connection management list view control
    /// </summary>
    public partial class ConnectionManagementListView
        : UserControl
    {
        private readonly IConnectionsGraph _connections;

        public ConnectionManagementListView(IConnectionsGraph connections)
        {
            if (connections == null) throw new ArgumentNullException("connections");
            this._connections = connections;
            InitializeComponent();
            this.BindData();

            // Subscribe to events on the connection graph
            this._connections.CollectionChanged += this.HandleCollectionChanged;
        }

        /// <summary>
        /// Handles collection changed events on the connection graph updating the list view automatically as necessary
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Add new item
                    this.lvwConnections.BeginUpdate();
                    foreach (Connection connection in e.NewItems.OfType<Connection>())
                    {
                        ListViewItem item = this.BindItem(connection);
                        this.lvwConnections.Items.Add(item);
                    }
                    this.lvwConnections.EndUpdate();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Remove all matched items
                    this.lvwConnections.BeginUpdate();
                    foreach (Connection connection in e.OldItems.OfType<Connection>())
                    {
                        for (int i = 0; i < this.lvwConnections.Items.Count; i++)
                        {
                            if (!ReferenceEquals(connection, this.lvwConnections.Items[i].Tag)) continue;
                            this.lvwConnections.Items.RemoveAt(i);
                            i--;
                        }
                    }
                    this.lvwConnections.EndUpdate();
                    break;
                default:
                    // Rebind the whole list
                    this.BindData();
                    break;
            }
        }

        /// <summary>
        /// Rebinds the entire list view
        /// </summary>
        private void BindData()
        {
            this.lvwConnections.BeginUpdate();
            this.lvwConnections.Items.Clear();

            foreach (Connection connection in this._connections.Connections)
            {
                ListViewItem item = this.BindItem(connection);
                this.lvwConnections.Items.Add(item);
            }

            this.lvwConnections.EndUpdate();
        }

        /// <summary>
        /// Binds values for a connection to a new item but does not add it to the list view itself
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <returns>List View item</returns>
        private ListViewItem BindItem(Connection connection)
        {
            ListViewItem item = new ListViewItem(connection.Name);
            item.Tag = connection;
            item.SubItems.Add(connection.Definition.StoreName);
            item.SubItems.Add(connection.Created.ToString());
            item.SubItems.Add(connection.LastModified.ToString());
            if (connection.LastOpened.HasValue)
            {
                item.SubItems.Add(connection.LastOpened.Value.ToString());
            }
            return item;
        }

        /// <summary>
        /// Gets/Sets whether multiple connections may be selected
        /// </summary>
        public bool MultiSelect
        {
            get { return this.lvwConnections.MultiSelect; }
            set { this.lvwConnections.MultiSelect = value; }
        }

        /// <summary>
        /// Gets/Sets the view mode
        /// </summary>
        public View View
        {
            get { return this.lvwConnections.View; }
            set { this.lvwConnections.View = value; }
        }
    }
}
