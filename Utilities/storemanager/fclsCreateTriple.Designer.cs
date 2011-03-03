namespace VDS.RDF.Utilities.StoreManager
{
    partial class fclsCreateTriple
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
            this.lvwTriples = new System.Windows.Forms.ListView();
            this.colSubject = new System.Windows.Forms.ColumnHeader();
            this.colPredicate = new System.Windows.Forms.ColumnHeader();
            this.colObject = new System.Windows.Forms.ColumnHeader();
            this.mnuTriples = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.grpNodeCreator = new System.Windows.Forms.GroupBox();
            this.radURI = new System.Windows.Forms.RadioButton();
            this.txtURI = new System.Windows.Forms.TextBox();
            this.radBaseURI = new System.Windows.Forms.RadioButton();
            this.radQName = new System.Windows.Forms.RadioButton();
            this.txtQName = new System.Windows.Forms.TextBox();
            this.radBlank = new System.Windows.Forms.RadioButton();
            this.radBlankWithID = new System.Windows.Forms.RadioButton();
            this.txtNodeID = new System.Windows.Forms.TextBox();
            this.radPlainLiteral = new System.Windows.Forms.RadioButton();
            this.txtPlainLiteral = new System.Windows.Forms.TextBox();
            this.txtLangLiteral = new System.Windows.Forms.TextBox();
            this.radLangLiteral = new System.Windows.Forms.RadioButton();
            this.lblLangSpec = new System.Windows.Forms.Label();
            this.txtLang = new System.Windows.Forms.TextBox();
            this.txtDatatype = new System.Windows.Forms.TextBox();
            this.txtTypedLiteral = new System.Windows.Forms.TextBox();
            this.radTypedLiteral = new System.Windows.Forms.RadioButton();
            this.lblDatatype = new System.Windows.Forms.Label();
            this.btnSubject = new System.Windows.Forms.Button();
            this.btnPredicate = new System.Windows.Forms.Button();
            this.btnObject = new System.Windows.Forms.Button();
            this.btnNewTriple = new System.Windows.Forms.Button();
            this.btnSaveTriples = new System.Windows.Forms.Button();
            this.mnuTriples.SuspendLayout();
            this.grpNodeCreator.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvwTriples
            // 
            this.lvwTriples.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSubject,
            this.colPredicate,
            this.colObject});
            this.lvwTriples.ContextMenuStrip = this.mnuTriples;
            this.lvwTriples.FullRowSelect = true;
            this.lvwTriples.GridLines = true;
            this.lvwTriples.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvwTriples.Location = new System.Drawing.Point(12, 12);
            this.lvwTriples.MultiSelect = false;
            this.lvwTriples.Name = "lvwTriples";
            this.lvwTriples.Size = new System.Drawing.Size(681, 75);
            this.lvwTriples.TabIndex = 5;
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
            // mnuTriples
            // 
            this.mnuTriples.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDelete});
            this.mnuTriples.Name = "mnuTriples";
            this.mnuTriples.ShowImageMargin = false;
            this.mnuTriples.Size = new System.Drawing.Size(116, 26);
            this.mnuTriples.Opening += new System.ComponentModel.CancelEventHandler(this.mnuTriples_Opening);
            // 
            // mnuDelete
            // 
            this.mnuDelete.Name = "mnuDelete";
            this.mnuDelete.Size = new System.Drawing.Size(115, 22);
            this.mnuDelete.Text = "Delete Triple";
            // 
            // grpNodeCreator
            // 
            this.grpNodeCreator.Controls.Add(this.btnObject);
            this.grpNodeCreator.Controls.Add(this.btnPredicate);
            this.grpNodeCreator.Controls.Add(this.btnSubject);
            this.grpNodeCreator.Controls.Add(this.txtDatatype);
            this.grpNodeCreator.Controls.Add(this.txtTypedLiteral);
            this.grpNodeCreator.Controls.Add(this.radTypedLiteral);
            this.grpNodeCreator.Controls.Add(this.lblDatatype);
            this.grpNodeCreator.Controls.Add(this.txtLang);
            this.grpNodeCreator.Controls.Add(this.txtLangLiteral);
            this.grpNodeCreator.Controls.Add(this.radLangLiteral);
            this.grpNodeCreator.Controls.Add(this.txtPlainLiteral);
            this.grpNodeCreator.Controls.Add(this.radPlainLiteral);
            this.grpNodeCreator.Controls.Add(this.txtNodeID);
            this.grpNodeCreator.Controls.Add(this.radBlankWithID);
            this.grpNodeCreator.Controls.Add(this.radBlank);
            this.grpNodeCreator.Controls.Add(this.txtQName);
            this.grpNodeCreator.Controls.Add(this.radQName);
            this.grpNodeCreator.Controls.Add(this.radBaseURI);
            this.grpNodeCreator.Controls.Add(this.txtURI);
            this.grpNodeCreator.Controls.Add(this.radURI);
            this.grpNodeCreator.Controls.Add(this.lblLangSpec);
            this.grpNodeCreator.Location = new System.Drawing.Point(12, 93);
            this.grpNodeCreator.Name = "grpNodeCreator";
            this.grpNodeCreator.Size = new System.Drawing.Size(681, 251);
            this.grpNodeCreator.TabIndex = 7;
            this.grpNodeCreator.TabStop = false;
            this.grpNodeCreator.Text = "Node Creator";
            // 
            // radURI
            // 
            this.radURI.AutoSize = true;
            this.radURI.Checked = true;
            this.radURI.Location = new System.Drawing.Point(6, 42);
            this.radURI.Name = "radURI";
            this.radURI.Size = new System.Drawing.Size(76, 17);
            this.radURI.TabIndex = 0;
            this.radURI.TabStop = true;
            this.radURI.Text = "URI Node:";
            this.radURI.UseVisualStyleBackColor = true;
            // 
            // txtURI
            // 
            this.txtURI.Location = new System.Drawing.Point(79, 41);
            this.txtURI.Name = "txtURI";
            this.txtURI.Size = new System.Drawing.Size(596, 20);
            this.txtURI.TabIndex = 1;
            this.txtURI.Text = "http://example.org";
            // 
            // radBaseURI
            // 
            this.radBaseURI.AutoSize = true;
            this.radBaseURI.Location = new System.Drawing.Point(6, 19);
            this.radBaseURI.Name = "radBaseURI";
            this.radBaseURI.Size = new System.Drawing.Size(210, 17);
            this.radBaseURI.TabIndex = 2;
            this.radBaseURI.TabStop = true;
            this.radBaseURI.Text = "URI Node referencing Graph Base URI";
            this.radBaseURI.UseVisualStyleBackColor = true;
            // 
            // radQName
            // 
            this.radQName.AutoSize = true;
            this.radQName.Location = new System.Drawing.Point(6, 67);
            this.radQName.Name = "radQName";
            this.radQName.Size = new System.Drawing.Size(138, 17);
            this.radQName.TabIndex = 3;
            this.radQName.TabStop = true;
            this.radQName.Text = "URI Node from QName:";
            this.radQName.UseVisualStyleBackColor = true;
            // 
            // txtQName
            // 
            this.txtQName.Location = new System.Drawing.Point(139, 66);
            this.txtQName.Name = "txtQName";
            this.txtQName.Size = new System.Drawing.Size(152, 20);
            this.txtQName.TabIndex = 4;
            this.txtQName.Text = "rdf:type";
            // 
            // radBlank
            // 
            this.radBlank.AutoSize = true;
            this.radBlank.Location = new System.Drawing.Point(6, 90);
            this.radBlank.Name = "radBlank";
            this.radBlank.Size = new System.Drawing.Size(81, 17);
            this.radBlank.TabIndex = 5;
            this.radBlank.TabStop = true;
            this.radBlank.Text = "Blank Node";
            this.radBlank.UseVisualStyleBackColor = true;
            // 
            // radBlankWithID
            // 
            this.radBlankWithID.AutoSize = true;
            this.radBlankWithID.Location = new System.Drawing.Point(6, 113);
            this.radBlankWithID.Name = "radBlankWithID";
            this.radBlankWithID.Size = new System.Drawing.Size(191, 17);
            this.radBlankWithID.TabIndex = 6;
            this.radBlankWithID.TabStop = true;
            this.radBlankWithID.Text = "Blank Node with User Assigned ID:";
            this.radBlankWithID.UseVisualStyleBackColor = true;
            // 
            // txtNodeID
            // 
            this.txtNodeID.Location = new System.Drawing.Point(194, 112);
            this.txtNodeID.Name = "txtNodeID";
            this.txtNodeID.Size = new System.Drawing.Size(152, 20);
            this.txtNodeID.TabIndex = 7;
            this.txtNodeID.Text = "myNodeID";
            // 
            // radPlainLiteral
            // 
            this.radPlainLiteral.AutoSize = true;
            this.radPlainLiteral.Location = new System.Drawing.Point(6, 136);
            this.radPlainLiteral.Name = "radPlainLiteral";
            this.radPlainLiteral.Size = new System.Drawing.Size(111, 17);
            this.radPlainLiteral.TabIndex = 8;
            this.radPlainLiteral.TabStop = true;
            this.radPlainLiteral.Text = "Plain Literal Node:";
            this.radPlainLiteral.UseVisualStyleBackColor = true;
            // 
            // txtPlainLiteral
            // 
            this.txtPlainLiteral.Location = new System.Drawing.Point(112, 135);
            this.txtPlainLiteral.Name = "txtPlainLiteral";
            this.txtPlainLiteral.Size = new System.Drawing.Size(563, 20);
            this.txtPlainLiteral.TabIndex = 9;
            this.txtPlainLiteral.Text = "Some Text";
            // 
            // txtLangLiteral
            // 
            this.txtLangLiteral.Location = new System.Drawing.Point(161, 161);
            this.txtLangLiteral.Name = "txtLangLiteral";
            this.txtLangLiteral.Size = new System.Drawing.Size(440, 20);
            this.txtLangLiteral.TabIndex = 11;
            this.txtLangLiteral.Text = "Some Text";
            // 
            // radLangLiteral
            // 
            this.radLangLiteral.AutoSize = true;
            this.radLangLiteral.Location = new System.Drawing.Point(6, 162);
            this.radLangLiteral.Name = "radLangLiteral";
            this.radLangLiteral.Size = new System.Drawing.Size(158, 17);
            this.radLangLiteral.TabIndex = 10;
            this.radLangLiteral.TabStop = true;
            this.radLangLiteral.Text = "Literal Node with Language:";
            this.radLangLiteral.UseVisualStyleBackColor = true;
            // 
            // lblLangSpec
            // 
            this.lblLangSpec.Location = new System.Drawing.Point(600, 159);
            this.lblLangSpec.Name = "lblLangSpec";
            this.lblLangSpec.Size = new System.Drawing.Size(25, 23);
            this.lblLangSpec.TabIndex = 12;
            this.lblLangSpec.Text = "@";
            this.lblLangSpec.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtLang
            // 
            this.txtLang.Location = new System.Drawing.Point(622, 161);
            this.txtLang.Name = "txtLang";
            this.txtLang.Size = new System.Drawing.Size(53, 20);
            this.txtLang.TabIndex = 13;
            this.txtLang.Text = "en-US";
            // 
            // txtDatatype
            // 
            this.txtDatatype.Location = new System.Drawing.Point(433, 187);
            this.txtDatatype.Name = "txtDatatype";
            this.txtDatatype.Size = new System.Drawing.Size(242, 20);
            this.txtDatatype.TabIndex = 17;
            this.txtDatatype.Text = "http://www.w3.org/2001/XMLSchema#integer";
            // 
            // txtTypedLiteral
            // 
            this.txtTypedLiteral.Location = new System.Drawing.Point(161, 187);
            this.txtTypedLiteral.Name = "txtTypedLiteral";
            this.txtTypedLiteral.Size = new System.Drawing.Size(252, 20);
            this.txtTypedLiteral.TabIndex = 15;
            this.txtTypedLiteral.Text = "123456";
            // 
            // radTypedLiteral
            // 
            this.radTypedLiteral.AutoSize = true;
            this.radTypedLiteral.Location = new System.Drawing.Point(6, 188);
            this.radTypedLiteral.Name = "radTypedLiteral";
            this.radTypedLiteral.Size = new System.Drawing.Size(158, 17);
            this.radTypedLiteral.TabIndex = 14;
            this.radTypedLiteral.TabStop = true;
            this.radTypedLiteral.Text = "Literal Node with Language:";
            this.radTypedLiteral.UseVisualStyleBackColor = true;
            // 
            // lblDatatype
            // 
            this.lblDatatype.Location = new System.Drawing.Point(410, 184);
            this.lblDatatype.Name = "lblDatatype";
            this.lblDatatype.Size = new System.Drawing.Size(25, 23);
            this.lblDatatype.TabIndex = 16;
            this.lblDatatype.Text = "^^";
            this.lblDatatype.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSubject
            // 
            this.btnSubject.Location = new System.Drawing.Point(174, 214);
            this.btnSubject.Name = "btnSubject";
            this.btnSubject.Size = new System.Drawing.Size(107, 23);
            this.btnSubject.TabIndex = 18;
            this.btnSubject.Text = "Use as &Subject";
            this.btnSubject.UseVisualStyleBackColor = true;
            this.btnSubject.Click += new System.EventHandler(this.btnSubject_Click);
            // 
            // btnPredicate
            // 
            this.btnPredicate.Location = new System.Drawing.Point(287, 213);
            this.btnPredicate.Name = "btnPredicate";
            this.btnPredicate.Size = new System.Drawing.Size(107, 23);
            this.btnPredicate.TabIndex = 19;
            this.btnPredicate.Text = "Use as &Predicate";
            this.btnPredicate.UseVisualStyleBackColor = true;
            // 
            // btnObject
            // 
            this.btnObject.Location = new System.Drawing.Point(400, 213);
            this.btnObject.Name = "btnObject";
            this.btnObject.Size = new System.Drawing.Size(107, 23);
            this.btnObject.TabIndex = 20;
            this.btnObject.Text = "Use as &Object";
            this.btnObject.UseVisualStyleBackColor = true;
            // 
            // btnNewTriple
            // 
            this.btnNewTriple.Location = new System.Drawing.Point(263, 351);
            this.btnNewTriple.Name = "btnNewTriple";
            this.btnNewTriple.Size = new System.Drawing.Size(87, 23);
            this.btnNewTriple.TabIndex = 8;
            this.btnNewTriple.Text = "&New Triple";
            this.btnNewTriple.UseVisualStyleBackColor = true;
            this.btnNewTriple.Click += new System.EventHandler(this.btnNewTriple_Click);
            // 
            // btnSaveTriples
            // 
            this.btnSaveTriples.Location = new System.Drawing.Point(355, 351);
            this.btnSaveTriples.Name = "btnSaveTriples";
            this.btnSaveTriples.Size = new System.Drawing.Size(87, 23);
            this.btnSaveTriples.TabIndex = 9;
            this.btnSaveTriples.Text = "Save Triples";
            this.btnSaveTriples.UseVisualStyleBackColor = true;
            // 
            // fclsCreateTriple
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 377);
            this.Controls.Add(this.btnSaveTriples);
            this.Controls.Add(this.btnNewTriple);
            this.Controls.Add(this.lvwTriples);
            this.Controls.Add(this.grpNodeCreator);
            this.Name = "fclsCreateTriple";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Triple(s)";
            this.mnuTriples.ResumeLayout(false);
            this.grpNodeCreator.ResumeLayout(false);
            this.grpNodeCreator.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvwTriples;
        private System.Windows.Forms.ColumnHeader colSubject;
        private System.Windows.Forms.ColumnHeader colPredicate;
        private System.Windows.Forms.ColumnHeader colObject;
        private System.Windows.Forms.ContextMenuStrip mnuTriples;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.GroupBox grpNodeCreator;
        private System.Windows.Forms.RadioButton radBaseURI;
        private System.Windows.Forms.TextBox txtURI;
        private System.Windows.Forms.RadioButton radURI;
        private System.Windows.Forms.TextBox txtQName;
        private System.Windows.Forms.RadioButton radQName;
        private System.Windows.Forms.TextBox txtNodeID;
        private System.Windows.Forms.RadioButton radBlankWithID;
        private System.Windows.Forms.RadioButton radBlank;
        private System.Windows.Forms.TextBox txtLang;
        private System.Windows.Forms.TextBox txtLangLiteral;
        private System.Windows.Forms.RadioButton radLangLiteral;
        private System.Windows.Forms.TextBox txtPlainLiteral;
        private System.Windows.Forms.RadioButton radPlainLiteral;
        private System.Windows.Forms.Label lblLangSpec;
        private System.Windows.Forms.TextBox txtDatatype;
        private System.Windows.Forms.TextBox txtTypedLiteral;
        private System.Windows.Forms.RadioButton radTypedLiteral;
        private System.Windows.Forms.Label lblDatatype;
        private System.Windows.Forms.Button btnObject;
        private System.Windows.Forms.Button btnPredicate;
        private System.Windows.Forms.Button btnSubject;
        private System.Windows.Forms.Button btnNewTriple;
        private System.Windows.Forms.Button btnSaveTriples;
    }
}