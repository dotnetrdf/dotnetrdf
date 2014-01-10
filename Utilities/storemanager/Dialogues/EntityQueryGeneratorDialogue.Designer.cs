/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    partial class EntityQueryGeneratorDialogue
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityQueryGeneratorDialogue));
            this.lblEntityPredicatesLimit = new System.Windows.Forms.Label();
            this.numValuesPerPredicateLimit = new System.Windows.Forms.NumericUpDown();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblExplanation = new System.Windows.Forms.Label();
            this.lblIntro = new System.Windows.Forms.Label();
            this.txtSampleOutput = new System.Windows.Forms.TextBox();
            this.lblSampleOutput = new System.Windows.Forms.Label();
            this.lblSampleInput = new System.Windows.Forms.Label();
            this.txtSampleInput = new System.Windows.Forms.TextBox();
            this.ttpTips = new System.Windows.Forms.ToolTip(this.components);
            this.lblCurrentQuery = new System.Windows.Forms.Label();
            this.txtCurrentQuery = new System.Windows.Forms.TextBox();
            this.flpLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.grpExamples = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numValuesPerPredicateLimit)).BeginInit();
            this.flpLayout.SuspendLayout();
            this.grpExamples.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblEntityPredicatesLimit
            // 
            this.lblEntityPredicatesLimit.AutoSize = true;
            this.lblEntityPredicatesLimit.Location = new System.Drawing.Point(0, 9);
            this.lblEntityPredicatesLimit.Name = "lblEntityPredicatesLimit";
            this.lblEntityPredicatesLimit.Size = new System.Drawing.Size(157, 13);
            this.lblEntityPredicatesLimit.TabIndex = 0;
            this.lblEntityPredicatesLimit.Text = "Predicate Minimum Usage Limit:";
            // 
            // numValuesPerPredicateLimit
            // 
            this.numValuesPerPredicateLimit.Location = new System.Drawing.Point(163, 7);
            this.numValuesPerPredicateLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numValuesPerPredicateLimit.Name = "numValuesPerPredicateLimit";
            this.numValuesPerPredicateLimit.Size = new System.Drawing.Size(35, 20);
            this.numValuesPerPredicateLimit.TabIndex = 1;
            this.numValuesPerPredicateLimit.ThousandsSeparator = true;
            this.ttpTips.SetToolTip(this.numValuesPerPredicateLimit, "Minimum number of entities for which the predicate must be used for the predicate" +
        " to be included in the generated query");
            this.numValuesPerPredicateLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(279, 505);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(198, 505);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lblExplanation
            // 
            this.lblExplanation.Location = new System.Drawing.Point(3, 18);
            this.lblExplanation.Name = "lblExplanation";
            this.lblExplanation.Size = new System.Drawing.Size(463, 60);
            this.lblExplanation.TabIndex = 1;
            this.lblExplanation.Text = resources.GetString("lblExplanation.Text");
            // 
            // lblIntro
            // 
            this.lblIntro.Location = new System.Drawing.Point(3, 0);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new System.Drawing.Size(463, 18);
            this.lblIntro.TabIndex = 0;
            this.lblIntro.Text = "Based on current query generates a query for all the predicates of first query va" +
    "riable.";
            // 
            // txtSampleOutput
            // 
            this.txtSampleOutput.Location = new System.Drawing.Point(9, 124);
            this.txtSampleOutput.Multiline = true;
            this.txtSampleOutput.Name = "txtSampleOutput";
            this.txtSampleOutput.ReadOnly = true;
            this.txtSampleOutput.Size = new System.Drawing.Size(459, 100);
            this.txtSampleOutput.TabIndex = 3;
            this.txtSampleOutput.TabStop = false;
            this.txtSampleOutput.Text = "SELECT ?person ?name ?age \r\nWHERE\r\n{\r\n   { SELECT ?person WHERE { ?person rdf:typ" +
    "e :Person. } }\r\n   OPTIONAL { ?person :Name ?name }\r\n   OPTIONAL { ?person :hasA" +
    "ge ?age }\r\n}\r\n";
            // 
            // lblSampleOutput
            // 
            this.lblSampleOutput.AutoSize = true;
            this.lblSampleOutput.Location = new System.Drawing.Point(6, 108);
            this.lblSampleOutput.Name = "lblSampleOutput";
            this.lblSampleOutput.Size = new System.Drawing.Size(113, 13);
            this.lblSampleOutput.TabIndex = 2;
            this.lblSampleOutput.Text = "Example Output Query";
            // 
            // lblSampleInput
            // 
            this.lblSampleInput.AutoSize = true;
            this.lblSampleInput.Location = new System.Drawing.Point(6, 16);
            this.lblSampleInput.Name = "lblSampleInput";
            this.lblSampleInput.Size = new System.Drawing.Size(105, 13);
            this.lblSampleInput.TabIndex = 0;
            this.lblSampleInput.Text = "Example Input Query";
            // 
            // txtSampleInput
            // 
            this.txtSampleInput.Location = new System.Drawing.Point(9, 32);
            this.txtSampleInput.Multiline = true;
            this.txtSampleInput.Name = "txtSampleInput";
            this.txtSampleInput.ReadOnly = true;
            this.txtSampleInput.Size = new System.Drawing.Size(459, 73);
            this.txtSampleInput.TabIndex = 1;
            this.txtSampleInput.TabStop = false;
            this.txtSampleInput.Text = "SELECT ?person\r\nWHERE\r\n{\r\n  ?person rdf:type :Person.\r\n}\r\n";
            // 
            // lblCurrentQuery
            // 
            this.lblCurrentQuery.AutoSize = true;
            this.lblCurrentQuery.Location = new System.Drawing.Point(3, 319);
            this.lblCurrentQuery.Name = "lblCurrentQuery";
            this.lblCurrentQuery.Size = new System.Drawing.Size(72, 13);
            this.lblCurrentQuery.TabIndex = 3;
            this.lblCurrentQuery.Text = "Current Query";
            // 
            // txtCurrentQuery
            // 
            this.txtCurrentQuery.Location = new System.Drawing.Point(3, 335);
            this.txtCurrentQuery.Multiline = true;
            this.txtCurrentQuery.Name = "txtCurrentQuery";
            this.txtCurrentQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCurrentQuery.Size = new System.Drawing.Size(481, 100);
            this.txtCurrentQuery.TabIndex = 4;
            // 
            // flpLayout
            // 
            this.flpLayout.Controls.Add(this.lblIntro);
            this.flpLayout.Controls.Add(this.lblExplanation);
            this.flpLayout.Controls.Add(this.grpExamples);
            this.flpLayout.Controls.Add(this.lblCurrentQuery);
            this.flpLayout.Controls.Add(this.txtCurrentQuery);
            this.flpLayout.Controls.Add(this.panel1);
            this.flpLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpLayout.Location = new System.Drawing.Point(14, 12);
            this.flpLayout.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this.flpLayout.Name = "flpLayout";
            this.flpLayout.Size = new System.Drawing.Size(487, 487);
            this.flpLayout.TabIndex = 0;
            // 
            // grpExamples
            // 
            this.grpExamples.Controls.Add(this.lblSampleInput);
            this.grpExamples.Controls.Add(this.txtSampleInput);
            this.grpExamples.Controls.Add(this.lblSampleOutput);
            this.grpExamples.Controls.Add(this.txtSampleOutput);
            this.grpExamples.Location = new System.Drawing.Point(3, 81);
            this.grpExamples.Name = "grpExamples";
            this.grpExamples.Size = new System.Drawing.Size(481, 235);
            this.grpExamples.TabIndex = 2;
            this.grpExamples.TabStop = false;
            this.grpExamples.Text = "Examples";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblEntityPredicatesLimit);
            this.panel1.Controls.Add(this.numValuesPerPredicateLimit);
            this.panel1.Location = new System.Drawing.Point(3, 441);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(481, 38);
            this.panel1.TabIndex = 5;
            // 
            // EntityQueryGeneratorDialogue
            // 
            this.AcceptButton = this.btnGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(518, 540);
            this.Controls.Add(this.flpLayout);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EntityQueryGeneratorDialogue";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Generate Entity Query?";
            ((System.ComponentModel.ISupportInitialize)(this.numValuesPerPredicateLimit)).EndInit();
            this.flpLayout.ResumeLayout(false);
            this.flpLayout.PerformLayout();
            this.grpExamples.ResumeLayout(false);
            this.grpExamples.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblEntityPredicatesLimit;
        private System.Windows.Forms.NumericUpDown numValuesPerPredicateLimit;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblExplanation;
        private System.Windows.Forms.Label lblIntro;
        private System.Windows.Forms.TextBox txtSampleOutput;
        private System.Windows.Forms.Label lblSampleOutput;
        private System.Windows.Forms.Label lblSampleInput;
        private System.Windows.Forms.TextBox txtSampleInput;
        private System.Windows.Forms.ToolTip ttpTips;
        private System.Windows.Forms.Label lblCurrentQuery;
        private System.Windows.Forms.TextBox txtCurrentQuery;
        private System.Windows.Forms.FlowLayoutPanel flpLayout;
        private System.Windows.Forms.GroupBox grpExamples;
        private System.Windows.Forms.Panel panel1;
    }
}