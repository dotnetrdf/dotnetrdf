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

namespace VDS.RDF.Utilities.StoreManager
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.tlpAbout = new System.Windows.Forms.TableLayoutPanel();
            this.lblCoreVersionActual = new System.Windows.Forms.Label();
            this.lblCoreVersion = new System.Windows.Forms.Label();
            this.lblApiVersionActual = new System.Windows.Forms.Label();
            this.lblAppVersionActual = new System.Windows.Forms.Label();
            this.lblAppVersion = new System.Windows.Forms.Label();
            this.lblApiVersion = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.grpPlugins = new System.Windows.Forms.GroupBox();
            this.btnRescan = new System.Windows.Forms.Button();
            this.lstPlugins = new System.Windows.Forms.ListBox();
            this.tlpAbout.SuspendLayout();
            this.grpPlugins.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpAbout
            // 
            this.tlpAbout.ColumnCount = 2;
            this.tlpAbout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAbout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpAbout.Controls.Add(this.lblCoreVersionActual, 1, 1);
            this.tlpAbout.Controls.Add(this.lblCoreVersion, 0, 1);
            this.tlpAbout.Controls.Add(this.lblApiVersionActual, 1, 2);
            this.tlpAbout.Controls.Add(this.lblAppVersionActual, 1, 0);
            this.tlpAbout.Controls.Add(this.lblAppVersion, 0, 0);
            this.tlpAbout.Controls.Add(this.lblApiVersion, 0, 2);
            this.tlpAbout.Controls.Add(this.lblInfo, 0, 3);
            this.tlpAbout.Location = new System.Drawing.Point(12, 12);
            this.tlpAbout.Name = "tlpAbout";
            this.tlpAbout.RowCount = 4;
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpAbout.Size = new System.Drawing.Size(411, 214);
            this.tlpAbout.TabIndex = 0;
            // 
            // lblCoreVersionActual
            // 
            this.lblCoreVersionActual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCoreVersionActual.AutoSize = true;
            this.lblCoreVersionActual.Location = new System.Drawing.Point(208, 30);
            this.lblCoreVersionActual.Name = "lblCoreVersionActual";
            this.lblCoreVersionActual.Size = new System.Drawing.Size(200, 30);
            this.lblCoreVersionActual.TabIndex = 6;
            this.lblCoreVersionActual.Text = "{0}";
            this.lblCoreVersionActual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCoreVersion
            // 
            this.lblCoreVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCoreVersion.AutoSize = true;
            this.lblCoreVersion.Location = new System.Drawing.Point(3, 30);
            this.lblCoreVersion.Name = "lblCoreVersion";
            this.lblCoreVersion.Size = new System.Drawing.Size(199, 30);
            this.lblCoreVersion.TabIndex = 5;
            this.lblCoreVersion.Text = "StoreManager.Core Version:";
            this.lblCoreVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblApiVersionActual
            // 
            this.lblApiVersionActual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblApiVersionActual.AutoSize = true;
            this.lblApiVersionActual.Location = new System.Drawing.Point(208, 60);
            this.lblApiVersionActual.Name = "lblApiVersionActual";
            this.lblApiVersionActual.Size = new System.Drawing.Size(200, 30);
            this.lblApiVersionActual.TabIndex = 3;
            this.lblApiVersionActual.Text = "{0}";
            this.lblApiVersionActual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAppVersionActual
            // 
            this.lblAppVersionActual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAppVersionActual.AutoSize = true;
            this.lblAppVersionActual.Location = new System.Drawing.Point(208, 0);
            this.lblAppVersionActual.Name = "lblAppVersionActual";
            this.lblAppVersionActual.Size = new System.Drawing.Size(200, 30);
            this.lblAppVersionActual.TabIndex = 2;
            this.lblAppVersionActual.Text = "{0}";
            this.lblAppVersionActual.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAppVersion
            // 
            this.lblAppVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAppVersion.AutoSize = true;
            this.lblAppVersion.Location = new System.Drawing.Point(3, 0);
            this.lblAppVersion.Name = "lblAppVersion";
            this.lblAppVersion.Size = new System.Drawing.Size(199, 30);
            this.lblAppVersion.TabIndex = 0;
            this.lblAppVersion.Text = "Store Manager Version:";
            this.lblAppVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblApiVersion
            // 
            this.lblApiVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblApiVersion.AutoSize = true;
            this.lblApiVersion.Location = new System.Drawing.Point(3, 60);
            this.lblApiVersion.Name = "lblApiVersion";
            this.lblApiVersion.Size = new System.Drawing.Size(199, 30);
            this.lblApiVersion.TabIndex = 1;
            this.lblApiVersion.Text = "dotNetRDF Version:";
            this.lblApiVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.tlpAbout.SetColumnSpan(this.lblInfo, 2);
            this.lblInfo.Location = new System.Drawing.Point(3, 90);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(404, 104);
            this.lblInfo.TabIndex = 4;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // grpPlugins
            // 
            this.grpPlugins.Controls.Add(this.btnRescan);
            this.grpPlugins.Controls.Add(this.lstPlugins);
            this.grpPlugins.Location = new System.Drawing.Point(12, 232);
            this.grpPlugins.Name = "grpPlugins";
            this.grpPlugins.Size = new System.Drawing.Size(411, 127);
            this.grpPlugins.TabIndex = 1;
            this.grpPlugins.TabStop = false;
            this.grpPlugins.Text = "Detected Connection Definition Sources and Plugins";
            // 
            // btnRescan
            // 
            this.btnRescan.Location = new System.Drawing.Point(327, 94);
            this.btnRescan.Name = "btnRescan";
            this.btnRescan.Size = new System.Drawing.Size(75, 23);
            this.btnRescan.TabIndex = 1;
            this.btnRescan.Text = "Rescan Plugins";
            this.btnRescan.UseVisualStyleBackColor = true;
            this.btnRescan.Click += new System.EventHandler(this.btnRescan_Click);
            // 
            // lstPlugins
            // 
            this.lstPlugins.FormattingEnabled = true;
            this.lstPlugins.Location = new System.Drawing.Point(6, 19);
            this.lstPlugins.Name = "lstPlugins";
            this.lstPlugins.Size = new System.Drawing.Size(396, 69);
            this.lstPlugins.TabIndex = 0;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 368);
            this.Controls.Add(this.grpPlugins);
            this.Controls.Add(this.tlpAbout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Store Manager";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.tlpAbout.ResumeLayout(false);
            this.tlpAbout.PerformLayout();
            this.grpPlugins.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpAbout;
        private System.Windows.Forms.Label lblAppVersion;
        private System.Windows.Forms.Label lblApiVersion;
        private System.Windows.Forms.Label lblApiVersionActual;
        private System.Windows.Forms.Label lblAppVersionActual;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpPlugins;
        private System.Windows.Forms.ListBox lstPlugins;
        private System.Windows.Forms.Button btnRescan;
        private System.Windows.Forms.Label lblCoreVersionActual;
        private System.Windows.Forms.Label lblCoreVersion;
    }
}