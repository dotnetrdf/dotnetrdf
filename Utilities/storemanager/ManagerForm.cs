/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF;
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
        private IGraph _recentConnections = new QueryableGraph();
        private String _recentConnectionsFile;

        private IGraph _faveConnections = new QueryableGraph();
        private String _faveConnectionsFile;

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
                String sepChar = new String(new char[] { Path.DirectorySeparatorChar });
                if (!appDataDir.EndsWith(sepChar)) appDataDir += sepChar;
                appDataDir = Path.Combine(appDataDir, "dotNetRDF" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                appDataDir = Path.Combine(appDataDir, "Store Manager" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                this._recentConnectionsFile = Path.Combine(appDataDir, "recent.ttl");
                this._faveConnectionsFile = Path.Combine(appDataDir, "favourite.ttl");

                if (File.Exists(this._recentConnectionsFile))
                {
                    //Load Recent Connections
                    FileLoader.Load(this._recentConnections, this._recentConnectionsFile);
                    this.FillConnectionsMenu(this.mnuRecentConnections, this._recentConnections, MaxRecentConnections);
                }
                if (File.Exists(this._faveConnectionsFile))
                {
                    //Load Favourite Connections
                    FileLoader.Load(this._faveConnections, this._faveConnectionsFile);
                    this.FillConnectionsMenu(this.mnuFavouriteConnections, this._faveConnections, 0, true, this._faveConnectionsFile);
                }
            }
            catch (Exception ex)
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
                StartPage start = new StartPage(this._recentConnections, this._faveConnections);
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
                    Object manager;
                    if (this.ActiveMdiChild is StoreManagerForm)
                    {
                        manager = ((StoreManagerForm)this.ActiveMdiChild).Manager;
                    }
                    else
                    {
                        return;
                    }
                    if (manager is IConfigurationSerializable)
                    {
                        this.sfdConnection.Filter = MimeTypesHelper.GetFilenameFilter(true, false, false, false, false, false);
                        if (this.sfdConnection.ShowDialog() == DialogResult.OK)
                        {
                            //Append to existing configuration file or overwrite?
                            ConfigurationSerializationContext context;
                            if (File.Exists(this.sfdConnection.FileName))
                            {
                                DialogResult result = MessageBox.Show("The selected connection file already exists - would you like to append this connection to that file?  Click Yes to append to this file, No to overwrite and Cancel to abort", "Append Connection?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                                switch (result)
                                {
                                    case DialogResult.Yes:
                                        Graph g = new Graph();
                                        FileLoader.Load(g, this.sfdConnection.FileName);
                                        context = new ConfigurationSerializationContext(g);
                                        break;
                                    case DialogResult.No:
                                        context = new ConfigurationSerializationContext();
                                        break;
                                    default:
                                        return;
                                }
                            }
                            else
                            {
                                //Create new configuration file
                                context = new ConfigurationSerializationContext();
                            }

                            //Save the Connection
                            ((IConfigurationSerializable)manager).SerializeConfiguration(context);

                            try
                            {
                                IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(MimeTypesHelper.GetTrueFileExtension(this.sfdConnection.FileName));
                                writer.Save(context.Graph, this.sfdConnection.FileName);
                            }
                            catch (RdfWriterSelectionException)
                            {
                                CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
                                ttlwriter.Save(context.Graph, this.sfdConnection.FileName);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to save the current connection as it does not support this feature", "Save Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    Graph g = new Graph();
                    FileLoader.Load(g, this.ofdConnection.FileName);

                    OpenConnectionForm openConnections = new OpenConnectionForm(g);
                    openConnections.MdiParent = this;
                    if (openConnections.ShowDialog() == DialogResult.OK)
                    {
                        IStorageProvider manager = openConnections.Connection;
                        StoreManagerForm genManagerForm = new StoreManagerForm(manager);
                        genManagerForm.MdiParent = this;
                        genManagerForm.Show();

                        //Add to Recent Connections
                        this.AddRecentConnection(manager);
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

        private void FillConnectionsMenu(ToolStripMenuItem menu, IGraph config)
        {
            this.FillConnectionsMenu(menu, config, 0);
        }

        private void FillConnectionsMenu(ToolStripMenuItem menu, IGraph config, int maxItems)
        {
            this.FillConnectionsMenu(menu, config, maxItems, false, null);
        }

        private void FillConnectionsMenu(ToolStripMenuItem menu, IGraph config, int maxItems, bool addRemoveOption)
        {
            this.FillConnectionsMenu(menu, config, maxItems, addRemoveOption, null);
        }

        private void FillConnectionsMenu(ToolStripMenuItem menu, IGraph config, int maxItems, bool addRemoveOption, String persistentFile)
        {
            if (config == null || config.Triples.Count == 0) return;

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));

            query.CommandText = "SELECT * WHERE { ?obj a @type . OPTIONAL { ?obj rdfs:label ?label } }";
            query.CommandText += " ORDER BY DESC(?obj)";

            Graph g = new Graph();
            query.SetParameter("type", g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));

            if (maxItems > 0) query.CommandText += " LIMIT " + maxItems;

            SparqlResultSet results = config.ExecuteQuery(query) as SparqlResultSet;
            if (results != null)
            {
                foreach (SparqlResult r in results)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    if (r.HasValue("label") && r["label"] != null)
                    {
                        INode lblNode = r["label"];
                        if (lblNode.NodeType == NodeType.Literal)
                        {
                            item.Text = ((ILiteralNode)lblNode).Value;
                        }
                        else
                        {
                            item.Text = lblNode.ToString();
                        }
                    }
                    else
                    {
                        item.Text = r["obj"].ToString();
                    }
                    item.Tag = new QuickConnect(config, r["obj"]);
                    item.Click += new EventHandler(QuickConnectClick);

                    ToolStripMenuItem edit = new ToolStripMenuItem();
                    edit.Text = "Edit Connection";
                    edit.Tag = item.Tag;
                    edit.Click += new EventHandler(QuickEditClick);
                    item.DropDownItems.Add(edit);

                    if (addRemoveOption)
                    {
                        ToolStripMenuItem remove = new ToolStripMenuItem();
                        remove.Text = "Remove Connection from this List";
                        remove.Tag = new QuickRemove(menu, config, r["obj"], persistentFile);
                        remove.Click += new EventHandler(QuickRemoveClick);
                        item.DropDownItems.Add(remove);

                        ToolStripMenuItem connect = new ToolStripMenuItem();
                        connect.Text = "Open Connection";
                        connect.Tag = item.Tag;
                        connect.Click += new EventHandler(QuickConnectClick);
                        item.DropDownItems.Add(connect);
                    }

                    menu.DropDownItems.Add(item);
                }
            }
        }

        private void RemoveFromConnectionsMenu(ToolStripMenuItem menu, INode objNode)
        {
            if (menu.DropDownItems.Count > 2)
            {
                int i = 2;
                while (i < menu.DropDownItems.Count)
                {
                    Object tag = menu.DropDownItems[i].Tag;
                    if (tag is QuickConnect)
                    {
                        if (((QuickConnect)tag).ObjectNode.Equals(objNode))
                        {
                            menu.DropDownItems.RemoveAt(i);
                            i--;
                        }
                    }
                    i++;
                }
            }
        }

        private void QuickConnectClick(object sender, EventArgs e)
        {
            if (sender == null) return;
            Object tag = null;
            if (sender is Control)
            {
                tag = ((Control)sender).Tag;
            }
            else if (sender is ToolStripItem)
            {
                tag = ((ToolStripItem)sender).Tag;
            }
            else if (sender is Menu)
            {
                tag = ((Menu)sender).Tag;
            }

            if (tag != null)
            {
                if (tag is QuickConnect)
                {
                    QuickConnect qc = (QuickConnect)tag;
                    try
                    {
                        IStorageProvider manager = qc.GetConnection();
                        StoreManagerForm genManager = new StoreManagerForm(manager);
                        genManager.MdiParent = this;
                        genManager.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to load the Connection due to an error: " + ex.Message, "Quick Connect Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void QuickEditClick(object sender, EventArgs e)
        {
            if (sender == null) return;
            Object tag = null;
            if (sender is Control)
            {
                tag = ((Control)sender).Tag;
            }
            else if (sender is ToolStripItem)
            {
                tag = ((ToolStripItem)sender).Tag;
            }
            else if (sender is Menu)
            {
                tag = ((Menu)sender).Tag;
            }

            if (tag != null)
            {
                if (tag is QuickConnect)
                {
                    QuickConnect qc = (QuickConnect)tag;
                    try
                    {
                        if (qc.Type != null)
                        {
                            IConnectionDefinition def = ConnectionDefinitionManager.GetDefinition(qc.Type);
                            if (def != null)
                            {
                                def.PopulateFrom(qc.Graph, qc.ObjectNode);
                                EditConnectionForm editConn = new EditConnectionForm(def);
                                if (editConn.ShowDialog() == DialogResult.OK)
                                {
                                    IStorageProvider manager = editConn.Connection;
                                    StoreManagerForm storeManager = new StoreManagerForm(manager);
                                    storeManager.MdiParent = Program.MainForm;
                                    storeManager.Show();

                                    //Add to Recent Connections
                                    this.AddRecentConnection(manager);

                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("This Connection is not ediatable", "Quick Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("This Connection is not ediatable", "Quick Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to edit the Connection due to an error: " + ex.Message, "Quick Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void QuickRemoveClick(object sender, EventArgs e)
        {
            if (sender == null) return;
            Object tag = null;
            if (sender is ToolStripItem)
            {
                tag = ((ToolStripItem)sender).Tag;
            }

            if (tag != null)
            {
                if (tag is QuickRemove)
                {
                    QuickRemove rem = (QuickRemove)tag;
                    rem.Remove();
                    this.RemoveFromConnectionsMenu(rem.Menu, rem.ObjectNode);
                }
            }
        }

        public void AddRecentConnection(IStorageProvider manager)
        {
            INode objNode = this.AddConnection(this._recentConnections, manager, this._recentConnectionsFile);

            if (objNode != null) this.AddConnectionToMenu(manager, this._recentConnections, objNode, this.mnuRecentConnections, this._recentConnectionsFile, false);

            //Check the number of Recent Connections and delete the Oldest if more than 9
            INode rdfType = this._recentConnections.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode storageProvider = this._recentConnections.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            List<INode> conns = (from t in
                                 this._recentConnections.GetTriplesWithPredicateObject(rdfType, storageProvider)
                                 select t.Subject).ToList();
            if (conns.Count > MaxRecentConnections)
            {
                
                conns.Sort();
                conns.Reverse();

                conns.RemoveRange(0, MaxRecentConnections);

                //Remember the ToList() on the retract otherwise we'll hit an error
                foreach (INode obj in conns)
                {
                    this._recentConnections.Retract(this._recentConnections.GetTriplesWithSubject(obj).ToList());
                    this.RemoveFromConnectionsMenu(this.mnuRecentConnections, obj);
                }

                try
                {
                    //Persist the graph to disk
                    CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter();
                    ttlwriter.Save(this._recentConnections, this._recentConnectionsFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to persist a Connections File to disk", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }            
        }

        public void AddFavouriteConnection(IStorageProvider manager)
        {
            INode objNode = this.AddConnection(this._faveConnections, manager, this._faveConnectionsFile);
            this.AddConnectionToMenu(manager, this._faveConnections, objNode, this.mnuFavouriteConnections, this._faveConnectionsFile, true);    
        }

        private void AddConnectionToMenu(IStorageProvider manager, IGraph g, INode objNode, ToolStripMenuItem parentItem, String persistentFile, bool addRemove)
        {
            if (objNode != null)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = manager.ToString();
                item.Tag = new QuickConnect(g, objNode);
                item.Click += new EventHandler(QuickConnectClick);

                ToolStripMenuItem edit = new ToolStripMenuItem();
                edit.Text = "Edit Connection";
                edit.Tag = item.Tag;
                edit.Click += new EventHandler(QuickEditClick);
                item.DropDownItems.Add(edit);

                if (addRemove)
                {
                    ToolStripMenuItem remove = new ToolStripMenuItem();
                    remove.Text = "Remove Connection from this List";
                    remove.Tag = new QuickRemove(parentItem, g, objNode, persistentFile);
                    remove.Click += new EventHandler(QuickRemoveClick);
                    item.DropDownItems.Add(remove);

                    ToolStripMenuItem connect = new ToolStripMenuItem();
                    connect.Text = "Open Connection";
                    connect.Tag = item.Tag;
                    connect.Click += new EventHandler(QuickConnectClick);
                    item.DropDownItems.Add(connect);
                }

                parentItem.DropDownItems.Add(item);
            }
        }

        private INode AddConnection(IGraph config, IStorageProvider manager, String persistentFile)
        {
            if (config == null) return null;

            ConfigurationSerializationContext context = new ConfigurationSerializationContext(config);

            if (manager is IConfigurationSerializable)
            {
                INode objNode = context.Graph.CreateUriNode(new Uri("dotnetrdf:storemanager:" + DateTime.Now.ToString("yyyyMMddhhmmss")));
                context.NextSubject = objNode;
                ((IConfigurationSerializable)manager).SerializeConfiguration(context);

                if (persistentFile != null)
                {
                    try
                    {
                        //Persist the graph to disk
                        CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter();
                        ttlwriter.Save(config, persistentFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to persist a Connections File to disk", "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                return objNode;
            }

            return null;
        }

        private void ClearRecentConnections()
        {
            this.ClearConnections(this.mnuRecentConnections, this._recentConnections, this._recentConnectionsFile);
        }

        private void ClearFavouriteConnections()
        {
            if (MessageBox.Show("Are you sure you wish to clear your Favourite Connections?", "Confirm Clear Favourite Connections", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.ClearConnections(this.mnuFavouriteConnections, this._faveConnections, this._faveConnectionsFile);
            }
        }

        private void ClearConnections(ToolStripMenuItem menu, IGraph g, String persistentFile)
        {
            g.Clear();
            if (persistentFile != null && File.Exists(persistentFile)) File.Delete(persistentFile);

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
                    IStorageProvider manager = ((StoreManagerForm)this.ActiveMdiChild).Manager;
                    this.AddFavouriteConnection(manager);
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
                IStorageProvider manager = newConn.Connection;
                StoreManagerForm storeManager = new StoreManagerForm(manager);
                storeManager.MdiParent = this;
                storeManager.Show();

                //Add to Recent Connections
                this.AddRecentConnection(manager);
            }
        }

        private void mnuNewFromExisting_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is StoreManagerForm)
                {
                    IStorageProvider manager = ((StoreManagerForm)this.ActiveMdiChild).Manager;
                    IConnectionDefinition def = ConnectionDefinitionManager.GetDefinition(manager.GetType());
                    if (def != null)
                    {
                        if (manager is IConfigurationSerializable)
                        {
                            Graph g = new Graph();
                            ConfigurationSerializationContext ctx = new ConfigurationSerializationContext(g);
                            INode n = g.CreateBlankNode();
                            ctx.NextSubject = n;
                            ((IConfigurationSerializable)manager).SerializeConfiguration(ctx);
                            def.PopulateFrom(g, n);

                            EditConnectionForm editConn = new EditConnectionForm(def);
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
            StartPage start = new StartPage(this._recentConnections, this._faveConnections);
            start.Owner = this;
            start.ShowDialog();
        }
    }
}
