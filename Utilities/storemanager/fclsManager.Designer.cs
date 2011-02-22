namespace dotNetRDFStore
{
    partial class fclsManager
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
            this.mnuStrip = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewSQLStoreManager = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewGenericStoreManager = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSaveConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddFavourite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFavouriteConnections = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClearFavouriteConnections = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuRecentConnections = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClearRecentConnections = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUseUtf8Bom = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCascade = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTileVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTileHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuArrangeIcons = new System.Windows.Forms.ToolStripMenuItem();
            this.stsBar = new System.Windows.Forms.StatusStrip();
            this.sfdConnection = new System.Windows.Forms.SaveFileDialog();
            this.ofdConnection = new System.Windows.Forms.OpenFileDialog();
            this.mnuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuStrip
            // 
            this.mnuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuOptions,
            this.mnuWindows});
            this.mnuStrip.Location = new System.Drawing.Point(0, 0);
            this.mnuStrip.MdiWindowListItem = this.mnuWindows;
            this.mnuStrip.Name = "mnuStrip";
            this.mnuStrip.Size = new System.Drawing.Size(632, 24);
            this.mnuStrip.TabIndex = 0;
            this.mnuStrip.Text = "MenuStrip";
            this.mnuStrip.MenuActivate += new System.EventHandler(this.mnuStrip_MenuActivate);
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewSQLStoreManager,
            this.mnuNewGenericStoreManager,
            this.mnuSeparator1,
            this.mnuSaveConnection,
            this.mnuAddFavourite,
            this.mnuOpenConnection,
            this.mnuSeparator2,
            this.mnuFavouriteConnections,
            this.mnuRecentConnections,
            this.mnuSeparator4,
            this.mnuExit});
            this.mnuFile.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "&File";
            // 
            // mnuNewSQLStoreManager
            // 
            this.mnuNewSQLStoreManager.Name = "mnuNewSQLStoreManager";
            this.mnuNewSQLStoreManager.Size = new System.Drawing.Size(232, 22);
            this.mnuNewSQLStoreManager.Text = "New SQL Store Manager";
            this.mnuNewSQLStoreManager.Click += new System.EventHandler(this.mnuNewSQLStoreManager_Click);
            // 
            // mnuNewGenericStoreManager
            // 
            this.mnuNewGenericStoreManager.Name = "mnuNewGenericStoreManager";
            this.mnuNewGenericStoreManager.Size = new System.Drawing.Size(232, 22);
            this.mnuNewGenericStoreManager.Text = "New Generic Store Manager";
            this.mnuNewGenericStoreManager.Click += new System.EventHandler(this.mnuNewGenericStoreManager_Click);
            // 
            // mnuSeparator1
            // 
            this.mnuSeparator1.Name = "mnuSeparator1";
            this.mnuSeparator1.Size = new System.Drawing.Size(229, 6);
            // 
            // mnuSaveConnection
            // 
            this.mnuSaveConnection.Name = "mnuSaveConnection";
            this.mnuSaveConnection.Size = new System.Drawing.Size(232, 22);
            this.mnuSaveConnection.Text = "&Save Connection";
            this.mnuSaveConnection.Click += new System.EventHandler(this.mnuSaveConnection_Click);
            // 
            // mnuAddFavourite
            // 
            this.mnuAddFavourite.Name = "mnuAddFavourite";
            this.mnuAddFavourite.Size = new System.Drawing.Size(232, 22);
            this.mnuAddFavourite.Text = "Add Connection to &Favourites";
            this.mnuAddFavourite.Click += new System.EventHandler(this.mnuAddFavourite_Click);
            // 
            // mnuOpenConnection
            // 
            this.mnuOpenConnection.Name = "mnuOpenConnection";
            this.mnuOpenConnection.Size = new System.Drawing.Size(232, 22);
            this.mnuOpenConnection.Text = "&Open Connection";
            this.mnuOpenConnection.Click += new System.EventHandler(this.mnuOpenConnection_Click);
            // 
            // mnuSeparator2
            // 
            this.mnuSeparator2.Name = "mnuSeparator2";
            this.mnuSeparator2.Size = new System.Drawing.Size(229, 6);
            // 
            // mnuFavouriteConnections
            // 
            this.mnuFavouriteConnections.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuClearFavouriteConnections,
            this.mnuSeparator5});
            this.mnuFavouriteConnections.Name = "mnuFavouriteConnections";
            this.mnuFavouriteConnections.Size = new System.Drawing.Size(232, 22);
            this.mnuFavouriteConnections.Text = "Favourite Connections...";
            // 
            // mnuClearFavouriteConnections
            // 
            this.mnuClearFavouriteConnections.Name = "mnuClearFavouriteConnections";
            this.mnuClearFavouriteConnections.Size = new System.Drawing.Size(223, 22);
            this.mnuClearFavouriteConnections.Text = "Clear Favourite Connections";
            this.mnuClearFavouriteConnections.Click += new System.EventHandler(this.mnuClearFavouriteConnections_Click);
            // 
            // mnuSeparator5
            // 
            this.mnuSeparator5.Name = "mnuSeparator5";
            this.mnuSeparator5.Size = new System.Drawing.Size(220, 6);
            // 
            // mnuRecentConnections
            // 
            this.mnuRecentConnections.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuClearRecentConnections,
            this.mnuSeparator3});
            this.mnuRecentConnections.Name = "mnuRecentConnections";
            this.mnuRecentConnections.Size = new System.Drawing.Size(232, 22);
            this.mnuRecentConnections.Text = "Recently Used Connections...";
            // 
            // mnuClearRecentConnections
            // 
            this.mnuClearRecentConnections.Name = "mnuClearRecentConnections";
            this.mnuClearRecentConnections.Size = new System.Drawing.Size(210, 22);
            this.mnuClearRecentConnections.Text = "Clear Recent Connections";
            this.mnuClearRecentConnections.Click += new System.EventHandler(this.mnuClearRecentConnections_Click);
            // 
            // mnuSeparator3
            // 
            this.mnuSeparator3.Name = "mnuSeparator3";
            this.mnuSeparator3.Size = new System.Drawing.Size(207, 6);
            // 
            // mnuSeparator4
            // 
            this.mnuSeparator4.Name = "mnuSeparator4";
            this.mnuSeparator4.Size = new System.Drawing.Size(229, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(232, 22);
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuOptions
            // 
            this.mnuOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuUseUtf8Bom});
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(61, 20);
            this.mnuOptions.Text = "&Options";
            // 
            // mnuUseUtf8Bom
            // 
            this.mnuUseUtf8Bom.Checked = true;
            this.mnuUseUtf8Bom.CheckOnClick = true;
            this.mnuUseUtf8Bom.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuUseUtf8Bom.Name = "mnuUseUtf8Bom";
            this.mnuUseUtf8Bom.Size = new System.Drawing.Size(217, 22);
            this.mnuUseUtf8Bom.Text = "Use BOM for UTF-8 Output";
            this.mnuUseUtf8Bom.Click += new System.EventHandler(this.mnuUseUtf8Bom_Click);
            // 
            // mnuWindows
            // 
            this.mnuWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCascade,
            this.mnuTileVertical,
            this.mnuTileHorizontal,
            this.mnuCloseAll,
            this.mnuArrangeIcons});
            this.mnuWindows.Name = "mnuWindows";
            this.mnuWindows.Size = new System.Drawing.Size(68, 20);
            this.mnuWindows.Text = "&Windows";
            // 
            // mnuCascade
            // 
            this.mnuCascade.Name = "mnuCascade";
            this.mnuCascade.Size = new System.Drawing.Size(151, 22);
            this.mnuCascade.Text = "&Cascade";
            this.mnuCascade.Click += new System.EventHandler(this.mnuCascade_Click);
            // 
            // mnuTileVertical
            // 
            this.mnuTileVertical.Name = "mnuTileVertical";
            this.mnuTileVertical.Size = new System.Drawing.Size(151, 22);
            this.mnuTileVertical.Text = "Tile &Vertical";
            this.mnuTileVertical.Click += new System.EventHandler(this.mnuTileVertical_Click);
            // 
            // mnuTileHorizontal
            // 
            this.mnuTileHorizontal.Name = "mnuTileHorizontal";
            this.mnuTileHorizontal.Size = new System.Drawing.Size(151, 22);
            this.mnuTileHorizontal.Text = "Tile &Horizontal";
            this.mnuTileHorizontal.Click += new System.EventHandler(this.mnuTileHorizontal_Click);
            // 
            // mnuCloseAll
            // 
            this.mnuCloseAll.Name = "mnuCloseAll";
            this.mnuCloseAll.Size = new System.Drawing.Size(151, 22);
            this.mnuCloseAll.Text = "C&lose All";
            this.mnuCloseAll.Click += new System.EventHandler(this.mnuCloseAll_Click);
            // 
            // mnuArrangeIcons
            // 
            this.mnuArrangeIcons.Name = "mnuArrangeIcons";
            this.mnuArrangeIcons.Size = new System.Drawing.Size(151, 22);
            this.mnuArrangeIcons.Text = "&Arrange Icons";
            this.mnuArrangeIcons.Click += new System.EventHandler(this.mnuArrangeIcons_Click);
            // 
            // stsBar
            // 
            this.stsBar.Location = new System.Drawing.Point(0, 431);
            this.stsBar.Name = "stsBar";
            this.stsBar.Size = new System.Drawing.Size(632, 22);
            this.stsBar.TabIndex = 2;
            this.stsBar.Text = "StatusStrip";
            // 
            // sfdConnection
            // 
            this.sfdConnection.DefaultExt = "ttl";
            this.sfdConnection.OverwritePrompt = false;
            this.sfdConnection.Title = "Save Connection";
            // 
            // ofdConnection
            // 
            this.ofdConnection.Title = "Open Connection";
            // 
            // fclsManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 453);
            this.Controls.Add(this.stsBar);
            this.Controls.Add(this.mnuStrip);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mnuStrip;
            this.Name = "fclsManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "dotNetRDF Store Manager";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.fclsManager_Load);
            this.mnuStrip.ResumeLayout(false);
            this.mnuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private System.Windows.Forms.MenuStrip mnuStrip;
        private System.Windows.Forms.StatusStrip stsBar;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuTileHorizontal;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuWindows;
        private System.Windows.Forms.ToolStripMenuItem mnuCascade;
        private System.Windows.Forms.ToolStripMenuItem mnuTileVertical;
        private System.Windows.Forms.ToolStripMenuItem mnuCloseAll;
        private System.Windows.Forms.ToolStripMenuItem mnuArrangeIcons;
        private System.Windows.Forms.ToolStripMenuItem mnuNewSQLStoreManager;
        private System.Windows.Forms.ToolStripMenuItem mnuNewGenericStoreManager;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveConnection;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenConnection;
        private System.Windows.Forms.SaveFileDialog sfdConnection;
        private System.Windows.Forms.OpenFileDialog ofdConnection;
        private System.Windows.Forms.ToolStripMenuItem mnuRecentConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuClearRecentConnections;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator3;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mnuFavouriteConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuClearFavouriteConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuAddFavourite;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuUseUtf8Bom;
    }
}



