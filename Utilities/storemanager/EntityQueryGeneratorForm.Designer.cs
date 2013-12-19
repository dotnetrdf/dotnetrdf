namespace VDS.RDF.Utilities.StoreManager
{
    partial class EntityQueryGeneratorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityQueryGeneratorForm));
            this.lblEntityPredicatesLimit = new System.Windows.Forms.Label();
            this.numValuesPerPredicateLimit = new System.Windows.Forms.NumericUpDown();
            this.btnClos = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblSampleOutput = new System.Windows.Forms.Label();
            this.lblParameters = new System.Windows.Forms.Label();
            this.numColumnWords = new System.Windows.Forms.NumericUpDown();
            this.lblColumnWords = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numValuesPerPredicateLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numColumnWords)).BeginInit();
            this.SuspendLayout();
            // 
            // lblEntityPredicatesLimit
            // 
            this.lblEntityPredicatesLimit.AutoSize = true;
            this.lblEntityPredicatesLimit.Location = new System.Drawing.Point(54, 346);
            this.lblEntityPredicatesLimit.Name = "lblEntityPredicatesLimit";
            this.lblEntityPredicatesLimit.Size = new System.Drawing.Size(122, 13);
            this.lblEntityPredicatesLimit.TabIndex = 14;
            this.lblEntityPredicatesLimit.Text = "predicate usage min limit";
            // 
            // numValuesPerPredicateLimit
            // 
            this.numValuesPerPredicateLimit.Location = new System.Drawing.Point(13, 344);
            this.numValuesPerPredicateLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numValuesPerPredicateLimit.Name = "numValuesPerPredicateLimit";
            this.numValuesPerPredicateLimit.Size = new System.Drawing.Size(35, 20);
            this.numValuesPerPredicateLimit.TabIndex = 13;
            this.numValuesPerPredicateLimit.ThousandsSeparator = true;
            this.numValuesPerPredicateLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnClos
            // 
            this.btnClos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClos.Location = new System.Drawing.Point(430, 411);
            this.btnClos.Name = "btnClos";
            this.btnClos.Size = new System.Drawing.Size(75, 23);
            this.btnClos.TabIndex = 15;
            this.btnClos.Text = "Close";
            this.btnClos.UseVisualStyleBackColor = true;
            this.btnClos.Click += new System.EventHandler(this.btnClos_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(349, 411);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 16;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(463, 60);
            this.label1.TabIndex = 17;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(463, 18);
            this.label2.TabIndex = 18;
            this.label2.Text = "Based on current query generates a query for all the predicates of first query va" +
                "riable.";
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(12, 195);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(459, 100);
            this.textBox1.TabIndex = 19;
            this.textBox1.Text = "select ?person ?name ?age ..\r\nwhere {\r\n?person rdf:type :Person.\r\noptional(?perso" +
                "n :Name ?name).\r\noptional(?person :hasAge ?age).\r\n....\r\n}\r\n";
            // 
            // lblSampleOutput
            // 
            this.lblSampleOutput.AutoSize = true;
            this.lblSampleOutput.Location = new System.Drawing.Point(9, 179);
            this.lblSampleOutput.Name = "lblSampleOutput";
            this.lblSampleOutput.Size = new System.Drawing.Size(74, 13);
            this.lblSampleOutput.TabIndex = 20;
            this.lblSampleOutput.Text = "SampleOutput";
            // 
            // lblParameters
            // 
            this.lblParameters.AutoSize = true;
            this.lblParameters.Location = new System.Drawing.Point(9, 312);
            this.lblParameters.Name = "lblParameters";
            this.lblParameters.Size = new System.Drawing.Size(60, 13);
            this.lblParameters.TabIndex = 21;
            this.lblParameters.Text = "Parameters";
            // 
            // numColumnWords
            // 
            this.numColumnWords.Location = new System.Drawing.Point(13, 370);
            this.numColumnWords.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numColumnWords.Name = "numColumnWords";
            this.numColumnWords.Size = new System.Drawing.Size(35, 20);
            this.numColumnWords.TabIndex = 22;
            this.numColumnWords.ThousandsSeparator = true;
            this.numColumnWords.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblColumnWords
            // 
            this.lblColumnWords.AutoSize = true;
            this.lblColumnWords.Location = new System.Drawing.Point(54, 372);
            this.lblColumnWords.Name = "lblColumnWords";
            this.lblColumnWords.Size = new System.Drawing.Size(456, 13);
            this.lblColumnWords.TabIndex = 23;
            this.lblColumnWords.Text = "column name (takes X words from predicate uri, delimited by / or #, right to left" +
                " as column name)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "SampleInputQuery";
            // 
            // textBox2
            // 
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(12, 112);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(459, 64);
            this.textBox2.TabIndex = 25;
            this.textBox2.Text = "select ?person\r\nwhere {\r\n?person rdf:type :Person.\r\n}\r\n";
            // 
            // EntityQueryGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 446);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblColumnWords);
            this.Controls.Add(this.numColumnWords);
            this.Controls.Add(this.lblParameters);
            this.Controls.Add(this.lblSampleOutput);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnClos);
            this.Controls.Add(this.lblEntityPredicatesLimit);
            this.Controls.Add(this.numValuesPerPredicateLimit);
            this.Name = "EntityQueryGeneratorForm";
            this.Text = "EntityQueryGeneratorForm";
            ((System.ComponentModel.ISupportInitialize)(this.numValuesPerPredicateLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numColumnWords)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblEntityPredicatesLimit;
        private System.Windows.Forms.NumericUpDown numValuesPerPredicateLimit;
        private System.Windows.Forms.Button btnClos;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblSampleOutput;
        private System.Windows.Forms.Label lblParameters;
        private System.Windows.Forms.NumericUpDown numColumnWords;
        private System.Windows.Forms.Label lblColumnWords;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
    }
}