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
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class AboutForm
        : Form
    {
        private HashSet<Assembly> _assemblies = new HashSet<Assembly>();

        public AboutForm()
        {
            InitializeComponent();
            this.lblAppVersionActual.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.lblCoreVersionActual.Text = Assembly.GetAssembly(typeof(IConnectionDefinition)).GetName().Version.ToString();
            this.lblApiVersionActual.Text = Assembly.GetAssembly(typeof(IGraph)).GetName().Version.ToString();

            ShowDetectedAssemblies();
        }

        private void ShowDetectedAssemblies()
        {
            Assembly current = Assembly.GetExecutingAssembly();
            foreach (Type t in ConnectionDefinitionManager.DefinitionTypes)
            {
                Assembly assm = Assembly.GetAssembly(t);
                if (!ReferenceEquals(current, assm))
                {
                    if (!this._assemblies.Contains(assm))
                    {
                        this._assemblies.Add(assm);
                        this.lstPlugins.Items.Add(assm.ToString());
                    }
                }
            }
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            this._assemblies.Clear();
            this.lstPlugins.BeginUpdate();
            this.lstPlugins.Items.Clear();
            ConnectionDefinitionManager.ScanPlugins();
            this.ShowDetectedAssemblies();
            this.lstPlugins.EndUpdate();
        }
    }
}
