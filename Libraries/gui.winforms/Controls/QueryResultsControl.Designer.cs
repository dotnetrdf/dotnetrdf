namespace VDS.RDF.GUI.WinForms.Controls
{
    partial class QueryResultsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.splPanel = new System.Windows.Forms.SplitContainer();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.flpToggleButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleQuery = new System.Windows.Forms.Button();
            this.btnToggleResults = new System.Windows.Forms.Button();
            this.flpControlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.tlpLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splPanel)).BeginInit();
            this.splPanel.Panel1.SuspendLayout();
            this.splPanel.SuspendLayout();
            this.flpToggleButtons.SuspendLayout();
            this.flpControlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpLayout
            // 
            this.tlpLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpLayout.ColumnCount = 2;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpLayout.Controls.Add(this.splPanel, 0, 1);
            this.tlpLayout.Controls.Add(this.flpToggleButtons, 0, 0);
            this.tlpLayout.Controls.Add(this.flpControlButtons, 1, 0);
            this.tlpLayout.Location = new System.Drawing.Point(3, 3);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 3;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.61947F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.38053F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLayout.Size = new System.Drawing.Size(837, 360);
            this.tlpLayout.TabIndex = 0;
            // 
            // splPanel
            // 
            this.splPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpLayout.SetColumnSpan(this.splPanel, 2);
            this.splPanel.Location = new System.Drawing.Point(3, 39);
            this.splPanel.Name = "splPanel";
            this.splPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splPanel.Panel1
            // 
            this.splPanel.Panel1.Controls.Add(this.txtQuery);
            this.splPanel.Size = new System.Drawing.Size(831, 297);
            this.splPanel.SplitterDistance = 87;
            this.splPanel.TabIndex = 0;
            // 
            // txtQuery
            // 
            this.txtQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuery.Location = new System.Drawing.Point(3, 3);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ReadOnly = true;
            this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQuery.Size = new System.Drawing.Size(825, 81);
            this.txtQuery.TabIndex = 0;
            // 
            // flpToggleButtons
            // 
            this.flpToggleButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpToggleButtons.Controls.Add(this.btnToggleQuery);
            this.flpToggleButtons.Controls.Add(this.btnToggleResults);
            this.flpToggleButtons.Location = new System.Drawing.Point(3, 3);
            this.flpToggleButtons.Name = "flpToggleButtons";
            this.flpToggleButtons.Size = new System.Drawing.Size(412, 30);
            this.flpToggleButtons.TabIndex = 3;
            // 
            // btnToggleQuery
            // 
            this.btnToggleQuery.Location = new System.Drawing.Point(3, 3);
            this.btnToggleQuery.Name = "btnToggleQuery";
            this.btnToggleQuery.Size = new System.Drawing.Size(91, 23);
            this.btnToggleQuery.TabIndex = 4;
            this.btnToggleQuery.Text = "Show &Query";
            this.btnToggleQuery.UseVisualStyleBackColor = true;
            this.btnToggleQuery.Click += new System.EventHandler(this.btnToggleQuery_Click);
            // 
            // btnToggleResults
            // 
            this.btnToggleResults.Location = new System.Drawing.Point(100, 3);
            this.btnToggleResults.Name = "btnToggleResults";
            this.btnToggleResults.Size = new System.Drawing.Size(91, 23);
            this.btnToggleResults.TabIndex = 5;
            this.btnToggleResults.Text = "Hide &Results";
            this.btnToggleResults.UseVisualStyleBackColor = true;
            this.btnToggleResults.Click += new System.EventHandler(this.btnToggleResults_Click);
            // 
            // flpControlButtons
            // 
            this.flpControlButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpControlButtons.Controls.Add(this.btnClose);
            this.flpControlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpControlButtons.Location = new System.Drawing.Point(421, 3);
            this.flpControlButtons.Name = "flpControlButtons";
            this.flpControlButtons.Size = new System.Drawing.Size(413, 30);
            this.flpControlButtons.TabIndex = 4;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(335, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // QueryResultsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpLayout);
            this.Name = "QueryResultsControl";
            this.Size = new System.Drawing.Size(843, 366);
            this.tlpLayout.ResumeLayout(false);
            this.splPanel.Panel1.ResumeLayout(false);
            this.splPanel.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splPanel)).EndInit();
            this.splPanel.ResumeLayout(false);
            this.flpToggleButtons.ResumeLayout(false);
            this.flpControlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.SplitContainer splPanel;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.FlowLayoutPanel flpToggleButtons;
        private System.Windows.Forms.Button btnToggleQuery;
        private System.Windows.Forms.FlowLayoutPanel flpControlButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnToggleResults;

    }
}
