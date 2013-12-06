namespace VDS.RDF.Utilities.StoreManager
{
    partial class ManageConnectionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageConnectionsForm));
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabConnections = new System.Windows.Forms.TabControl();
            this.tabRecentConnections = new System.Windows.Forms.TabPage();
            this.tabFavouriteConnections = new System.Windows.Forms.TabPage();
            this.tlpLayout.SuspendLayout();
            this.tabConnections.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpLayout
            // 
            this.tlpLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpLayout.ColumnCount = 1;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLayout.Controls.Add(this.btnClose, 0, 1);
            this.tlpLayout.Controls.Add(this.tabConnections, 0, 0);
            this.tlpLayout.Location = new System.Drawing.Point(3, 3);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 2;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpLayout.Size = new System.Drawing.Size(906, 388);
            this.tlpLayout.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(828, 362);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // tabConnections
            // 
            this.tabConnections.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabConnections.Controls.Add(this.tabRecentConnections);
            this.tabConnections.Controls.Add(this.tabFavouriteConnections);
            this.tabConnections.Location = new System.Drawing.Point(3, 3);
            this.tabConnections.Name = "tabConnections";
            this.tabConnections.SelectedIndex = 0;
            this.tabConnections.Size = new System.Drawing.Size(900, 352);
            this.tabConnections.TabIndex = 1;
            // 
            // tabRecentConnections
            // 
            this.tabRecentConnections.Location = new System.Drawing.Point(4, 22);
            this.tabRecentConnections.Name = "tabRecentConnections";
            this.tabRecentConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabRecentConnections.Size = new System.Drawing.Size(892, 326);
            this.tabRecentConnections.TabIndex = 0;
            this.tabRecentConnections.Text = "Recent Connections";
            this.tabRecentConnections.UseVisualStyleBackColor = true;
            // 
            // tabFavouriteConnections
            // 
            this.tabFavouriteConnections.Location = new System.Drawing.Point(4, 22);
            this.tabFavouriteConnections.Name = "tabFavouriteConnections";
            this.tabFavouriteConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabFavouriteConnections.Size = new System.Drawing.Size(892, 326);
            this.tabFavouriteConnections.TabIndex = 1;
            this.tabFavouriteConnections.Text = "Favourite Connections";
            this.tabFavouriteConnections.UseVisualStyleBackColor = true;
            // 
            // ManageConnectionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 395);
            this.Controls.Add(this.tlpLayout);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManageConnectionsForm";
            this.Text = "Manage Connections";
            this.tlpLayout.ResumeLayout(false);
            this.tabConnections.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TabControl tabConnections;
        private System.Windows.Forms.TabPage tabRecentConnections;
        private System.Windows.Forms.TabPage tabFavouriteConnections;
    }
}