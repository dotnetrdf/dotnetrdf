/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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