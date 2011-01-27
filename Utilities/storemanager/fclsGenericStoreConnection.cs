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
using System.Windows.Forms;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    public partial class fclsGenericStoreConnection : Form
    {
        private IGenericIOManager _manager;

        public fclsGenericStoreConnection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the Generic IO Manager that the Connection has been created for
        /// </summary>
        public IGenericIOManager Manager
        {
            get
            {
                //If Force Read-Only is checked and Manager is not already Read-Only then we must wrap in a (Queryable)ReadOnlyConnector
                if (this.chkReadOnly.Checked && !this._manager.IsReadOnly)
                {
                    if (this._manager is IQueryableGenericIOManager)
                    {
                        return new QueryableReadOnlyConnector((IQueryableGenericIOManager)this._manager);
                    }
                    else
                    {
                        return new ReadOnlyConnector(this._manager);
                    }
                }
                else
                {
                    return this._manager;
                }
            }
        }

        private void ParameterRequired(String parameter, String store)
        {
            MessageBox.Show("You must enter a value for the " + parameter + " in order to connect to " + store, "Required Parameter Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UriParameterInvalid(String parameter, String store)
        {
            MessageBox.Show("You must enter a valid URI for the " + parameter + " in order to connect to " + store, "Invalid URI Parameter", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ArgumentInvalid(String parameter, String store, ArgumentException argEx)
        {
            MessageBox.Show("The value for the " + parameter + " is invalid due to the following error - " + argEx.Message, "Invalid Parameter", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnConnect4Store_Click(object sender, EventArgs e)
        {
            if (this.txt4StoreServer.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Server URI", "4store");
            }
            else
            {
                this._manager = new FourStoreConnector(this.txt4StoreServer.Text, this.chk4storeUpdates.Checked);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnConnectJoseki_Click(object sender, EventArgs e)
        {
            if (this.txtJosekiServer.Text.Equals(String.Empty)) 
            {
                this.ParameterRequired("Server URI", "Joseki");
            } 
            else if (this.txtJosekiQueryPath.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Query Service Path", "Joseki");
            }
            else if (this.txtJosekiUpdatePath.Text.Equals(String.Empty) && !this.chkJosekiReadOnly.Checked)
            {
                this.ParameterRequired("Update Service Path", "Joseki");
            } 
            else 
            {
                if (this.chkJosekiReadOnly.Checked)
                {
                    this._manager = new JosekiConnector(this.txtJosekiServer.Text, this.txtJosekiQueryPath.Text);
                }
                else
                {
                    this._manager = new JosekiConnector(this.txtJosekiServer.Text, this.txtJosekiQueryPath.Text, this.txtJosekiUpdatePath.Text);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnConnectAllegroGraph_Click(object sender, EventArgs e)
        {
            if (this.txtAllegroGraphServer.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Server URI", "Allegro Graph");
            }
            else if (this.txtAllegroCatalogID.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Catalog ID", "Allegro Graph");
            }
            else if (this.txtAllegroStoreID.Equals(String.Empty))
            {
                this.ParameterRequired("Store ID", "Allegro Graph");
            }
            else
            {
                if (!this.txtAllegroUsername.Text.Equals(String.Empty) && !this.txtAllegroPassword.Equals(String.Empty))
                {
                    this._manager = new AllegroGraphConnector(this.txtAllegroGraphServer.Text, this.txtAllegroCatalogID.Text, this.txtAllegroStoreID.Text, this.txtAllegroUsername.Text, this.txtAllegroPassword.Text);
                }
                else
                {
                    this._manager = new AllegroGraphConnector(this.txtAllegroGraphServer.Text, this.txtAllegroCatalogID.Text, this.txtAllegroStoreID.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnConnectSesame_Click(object sender, EventArgs e)
        {
            if (this.txtSesameServer.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Server URI", "Sesame");
            }
            else if (this.txtSesameStoreID.Equals(String.Empty))
            {
                this.ParameterRequired("Store ID", "Sesame");
            }
            else
            {
                if (!this.txtSesameUsername.Text.Equals(String.Empty) && !this.txtSesamePassword.Equals(String.Empty))
                {
                    this._manager = new SesameHttpProtocolConnector(this.txtSesameServer.Text, this.txtSesameStoreID.Text, this.txtSesameUsername.Text, this.txtSesamePassword.Text);
                }
                else
                {
                    this._manager = new SesameHttpProtocolConnector(this.txtSesameServer.Text, this.txtSesameStoreID.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnConnectTalis_Click(object sender, EventArgs e)
        {
            if (this.txtTalisStoreID.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Store ID", "Talis");
            }
            else
            {
                if (!this.txtTalisUsername.Text.Equals(String.Empty) && !this.txtTalisPassword.Text.Equals(String.Empty))
                {
                    this._manager = new TalisPlatformConnector(this.txtTalisStoreID.Text, this.txtTalisUsername.Text, this.txtTalisPassword.Text);
                }
                else
                {
                    this._manager = new TalisPlatformConnector(this.txtTalisStoreID.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnConnectVirtuoso_Click(object sender, EventArgs e)
        {
            if (this.txtVirtuosoServer.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Server", "Virtuoso");
            }
            else if (this.txtVirtuosoDatabase.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Database", "Virtuoso");
            }
            else if (this.txtVirtuosoUsername.Equals(String.Empty)) 
            {
                this.ParameterRequired("Username", "Virtuoso");
            }
            else if (this.txtVirtuosoPassword.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Password", "Virtuoso");
            }
            else
            {
                int port = 1111;
                if (Int32.TryParse(this.txtVirtuosoPort.Text, out port))
                {
                    this._manager = new VirtuosoManager(this.txtVirtuosoServer.Text, port, this.txtVirtuosoDatabase.Text, this.txtVirtuosoUsername.Text, this.txtVirtuosoPassword.Text);
                }
                else
                {
                    this._manager = new VirtuosoManager(this.txtVirtuosoServer.Text, port, this.txtVirtuosoDatabase.Text, this.txtVirtuosoUsername.Text, this.txtVirtuosoPassword.Text);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnDatasetFileBrowse_Click(object sender, EventArgs e)
        {
            if (ofdDatasetFile.ShowDialog() == DialogResult.OK)
            {
                this.txtDatasetFile.Text = ofdDatasetFile.FileName;
            }
        }

        private void btnConnectDatasetFile_Click(object sender, EventArgs e)
        {
            if (this.txtDatasetFile.Equals(String.Empty))
            {
                this.ParameterRequired("File", "RDF Dataset File");
            }
            else
            {
                try
                {
                    this._manager = new DatasetFileManager(this.txtDatasetFile.Text, true);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to connect to the Dataset File due to the following error:\n" + ex.Message, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void btnConnectSparql_Click(object sender, EventArgs e)
        {
            if (this.txtSparqlEndpoint.Equals(String.Empty))
            {
                this.ParameterRequired("SPARQL Endpoint", "SPARQL Endpoint");
            }
            else
            {
                SparqlConnectorLoadMethod mode;
                if (this.radSparqlConstruct.Checked)
                {
                    mode = SparqlConnectorLoadMethod.Construct;
                }
                else
                {
                    mode = SparqlConnectorLoadMethod.Describe;
                }
                try
                {
                    this._manager = new SparqlConnector(new Uri(this.txtSparqlEndpoint.Text), mode);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (UriFormatException)
                {
                    this.UriParameterInvalid("SPARQL Endpoint", "SPARQL Endpoint");
                }
            }
        }

        private void btnSparqlHttpConnect_Click(object sender, EventArgs e)
        {
            if (this.txtSparqlHttpServer.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Protocol Server", "SPARQL Uniform HTTP Protocol");
            }
            else
            {
                try
                {
                    this._manager = new SparqlHttpProtocolConnector(new Uri(this.txtSparqlHttpServer.Text));
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (UriFormatException)
                {
                    this.UriParameterInvalid("Protocol Server", "SPARQL Uniform HTTP Protocol");
                }
            }
        }

        private void btnConnectFuseki_Click(object sender, EventArgs e)
        {
            if (this.txtFusekiUri.Text.Equals(String.Empty))
            {
                this.ParameterRequired("Fuseki Server URI", "Fuseki");
            }
            else
            {
                try
                {
                    this._manager = new FusekiConnector(this.txtFusekiUri.Text);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (ArgumentException argEx)
                {
                    this.ArgumentInvalid("Fuseki Server URI", "Fuseki", argEx);
                }
            }
        }

    }
}
