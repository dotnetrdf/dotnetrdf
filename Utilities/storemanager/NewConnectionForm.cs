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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Controls;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class NewConnectionForm 
        : Form
    {
        private readonly List<IConnectionDefinition> _definitions = new List<IConnectionDefinition>();
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
