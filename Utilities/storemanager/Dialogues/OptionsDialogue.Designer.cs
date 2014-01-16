/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    partial class OptionsDialogue
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
            this.btnSave = new System.Windows.Forms.Button();
            this.btnResetDefault = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStartUpOptions = new System.Windows.Forms.TabPage();
            this.grpActiveConnectionOptions = new System.Windows.Forms.GroupBox();
            this.chkAlwaysRestoreActiveConnections = new System.Windows.Forms.CheckBox();
            this.chkPromptRestoreActiveConnections = new System.Windows.Forms.CheckBox();
            this.chkShowStartPage = new System.Windows.Forms.CheckBox();
            this.tabEditorOptions = new System.Windows.Forms.TabPage();
            this.chkEditorWordWrap = new System.Windows.Forms.CheckBox();
            this.chkEditorDetectUrls = new System.Windows.Forms.CheckBox();
            this.chkEditorHighlighting = new System.Windows.Forms.CheckBox();
            this.tabOtherOptions = new System.Windows.Forms.TabPage();
            this.numMaxRecentConnections = new System.Windows.Forms.NumericUpDown();
            this.lblMaxRecentConnections = new System.Windows.Forms.Label();
            this.numPreviewSize = new System.Windows.Forms.NumericUpDown();
            this.lblPreviewSize = new System.Windows.Forms.Label();
            this.chkAlwaysEdit = new System.Windows.Forms.CheckBox();
            this.chkUtf8Bom = new System.Windows.Forms.CheckBox();
            this.btnResetCurrent = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabStartUpOptions.SuspendLayout();
            this.grpActiveConnectionOptions.SuspendLayout();
            this.tabEditorOptions.SuspendLayout();
            this.tabOtherOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRecentConnections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviewSize)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(64, 179);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(110, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "&Save Changes";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnResetDefault
            // 
            this.btnResetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetDefault.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnResetDefault.Location = new System.Drawing.Point(296, 179);
            this.btnResetDefault.Name = "btnResetDefault";
            this.btnResetDefault.Size = new System.Drawing.Size(110, 23);
            this.btnResetDefault.TabIndex = 1;
            this.btnResetDefault.Text = "Reset to &Defaults";
            this.btnResetDefault.UseVisualStyleBackColor = true;
            this.btnResetDefault.Click += new System.EventHandler(this.btnResetDefault_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(412, 179);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabStartUpOptions);
            this.tabControl1.Controls.Add(this.tabEditorOptions);
            this.tabControl1.Controls.Add(this.tabOtherOptions);
            this.tabControl1.Location = new System.Drawing.Point(1, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(580, 167);
            this.tabControl1.TabIndex = 3;
            // 
            // tabStartUpOptions
            // 
            this.tabStartUpOptions.Controls.Add(this.grpActiveConnectionOptions);
            this.tabStartUpOptions.Controls.Add(this.chkShowStartPage);
            this.tabStartUpOptions.Location = new System.Drawing.Point(4, 22);
            this.tabStartUpOptions.Name = "tabStartUpOptions";
            this.tabStartUpOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabStartUpOptions.Size = new System.Drawing.Size(572, 141);
            this.tabStartUpOptions.TabIndex = 0;
            this.tabStartUpOptions.Text = "Start Up Options";
            this.tabStartUpOptions.UseVisualStyleBackColor = true;
            // 
            // grpActiveConnectionOptions
            // 
            this.grpActiveConnectionOptions.Controls.Add(this.chkAlwaysRestoreActiveConnections);
            this.grpActiveConnectionOptions.Controls.Add(this.chkPromptRestoreActiveConnections);
            this.grpActiveConnectionOptions.Location = new System.Drawing.Point(7, 29);
            this.grpActiveConnectionOptions.Name = "grpActiveConnectionOptions";
            this.grpActiveConnectionOptions.Size = new System.Drawing.Size(559, 65);
            this.grpActiveConnectionOptions.TabIndex = 1;
            this.grpActiveConnectionOptions.TabStop = false;
            this.grpActiveConnectionOptions.Text = "Active Connection Management";
            // 
            // chkAlwaysRestoreActiveConnections
            // 
            this.chkAlwaysRestoreActiveConnections.AutoSize = true;
            this.chkAlwaysRestoreActiveConnections.Location = new System.Drawing.Point(6, 42);
            this.chkAlwaysRestoreActiveConnections.Name = "chkAlwaysRestoreActiveConnections";
            this.chkAlwaysRestoreActiveConnections.Size = new System.Drawing.Size(473, 17);
            this.chkAlwaysRestoreActiveConnections.TabIndex = 1;
            this.chkAlwaysRestoreActiveConnections.Text = "Always restore active connections on next start up when exiting (even if promptin" +
    "g is disabled)?";
            this.chkAlwaysRestoreActiveConnections.UseVisualStyleBackColor = true;
            // 
            // chkPromptRestoreActiveConnections
            // 
            this.chkPromptRestoreActiveConnections.AutoSize = true;
            this.chkPromptRestoreActiveConnections.Location = new System.Drawing.Point(6, 19);
            this.chkPromptRestoreActiveConnections.Name = "chkPromptRestoreActiveConnections";
            this.chkPromptRestoreActiveConnections.Size = new System.Drawing.Size(448, 17);
            this.chkPromptRestoreActiveConnections.TabIndex = 0;
            this.chkPromptRestoreActiveConnections.Text = "Prompt me for whether I want to restore active connections on next start up when " +
    "exiting?";
            this.chkPromptRestoreActiveConnections.UseVisualStyleBackColor = true;
            // 
            // chkShowStartPage
            // 
            this.chkShowStartPage.AutoSize = true;
            this.chkShowStartPage.Location = new System.Drawing.Point(7, 6);
            this.chkShowStartPage.Name = "chkShowStartPage";
            this.chkShowStartPage.Size = new System.Drawing.Size(199, 17);
            this.chkShowStartPage.TabIndex = 0;
            this.chkShowStartPage.Text = "Always show Start Page on start up?";
            this.chkShowStartPage.UseVisualStyleBackColor = true;
            // 
            // tabEditorOptions
            // 
            this.tabEditorOptions.Controls.Add(this.chkEditorWordWrap);
            this.tabEditorOptions.Controls.Add(this.chkEditorDetectUrls);
            this.tabEditorOptions.Controls.Add(this.chkEditorHighlighting);
            this.tabEditorOptions.Location = new System.Drawing.Point(4, 22);
            this.tabEditorOptions.Name = "tabEditorOptions";
            this.tabEditorOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabEditorOptions.Size = new System.Drawing.Size(572, 141);
            this.tabEditorOptions.TabIndex = 1;
            this.tabEditorOptions.Text = "Editor Options";
            this.tabEditorOptions.UseVisualStyleBackColor = true;
            // 
            // chkEditorWordWrap
            // 
            this.chkEditorWordWrap.AutoSize = true;
            this.chkEditorWordWrap.Location = new System.Drawing.Point(7, 52);
            this.chkEditorWordWrap.Name = "chkEditorWordWrap";
            this.chkEditorWordWrap.Size = new System.Drawing.Size(123, 17);
            this.chkEditorWordWrap.TabIndex = 2;
            this.chkEditorWordWrap.Text = "Enable Word Wrap?";
            this.chkEditorWordWrap.UseVisualStyleBackColor = true;
            // 
            // chkEditorDetectUrls
            // 
            this.chkEditorDetectUrls.AutoSize = true;
            this.chkEditorDetectUrls.Location = new System.Drawing.Point(7, 29);
            this.chkEditorDetectUrls.Name = "chkEditorDetectUrls";
            this.chkEditorDetectUrls.Size = new System.Drawing.Size(214, 17);
            this.chkEditorDetectUrls.TabIndex = 1;
            this.chkEditorDetectUrls.Text = "Enable URL detection and highlighting?";
            this.chkEditorDetectUrls.UseVisualStyleBackColor = true;
            // 
            // chkEditorHighlighting
            // 
            this.chkEditorHighlighting.AutoSize = true;
            this.chkEditorHighlighting.Location = new System.Drawing.Point(7, 6);
            this.chkEditorHighlighting.Name = "chkEditorHighlighting";
            this.chkEditorHighlighting.Size = new System.Drawing.Size(231, 17);
            this.chkEditorHighlighting.TabIndex = 0;
            this.chkEditorHighlighting.Text = "Enable SPARQL Query syntax highlighting?";
            this.chkEditorHighlighting.UseVisualStyleBackColor = true;
            // 
            // tabOtherOptions
            // 
            this.tabOtherOptions.Controls.Add(this.numMaxRecentConnections);
            this.tabOtherOptions.Controls.Add(this.lblMaxRecentConnections);
            this.tabOtherOptions.Controls.Add(this.numPreviewSize);
            this.tabOtherOptions.Controls.Add(this.lblPreviewSize);
            this.tabOtherOptions.Controls.Add(this.chkAlwaysEdit);
            this.tabOtherOptions.Controls.Add(this.chkUtf8Bom);
            this.tabOtherOptions.Location = new System.Drawing.Point(4, 22);
            this.tabOtherOptions.Name = "tabOtherOptions";
            this.tabOtherOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOtherOptions.Size = new System.Drawing.Size(572, 141);
            this.tabOtherOptions.TabIndex = 2;
            this.tabOtherOptions.Text = "Other Options";
            this.tabOtherOptions.UseVisualStyleBackColor = true;
            // 
            // numMaxRecentConnections
            // 
            this.numMaxRecentConnections.Location = new System.Drawing.Point(314, 69);
            this.numMaxRecentConnections.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numMaxRecentConnections.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxRecentConnections.Name = "numMaxRecentConnections";
            this.numMaxRecentConnections.Size = new System.Drawing.Size(40, 20);
            this.numMaxRecentConnections.TabIndex = 5;
            this.numMaxRecentConnections.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // lblMaxRecentConnections
            // 
            this.lblMaxRecentConnections.AutoSize = true;
            this.lblMaxRecentConnections.Location = new System.Drawing.Point(4, 71);
            this.lblMaxRecentConnections.Name = "lblMaxRecentConnections";
            this.lblMaxRecentConnections.Size = new System.Drawing.Size(304, 13);
            this.lblMaxRecentConnections.TabIndex = 4;
            this.lblMaxRecentConnections.Text = "Maximum Recent Connections to remember (applies on restart):";
            // 
            // numPreviewSize
            // 
            this.numPreviewSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numPreviewSize.Location = new System.Drawing.Point(269, 47);
            this.numPreviewSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPreviewSize.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numPreviewSize.Name = "numPreviewSize";
            this.numPreviewSize.Size = new System.Drawing.Size(62, 20);
            this.numPreviewSize.TabIndex = 3;
            this.numPreviewSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblPreviewSize
            // 
            this.lblPreviewSize.AutoSize = true;
            this.lblPreviewSize.Location = new System.Drawing.Point(4, 49);
            this.lblPreviewSize.Name = "lblPreviewSize";
            this.lblPreviewSize.Size = new System.Drawing.Size(259, 13);
            this.lblPreviewSize.TabIndex = 2;
            this.lblPreviewSize.Text = "Preview size for previewing graphs (number of triples):";
            // 
            // chkAlwaysEdit
            // 
            this.chkAlwaysEdit.AutoSize = true;
            this.chkAlwaysEdit.Location = new System.Drawing.Point(7, 29);
            this.chkAlwaysEdit.Name = "chkAlwaysEdit";
            this.chkAlwaysEdit.Size = new System.Drawing.Size(540, 17);
            this.chkAlwaysEdit.TabIndex = 1;
            this.chkAlwaysEdit.Text = "Always edit connections when quick launching them through the Recent/Favourites c" +
    "onnection menus/lists?";
            this.chkAlwaysEdit.UseVisualStyleBackColor = true;
            // 
            // chkUtf8Bom
            // 
            this.chkUtf8Bom.AutoSize = true;
            this.chkUtf8Bom.Location = new System.Drawing.Point(7, 6);
            this.chkUtf8Bom.Name = "chkUtf8Bom";
            this.chkUtf8Bom.Size = new System.Drawing.Size(550, 17);
            this.chkUtf8Bom.TabIndex = 0;
            this.chkUtf8Bom.Text = "Use UTF-8 BOM for output (for maximum compatibility with other tools it is recomm" +
    "ended to leave this disabled)?";
            this.chkUtf8Bom.UseVisualStyleBackColor = true;
            // 
            // btnResetCurrent
            // 
            this.btnResetCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetCurrent.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnResetCurrent.Location = new System.Drawing.Point(180, 179);
            this.btnResetCurrent.Name = "btnResetCurrent";
            this.btnResetCurrent.Size = new System.Drawing.Size(110, 23);
            this.btnResetCurrent.TabIndex = 4;
            this.btnResetCurrent.Text = "&Reset";
            this.btnResetCurrent.UseVisualStyleBackColor = true;
            this.btnResetCurrent.Click += new System.EventHandler(this.btnResetCurrent_Click);
            // 
            // OptionsDialogue
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnResetDefault;
            this.ClientSize = new System.Drawing.Size(586, 214);
            this.Controls.Add(this.btnResetCurrent);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnResetDefault);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OptionsDialogue";
            this.Text = "Options";
            this.tabControl1.ResumeLayout(false);
            this.tabStartUpOptions.ResumeLayout(false);
            this.tabStartUpOptions.PerformLayout();
            this.grpActiveConnectionOptions.ResumeLayout(false);
            this.grpActiveConnectionOptions.PerformLayout();
            this.tabEditorOptions.ResumeLayout(false);
            this.tabEditorOptions.PerformLayout();
            this.tabOtherOptions.ResumeLayout(false);
            this.tabOtherOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxRecentConnections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviewSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnResetDefault;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabStartUpOptions;
        private System.Windows.Forms.TabPage tabEditorOptions;
        private System.Windows.Forms.TabPage tabOtherOptions;
        private System.Windows.Forms.CheckBox chkShowStartPage;
        private System.Windows.Forms.GroupBox grpActiveConnectionOptions;
        private System.Windows.Forms.CheckBox chkPromptRestoreActiveConnections;
        private System.Windows.Forms.CheckBox chkAlwaysRestoreActiveConnections;
        private System.Windows.Forms.CheckBox chkEditorHighlighting;
        private System.Windows.Forms.CheckBox chkEditorWordWrap;
        private System.Windows.Forms.CheckBox chkEditorDetectUrls;
        private System.Windows.Forms.CheckBox chkUtf8Bom;
        private System.Windows.Forms.CheckBox chkAlwaysEdit;
        private System.Windows.Forms.NumericUpDown numPreviewSize;
        private System.Windows.Forms.Label lblPreviewSize;
        private System.Windows.Forms.NumericUpDown numMaxRecentConnections;
        private System.Windows.Forms.Label lblMaxRecentConnections;
        private System.Windows.Forms.Button btnResetCurrent;
    }
}