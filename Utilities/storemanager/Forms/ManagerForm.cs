/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.GUI;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Dialogues;
using VDS.RDF.Utilities.StoreManager.Properties;

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    /// <summary>
    /// A form which provides an interface for managing connections to multiple stores
    /// </summary>
    public partial class ManagerForm
        : CrossThreadForm
    {
        /// <summary>
        /// Creates a new form
        /// </summary>
        public ManagerForm()
        {
            InitializeComponent();
            this.Closing += OnClosing;
            Constants.WindowIcon = this.Icon;

            //Ensure we upgrade settings if user has come from an older version of the application
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            //Enable UTF-8 BOM Output if relevant
            Options.UseBomForUtf8 = Settings.Default.UseUtf8Bom;

            //Ensure Configuration Loader has known required Object Factorires registered
            ConfigurationLoader.AddObjectFactory(new VirtuosoObjectFactory());
            ConfigurationLoader.AddObjectFactory(new FullTextObjectFactory());

            //Prepare Connection Definitions so users don't get a huge lag the first time they use these
            ConnectionDefinitionManager.GetDefinitions().Count();

            //Check whether we have a Recent and Favourites Connections Graph
            this.LoadConnections();
        }

        #region Connection Management

        /// <summary>
        /// Loads in favourite, recent and active connections
        /// </summary>
        private void LoadConnections()
        {
            try
            {
                String appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                String sepChar = new String(new char[] {Path.DirectorySeparatorChar});
                if (!appDataDir.EndsWith(sepChar)) appDataDir += sepChar;
                appDataDir = Path.Combine(appDataDir, "dotNetRDF" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                appDataDir = Path.Combine(appDataDir, "Store Manager" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                String recentConnectionsFile = Path.Combine(appDataDir, "recent.ttl");
                String faveConnectionsFile = Path.Combine(appDataDir, "favourite.ttl");
                String activeConnectionsFile = Path.Combine(appDataDir, "active.ttl");

                // Load Favourite Connections
                IGraph faves = new Graph();
                if (File.Exists(faveConnectionsFile)) faves.LoadFromFile(faveConnectionsFile);
                this.FavouriteConnections = new ConnectionsGraph(faves, faveConnectionsFile);
                this.FillConnectionsMenu(this.mnuFavouriteConnections, this.FavouriteConnections, 0);

                // Subscribe to collection changed events
                this.FavouriteConnections.CollectionChanged += FavouriteConnectionsOnCollectionChanged;

                // Load Recent Connections
                IGraph recent = new Graph();
                if (File.Exists(recentConnectionsFile)) recent.LoadFromFile(recentConnectionsFile);
                this.RecentConnections = new RecentConnectionsesGraph(recent, recentConnectionsFile, Settings.Default.MaxRecentConnections);
                this.FillConnectionsMenu(this.mnuRecentConnections, this.RecentConnections, Settings.Default.MaxRecentConnections);

                // Subscribe to collection changed events
                this.RecentConnections.CollectionChanged += RecentConnectionsOnCollectionChanged;

                // Load Active Connections
                IGraph active = new Graph();
                if (File.Exists(activeConnectionsFile)) active.LoadFromFile(activeConnectionsFile);
                this.ActiveConnections = new ActiveConnectionsGraph(active, activeConnectionsFile);
            }
            catch (Exception ex)
            {
                Program.HandleInternalError(Resources.LoadConnections_Error, ex);
            }
        }

        /// <summary>
        /// Gets the favourite connections (may be null)
        /// </summary>
        public IConnectionsGraph FavouriteConnections { get; private set; }

        /// <summary>
        /// Gets the recent connections (may be null)
        /// </summary>
        public IConnectionsGraph RecentConnections { get; private set; }

        /// <summary>
        /// Gets the active connections (may be null)
        /// </summary>
        public ActiveConnectionsGraph ActiveConnections { get; private set; }

        /// <summary>
        /// Gets the first form associated with a given connection (if any)
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <returns>Form if found, null otherwise</returns>
        public StoreManagerForm GetStoreManagerForm(Connection connection)
        {
            return this.MdiChildren.OfType<StoreManagerForm>().FirstOrDefault(form => ReferenceEquals(form.Connection, connection));
        }

        private void FavouriteConnectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.HandleConnectionsGraphChanged(notifyCollectionChangedEventArgs, this.FavouriteConnections, this.mnuFavouriteConnections, 0);
        }

        private void RecentConnectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.HandleConnectionsGraphChanged(notifyCollectionChangedEventArgs, this.RecentConnections, this.mnuRecentConnections, Settings.Default.MaxRecentConnections);
        }

        private void HandleConnectionsGraphChanged(NotifyCollectionChangedEventArgs args, IConnectionsGraph connections, ToolStripMenuItem item, int maxItems)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Connection connection in args.NewItems.OfType<Connection>())
                    {
                        this.AddConnectionToMenu(connection, item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Connection connection in args.OldItems.OfType<Connection>())
                    {
                        RemoveConnectionFromMenu(connection, item);
                    }
                    break;
                default:
                    this.FillConnectionsMenu(item, connections, maxItems);
                    break;
            }
        }

        public void AddRecentConnection(Connection connection)
        {
            try
            {
                if (this.RecentConnections != null) this.RecentConnections.Add(connection);
                if (this.ActiveConnections != null) this.ActiveConnections.Add(connection);
            }
            catch (Exception ex)
            {
                Program.HandleInternalError(Resources.RecentConnections_Update_Error, ex);
            }
        }

        public void AddFavouriteConnection(Connection connection)
        {
            if (this.FavouriteConnections == null)
            {
                Program.HandleInternalError(Resources.FavouriteConnections_NoFile);
                return;
            }
            try
            {
                this.FavouriteConnections.Add(connection);
            }
            catch (Exception ex)
            {
                Program.HandleInternalError(Resources.FavouriteConnections_Update_Error, ex);
            }
        }

        private void AddConnectionToMenu(Connection connection, ToolStripDropDownItem parentItem)
        {
            if (connection == null) return;
            ToolStripMenuItem item = new ToolStripMenuItem {Text = connection.Name, Tag = connection};
            item.Click += QuickConnectClick;

            ToolStripMenuItem edit = new ToolStripMenuItem();
            edit.Text = Resources.EditConnection;
            edit.Tag = item.Tag;
            edit.Click += QuickEditClick;
            item.DropDownItems.Add(edit);

            parentItem.DropDownItems.Add(item);
        }

        private static void RemoveConnectionFromMenu(Connection connection, ToolStripDropDownItem parentItem)
        {
            if (connection == null) return;
            for (int i = 0; i < parentItem.DropDownItems.Count; i++)
            {
                if (!ReferenceEquals(parentItem.DropDownItems[i].Tag, connection)) continue;
                parentItem.DropDownItems.RemoveAt(i);
                i--;
            }
        }

        private void ClearRecentConnections()
        {
            ClearConnections(this.mnuRecentConnections, this.RecentConnections);
        }

        private void ClearFavouriteConnections()
        {
            if (MessageBox.Show(Resources.FavouriteConnections_ConfirmClear_Text, Resources.FavouriteConnections_ConfirmClear_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ClearConnections(this.mnuFavouriteConnections, this.FavouriteConnections);
            }
        }

        private static void ClearConnections(ToolStripMenuItem menu, IConnectionsGraph connections)
        {
            if (connections == null) return;
            try
            {
                connections.Clear();
            }
            catch (Exception ex)
            {
                Program.HandleInternalError(Resources.ClearConnections_Error, ex);
            }

            if (menu == null) return;
            while (menu.DropDownItems.Count > 2)
            {
                menu.DropDownItems.RemoveAt(2);
            }
        }

        private void FillConnectionsMenu(ToolStripDropDownItem menu, IConnectionsGraph config, int maxItems)
        {
            // Clear existing items (except the items that are the clear options)
            while (menu.DropDownItems.Count > 2)
            {
                menu.DropDownItems.RemoveAt(2);
            }
            if (config == null || config.Count == 0) return;

            int count = 0;
            foreach (Connection connection in config.Connections)
            {
                ToolStripMenuItem item = new ToolStripMenuItem {Text = connection.Name, Tag = connection};
                item.Click += QuickConnectClick;

                ToolStripMenuItem edit = new ToolStripMenuItem {Text = Resources.EditConnection, Tag = item.Tag};
                edit.Click += QuickEditClick;
                item.DropDownItems.Add(edit);

                menu.DropDownItems.Add(item);

                count++;
                if (maxItems > 0 && count >= maxItems) break;
            }
        }

        private void QuickConnectClick(object sender, EventArgs e)
        {
            if (sender == null) return;
            Object tag = null;
            if (sender is Control)
            {
                tag = ((Control) sender).Tag;
            }
            else if (sender is ToolStripItem)
            {
                tag = ((ToolStripItem) sender).Tag;
            }
            else if (sender is Menu)
            {
                tag = ((Menu) sender).Tag;
            }
            if (tag == null) return;
            if (!(tag is Connection)) return;
            Connection connection = (Connection) tag;
            try
            {
                connection.Open();
                this.ShowStoreManagerForm(connection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.QuickConnect_Error_Text, ex.Message), Resources.QuickConnect_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void QuickEditClick(object sender, EventArgs e)
        {
            if (sender == null) return;
            Object tag = null;
            if (sender is Control)
            {
                tag = ((Control) sender).Tag;
            }
            else if (sender is ToolStripItem)
            {
                tag = ((ToolStripItem) sender).Tag;
            }
            else if (sender is Menu)
            {
                tag = ((Menu) sender).Tag;
            }

            if (tag != null)
            {
                if (tag is Connection)
                {
                    Connection connection = (Connection) tag;
                    if (connection.IsOpen)
                    {
                        MessageBox.Show(Resources.EditConnection_Forbidden_Text, Resources.EditConnection_Forbidden_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    try
                    {
                        EditConnectionForm editConn = new EditConnectionForm(connection, false);
                        if (editConn.ShowDialog() == DialogResult.OK)
                        {
                            connection = editConn.Connection;
                            this.ShowStoreManagerForm(connection);

                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Resources.QuickEdit_Error_Text, ex.Message), Resources.QuickEdit_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool PromptRestoreConnections()
        {
            if (this.ActiveConnections == null) return false;
            if (Settings.Default.PromptRestoreActiveConnections)
            {
                if (!this.ActiveConnections.IsClosed && this.ActiveConnections.Count > 0 && this.ActiveConnections.Connections.Any(c => c.IsOpen))
                {
                    try
                    {
                        RestoreConnectionsDialogue restoreConnections = new RestoreConnectionsDialogue();
                        DialogResult result = restoreConnections.ShowDialog();
                        switch (result)
                        {
                            case DialogResult.Cancel:
                                return true;
                            case DialogResult.Yes:
                                Settings.Default.RestoreActiveConnections = true;
                                this.ActiveConnections.Close();
                                break;
                            default:
                                Settings.Default.RestoreActiveConnections = false;
                                this.ActiveConnections.Clear();
                                break;
                        }
                        Settings.Default.Save();
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Program.HandleInternalError(Resources.ActiveConnections_Update_Error, ex);
                    }
                }
            }
            else if (Settings.Default.AlwaysRestoreActiveConnections)
            {
                if (!this.ActiveConnections.IsClosed && this.ActiveConnections.Count > 0 && this.ActiveConnections.Connections.Any(c => c.IsOpen))
                {
                    Settings.Default.RestoreActiveConnections = true;
                    this.ActiveConnections.Close();
                }
            }
            else
            {
                this.ActiveConnections.Clear();
            }
            return false;
        }

        private void RestoreConnections()
        {
            if (!Settings.Default.RestoreActiveConnections) return;
            foreach (Connection connection in this.ActiveConnections.Connections.ToList())
            {
                try
                {
                    connection.Open();
                    this.ShowStoreManagerForm(connection);
                }
                catch (Exception ex)
                {
                    Program.HandleInternalError(string.Format(Resources.RestoreConnection_Error, connection.Name), ex);
                }
            }
            this.LayoutMdi(MdiLayout.Cascade);
        }

        #endregion

        #region Event Handlers

        private void fclsManager_Load(object sender, EventArgs e)
        {
            this.RestoreConnections();

            if (!Settings.Default.ShowStartPage) return;
            StartPage start = new StartPage(this.RecentConnections, this.FavouriteConnections);
            start.ShowDialog();
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            this.PromptRestoreConnections();
        }

        #endregion

        #region Menu Event Handlers

        private void mnuStrip_MenuActivate(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                Form activeChild = this.ActiveMdiChild;
                this.ActivateMdiChild(null);
                this.ActivateMdiChild(activeChild);

                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    this.mnuSaveConnection.Enabled = true;
                    this.mnuAddFavourite.Enabled = true;
                    this.mnuNewFromExisting.Enabled = true;
                }
                else
                {
                    this.mnuSaveConnection.Enabled = false;
                    this.mnuAddFavourite.Enabled = false;
                    this.mnuNewFromExisting.Enabled = false;
                }
            }
            else
            {
                this.mnuSaveConnection.Enabled = false;
                this.mnuAddFavourite.Enabled = false;
                this.mnuNewFromExisting.Enabled = false;
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            // Prompt for restoring connections
            this.PromptRestoreConnections();

            // Close children
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }
            this.Close();
        }

        private void mnuCascade_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void mnuTileVertical_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void mnuTileHorizontal_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void mnuArrangeIcons_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void mnuCloseAll_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void mnuSaveConnection_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    try
                    {
                        Connection connection = ((StoreManagerForm) this.ActiveMdiChild).Connection;
                        this.sfdConnection.Filter = MimeTypesHelper.GetFilenameFilter(true, false, false, false, false, false);
                        if (this.sfdConnection.ShowDialog() == DialogResult.OK)
                        {
                            //Append to existing configuration file or overwrite?
                            IGraph cs = new Graph();
                            if (File.Exists(this.sfdConnection.FileName))
                            {
                                DialogResult result = MessageBox.Show(Resources.SaveConnection_Append_Text, Resources.SaveConnection_Append_Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                switch (result)
                                {
                                    case DialogResult.Yes:
                                        // Load in existing connections
                                        cs.LoadFromFile(this.sfdConnection.FileName);
                                        break;
                                    case DialogResult.No:
                                        File.Delete(this.sfdConnection.FileName);
                                        break;
                                    default:
                                        return;
                                }
                            }

                            // Open the connections file and add to it which automatically causes it to be saved
                            IConnectionsGraph connections = new ConnectionsGraph(cs, this.sfdConnection.FileName);
                            connections.Add(connection);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.HandleInternalError(Resources.SaveConnection_Error, ex);
                    }
                }
                else
                {
                    this.mnuSaveConnection.Enabled = false;
                }
            }
            else
            {
                this.mnuSaveConnection.Enabled = false;
            }
        }

        private void mnuOpenConnection_Click(object sender, EventArgs e)
        {
            this.ofdConnection.Filter = MimeTypesHelper.GetFilenameFilter(true, false, false, false, false, false);
            if (this.ofdConnection.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    IGraph cs = new Graph();
                    cs.LoadFromFile(this.ofdConnection.FileName);
                    IConnectionsGraph connections = new ConnectionsGraph(cs, this.ofdConnection.FileName);

                    OpenConnectionForm openConnections = new OpenConnectionForm(connections);
                    openConnections.MdiParent = this;
                    if (openConnections.ShowDialog() == DialogResult.OK)
                    {
                        Connection connection = openConnections.Connection;
                        ShowStoreManagerForm(connection);
                    }
                }
                catch (RdfParseException)
                {
                    MessageBox.Show(Resources.OpenConnection_InvalidFile_Text, Resources.OpenConnection_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Resources.OpenConnection_Error_Text, ex.Message), Resources.OpenConnection_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuClearRecentConnections_Click(object sender, EventArgs e)
        {
            this.ClearRecentConnections();
        }

        private void mnuClearFavouriteConnections_Click(object sender, EventArgs e)
        {
            this.ClearFavouriteConnections();
        }

        private void mnuAddFavourite_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == null) return;
            if (!(this.ActiveMdiChild is StoreManagerForm)) return;
            Connection connection = ((StoreManagerForm) this.ActiveMdiChild).Connection;
            this.AddFavouriteConnection(connection);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.ShowDialog();
        }

        private void mnuNewConnection_Click(object sender, EventArgs e)
        {
            NewConnectionForm newConn = new NewConnectionForm();
            newConn.StartPosition = FormStartPosition.CenterParent;
            if (newConn.ShowDialog() != DialogResult.OK) return;
            Connection connection = newConn.Connection;
            ShowStoreManagerForm(connection);
        }

        private void mnuNewFromExisting_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    Connection connection = ((StoreManagerForm) this.ActiveMdiChild).Connection;
                    EditConnectionForm editConn = new EditConnectionForm(connection, true);
                    if (editConn.ShowDialog() == DialogResult.OK)
                    {
                        ShowStoreManagerForm(editConn.Connection);
                    }
                    return;
                }
            }
            MessageBox.Show(Resources.NewFromExisting_Error_Text, Resources.NewFromExisting_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowStoreManagerForm(Connection connection)
        {
            StoreManagerForm storeManagerForm = new StoreManagerForm(connection);
            CrossThreadSetMdiParent(storeManagerForm, this);
            CrossThreadShow(storeManagerForm);

            ToolStripButton quickJumpButton = new ToolStripButton(connection.Name);
            connection.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                {
                    if (args.PropertyName.Equals("Name")) CrossThreadSetText(quickJumpButton, connection.Name);
                    if (args.PropertyName.Equals("IsOpen") && !connection.IsOpen)
                    {
                        this.quickJumpBar.Items.Remove(quickJumpButton);
                        if (this.quickJumpBar.Items.Count == 1) this.quickJumpBar.Visible = false;
                    }
                };
            quickJumpButton.Click += delegate(object sender, EventArgs args)
                {
                    StoreManagerForm form = this.GetStoreManagerForm(connection);
                    if (form == null) return;
                    form.Show();
                    form.Focus();
                };
            quickJumpBar.Items.Add(quickJumpButton);
            this.quickJumpBar.Visible = true;

            this.AddRecentConnection(connection);
        }

        private void mnuStartPage_Click(object sender, EventArgs e)
        {
            StartPage start = new StartPage(this.RecentConnections, this.FavouriteConnections);
            start.Owner = this;
            start.ShowDialog();
        }

        private void mnuManageConnections_Click(object sender, EventArgs e)
        {
            ManageConnectionsForm manageConnectionsForm = new ManageConnectionsForm();
            manageConnectionsForm.ActiveConnections = this.ActiveConnections;
            manageConnectionsForm.RecentConnections = this.RecentConnections;
            manageConnectionsForm.FavouriteConnections = this.FavouriteConnections;
            manageConnectionsForm.MdiParent = this;
            manageConnectionsForm.Show();
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            // Show options dialogue
            OptionsDialogue dialogue = new OptionsDialogue();
            dialogue.MdiParent = this;
            dialogue.Show();

            // Want to apply updated editor options when the dialogue is closed
            dialogue.Closed += (o, args) => this.UpdateEditors();
        }

        private void UpdateEditors()
        {
            foreach (StoreManagerForm storeManagerForm in this.MdiChildren.OfType<StoreManagerForm>())
            {
                storeManagerForm.ApplyEditorOptions();
            }
        }

        #endregion
    }
}