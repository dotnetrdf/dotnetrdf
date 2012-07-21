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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Controls;

namespace VDS.RDF.Utilities.StoreManager
{
    /// <summary>
    /// Form for editing connection definitions
    /// </summary>
    public partial class EditConnectionForm
        : Form
    {
        /// <summary>
        /// Creates a new Edit Connection Form
        /// </summary>
        /// <param name="def">Definition</param>
        public EditConnectionForm(IConnectionDefinition def)
        {
            InitializeComponent();
            this.connSettings.Definition = def;
            this.connSettings.Connected += this.HandleConnected;
        }

        /// <summary>
        /// Handles successfull connections
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Connection Event arguments</param>
        private void HandleConnected(Object sender, ConnectedEventArgs e)
        {
            this.Connection = e.Connection;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Gets the Connection if it has been created
        /// </summary>
        public IStorageProvider Connection
        {
            get;
            private set;
        }
    }
}
