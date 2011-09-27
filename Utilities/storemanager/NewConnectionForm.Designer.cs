namespace VDS.RDF.Utilities.StoreManager
{
    partial class NewConnectionForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstStoreTypes = new System.Windows.Forms.ListBox();
            this.lblDescrip = new System.Windows.Forms.Label();
            this.grpConnectionSettings = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.86521F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 74.13479F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDescrip, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.grpConnectionSettings, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.85019F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.14981F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(549, 267);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lstStoreTypes);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.tableLayoutPanel1.SetRowSpan(this.panel1, 2);
            this.panel1.Size = new System.Drawing.Size(136, 261);
            this.panel1.TabIndex = 0;
            // 
            // lstStoreTypes
            // 
            this.lstStoreTypes.FormattingEnabled = true;
            this.lstStoreTypes.Location = new System.Drawing.Point(3, 9);
            this.lstStoreTypes.Name = "lstStoreTypes";
            this.lstStoreTypes.Size = new System.Drawing.Size(130, 251);
            this.lstStoreTypes.TabIndex = 1;
            this.lstStoreTypes.SelectedIndexChanged += new System.EventHandler(this.lstStoreTypes_SelectedIndexChanged);
            // 
            // lblDescrip
            // 
            this.lblDescrip.Location = new System.Drawing.Point(145, 10);
            this.lblDescrip.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.lblDescrip.Name = "lblDescrip";
            this.lblDescrip.Size = new System.Drawing.Size(395, 40);
            this.lblDescrip.TabIndex = 1;
            this.lblDescrip.Text = "To create a New Connection please select a Store Type from the list on the left h" +
                "and side.";
            // 
            // grpConnectionSettings
            // 
            this.grpConnectionSettings.Location = new System.Drawing.Point(145, 56);
            this.grpConnectionSettings.Name = "grpConnectionSettings";
            this.grpConnectionSettings.Size = new System.Drawing.Size(395, 203);
            this.grpConnectionSettings.TabIndex = 2;
            this.grpConnectionSettings.TabStop = false;
            this.grpConnectionSettings.Text = "Connection Settings";
            // 
            // NewConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 271);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewConnectionForm";
            this.Text = "New Connection";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lstStoreTypes;
        private System.Windows.Forms.Label lblDescrip;
        private System.Windows.Forms.GroupBox grpConnectionSettings;
    }
}