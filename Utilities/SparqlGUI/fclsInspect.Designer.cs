namespace VDS.RDF.Utilities.Sparql
{
    partial class fclsInspect
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
            this.grpQuery = new System.Windows.Forms.GroupBox();
            this.lblSyntaxCompatability = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.grpAlgebra = new System.Windows.Forms.GroupBox();
            this.txtAlgebra = new System.Windows.Forms.TextBox();
            this.lblParseTime = new System.Windows.Forms.Label();
            this.grpQuery.SuspendLayout();
            this.grpAlgebra.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpQuery
            // 
            this.grpQuery.Controls.Add(this.lblSyntaxCompatability);
            this.grpQuery.Controls.Add(this.txtQuery);
            this.grpQuery.Location = new System.Drawing.Point(12, 26);
            this.grpQuery.Name = "grpQuery";
            this.grpQuery.Size = new System.Drawing.Size(582, 205);
            this.grpQuery.TabIndex = 1;
            this.grpQuery.TabStop = false;
            this.grpQuery.Text = "Query - Optimised && Explictly Nested Form";
            // 
            // lblSyntaxCompatability
            // 
            this.lblSyntaxCompatability.AutoSize = true;
            this.lblSyntaxCompatability.Location = new System.Drawing.Point(3, 180);
            this.lblSyntaxCompatability.Name = "lblSyntaxCompatability";
            this.lblSyntaxCompatability.Size = new System.Drawing.Size(107, 13);
            this.lblSyntaxCompatability.TabIndex = 1;
            this.lblSyntaxCompatability.Text = "Syntax Compatability:";
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(6, 19);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ReadOnly = true;
            this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQuery.Size = new System.Drawing.Size(566, 150);
            this.txtQuery.TabIndex = 0;
            // 
            // grpAlgebra
            // 
            this.grpAlgebra.Controls.Add(this.txtAlgebra);
            this.grpAlgebra.Location = new System.Drawing.Point(12, 237);
            this.grpAlgebra.Name = "grpAlgebra";
            this.grpAlgebra.Size = new System.Drawing.Size(582, 152);
            this.grpAlgebra.TabIndex = 2;
            this.grpAlgebra.TabStop = false;
            this.grpAlgebra.Text = "Query Algebra";
            // 
            // txtAlgebra
            // 
            this.txtAlgebra.Location = new System.Drawing.Point(10, 19);
            this.txtAlgebra.Multiline = true;
            this.txtAlgebra.Name = "txtAlgebra";
            this.txtAlgebra.ReadOnly = true;
            this.txtAlgebra.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAlgebra.Size = new System.Drawing.Size(566, 127);
            this.txtAlgebra.TabIndex = 0;
            // 
            // lblParseTime
            // 
            this.lblParseTime.AutoSize = true;
            this.lblParseTime.Location = new System.Drawing.Point(15, 9);
            this.lblParseTime.Name = "lblParseTime";
            this.lblParseTime.Size = new System.Drawing.Size(95, 13);
            this.lblParseTime.TabIndex = 0;
            this.lblParseTime.Text = "Took ?ms to parse";
            // 
            // fclsInspect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 401);
            this.Controls.Add(this.lblParseTime);
            this.Controls.Add(this.grpAlgebra);
            this.Controls.Add(this.grpQuery);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "fclsInspect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Query Inspector";
            this.grpQuery.ResumeLayout(false);
            this.grpQuery.PerformLayout();
            this.grpAlgebra.ResumeLayout(false);
            this.grpAlgebra.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpQuery;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.GroupBox grpAlgebra;
        private System.Windows.Forms.TextBox txtAlgebra;
        private System.Windows.Forms.Label lblParseTime;
        private System.Windows.Forms.Label lblSyntaxCompatability;
    }
}