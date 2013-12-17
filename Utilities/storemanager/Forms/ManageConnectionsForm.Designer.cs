/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
            this.tabActiveConnections = new System.Windows.Forms.TabPage();
            this.lvwActive = new VDS.RDF.Utilities.StoreManager.Controls.ConnectionManagementListView();
            this.tabRecentConnections = new System.Windows.Forms.TabPage();
            this.lvwRecent = new VDS.RDF.Utilities.StoreManager.Controls.ConnectionManagementListView();
            this.tabFavouriteConnections = new System.Windows.Forms.TabPage();
            this.lvwFavourite = new VDS.RDF.Utilities.StoreManager.Controls.ConnectionManagementListView();
            this.tlpLayout.SuspendLayout();
            this.tabConnections.SuspendLayout();
            this.tabActiveConnections.SuspendLayout();
            this.tabRecentConnections.SuspendLayout();
            this.tabFavouriteConnections.SuspendLayout();
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
            this.tabConnections.Controls.Add(this.tabActiveConnections);
            this.tabConnections.Controls.Add(this.tabRecentConnections);
            this.tabConnections.Controls.Add(this.tabFavouriteConnections);
            this.tabConnections.Location = new System.Drawing.Point(3, 3);
            this.tabConnections.Name = "tabConnections";
            this.tabConnections.SelectedIndex = 0;
            this.tabConnections.Size = new System.Drawing.Size(900, 352);
            this.tabConnections.TabIndex = 1;
            // 
            // tabActiveConnections
            // 
            this.tabActiveConnections.Controls.Add(this.lvwActive);
            this.tabActiveConnections.Location = new System.Drawing.Point(4, 22);
            this.tabActiveConnections.Name = "tabActiveConnections";
            this.tabActiveConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabActiveConnections.Size = new System.Drawing.Size(892, 326);
            this.tabActiveConnections.TabIndex = 2;
            this.tabActiveConnections.Text = "Active Connections";
            this.tabActiveConnections.UseVisualStyleBackColor = true;
            // 
            // lvwActive
            // 
            this.lvwActive.AllowClose = true;
            this.lvwActive.AllowCopy = true;
            this.lvwActive.AllowEdit = true;
            this.lvwActive.AllowOpen = true;
            this.lvwActive.AllowRemove = false;
            this.lvwActive.AllowRename = true;
            this.lvwActive.AllowShow = true;
            this.lvwActive.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwActive.DataSource = null;
            this.lvwActive.Location = new System.Drawing.Point(6, 6);
            this.lvwActive.MultiSelect = true;
            this.lvwActive.Name = "lvwActive";
            this.lvwActive.RequireConfirmation = false;
            this.lvwActive.Size = new System.Drawing.Size(880, 317);
            this.lvwActive.TabIndex = 0;
            this.lvwActive.View = System.Windows.Forms.View.Details;
            // 
            // tabRecentConnections
            // 
            this.tabRecentConnections.Controls.Add(this.lvwRecent);
            this.tabRecentConnections.Location = new System.Drawing.Point(4, 22);
            this.tabRecentConnections.Name = "tabRecentConnections";
            this.tabRecentConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabRecentConnections.Size = new System.Drawing.Size(892, 326);
            this.tabRecentConnections.TabIndex = 0;
            this.tabRecentConnections.Text = "Recent Connections";
            this.tabRecentConnections.UseVisualStyleBackColor = true;
            // 
            // lvwRecent
            // 
            this.lvwRecent.AllowClose = true;
            this.lvwRecent.AllowCopy = true;
            this.lvwRecent.AllowEdit = true;
            this.lvwRecent.AllowOpen = true;
            this.lvwRecent.AllowRemove = true;
            this.lvwRecent.AllowRename = true;
            this.lvwRecent.AllowShow = true;
            this.lvwRecent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwRecent.DataSource = null;
            this.lvwRecent.Location = new System.Drawing.Point(6, 5);
            this.lvwRecent.MultiSelect = true;
            this.lvwRecent.Name = "lvwRecent";
            this.lvwRecent.RequireConfirmation = false;
            this.lvwRecent.Size = new System.Drawing.Size(880, 317);
            this.lvwRecent.TabIndex = 1;
            this.lvwRecent.View = System.Windows.Forms.View.Details;
            // 
            // tabFavouriteConnections
            // 
            this.tabFavouriteConnections.Controls.Add(this.lvwFavourite);
            this.tabFavouriteConnections.Location = new System.Drawing.Point(4, 22);
            this.tabFavouriteConnections.Name = "tabFavouriteConnections";
            this.tabFavouriteConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabFavouriteConnections.Size = new System.Drawing.Size(892, 326);
            this.tabFavouriteConnections.TabIndex = 1;
            this.tabFavouriteConnections.Text = "Favourite Connections";
            this.tabFavouriteConnections.UseVisualStyleBackColor = true;
            // 
            // lvwFavourite
            // 
            this.lvwFavourite.AllowClose = true;
            this.lvwFavourite.AllowCopy = true;
            this.lvwFavourite.AllowEdit = true;
            this.lvwFavourite.AllowOpen = true;
            this.lvwFavourite.AllowRemove = true;
            this.lvwFavourite.AllowRename = true;
            this.lvwFavourite.AllowShow = true;
            this.lvwFavourite.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwFavourite.DataSource = null;
            this.lvwFavourite.Location = new System.Drawing.Point(6, 5);
            this.lvwFavourite.MultiSelect = true;
            this.lvwFavourite.Name = "lvwFavourite";
            this.lvwFavourite.RequireConfirmation = false;
            this.lvwFavourite.Size = new System.Drawing.Size(880, 317);
            this.lvwFavourite.TabIndex = 1;
            this.lvwFavourite.View = System.Windows.Forms.View.Details;
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
            this.tabActiveConnections.ResumeLayout(false);
            this.tabRecentConnections.ResumeLayout(false);
            this.tabFavouriteConnections.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TabControl tabConnections;
        private System.Windows.Forms.TabPage tabRecentConnections;
        private System.Windows.Forms.TabPage tabFavouriteConnections;
        private System.Windows.Forms.TabPage tabActiveConnections;
        private Controls.ConnectionManagementListView lvwActive;
        private Controls.ConnectionManagementListView lvwRecent;
        private Controls.ConnectionManagementListView lvwFavourite;
    }
}