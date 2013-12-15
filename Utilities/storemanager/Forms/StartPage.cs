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
        public StartPage(IConnectionsGraph recent, IConnectionsGraph faves)
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

        private void FillConnectionList(IConnectionsGraph connections, ListBox lbox)
        {
            foreach (Connection connection in connections.Connections)
            {
                lbox.Items.Add(connection);
            }
            lbox.DoubleClick += (sender, args) =>
                {
                    Connection connection = lbox.SelectedItem as Connection;
                    if (connection == null) return;
                    if (Properties.Settings.Default.AlwaysEdit)
                    {
                        EditConnectionForm edit = new EditConnectionForm(connection.Definition);
                        if (edit.ShowDialog() == DialogResult.OK)
                        {
                            connection = edit.Connection;
                            StoreManagerForm storeManager = new StoreManagerForm(connection);
                            storeManager.MdiParent = Program.MainForm;
                            storeManager.Show();

                            //Add to Recent Connections
                            Program.MainForm.AddRecentConnection(connection);

                            this.Close();
                        }
                    }
                    else
                    {
                        try
                        {
                            connection.Open();
                            StoreManagerForm storeManager = new StoreManagerForm(connection);
                            storeManager.MdiParent = Program.MainForm;
                            storeManager.Show();
                            //Add to Recent Connections
                            Program.MainForm.AddRecentConnection(connection);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error Opening Connection " + connection.Name + ":\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };
        }

        private void btnNewConnection_Click(object sender, EventArgs e)
        {
            NewConnectionForm newConn = new NewConnectionForm();
            if (newConn.ShowDialog() == DialogResult.OK)
            {
                Connection connection = newConn.Connection;
                StoreManagerForm storeManager = new StoreManagerForm(connection);
                storeManager.MdiParent = Program.MainForm;
                storeManager.Show();

                //Add to Recent Connections
                Program.MainForm.AddRecentConnection(connection);

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
            Connection connection = lbox.SelectedItem as Connection;
            if (connection == null) return;

            EditConnectionForm edit = new EditConnectionForm(connection.Definition);
            if (edit.ShowDialog() == DialogResult.OK)
            {
                connection = edit.Connection;
                StoreManagerForm storeManager = new StoreManagerForm(connection);
                storeManager.MdiParent = Program.MainForm;
                storeManager.Show();

                //Add to Recent Connections
                Program.MainForm.AddRecentConnection(connection);

                this.Close();
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