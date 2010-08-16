namespace dotNetRDFVirtuosoSample
{
    partial class fclsGraphBrowser
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
            this.lblDescribe = new System.Windows.Forms.Label();
            this.txtURI = new System.Windows.Forms.TextBox();
            this.btnViewGraph = new System.Windows.Forms.Button();
            this.dvwBrowser = new System.Windows.Forms.DataGridView();
            this.colSubject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPredicate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colObject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dvwBrowser)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDescribe
            // 
            this.lblDescribe.AutoSize = true;
            this.lblDescribe.Location = new System.Drawing.Point(12, 9);
            this.lblDescribe.Name = "lblDescribe";
            this.lblDescribe.Size = new System.Drawing.Size(329, 13);
            this.lblDescribe.TabIndex = 0;
            this.lblDescribe.Text = "Enter a URI whose Graph/SPARQL Description you wish to browse:";
            // 
            // txtURI
            // 
            this.txtURI.Location = new System.Drawing.Point(15, 28);
            this.txtURI.Name = "txtURI";
            this.txtURI.Size = new System.Drawing.Size(573, 20);
            this.txtURI.TabIndex = 1;
            this.txtURI.Text = "http://example.org";
            // 
            // btnViewGraph
            // 
            this.btnViewGraph.Location = new System.Drawing.Point(593, 26);
            this.btnViewGraph.Name = "btnViewGraph";
            this.btnViewGraph.Size = new System.Drawing.Size(70, 23);
            this.btnViewGraph.TabIndex = 2;
            this.btnViewGraph.Text = "&Browse";
            this.btnViewGraph.UseVisualStyleBackColor = true;
            this.btnViewGraph.Click += new System.EventHandler(this.btnViewGraph_Click);
            // 
            // dvwBrowser
            // 
            this.dvwBrowser.AllowUserToAddRows = false;
            this.dvwBrowser.AllowUserToDeleteRows = false;
            this.dvwBrowser.AllowUserToResizeColumns = false;
            this.dvwBrowser.AllowUserToResizeRows = false;
            this.dvwBrowser.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dvwBrowser.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            this.dvwBrowser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvwBrowser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSubject,
            this.colPredicate,
            this.colObject});
            this.dvwBrowser.Location = new System.Drawing.Point(12, 54);
            this.dvwBrowser.MultiSelect = false;
            this.dvwBrowser.Name = "dvwBrowser";
            this.dvwBrowser.ReadOnly = true;
            this.dvwBrowser.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dvwBrowser.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dvwBrowser.Size = new System.Drawing.Size(651, 283);
            this.dvwBrowser.TabIndex = 3;
            this.dvwBrowser.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dvwBrowser_CellDoubleClick);
            // 
            // colSubject
            // 
            this.colSubject.HeaderText = "Subject";
            this.colSubject.Name = "colSubject";
            this.colSubject.ReadOnly = true;
            this.colSubject.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSubject.Width = 49;
            // 
            // colPredicate
            // 
            this.colPredicate.HeaderText = "Predicate";
            this.colPredicate.Name = "colPredicate";
            this.colPredicate.ReadOnly = true;
            this.colPredicate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colPredicate.Width = 58;
            // 
            // colObject
            // 
            this.colObject.HeaderText = "Object";
            this.colObject.Name = "colObject";
            this.colObject.ReadOnly = true;
            this.colObject.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colObject.Width = 44;
            // 
            // fclsGraphBrowser
            // 
            this.AcceptButton = this.btnViewGraph;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 349);
            this.Controls.Add(this.dvwBrowser);
            this.Controls.Add(this.btnViewGraph);
            this.Controls.Add(this.txtURI);
            this.Controls.Add(this.lblDescribe);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "fclsGraphBrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Graph Browser";
            ((System.ComponentModel.ISupportInitialize)(this.dvwBrowser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescribe;
        private System.Windows.Forms.TextBox txtURI;
        private System.Windows.Forms.Button btnViewGraph;
        private System.Windows.Forms.DataGridView dvwBrowser;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubject;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPredicate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObject;
    }
}

