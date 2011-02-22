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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
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

namespace dotNetRDFStore
{
    public partial class fclsManager : Form
    {
        private IGraph _recentConnections = new Graph();
        private String _recentConnectionsFile;

        public fclsManager()
        {
            InitializeComponent();

            //Check whether we have a Recent Connections Graph
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

                if (File.Exists(this._recentConnectionsFile))
                {
                    FileLoader.Load(this._recentConnections, this._recentConnectionsFile);

                    this.FillConnectionsMenu(this.mnuRecentConnections, this._recentConnections, 9);
                }
            }
            catch
            {
                //If errors occur then ignore Recent Connections
            }
        }

        private void fclsManager_Load(object sender, EventArgs e)
        {
            
        }

        private void mnuStrip_MenuActivate(object sender, System.EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                Form activeChild = this.ActiveMdiChild;
                this.ActivateMdiChild(null);
                this.ActivateMdiChild(activeChild);

                if (this.ActiveMdiChild is fclsGenericStoreManager)
                {
                    this.mnuSaveConnection.Enabled = true;
                }
                else if (this.ActiveMdiChild is fclsSQLStoreManager)
                {
                    this.mnuSaveConnection.Enabled = true;
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

        private void mnuNewSQLStoreManager_Click(object sender, EventArgs e)
        {
            fclsSQLStoreManager storeManager = new fclsSQLStoreManager();
            storeManager.MdiParent = this;
            storeManager.Show();

            if (this.MdiChildren.Count() > 1)
            {
                this.LayoutMdi(MdiLayout.Cascade);
            }
        }

        private void mnuNewGenericStoreManager_Click(object sender, EventArgs e)
        {
            fclsGenericStoreConnection connector = new fclsGenericStoreConnection();
            if (connector.ShowDialog() == DialogResult.OK)
            {
                IGenericIOManager manager = connector.Manager;
                fclsGenericStoreManager storeManager = new fclsGenericStoreManager(manager);
                storeManager.MdiParent = this;
                storeManager.Show();

                //Add to Recent Connections
                this.AddRecentConnection(manager);

                if (this.MdiChildren.Count() > 1)
                {
                    this.LayoutMdi(MdiLayout.Cascade);
                }
            }
        }

        private void mnuSaveConnection_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild != null)
            {
                if (this.ActiveMdiChild is fclsGenericStoreManager || this.ActiveMdiChild is fclsSQLStoreManager)
                {
                    Object manager;
                    if (this.ActiveMdiChild is fclsGenericStoreManager)
                    {
                        manager = ((fclsGenericStoreManager)this.ActiveMdiChild).Manager;
                    }
                    else if (this.ActiveMdiChild is fclsSQLStoreManager)
                    {
                        manager = ((fclsSQLStoreManager)this.ActiveMdiChild).Manager;
                        if (manager == null) return;
                    }
                    else
                    {
                        return;
                    }
                    if (manager is IConfigurationSerializable)
                    {
                        this.sfdConnection.Filter = Constants.RdfFilter;
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
                                IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(Path.GetExtension(this.sfdConnection.FileName)));
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
            this.ofdConnection.Filter = Constants.RdfFilter;
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
                        IGenericIOManager manager = openConnections.Connection;
                        fclsGenericStoreManager genManagerForm = new fclsGenericStoreManager(manager);
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

        private void FillConnectionsMenu(ToolStripMenuItem menu, IGraph config, int maxItems)
        {
            if (config == null || config.Triples.Count == 0) return;

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));

            query.CommandText = "SELECT * WHERE { ?obj a " + ConfigurationLoader.ClassGenericManager + " . OPTIONAL { ?obj rdfs:label ?label } }";
            query.CommandText += " ORDER BY DESC(?obj)";
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
                            item.Text = ((LiteralNode)lblNode).Value;
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

                    menu.DropDownItems.Add(item);
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
                        IGenericIOManager manager = qc.GetConnection();
                        fclsGenericStoreManager genManager = new fclsGenericStoreManager(manager);
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

        private void AddRecentConnection(IGenericIOManager manager)
        {
            INode objNode = this.AddConnection(this._recentConnections, manager, this._recentConnectionsFile);

            if (objNode != null)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = manager.ToString();
                item.Tag = new QuickConnect(this._recentConnections, objNode);
                item.Click += new EventHandler(QuickConnectClick);
                this.mnuRecentConnections.DropDownItems.Add(item);
            }
        }

        private INode AddConnection(IGraph config, IGenericIOManager manager, String persistentFile)
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
                        using (StreamWriter writer = new StreamWriter(persistentFile, false, Encoding.UTF8))
                        {
                            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter();
                            ttlwriter.Save(config, writer);
                            writer.Close();
                        }
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
            this._recentConnections.Clear();
            File.Delete(this._recentConnectionsFile);

            while (this.mnuRecentConnections.DropDownItems.Count > 2)
            {
                this.mnuRecentConnections.DropDownItems.RemoveAt(2);
            }
        }

        private void mnuClearRecentConnections_Click(object sender, EventArgs e)
        {
            this.ClearRecentConnections();
        }
    }
}
