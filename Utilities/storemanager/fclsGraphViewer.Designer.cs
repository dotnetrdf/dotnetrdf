namespace dotNetRDFStore
{
    partial class fclsGraphViewer
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
            this.lvwTriples = new System.Windows.Forms.ListView();
            this.colSubject = new System.Windows.Forms.ColumnHeader();
            this.colPredicate = new System.Windows.Forms.ColumnHeader();
            this.colObject = new System.Windows.Forms.ColumnHeader();
            this.btnExport = new System.Windows.Forms.Button();
            this.lblBaseURI = new System.Windows.Forms.Label();
            this.lnkBaseURI = new System.Windows.Forms.LinkLabel();
            this.btnVisualise = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lvwTriples
            // 
            this.lvwTriples.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvwTriples.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSubject,
            this.colPredicate,
            this.colObject});
            this.lvwTriples.FullRowSelect = true;
            this.lvwTriples.GridLines = true;
            this.lvwTriples.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvwTriples.Location = new System.Drawing.Point(15, 31);
            this.lvwTriples.Name = "lvwTriples";
            this.lvwTriples.Size = new System.Drawing.Size(681, 309);
            this.lvwTriples.TabIndex = 2;
            this.lvwTriples.UseCompatibleStateImageBehavior = false;
            this.lvwTriples.View = System.Windows.Forms.View.Details;
            // 
            // colSubject
            // 
            this.colSubject.Text = "Subject";
            this.colSubject.Width = 225;
            // 
            // colPredicate
            // 
            this.colPredicate.Text = "Predicate";
            this.colPredicate.Width = 225;
            // 
            // colObject
            // 
            this.colObject.Text = "Object";
            this.colObject.Width = 225;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(275, 346);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Export Data";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // lblBaseURI
            // 
            this.lblBaseURI.AutoSize = true;
            this.lblBaseURI.Location = new System.Drawing.Point(12, 9);
            this.lblBaseURI.Name = "lblBaseURI";
            this.lblBaseURI.Size = new System.Drawing.Size(56, 13);
            this.lblBaseURI.TabIndex = 0;
            this.lblBaseURI.Text = "Base URI:";
            // 
            // lnkBaseURI
            // 
            this.lnkBaseURI.AutoEllipsis = true;
            this.lnkBaseURI.Location = new System.Drawing.Point(74, 9);
            this.lnkBaseURI.Name = "lnkBaseURI";
            this.lnkBaseURI.Size = new System.Drawing.Size(622, 13);
            this.lnkBaseURI.TabIndex = 1;
            // 
            // btnVisualise
            // 
            this.btnVisualise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVisualise.Location = new System.Drawing.Point(356, 346);
            this.btnVisualise.Name = "btnVisualise";
            this.btnVisualise.Size = new System.Drawing.Size(78, 23);
            this.btnVisualise.TabIndex = 4;
            this.btnVisualise.Text = "Visualise";
            this.btnVisualise.UseVisualStyleBackColor = true;
            this.btnVisualise.Click += new System.EventHandler(this.btnVisualise_Click);
            // 
            // fclsGraphViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 375);
            this.Controls.Add(this.btnVisualise);
            this.Controls.Add(this.lnkBaseURI);
            this.Controls.Add(this.lblBaseURI);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.lvwTriples);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "fclsGraphViewer";
            this.Text = "dotNetRDF Store Manager - Graph Viewer";
            this.Load += new System.EventHandler(this.fclsGraphManager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.ListView lvwTriples;
        private System.Windows.Forms.ColumnHeader colSubject;
        private System.Windows.Forms.ColumnHeader colPredicate;
        private System.Windows.Forms.ColumnHeader colObject;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label lblBaseURI;
        private System.Windows.Forms.LinkLabel lnkBaseURI;
        private System.Windows.Forms.Button btnVisualise;
    }
}