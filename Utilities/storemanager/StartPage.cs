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
using System.ComponentModel;
using System.Windows.Forms;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class StartPage : Form
    {
        public StartPage(IGraph recent, IGraph faves)
        {
            InitializeComponent();

            this.FillConnectionList(recent, this.lstRecent);
            this.FillConnectionList(faves, this.lstFaves);
            this.chkAlwaysEdit.Checked = Properties.Settings.Default.AlwaysEdit;
            this.chkAlwaysShow.Checked = Properties.Settings.Default.ShowStartPage;
        }

        private void StartPage_Load(object sender, EventArgs e)
        {

        }

        private void FillConnectionList(IGraph config, ListBox lbox)
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));

            query.CommandText = "SELECT * WHERE { ?obj a @type . OPTIONAL { ?obj rdfs:label ?label } }";
            query.CommandText += " ORDER BY DESC(?obj)";
            query.SetParameter("type", config.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider)));

            SparqlResultSet results = config.ExecuteQuery(query) as SparqlResultSet;
            if (results != null)
            {
                foreach (SparqlResult r in results)
                {
                    QuickConnect connect;
                    if (r.HasValue("label") && r["label"] != null)
                    {
                        connect = new QuickConnect(config, r["obj"], r["label"].ToString());
                    }
                    else
                    {
                        connect = new QuickConnect(config, r["obj"]);
                    }
                    lbox.Items.Add(connect);
                }
            }
            lbox.DoubleClick += new EventHandler((sender, args) =>
                {
                    QuickConnect connect = lbox.SelectedItem as QuickConnect;
                    if (connect != null)
                    {
                        if (Properties.Settings.Default.AlwaysEdit)
                        {
                            IConnectionDefinition def = ConnectionDefinitionManager.GetDefinitionByTargetType(connect.Type);
                            if (def != null)
                            {
                                def.PopulateFrom(connect.Graph, connect.ObjectNode);
                                EditConnectionForm edit = new EditConnectionForm(def);
                                if (edit.ShowDialog() == DialogResult.OK)
                                {
                                    IStorageProvider manager = edit.Connection;
                                    StoreManagerForm storeManager = new StoreManagerForm(manager);
                                    storeManager.MdiParent = Program.MainForm;
                                    storeManager.Show();

                                    //Add to Recent Connections
                                    Program.MainForm.AddRecentConnection(manager);

                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Selected Connection is not editable", "Connection Edit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            try
                            {
                                IStorageProvider manager = connect.GetConnection();
                                StoreManagerForm storeManager = new StoreManagerForm(manager);
                                storeManager.MdiParent = Program.MainForm;
                                storeManager.Show();
                                try
                                {
                                    //Add to Recent Connections
                                    Program.MainForm.AddRecentConnection(manager);
                                }
                                catch
                                {
                                    // TODO Issue a warning if this happens
                                }
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error Opening Connection " + connect.ToString() + ":\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                });
        }

        private void btnNewConnection_Click(object sender, EventArgs e)
        {
            NewConnectionForm newConn = new NewConnectionForm();
            if (newConn.ShowDialog() == DialogResult.OK)
            {
                IStorageProvider manager = newConn.Connection;
                StoreManagerForm storeManager = new StoreManagerForm(manager);
                storeManager.MdiParent = Program.MainForm;
                storeManager.Show();

                //Add to Recent Connections
                Program.MainForm.AddRecentConnection(manager);

                this.Close();
            }
        }

        private void chkAlwaysShow_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowStartPage = this.chkAlwaysShow.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkAlwaysEdit_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AlwaysEdit = this.chkAlwaysEdit.Checked;
            Properties.Settings.Default.Save();
        }


        private void mnuEditFave_Click(object sender, EventArgs e)
        {
            EditConnection(this.lstFaves);   
        }

        private void mnuEditRecent_Click(object sender, EventArgs e)
        {
            EditConnection(this.lstRecent);
        }

        private void EditConnection(ListBox lbox)
        {
            QuickConnect connect = lbox.SelectedItem as QuickConnect;
            if (connect == null) return;

            Type t = connect.Type;
            if (t != null)
            {
                IConnectionDefinition def = ConnectionDefinitionManager.GetDefinitionByTargetType(t);
                if (def != null)
                {
                    def.PopulateFrom(connect.Graph, connect.ObjectNode);
                    EditConnectionForm edit = new EditConnectionForm(def);
                    if (edit.ShowDialog() == DialogResult.OK)
                    {
                        IStorageProvider manager = edit.Connection;
                        StoreManagerForm storeManager = new StoreManagerForm(manager);
                        storeManager.MdiParent = Program.MainForm;
                        storeManager.Show();

                        //Add to Recent Connections
                        Program.MainForm.AddRecentConnection(manager);

                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("The selected connection is not editable", "Edit Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The selected connection is not editable", "Edit Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckConnectionContext(ListBox lbox, CancelEventArgs e)
        {
            if (lbox == null) e.Cancel = true;
            if (lbox.SelectedItem == null) e.Cancel = true;
        }

        private void mnuFaveConnections_Opening(object sender, CancelEventArgs e)
        {
            CheckConnectionContext(this.lstFaves, e);
        }

        private void mnuRecentConnections_Opening(object sender, CancelEventArgs e)
        {
            CheckConnectionContext(this.lstRecent, e);
        }
    }
}
