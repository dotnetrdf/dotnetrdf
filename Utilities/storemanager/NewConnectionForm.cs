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
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Controls;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class NewConnectionForm 
        : Form
    {
        private List<IConnectionDefinition> _definitions = new List<IConnectionDefinition>();
        private IStorageProvider _connection;

        public NewConnectionForm()
        {
            InitializeComponent();

            this._definitions.AddRange(ConnectionDefinitionManager.GetDefinitions().OrderBy(d => d.StoreName));
            this.lstStoreTypes.DataSource = this._definitions;
            this.lstStoreTypes.DisplayMember = "StoreName";
            this.connSettings.Connected += this.HandleConnected;
        }

        public NewConnectionForm(IConnectionDefinition def)
            : this()
        {
            this.lstStoreTypes.SelectedItem = def;
        }

        public IStorageProvider Connection
        {
            get
            {
                return this._connection;
            }
        }

        private void lstStoreTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            IConnectionDefinition def = this.lstStoreTypes.SelectedItem as IConnectionDefinition;
            if (def != null)
            {
                connSettings.Definition = def;
            }
        }

        private void HandleConnected(Object Sender, ConnectedEventArgs e)
        {
            this._connection = e.Connection;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
