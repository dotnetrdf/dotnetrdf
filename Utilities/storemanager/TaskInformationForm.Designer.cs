/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace VDS.RDF.Utilities.StoreManager
{
    partial class TaskInformationForm<T> where T : class
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
            this.lblTaskState = new System.Windows.Forms.Label();
            this.lblElapsed = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnErrorTrace = new System.Windows.Forms.Button();
            this.tabInfo = new System.Windows.Forms.TabControl();
            this.tabBasicInfo = new System.Windows.Forms.TabPage();
            this.txtBasicInfo = new System.Windows.Forms.TextBox();
            this.tabAdvInfo = new System.Windows.Forms.TabPage();
            this.txtAdvInfo = new System.Windows.Forms.TextBox();
            this.btnViewResults = new System.Windows.Forms.Button();
            this.tabInfo.SuspendLayout();
            this.tabBasicInfo.SuspendLayout();
            this.tabAdvInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTaskState
            // 
            this.lblTaskState.Location = new System.Drawing.Point(1, 9);
            this.lblTaskState.Name = "lblTaskState";
            this.lblTaskState.Size = new System.Drawing.Size(305, 14);
            this.lblTaskState.TabIndex = 0;
            this.lblTaskState.Text = "Task State: {0}";
            // 
            // lblElapsed
            // 
            this.lblElapsed.Location = new System.Drawing.Point(1, 32);
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new System.Drawing.Size(305, 17);
            this.lblElapsed.TabIndex = 2;
            this.lblElapsed.Text = "Time Elapsed: {0}";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(72, 201);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(97, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnErrorTrace
            // 
            this.btnErrorTrace.Location = new System.Drawing.Point(278, 201);
            this.btnErrorTrace.Name = "btnErrorTrace";
            this.btnErrorTrace.Size = new System.Drawing.Size(97, 23);
            this.btnErrorTrace.TabIndex = 4;
            this.btnErrorTrace.Text = "View &Error Trace";
            this.btnErrorTrace.UseVisualStyleBackColor = true;
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.tabBasicInfo);
            this.tabInfo.Controls.Add(this.tabAdvInfo);
            this.tabInfo.Location = new System.Drawing.Point(4, 52);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.SelectedIndex = 0;
            this.tabInfo.Size = new System.Drawing.Size(438, 143);
            this.tabInfo.TabIndex = 6;
            // 
            // tabBasicInfo
            // 
            this.tabBasicInfo.Controls.Add(this.txtBasicInfo);
            this.tabBasicInfo.Location = new System.Drawing.Point(4, 22);
            this.tabBasicInfo.Name = "tabBasicInfo";
            this.tabBasicInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabBasicInfo.Size = new System.Drawing.Size(430, 117);
            this.tabBasicInfo.TabIndex = 0;
            this.tabBasicInfo.Text = "Basic Information";
            this.tabBasicInfo.UseVisualStyleBackColor = true;
            // 
            // txtBasicInfo
            // 
            this.txtBasicInfo.Location = new System.Drawing.Point(6, 6);
            this.txtBasicInfo.Multiline = true;
            this.txtBasicInfo.Name = "txtBasicInfo";
            this.txtBasicInfo.ReadOnly = true;
            this.txtBasicInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBasicInfo.Size = new System.Drawing.Size(418, 105);
            this.txtBasicInfo.TabIndex = 6;
            // 
            // tabAdvInfo
            // 
            this.tabAdvInfo.Controls.Add(this.txtAdvInfo);
            this.tabAdvInfo.Location = new System.Drawing.Point(4, 22);
            this.tabAdvInfo.Name = "tabAdvInfo";
            this.tabAdvInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvInfo.Size = new System.Drawing.Size(430, 117);
            this.tabAdvInfo.TabIndex = 1;
            this.tabAdvInfo.Text = "Advanced Information";
            this.tabAdvInfo.UseVisualStyleBackColor = true;
            // 
            // txtAdvInfo
            // 
            this.txtAdvInfo.Location = new System.Drawing.Point(6, 6);
            this.txtAdvInfo.Multiline = true;
            this.txtAdvInfo.Name = "txtAdvInfo";
            this.txtAdvInfo.ReadOnly = true;
            this.txtAdvInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAdvInfo.Size = new System.Drawing.Size(418, 105);
            this.txtAdvInfo.TabIndex = 7;
            // 
            // btnViewResults
            // 
            this.btnViewResults.Location = new System.Drawing.Point(175, 201);
            this.btnViewResults.Name = "btnViewResults";
            this.btnViewResults.Size = new System.Drawing.Size(97, 23);
            this.btnViewResults.TabIndex = 7;
            this.btnViewResults.Text = "View &Results";
            this.btnViewResults.UseVisualStyleBackColor = true;
            // 
            // TaskInformationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 232);
            this.Controls.Add(this.btnViewResults);
            this.Controls.Add(this.tabInfo);
            this.Controls.Add(this.btnErrorTrace);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblElapsed);
            this.Controls.Add(this.lblTaskState);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TaskInformationForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "{0}";
            this.tabInfo.ResumeLayout(false);
            this.tabBasicInfo.ResumeLayout(false);
            this.tabBasicInfo.PerformLayout();
            this.tabAdvInfo.ResumeLayout(false);
            this.tabAdvInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTaskState;
        private System.Windows.Forms.Label lblElapsed;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnErrorTrace;
        private System.Windows.Forms.TabControl tabInfo;
        private System.Windows.Forms.TabPage tabBasicInfo;
        private System.Windows.Forms.TabPage tabAdvInfo;
        private System.Windows.Forms.TextBox txtBasicInfo;
        private System.Windows.Forms.TextBox txtAdvInfo;
        private System.Windows.Forms.Button btnViewResults;
    }
}