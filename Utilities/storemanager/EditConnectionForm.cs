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
using System.Windows.Forms;
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
            this.connSettings.Definition = def.Copy();
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
        public Connection Connection
        {
            get;
            private set;
        }
    }
}
