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
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Forms;
using VDS.RDF.Utilities.StoreManager.Properties;

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    /// <summary>
    /// A connection management list view control
    /// </summary>
    public partial class ConnectionManagementListView
        : UserControl
    {
        private IConnectionsGraph _connections;
        private readonly NotifyCollectionChangedEventHandler _handler;

        /// <summary>
        /// Creates a new empty list view
        /// </summary>
        public ConnectionManagementListView()
        {
            InitializeComponent();
            this._handler = this.HandleCollectionChanged;

            // Subscribe to context menu events so we can configure the available options
            this.mnuContext.Opening += MnuContextOnOpening;
        }

        /// <summary>
        /// Creates a new list view with the given data source
        /// </summary>
        /// <param name="connections">Data source</param>
        public ConnectionManagementListView(IConnectionsGraph connections)
            : this()
        {
            if (connections == null) throw new ArgumentNullException("connections");
            this._connections = connections;
            this.BindData();

            // Subscribe to events on connections graph
            this._connections.CollectionChanged += this._handler;
        }

        /// <summary>
        /// Gets the currently selected connections
        /// </summary>
        /// <returns>Connections</returns>
        private List<Connection> GetSelectedConnections()
        {
            return (from ListViewItem item
                        in this.lvwConnections.SelectedItems
                    where item.Tag is Connection
                    select (Connection) item.Tag).ToList();
        }

        private void MnuContextOnOpening(object sender, CancelEventArgs cancelEventArgs)
        {
            if (this.lvwConnections.SelectedItems.Count == 0)
            {
                cancelEventArgs.Cancel = true;
                return;
            }

            // Enable appropriate options based on settings and selected connection states
            List<Connection> selectedConnections = this.GetSelectedConnections();
            this.mnuClose.Enabled = this.AllowClose && selectedConnections.All(c => c.IsOpen);
            this.mnuEdit.Enabled = this.AllowEdit && selectedConnections.All(c => !c.IsOpen);
            this.mnuNewFromExisting.Enabled = this.AllowNewFromExisting;
            this.mnuOpen.Enabled = this.AllowOpen && selectedConnections.All(c => !c.IsOpen);
            this.mnuRemove.Enabled = this.AllowRemove;
            this.mnuRename.Enabled = this.AllowRename;
            this.mnuShow.Enabled = this.AllowShow && selectedConnections.All(c => c.IsOpen);
        }

        /// <summary>
        /// Handles collection changed events on connections graph updating the list view automatically as necessary
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
                        ListViewItem item = BindItem(connection);
                        this.lvwConnections.Items.Add(item);
                    }
                    this.ResizeColumns();
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
                    this.ResizeColumns();
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

            if (!ReferenceEquals(this._connections, null))
            {
                foreach (ListViewItem item in this._connections.Connections.Select(connection => BindItem(connection)))
                {
                    this.lvwConnections.Items.Add(item);
                }
            }
            this.ResizeColumns();

            this.lvwConnections.EndUpdate();
        }

        /// <summary>
        /// Binds values for a connection to a new item but does not add it to the list view itself
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <returns>List View item</returns>
        private static ListViewItem BindItem(Connection connection)
        {
            ListViewItem item = new ListViewItem(connection.Name);
            item.Tag = connection;
            item.SubItems.Add(connection.Definition.StoreName);
            item.SubItems.Add(connection.Created.ToString());
            item.SubItems.Add(connection.LastModified.ToString());
            item.SubItems.Add(connection.LastOpened.HasValue ? connection.LastOpened.Value.ToString() : String.Empty);
            item.SubItems.Add(connection.IsOpen ? Resources.Yes : Resources.No);
            item.SubItems.Add(connection.ActiveUsers.ToString(System.Globalization.CultureInfo.CurrentUICulture));
            return item;
        }

        private void ResizeColumns()
        {
            bool hasItems = this.lvwConnections.Items.Count > 0;
            ColumnHeaderAutoResizeStyle resizeStyle = hasItems ? ColumnHeaderAutoResizeStyle.ColumnContent : ColumnHeaderAutoResizeStyle.HeaderSize;

            this.lvwConnections.AutoResizeColumn(0, resizeStyle);
            this.lvwConnections.AutoResizeColumn(1, resizeStyle);
            this.lvwConnections.AutoResizeColumn(2, resizeStyle);
            this.lvwConnections.AutoResizeColumn(3, resizeStyle);
            this.lvwConnections.AutoResizeColumn(4, ColumnHeaderAutoResizeStyle.HeaderSize);
            this.lvwConnections.AutoResizeColumn(5, ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        /// <summary>
        /// Gets/Sets the data source for the control
        /// </summary>
        public IConnectionsGraph DataSource
        {
            get { return this._connections; }
            set
            {
                if (this._connections != null)
                {
                    if (ReferenceEquals(this._connections, value)) return;
                    this._connections.CollectionChanged -= this._handler;
                    this._connections = value;
                    if (value != null) this._connections.CollectionChanged += this._handler;
                }
                else
                {
                    this._connections = value;
                    if (value != null) this._connections.CollectionChanged += this._handler;
                }
                // Always bind data after the data source changes
                this.BindData();
            }
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

        /// <summary>
        /// Gets/Sets whether connections may be closed from this view
        /// </summary>
        public bool AllowClose { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be opened from this view
        /// </summary>
        public bool AllowOpen { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be shown from this view
        /// </summary>
        public bool AllowShow { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be edited from this view
        /// </summary>
        public bool AllowEdit { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be renamed from this view
        /// </summary>
        public bool AllowRename { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be removed from this view
        /// </summary>
        public bool AllowRemove { get; set; }

        /// <summary>
        /// Gets/Sets whether connections may be copied from this view
        /// </summary>
        public bool AllowNewFromExisting { get; set; }

        /// <summary>
        /// Gets/Sets whether actions that are not themselves associated with a dialogue require confirmations
        /// </summary>
        public bool RequireConfirmation { get; set; }

        private void mnuRemove_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                if (this.RequireConfirmation && MessageBox.Show(string.Format(Resources.ConnectionManagement_ConfirmRemove_Text, connection.Name), Resources.ConnectionManagement_ConfirmRemove_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) continue;
                try
                {
                    this._connections.Remove(connection);
                }
                catch (Exception ex)
                {
                    Program.HandleInternalError(string.Format(Resources.ConnectionManagement_Remove_Error, connection.Name), ex);
                }
            }
        }

        private void mnuClose_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                if (this.RequireConfirmation && MessageBox.Show(string.Format(Resources.ConnectionManagement_ConfirmClose_Text, connection.Name), Resources.ConnectionManagement_ConfirmClose_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) continue;
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                   Program.HandleInternalError(string.Format(Resources.ConnectionManagement_Close_Error, connection.Name), ex);
                }
            }
        }

        private void mnuShow_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                StoreManagerForm form = Program.MainForm.GetStoreManagerForm(connection);
                if (form == null) continue;
                form.Show();
                form.Focus();
            }
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                try
                {
                    connection.Open();
                    Program.MainForm.ShowStoreManagerForm(connection);
                }
                catch (Exception ex)
                {
                    Program.HandleInternalError(string.Format(Resources.ConnectionManagement_Open_Error, connection.Name), ex);
                }
            }
        }

        private void mnuEdit_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                try
                {
                    EditConnectionForm editConnection = new EditConnectionForm(connection, false);
                    editConnection.MdiParent = Program.MainForm;
                    if (editConnection.ShowDialog() != DialogResult.OK) continue;
                    Program.MainForm.ShowStoreManagerForm(editConnection.Connection);
                }
                catch (Exception ex)
                {
                    Program.HandleInternalError(string.Format(Resources.ConnectionManagement_Edit_Error, connection.Name), ex);
                }
            }
        }

        private void mnuNewFromExisting_Click(object sender, EventArgs e)
        {
            List<Connection> selectedConnections = this.GetSelectedConnections();
            foreach (Connection connection in selectedConnections)
            {
                try
                {
                    EditConnectionForm editConnection = new EditConnectionForm(connection, true);
                    editConnection.MdiParent = Program.MainForm;
                    if (editConnection.ShowDialog() != DialogResult.OK) continue;
                    Program.MainForm.ShowStoreManagerForm(editConnection.Connection);
                }
                catch (Exception ex)
                {
                    Program.HandleInternalError(string.Format(Resources.ConnectionManagement_Edit_Error, connection.Name), ex);
                }
            }
        }
    }
}