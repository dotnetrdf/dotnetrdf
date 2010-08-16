namespace ParserGUI
{
    partial class fclsParserGUI
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
            this.grpSource = new System.Windows.Forms.GroupBox();
            this.txtRawData = new System.Windows.Forms.TextBox();
            this.radSourceRaw = new System.Windows.Forms.RadioButton();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.radSourceFile = new System.Windows.Forms.RadioButton();
            this.txtURI = new System.Windows.Forms.TextBox();
            this.radSourceURI = new System.Windows.Forms.RadioButton();
            this.ofdFile = new System.Windows.Forms.OpenFileDialog();
            this.grpParser = new System.Windows.Forms.GroupBox();
            this.cboParser = new System.Windows.Forms.ComboBox();
            this.radParserManual = new System.Windows.Forms.RadioButton();
            this.radParserAuto = new System.Windows.Forms.RadioButton();
            this.grpResults = new System.Windows.Forms.GroupBox();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnParse = new System.Windows.Forms.Button();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.chkParserTrace = new System.Windows.Forms.CheckBox();
            this.chkTokeniserTrace = new System.Windows.Forms.CheckBox();
            this.btnOutput = new System.Windows.Forms.Button();
            this.grpSource.SuspendLayout();
            this.grpParser.SuspendLayout();
            this.grpResults.SuspendLayout();
            this.grpOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSource
            // 
            this.grpSource.Controls.Add(this.txtRawData);
            this.grpSource.Controls.Add(this.radSourceRaw);
            this.grpSource.Controls.Add(this.btnBrowse);
            this.grpSource.Controls.Add(this.txtFile);
            this.grpSource.Controls.Add(this.radSourceFile);
            this.grpSource.Controls.Add(this.txtURI);
            this.grpSource.Controls.Add(this.radSourceURI);
            this.grpSource.Location = new System.Drawing.Point(12, 12);
            this.grpSource.Name = "grpSource";
            this.grpSource.Size = new System.Drawing.Size(709, 183);
            this.grpSource.TabIndex = 0;
            this.grpSource.TabStop = false;
            this.grpSource.Text = "Select RDF Source";
            // 
            // txtRawData
            // 
            this.txtRawData.Enabled = false;
            this.txtRawData.Location = new System.Drawing.Point(6, 88);
            this.txtRawData.Multiline = true;
            this.txtRawData.Name = "txtRawData";
            this.txtRawData.Size = new System.Drawing.Size(697, 89);
            this.txtRawData.TabIndex = 6;
            // 
            // radSourceRaw
            // 
            this.radSourceRaw.AutoSize = true;
            this.radSourceRaw.Location = new System.Drawing.Point(6, 65);
            this.radSourceRaw.Name = "radSourceRaw";
            this.radSourceRaw.Size = new System.Drawing.Size(126, 17);
            this.radSourceRaw.TabIndex = 5;
            this.radSourceRaw.TabStop = true;
            this.radSourceRaw.Text = "Enter Raw RDF Data";
            this.radSourceRaw.UseVisualStyleBackColor = true;
            this.radSourceRaw.CheckedChanged += new System.EventHandler(this.radSourceRaw_CheckedChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(639, 41);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(64, 21);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFile
            // 
            this.txtFile.Enabled = false;
            this.txtFile.Location = new System.Drawing.Point(121, 41);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(512, 20);
            this.txtFile.TabIndex = 3;
            // 
            // radSourceFile
            // 
            this.radSourceFile.AutoSize = true;
            this.radSourceFile.Location = new System.Drawing.Point(6, 42);
            this.radSourceFile.Name = "radSourceFile";
            this.radSourceFile.Size = new System.Drawing.Size(118, 17);
            this.radSourceFile.TabIndex = 2;
            this.radSourceFile.TabStop = true;
            this.radSourceFile.Text = "Read RDF from File";
            this.radSourceFile.UseVisualStyleBackColor = true;
            this.radSourceFile.CheckedChanged += new System.EventHandler(this.radSourceFile_CheckedChanged);
            // 
            // txtURI
            // 
            this.txtURI.Location = new System.Drawing.Point(147, 18);
            this.txtURI.Name = "txtURI";
            this.txtURI.Size = new System.Drawing.Size(556, 20);
            this.txtURI.TabIndex = 1;
            // 
            // radSourceURI
            // 
            this.radSourceURI.AutoSize = true;
            this.radSourceURI.Checked = true;
            this.radSourceURI.Location = new System.Drawing.Point(6, 19);
            this.radSourceURI.Name = "radSourceURI";
            this.radSourceURI.Size = new System.Drawing.Size(135, 17);
            this.radSourceURI.TabIndex = 0;
            this.radSourceURI.TabStop = true;
            this.radSourceURI.Text = "Retrieve RDF from URI";
            this.radSourceURI.UseVisualStyleBackColor = true;
            this.radSourceURI.CheckedChanged += new System.EventHandler(this.radSourceURI_CheckedChanged);
            // 
            // ofdFile
            // 
            this.ofdFile.DefaultExt = "rdf";
            this.ofdFile.Filter = "All RDF Files|*.rdf;*.ttl;*.nt;*.n3;*.json|RDF/XML Files|*.rdf|NTriples Files|*.n" +
                "t|Turtle Files|*.ttl|Notation 3 Files|*.n3";
            this.ofdFile.Title = "Select RDF File to Open";
            // 
            // grpParser
            // 
            this.grpParser.Controls.Add(this.cboParser);
            this.grpParser.Controls.Add(this.radParserManual);
            this.grpParser.Controls.Add(this.radParserAuto);
            this.grpParser.Location = new System.Drawing.Point(12, 201);
            this.grpParser.Name = "grpParser";
            this.grpParser.Size = new System.Drawing.Size(410, 68);
            this.grpParser.TabIndex = 1;
            this.grpParser.TabStop = false;
            this.grpParser.Text = "Select RDF Parser";
            // 
            // cboParser
            // 
            this.cboParser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboParser.Enabled = false;
            this.cboParser.FormattingEnabled = true;
            this.cboParser.Items.AddRange(new object[] {
            "NTriples (Native Parser)",
            "NTriples (Turtle Parser)",
            "Turtle Parser",
            "Notation 3 Parser",
            "RDF/XML Parser",
            "RDF/JSON Parser"});
            this.cboParser.Location = new System.Drawing.Point(147, 41);
            this.cboParser.Name = "cboParser";
            this.cboParser.Size = new System.Drawing.Size(254, 21);
            this.cboParser.TabIndex = 2;
            // 
            // radParserManual
            // 
            this.radParserManual.AutoSize = true;
            this.radParserManual.Location = new System.Drawing.Point(6, 42);
            this.radParserManual.Name = "radParserManual";
            this.radParserManual.Size = new System.Drawing.Size(139, 17);
            this.radParserManual.TabIndex = 1;
            this.radParserManual.Text = "Use the following Parser";
            this.radParserManual.UseVisualStyleBackColor = true;
            this.radParserManual.CheckedChanged += new System.EventHandler(this.radParserManual_CheckedChanged);
            // 
            // radParserAuto
            // 
            this.radParserAuto.AutoSize = true;
            this.radParserAuto.Checked = true;
            this.radParserAuto.Location = new System.Drawing.Point(6, 19);
            this.radParserAuto.Name = "radParserAuto";
            this.radParserAuto.Size = new System.Drawing.Size(225, 17);
            this.radParserAuto.TabIndex = 0;
            this.radParserAuto.TabStop = true;
            this.radParserAuto.Text = "Automatically select the appropriate Parser";
            this.radParserAuto.UseVisualStyleBackColor = true;
            // 
            // grpResults
            // 
            this.grpResults.Controls.Add(this.txtResults);
            this.grpResults.Location = new System.Drawing.Point(12, 294);
            this.grpResults.Name = "grpResults";
            this.grpResults.Size = new System.Drawing.Size(709, 184);
            this.grpResults.TabIndex = 2;
            this.grpResults.TabStop = false;
            this.grpResults.Text = "Parsing Results";
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(6, 19);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(697, 160);
            this.txtResults.TabIndex = 0;
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(286, 272);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 3;
            this.btnParse.Text = "&Parse RDF";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.chkParserTrace);
            this.grpOptions.Controls.Add(this.chkTokeniserTrace);
            this.grpOptions.Location = new System.Drawing.Point(428, 201);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(293, 68);
            this.grpOptions.TabIndex = 4;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Parser Options";
            // 
            // chkParserTrace
            // 
            this.chkParserTrace.AutoSize = true;
            this.chkParserTrace.Location = new System.Drawing.Point(6, 41);
            this.chkParserTrace.Name = "chkParserTrace";
            this.chkParserTrace.Size = new System.Drawing.Size(199, 17);
            this.chkParserTrace.TabIndex = 1;
            this.chkParserTrace.Text = "Show Parser Trace where supported";
            this.chkParserTrace.UseVisualStyleBackColor = true;
            // 
            // chkTokeniserTrace
            // 
            this.chkTokeniserTrace.AutoSize = true;
            this.chkTokeniserTrace.Location = new System.Drawing.Point(6, 19);
            this.chkTokeniserTrace.Name = "chkTokeniserTrace";
            this.chkTokeniserTrace.Size = new System.Drawing.Size(216, 17);
            this.chkTokeniserTrace.TabIndex = 0;
            this.chkTokeniserTrace.Text = "Show Tokeniser Trace where supported";
            this.chkTokeniserTrace.UseVisualStyleBackColor = true;
            // 
            // btnOutput
            // 
            this.btnOutput.Enabled = false;
            this.btnOutput.Location = new System.Drawing.Point(367, 272);
            this.btnOutput.Name = "btnOutput";
            this.btnOutput.Size = new System.Drawing.Size(75, 23);
            this.btnOutput.TabIndex = 5;
            this.btnOutput.Text = "&Output RDF";
            this.btnOutput.UseVisualStyleBackColor = true;
            this.btnOutput.Click += new System.EventHandler(this.btnOutput_Click);
            // 
            // fclsParserGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 484);
            this.Controls.Add(this.btnOutput);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.grpResults);
            this.Controls.Add(this.grpParser);
            this.Controls.Add(this.grpSource);
            this.MaximizeBox = false;
            this.Name = "fclsParserGUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "dotNetRDF Parser Testing GUI";
            this.Load += new System.EventHandler(this.fclsParserGUI_Load);
            this.grpSource.ResumeLayout(false);
            this.grpSource.PerformLayout();
            this.grpParser.ResumeLayout(false);
            this.grpParser.PerformLayout();
            this.grpResults.ResumeLayout(false);
            this.grpResults.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpSource;
        private System.Windows.Forms.TextBox txtURI;
        private System.Windows.Forms.RadioButton radSourceURI;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.RadioButton radSourceFile;
        private System.Windows.Forms.OpenFileDialog ofdFile;
        private System.Windows.Forms.TextBox txtRawData;
        private System.Windows.Forms.RadioButton radSourceRaw;
        private System.Windows.Forms.GroupBox grpParser;
        private System.Windows.Forms.ComboBox cboParser;
        private System.Windows.Forms.RadioButton radParserManual;
        private System.Windows.Forms.RadioButton radParserAuto;
        private System.Windows.Forms.GroupBox grpResults;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.CheckBox chkParserTrace;
        private System.Windows.Forms.CheckBox chkTokeniserTrace;
        private System.Windows.Forms.Button btnOutput;
    }
}

