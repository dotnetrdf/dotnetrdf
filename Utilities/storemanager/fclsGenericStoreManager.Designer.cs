namespace dotNetRDFStore
{
    partial class fclsGenericStoreManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fclsGenericStoreManager));
            this.tabFunctions = new System.Windows.Forms.TabControl();
            this.tabGraphs = new System.Windows.Forms.TabPage();
            this.lblGraphListUnavailable = new System.Windows.Forms.Label();
            this.btnGraphRefresh = new System.Windows.Forms.Button();
            this.lvwGraphs = new System.Windows.Forms.ListView();
            this.colGraphURI = new System.Windows.Forms.ColumnHeader();
            this.tabSparqlQuery = new System.Windows.Forms.TabPage();
            this.btnLoadQuery = new System.Windows.Forms.Button();
            this.btnSaveQuery = new System.Windows.Forms.Button();
            this.btnSparqlQuery = new System.Windows.Forms.Button();
            this.cboSPARQLResultFormat = new System.Windows.Forms.ComboBox();
            this.lblResultsFormat = new System.Windows.Forms.Label();
            this.txtSparqlQuery = new System.Windows.Forms.TextBox();
            this.lblQueryIntro = new System.Windows.Forms.Label();
            this.tabSparqlUpdate = new System.Windows.Forms.TabPage();
            this.lblUpdateMode = new System.Windows.Forms.Label();
            this.btnSparqlUpdate = new System.Windows.Forms.Button();
            this.txtSparqlUpdate = new System.Windows.Forms.TextBox();
            this.lblUpdateIntro = new System.Windows.Forms.Label();
            this.tabImportExport = new System.Windows.Forms.TabPage();
            this.grpImport = new System.Windows.Forms.GroupBox();
            this.btnImportUri = new System.Windows.Forms.Button();
            this.txtImportUri = new System.Windows.Forms.TextBox();
            this.lblUri = new System.Windows.Forms.Label();
            this.btnImportFile = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtImportFile = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.lblImport = new System.Windows.Forms.Label();
            this.ofdImport = new System.Windows.Forms.OpenFileDialog();
            this.stsStatus = new System.Windows.Forms.StatusStrip();
            this.stsCurrent = new System.Windows.Forms.ToolStripStatusLabel();
            this.timStartup = new System.Windows.Forms.Timer(this.components);
            this.ofdQuery = new System.Windows.Forms.OpenFileDialog();
            this.sfdQuery = new System.Windows.Forms.SaveFileDialog();
            this.tabFunctions.SuspendLayout();
            this.tabGraphs.SuspendLayout();
            this.tabSparqlQuery.SuspendLayout();
            this.tabSparqlUpdate.SuspendLayout();
            this.tabImportExport.SuspendLayout();
            this.grpImport.SuspendLayout();
            this.stsStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabFunctions
            // 
            this.tabFunctions.Controls.Add(this.tabGraphs);
            this.tabFunctions.Controls.Add(this.tabSparqlQuery);
            this.tabFunctions.Controls.Add(this.tabSparqlUpdate);
            this.tabFunctions.Controls.Add(this.tabImportExport);
            this.tabFunctions.Location = new System.Drawing.Point(12, 12);
            this.tabFunctions.Name = "tabFunctions";
            this.tabFunctions.SelectedIndex = 0;
            this.tabFunctions.Size = new System.Drawing.Size(522, 300);
            this.tabFunctions.TabIndex = 0;
            // 
            // tabGraphs
            // 
            this.tabGraphs.Controls.Add(this.lblGraphListUnavailable);
            this.tabGraphs.Controls.Add(this.btnGraphRefresh);
            this.tabGraphs.Controls.Add(this.lvwGraphs);
            this.tabGraphs.Location = new System.Drawing.Point(4, 22);
            this.tabGraphs.Name = "tabGraphs";
            this.tabGraphs.Padding = new System.Windows.Forms.Padding(3);
            this.tabGraphs.Size = new System.Drawing.Size(514, 274);
            this.tabGraphs.TabIndex = 0;
            this.tabGraphs.Text = "Graphs";
            this.tabGraphs.UseVisualStyleBackColor = true;
            // 
            // lblGraphListUnavailable
            // 
            this.lblGraphListUnavailable.AutoSize = true;
            this.lblGraphListUnavailable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGraphListUnavailable.Location = new System.Drawing.Point(26, 112);
            this.lblGraphListUnavailable.Name = "lblGraphListUnavailable";
            this.lblGraphListUnavailable.Size = new System.Drawing.Size(463, 13);
            this.lblGraphListUnavailable.TabIndex = 6;
            this.lblGraphListUnavailable.Text = "Unable to list Graphs since your selected Store does not support SPARQL Query";
            this.lblGraphListUnavailable.Visible = false;
            // 
            // btnGraphRefresh
            // 
            this.btnGraphRefresh.Enabled = false;
            this.btnGraphRefresh.Location = new System.Drawing.Point(219, 242);
            this.btnGraphRefresh.Name = "btnGraphRefresh";
            this.btnGraphRefresh.Size = new System.Drawing.Size(76, 23);
            this.btnGraphRefresh.TabIndex = 5;
            this.btnGraphRefresh.Text = "&Refresh";
            this.btnGraphRefresh.UseVisualStyleBackColor = true;
            this.btnGraphRefresh.Click += new System.EventHandler(this.btnGraphRefresh_Click);
            // 
            // lvwGraphs
            // 
            this.lvwGraphs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGraphURI});
            this.lvwGraphs.FullRowSelect = true;
            this.lvwGraphs.GridLines = true;
            this.lvwGraphs.Location = new System.Drawing.Point(6, 6);
            this.lvwGraphs.Name = "lvwGraphs";
            this.lvwGraphs.Size = new System.Drawing.Size(502, 230);
            this.lvwGraphs.TabIndex = 4;
            this.lvwGraphs.UseCompatibleStateImageBehavior = false;
            this.lvwGraphs.View = System.Windows.Forms.View.Details;
            this.lvwGraphs.DoubleClick += new System.EventHandler(this.lvwGraphs_DoubleClick);
            // 
            // colGraphURI
            // 
            this.colGraphURI.Text = "Graph URI";
            this.colGraphURI.Width = 490;
            // 
            // tabSparqlQuery
            // 
            this.tabSparqlQuery.Controls.Add(this.btnLoadQuery);
            this.tabSparqlQuery.Controls.Add(this.btnSaveQuery);
            this.tabSparqlQuery.Controls.Add(this.btnSparqlQuery);
            this.tabSparqlQuery.Controls.Add(this.cboSPARQLResultFormat);
            this.tabSparqlQuery.Controls.Add(this.lblResultsFormat);
            this.tabSparqlQuery.Controls.Add(this.txtSparqlQuery);
            this.tabSparqlQuery.Controls.Add(this.lblQueryIntro);
            this.tabSparqlQuery.Location = new System.Drawing.Point(4, 22);
            this.tabSparqlQuery.Name = "tabSparqlQuery";
            this.tabSparqlQuery.Padding = new System.Windows.Forms.Padding(3);
            this.tabSparqlQuery.Size = new System.Drawing.Size(514, 274);
            this.tabSparqlQuery.TabIndex = 1;
            this.tabSparqlQuery.Text = "SPARQL Query";
            this.tabSparqlQuery.UseVisualStyleBackColor = true;
            // 
            // btnLoadQuery
            // 
            this.btnLoadQuery.Location = new System.Drawing.Point(220, 244);
            this.btnLoadQuery.Name = "btnLoadQuery";
            this.btnLoadQuery.Size = new System.Drawing.Size(75, 23);
            this.btnLoadQuery.TabIndex = 6;
            this.btnLoadQuery.Text = "Load Query";
            this.btnLoadQuery.UseVisualStyleBackColor = true;
            this.btnLoadQuery.Click += new System.EventHandler(this.btnLoadQuery_Click);
            // 
            // btnSaveQuery
            // 
            this.btnSaveQuery.Location = new System.Drawing.Point(139, 244);
            this.btnSaveQuery.Name = "btnSaveQuery";
            this.btnSaveQuery.Size = new System.Drawing.Size(75, 23);
            this.btnSaveQuery.TabIndex = 5;
            this.btnSaveQuery.Text = "Save Query";
            this.btnSaveQuery.UseVisualStyleBackColor = true;
            this.btnSaveQuery.Click += new System.EventHandler(this.btnSaveQuery_Click);
            // 
            // btnSparqlQuery
            // 
            this.btnSparqlQuery.Location = new System.Drawing.Point(301, 244);
            this.btnSparqlQuery.Name = "btnSparqlQuery";
            this.btnSparqlQuery.Size = new System.Drawing.Size(75, 23);
            this.btnSparqlQuery.TabIndex = 4;
            this.btnSparqlQuery.Text = "Run Query";
            this.btnSparqlQuery.UseVisualStyleBackColor = true;
            this.btnSparqlQuery.Click += new System.EventHandler(this.btnSparqlQuery_Click);
            // 
            // cboSPARQLResultFormat
            // 
            this.cboSPARQLResultFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSPARQLResultFormat.FormattingEnabled = true;
            this.cboSPARQLResultFormat.Items.AddRange(new object[] {
            "HTML",
            "XML Results Format",
            "JSON Results Format"});
            this.cboSPARQLResultFormat.Location = new System.Drawing.Point(159, 217);
            this.cboSPARQLResultFormat.Name = "cboSPARQLResultFormat";
            this.cboSPARQLResultFormat.Size = new System.Drawing.Size(135, 21);
            this.cboSPARQLResultFormat.TabIndex = 3;
            // 
            // lblResultsFormat
            // 
            this.lblResultsFormat.AutoSize = true;
            this.lblResultsFormat.Location = new System.Drawing.Point(3, 220);
            this.lblResultsFormat.Name = "lblResultsFormat";
            this.lblResultsFormat.Size = new System.Drawing.Size(150, 13);
            this.lblResultsFormat.TabIndex = 2;
            this.lblResultsFormat.Text = "SELECT/ASK Results Format:";
            // 
            // txtSparqlQuery
            // 
            this.txtSparqlQuery.Location = new System.Drawing.Point(6, 69);
            this.txtSparqlQuery.Multiline = true;
            this.txtSparqlQuery.Name = "txtSparqlQuery";
            this.txtSparqlQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSparqlQuery.Size = new System.Drawing.Size(502, 139);
            this.txtSparqlQuery.TabIndex = 1;
            this.txtSparqlQuery.Text = "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\r\nPREFIX rdfs: <http://w" +
                "ww.w3.org/2000/01/rdf-schema#>\r\nPREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\r" +
                "\n";
            // 
            // lblQueryIntro
            // 
            this.lblQueryIntro.Location = new System.Drawing.Point(3, 13);
            this.lblQueryIntro.Name = "lblQueryIntro";
            this.lblQueryIntro.Size = new System.Drawing.Size(505, 61);
            this.lblQueryIntro.TabIndex = 0;
            this.lblQueryIntro.Text = resources.GetString("lblQueryIntro.Text");
            // 
            // tabSparqlUpdate
            // 
            this.tabSparqlUpdate.Controls.Add(this.lblUpdateMode);
            this.tabSparqlUpdate.Controls.Add(this.btnSparqlUpdate);
            this.tabSparqlUpdate.Controls.Add(this.txtSparqlUpdate);
            this.tabSparqlUpdate.Controls.Add(this.lblUpdateIntro);
            this.tabSparqlUpdate.Location = new System.Drawing.Point(4, 22);
            this.tabSparqlUpdate.Name = "tabSparqlUpdate";
            this.tabSparqlUpdate.Size = new System.Drawing.Size(514, 274);
            this.tabSparqlUpdate.TabIndex = 3;
            this.tabSparqlUpdate.Text = "SPARQL Update";
            this.tabSparqlUpdate.UseVisualStyleBackColor = true;
            // 
            // lblUpdateMode
            // 
            this.lblUpdateMode.AutoSize = true;
            this.lblUpdateMode.Location = new System.Drawing.Point(3, 234);
            this.lblUpdateMode.Name = "lblUpdateMode";
            this.lblUpdateMode.Size = new System.Drawing.Size(75, 13);
            this.lblUpdateMode.TabIndex = 4;
            this.lblUpdateMode.Text = "Update Mode:";
            // 
            // btnSparqlUpdate
            // 
            this.btnSparqlUpdate.Location = new System.Drawing.Point(220, 237);
            this.btnSparqlUpdate.Name = "btnSparqlUpdate";
            this.btnSparqlUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnSparqlUpdate.TabIndex = 3;
            this.btnSparqlUpdate.Text = "Run Update";
            this.btnSparqlUpdate.UseVisualStyleBackColor = true;
            this.btnSparqlUpdate.Click += new System.EventHandler(this.btnSparqlUpdate_Click);
            // 
            // txtSparqlUpdate
            // 
            this.txtSparqlUpdate.Location = new System.Drawing.Point(6, 61);
            this.txtSparqlUpdate.Multiline = true;
            this.txtSparqlUpdate.Name = "txtSparqlUpdate";
            this.txtSparqlUpdate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSparqlUpdate.Size = new System.Drawing.Size(502, 170);
            this.txtSparqlUpdate.TabIndex = 2;
            this.txtSparqlUpdate.Text = "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\r\nPREFIX rdfs: <http://w" +
                "ww.w3.org/2000/01/rdf-schema#>\r\nPREFIX xsd: <http://www.w3.org/2001/XMLSchema#>\r" +
                "\n";
            // 
            // lblUpdateIntro
            // 
            this.lblUpdateIntro.Location = new System.Drawing.Point(3, 12);
            this.lblUpdateIntro.Name = "lblUpdateIntro";
            this.lblUpdateIntro.Size = new System.Drawing.Size(508, 46);
            this.lblUpdateIntro.TabIndex = 0;
            this.lblUpdateIntro.Text = resources.GetString("lblUpdateIntro.Text");
            // 
            // tabImportExport
            // 
            this.tabImportExport.Controls.Add(this.grpImport);
            this.tabImportExport.Location = new System.Drawing.Point(4, 22);
            this.tabImportExport.Name = "tabImportExport";
            this.tabImportExport.Size = new System.Drawing.Size(514, 274);
            this.tabImportExport.TabIndex = 2;
            this.tabImportExport.Text = "Import/Export";
            this.tabImportExport.UseVisualStyleBackColor = true;
            // 
            // grpImport
            // 
            this.grpImport.Controls.Add(this.btnImportUri);
            this.grpImport.Controls.Add(this.txtImportUri);
            this.grpImport.Controls.Add(this.lblUri);
            this.grpImport.Controls.Add(this.btnImportFile);
            this.grpImport.Controls.Add(this.btnBrowse);
            this.grpImport.Controls.Add(this.txtImportFile);
            this.grpImport.Controls.Add(this.lblFile);
            this.grpImport.Controls.Add(this.lblImport);
            this.grpImport.Location = new System.Drawing.Point(3, 3);
            this.grpImport.Name = "grpImport";
            this.grpImport.Size = new System.Drawing.Size(508, 139);
            this.grpImport.TabIndex = 0;
            this.grpImport.TabStop = false;
            this.grpImport.Text = "Import";
            // 
            // btnImportUri
            // 
            this.btnImportUri.Location = new System.Drawing.Point(427, 99);
            this.btnImportUri.Name = "btnImportUri";
            this.btnImportUri.Size = new System.Drawing.Size(75, 23);
            this.btnImportUri.TabIndex = 7;
            this.btnImportUri.Text = "Import URI";
            this.btnImportUri.UseVisualStyleBackColor = true;
            this.btnImportUri.Click += new System.EventHandler(this.btnImportUri_Click);
            // 
            // txtImportUri
            // 
            this.txtImportUri.Location = new System.Drawing.Point(38, 101);
            this.txtImportUri.Name = "txtImportUri";
            this.txtImportUri.Size = new System.Drawing.Size(383, 20);
            this.txtImportUri.TabIndex = 6;
            // 
            // lblUri
            // 
            this.lblUri.AutoSize = true;
            this.lblUri.Location = new System.Drawing.Point(6, 104);
            this.lblUri.Name = "lblUri";
            this.lblUri.Size = new System.Drawing.Size(23, 13);
            this.lblUri.TabIndex = 5;
            this.lblUri.Text = "Uri:";
            // 
            // btnImportFile
            // 
            this.btnImportFile.Location = new System.Drawing.Point(427, 73);
            this.btnImportFile.Name = "btnImportFile";
            this.btnImportFile.Size = new System.Drawing.Size(75, 23);
            this.btnImportFile.TabIndex = 4;
            this.btnImportFile.Text = "Import";
            this.btnImportFile.UseVisualStyleBackColor = true;
            this.btnImportFile.Click += new System.EventHandler(this.btnImportFile_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(346, 73);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtImportFile
            // 
            this.txtImportFile.Location = new System.Drawing.Point(38, 75);
            this.txtImportFile.Name = "txtImportFile";
            this.txtImportFile.Size = new System.Drawing.Size(302, 20);
            this.txtImportFile.TabIndex = 2;
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(6, 78);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(26, 13);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "File:";
            // 
            // lblImport
            // 
            this.lblImport.Location = new System.Drawing.Point(6, 16);
            this.lblImport.Name = "lblImport";
            this.lblImport.Size = new System.Drawing.Size(502, 62);
            this.lblImport.TabIndex = 0;
            this.lblImport.Text = resources.GetString("lblImport.Text");
            // 
            // ofdImport
            // 
            this.ofdImport.FileName = "example.rdf";
            this.ofdImport.Title = "Import from File";
            // 
            // stsStatus
            // 
            this.stsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stsCurrent});
            this.stsStatus.Location = new System.Drawing.Point(0, 320);
            this.stsStatus.Name = "stsStatus";
            this.stsStatus.Size = new System.Drawing.Size(546, 22);
            this.stsStatus.SizingGrip = false;
            this.stsStatus.TabIndex = 1;
            // 
            // stsCurrent
            // 
            this.stsCurrent.Name = "stsCurrent";
            this.stsCurrent.Size = new System.Drawing.Size(217, 17);
            this.stsCurrent.Text = "Waiting for the Store to become ready...";
            // 
            // timStartup
            // 
            this.timStartup.Enabled = true;
            this.timStartup.Interval = 250;
            this.timStartup.Tick += new System.EventHandler(this.timStartup_Tick);
            // 
            // ofdQuery
            // 
            this.ofdQuery.DefaultExt = "rq";
            this.ofdQuery.FileName = "ofdQuery";
            this.ofdQuery.Filter = "SPARQL Query Files|*.rq|All Files|*.*";
            this.ofdQuery.Title = "Open SPARQL Query";
            // 
            // sfdQuery
            // 
            this.sfdQuery.FileName = "query.rq";
            this.sfdQuery.Title = "Load SPARQL Query";
            // 
            // fclsGenericStoreManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 342);
            this.Controls.Add(this.stsStatus);
            this.Controls.Add(this.tabFunctions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "fclsGenericStoreManager";
            this.Text = "fclsGenericStoreManager";
            this.Load += new System.EventHandler(this.fclsGenericStoreManager_Load);
            this.tabFunctions.ResumeLayout(false);
            this.tabGraphs.ResumeLayout(false);
            this.tabGraphs.PerformLayout();
            this.tabSparqlQuery.ResumeLayout(false);
            this.tabSparqlQuery.PerformLayout();
            this.tabSparqlUpdate.ResumeLayout(false);
            this.tabSparqlUpdate.PerformLayout();
            this.tabImportExport.ResumeLayout(false);
            this.grpImport.ResumeLayout(false);
            this.grpImport.PerformLayout();
            this.stsStatus.ResumeLayout(false);
            this.stsStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabFunctions;
        private System.Windows.Forms.TabPage tabGraphs;
        private System.Windows.Forms.TabPage tabSparqlQuery;
        private System.Windows.Forms.TabPage tabImportExport;
        private System.Windows.Forms.Button btnGraphRefresh;
        private System.Windows.Forms.ListView lvwGraphs;
        private System.Windows.Forms.ColumnHeader colGraphURI;
        private System.Windows.Forms.Label lblGraphListUnavailable;
        private System.Windows.Forms.Label lblQueryIntro;
        private System.Windows.Forms.TextBox txtSparqlQuery;
        private System.Windows.Forms.Label lblResultsFormat;
        private System.Windows.Forms.ComboBox cboSPARQLResultFormat;
        private System.Windows.Forms.Button btnSparqlQuery;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.Label lblImport;
        private System.Windows.Forms.Button btnImportFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtImportFile;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.OpenFileDialog ofdImport;
        private System.Windows.Forms.StatusStrip stsStatus;
        private System.Windows.Forms.ToolStripStatusLabel stsCurrent;
        private System.Windows.Forms.Timer timStartup;
        private System.Windows.Forms.Button btnLoadQuery;
        private System.Windows.Forms.Button btnSaveQuery;
        private System.Windows.Forms.OpenFileDialog ofdQuery;
        private System.Windows.Forms.SaveFileDialog sfdQuery;
        private System.Windows.Forms.Button btnImportUri;
        private System.Windows.Forms.TextBox txtImportUri;
        private System.Windows.Forms.Label lblUri;
        private System.Windows.Forms.TabPage tabSparqlUpdate;
        private System.Windows.Forms.Label lblUpdateIntro;
        private System.Windows.Forms.TextBox txtSparqlUpdate;
        private System.Windows.Forms.Button btnSparqlUpdate;
        private System.Windows.Forms.Label lblUpdateMode;
    }
}