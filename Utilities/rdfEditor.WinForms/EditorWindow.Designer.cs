namespace VDS.RDF.Utilities.Editor.WinForms
{
    partial class EditorWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorWindow));
            this.tabFiles = new System.Windows.Forms.TabControl();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileNewFromActive = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenUri = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenQueryResults = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveWith = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileUseUtf8Bom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFilePageSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFilePrintPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuPrintPreviewWithoutHighlighting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPrintWithoutHighlighting = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewNextDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewPrevDocument = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewLineNumbers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewSpecialChars = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowSpecialAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShowSpecialEol = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowSpecialSpaces = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowSpecialTabs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.stsInfo = new System.Windows.Forms.StatusStrip();
            this.stsSyntax = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsValidation = new System.Windows.Forms.ToolStripStatusLabel();
            this.sfdSave = new System.Windows.Forms.SaveFileDialog();
            this.ofdOpen = new System.Windows.Forms.OpenFileDialog();
            this.ttpTips = new System.Windows.Forms.ToolTip(this.components);
            this.mnuMain.SuspendLayout();
            this.stsInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabFiles
            // 
            this.tabFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabFiles.Location = new System.Drawing.Point(12, 27);
            this.tabFiles.Name = "tabFiles";
            this.tabFiles.SelectedIndex = 0;
            this.tabFiles.Size = new System.Drawing.Size(831, 461);
            this.tabFiles.TabIndex = 0;
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuView,
            this.mnuOptions,
            this.mnuHelp});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(855, 24);
            this.mnuMain.TabIndex = 1;
            this.mnuMain.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileNew,
            this.mnuFileNewFromActive,
            this.toolStripSeparator1,
            this.mnuFileOpen,
            this.mnuFileOpenUri,
            this.mnuFileOpenQueryResults,
            this.toolStripSeparator2,
            this.mnuFileSave,
            this.mnuSaveAll,
            this.mnuFileSaveAs,
            this.mnuFileSaveWith,
            this.mnuFileUseUtf8Bom,
            this.toolStripMenuItem1,
            this.mnuFilePageSetup,
            this.toolStripMenuItem2,
            this.mnuFilePrintPreview,
            this.mnuFilePrint,
            this.toolStripMenuItem3,
            this.mnuPrintPreviewWithoutHighlighting,
            this.mnuPrintWithoutHighlighting,
            this.toolStripMenuItem4,
            this.mnuFileClose,
            this.mnuFileCloseAll,
            this.toolStripMenuItem5,
            this.mnuFileExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 20);
            this.mnuFile.Text = "File";
            // 
            // mnuFileNew
            // 
            this.mnuFileNew.Name = "mnuFileNew";
            this.mnuFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuFileNew.Size = new System.Drawing.Size(257, 22);
            this.mnuFileNew.Text = "&New";
            this.mnuFileNew.Click += new System.EventHandler(this.mnuFileNew_Click);
            // 
            // mnuFileNewFromActive
            // 
            this.mnuFileNewFromActive.Name = "mnuFileNewFromActive";
            this.mnuFileNewFromActive.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.N)));
            this.mnuFileNewFromActive.Size = new System.Drawing.Size(257, 22);
            this.mnuFileNewFromActive.Text = "New from Active";
            this.mnuFileNewFromActive.Click += new System.EventHandler(this.mnuFileNewFromActive_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuFileOpen.Size = new System.Drawing.Size(257, 22);
            this.mnuFileOpen.Text = "&Open";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // mnuFileOpenUri
            // 
            this.mnuFileOpenUri.Name = "mnuFileOpenUri";
            this.mnuFileOpenUri.Size = new System.Drawing.Size(257, 22);
            this.mnuFileOpenUri.Text = "Open Uri";
            // 
            // mnuFileOpenQueryResults
            // 
            this.mnuFileOpenQueryResults.Name = "mnuFileOpenQueryResults";
            this.mnuFileOpenQueryResults.Size = new System.Drawing.Size(257, 22);
            this.mnuFileOpenQueryResults.Text = "Open Query Results";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFileSave
            // 
            this.mnuFileSave.Name = "mnuFileSave";
            this.mnuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuFileSave.Size = new System.Drawing.Size(257, 22);
            this.mnuFileSave.Text = "&Save";
            this.mnuFileSave.Click += new System.EventHandler(this.mnuFileSave_Click);
            // 
            // mnuSaveAll
            // 
            this.mnuSaveAll.Name = "mnuSaveAll";
            this.mnuSaveAll.Size = new System.Drawing.Size(257, 22);
            this.mnuSaveAll.Text = "Save All";
            this.mnuSaveAll.Click += new System.EventHandler(this.mnuSaveAll_Click);
            // 
            // mnuFileSaveAs
            // 
            this.mnuFileSaveAs.Name = "mnuFileSaveAs";
            this.mnuFileSaveAs.Size = new System.Drawing.Size(257, 22);
            this.mnuFileSaveAs.Text = "Save &As...";
            this.mnuFileSaveAs.Click += new System.EventHandler(this.mnuFileSaveAs_Click);
            // 
            // mnuFileSaveWith
            // 
            this.mnuFileSaveWith.Name = "mnuFileSaveWith";
            this.mnuFileSaveWith.Size = new System.Drawing.Size(257, 22);
            this.mnuFileSaveWith.Text = "Save With";
            // 
            // mnuFileUseUtf8Bom
            // 
            this.mnuFileUseUtf8Bom.Checked = true;
            this.mnuFileUseUtf8Bom.CheckOnClick = true;
            this.mnuFileUseUtf8Bom.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuFileUseUtf8Bom.Name = "mnuFileUseUtf8Bom";
            this.mnuFileUseUtf8Bom.Size = new System.Drawing.Size(257, 22);
            this.mnuFileUseUtf8Bom.Text = "Use BOM for UTF-8 Output";
            this.mnuFileUseUtf8Bom.Click += new System.EventHandler(this.mnuFileUseUtf8Bom_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFilePageSetup
            // 
            this.mnuFilePageSetup.Name = "mnuFilePageSetup";
            this.mnuFilePageSetup.Size = new System.Drawing.Size(257, 22);
            this.mnuFilePageSetup.Text = "Page Setup";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFilePrintPreview
            // 
            this.mnuFilePrintPreview.Name = "mnuFilePrintPreview";
            this.mnuFilePrintPreview.Size = new System.Drawing.Size(257, 22);
            this.mnuFilePrintPreview.Text = "Print Preview";
            // 
            // mnuFilePrint
            // 
            this.mnuFilePrint.Name = "mnuFilePrint";
            this.mnuFilePrint.Size = new System.Drawing.Size(257, 22);
            this.mnuFilePrint.Text = "Print";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuPrintPreviewWithoutHighlighting
            // 
            this.mnuPrintPreviewWithoutHighlighting.Name = "mnuPrintPreviewWithoutHighlighting";
            this.mnuPrintPreviewWithoutHighlighting.Size = new System.Drawing.Size(257, 22);
            this.mnuPrintPreviewWithoutHighlighting.Text = "Print Preview without Highlighting";
            // 
            // mnuPrintWithoutHighlighting
            // 
            this.mnuPrintWithoutHighlighting.Name = "mnuPrintWithoutHighlighting";
            this.mnuPrintWithoutHighlighting.Size = new System.Drawing.Size(257, 22);
            this.mnuPrintWithoutHighlighting.Text = "Print without Highlighting";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFileClose
            // 
            this.mnuFileClose.Name = "mnuFileClose";
            this.mnuFileClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.mnuFileClose.Size = new System.Drawing.Size(257, 22);
            this.mnuFileClose.Text = "Close";
            this.mnuFileClose.Click += new System.EventHandler(this.mnuFileClose_Click);
            // 
            // mnuFileCloseAll
            // 
            this.mnuFileCloseAll.Name = "mnuFileCloseAll";
            this.mnuFileCloseAll.Size = new System.Drawing.Size(257, 22);
            this.mnuFileCloseAll.Text = "Close All";
            this.mnuFileCloseAll.Click += new System.EventHandler(this.mnuFileCloseAll_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuFileExit.Size = new System.Drawing.Size(257, 22);
            this.mnuFileExit.Text = "Exit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditUndo,
            this.mnuEditRedo,
            this.toolStripMenuItem6,
            this.mnuEditCut,
            this.mnuEditCopy,
            this.mnuEditPaste});
            this.mnuEdit.Name = "mnuEdit";
            this.mnuEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuEdit.Text = "Edit";
            // 
            // mnuEditUndo
            // 
            this.mnuEditUndo.Name = "mnuEditUndo";
            this.mnuEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.mnuEditUndo.Size = new System.Drawing.Size(144, 22);
            this.mnuEditUndo.Text = "Undo";
            this.mnuEditUndo.Click += new System.EventHandler(this.mnuEditUndo_Click);
            // 
            // mnuEditRedo
            // 
            this.mnuEditRedo.Name = "mnuEditRedo";
            this.mnuEditRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.mnuEditRedo.Size = new System.Drawing.Size(144, 22);
            this.mnuEditRedo.Text = "Redo";
            this.mnuEditRedo.Click += new System.EventHandler(this.mnuEditRedo_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(141, 6);
            // 
            // mnuEditCut
            // 
            this.mnuEditCut.Name = "mnuEditCut";
            this.mnuEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.mnuEditCut.Size = new System.Drawing.Size(144, 22);
            this.mnuEditCut.Text = "Cut";
            this.mnuEditCut.Click += new System.EventHandler(this.mnuEditCut_Click);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Name = "mnuEditCopy";
            this.mnuEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuEditCopy.Size = new System.Drawing.Size(144, 22);
            this.mnuEditCopy.Text = "Copy";
            this.mnuEditCopy.Click += new System.EventHandler(this.mnuEditCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Name = "mnuEditPaste";
            this.mnuEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.mnuEditPaste.Size = new System.Drawing.Size(144, 22);
            this.mnuEditPaste.Text = "Paste";
            this.mnuEditPaste.Click += new System.EventHandler(this.mnuEditPaste_Click);
            // 
            // mnuView
            // 
            this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewNextDocument,
            this.mnuViewPrevDocument,
            this.toolStripSeparator3,
            this.mnuViewLineNumbers,
            this.toolStripSeparator4,
            this.mnuViewSpecialChars});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(44, 20);
            this.mnuView.Text = "View";
            // 
            // mnuViewNextDocument
            // 
            this.mnuViewNextDocument.Name = "mnuViewNextDocument";
            this.mnuViewNextDocument.Size = new System.Drawing.Size(202, 22);
            this.mnuViewNextDocument.Text = "Next Document";
            this.mnuViewNextDocument.Click += new System.EventHandler(this.mnuViewNextDocument_Click);
            // 
            // mnuViewPrevDocument
            // 
            this.mnuViewPrevDocument.Name = "mnuViewPrevDocument";
            this.mnuViewPrevDocument.Size = new System.Drawing.Size(202, 22);
            this.mnuViewPrevDocument.Text = "Previous Document";
            this.mnuViewPrevDocument.Click += new System.EventHandler(this.mnuViewPrevDocument_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(199, 6);
            // 
            // mnuViewLineNumbers
            // 
            this.mnuViewLineNumbers.Checked = true;
            this.mnuViewLineNumbers.CheckOnClick = true;
            this.mnuViewLineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewLineNumbers.Name = "mnuViewLineNumbers";
            this.mnuViewLineNumbers.Size = new System.Drawing.Size(202, 22);
            this.mnuViewLineNumbers.Text = "Show Line Numbers";
            this.mnuViewLineNumbers.Click += new System.EventHandler(this.mnuViewLineNumbers_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(199, 6);
            // 
            // mnuViewSpecialChars
            // 
            this.mnuViewSpecialChars.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShowSpecialAll,
            this.toolStripSeparator5,
            this.mnuShowSpecialEol,
            this.mnuShowSpecialSpaces,
            this.mnuShowSpecialTabs});
            this.mnuViewSpecialChars.Name = "mnuViewSpecialChars";
            this.mnuViewSpecialChars.Size = new System.Drawing.Size(202, 22);
            this.mnuViewSpecialChars.Text = "Show Special Characters";
            // 
            // mnuShowSpecialAll
            // 
            this.mnuShowSpecialAll.CheckOnClick = true;
            this.mnuShowSpecialAll.Name = "mnuShowSpecialAll";
            this.mnuShowSpecialAll.Size = new System.Drawing.Size(165, 22);
            this.mnuShowSpecialAll.Text = "Show All";
            this.mnuShowSpecialAll.Click += new System.EventHandler(this.mnuShowSpecialAll_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(162, 6);
            // 
            // mnuShowSpecialEol
            // 
            this.mnuShowSpecialEol.CheckOnClick = true;
            this.mnuShowSpecialEol.Name = "mnuShowSpecialEol";
            this.mnuShowSpecialEol.Size = new System.Drawing.Size(165, 22);
            this.mnuShowSpecialEol.Text = "Show End of Line";
            this.mnuShowSpecialEol.Click += new System.EventHandler(this.mnuShowSpecialEol_Click);
            // 
            // mnuShowSpecialSpaces
            // 
            this.mnuShowSpecialSpaces.CheckOnClick = true;
            this.mnuShowSpecialSpaces.Name = "mnuShowSpecialSpaces";
            this.mnuShowSpecialSpaces.Size = new System.Drawing.Size(165, 22);
            this.mnuShowSpecialSpaces.Text = "Show Spaces";
            this.mnuShowSpecialSpaces.Click += new System.EventHandler(this.mnuShowSpecialSpaces_Click);
            // 
            // mnuShowSpecialTabs
            // 
            this.mnuShowSpecialTabs.CheckOnClick = true;
            this.mnuShowSpecialTabs.Name = "mnuShowSpecialTabs";
            this.mnuShowSpecialTabs.Size = new System.Drawing.Size(165, 22);
            this.mnuShowSpecialTabs.Text = "Show Tabs";
            this.mnuShowSpecialTabs.Click += new System.EventHandler(this.mnuShowSpecialTabs_Click);
            // 
            // mnuOptions
            // 
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(61, 20);
            this.mnuOptions.Text = "Options";
            // 
            // mnuHelp
            // 
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(44, 20);
            this.mnuHelp.Text = "Help";
            // 
            // stsInfo
            // 
            this.stsInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stsSyntax,
            this.stsValidation});
            this.stsInfo.Location = new System.Drawing.Point(0, 498);
            this.stsInfo.Name = "stsInfo";
            this.stsInfo.Size = new System.Drawing.Size(855, 22);
            this.stsInfo.TabIndex = 2;
            this.stsInfo.Text = "statusStrip1";
            // 
            // stsSyntax
            // 
            this.stsSyntax.Name = "stsSyntax";
            this.stsSyntax.Size = new System.Drawing.Size(76, 17);
            this.stsSyntax.Text = "Syntax: None";
            // 
            // stsValidation
            // 
            this.stsValidation.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stsValidation.Name = "stsValidation";
            this.stsValidation.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            this.stsValidation.Size = new System.Drawing.Size(764, 17);
            this.stsValidation.Spring = true;
            this.stsValidation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sfdSave
            // 
            this.sfdSave.Title = "Save As...";
            // 
            // ofdOpen
            // 
            this.ofdOpen.Multiselect = true;
            this.ofdOpen.Title = "Open File";
            // 
            // ttpTips
            // 
            this.ttpTips.AutomaticDelay = 200;
            this.ttpTips.IsBalloon = true;
            // 
            // EditorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 520);
            this.Controls.Add(this.stsInfo);
            this.Controls.Add(this.tabFiles);
            this.Controls.Add(this.mnuMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditorWindow";
            this.Text = "rdfEditor";
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.stsInfo.ResumeLayout(false);
            this.stsInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabFiles;
        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.StatusStrip stsInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuFileNew;
        private System.Windows.Forms.ToolStripMenuItem mnuFileNewFromActive;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSave;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveWith;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpenUri;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpenQueryResults;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePageSetup;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrintPreview;
        private System.Windows.Forms.ToolStripMenuItem mnuPrintPreviewWithoutHighlighting;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripMenuItem mnuPrintWithoutHighlighting;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripMenuItem mnuFileCloseAll;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveAll;
        private System.Windows.Forms.SaveFileDialog sfdSave;
        private System.Windows.Forms.OpenFileDialog ofdOpen;
        private System.Windows.Forms.ToolStripStatusLabel stsSyntax;
        private System.Windows.Forms.ToolStripStatusLabel stsValidation;
        private System.Windows.Forms.ToolTip ttpTips;
        private System.Windows.Forms.ToolStripMenuItem mnuEditUndo;
        private System.Windows.Forms.ToolStripMenuItem mnuEditRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCut;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEditPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuViewNextDocument;
        private System.Windows.Forms.ToolStripMenuItem mnuViewPrevDocument;
        private System.Windows.Forms.ToolStripMenuItem mnuFileUseUtf8Bom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuViewLineNumbers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mnuViewSpecialChars;
        private System.Windows.Forms.ToolStripMenuItem mnuShowSpecialAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuShowSpecialEol;
        private System.Windows.Forms.ToolStripMenuItem mnuShowSpecialSpaces;
        private System.Windows.Forms.ToolStripMenuItem mnuShowSpecialTabs;
    }
}

