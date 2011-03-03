namespace VDS.RDF.Utilities.StoreManager
{
    partial class fclsSQLGraphManager
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
            this.lblGraphURI = new System.Windows.Forms.Label();
            this.lnkGraphURI = new System.Windows.Forms.LinkLabel();
            this.lblLoadStatus = new System.Windows.Forms.Label();
            this.lblLoadInfo = new System.Windows.Forms.Label();
            this.lvwTriples = new System.Windows.Forms.ListView();
            this.colSubject = new System.Windows.Forms.ColumnHeader();
            this.colPredicate = new System.Windows.Forms.ColumnHeader();
            this.colObject = new System.Windows.Forms.ColumnHeader();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnAddTriples = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblGraphURI
            // 
            this.lblGraphURI.AutoSize = true;
            this.lblGraphURI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGraphURI.Location = new System.Drawing.Point(12, 9);
            this.lblGraphURI.Name = "lblGraphURI";
            this.lblGraphURI.Size = new System.Drawing.Size(84, 13);
            this.lblGraphURI.TabIndex = 0;
            this.lblGraphURI.Text = "Selected Graph:";
            // 
            // lnkGraphURI
            // 
            this.lnkGraphURI.AutoEllipsis = true;
            this.lnkGraphURI.Location = new System.Drawing.Point(93, 9);
            this.lnkGraphURI.Name = "lnkGraphURI";
            this.lnkGraphURI.Size = new System.Drawing.Size(603, 13);
            this.lnkGraphURI.TabIndex = 1;
            this.lnkGraphURI.TabStop = true;
            this.lnkGraphURI.Text = "linkLabel1";
            // 
            // lblLoadStatus
            // 
            this.lblLoadStatus.AutoSize = true;
            this.lblLoadStatus.Location = new System.Drawing.Point(12, 32);
            this.lblLoadStatus.Name = "lblLoadStatus";
            this.lblLoadStatus.Size = new System.Drawing.Size(67, 13);
            this.lblLoadStatus.TabIndex = 2;
            this.lblLoadStatus.Text = "Load Status:";
            // 
            // lblLoadInfo
            // 
            this.lblLoadInfo.AutoSize = true;
            this.lblLoadInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLoadInfo.Location = new System.Drawing.Point(93, 32);
            this.lblLoadInfo.Name = "lblLoadInfo";
            this.lblLoadInfo.Size = new System.Drawing.Size(54, 13);
            this.lblLoadInfo.TabIndex = 3;
            this.lblLoadInfo.Text = "Loading...";
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
            this.lvwTriples.Location = new System.Drawing.Point(15, 48);
            this.lvwTriples.Name = "lvwTriples";
            this.lvwTriples.Size = new System.Drawing.Size(681, 292);
            this.lvwTriples.TabIndex = 4;
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
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(236, 346);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnAddTriples
            // 
            this.btnAddTriples.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddTriples.Location = new System.Drawing.Point(317, 346);
            this.btnAddTriples.Name = "btnAddTriples";
            this.btnAddTriples.Size = new System.Drawing.Size(75, 23);
            this.btnAddTriples.TabIndex = 6;
            this.btnAddTriples.Text = "Add Triples";
            this.btnAddTriples.UseVisualStyleBackColor = true;
            this.btnAddTriples.Click += new System.EventHandler(this.btnAddTriples_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(398, 346);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "Export Data";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // fclsSQLGraphManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 375);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnAddTriples);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lvwTriples);
            this.Controls.Add(this.lblLoadInfo);
            this.Controls.Add(this.lblLoadStatus);
            this.Controls.Add(this.lnkGraphURI);
            this.Controls.Add(this.lblGraphURI);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "fclsSQLGraphManager";
            this.Text = "dotNetRDF Store Manager - Graph Manager";
            this.Load += new System.EventHandler(this.fclsGraphManager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Label lblGraphURI;
        private System.Windows.Forms.LinkLabel lnkGraphURI;
        private System.Windows.Forms.Label lblLoadStatus;
        private System.Windows.Forms.Label lblLoadInfo;
        private System.Windows.Forms.ListView lvwTriples;
        private System.Windows.Forms.ColumnHeader colSubject;
        private System.Windows.Forms.ColumnHeader colPredicate;
        private System.Windows.Forms.ColumnHeader colObject;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddTriples;
        private System.Windows.Forms.Button btnExport;
    }
}