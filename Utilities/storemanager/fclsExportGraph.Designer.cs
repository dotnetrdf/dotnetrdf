namespace dotNetRDFStore
{
    partial class fclsExportGraph
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
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblFormat = new System.Windows.Forms.Label();
            this.cboCompression = new System.Windows.Forms.ComboBox();
            this.lblCompression = new System.Windows.Forms.Label();
            this.chkHighSpeed = new System.Windows.Forms.CheckBox();
            this.chkPrettyPrinting = new System.Windows.Forms.CheckBox();
            this.cboWriter = new System.Windows.Forms.ComboBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.sfdExport = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(2, 9);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(52, 13);
            this.lblFile.TabIndex = 0;
            this.lblFile.Text = "Filename:";
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(52, 6);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(291, 20);
            this.txtFile.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(349, 4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(2, 39);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(42, 13);
            this.lblFormat.TabIndex = 3;
            this.lblFormat.Text = "Format:";
            // 
            // cboCompression
            // 
            this.cboCompression.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCompression.FormattingEnabled = true;
            this.cboCompression.Items.AddRange(new object[] {
            "None",
            "Minimal",
            "Default",
            "Medium",
            "More",
            "High"});
            this.cboCompression.Location = new System.Drawing.Point(192, 95);
            this.cboCompression.Name = "cboCompression";
            this.cboCompression.Size = new System.Drawing.Size(151, 21);
            this.cboCompression.TabIndex = 10;
            // 
            // lblCompression
            // 
            this.lblCompression.AutoSize = true;
            this.lblCompression.Location = new System.Drawing.Point(2, 98);
            this.lblCompression.Name = "lblCompression";
            this.lblCompression.Size = new System.Drawing.Size(187, 13);
            this.lblCompression.TabIndex = 9;
            this.lblCompression.Text = "Compression Level (where supported):";
            // 
            // chkHighSpeed
            // 
            this.chkHighSpeed.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chkHighSpeed.Location = new System.Drawing.Point(182, 63);
            this.chkHighSpeed.Name = "chkHighSpeed";
            this.chkHighSpeed.Size = new System.Drawing.Size(242, 32);
            this.chkHighSpeed.TabIndex = 8;
            this.chkHighSpeed.Text = "Use High Speed Mode if Graph is ill-suited to syntax compression";
            this.chkHighSpeed.UseVisualStyleBackColor = true;
            // 
            // chkPrettyPrinting
            // 
            this.chkPrettyPrinting.AutoSize = true;
            this.chkPrettyPrinting.Checked = true;
            this.chkPrettyPrinting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrettyPrinting.Location = new System.Drawing.Point(5, 63);
            this.chkPrettyPrinting.Name = "chkPrettyPrinting";
            this.chkPrettyPrinting.Size = new System.Drawing.Size(171, 17);
            this.chkPrettyPrinting.TabIndex = 7;
            this.chkPrettyPrinting.Text = "Use Pretty Printing if supported";
            this.chkPrettyPrinting.UseVisualStyleBackColor = true;
            // 
            // cboWriter
            // 
            this.cboWriter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWriter.FormattingEnabled = true;
            this.cboWriter.Items.AddRange(new object[] {
            "NTriples",
            "Turtle",
            "Turtle (Compressing Writer)",
            "Notation 3",
            "RDF/XML (DOM Writer)",
            "RDF/XML (Fast Writer)",
            "RDF/JSON",
            "XHTML+RDFa",
            "CSV",
            "TSV"});
            this.cboWriter.Location = new System.Drawing.Point(53, 36);
            this.cboWriter.Name = "cboWriter";
            this.cboWriter.Size = new System.Drawing.Size(290, 21);
            this.cboWriter.TabIndex = 6;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(135, 122);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 11;
            this.btnExport.Text = "&Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(216, 122);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // sfdExport
            // 
            this.sfdExport.Title = "Select file to export Graph to";
            // 
            // fclsExportGraph
            // 
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(426, 148);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.cboCompression);
            this.Controls.Add(this.lblCompression);
            this.Controls.Add(this.chkHighSpeed);
            this.Controls.Add(this.chkPrettyPrinting);
            this.Controls.Add(this.cboWriter);
            this.Controls.Add(this.lblFormat);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.lblFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fclsExportGraph";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Graph";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cboCompression;
        private System.Windows.Forms.Label lblCompression;
        private System.Windows.Forms.CheckBox chkHighSpeed;
        private System.Windows.Forms.CheckBox chkPrettyPrinting;
        private System.Windows.Forms.ComboBox cboWriter;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.SaveFileDialog sfdExport;
    }
}