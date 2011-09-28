using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class StartPage : Form
    {
        public StartPage(IGraph recent, IGraph faves)
        {
            InitializeComponent();

            this.FillConnectionList(recent, this.lstRecent);
            this.FillConnectionList(faves, this.lstFaves);
        }

        private void StartPage_Load(object sender, EventArgs e)
        {

        }

        private void FillConnectionList(IGraph config, ListBox lbox)
        {
             SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            query.Namespaces.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));

            query.CommandText = "SELECT * WHERE { ?obj a " + ConfigurationLoader.ClassGenericManager + " . OPTIONAL { ?obj rdfs:label ?label } }";
            query.CommandText += " ORDER BY DESC(?obj)";

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
                        try
                        {
                            IGenericIOManager manager = connect.GetConnection();
                            fclsGenericStoreManager storeManager = new fclsGenericStoreManager(manager);
                            storeManager.MdiParent = Program.MainForm;
                            storeManager.Show();

                            //Add to Recent Connections
                            Program.MainForm.AddRecentConnection(manager);

                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error Opening Connection " + connect.ToString() + ":\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });
        }

        private void btnNewConnection_Click(object sender, EventArgs e)
        {
            NewConnectionForm newConn = new NewConnectionForm();
            if (newConn.ShowDialog() == DialogResult.OK)
            {
                IGenericIOManager manager = newConn.Connection;
                fclsGenericStoreManager storeManager = new fclsGenericStoreManager(manager);
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
    }
}
