namespace VDS.RDF.Utilities.GraphBenchmarker
{
    partial class fclsGraphBenchmarker
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
            this.grpToTest = new System.Windows.Forms.GroupBox();
            this.btnAddTestCase = new System.Windows.Forms.Button();
            this.lstTripleCollectionImpl = new System.Windows.Forms.ListBox();
            this.chkUseDefault = new System.Windows.Forms.CheckBox();
            this.lstIGraphImpl = new System.Windows.Forms.ListBox();
            this.lblIGraphImpl = new System.Windows.Forms.Label();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.grpTestSet = new System.Windows.Forms.GroupBox();
            this.radLoadAndMem = new System.Windows.Forms.RadioButton();
            this.radStandard = new System.Windows.Forms.RadioButton();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblIterations2 = new System.Windows.Forms.Label();
            this.numIterations = new System.Windows.Forms.NumericUpDown();
            this.lblIterations = new System.Windows.Forms.Label();
            this.lstTestData = new System.Windows.Forms.ListBox();
            this.lblTestData = new System.Windows.Forms.Label();
            this.grpTestCases = new System.Windows.Forms.GroupBox();
            this.btnRemoveTestCase = new System.Windows.Forms.Button();
            this.lstTestCases = new System.Windows.Forms.ListBox();
            this.grpToTest.SuspendLayout();
            this.grpOptions.SuspendLayout();
            this.grpTestSet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).BeginInit();
            this.grpTestCases.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpToTest
            // 
            this.grpToTest.Controls.Add(this.btnAddTestCase);
            this.grpToTest.Controls.Add(this.lstTripleCollectionImpl);
            this.grpToTest.Controls.Add(this.chkUseDefault);
            this.grpToTest.Controls.Add(this.lstIGraphImpl);
            this.grpToTest.Controls.Add(this.lblIGraphImpl);
            this.grpToTest.Location = new System.Drawing.Point(12, 12);
            this.grpToTest.Name = "grpToTest";
            this.grpToTest.Size = new System.Drawing.Size(341, 272);
            this.grpToTest.TabIndex = 0;
            this.grpToTest.TabStop = false;
            this.grpToTest.Text = "Implementation to Test";
            // 
            // btnAddTestCase
            // 
            this.btnAddTestCase.Location = new System.Drawing.Point(227, 245);
            this.btnAddTestCase.Name = "btnAddTestCase";
            this.btnAddTestCase.Size = new System.Drawing.Size(108, 23);
            this.btnAddTestCase.TabIndex = 4;
            this.btnAddTestCase.Text = "Add Test Case";
            this.btnAddTestCase.UseVisualStyleBackColor = true;
            this.btnAddTestCase.Click += new System.EventHandler(this.btnAddTestCase_Click);
            // 
            // lstTripleCollectionImpl
            // 
            this.lstTripleCollectionImpl.Enabled = false;
            this.lstTripleCollectionImpl.FormattingEnabled = true;
            this.lstTripleCollectionImpl.Location = new System.Drawing.Point(6, 170);
            this.lstTripleCollectionImpl.Name = "lstTripleCollectionImpl";
            this.lstTripleCollectionImpl.ScrollAlwaysVisible = true;
            this.lstTripleCollectionImpl.Size = new System.Drawing.Size(329, 69);
            this.lstTripleCollectionImpl.TabIndex = 3;
            // 
            // chkUseDefault
            // 
            this.chkUseDefault.AutoSize = true;
            this.chkUseDefault.Location = new System.Drawing.Point(9, 147);
            this.chkUseDefault.Name = "chkUseDefault";
            this.chkUseDefault.Size = new System.Drawing.Size(256, 17);
            this.chkUseDefault.TabIndex = 2;
            this.chkUseDefault.Text = "Use following Triple Collection instead of Default:";
            this.chkUseDefault.UseVisualStyleBackColor = true;
            this.chkUseDefault.CheckedChanged += new System.EventHandler(this.chkUseDefault_CheckedChanged);
            // 
            // lstIGraphImpl
            // 
            this.lstIGraphImpl.FormattingEnabled = true;
            this.lstIGraphImpl.Location = new System.Drawing.Point(6, 42);
            this.lstIGraphImpl.Name = "lstIGraphImpl";
            this.lstIGraphImpl.ScrollAlwaysVisible = true;
            this.lstIGraphImpl.Size = new System.Drawing.Size(329, 95);
            this.lstIGraphImpl.TabIndex = 1;
            this.lstIGraphImpl.SelectedIndexChanged += new System.EventHandler(this.lstIGraphImpl_SelectedIndexChanged);
            // 
            // lblIGraphImpl
            // 
            this.lblIGraphImpl.AutoSize = true;
            this.lblIGraphImpl.Location = new System.Drawing.Point(6, 16);
            this.lblIGraphImpl.Name = "lblIGraphImpl";
            this.lblIGraphImpl.Size = new System.Drawing.Size(116, 13);
            this.lblIGraphImpl.TabIndex = 0;
            this.lblIGraphImpl.Text = "IGraph Implementation:";
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.grpTestSet);
            this.grpOptions.Controls.Add(this.btnRun);
            this.grpOptions.Controls.Add(this.lblIterations2);
            this.grpOptions.Controls.Add(this.numIterations);
            this.grpOptions.Controls.Add(this.lblIterations);
            this.grpOptions.Controls.Add(this.lstTestData);
            this.grpOptions.Controls.Add(this.lblTestData);
            this.grpOptions.Location = new System.Drawing.Point(577, 12);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(295, 272);
            this.grpOptions.TabIndex = 2;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "Test Options";
            // 
            // grpTestSet
            // 
            this.grpTestSet.Controls.Add(this.radLoadAndMem);
            this.grpTestSet.Controls.Add(this.radStandard);
            this.grpTestSet.Location = new System.Drawing.Point(9, 182);
            this.grpTestSet.Name = "grpTestSet";
            this.grpTestSet.Size = new System.Drawing.Size(280, 57);
            this.grpTestSet.TabIndex = 5;
            this.grpTestSet.TabStop = false;
            this.grpTestSet.Text = "Test Set";
            // 
            // radLoadAndMem
            // 
            this.radLoadAndMem.AutoSize = true;
            this.radLoadAndMem.Location = new System.Drawing.Point(80, 19);
            this.radLoadAndMem.Name = "radLoadAndMem";
            this.radLoadAndMem.Size = new System.Drawing.Size(168, 17);
            this.radLoadAndMem.TabIndex = 1;
            this.radLoadAndMem.Text = "Load and Memory Usage Only";
            this.radLoadAndMem.UseVisualStyleBackColor = true;
            // 
            // radStandard
            // 
            this.radStandard.AutoSize = true;
            this.radStandard.Checked = true;
            this.radStandard.Location = new System.Drawing.Point(6, 19);
            this.radStandard.Name = "radStandard";
            this.radStandard.Size = new System.Drawing.Size(68, 17);
            this.radStandard.TabIndex = 0;
            this.radStandard.TabStop = true;
            this.radStandard.Text = "Standard";
            this.radStandard.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(214, 245);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "Run Tests";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblIterations2
            // 
            this.lblIterations2.AutoSize = true;
            this.lblIterations2.Location = new System.Drawing.Point(6, 166);
            this.lblIterations2.Name = "lblIterations2";
            this.lblIterations2.Size = new System.Drawing.Size(49, 13);
            this.lblIterations2.TabIndex = 4;
            this.lblIterations2.Text = "iterations";
            // 
            // numIterations
            // 
            this.numIterations.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numIterations.Location = new System.Drawing.Point(209, 146);
            this.numIterations.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numIterations.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numIterations.Name = "numIterations";
            this.numIterations.Size = new System.Drawing.Size(80, 20);
            this.numIterations.TabIndex = 3;
            this.numIterations.ThousandsSeparator = true;
            this.numIterations.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // lblIterations
            // 
            this.lblIterations.Location = new System.Drawing.Point(6, 148);
            this.lblIterations.Name = "lblIterations";
            this.lblIterations.Size = new System.Drawing.Size(209, 18);
            this.lblIterations.TabIndex = 2;
            this.lblIterations.Text = "Where applicable average test result over ";
            // 
            // lstTestData
            // 
            this.lstTestData.FormattingEnabled = true;
            this.lstTestData.Location = new System.Drawing.Point(9, 42);
            this.lstTestData.Name = "lstTestData";
            this.lstTestData.ScrollAlwaysVisible = true;
            this.lstTestData.Size = new System.Drawing.Size(280, 95);
            this.lstTestData.TabIndex = 1;
            // 
            // lblTestData
            // 
            this.lblTestData.AutoSize = true;
            this.lblTestData.Location = new System.Drawing.Point(6, 16);
            this.lblTestData.Name = "lblTestData";
            this.lblTestData.Size = new System.Drawing.Size(89, 13);
            this.lblTestData.TabIndex = 0;
            this.lblTestData.Text = "Test Data to use:";
            // 
            // grpTestCases
            // 
            this.grpTestCases.Controls.Add(this.btnRemoveTestCase);
            this.grpTestCases.Controls.Add(this.lstTestCases);
            this.grpTestCases.Location = new System.Drawing.Point(359, 12);
            this.grpTestCases.Name = "grpTestCases";
            this.grpTestCases.Size = new System.Drawing.Size(212, 272);
            this.grpTestCases.TabIndex = 1;
            this.grpTestCases.TabStop = false;
            this.grpTestCases.Text = "Test Cases";
            // 
            // btnRemoveTestCase
            // 
            this.btnRemoveTestCase.Location = new System.Drawing.Point(98, 245);
            this.btnRemoveTestCase.Name = "btnRemoveTestCase";
            this.btnRemoveTestCase.Size = new System.Drawing.Size(108, 23);
            this.btnRemoveTestCase.TabIndex = 1;
            this.btnRemoveTestCase.Text = "Remove Test Case";
            this.btnRemoveTestCase.UseVisualStyleBackColor = true;
            this.btnRemoveTestCase.Click += new System.EventHandler(this.btnRemoveTestCase_Click);
            // 
            // lstTestCases
            // 
            this.lstTestCases.FormattingEnabled = true;
            this.lstTestCases.Location = new System.Drawing.Point(6, 19);
            this.lstTestCases.Name = "lstTestCases";
            this.lstTestCases.ScrollAlwaysVisible = true;
            this.lstTestCases.Size = new System.Drawing.Size(200, 212);
            this.lstTestCases.TabIndex = 0;
            // 
            // fclsGraphBenchmarker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 289);
            this.Controls.Add(this.grpTestCases);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.grpToTest);
            this.MaximizeBox = false;
            this.Name = "fclsGraphBenchmarker";
            this.Text = "Graph Benchmarker - Setup Test Run";
            this.grpToTest.ResumeLayout(false);
            this.grpToTest.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.grpTestSet.ResumeLayout(false);
            this.grpTestSet.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numIterations)).EndInit();
            this.grpTestCases.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpToTest;
        private System.Windows.Forms.Label lblIGraphImpl;
        private System.Windows.Forms.ListBox lstTripleCollectionImpl;
        private System.Windows.Forms.CheckBox chkUseDefault;
        private System.Windows.Forms.ListBox lstIGraphImpl;
        private System.Windows.Forms.Button btnAddTestCase;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.Label lblTestData;
        private System.Windows.Forms.ListBox lstTestData;
        private System.Windows.Forms.GroupBox grpTestCases;
        private System.Windows.Forms.Button btnRemoveTestCase;
        private System.Windows.Forms.ListBox lstTestCases;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblIterations2;
        private System.Windows.Forms.NumericUpDown numIterations;
        private System.Windows.Forms.Label lblIterations;
        private System.Windows.Forms.GroupBox grpTestSet;
        private System.Windows.Forms.RadioButton radLoadAndMem;
        private System.Windows.Forms.RadioButton radStandard;
    }
}

