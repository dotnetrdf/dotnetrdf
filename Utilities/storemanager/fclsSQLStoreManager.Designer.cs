namespace VDS.RDF.Utilities.StoreManager
{
    partial class fclsSQLStoreManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fclsSQLStoreManager));
            this.tabFunctions = new System.Windows.Forms.TabControl();
            this.tabConnection = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblDBPort = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.lblDBName = new System.Windows.Forms.Label();
            this.txtDBServer = new System.Windows.Forms.TextBox();
            this.lblDBServer = new System.Windows.Forms.Label();
            this.cboDBFormat = new System.Windows.Forms.ComboBox();
            this.lblDBFormat = new System.Windows.Forms.Label();
            this.tabInfo = new System.Windows.Forms.TabPage();
            this.btnInfoRefresh = new System.Windows.Forms.Button();
            this.lblConnectionRequired = new System.Windows.Forms.Label();
            this.lvwDBInfo = new System.Windows.Forms.ListView();
            this.colProperty = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.tabTools = new System.Windows.Forms.TabPage();
            this.lblConnectionRequired2 = new System.Windows.Forms.Label();
            this.panTools = new System.Windows.Forms.Panel();
            this.grpCompact = new System.Windows.Forms.GroupBox();
            this.prgCompact = new System.Windows.Forms.ProgressBar();
            this.btnCompactStore = new System.Windows.Forms.Button();
            this.chkCompactFull = new System.Windows.Forms.CheckBox();
            this.lblCompact = new System.Windows.Forms.Label();
            this.grpUpgrade = new System.Windows.Forms.GroupBox();
            this.prgUpgrade = new System.Windows.Forms.ProgressBar();
            this.btnUpgradeStore = new System.Windows.Forms.Button();
            this.lblUpgrade = new System.Windows.Forms.Label();
            this.grpSetup = new System.Windows.Forms.GroupBox();
            this.btnDeleteStore = new System.Windows.Forms.Button();
            this.btnCreateStore = new System.Windows.Forms.Button();
            this.lblSetup = new System.Windows.Forms.Label();
            this.tabGraphs = new System.Windows.Forms.TabPage();
            this.btnGraphRefresh = new System.Windows.Forms.Button();
            this.lblConnectionRequired3 = new System.Windows.Forms.Label();
            this.lvwGraphs = new System.Windows.Forms.ListView();
            this.colGraphURI = new System.Windows.Forms.ColumnHeader();
            this.colGraphTriples = new System.Windows.Forms.ColumnHeader();
            this.tabIO = new System.Windows.Forms.TabPage();
            this.grpExport = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.cboExportFormat = new System.Windows.Forms.ComboBox();
            this.lblExportToFile = new System.Windows.Forms.Label();
            this.grpImport = new System.Windows.Forms.GroupBox();
            this.btnImportFile = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtImportFile = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.lblImport = new System.Windows.Forms.Label();
            this.lblConnectionRequired4 = new System.Windows.Forms.Label();
            this.sfdExport = new System.Windows.Forms.SaveFileDialog();
            this.ofdImport = new System.Windows.Forms.OpenFileDialog();
            this.tabFunctions.SuspendLayout();
            this.tabConnection.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tabTools.SuspendLayout();
            this.panTools.SuspendLayout();
            this.grpCompact.SuspendLayout();
            this.grpUpgrade.SuspendLayout();
            this.grpSetup.SuspendLayout();
            this.tabGraphs.SuspendLayout();
            this.tabIO.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.grpImport.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabFunctions
            // 
            this.tabFunctions.Controls.Add(this.tabConnection);
            this.tabFunctions.Controls.Add(this.tabInfo);
            this.tabFunctions.Controls.Add(this.tabTools);
            this.tabFunctions.Controls.Add(this.tabGraphs);
            this.tabFunctions.Controls.Add(this.tabIO);
            this.tabFunctions.Location = new System.Drawing.Point(12, 12);
            this.tabFunctions.Name = "tabFunctions";
            this.tabFunctions.SelectedIndex = 0;
            this.tabFunctions.Size = new System.Drawing.Size(467, 360);
            this.tabFunctions.TabIndex = 0;
            // 
            // tabConnection
            // 
            this.tabConnection.Controls.Add(this.label2);
            this.tabConnection.Controls.Add(this.txtPort);
            this.tabConnection.Controls.Add(this.lblDBPort);
            this.tabConnection.Controls.Add(this.btnDisconnect);
            this.tabConnection.Controls.Add(this.btnConnect);
            this.tabConnection.Controls.Add(this.txtPassword);
            this.tabConnection.Controls.Add(this.lblPassword);
            this.tabConnection.Controls.Add(this.txtUsername);
            this.tabConnection.Controls.Add(this.lblUsername);
            this.tabConnection.Controls.Add(this.txtDBName);
            this.tabConnection.Controls.Add(this.lblDBName);
            this.tabConnection.Controls.Add(this.txtDBServer);
            this.tabConnection.Controls.Add(this.lblDBServer);
            this.tabConnection.Controls.Add(this.cboDBFormat);
            this.tabConnection.Controls.Add(this.lblDBFormat);
            this.tabConnection.Location = new System.Drawing.Point(4, 22);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnection.Size = new System.Drawing.Size(459, 334);
            this.tabConnection.TabIndex = 0;
            this.tabConnection.Text = "Database Connection";
            this.tabConnection.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(168, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(258, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Not required for Microsoft SQL Server/MySQL Server";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(102, 63);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(60, 20);
            this.txtPort.TabIndex = 5;
            // 
            // lblDBPort
            // 
            this.lblDBPort.AutoSize = true;
            this.lblDBPort.Location = new System.Drawing.Point(6, 66);
            this.lblDBPort.Name = "lblDBPort";
            this.lblDBPort.Size = new System.Drawing.Size(78, 13);
            this.lblDBPort.TabIndex = 4;
            this.lblDBPort.Text = "Database Port:";
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(232, 169);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 14;
            this.btnDisconnect.Text = "&Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(151, 169);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 13;
            this.btnConnect.Text = "&Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(102, 143);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(351, 20);
            this.txtPassword.TabIndex = 12;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(6, 146);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 11;
            this.lblPassword.Text = "Password:";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(102, 117);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(351, 20);
            this.txtUsername.TabIndex = 10;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(6, 120);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.TabIndex = 9;
            this.lblUsername.Text = "Username:";
            // 
            // txtDBName
            // 
            this.txtDBName.Location = new System.Drawing.Point(102, 89);
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Size = new System.Drawing.Size(351, 20);
            this.txtDBName.TabIndex = 8;
            // 
            // lblDBName
            // 
            this.lblDBName.AutoSize = true;
            this.lblDBName.Location = new System.Drawing.Point(6, 92);
            this.lblDBName.Name = "lblDBName";
            this.lblDBName.Size = new System.Drawing.Size(87, 13);
            this.lblDBName.TabIndex = 7;
            this.lblDBName.Text = "Database Name:";
            // 
            // txtDBServer
            // 
            this.txtDBServer.Location = new System.Drawing.Point(102, 37);
            this.txtDBServer.Name = "txtDBServer";
            this.txtDBServer.Size = new System.Drawing.Size(351, 20);
            this.txtDBServer.TabIndex = 3;
            this.txtDBServer.Text = "localhost";
            // 
            // lblDBServer
            // 
            this.lblDBServer.AutoSize = true;
            this.lblDBServer.Location = new System.Drawing.Point(6, 40);
            this.lblDBServer.Name = "lblDBServer";
            this.lblDBServer.Size = new System.Drawing.Size(90, 13);
            this.lblDBServer.TabIndex = 2;
            this.lblDBServer.Text = "Database Server:";
            // 
            // cboDBFormat
            // 
            this.cboDBFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDBFormat.FormattingEnabled = true;
            this.cboDBFormat.Items.AddRange(new object[] {
            "Microsoft SQL Server",
            "MySQL Server"});
            this.cboDBFormat.Location = new System.Drawing.Point(102, 11);
            this.cboDBFormat.Name = "cboDBFormat";
            this.cboDBFormat.Size = new System.Drawing.Size(351, 21);
            this.cboDBFormat.TabIndex = 1;
            // 
            // lblDBFormat
            // 
            this.lblDBFormat.AutoSize = true;
            this.lblDBFormat.Location = new System.Drawing.Point(6, 14);
            this.lblDBFormat.Name = "lblDBFormat";
            this.lblDBFormat.Size = new System.Drawing.Size(91, 13);
            this.lblDBFormat.TabIndex = 0;
            this.lblDBFormat.Text = "Database Format:";
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.btnInfoRefresh);
            this.tabInfo.Controls.Add(this.lblConnectionRequired);
            this.tabInfo.Controls.Add(this.lvwDBInfo);
            this.tabInfo.Location = new System.Drawing.Point(4, 22);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabInfo.Size = new System.Drawing.Size(459, 334);
            this.tabInfo.TabIndex = 1;
            this.tabInfo.Text = "Database Information";
            this.tabInfo.UseVisualStyleBackColor = true;
            // 
            // btnInfoRefresh
            // 
            this.btnInfoRefresh.Location = new System.Drawing.Point(192, 304);
            this.btnInfoRefresh.Name = "btnInfoRefresh";
            this.btnInfoRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnInfoRefresh.TabIndex = 2;
            this.btnInfoRefresh.Text = "Refresh";
            this.btnInfoRefresh.UseVisualStyleBackColor = true;
            this.btnInfoRefresh.Click += new System.EventHandler(this.btnInfoRefresh_Click);
            // 
            // lblConnectionRequired
            // 
            this.lblConnectionRequired.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionRequired.Location = new System.Drawing.Point(7, 148);
            this.lblConnectionRequired.Name = "lblConnectionRequired";
            this.lblConnectionRequired.Size = new System.Drawing.Size(445, 38);
            this.lblConnectionRequired.TabIndex = 0;
            this.lblConnectionRequired.Text = "You are not currently connected to any Database.  Please use the Database Connect" +
                "ion tab to connect to a Database.";
            this.lblConnectionRequired.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvwDBInfo
            // 
            this.lvwDBInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colProperty,
            this.colValue});
            this.lvwDBInfo.FullRowSelect = true;
            this.lvwDBInfo.GridLines = true;
            this.lvwDBInfo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvwDBInfo.Location = new System.Drawing.Point(3, 6);
            this.lvwDBInfo.Name = "lvwDBInfo";
            this.lvwDBInfo.Size = new System.Drawing.Size(451, 292);
            this.lvwDBInfo.TabIndex = 1;
            this.lvwDBInfo.UseCompatibleStateImageBehavior = false;
            this.lvwDBInfo.View = System.Windows.Forms.View.Details;
            this.lvwDBInfo.Visible = false;
            // 
            // colProperty
            // 
            this.colProperty.Text = "Database Property";
            this.colProperty.Width = 208;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 227;
            // 
            // tabTools
            // 
            this.tabTools.Controls.Add(this.lblConnectionRequired2);
            this.tabTools.Controls.Add(this.panTools);
            this.tabTools.Location = new System.Drawing.Point(4, 22);
            this.tabTools.Name = "tabTools";
            this.tabTools.Size = new System.Drawing.Size(459, 334);
            this.tabTools.TabIndex = 2;
            this.tabTools.Text = "Database Tools";
            this.tabTools.UseVisualStyleBackColor = true;
            // 
            // lblConnectionRequired2
            // 
            this.lblConnectionRequired2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionRequired2.Location = new System.Drawing.Point(7, 148);
            this.lblConnectionRequired2.Name = "lblConnectionRequired2";
            this.lblConnectionRequired2.Size = new System.Drawing.Size(445, 38);
            this.lblConnectionRequired2.TabIndex = 1;
            this.lblConnectionRequired2.Text = "You are not currently connected to any Database.  Please use the Database Connect" +
                "ion tab to connect to a Database.";
            this.lblConnectionRequired2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panTools
            // 
            this.panTools.Controls.Add(this.grpCompact);
            this.panTools.Controls.Add(this.grpUpgrade);
            this.panTools.Controls.Add(this.grpSetup);
            this.panTools.Location = new System.Drawing.Point(3, 3);
            this.panTools.Name = "panTools";
            this.panTools.Size = new System.Drawing.Size(453, 328);
            this.panTools.TabIndex = 2;
            this.panTools.Visible = false;
            // 
            // grpCompact
            // 
            this.grpCompact.Controls.Add(this.prgCompact);
            this.grpCompact.Controls.Add(this.btnCompactStore);
            this.grpCompact.Controls.Add(this.chkCompactFull);
            this.grpCompact.Controls.Add(this.lblCompact);
            this.grpCompact.Location = new System.Drawing.Point(7, 189);
            this.grpCompact.Name = "grpCompact";
            this.grpCompact.Size = new System.Drawing.Size(442, 136);
            this.grpCompact.TabIndex = 2;
            this.grpCompact.TabStop = false;
            this.grpCompact.Text = "Database Compaction";
            // 
            // prgCompact
            // 
            this.prgCompact.Location = new System.Drawing.Point(6, 78);
            this.prgCompact.Name = "prgCompact";
            this.prgCompact.Size = new System.Drawing.Size(427, 23);
            this.prgCompact.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgCompact.TabIndex = 3;
            // 
            // btnCompactStore
            // 
            this.btnCompactStore.Location = new System.Drawing.Point(150, 107);
            this.btnCompactStore.Name = "btnCompactStore";
            this.btnCompactStore.Size = new System.Drawing.Size(143, 23);
            this.btnCompactStore.TabIndex = 2;
            this.btnCompactStore.Text = "Compact dotNetRDF Store";
            this.btnCompactStore.UseVisualStyleBackColor = true;
            this.btnCompactStore.Click += new System.EventHandler(this.btnCompactStore_Click);
            // 
            // chkCompactFull
            // 
            this.chkCompactFull.AutoSize = true;
            this.chkCompactFull.Location = new System.Drawing.Point(9, 57);
            this.chkCompactFull.Name = "chkCompactFull";
            this.chkCompactFull.Size = new System.Drawing.Size(259, 17);
            this.chkCompactFull.TabIndex = 1;
            this.chkCompactFull.Text = "Remove unused Namespaces and Empty Graphs";
            this.chkCompactFull.UseVisualStyleBackColor = true;
            // 
            // lblCompact
            // 
            this.lblCompact.Location = new System.Drawing.Point(6, 16);
            this.lblCompact.Name = "lblCompact";
            this.lblCompact.Size = new System.Drawing.Size(430, 47);
            this.lblCompact.TabIndex = 0;
            this.lblCompact.Text = "Use this Tool to compact a dotNetRDF Store to remove unnecessary Nodes and Triple" +
                "s from the Store.  You can also opt to remove unused Namespaces and Empty Graphs" +
                " from the Store.";
            // 
            // grpUpgrade
            // 
            this.grpUpgrade.Controls.Add(this.prgUpgrade);
            this.grpUpgrade.Controls.Add(this.btnUpgradeStore);
            this.grpUpgrade.Controls.Add(this.lblUpgrade);
            this.grpUpgrade.Location = new System.Drawing.Point(7, 80);
            this.grpUpgrade.Name = "grpUpgrade";
            this.grpUpgrade.Size = new System.Drawing.Size(442, 103);
            this.grpUpgrade.TabIndex = 1;
            this.grpUpgrade.TabStop = false;
            this.grpUpgrade.Text = "Store Upgrade";
            // 
            // prgUpgrade
            // 
            this.prgUpgrade.Location = new System.Drawing.Point(6, 43);
            this.prgUpgrade.Name = "prgUpgrade";
            this.prgUpgrade.Size = new System.Drawing.Size(427, 23);
            this.prgUpgrade.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgUpgrade.TabIndex = 2;
            // 
            // btnUpgradeStore
            // 
            this.btnUpgradeStore.Location = new System.Drawing.Point(150, 72);
            this.btnUpgradeStore.Name = "btnUpgradeStore";
            this.btnUpgradeStore.Size = new System.Drawing.Size(143, 23);
            this.btnUpgradeStore.TabIndex = 1;
            this.btnUpgradeStore.Text = "Upgrade dotNetRDF Store";
            this.btnUpgradeStore.UseVisualStyleBackColor = true;
            this.btnUpgradeStore.Click += new System.EventHandler(this.btnUpgradeStore_Click);
            // 
            // lblUpgrade
            // 
            this.lblUpgrade.Location = new System.Drawing.Point(6, 16);
            this.lblUpgrade.Name = "lblUpgrade";
            this.lblUpgrade.Size = new System.Drawing.Size(430, 38);
            this.lblUpgrade.TabIndex = 0;
            this.lblUpgrade.Text = "Use this Tool if you have an existing dotNetRDF Store which is not in the latest " +
                "database format which you wish to Upgrade";
            // 
            // grpSetup
            // 
            this.grpSetup.Controls.Add(this.btnDeleteStore);
            this.grpSetup.Controls.Add(this.btnCreateStore);
            this.grpSetup.Controls.Add(this.lblSetup);
            this.grpSetup.Location = new System.Drawing.Point(7, 3);
            this.grpSetup.Name = "grpSetup";
            this.grpSetup.Size = new System.Drawing.Size(442, 71);
            this.grpSetup.TabIndex = 0;
            this.grpSetup.TabStop = false;
            this.grpSetup.Text = "Store Creation and Deletion";
            // 
            // btnDeleteStore
            // 
            this.btnDeleteStore.Location = new System.Drawing.Point(224, 43);
            this.btnDeleteStore.Name = "btnDeleteStore";
            this.btnDeleteStore.Size = new System.Drawing.Size(143, 23);
            this.btnDeleteStore.TabIndex = 2;
            this.btnDeleteStore.Text = "Delete dotNetRDF Store";
            this.btnDeleteStore.UseVisualStyleBackColor = true;
            this.btnDeleteStore.Click += new System.EventHandler(this.btnDeleteStore_Click);
            // 
            // btnCreateStore
            // 
            this.btnCreateStore.Location = new System.Drawing.Point(75, 43);
            this.btnCreateStore.Name = "btnCreateStore";
            this.btnCreateStore.Size = new System.Drawing.Size(143, 23);
            this.btnCreateStore.TabIndex = 0;
            this.btnCreateStore.Text = "Create dotNetRDF Store";
            this.btnCreateStore.UseVisualStyleBackColor = true;
            this.btnCreateStore.Click += new System.EventHandler(this.btnCreateStore_Click);
            // 
            // lblSetup
            // 
            this.lblSetup.Location = new System.Drawing.Point(6, 16);
            this.lblSetup.Name = "lblSetup";
            this.lblSetup.Size = new System.Drawing.Size(430, 34);
            this.lblSetup.TabIndex = 1;
            this.lblSetup.Text = "Use these Tools to Create/Delete the Tables necessary to turn a database into a d" +
                "otNetRDFStore";
            // 
            // tabGraphs
            // 
            this.tabGraphs.Controls.Add(this.btnGraphRefresh);
            this.tabGraphs.Controls.Add(this.lblConnectionRequired3);
            this.tabGraphs.Controls.Add(this.lvwGraphs);
            this.tabGraphs.Location = new System.Drawing.Point(4, 22);
            this.tabGraphs.Name = "tabGraphs";
            this.tabGraphs.Padding = new System.Windows.Forms.Padding(3);
            this.tabGraphs.Size = new System.Drawing.Size(459, 334);
            this.tabGraphs.TabIndex = 3;
            this.tabGraphs.Text = "Graphs";
            this.tabGraphs.UseVisualStyleBackColor = true;
            // 
            // btnGraphRefresh
            // 
            this.btnGraphRefresh.Enabled = false;
            this.btnGraphRefresh.Location = new System.Drawing.Point(192, 305);
            this.btnGraphRefresh.Name = "btnGraphRefresh";
            this.btnGraphRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnGraphRefresh.TabIndex = 3;
            this.btnGraphRefresh.Text = "&Refresh";
            this.btnGraphRefresh.UseVisualStyleBackColor = true;
            this.btnGraphRefresh.Click += new System.EventHandler(this.btnGraphRefresh_Click);
            // 
            // lblConnectionRequired3
            // 
            this.lblConnectionRequired3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionRequired3.Location = new System.Drawing.Point(7, 148);
            this.lblConnectionRequired3.Name = "lblConnectionRequired3";
            this.lblConnectionRequired3.Size = new System.Drawing.Size(445, 38);
            this.lblConnectionRequired3.TabIndex = 1;
            this.lblConnectionRequired3.Text = "You are not currently connected to any Database.  Please use the Database Connect" +
                "ion tab to connect to a Database.";
            this.lblConnectionRequired3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvwGraphs
            // 
            this.lvwGraphs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGraphURI,
            this.colGraphTriples});
            this.lvwGraphs.FullRowSelect = true;
            this.lvwGraphs.GridLines = true;
            this.lvwGraphs.Location = new System.Drawing.Point(10, 6);
            this.lvwGraphs.Name = "lvwGraphs";
            this.lvwGraphs.Size = new System.Drawing.Size(442, 294);
            this.lvwGraphs.TabIndex = 2;
            this.lvwGraphs.UseCompatibleStateImageBehavior = false;
            this.lvwGraphs.View = System.Windows.Forms.View.Details;
            this.lvwGraphs.Visible = false;
            this.lvwGraphs.DoubleClick += new System.EventHandler(this.lvwGraphs_DoubleClick);
            this.lvwGraphs.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvwGraphs_ColumnClick);
            // 
            // colGraphURI
            // 
            this.colGraphURI.Text = "Graph URI";
            this.colGraphURI.Width = 328;
            // 
            // colGraphTriples
            // 
            this.colGraphTriples.Text = "Total Triples";
            this.colGraphTriples.Width = 93;
            // 
            // tabIO
            // 
            this.tabIO.Controls.Add(this.grpExport);
            this.tabIO.Controls.Add(this.grpImport);
            this.tabIO.Controls.Add(this.lblConnectionRequired4);
            this.tabIO.Location = new System.Drawing.Point(4, 22);
            this.tabIO.Name = "tabIO";
            this.tabIO.Size = new System.Drawing.Size(459, 334);
            this.tabIO.TabIndex = 4;
            this.tabIO.Text = "Import/Export";
            this.tabIO.UseVisualStyleBackColor = true;
            // 
            // grpExport
            // 
            this.grpExport.Controls.Add(this.btnExport);
            this.grpExport.Controls.Add(this.cboExportFormat);
            this.grpExport.Controls.Add(this.lblExportToFile);
            this.grpExport.Location = new System.Drawing.Point(10, 165);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(442, 160);
            this.grpExport.TabIndex = 4;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(136, 59);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 21);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "&Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // cboExportFormat
            // 
            this.cboExportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboExportFormat.FormattingEnabled = true;
            this.cboExportFormat.Items.AddRange(new object[] {
            "TriG",
            "TriX",
            "NQuads"});
            this.cboExportFormat.Location = new System.Drawing.Point(9, 59);
            this.cboExportFormat.Name = "cboExportFormat";
            this.cboExportFormat.Size = new System.Drawing.Size(121, 21);
            this.cboExportFormat.TabIndex = 1;
            // 
            // lblExportToFile
            // 
            this.lblExportToFile.Location = new System.Drawing.Point(6, 21);
            this.lblExportToFile.Name = "lblExportToFile";
            this.lblExportToFile.Size = new System.Drawing.Size(425, 35);
            this.lblExportToFile.TabIndex = 0;
            this.lblExportToFile.Text = "You can export your Store in one of the supported serialization formats.  Current" +
                "ly TriG, TriX and NQuads are supported:";
            // 
            // grpImport
            // 
            this.grpImport.Controls.Add(this.btnImportFile);
            this.grpImport.Controls.Add(this.btnBrowse);
            this.grpImport.Controls.Add(this.txtImportFile);
            this.grpImport.Controls.Add(this.lblFile);
            this.grpImport.Controls.Add(this.lblImport);
            this.grpImport.Location = new System.Drawing.Point(10, 3);
            this.grpImport.Name = "grpImport";
            this.grpImport.Size = new System.Drawing.Size(442, 160);
            this.grpImport.TabIndex = 3;
            this.grpImport.TabStop = false;
            this.grpImport.Text = "Import";
            // 
            // btnImportFile
            // 
            this.btnImportFile.Location = new System.Drawing.Point(356, 119);
            this.btnImportFile.Name = "btnImportFile";
            this.btnImportFile.Size = new System.Drawing.Size(75, 23);
            this.btnImportFile.TabIndex = 9;
            this.btnImportFile.Text = "Import";
            this.btnImportFile.UseVisualStyleBackColor = true;
            this.btnImportFile.Click += new System.EventHandler(this.btnImportFile_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(356, 90);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 8;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtImportFile
            // 
            this.txtImportFile.Location = new System.Drawing.Point(38, 92);
            this.txtImportFile.Name = "txtImportFile";
            this.txtImportFile.Size = new System.Drawing.Size(312, 20);
            this.txtImportFile.TabIndex = 7;
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(6, 95);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(26, 13);
            this.lblFile.TabIndex = 6;
            this.lblFile.Text = "File:";
            // 
            // lblImport
            // 
            this.lblImport.Location = new System.Drawing.Point(6, 16);
            this.lblImport.Name = "lblImport";
            this.lblImport.Size = new System.Drawing.Size(425, 73);
            this.lblImport.TabIndex = 5;
            this.lblImport.Text = resources.GetString("lblImport.Text");
            // 
            // lblConnectionRequired4
            // 
            this.lblConnectionRequired4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionRequired4.Location = new System.Drawing.Point(7, 148);
            this.lblConnectionRequired4.Name = "lblConnectionRequired4";
            this.lblConnectionRequired4.Size = new System.Drawing.Size(445, 38);
            this.lblConnectionRequired4.TabIndex = 2;
            this.lblConnectionRequired4.Text = "You are not currently connected to any Database.  Please use the Database Connect" +
                "ion tab to connect to a Database.";
            this.lblConnectionRequired4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sfdExport
            // 
            this.sfdExport.DefaultExt = "trig";
            this.sfdExport.FileName = "export";
            this.sfdExport.Filter = "TriG files (*.trig)|*.trig|TriX files (*.xml)|*.xml|NQuads files (*.nq)|*.nq";
            this.sfdExport.Title = "Select file to export to";
            // 
            // ofdImport
            // 
            this.ofdImport.FileName = "example.rdf";
            this.ofdImport.Title = "Import from File";
            // 
            // fclsSQLStoreManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 384);
            this.Controls.Add(this.tabFunctions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "fclsSQLStoreManager";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Store - Disconnected";
            this.Load += new System.EventHandler(this.fclsStoreManager_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fclsStoreManager_FormClosing);
            this.tabFunctions.ResumeLayout(false);
            this.tabConnection.ResumeLayout(false);
            this.tabConnection.PerformLayout();
            this.tabInfo.ResumeLayout(false);
            this.tabTools.ResumeLayout(false);
            this.panTools.ResumeLayout(false);
            this.grpCompact.ResumeLayout(false);
            this.grpCompact.PerformLayout();
            this.grpUpgrade.ResumeLayout(false);
            this.grpSetup.ResumeLayout(false);
            this.tabGraphs.ResumeLayout(false);
            this.tabIO.ResumeLayout(false);
            this.grpExport.ResumeLayout(false);
            this.grpImport.ResumeLayout(false);
            this.grpImport.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabFunctions;
        private System.Windows.Forms.TabPage tabConnection;
        private System.Windows.Forms.TabPage tabInfo;
        private System.Windows.Forms.Label lblConnectionRequired;
        private System.Windows.Forms.ListView lvwDBInfo;
        private System.Windows.Forms.ColumnHeader colProperty;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.Label lblDBFormat;
        private System.Windows.Forms.ComboBox cboDBFormat;
        private System.Windows.Forms.Label lblDBServer;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtDBName;
        private System.Windows.Forms.Label lblDBName;
        private System.Windows.Forms.TextBox txtDBServer;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblDBPort;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabTools;
        private System.Windows.Forms.Label lblConnectionRequired2;
        private System.Windows.Forms.Panel panTools;
        private System.Windows.Forms.GroupBox grpSetup;
        private System.Windows.Forms.Label lblSetup;
        private System.Windows.Forms.Button btnCreateStore;
        private System.Windows.Forms.Button btnDeleteStore;
        private System.Windows.Forms.GroupBox grpUpgrade;
        private System.Windows.Forms.ProgressBar prgUpgrade;
        private System.Windows.Forms.Button btnUpgradeStore;
        private System.Windows.Forms.Label lblUpgrade;
        private System.Windows.Forms.TabPage tabGraphs;
        private System.Windows.Forms.Label lblConnectionRequired3;
        private System.Windows.Forms.ListView lvwGraphs;
        private System.Windows.Forms.ColumnHeader colGraphURI;
        private System.Windows.Forms.ColumnHeader colGraphTriples;
        private System.Windows.Forms.Button btnGraphRefresh;
        private System.Windows.Forms.GroupBox grpCompact;
        private System.Windows.Forms.Label lblCompact;
        private System.Windows.Forms.Button btnCompactStore;
        private System.Windows.Forms.CheckBox chkCompactFull;
        private System.Windows.Forms.ProgressBar prgCompact;
        private System.Windows.Forms.Button btnInfoRefresh;
        private System.Windows.Forms.TabPage tabIO;
        private System.Windows.Forms.Label lblConnectionRequired4;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Label lblExportToFile;
        private System.Windows.Forms.ComboBox cboExportFormat;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.SaveFileDialog sfdExport;
        private System.Windows.Forms.Button btnImportFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtImportFile;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Label lblImport;
        private System.Windows.Forms.OpenFileDialog ofdImport;
    }
}

