namespace VDS.RDF.Utilities.StoreManager
{
    partial class fclsAddTriples
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
            this.lblIntro = new System.Windows.Forms.Label();
            this.txtRDF = new System.Windows.Forms.TextBox();
            this.btnAddTriples = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblFormat = new System.Windows.Forms.Label();
            this.cboFormat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblIntro
            // 
            this.lblIntro.Location = new System.Drawing.Point(12, 9);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new System.Drawing.Size(461, 31);
            this.lblIntro.TabIndex = 0;
            this.lblIntro.Text = "Enter RDF data into the following box to add the resulting Triples to the Graph. " +
                " The format of the RDF entered will be automatically detected for you unless you" +
                " specify a format";
            // 
            // txtRDF
            // 
            this.txtRDF.Location = new System.Drawing.Point(13, 66);
            this.txtRDF.Multiline = true;
            this.txtRDF.Name = "txtRDF";
            this.txtRDF.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRDF.Size = new System.Drawing.Size(458, 146);
            this.txtRDF.TabIndex = 1;
            // 
            // btnAddTriples
            // 
            this.btnAddTriples.Location = new System.Drawing.Point(162, 218);
            this.btnAddTriples.Name = "btnAddTriples";
            this.btnAddTriples.Size = new System.Drawing.Size(75, 23);
            this.btnAddTriples.TabIndex = 2;
            this.btnAddTriples.Text = "&Add Triples";
            this.btnAddTriples.UseVisualStyleBackColor = true;
            this.btnAddTriples.Click += new System.EventHandler(this.btnAddTriples_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(243, 218);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(12, 40);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(67, 13);
            this.lblFormat.TabIndex = 4;
            this.lblFormat.Text = "RDF Format:";
            // 
            // cboFormat
            // 
            this.cboFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFormat.FormattingEnabled = true;
            this.cboFormat.Items.AddRange(new object[] {
            "Automatic Format Detection",
            "NTriples",
            "Turtle",
            "Notation 3",
            "RDF/XML",
            "RDF/JSON"});
            this.cboFormat.Location = new System.Drawing.Point(85, 37);
            this.cboFormat.Name = "cboFormat";
            this.cboFormat.Size = new System.Drawing.Size(185, 21);
            this.cboFormat.TabIndex = 5;
            // 
            // fclsAddTriples
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(485, 243);
            this.Controls.Add(this.cboFormat);
            this.Controls.Add(this.lblFormat);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAddTriples);
            this.Controls.Add(this.txtRDF);
            this.Controls.Add(this.lblIntro);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fclsAddTriples";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Triples";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIntro;
        private System.Windows.Forms.TextBox txtRDF;
        private System.Windows.Forms.Button btnAddTriples;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cboFormat;
    }
}