namespace ParserGUI
{
    partial class fclsOutput
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
            this.grpTriples = new System.Windows.Forms.GroupBox();
            this.lvwTriples = new System.Windows.Forms.ListView();
            this.colSubject = new System.Windows.Forms.ColumnHeader();
            this.colPredicate = new System.Windows.Forms.ColumnHeader();
            this.colObject = new System.Windows.Forms.ColumnHeader();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.cboCompression = new System.Windows.Forms.ComboBox();
            this.lblCompression = new System.Windows.Forms.Label();
            this.chkHighSpeed = new System.Windows.Forms.CheckBox();
            this.chkPrettyPrinting = new System.Windows.Forms.CheckBox();
            this.lblWriter = new System.Windows.Forms.Label();
            this.cboWriter = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnOutput = new System.Windows.Forms.Button();
            this.grpTriples.SuspendLayout();
            this.grpOutput.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTriples
            // 
            this.grpTriples.Controls.Add(this.lvwTriples);
            this.grpTriples.Location = new System.Drawing.Point(4, 12);
            this.grpTriples.Name = "grpTriples";
            this.grpTriples.Size = new System.Drawing.Size(613, 153);
            this.grpTriples.TabIndex = 2;
            this.grpTriples.TabStop = false;
            this.grpTriples.Text = "Graph Contents";
            // 
            // lvwTriples
            // 
            this.lvwTriples.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSubject,
            this.colPredicate,
            this.colObject});
            this.lvwTriples.FullRowSelect = true;
            this.lvwTriples.GridLines = true;
            this.lvwTriples.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvwTriples.Location = new System.Drawing.Point(6, 19);
            this.lvwTriples.Name = "lvwTriples";
            this.lvwTriples.Size = new System.Drawing.Size(600, 128);
            this.lvwTriples.TabIndex = 1;
            this.lvwTriples.UseCompatibleStateImageBehavior = false;
            this.lvwTriples.View = System.Windows.Forms.View.Details;
            // 
            // colSubject
            // 
            this.colSubject.Text = "Subject";
            this.colSubject.Width = 209;
            // 
            // colPredicate
            // 
            this.colPredicate.Text = "Predicate";
            this.colPredicate.Width = 187;
            // 
            // colObject
            // 
            this.colObject.Text = "Object";
            this.colObject.Width = 188;
            // 
            // grpOutput
            // 
            this.grpOutput.Controls.Add(this.cboCompression);
            this.grpOutput.Controls.Add(this.lblCompression);
            this.grpOutput.Controls.Add(this.chkHighSpeed);
            this.grpOutput.Controls.Add(this.chkPrettyPrinting);
            this.grpOutput.Controls.Add(this.lblWriter);
            this.grpOutput.Controls.Add(this.cboWriter);
            this.grpOutput.Location = new System.Drawing.Point(4, 171);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(613, 94);
            this.grpOutput.TabIndex = 3;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "Select RDF Writer and Options";
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
            this.cboCompression.Location = new System.Drawing.Point(199, 67);
            this.cboCompression.Name = "cboCompression";
            this.cboCompression.Size = new System.Drawing.Size(146, 21);
            this.cboCompression.TabIndex = 5;
            // 
            // lblCompression
            // 
            this.lblCompression.AutoSize = true;
            this.lblCompression.Location = new System.Drawing.Point(8, 70);
            this.lblCompression.Name = "lblCompression";
            this.lblCompression.Size = new System.Drawing.Size(187, 13);
            this.lblCompression.TabIndex = 4;
            this.lblCompression.Text = "Compression Level (where supported):";
            // 
            // chkHighSpeed
            // 
            this.chkHighSpeed.AutoSize = true;
            this.chkHighSpeed.Checked = true;
            this.chkHighSpeed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighSpeed.Location = new System.Drawing.Point(199, 44);
            this.chkHighSpeed.Name = "chkHighSpeed";
            this.chkHighSpeed.Size = new System.Drawing.Size(331, 17);
            this.chkHighSpeed.TabIndex = 3;
            this.chkHighSpeed.Text = "Use High Speed Mode if Graph is ill-suited to syntax compression";
            this.chkHighSpeed.UseVisualStyleBackColor = true;
            // 
            // chkPrettyPrinting
            // 
            this.chkPrettyPrinting.AutoSize = true;
            this.chkPrettyPrinting.Checked = true;
            this.chkPrettyPrinting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrettyPrinting.Location = new System.Drawing.Point(11, 44);
            this.chkPrettyPrinting.Name = "chkPrettyPrinting";
            this.chkPrettyPrinting.Size = new System.Drawing.Size(171, 17);
            this.chkPrettyPrinting.TabIndex = 2;
            this.chkPrettyPrinting.Text = "Use Pretty Printing if supported";
            this.chkPrettyPrinting.UseVisualStyleBackColor = true;
            // 
            // lblWriter
            // 
            this.lblWriter.AutoSize = true;
            this.lblWriter.Location = new System.Drawing.Point(8, 19);
            this.lblWriter.Name = "lblWriter";
            this.lblWriter.Size = new System.Drawing.Size(122, 13);
            this.lblWriter.TabIndex = 1;
            this.lblWriter.Text = "Use the following Writer:";
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
            "RDF/JSON"});
            this.cboWriter.Location = new System.Drawing.Point(136, 16);
            this.cboWriter.Name = "cboWriter";
            this.cboWriter.Size = new System.Drawing.Size(285, 21);
            this.cboWriter.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtResults);
            this.groupBox1.Location = new System.Drawing.Point(4, 287);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(613, 139);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Writing Output";
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(6, 28);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(600, 105);
            this.txtResults.TabIndex = 0;
            // 
            // btnOutput
            // 
            this.btnOutput.Location = new System.Drawing.Point(274, 266);
            this.btnOutput.Name = "btnOutput";
            this.btnOutput.Size = new System.Drawing.Size(75, 23);
            this.btnOutput.TabIndex = 5;
            this.btnOutput.Text = "&Output";
            this.btnOutput.UseVisualStyleBackColor = true;
            this.btnOutput.Click += new System.EventHandler(this.btnOutput_Click);
            // 
            // fclsOutput
            // 
            this.AcceptButton = this.btnOutput;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 432);
            this.Controls.Add(this.btnOutput);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.grpTriples);
            this.Name = "fclsOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "dotNetRDF Parser Testing GUI - Output Validator";
            this.grpTriples.ResumeLayout(false);
            this.grpOutput.ResumeLayout(false);
            this.grpOutput.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTriples;
        private System.Windows.Forms.ListView lvwTriples;
        private System.Windows.Forms.ColumnHeader colSubject;
        private System.Windows.Forms.ColumnHeader colPredicate;
        private System.Windows.Forms.ColumnHeader colObject;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.ComboBox cboWriter;
        private System.Windows.Forms.Label lblWriter;
        private System.Windows.Forms.CheckBox chkHighSpeed;
        private System.Windows.Forms.CheckBox chkPrettyPrinting;
        private System.Windows.Forms.ComboBox cboCompression;
        private System.Windows.Forms.Label lblCompression;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button btnOutput;

    }
}