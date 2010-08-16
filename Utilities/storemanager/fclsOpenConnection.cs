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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    public partial class fclsOpenConnection : Form
    {
        private IGraph _g;
        private List<INode> _connectionNodes = new List<INode>();

        public fclsOpenConnection(IGraph g)
        {
            InitializeComponent();

            //Find connections defined in the Configuration Graph
            this._g = g;
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.ClassGenericManager);
            INode rdfsLabel = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));

            SparqlParameterizedString getConnections = new SparqlParameterizedString();
            getConnections.QueryText = "SELECT ?obj ?label WHERE { ?obj a @type . OPTIONAL { ?obj @label ?label } } ORDER BY ?label";
            getConnections.SetParameter("type", genericManager);
            getConnections.SetParameter("label", rdfsLabel);

            Object results = this._g.ExecuteQuery(getConnections.ToString());
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    this._connectionNodes.Add(r["obj"]);
                    if (r.HasValue("label"))
                    {
                        this.lstConnections.Items.Add(r["label"]);
                    }
                    else
                    {
                        this.lstConnections.Items.Add(r["obj"]);
                    }
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (this.lstConnections.SelectedIndex != -1)
            {
                int i = this.lstConnections.SelectedIndex;
                INode objNode = this._connectionNodes[i];

                try
                {
                    Object temp = ConfigurationLoader.LoadObject(this._g, objNode);
                    if (temp is IGenericIOManager)
                    {
                        fclsGenericStoreManager storeManager = new fclsGenericStoreManager((IGenericIOManager)temp);
                        storeManager.MdiParent = this.MdiParent;
                        storeManager.Show();
                    }
                    else
                    {
                        MessageBox.Show("Unable to open the selected connection as it was loaded by the Configuration Loader as an object of type '" + temp.GetType().ToString() + "' which does not implement the IGenericIOManager interface", "Open Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open the selected connection due to the following error:\n" + ex.Message, "Open Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
