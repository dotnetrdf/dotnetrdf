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
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Configuration;
using VDS.RDF.Writing;

namespace dotNetRDFStore
{
    public partial class fclsManager : Form
    {
        public fclsManager()
        {
            InitializeComponent();
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
                fclsGenericStoreManager storeManager = new fclsGenericStoreManager(connector.Manager);
                storeManager.MdiParent = this;
                storeManager.Show();

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

                    fclsOpenConnection openConnections = new fclsOpenConnection(g);
                    openConnections.MdiParent = this;
                    openConnections.Show();
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
    }
}
