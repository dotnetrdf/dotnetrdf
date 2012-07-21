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
    partial class StartPage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartPage));
            this.btnNewConnection = new System.Windows.Forms.Button();
            this.grpRecent = new System.Windows.Forms.GroupBox();
            this.lstRecent = new System.Windows.Forms.ListBox();
            this.mnuRecentConnections = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuEditRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFaveConnections = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuEditFave = new System.Windows.Forms.ToolStripMenuItem();
            this.grpFavourites = new System.Windows.Forms.GroupBox();
            this.lstFaves = new System.Windows.Forms.ListBox();
            this.chkAlwaysShow = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAlwaysEdit = new System.Windows.Forms.CheckBox();
            this.grpRecent.SuspendLayout();
            this.mnuRecentConnections.SuspendLayout();
            this.mnuFaveConnections.SuspendLayout();
            this.grpFavourites.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnNewConnection
            // 
            this.btnNewConnection.FlatAppearance.BorderSize = 0;
            this.btnNewConnection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("btnNewConnection.Image")));
            this.btnNewConnection.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnNewConnection.Location = new System.Drawing.Point(12, 12);
            this.btnNewConnection.Name = "btnNewConnection";
            this.btnNewConnection.Size = new System.Drawing.Size(474, 23);
            this.btnNewConnection.TabIndex = 0;
            this.btnNewConnection.Text = "Connect to a Triple Store";
            this.btnNewConnection.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.btnNewConnection.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnNewConnection.UseVisualStyleBackColor = true;
            this.btnNewConnection.Click += new System.EventHandler(this.btnNewConnection_Click);
            // 
            // grpRecent
            // 
            this.grpRecent.Controls.Add(this.lstRecent);
            this.grpRecent.Location = new System.Drawing.Point(12, 218);
            this.grpRecent.Name = "grpRecent";
            this.grpRecent.Size = new System.Drawing.Size(474, 141);
            this.grpRecent.TabIndex = 3;
            this.grpRecent.TabStop = false;
            this.grpRecent.Text = "Recent Connections";
            // 
            // lstRecent
            // 
            this.lstRecent.ContextMenuStrip = this.mnuRecentConnections;
            this.lstRecent.FormattingEnabled = true;
            this.lstRecent.Location = new System.Drawing.Point(6, 19);
            this.lstRecent.Name = "lstRecent";
            this.lstRecent.Size = new System.Drawing.Size(462, 108);
            this.lstRecent.TabIndex = 0;
            // 
            // mnuRecentConnections
            // 
            this.mnuRecentConnections.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditRecent});
            this.mnuRecentConnections.Name = "mnuConnections";
            this.mnuRecentConnections.Size = new System.Drawing.Size(160, 26);
            this.mnuRecentConnections.Opening += new System.ComponentModel.CancelEventHandler(this.mnuRecentConnections_Opening);
            // 
            // mnuEditRecent
            // 
            this.mnuEditRecent.Name = "mnuEditRecent";
            this.mnuEditRecent.Size = new System.Drawing.Size(159, 22);
            this.mnuEditRecent.Text = "Edit Connection";
            this.mnuEditRecent.Click += new System.EventHandler(this.mnuEditRecent_Click);
            // 
            // mnuFaveConnections
            // 
            this.mnuFaveConnections.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditFave});
            this.mnuFaveConnections.Name = "mnuConnections";
            this.mnuFaveConnections.Size = new System.Drawing.Size(160, 26);
            this.mnuFaveConnections.Opening += new System.ComponentModel.CancelEventHandler(this.mnuFaveConnections_Opening);
            // 
            // mnuEditFave
            // 
            this.mnuEditFave.Name = "mnuEditFave";
            this.mnuEditFave.Size = new System.Drawing.Size(159, 22);
            this.mnuEditFave.Text = "Edit Connection";
            this.mnuEditFave.Click += new System.EventHandler(this.mnuEditFave_Click);
            // 
            // grpFavourites
            // 
            this.grpFavourites.Controls.Add(this.lstFaves);
            this.grpFavourites.Location = new System.Drawing.Point(12, 71);
            this.grpFavourites.Name = "grpFavourites";
            this.grpFavourites.Size = new System.Drawing.Size(474, 141);
            this.grpFavourites.TabIndex = 2;
            this.grpFavourites.TabStop = false;
            this.grpFavourites.Text = "Favourite Connections";
            // 
            // lstFaves
            // 
            this.lstFaves.ContextMenuStrip = this.mnuFaveConnections;
            this.lstFaves.FormattingEnabled = true;
            this.lstFaves.Location = new System.Drawing.Point(6, 19);
            this.lstFaves.Name = "lstFaves";
            this.lstFaves.Size = new System.Drawing.Size(462, 108);
            this.lstFaves.TabIndex = 0;
            // 
            // chkAlwaysShow
            // 
            this.chkAlwaysShow.AutoSize = true;
            this.chkAlwaysShow.Checked = true;
            this.chkAlwaysShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlwaysShow.Location = new System.Drawing.Point(12, 383);
            this.chkAlwaysShow.Name = "chkAlwaysShow";
            this.chkAlwaysShow.Size = new System.Drawing.Size(246, 17);
            this.chkAlwaysShow.TabIndex = 5;
            this.chkAlwaysShow.Text = "Always show this Start Page when starting up?";
            this.chkAlwaysShow.UseVisualStyleBackColor = true;
            this.chkAlwaysShow.CheckedChanged += new System.EventHandler(this.chkAlwaysShow_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(471, 30);
            this.label1.TabIndex = 1;
            this.label1.Text = "Double click any connection below to open it, alternatively click once to select," +
    " then right click and hit Edit Connection to edit a previous connection.";
            // 
            // chkAlwaysEdit
            // 
            this.chkAlwaysEdit.AutoSize = true;
            this.chkAlwaysEdit.Location = new System.Drawing.Point(12, 365);
            this.chkAlwaysEdit.Name = "chkAlwaysEdit";
            this.chkAlwaysEdit.Size = new System.Drawing.Size(334, 17);
            this.chkAlwaysEdit.TabIndex = 4;
            this.chkAlwaysEdit.Text = "Always Edit rather than Open Connections when double clicking?";
            this.chkAlwaysEdit.UseVisualStyleBackColor = true;
            this.chkAlwaysEdit.CheckedChanged += new System.EventHandler(this.chkAlwaysEdit_CheckedChanged);
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 412);
            this.Controls.Add(this.chkAlwaysEdit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnNewConnection);
            this.Controls.Add(this.grpFavourites);
            this.Controls.Add(this.chkAlwaysShow);
            this.Controls.Add(this.grpRecent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StartPage";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Get Started";
            this.Load += new System.EventHandler(this.StartPage_Load);
            this.grpRecent.ResumeLayout(false);
            this.mnuRecentConnections.ResumeLayout(false);
            this.mnuFaveConnections.ResumeLayout(false);
            this.grpFavourites.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnNewConnection;
        private System.Windows.Forms.GroupBox grpRecent;
        private System.Windows.Forms.GroupBox grpFavourites;
        private System.Windows.Forms.ListBox lstRecent;
        private System.Windows.Forms.ListBox lstFaves;
        private System.Windows.Forms.CheckBox chkAlwaysShow;
        private System.Windows.Forms.ContextMenuStrip mnuFaveConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFave;
        private System.Windows.Forms.ContextMenuStrip mnuRecentConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuEditRecent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAlwaysEdit;
    }
}