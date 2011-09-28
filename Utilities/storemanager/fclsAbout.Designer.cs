namespace VDS.RDF.Utilities.StoreManager
{
    partial class fclsAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fclsAbout));
            this.tlpAbout = new System.Windows.Forms.TableLayoutPanel();
            this.lblApiVersionActual = new System.Windows.Forms.Label();
            this.lblAppVersionActual = new System.Windows.Forms.Label();
            this.lblAppVersion = new System.Windows.Forms.Label();
            this.lblApiVersion = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.grpPlugins = new System.Windows.Forms.GroupBox();
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
            this.tlpAbout.Controls.Add(this.lblApiVersionActual, 1, 1);
            this.tlpAbout.Controls.Add(this.lblAppVersionActual, 1, 0);
            this.tlpAbout.Controls.Add(this.lblAppVersion, 0, 0);
            this.tlpAbout.Controls.Add(this.lblApiVersion, 0, 1);
            this.tlpAbout.Controls.Add(this.lblInfo, 0, 2);
            this.tlpAbout.Location = new System.Drawing.Point(12, 12);
            this.tlpAbout.Name = "tlpAbout";
            this.tlpAbout.RowCount = 3;
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpAbout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAbout.Size = new System.Drawing.Size(284, 162);
            this.tlpAbout.TabIndex = 0;
            // 
            // lblApiVersionActual
            // 
            this.lblApiVersionActual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblApiVersionActual.AutoSize = true;
            this.lblApiVersionActual.Location = new System.Drawing.Point(145, 30);
            this.lblApiVersionActual.Name = "lblApiVersionActual";
            this.lblApiVersionActual.Size = new System.Drawing.Size(136, 30);
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
            this.lblAppVersionActual.Location = new System.Drawing.Point(145, 0);
            this.lblAppVersionActual.Name = "lblAppVersionActual";
            this.lblAppVersionActual.Size = new System.Drawing.Size(136, 30);
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
            this.lblAppVersion.Size = new System.Drawing.Size(136, 30);
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
            this.lblApiVersion.Location = new System.Drawing.Point(3, 30);
            this.lblApiVersion.Name = "lblApiVersion";
            this.lblApiVersion.Size = new System.Drawing.Size(136, 30);
            this.lblApiVersion.TabIndex = 1;
            this.lblApiVersion.Text = "dotNetRDF Version:";
            this.lblApiVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.tlpAbout.SetColumnSpan(this.lblInfo, 2);
            this.lblInfo.Location = new System.Drawing.Point(3, 60);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(275, 91);
            this.lblInfo.TabIndex = 4;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // grpPlugins
            // 
            this.grpPlugins.Controls.Add(this.lstPlugins);
            this.grpPlugins.Location = new System.Drawing.Point(12, 180);
            this.grpPlugins.Name = "grpPlugins";
            this.grpPlugins.Size = new System.Drawing.Size(284, 100);
            this.grpPlugins.TabIndex = 1;
            this.grpPlugins.TabStop = false;
            this.grpPlugins.Text = "Detected Plugins";
            // 
            // lstPlugins
            // 
            this.lstPlugins.FormattingEnabled = true;
            this.lstPlugins.Location = new System.Drawing.Point(6, 19);
            this.lstPlugins.Name = "lstPlugins";
            this.lstPlugins.Size = new System.Drawing.Size(272, 69);
            this.lstPlugins.TabIndex = 0;
            // 
            // fclsAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 284);
            this.Controls.Add(this.grpPlugins);
            this.Controls.Add(this.tlpAbout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fclsAbout";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Store Manager";
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
    }
}