namespace VDS.RDF.Utilities.Sparql
{
    partial class fclsSparqlGui
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fclsSparqlGui));
            this.grpDataset = new System.Windows.Forms.GroupBox();
            this.btnClearDataset = new System.Windows.Forms.Button();
            this.btnImportUri = new System.Windows.Forms.Button();
            this.txtSourceUri = new System.Windows.Forms.TextBox();
            this.lblImportUri = new System.Windows.Forms.Label();
            this.btnImportFile = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtSourceFile = new System.Windows.Forms.TextBox();
            this.lblSourceFile = new System.Windows.Forms.Label();
            this.lblDataset = new System.Windows.Forms.Label();
            this.grpQuery = new System.Windows.Forms.GroupBox();
            this.btnExplain = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnViewLog = new System.Windows.Forms.Button();
            this.btnSaveQuery = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnInspect = new System.Windows.Forms.Button();
            this.btnQuery = new System.Windows.Forms.Button();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.ofdBrowse = new System.Windows.Forms.OpenFileDialog();
            this.grpQueryOptions = new System.Windows.Forms.GroupBox();
            this.chkLogExplanation = new System.Windows.Forms.CheckBox();
            this.chkUseUtf8Bom = new System.Windows.Forms.CheckBox();
            this.chkViewResultsInApp = new System.Windows.Forms.CheckBox();
            this.chkAlgebraOptimisation = new System.Windows.Forms.CheckBox();
            this.chkQueryOptimisation = new System.Windows.Forms.CheckBox();
            this.chkAllowUnknownFunctions = new System.Windows.Forms.CheckBox();
            this.chkWebDemand = new System.Windows.Forms.CheckBox();
            this.chkPartialResults = new System.Windows.Forms.CheckBox();
            this.lblMilliseconds = new System.Windows.Forms.Label();
            this.numTimeout = new System.Windows.Forms.NumericUpDown();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.grpQuerySyntax = new System.Windows.Forms.GroupBox();
            this.radSparqlExtended = new System.Windows.Forms.RadioButton();
            this.radSparql11 = new System.Windows.Forms.RadioButton();
            this.radSparql10 = new System.Windows.Forms.RadioButton();
            this.stsStatus = new System.Windows.Forms.StatusStrip();
            this.stsGraphs = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsTriples = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsLastQuery = new System.Windows.Forms.ToolStripStatusLabel();
            this.grpResultsFormat = new System.Windows.Forms.GroupBox();
            this.cboResultsFormat = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cboGraphFormat = new System.Windows.Forms.ComboBox();
            this.ofdQuery = new System.Windows.Forms.OpenFileDialog();
            this.sfdQuery = new System.Windows.Forms.SaveFileDialog();
            this.ttpTips = new System.Windows.Forms.ToolTip(this.components);
            this.chkFullTextIndexing = new System.Windows.Forms.CheckBox();
            this.grpQueryTools = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.grpDataset.SuspendLayout();
            this.grpQuery.SuspendLayout();
            this.grpQueryOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).BeginInit();
            this.grpQuerySyntax.SuspendLayout();
            this.stsStatus.SuspendLayout();
            this.grpResultsFormat.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpQueryTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDataset
            // 
            this.grpDataset.Controls.Add(this.btnClearDataset);
            this.grpDataset.Controls.Add(this.btnImportUri);
            this.grpDataset.Controls.Add(this.txtSourceUri);
            this.grpDataset.Controls.Add(this.lblImportUri);
            this.grpDataset.Controls.Add(this.btnImportFile);
            this.grpDataset.Controls.Add(this.btnBrowse);
            this.grpDataset.Controls.Add(this.txtSourceFile);
            this.grpDataset.Controls.Add(this.lblSourceFile);
            this.grpDataset.Controls.Add(this.lblDataset);
            this.grpDataset.Location = new System.Drawing.Point(12, 12);
            this.grpDataset.Name = "grpDataset";
            this.grpDataset.Size = new System.Drawing.Size(582, 130);
            this.grpDataset.TabIndex = 0;
            this.grpDataset.TabStop = false;
            this.grpDataset.Text = "Dataset Creation";
            // 
            // btnClearDataset
            // 
            this.btnClearDataset.Location = new System.Drawing.Point(252, 101);
            this.btnClearDataset.Name = "btnClearDataset";
            this.btnClearDataset.Size = new System.Drawing.Size(75, 23);
            this.btnClearDataset.TabIndex = 8;
            this.btnClearDataset.Text = "Clear";
            this.btnClearDataset.UseVisualStyleBackColor = true;
            this.btnClearDataset.Click += new System.EventHandler(this.btnClearDataset_Click);
            // 
            // btnImportUri
            // 
            this.btnImportUri.Location = new System.Drawing.Point(495, 75);
            this.btnImportUri.Name = "btnImportUri";
            this.btnImportUri.Size = new System.Drawing.Size(75, 23);
            this.btnImportUri.TabIndex = 7;
            this.btnImportUri.Text = "Import &URI";
            this.btnImportUri.UseVisualStyleBackColor = true;
            this.btnImportUri.Click += new System.EventHandler(this.btnImportUri_Click);
            // 
            // txtSourceUri
            // 
            this.txtSourceUri.Location = new System.Drawing.Point(118, 77);
            this.txtSourceUri.Name = "txtSourceUri";
            this.txtSourceUri.Size = new System.Drawing.Size(371, 20);
            this.txtSourceUri.TabIndex = 6;
            // 
            // lblImportUri
            // 
            this.lblImportUri.AutoSize = true;
            this.lblImportUri.Location = new System.Drawing.Point(6, 80);
            this.lblImportUri.Name = "lblImportUri";
            this.lblImportUri.Size = new System.Drawing.Size(109, 13);
            this.lblImportUri.TabIndex = 5;
            this.lblImportUri.Text = "Import RDF from URI:";
            // 
            // btnImportFile
            // 
            this.btnImportFile.Location = new System.Drawing.Point(495, 49);
            this.btnImportFile.Name = "btnImportFile";
            this.btnImportFile.Size = new System.Drawing.Size(75, 23);
            this.btnImportFile.TabIndex = 4;
            this.btnImportFile.Text = "Import &File";
            this.btnImportFile.UseVisualStyleBackColor = true;
            this.btnImportFile.Click += new System.EventHandler(this.btnImportFile_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(414, 49);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtSourceFile
            // 
            this.txtSourceFile.Location = new System.Drawing.Point(118, 51);
            this.txtSourceFile.Name = "txtSourceFile";
            this.txtSourceFile.Size = new System.Drawing.Size(290, 20);
            this.txtSourceFile.TabIndex = 2;
            // 
            // lblSourceFile
            // 
            this.lblSourceFile.AutoSize = true;
            this.lblSourceFile.Location = new System.Drawing.Point(6, 54);
            this.lblSourceFile.Name = "lblSourceFile";
            this.lblSourceFile.Size = new System.Drawing.Size(106, 13);
            this.lblSourceFile.TabIndex = 1;
            this.lblSourceFile.Text = "Import RDF from File:";
            // 
            // lblDataset
            // 
            this.lblDataset.Location = new System.Drawing.Point(6, 16);
            this.lblDataset.Name = "lblDataset";
            this.lblDataset.Size = new System.Drawing.Size(566, 28);
            this.lblDataset.TabIndex = 0;
            this.lblDataset.Text = "Use this section of the GUI to import RDF data from Files/URIs.  You can reset th" +
                "e Dataset to the empty dataset by clicking the Clear button";
            // 
            // grpQuery
            // 
            this.grpQuery.Controls.Add(this.btnQuery);
            this.grpQuery.Controls.Add(this.txtQuery);
            this.grpQuery.Location = new System.Drawing.Point(12, 148);
            this.grpQuery.Name = "grpQuery";
            this.grpQuery.Size = new System.Drawing.Size(582, 283);
            this.grpQuery.TabIndex = 1;
            this.grpQuery.TabStop = false;
            this.grpQuery.Text = "Query";
            // 
            // btnExplain
            // 
            this.btnExplain.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExplain.Location = new System.Drawing.Point(249, 48);
            this.btnExplain.Name = "btnExplain";
            this.btnExplain.Size = new System.Drawing.Size(129, 23);
            this.btnExplain.TabIndex = 5;
            this.btnExplain.Text = "&Explain";
            this.btnExplain.UseVisualStyleBackColor = true;
            this.btnExplain.Click += new System.EventHandler(this.btnExplain_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClearLog.Location = new System.Drawing.Point(250, 78);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(128, 23);
            this.btnClearLog.TabIndex = 8;
            this.btnClearLog.Text = "&Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnViewLog
            // 
            this.btnViewLog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnViewLog.Location = new System.Drawing.Point(104, 77);
            this.btnViewLog.Name = "btnViewLog";
            this.btnViewLog.Size = new System.Drawing.Size(128, 23);
            this.btnViewLog.TabIndex = 7;
            this.btnViewLog.Text = "View &Log";
            this.btnViewLog.UseVisualStyleBackColor = true;
            this.btnViewLog.Click += new System.EventHandler(this.btnViewLog_Click);
            // 
            // btnSaveQuery
            // 
            this.btnSaveQuery.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnSaveQuery.Location = new System.Drawing.Point(250, 19);
            this.btnSaveQuery.Name = "btnSaveQuery";
            this.btnSaveQuery.Size = new System.Drawing.Size(128, 23);
            this.btnSaveQuery.TabIndex = 2;
            this.btnSaveQuery.Text = "&Save Query";
            this.btnSaveQuery.UseVisualStyleBackColor = true;
            this.btnSaveQuery.Click += new System.EventHandler(this.btnSaveQuery_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnLoad.Location = new System.Drawing.Point(104, 19);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(128, 23);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "&Load Query";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnInspect
            // 
            this.btnInspect.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnInspect.Location = new System.Drawing.Point(104, 48);
            this.btnInspect.Name = "btnInspect";
            this.btnInspect.Size = new System.Drawing.Size(127, 23);
            this.btnInspect.TabIndex = 4;
            this.btnInspect.Text = "&Inspect";
            this.btnInspect.UseVisualStyleBackColor = true;
            this.btnInspect.Click += new System.EventHandler(this.btnInspect_Click);
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnQuery.Location = new System.Drawing.Point(236, 254);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(111, 23);
            this.btnQuery.TabIndex = 1;
            this.btnQuery.Text = "Run &Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // txtQuery
            // 
            this.txtQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.txtQuery.Location = new System.Drawing.Point(6, 19);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQuery.Size = new System.Drawing.Size(566, 229);
            this.txtQuery.TabIndex = 0;
            // 
            // ofdBrowse
            // 
            this.ofdBrowse.Title = "Import RDF from File";
            // 
            // grpQueryOptions
            // 
            this.grpQueryOptions.Controls.Add(this.chkFullTextIndexing);
            this.grpQueryOptions.Controls.Add(this.chkLogExplanation);
            this.grpQueryOptions.Controls.Add(this.chkUseUtf8Bom);
            this.grpQueryOptions.Controls.Add(this.chkViewResultsInApp);
            this.grpQueryOptions.Controls.Add(this.chkAlgebraOptimisation);
            this.grpQueryOptions.Controls.Add(this.chkQueryOptimisation);
            this.grpQueryOptions.Controls.Add(this.chkAllowUnknownFunctions);
            this.grpQueryOptions.Controls.Add(this.chkWebDemand);
            this.grpQueryOptions.Controls.Add(this.chkPartialResults);
            this.grpQueryOptions.Controls.Add(this.lblMilliseconds);
            this.grpQueryOptions.Controls.Add(this.numTimeout);
            this.grpQueryOptions.Controls.Add(this.lblTimeout);
            this.grpQueryOptions.Location = new System.Drawing.Point(600, 136);
            this.grpQueryOptions.Name = "grpQueryOptions";
            this.grpQueryOptions.Size = new System.Drawing.Size(384, 182);
            this.grpQueryOptions.TabIndex = 5;
            this.grpQueryOptions.TabStop = false;
            this.grpQueryOptions.Text = "Query Options";
            // 
            // chkLogExplanation
            // 
            this.chkLogExplanation.AutoSize = true;
            this.chkLogExplanation.Location = new System.Drawing.Point(189, 98);
            this.chkLogExplanation.Name = "chkLogExplanation";
            this.chkLogExplanation.Size = new System.Drawing.Size(138, 17);
            this.chkLogExplanation.TabIndex = 7;
            this.chkLogExplanation.Text = "Log Query Explanations";
            this.ttpTips.SetToolTip(this.chkLogExplanation, "Enable this option if you wish to have detailed query explanations logged to the " +
                    "log file, this may make queries significantly slower in some cases");
            this.chkLogExplanation.UseVisualStyleBackColor = true;
            this.chkLogExplanation.CheckedChanged += new System.EventHandler(this.chkLogExplanation_CheckedChanged);
            // 
            // chkUseUtf8Bom
            // 
            this.chkUseUtf8Bom.AutoSize = true;
            this.chkUseUtf8Bom.Location = new System.Drawing.Point(189, 40);
            this.chkUseUtf8Bom.Name = "chkUseUtf8Bom";
            this.chkUseUtf8Bom.Size = new System.Drawing.Size(155, 17);
            this.chkUseUtf8Bom.TabIndex = 3;
            this.chkUseUtf8Bom.Text = "Use BOM for UTF-8 Output";
            this.ttpTips.SetToolTip(this.chkUseUtf8Bom, "Disable this option if you plan to use data exported from this tool in non-Window" +
                    "s tools that may have an issue with the UTF-8 BOM");
            this.chkUseUtf8Bom.UseVisualStyleBackColor = true;
            this.chkUseUtf8Bom.CheckedChanged += new System.EventHandler(this.chkUseUtf8Bom_CheckedChanged);
            // 
            // chkViewResultsInApp
            // 
            this.chkViewResultsInApp.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkViewResultsInApp.Checked = true;
            this.chkViewResultsInApp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkViewResultsInApp.Location = new System.Drawing.Point(189, 63);
            this.chkViewResultsInApp.Name = "chkViewResultsInApp";
            this.chkViewResultsInApp.Size = new System.Drawing.Size(166, 39);
            this.chkViewResultsInApp.TabIndex = 5;
            this.chkViewResultsInApp.Text = "View Results and Graphs in Application";
            this.chkViewResultsInApp.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.ttpTips.SetToolTip(this.chkViewResultsInApp, resources.GetString("chkViewResultsInApp.ToolTip"));
            this.chkViewResultsInApp.UseVisualStyleBackColor = true;
            // 
            // chkAlgebraOptimisation
            // 
            this.chkAlgebraOptimisation.Checked = true;
            this.chkAlgebraOptimisation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAlgebraOptimisation.Location = new System.Drawing.Point(6, 150);
            this.chkAlgebraOptimisation.Name = "chkAlgebraOptimisation";
            this.chkAlgebraOptimisation.Size = new System.Drawing.Size(177, 31);
            this.chkAlgebraOptimisation.TabIndex = 10;
            this.chkAlgebraOptimisation.Text = "Optimise Query Algebra";
            this.ttpTips.SetToolTip(this.chkAlgebraOptimisation, "Determines whether the Engine will use Algebra optimisations to evaluate some que" +
                    "ries more efficiently");
            this.chkAlgebraOptimisation.UseVisualStyleBackColor = true;
            this.chkAlgebraOptimisation.CheckedChanged += new System.EventHandler(this.chkAlgebraOptimisation_CheckedChanged);
            // 
            // chkQueryOptimisation
            // 
            this.chkQueryOptimisation.Checked = true;
            this.chkQueryOptimisation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkQueryOptimisation.Location = new System.Drawing.Point(6, 120);
            this.chkQueryOptimisation.Name = "chkQueryOptimisation";
            this.chkQueryOptimisation.Size = new System.Drawing.Size(177, 31);
            this.chkQueryOptimisation.TabIndex = 8;
            this.chkQueryOptimisation.Text = "Optimise Queries";
            this.ttpTips.SetToolTip(this.chkQueryOptimisation, "Determines whether the Engine will optimise the ordering of Triple Patterns in yo" +
                    "ur Query (highly recommended)");
            this.chkQueryOptimisation.UseVisualStyleBackColor = true;
            this.chkQueryOptimisation.CheckedChanged += new System.EventHandler(this.chkQueryOptimisation_CheckedChanged);
            // 
            // chkAllowUnknownFunctions
            // 
            this.chkAllowUnknownFunctions.Checked = true;
            this.chkAllowUnknownFunctions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllowUnknownFunctions.Location = new System.Drawing.Point(6, 91);
            this.chkAllowUnknownFunctions.Name = "chkAllowUnknownFunctions";
            this.chkAllowUnknownFunctions.Size = new System.Drawing.Size(177, 31);
            this.chkAllowUnknownFunctions.TabIndex = 6;
            this.chkAllowUnknownFunctions.Text = "Permit Unknown Function URIs";
            this.ttpTips.SetToolTip(this.chkAllowUnknownFunctions, "Unknown Functions allow unrecognised extension Function URIs to be used but the e" +
                    "ngine will not evaluate these when evaluating queries");
            this.chkAllowUnknownFunctions.UseVisualStyleBackColor = true;
            this.chkAllowUnknownFunctions.CheckedChanged += new System.EventHandler(this.chkAllowUnknownFunctions_CheckedChanged);
            // 
            // chkWebDemand
            // 
            this.chkWebDemand.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkWebDemand.Location = new System.Drawing.Point(6, 63);
            this.chkWebDemand.Name = "chkWebDemand";
            this.chkWebDemand.Size = new System.Drawing.Size(177, 35);
            this.chkWebDemand.TabIndex = 4;
            this.chkWebDemand.Text = "Allow Graphs to be loaded \'on demand\' from the Web?";
            this.chkWebDemand.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkWebDemand.UseVisualStyleBackColor = true;
            this.chkWebDemand.CheckedChanged += new System.EventHandler(this.chkWebDemand_CheckedChanged);
            // 
            // chkPartialResults
            // 
            this.chkPartialResults.AutoSize = true;
            this.chkPartialResults.Location = new System.Drawing.Point(6, 40);
            this.chkPartialResults.Name = "chkPartialResults";
            this.chkPartialResults.Size = new System.Drawing.Size(149, 17);
            this.chkPartialResults.TabIndex = 2;
            this.chkPartialResults.Text = "Partial Results on Timeout";
            this.chkPartialResults.UseVisualStyleBackColor = true;
            // 
            // lblMilliseconds
            // 
            this.lblMilliseconds.AutoSize = true;
            this.lblMilliseconds.Location = new System.Drawing.Point(120, 44);
            this.lblMilliseconds.Name = "lblMilliseconds";
            this.lblMilliseconds.Size = new System.Drawing.Size(20, 13);
            this.lblMilliseconds.TabIndex = 3;
            this.lblMilliseconds.Text = "ms";
            // 
            // numTimeout
            // 
            this.numTimeout.Location = new System.Drawing.Point(54, 14);
            this.numTimeout.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numTimeout.Name = "numTimeout";
            this.numTimeout.Size = new System.Drawing.Size(60, 20);
            this.numTimeout.TabIndex = 1;
            this.numTimeout.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(3, 16);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(48, 13);
            this.lblTimeout.TabIndex = 0;
            this.lblTimeout.Text = "Timeout:";
            // 
            // grpQuerySyntax
            // 
            this.grpQuerySyntax.Controls.Add(this.radSparqlExtended);
            this.grpQuerySyntax.Controls.Add(this.radSparql11);
            this.grpQuerySyntax.Controls.Add(this.radSparql10);
            this.grpQuerySyntax.Location = new System.Drawing.Point(600, 18);
            this.grpQuerySyntax.Name = "grpQuerySyntax";
            this.grpQuerySyntax.Size = new System.Drawing.Size(189, 112);
            this.grpQuerySyntax.TabIndex = 2;
            this.grpQuerySyntax.TabStop = false;
            this.grpQuerySyntax.Text = "Syntax Options";
            // 
            // radSparqlExtended
            // 
            this.radSparqlExtended.AutoSize = true;
            this.radSparqlExtended.Location = new System.Drawing.Point(6, 65);
            this.radSparqlExtended.Name = "radSparqlExtended";
            this.radSparqlExtended.Size = new System.Drawing.Size(134, 17);
            this.radSparqlExtended.TabIndex = 2;
            this.radSparqlExtended.Text = "SPARQL 1.1 Extended";
            this.radSparqlExtended.UseVisualStyleBackColor = true;
            this.radSparqlExtended.CheckedChanged += new System.EventHandler(this.radSparqlExtended_CheckedChanged);
            // 
            // radSparql11
            // 
            this.radSparql11.AutoSize = true;
            this.radSparql11.Checked = true;
            this.radSparql11.Location = new System.Drawing.Point(6, 42);
            this.radSparql11.Name = "radSparql11";
            this.radSparql11.Size = new System.Drawing.Size(86, 17);
            this.radSparql11.TabIndex = 1;
            this.radSparql11.TabStop = true;
            this.radSparql11.Text = "SPARQL 1.1";
            this.radSparql11.UseVisualStyleBackColor = true;
            this.radSparql11.CheckedChanged += new System.EventHandler(this.radSparql11_CheckedChanged);
            // 
            // radSparql10
            // 
            this.radSparql10.AutoSize = true;
            this.radSparql10.Location = new System.Drawing.Point(6, 19);
            this.radSparql10.Name = "radSparql10";
            this.radSparql10.Size = new System.Drawing.Size(86, 17);
            this.radSparql10.TabIndex = 0;
            this.radSparql10.Text = "SPARQL 1.0";
            this.radSparql10.UseVisualStyleBackColor = true;
            this.radSparql10.CheckedChanged += new System.EventHandler(this.radSparql10_CheckedChanged);
            // 
            // stsStatus
            // 
            this.stsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stsGraphs,
            this.stsTriples,
            this.stsLastQuery});
            this.stsStatus.Location = new System.Drawing.Point(0, 439);
            this.stsStatus.Name = "stsStatus";
            this.stsStatus.Size = new System.Drawing.Size(994, 22);
            this.stsStatus.TabIndex = 7;
            this.stsStatus.Text = "statusStrip1";
            // 
            // stsGraphs
            // 
            this.stsGraphs.Name = "stsGraphs";
            this.stsGraphs.Size = new System.Drawing.Size(53, 17);
            this.stsGraphs.Text = "0 Graphs";
            // 
            // stsTriples
            // 
            this.stsTriples.Name = "stsTriples";
            this.stsTriples.Size = new System.Drawing.Size(51, 17);
            this.stsTriples.Text = "0 Triples";
            // 
            // stsLastQuery
            // 
            this.stsLastQuery.Name = "stsLastQuery";
            this.stsLastQuery.Size = new System.Drawing.Size(117, 17);
            this.stsLastQuery.Text = "Last Query took ? ms";
            // 
            // grpResultsFormat
            // 
            this.grpResultsFormat.Controls.Add(this.cboResultsFormat);
            this.grpResultsFormat.Location = new System.Drawing.Point(795, 18);
            this.grpResultsFormat.Name = "grpResultsFormat";
            this.grpResultsFormat.Size = new System.Drawing.Size(189, 53);
            this.grpResultsFormat.TabIndex = 3;
            this.grpResultsFormat.TabStop = false;
            this.grpResultsFormat.Text = "SPARQL Results Format";
            // 
            // cboResultsFormat
            // 
            this.cboResultsFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboResultsFormat.FormattingEnabled = true;
            this.cboResultsFormat.Items.AddRange(new object[] {
            "CSV",
            "Graph",
            "HTML",
            "HTML with CSS Stylesheet",
            "JSON",
            "TSV",
            "XML",
            "XML with XSLT Stylesheet"});
            this.cboResultsFormat.Location = new System.Drawing.Point(9, 19);
            this.cboResultsFormat.Name = "cboResultsFormat";
            this.cboResultsFormat.Size = new System.Drawing.Size(174, 21);
            this.cboResultsFormat.TabIndex = 0;
            this.cboResultsFormat.SelectedIndexChanged += new System.EventHandler(this.cboResultsFormat_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cboGraphFormat);
            this.groupBox1.Location = new System.Drawing.Point(795, 77);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(189, 53);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Graph Format";
            // 
            // cboGraphFormat
            // 
            this.cboGraphFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboGraphFormat.FormattingEnabled = true;
            this.cboGraphFormat.Items.AddRange(new object[] {
            "CSV",
            "HTML+RDFa",
            "Notation 3",
            "NTriples",
            "RDF/JSON",
            "RDF/XML",
            "Turtle",
            "TSV"});
            this.cboGraphFormat.Location = new System.Drawing.Point(9, 19);
            this.cboGraphFormat.Name = "cboGraphFormat";
            this.cboGraphFormat.Size = new System.Drawing.Size(174, 21);
            this.cboGraphFormat.TabIndex = 0;
            this.cboGraphFormat.SelectedIndexChanged += new System.EventHandler(this.cboGraphFormat_SelectedIndexChanged);
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
            this.sfdQuery.Filter = "SPARQL Query Files|*.rq|All Files|*.*";
            this.sfdQuery.Title = "Load SPARQL Query";
            // 
            // chkFullTextIndexing
            // 
            this.chkFullTextIndexing.AutoSize = true;
            this.chkFullTextIndexing.Location = new System.Drawing.Point(189, 127);
            this.chkFullTextIndexing.Name = "chkFullTextIndexing";
            this.chkFullTextIndexing.Size = new System.Drawing.Size(145, 17);
            this.chkFullTextIndexing.TabIndex = 9;
            this.chkFullTextIndexing.Text = "Enable Full Text Indexing";
            this.chkFullTextIndexing.UseVisualStyleBackColor = true;
            this.chkFullTextIndexing.CheckedChanged += new System.EventHandler(this.chkFullTextIndexing_CheckedChanged);
            // 
            // grpQueryTools
            // 
            this.grpQueryTools.Controls.Add(this.label3);
            this.grpQueryTools.Controls.Add(this.label2);
            this.grpQueryTools.Controls.Add(this.label1);
            this.grpQueryTools.Controls.Add(this.btnClearLog);
            this.grpQueryTools.Controls.Add(this.btnExplain);
            this.grpQueryTools.Controls.Add(this.btnViewLog);
            this.grpQueryTools.Controls.Add(this.btnLoad);
            this.grpQueryTools.Controls.Add(this.btnSaveQuery);
            this.grpQueryTools.Controls.Add(this.btnInspect);
            this.grpQueryTools.Location = new System.Drawing.Point(600, 324);
            this.grpQueryTools.Name = "grpQueryTools";
            this.grpQueryTools.Size = new System.Drawing.Size(384, 107);
            this.grpQueryTools.TabIndex = 6;
            this.grpQueryTools.TabStop = false;
            this.grpQueryTools.Text = "Query Tools";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Manage Queries:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Explain Queries:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Query Log:";
            // 
            // fclsSparqlGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(994, 461);
            this.Controls.Add(this.grpQueryOptions);
            this.Controls.Add(this.grpQueryTools);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpResultsFormat);
            this.Controls.Add(this.stsStatus);
            this.Controls.Add(this.grpQuerySyntax);
            this.Controls.Add(this.grpQuery);
            this.Controls.Add(this.grpDataset);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "fclsSparqlGui";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SPARQL GUI";
            this.Load += new System.EventHandler(this.fclsSparqlGui_Load);
            this.grpDataset.ResumeLayout(false);
            this.grpDataset.PerformLayout();
            this.grpQuery.ResumeLayout(false);
            this.grpQuery.PerformLayout();
            this.grpQueryOptions.ResumeLayout(false);
            this.grpQueryOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimeout)).EndInit();
            this.grpQuerySyntax.ResumeLayout(false);
            this.grpQuerySyntax.PerformLayout();
            this.stsStatus.ResumeLayout(false);
            this.stsStatus.PerformLayout();
            this.grpResultsFormat.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.grpQueryTools.ResumeLayout(false);
            this.grpQueryTools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDataset;
        private System.Windows.Forms.Button btnImportFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtSourceFile;
        private System.Windows.Forms.Label lblSourceFile;
        private System.Windows.Forms.Label lblDataset;
        private System.Windows.Forms.Button btnClearDataset;
        private System.Windows.Forms.Button btnImportUri;
        private System.Windows.Forms.TextBox txtSourceUri;
        private System.Windows.Forms.Label lblImportUri;
        private System.Windows.Forms.GroupBox grpQuery;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.OpenFileDialog ofdBrowse;
        private System.Windows.Forms.GroupBox grpQueryOptions;
        private System.Windows.Forms.GroupBox grpQuerySyntax;
        private System.Windows.Forms.RadioButton radSparqlExtended;
        private System.Windows.Forms.RadioButton radSparql11;
        private System.Windows.Forms.RadioButton radSparql10;
        private System.Windows.Forms.StatusStrip stsStatus;
        private System.Windows.Forms.ToolStripStatusLabel stsGraphs;
        private System.Windows.Forms.ToolStripStatusLabel stsTriples;
        private System.Windows.Forms.ToolStripStatusLabel stsLastQuery;
        private System.Windows.Forms.NumericUpDown numTimeout;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.Button btnInspect;
        private System.Windows.Forms.CheckBox chkPartialResults;
        private System.Windows.Forms.Label lblMilliseconds;
        private System.Windows.Forms.CheckBox chkWebDemand;
        private System.Windows.Forms.GroupBox grpResultsFormat;
        private System.Windows.Forms.ComboBox cboResultsFormat;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cboGraphFormat;
        private System.Windows.Forms.Button btnSaveQuery;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.OpenFileDialog ofdQuery;
        private System.Windows.Forms.SaveFileDialog sfdQuery;
        private System.Windows.Forms.CheckBox chkAllowUnknownFunctions;
        private System.Windows.Forms.CheckBox chkAlgebraOptimisation;
        private System.Windows.Forms.CheckBox chkQueryOptimisation;
        private System.Windows.Forms.ToolTip ttpTips;
        private System.Windows.Forms.CheckBox chkViewResultsInApp;
        private System.Windows.Forms.CheckBox chkUseUtf8Bom;
        private System.Windows.Forms.Button btnViewLog;
        private System.Windows.Forms.CheckBox chkLogExplanation;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnExplain;
        private System.Windows.Forms.CheckBox chkFullTextIndexing;
        private System.Windows.Forms.GroupBox grpQueryTools;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}

