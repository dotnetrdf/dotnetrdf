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

namespace VDS.RDF.Utilities.StoreManager
{
    partial class NewStoreForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblStoreID = new System.Windows.Forms.Label();
            this.txtStoreID = new System.Windows.Forms.TextBox();
            this.grpTemplates = new System.Windows.Forms.GroupBox();
            this.cboTemplates = new System.Windows.Forms.ComboBox();
            this.radTemplateSelected = new System.Windows.Forms.RadioButton();
            this.radTemplateDefault = new System.Windows.Forms.RadioButton();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.propConfig = new System.Windows.Forms.PropertyGrid();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpTemplates.SuspendLayout();
            this.grpConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStoreID
            // 
            this.lblStoreID.AutoSize = true;
            this.lblStoreID.Location = new System.Drawing.Point(12, 9);
            this.lblStoreID.Name = "lblStoreID";
            this.lblStoreID.Size = new System.Drawing.Size(49, 13);
            this.lblStoreID.TabIndex = 0;
            this.lblStoreID.Text = "Store ID:";
            // 
            // txtStoreID
            // 
            this.txtStoreID.Location = new System.Drawing.Point(67, 6);
            this.txtStoreID.Name = "txtStoreID";
            this.txtStoreID.Size = new System.Drawing.Size(170, 20);
            this.txtStoreID.TabIndex = 1;
            this.txtStoreID.TextChanged += new System.EventHandler(this.txtStoreID_TextChanged);
            // 
            // grpTemplates
            // 
            this.grpTemplates.Controls.Add(this.cboTemplates);
            this.grpTemplates.Controls.Add(this.radTemplateSelected);
            this.grpTemplates.Controls.Add(this.radTemplateDefault);
            this.grpTemplates.Enabled = false;
            this.grpTemplates.Location = new System.Drawing.Point(15, 32);
            this.grpTemplates.Name = "grpTemplates";
            this.grpTemplates.Size = new System.Drawing.Size(517, 83);
            this.grpTemplates.TabIndex = 2;
            this.grpTemplates.TabStop = false;
            this.grpTemplates.Text = "Store Templates";
            // 
            // cboTemplates
            // 
            this.cboTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTemplates.Enabled = false;
            this.cboTemplates.FormattingEnabled = true;
            this.cboTemplates.Location = new System.Drawing.Point(155, 50);
            this.cboTemplates.Name = "cboTemplates";
            this.cboTemplates.Size = new System.Drawing.Size(344, 21);
            this.cboTemplates.TabIndex = 2;
            this.cboTemplates.SelectedIndexChanged += new System.EventHandler(this.cboTemplates_SelectedIndexChanged);
            // 
            // radTemplateSelected
            // 
            this.radTemplateSelected.AutoSize = true;
            this.radTemplateSelected.Location = new System.Drawing.Point(16, 51);
            this.radTemplateSelected.Name = "radTemplateSelected";
            this.radTemplateSelected.Size = new System.Drawing.Size(133, 17);
            this.radTemplateSelected.TabIndex = 1;
            this.radTemplateSelected.TabStop = true;
            this.radTemplateSelected.Text = "Use selected template:";
            this.radTemplateSelected.UseVisualStyleBackColor = true;
            this.radTemplateSelected.CheckedChanged += new System.EventHandler(this.radTemplateSelected_CheckedChanged);
            // 
            // radTemplateDefault
            // 
            this.radTemplateDefault.AutoSize = true;
            this.radTemplateDefault.Checked = true;
            this.radTemplateDefault.Location = new System.Drawing.Point(16, 28);
            this.radTemplateDefault.Name = "radTemplateDefault";
            this.radTemplateDefault.Size = new System.Drawing.Size(151, 17);
            this.radTemplateDefault.TabIndex = 0;
            this.radTemplateDefault.TabStop = true;
            this.radTemplateDefault.Text = "Use Default Template ({0})";
            this.radTemplateDefault.UseVisualStyleBackColor = true;
            this.radTemplateDefault.CheckedChanged += new System.EventHandler(this.radTemplateDefault_CheckedChanged);
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.propConfig);
            this.grpConfig.Enabled = false;
            this.grpConfig.Location = new System.Drawing.Point(15, 121);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(517, 211);
            this.grpConfig.TabIndex = 3;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Template Configuration";
            // 
            // propConfig
            // 
            this.propConfig.Location = new System.Drawing.Point(16, 19);
            this.propConfig.Name = "propConfig";
            this.propConfig.Size = new System.Drawing.Size(483, 186);
            this.propConfig.TabIndex = 0;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(194, 339);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 4;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(275, 339);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // NewStoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 374);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.grpTemplates);
            this.Controls.Add(this.txtStoreID);
            this.Controls.Add(this.lblStoreID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewStoreForm";
            this.Text = "Create New Store";
            this.grpTemplates.ResumeLayout(false);
            this.grpTemplates.PerformLayout();
            this.grpConfig.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStoreID;
        private System.Windows.Forms.TextBox txtStoreID;
        private System.Windows.Forms.GroupBox grpTemplates;
        private System.Windows.Forms.ComboBox cboTemplates;
        private System.Windows.Forms.RadioButton radTemplateSelected;
        private System.Windows.Forms.RadioButton radTemplateDefault;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.PropertyGrid propConfig;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}