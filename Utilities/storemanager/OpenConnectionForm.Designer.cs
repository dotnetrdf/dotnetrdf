/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

namespace VDS.RDF.Utilities.StoreManager
{
    partial class OpenConnectionForm
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
            this.lstConnections = new System.Windows.Forms.ListBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblIntro
            // 
            this.lblIntro.Location = new System.Drawing.Point(10, 9);
            this.lblIntro.Name = "lblIntro";
            this.lblIntro.Size = new System.Drawing.Size(360, 38);
            this.lblIntro.TabIndex = 0;
            this.lblIntro.Text = "The selected connection file contains the following connections.  Select a connec" +
                "tion from the list and click Open to attempt to open that connection:";
            // 
            // lstConnections
            // 
            this.lstConnections.FormattingEnabled = true;
            this.lstConnections.HorizontalScrollbar = true;
            this.lstConnections.Location = new System.Drawing.Point(10, 41);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(360, 121);
            this.lstConnections.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(111, 168);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.Text = "&Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(192, 168);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // OpenConnectionForm
            // 
            this.AcceptButton = this.btnOpen;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(379, 197);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lstConnections);
            this.Controls.Add(this.lblIntro);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpenConnectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Connection";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblIntro;
        private System.Windows.Forms.ListBox lstConnections;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnCancel;
    }
}