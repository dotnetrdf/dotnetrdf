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

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    partial class NewConnectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewConnectionForm));
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.panStoreTypes = new System.Windows.Forms.Panel();
            this.lstStoreTypes = new System.Windows.Forms.ListBox();
            this.connSettings = new VDS.RDF.Utilities.StoreManager.Controls.ConnectionSettingsGrid();
            this.ofdBrowse = new System.Windows.Forms.OpenFileDialog();
            this.panName = new System.Windows.Forms.Panel();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.tlpLayout.SuspendLayout();
            this.panStoreTypes.SuspendLayout();
            this.panName.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpLayout
            // 
            this.tlpLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLayout.ColumnCount = 2;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.69014F));
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.30986F));
            this.tlpLayout.Controls.Add(this.panStoreTypes, 0, 0);
            this.tlpLayout.Controls.Add(this.connSettings, 1, 1);
            this.tlpLayout.Controls.Add(this.panName, 1, 0);
            this.tlpLayout.Location = new System.Drawing.Point(0, 0);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 2;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Size = new System.Drawing.Size(959, 352);
            this.tlpLayout.TabIndex = 0;
            // 
            // panStoreTypes
            // 
            this.panStoreTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panStoreTypes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panStoreTypes.Controls.Add(this.lstStoreTypes);
            this.panStoreTypes.Location = new System.Drawing.Point(3, 3);
            this.panStoreTypes.Name = "panStoreTypes";
            this.tlpLayout.SetRowSpan(this.panStoreTypes, 2);
            this.panStoreTypes.Size = new System.Drawing.Size(202, 346);
            this.panStoreTypes.TabIndex = 0;
            // 
            // lstStoreTypes
            // 
            this.lstStoreTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstStoreTypes.FormattingEnabled = true;
            this.lstStoreTypes.Location = new System.Drawing.Point(3, 9);
            this.lstStoreTypes.Name = "lstStoreTypes";
            this.lstStoreTypes.Size = new System.Drawing.Size(196, 329);
            this.lstStoreTypes.TabIndex = 0;
            this.lstStoreTypes.SelectedIndexChanged += new System.EventHandler(this.lstStoreTypes_SelectedIndexChanged);
            // 
            // connSettings
            // 
            this.connSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connSettings.AutoSize = true;
            this.connSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.connSettings.Definition = null;
            this.connSettings.Location = new System.Drawing.Point(211, 38);
            this.connSettings.Name = "connSettings";
            this.connSettings.Size = new System.Drawing.Size(745, 311);
            this.connSettings.TabIndex = 1;
            // 
            // panName
            // 
            this.panName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panName.Controls.Add(this.txtName);
            this.panName.Controls.Add(this.lblName);
            this.panName.Location = new System.Drawing.Point(211, 3);
            this.panName.Name = "panName";
            this.panName.Size = new System.Drawing.Size(745, 29);
            this.panName.TabIndex = 2;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(182, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Connection Friendly Name (Optional):";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(191, 6);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(547, 20);
            this.txtName.TabIndex = 1;
            // 
            // NewConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 356);
            this.Controls.Add(this.tlpLayout);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewConnectionForm";
            this.Text = "New Connection";
            this.tlpLayout.ResumeLayout(false);
            this.tlpLayout.PerformLayout();
            this.panStoreTypes.ResumeLayout(false);
            this.panName.ResumeLayout(false);
            this.panName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.Panel panStoreTypes;
        private System.Windows.Forms.ListBox lstStoreTypes;
        private System.Windows.Forms.OpenFileDialog ofdBrowse;
        private Controls.ConnectionSettingsGrid connSettings;
        private System.Windows.Forms.Panel panName;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
    }
}