namespace VDS.RDF.Utilities.StoreManager
{
    partial class TaskErrorTraceForm<T>
        where T : class
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
            this.txtErrorTrace = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtErrorTrace
            // 
            this.txtErrorTrace.Location = new System.Drawing.Point(12, 12);
            this.txtErrorTrace.Multiline = true;
            this.txtErrorTrace.Name = "txtErrorTrace";
            this.txtErrorTrace.ReadOnly = true;
            this.txtErrorTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtErrorTrace.Size = new System.Drawing.Size(497, 168);
            this.txtErrorTrace.TabIndex = 0;
            this.txtErrorTrace.TabStop = false;
            this.txtErrorTrace.WordWrap = false;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(434, 186);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // fclsTaskErrorTrace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(521, 212);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtErrorTrace);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fclsTaskErrorTrace";
            this.Text = "Error Trace - {0} on {1}";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtErrorTrace;
        private System.Windows.Forms.Button btnClose;
    }
}