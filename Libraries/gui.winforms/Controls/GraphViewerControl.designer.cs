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
    partial class GraphViewerControl
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
            this.dgvTriples = new System.Windows.Forms.DataGridView();
            this.btnVisualise = new System.Windows.Forms.Button();
            this.lnkBaseURI = new System.Windows.Forms.LinkLabel();
            this.lblBaseURI = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.fmtSelector = new VDS.RDF.GUI.WinForms.Controls.FormatterControl();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriples)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvTriples
            // 
            this.dgvTriples.AllowUserToAddRows = false;
            this.dgvTriples.AllowUserToDeleteRows = false;
            this.dgvTriples.AllowUserToOrderColumns = true;
            this.dgvTriples.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTriples.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvTriples.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTriples.Location = new System.Drawing.Point(-5, 71);
            this.dgvTriples.Name = "dgvTriples";
            this.dgvTriples.ReadOnly = true;
            this.dgvTriples.Size = new System.Drawing.Size(978, 214);
            this.dgvTriples.TabIndex = 11;
            // 
            // btnVisualise
            // 
            this.btnVisualise.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVisualise.Location = new System.Drawing.Point(895, 30);
            this.btnVisualise.Name = "btnVisualise";
            this.btnVisualise.Size = new System.Drawing.Size(78, 23);
            this.btnVisualise.TabIndex = 13;
            this.btnVisualise.Text = "Visualise";
            this.btnVisualise.UseVisualStyleBackColor = true;
            this.btnVisualise.Click += new System.EventHandler(this.btnVisualise_Click);
            // 
            // lnkBaseURI
            // 
            this.lnkBaseURI.AutoEllipsis = true;
            this.lnkBaseURI.Location = new System.Drawing.Point(66, 11);
            this.lnkBaseURI.Name = "lnkBaseURI";
            this.lnkBaseURI.Size = new System.Drawing.Size(622, 13);
            this.lnkBaseURI.TabIndex = 8;
            // 
            // lblBaseURI
            // 
            this.lblBaseURI.AutoSize = true;
            this.lblBaseURI.Location = new System.Drawing.Point(4, 11);
            this.lblBaseURI.Name = "lblBaseURI";
            this.lblBaseURI.Size = new System.Drawing.Size(56, 13);
            this.lblBaseURI.TabIndex = 7;
            this.lblBaseURI.Text = "Base URI:";
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(814, 30);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 12;
            this.btnExport.Text = "Export Data";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // fmtSelector
            // 
            this.fmtSelector.CurrentFormatter = null;
            this.fmtSelector.DefaultFormatter = null;
            this.fmtSelector.Location = new System.Drawing.Point(7, 30);
            this.fmtSelector.Name = "fmtSelector";
            this.fmtSelector.Size = new System.Drawing.Size(251, 28);
            this.fmtSelector.TabIndex = 14;
            // 
            // GraphViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fmtSelector);
            this.Controls.Add(this.dgvTriples);
            this.Controls.Add(this.btnVisualise);
            this.Controls.Add(this.lnkBaseURI);
            this.Controls.Add(this.lblBaseURI);
            this.Controls.Add(this.btnExport);
            this.Name = "GraphViewerControl";
            this.Size = new System.Drawing.Size(976, 288);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriples)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTriples;
        private System.Windows.Forms.Button btnVisualise;
        private System.Windows.Forms.LinkLabel lnkBaseURI;
        private System.Windows.Forms.Label lblBaseURI;
        private System.Windows.Forms.Button btnExport;
        private FormatterControl fmtSelector;
    }
}
