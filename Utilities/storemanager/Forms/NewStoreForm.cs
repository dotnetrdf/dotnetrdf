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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    public partial class NewStoreForm : Form
    {
        private readonly IStorageServer _server;
        private readonly IStoreTemplate _defaultTemplate;
        private readonly BindingList<IStoreTemplate> _templates = new BindingList<IStoreTemplate>();

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

        /// <summary>
        /// Gets/Sets the template
        /// </summary>
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
                this.propConfig.SelectedObject = this.radTemplateDefault.Checked ? this._defaultTemplate : this.cboTemplates.SelectedItem;
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
            if (e.ChangedItem.Label == null || !e.ChangedItem.Label.Equals("ID")) return;
            String id = e.ChangedItem.Value.ToString();
            if (!id.Equals(this.txtStoreID.Text))
            {
                this.txtStoreID.Text = id;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            this.Template = this.propConfig.SelectedObject as IStoreTemplate;

            List<String> errors = this.Template != null ? this.Template.Validate().ToList() : new List<string> { "No template selected"};
            if (errors.Count > 0)
            {
                InvalidTemplateForm invalid = new InvalidTemplateForm(errors);
                invalid.ShowDialog();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
