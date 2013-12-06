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

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    partial class ConnectionSettingsGrid
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpSettings = new System.Windows.Forms.TableLayoutPanel();
            this.panSettings = new System.Windows.Forms.Panel();
            this.grpConnectionSettings = new System.Windows.Forms.GroupBox();
            this.chkForceReadOnly = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tblSettings = new System.Windows.Forms.TableLayoutPanel();
            this.lblDescrip = new System.Windows.Forms.Label();
            this.ofdBrowse = new System.Windows.Forms.OpenFileDialog();
            this.tlpSettings.SuspendLayout();
            this.panSettings.SuspendLayout();
            this.grpConnectionSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpSettings
            // 
            this.tlpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpSettings.AutoSize = true;
            this.tlpSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpSettings.ColumnCount = 1;
            this.tlpSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSettings.Controls.Add(this.panSettings, 0, 1);
            this.tlpSettings.Controls.Add(this.lblDescrip, 0, 0);
            this.tlpSettings.Location = new System.Drawing.Point(3, 3);
            this.tlpSettings.Name = "tlpSettings";
            this.tlpSettings.RowCount = 2;
            this.tlpSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSettings.Size = new System.Drawing.Size(744, 343);
            this.tlpSettings.TabIndex = 0;
            // 
            // panSettings
            // 
            this.panSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panSettings.AutoScroll = true;
            this.panSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panSettings.Controls.Add(this.grpConnectionSettings);
            this.panSettings.Location = new System.Drawing.Point(3, 53);
            this.panSettings.Name = "panSettings";
            this.panSettings.Size = new System.Drawing.Size(738, 287);
            this.panSettings.TabIndex = 1;
            // 
            // grpConnectionSettings
            // 
            this.grpConnectionSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConnectionSettings.Controls.Add(this.chkForceReadOnly);
            this.grpConnectionSettings.Controls.Add(this.btnConnect);
            this.grpConnectionSettings.Controls.Add(this.tblSettings);
            this.grpConnectionSettings.Location = new System.Drawing.Point(3, 3);
            this.grpConnectionSettings.Name = "grpConnectionSettings";
            this.grpConnectionSettings.Size = new System.Drawing.Size(732, 282);
            this.grpConnectionSettings.TabIndex = 0;
            this.grpConnectionSettings.TabStop = false;
            this.grpConnectionSettings.Text = "Connection Settings";
            // 
            // chkForceReadOnly
            // 
            this.chkForceReadOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkForceReadOnly.AutoSize = true;
            this.chkForceReadOnly.Location = new System.Drawing.Point(6, 261);
            this.chkForceReadOnly.Name = "chkForceReadOnly";
            this.chkForceReadOnly.Size = new System.Drawing.Size(189, 17);
            this.chkForceReadOnly.TabIndex = 1;
            this.chkForceReadOnly.Text = "Force Connection to be read-only?";
            this.chkForceReadOnly.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(651, 255);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "&Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tblSettings
            // 
            this.tblSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblSettings.AutoScroll = true;
            this.tblSettings.ColumnCount = 2;
            this.tblSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.88848F));
            this.tblSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.11152F));
            this.tblSettings.Location = new System.Drawing.Point(6, 14);
            this.tblSettings.Name = "tblSettings";
            this.tblSettings.RowCount = 10;
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tblSettings.Size = new System.Drawing.Size(720, 242);
            this.tblSettings.TabIndex = 0;
            // 
            // lblDescrip
            // 
            this.lblDescrip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDescrip.AutoEllipsis = true;
            this.lblDescrip.Location = new System.Drawing.Point(3, 10);
            this.lblDescrip.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.lblDescrip.Name = "lblDescrip";
            this.lblDescrip.Size = new System.Drawing.Size(738, 40);
            this.lblDescrip.TabIndex = 0;
            this.lblDescrip.Text = "No Connection Definition has been specified";
            // 
            // ConnectionSettingsGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpSettings);
            this.Name = "ConnectionSettingsGrid";
            this.Size = new System.Drawing.Size(750, 350);
            this.tlpSettings.ResumeLayout(false);
            this.panSettings.ResumeLayout(false);
            this.grpConnectionSettings.ResumeLayout(false);
            this.grpConnectionSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpSettings;
        private System.Windows.Forms.Panel panSettings;
        private System.Windows.Forms.GroupBox grpConnectionSettings;
        private System.Windows.Forms.CheckBox chkForceReadOnly;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TableLayoutPanel tblSettings;
        private System.Windows.Forms.Label lblDescrip;
        private System.Windows.Forms.OpenFileDialog ofdBrowse;
    }
}
