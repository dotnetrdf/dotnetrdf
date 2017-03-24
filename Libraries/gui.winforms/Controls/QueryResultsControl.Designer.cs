/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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
            this.splResults = new System.Windows.Forms.SplitContainer();
            this.resultsViewer = new VDS.RDF.GUI.WinForms.Controls.ResultSetViewerControl();
            this.graphViewer = new VDS.RDF.GUI.WinForms.Controls.GraphViewerControl();
            this.flpToggleButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnToggleQuery = new System.Windows.Forms.Button();
            this.btnToggleResults = new System.Windows.Forms.Button();
            this.flpControls = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDetach = new System.Windows.Forms.Button();
            this.tlpLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splPanel)).BeginInit();
            this.splPanel.Panel1.SuspendLayout();
            this.splPanel.Panel2.SuspendLayout();
            this.splPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splResults)).BeginInit();
            this.splResults.Panel1.SuspendLayout();
            this.splResults.Panel2.SuspendLayout();
            this.splResults.SuspendLayout();
            this.flpToggleButtons.SuspendLayout();
            this.flpControls.SuspendLayout();
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
            this.tlpLayout.Controls.Add(this.flpControls, 1, 0);
            this.tlpLayout.Location = new System.Drawing.Point(3, 3);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 2;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Size = new System.Drawing.Size(528, 360);
            this.tlpLayout.TabIndex = 0;
            // 
            // splPanel
            // 
            this.splPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpLayout.SetColumnSpan(this.splPanel, 2);
            this.splPanel.Location = new System.Drawing.Point(3, 38);
            this.splPanel.Name = "splPanel";
            this.splPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splPanel.Panel1
            // 
            this.splPanel.Panel1.Controls.Add(this.txtQuery);
            // 
            // splPanel.Panel2
            // 
            this.splPanel.Panel2.Controls.Add(this.splResults);
            this.splPanel.Size = new System.Drawing.Size(522, 319);
            this.splPanel.SplitterDistance = 93;
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
            this.txtQuery.Size = new System.Drawing.Size(516, 87);
            this.txtQuery.TabIndex = 0;
            // 
            // splResults
            // 
            this.splResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splResults.Location = new System.Drawing.Point(0, 0);
            this.splResults.Name = "splResults";
            this.splResults.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splResults.Panel1
            // 
            this.splResults.Panel1.Controls.Add(this.resultsViewer);
            // 
            // splResults.Panel2
            // 
            this.splResults.Panel2.Controls.Add(this.graphViewer);
            this.splResults.Size = new System.Drawing.Size(522, 222);
            this.splResults.SplitterDistance = 126;
            this.splResults.TabIndex = 0;
            this.splResults.Visible = false;
            // 
            // resultsViewer
            // 
            this.resultsViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultsViewer.Location = new System.Drawing.Point(3, 3);
            this.resultsViewer.Name = "resultsViewer";
            this.resultsViewer.Size = new System.Drawing.Size(516, 120);
            this.resultsViewer.TabIndex = 0;
            // 
            // graphViewer
            // 
            this.graphViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphViewer.Location = new System.Drawing.Point(3, 3);
            this.graphViewer.Name = "graphViewer";
            this.graphViewer.Size = new System.Drawing.Size(516, 86);
            this.graphViewer.TabIndex = 0;
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
            this.flpToggleButtons.Size = new System.Drawing.Size(258, 29);
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
            // flpControls
            // 
            this.flpControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flpControls.Controls.Add(this.btnClose);
            this.flpControls.Controls.Add(this.btnDetach);
            this.flpControls.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpControls.Location = new System.Drawing.Point(270, 3);
            this.flpControls.Name = "flpControls";
            this.flpControls.Size = new System.Drawing.Size(255, 29);
            this.flpControls.TabIndex = 4;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(177, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDetach
            // 
            this.btnDetach.Location = new System.Drawing.Point(96, 3);
            this.btnDetach.Name = "btnDetach";
            this.btnDetach.Size = new System.Drawing.Size(75, 23);
            this.btnDetach.TabIndex = 4;
            this.btnDetach.Text = "&Detach";
            this.btnDetach.UseVisualStyleBackColor = true;
            this.btnDetach.Click += new System.EventHandler(this.btnDetach_Click);
            // 
            // QueryResultsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpLayout);
            this.Name = "QueryResultsControl";
            this.Size = new System.Drawing.Size(534, 366);
            this.tlpLayout.ResumeLayout(false);
            this.splPanel.Panel1.ResumeLayout(false);
            this.splPanel.Panel1.PerformLayout();
            this.splPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splPanel)).EndInit();
            this.splPanel.ResumeLayout(false);
            this.splResults.Panel1.ResumeLayout(false);
            this.splResults.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splResults)).EndInit();
            this.splResults.ResumeLayout(false);
            this.flpToggleButtons.ResumeLayout(false);
            this.flpControls.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.SplitContainer splPanel;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.FlowLayoutPanel flpToggleButtons;
        private System.Windows.Forms.Button btnToggleQuery;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnToggleResults;
        private System.Windows.Forms.SplitContainer splResults;
        private ResultSetViewerControl resultsViewer;
        private GraphViewerControl graphViewer;
        private System.Windows.Forms.FlowLayoutPanel flpControls;
        private System.Windows.Forms.Button btnDetach;

    }
}
