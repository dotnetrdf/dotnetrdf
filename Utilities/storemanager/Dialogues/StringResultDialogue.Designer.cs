namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    partial class StringResultDialogue
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
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.btnCopyToControl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(12, 12);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResult.Size = new System.Drawing.Size(600, 211);
            this.txtResult.TabIndex = 0;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Location = new System.Drawing.Point(167, 228);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(142, 23);
            this.btnCopyToClipboard.TabIndex = 1;
            this.btnCopyToClipboard.Text = "Copy to Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // btnCopyToControl
            // 
            this.btnCopyToControl.Location = new System.Drawing.Point(315, 228);
            this.btnCopyToControl.Name = "btnCopyToControl";
            this.btnCopyToControl.Size = new System.Drawing.Size(142, 23);
            this.btnCopyToControl.TabIndex = 2;
            this.btnCopyToControl.Text = "Copy to {0}";
            this.btnCopyToControl.UseVisualStyleBackColor = true;
            this.btnCopyToControl.Click += new System.EventHandler(this.btnCopyToControl_Click);
            // 
            // StringResultDialogue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 258);
            this.Controls.Add(this.btnCopyToControl);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.txtResult);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StringResultDialogue";
            this.Text = "{0}";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.Button btnCopyToControl;
    }
}