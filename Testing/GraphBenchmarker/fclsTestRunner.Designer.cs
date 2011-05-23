using VDS.RDF.Utilities.StoreManager;

namespace VDS.RDF.Utilities.GraphBenchmarker
{
    partial class fclsTestRunner : CrossThreadForm
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
            this.grpTestCases = new System.Windows.Forms.GroupBox();
            this.lstTestCases = new System.Windows.Forms.ListBox();
            this.grpTests = new System.Windows.Forms.GroupBox();
            this.grpTestResult = new System.Windows.Forms.GroupBox();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.grpTestInfo = new System.Windows.Forms.GroupBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lstTests = new System.Windows.Forms.ListBox();
            this.prgTests = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpTestCases.SuspendLayout();
            this.grpTests.SuspendLayout();
            this.grpTestResult.SuspendLayout();
            this.grpTestInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTestCases
            // 
            this.grpTestCases.Controls.Add(this.lstTestCases);
            this.grpTestCases.Location = new System.Drawing.Point(12, 12);
            this.grpTestCases.Name = "grpTestCases";
            this.grpTestCases.Size = new System.Drawing.Size(212, 285);
            this.grpTestCases.TabIndex = 3;
            this.grpTestCases.TabStop = false;
            this.grpTestCases.Text = "Test Cases";
            // 
            // lstTestCases
            // 
            this.lstTestCases.FormattingEnabled = true;
            this.lstTestCases.Location = new System.Drawing.Point(6, 19);
            this.lstTestCases.Name = "lstTestCases";
            this.lstTestCases.Size = new System.Drawing.Size(200, 251);
            this.lstTestCases.TabIndex = 0;
            this.lstTestCases.SelectedIndexChanged += new System.EventHandler(this.lstTestCases_SelectedIndexChanged);
            // 
            // grpTests
            // 
            this.grpTests.Controls.Add(this.grpTestResult);
            this.grpTests.Controls.Add(this.grpTestInfo);
            this.grpTests.Controls.Add(this.lstTests);
            this.grpTests.Location = new System.Drawing.Point(230, 12);
            this.grpTests.Name = "grpTests";
            this.grpTests.Size = new System.Drawing.Size(470, 285);
            this.grpTests.TabIndex = 4;
            this.grpTests.TabStop = false;
            this.grpTests.Text = "Tests";
            // 
            // grpTestResult
            // 
            this.grpTestResult.Controls.Add(this.txtResults);
            this.grpTestResult.Location = new System.Drawing.Point(6, 201);
            this.grpTestResult.Name = "grpTestResult";
            this.grpTestResult.Size = new System.Drawing.Size(452, 78);
            this.grpTestResult.TabIndex = 3;
            this.grpTestResult.TabStop = false;
            this.grpTestResult.Text = "Test Result";
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(6, 18);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.Size = new System.Drawing.Size(446, 47);
            this.txtResults.TabIndex = 1;
            this.txtResults.Text = "No Test Case Currently Selected";
            // 
            // grpTestInfo
            // 
            this.grpTestInfo.Controls.Add(this.txtDescription);
            this.grpTestInfo.Location = new System.Drawing.Point(6, 120);
            this.grpTestInfo.Name = "grpTestInfo";
            this.grpTestInfo.Size = new System.Drawing.Size(458, 75);
            this.grpTestInfo.TabIndex = 2;
            this.grpTestInfo.TabStop = false;
            this.grpTestInfo.Text = "Test Information";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(6, 19);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(446, 47);
            this.txtDescription.TabIndex = 0;
            this.txtDescription.Text = "No Test Currently Selected";
            // 
            // lstTests
            // 
            this.lstTests.ColumnWidth = 150;
            this.lstTests.FormattingEnabled = true;
            this.lstTests.Location = new System.Drawing.Point(6, 19);
            this.lstTests.MultiColumn = true;
            this.lstTests.Name = "lstTests";
            this.lstTests.Size = new System.Drawing.Size(458, 95);
            this.lstTests.TabIndex = 1;
            this.lstTests.SelectedIndexChanged += new System.EventHandler(this.lstTests_SelectedIndexChanged);
            // 
            // prgTests
            // 
            this.prgTests.Location = new System.Drawing.Point(12, 303);
            this.prgTests.Name = "prgTests";
            this.prgTests.Size = new System.Drawing.Size(688, 23);
            this.prgTests.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgTests.TabIndex = 5;
            // 
            // lblProgress
            // 
            this.lblProgress.Location = new System.Drawing.Point(12, 339);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(688, 19);
            this.lblProgress.TabIndex = 6;
            this.lblProgress.Text = "label1";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(319, 361);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // fclsTestRunner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 388);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.prgTests);
            this.Controls.Add(this.grpTests);
            this.Controls.Add(this.grpTestCases);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "fclsTestRunner";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Graph Benchmarker - Test Runner";
            this.grpTestCases.ResumeLayout(false);
            this.grpTests.ResumeLayout(false);
            this.grpTestResult.ResumeLayout(false);
            this.grpTestResult.PerformLayout();
            this.grpTestInfo.ResumeLayout(false);
            this.grpTestInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTestCases;
        private System.Windows.Forms.ListBox lstTestCases;
        private System.Windows.Forms.GroupBox grpTests;
        private System.Windows.Forms.ListBox lstTests;
        private System.Windows.Forms.GroupBox grpTestInfo;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.GroupBox grpTestResult;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.ProgressBar prgTests;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button btnCancel;
    }
}