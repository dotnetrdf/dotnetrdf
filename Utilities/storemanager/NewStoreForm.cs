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
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class NewStoreForm : Form
    {
        private IStorageServer _server;
        private IStoreTemplate _defaultTemplate;
        private BindingList<IStoreTemplate> _templates = new BindingList<IStoreTemplate>();

        public NewStoreForm(IStorageServer server)
        {
            this._server = server;
            InitializeComponent();

            //Generate Templates
            this._defaultTemplate = this._server.GetDefaultTemplate(String.Empty);
            foreach (IStoreTemplate template in this._server.GetAvailableTemplates(String.Empty))
            {
                this._templates.Add(template);
            }
            this.cboTemplates.DataSource = this._templates;
            this.radTemplateDefault.Text = String.Format(this.radTemplateDefault.Text, this._defaultTemplate.ToString());

            //Wire up property grid
            this.propConfig.SelectedObjectsChanged += new EventHandler(propConfig_SelectedObjectsChanged);
            this.propConfig.PropertyValueChanged += new PropertyValueChangedEventHandler(propConfig_PropertyValueChanged);

        }

        public IStoreTemplate Template
        {
            get;
            set;
        }

        private void txtStoreID_TextChanged(object sender, EventArgs e)
        {
            bool enabled = !this.txtStoreID.Text.Equals(String.Empty);
            this.grpTemplates.Enabled = enabled;
            this.grpConfig.Enabled = enabled;

            this._defaultTemplate.ID = this.txtStoreID.Text;
            foreach (IStoreTemplate t in this._templates)
            {
                t.ID = this.txtStoreID.Text;
            }

            if (enabled)
            {
                //Update Property Grid
                if (this.radTemplateDefault.Checked)
                {
                    this.propConfig.SelectedObject = this._defaultTemplate;
                }
                else
                {
                    this.propConfig.SelectedObject = this.cboTemplates.SelectedItem;
                }
                this.propConfig.Refresh();
            }
            else
            {
                //Clear Templates
                this._templates.Clear();
                this.propConfig.SelectedObject = null;
            }
        }

        private void radTemplateDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radTemplateDefault.Checked)
            {
                this.propConfig.SelectedObject = this._defaultTemplate;
            }
        }

        private void radTemplateSelected_CheckedChanged(object sender, EventArgs e)
        {
            this.cboTemplates.Enabled = this.radTemplateSelected.Checked;
            if (this.radTemplateSelected.Checked)
            {
                this.propConfig.SelectedObject = this.cboTemplates.SelectedItem;
            }
        }

        private void cboTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.radTemplateSelected.Checked)
            {
                this.propConfig.SelectedObject = this.cboTemplates.SelectedItem;
            }
        }


        void propConfig_SelectedObjectsChanged(object sender, EventArgs e)
        {
            this.btnCreate.Enabled = this.propConfig.SelectedObject != null;
        }

        void propConfig_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label.Equals("ID"))
            {
                String id = e.ChangedItem.Value.ToString();
                if (!id.Equals(this.txtStoreID.Text))
                {
                    this.txtStoreID.Text = id;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.Template = this.propConfig.SelectedObject as IStoreTemplate;

            List<String> errors = this.Template.Validate().ToList();
            if (errors.Count > 0)
            {
                InvalidTemplateForm invalid = new InvalidTemplateForm(errors);
                invalid.ShowDialog();
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
    }
}
