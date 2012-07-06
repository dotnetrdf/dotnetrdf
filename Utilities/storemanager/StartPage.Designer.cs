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
            this.grpRecent.Location = new System.Drawing.Point(12, 188);
            this.grpRecent.Name = "grpRecent";
            this.grpRecent.Size = new System.Drawing.Size(474, 141);
            this.grpRecent.TabIndex = 1;
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
            this.mnuRecentConnections.Size = new System.Drawing.Size(160, 48);
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
            this.grpFavourites.Location = new System.Drawing.Point(12, 41);
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
            this.lstFaves.TabIndex = 1;
            // 
            // chkAlwaysShow
            // 
            this.chkAlwaysShow.AutoSize = true;
            this.chkAlwaysShow.Checked = true;
            this.chkAlwaysShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlwaysShow.Location = new System.Drawing.Point(12, 335);
            this.chkAlwaysShow.Name = "chkAlwaysShow";
            this.chkAlwaysShow.Size = new System.Drawing.Size(246, 17);
            this.chkAlwaysShow.TabIndex = 3;
            this.chkAlwaysShow.Text = "Always show this Start Page when starting up?";
            this.chkAlwaysShow.UseVisualStyleBackColor = true;
            this.chkAlwaysShow.CheckedChanged += new System.EventHandler(this.chkAlwaysShow_CheckedChanged);
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 357);
            this.Controls.Add(this.chkAlwaysShow);
            this.Controls.Add(this.grpFavourites);
            this.Controls.Add(this.grpRecent);
            this.Controls.Add(this.btnNewConnection);
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
    }
}