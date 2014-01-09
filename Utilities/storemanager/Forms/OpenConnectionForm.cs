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
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    /// <summary>
    /// A Form that can be used to select an IStorageProvider instance defined in an RDF Graph using the dotNetRDF Configuration Vocabulary
    /// </summary>
    public partial class OpenConnectionForm : Form
    {
        /// <summary>
        /// Creates a new Open Connection Form
        /// </summary>
        /// <param name="connections">Graph contaning Connection Definitions</param>
        public OpenConnectionForm(IConnectionsGraph connections)
        {
            InitializeComponent();

            foreach (Connection connection in connections.Connections)
            {
                this.lstConnections.Items.Add(connection);
            }
        }

        /// <summary>
        /// Gets the Connection created when the User clicked the Open button
        /// </summary>
        /// <remarks>
        /// May be null if the User closes/cancels the Form
        /// </remarks>
        public Connection Connection { get; private set; }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (this.lstConnections.SelectedIndex == -1) return;
            this.Connection = this.lstConnections.SelectedItem as Connection;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}