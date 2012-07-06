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
