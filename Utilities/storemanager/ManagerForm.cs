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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.GUI;
using VDS.RDF.GUI.WinForms;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Configuration;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class ManagerForm : Form
    {
        private readonly IConnectionsGraph _favouriteConnections, _recentConnections;

        public const int MaxRecentConnections = 9;

        public ManagerForm()
        {
            InitializeComponent();
            Constants.WindowIcon = this.Icon;

            //Ensure we upgrade settings if user has come from an older version of the application
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }

            //Enable UTF-8 BOM Output if relevant
            Options.UseBomForUtf8 = false;
            if (Properties.Settings.Default.UseUtf8Bom)
            {
                this.mnuUseUtf8Bom.Checked = true;
                Options.UseBomForUtf8 = true;
            }
            this.mnuShowStartPage.Checked = Properties.Settings.Default.ShowStartPage;

            //Check whether we have a Recent and Favourites Connections Graph
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

                if (File.Exists(recentConnectionsFile))
                {
                    //Load Recent Connections
                    this._recentConnections = new RecentConnectionsesGraph(new Graph(), recentConnectionsFile, MaxRecentConnections);
                    this.FillConnectionsMenu(this.mnuRecentConnections, this._recentConnections, MaxRecentConnections);
                }
                if (File.Exists(faveConnectionsFile))
                {
                    //Load Favourite Connections
                    this._favouriteConnections = new ConnectionsGraph(new Graph(), faveConnectionsFile);
                    this.FillConnectionsMenu(this.mnuFavouriteConnections, this._favouriteConnections, 0);
                }
            }
            catch
            {
                //If errors occur then ignore Recent Connections
            }

            //Ensure Configuration Loader has any required Object Factorires registered
            ConfigurationLoader.AddObjectFactory(new VirtuosoObjectFactory());
            ConfigurationLoader.AddObjectFactory(new FullTextObjectFactory());

            //Prepare Connection Definitions so users don't get a huge lag the first time they use these
            ConnectionDefinitionManager.GetDefinitions().Count();
        }

        private void fclsManager_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ShowStartPage)
            {
                StartPage start = new StartPage(this._recentConnections, this._favouriteConnections);
                start.ShowDialog();
            }
        }

        private void mnuStrip_MenuActivate(object sender, System.EventArgs e)
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
                            if (File.Exists(this.sfdConnection.FileName))
                            {
                                DialogResult result = MessageBox.Show("The selected connection file already exists - would you like to append this connection to that file?  Click Yes to append to this file, No to overwrite and Cancel to abort", "Append Connection?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                switch (result)
                                {
                                    case DialogResult.Yes:
                                        // Nothing to do here, the subsequent creation of the connections graph will cause existing connections to be loaded in so the operation will be an append
                                        break;
                                    case DialogResult.No:
                                        File.Delete(this.sfdConnection.FileName);
                                        break;
                                    default:
                                        return;
                                }
                            }

                            // Open the connections file and add to it which automatically causes it to be saved
                            IConnectionsGraph connections = new ConnectionsGraph(new Graph(), this.sfdConnection.FileName);
                            connections.Add(connection);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Unable to save connection", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    IConnectionsGraph connections = new ConnectionsGraph(new Graph(), this.ofdConnection.FileName);

                    OpenConnectionForm openConnections = new OpenConnectionForm(connections);
                    openConnections.MdiParent = this;
                    if (openConnections.ShowDialog() == DialogResult.OK)
                    {
                        Connection connection = openConnections.Connection;
                        StoreManagerForm genManagerForm = new StoreManagerForm(connection);
                        genManagerForm.MdiParent = this;
                        genManagerForm.Show();

                        //Add to Recent Connections
                        this.AddRecentConnection(connection);
                    }
                }
                catch (RdfParseException)
                {
                    MessageBox.Show("Unable to open a connection from the given file as it was not a valid RDF Graph or was in a format that the library does not understand", "Open Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open the given file due to the following error:\n" + ex.Message, "Open Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FillConnectionsMenu(ToolStripMenuItem menu, IConnectionsGraph config, int maxItems)
        {
            if (config == null || config.Count == 0) return;

            int count = 0;
            foreach (Connection connection in config.Connections)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = connection.Name;
                item.Tag = connection;
                item.Click += new EventHandler(QuickConnectClick);

                ToolStripMenuItem edit = new ToolStripMenuItem();
                edit.Text = "Edit Connection";
                edit.Tag = item.Tag;
                edit.Click += new EventHandler(QuickEditClick);
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
                StoreManagerForm genManager = new StoreManagerForm(connection);
                genManager.MdiParent = this;
                genManager.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open the connection due to the following error: " + ex.Message, "Quick Connect Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    try
                    {
                        EditConnectionForm editConn = new EditConnectionForm(connection.Definition);
                        if (editConn.ShowDialog() == DialogResult.OK)
                        {
                            connection = editConn.Connection;
                            StoreManagerForm storeManager = new StoreManagerForm(connection);
                            storeManager.MdiParent = Program.MainForm;
                            storeManager.Show();

                            //Add to Recent Connections
                            this.AddRecentConnection(connection);

                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to edit the Connection due to an error: " + ex.Message, "Quick Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        public void AddRecentConnection(Connection connection)
        {
            if (this._recentConnections == null) return;
            try
            {
                this._recentConnections.Add(connection);
            }
            catch
            {
                MessageBox.Show("Unable to update recent connections", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AddFavouriteConnection(Connection connection)
        {
            if (this._favouriteConnections == null)
            {
                MessageBox.Show("Unable to update favourite connections", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                this._favouriteConnections.Add(connection);
            }
            catch
            {
                MessageBox.Show("Unable to update favourite connections", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddConnectionToMenu(Connection connection, ToolStripMenuItem parentItem)
        {
            if (connection == null) return;
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = connection.Name;
            item.Tag = connection;
            item.Click += new EventHandler(QuickConnectClick);

            ToolStripMenuItem edit = new ToolStripMenuItem();
            edit.Text = "Edit Connection";
            edit.Tag = item.Tag;
            edit.Click += new EventHandler(QuickEditClick);
            item.DropDownItems.Add(edit);

            parentItem.DropDownItems.Add(item);
        }

        private void AddConnection(IConnectionsGraph connections, Connection connection)
        {
            if (connections == null) return;
            try
            {
                connections.Add(connection);
            }
            catch
            {
                MessageBox.Show("Unable to add a connection to a file", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearRecentConnections()
        {
            this.ClearConnections(this.mnuRecentConnections, this._recentConnections);
        }

        private void ClearFavouriteConnections()
        {
            if (MessageBox.Show("Are you sure you wish to clear your Favourite Connections?", "Confirm Clear Favourite Connections", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.ClearConnections(this.mnuFavouriteConnections, this._favouriteConnections);
            }
        }

        private void ClearConnections(ToolStripMenuItem menu, IConnectionsGraph connections)
        {
            if (connections == null) return;
            try
            {
                connections.Clear();
            }
            catch
            {
                MessageBox.Show("Unable to clear connections", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            while (menu.DropDownItems.Count > 2)
            {
                menu.DropDownItems.RemoveAt(2);
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
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    Connection connection = ((StoreManagerForm) this.ActiveMdiChild).Connection;
                    this.AddFavouriteConnection(connection);
                }
                else
                {
                    MessageBox.Show("Only Generic Store Connections may be added to your Favourites", "Add To Favourite Connections Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void mnuUseUtf8Bom_Click(object sender, EventArgs e)
        {
            Options.UseBomForUtf8 = this.mnuUseUtf8Bom.Checked;
            Properties.Settings.Default.UseUtf8Bom = Options.UseBomForUtf8;
            Properties.Settings.Default.Save();
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
            if (newConn.ShowDialog() == DialogResult.OK)
            {
                Connection connection = newConn.Connection;
                StoreManagerForm storeManager = new StoreManagerForm(connection);
                storeManager.MdiParent = this;
                storeManager.Show();

                //Add to Recent Connections
                this.AddRecentConnection(connection);
            }
        }

        private void mnuNewFromExisting_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    Connection connection = ((StoreManagerForm) this.ActiveMdiChild).Connection;
                    EditConnectionForm editConn = new EditConnectionForm(connection.Definition);
                    if (editConn.ShowDialog() == DialogResult.OK)
                    {
                        StoreManagerForm managerForm = new StoreManagerForm(editConn.Connection);
                        this.AddRecentConnection(editConn.Connection);
                        managerForm.MdiParent = this;
                        managerForm.Show();
                    }
                    return;
                }
            }
            MessageBox.Show("The current connection is not editable", "New Connection from Current Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mnuShowStartPage_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowStartPage = this.mnuShowStartPage.Checked;
            Properties.Settings.Default.Save();
        }

        private void mnuStartPage_Click(object sender, EventArgs e)
        {
            StartPage start = new StartPage(this._recentConnections, this._favouriteConnections);
            start.Owner = this;
            start.ShowDialog();
        }
    }
}