using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using VDS.RDF.GUI;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public partial class EditorWindow : Form
    {
        private Editor<TextEditorControl> _editor;

        public EditorWindow()
        {
            InitializeComponent();

            //Initialise Highlighting
            WinFormsHighlightingManager.Initialise();

            //Configure the Editor object appropriately
            this._editor = new Editor<TextEditorControl>(new WinFormsEditorFactory());
            this._editor.DocumentManager.DefaultSaveChangesCallback = new SaveChangesCallback<TextEditorControl>(this.SaveChangesCallback);
            this._editor.DocumentManager.DefaultSaveAsCallback = new SaveAsCallback<TextEditorControl>(this.SaveAsCallback);

            //Display an initial document for editing
            this.AddTextEditor();

            //Register event handlers
            this.FormClosing += new FormClosingEventHandler(EditorWindow_FormClosing);
            this.tabFiles.SelectedIndexChanged += new EventHandler(tabFiles_SelectedIndexChanged);
        }

        #region Event Handlers

        void EditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._editor.DocumentManager.Count > 0)
            {
                this.mnuFileCloseAll_Click(sender, e);
                if (this._editor.DocumentManager.Count > 0)
                {
                    e.Cancel = true;
                }
            }
        }

        void tabFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabFiles.SelectedIndex >= 0)
            {
                try
                {
                    this._editor.DocumentManager.SwitchTo(this.tabFiles.SelectedIndex);
                }
                catch (IndexOutOfRangeException)
                {
                    //Ignore this since we may get this when existing because of events firing after objects have already
                    //been thrown away
                }
            }
        }

        private void HandleValidatorChanged(Object sender, DocumentChangedEventArgs<TextEditorControl> args)
        {
            if (ReferenceEquals(args.Document, this._editor.DocumentManager.ActiveDocument))
            {
                if (args.Document.SyntaxValidator == null)
                {
                    this.stsValidation.Text = "No Syntax Validator available for the currently selected syntax";
                }
                else
                {
                    this.stsValidation.Text = "Syntax Validation available, enable Validate as you Type or select Tools > Validate to validate";
                }
            }
        }

        private void HandleValidation(Object sender, DocumentValidatedEventArgs<TextEditorControl> args)
        {
            if (ReferenceEquals(args.Document, this._editor.DocumentManager.ActiveDocument))
            {
                this.stsValidation.ToolTipText = String.Empty;
                if (args.ValidationResults != null)
                {
                    this.stsValidation.Text = args.ValidationResults.Message;
                    this.stsValidation.ToolTipText = args.ValidationResults.Message;
                    if (args.ValidationResults.Warnings.Any())
                    {
                        this.stsValidation.ToolTipText += "\n" + String.Join("\n", args.ValidationResults.Warnings.ToArray());
                    }
                }
                else
                {
                    this.stsValidation.Text = "Syntax Validation unavailable";
                }
            }
        }

        #endregion

        #region Text Editor Management

        private void AddTextEditor()
        {
            this.AddTextEditor(new TabPage());
        }

        private void AddTextEditor(TabPage tab)
        {
            Document<TextEditorControl> doc = this._editor.DocumentManager.New();
            this.AddTextEditor(tab, doc);
        }

        private void AddTextEditor(TabPage tab, Document<TextEditorControl> doc)
        {
            //Register for relevant events on the document
            doc.FilenameChanged +=
                new DocumentChangedHandler<TextEditorControl>((sender, e) =>
                {
                    if (e.Document.Filename != null && !e.Document.Filename.Equals(String.Empty))
                    {
                        tab.Text = Path.GetFileName(e.Document.Filename);
                    }
                });
            doc.TitleChanged +=new DocumentChangedHandler<TextEditorControl>((sender, e) =>
                {
                    if (e.Document.Title != null && !e.Document.Title.Equals(String.Empty))
                    {
                        tab.Text = e.Document.Title;
                    }
                });
            doc.SyntaxChanged += new DocumentChangedHandler<TextEditorControl>((sender, e) =>
                {
                    if (ReferenceEquals(this._editor.DocumentManager.ActiveDocument, e.Document))
                    {
                        this.stsSyntax.Text = "Syntax: " + e.Document.Syntax;
                    }
                });
            doc.Validated += new DocumentValidatedHandler<TextEditorControl>(this.HandleValidation);
            doc.ValidatorChanged += new DocumentChangedHandler<TextEditorControl>(this.HandleValidatorChanged);

            //Set Tab title where appropriate
            if (doc.Filename != null && !doc.Filename.Equals(String.Empty))
            {
                tab.Text = Path.GetFileName(doc.Filename);
            }
            else if (doc.Title != null && !doc.Title.Equals(String.Empty))
            {
                tab.Text = doc.Title;
            }

            //Add to Tabs
            this.tabFiles.TabPages.Add(tab);
            tab.Controls.Add(doc.TextEditor.Control);

            //Add appropriate event handlers on tabs
            tab.Enter +=
                new EventHandler((sender, e) =>
                {
                    var page = ((TabPage)sender);
                    if (page.Controls.Count > 0)
                    {
                        page.BeginInvoke(new Action<TabPage>(p => p.Controls[0].Focus()), page);
                    }
                });
        }

        #endregion

        #region File Menu

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            this.AddTextEditor();
            this._editor.DocumentManager.SwitchTo(this._editor.DocumentManager.Count - 1);
            this.tabFiles.SelectedIndex = this.tabFiles.TabCount - 1;
        }

        private void mnuFileNewFromActive_Click(object sender, EventArgs e)
        {
            Document<TextEditorControl> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                Document<TextEditorControl> newDoc = this._editor.DocumentManager.NewFromActive(true);

                TabPage tab = new TabPage(newDoc.Title);
                this.AddTextEditor(tab, newDoc);
                this.tabFiles.SelectedIndex = this.tabFiles.TabCount - 1;
            }
            else
            {
                this.AddTextEditor();
            }
        }

        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            Document<TextEditorControl> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                if (doc.Filename == null || doc.Filename.Equals(String.Empty))
                {
                    mnuFileSaveAs_Click(sender, e);
                }
                else
                {
                    doc.Save();
                }
            }
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            Document<TextEditorControl> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                String filename = this.SaveAsCallback(doc);
                if (filename != null)
                {
                    doc.SaveAs(sfdSave.FileName);
                }
            }
        }

        private void mnuFileUseUtf8Bom_Click(object sender, EventArgs e)
        {
            GlobalOptions.UseBomForUtf8 = this.mnuFileUseUtf8Bom.Checked;
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            this.ofdOpen.Filter = Constants.AllFilter;
            if (this.ofdOpen.ShowDialog() == DialogResult.OK)
            {
                Document<TextEditorControl> doc, active;
                active = this._editor.DocumentManager.ActiveDocument;
                if (active.TextLength == 0 && (active.Filename == null || active.Filename.Equals(String.Empty)))
                {
                    doc = active;
                    doc.Filename = this.ofdOpen.FileName;
                } 
                else
                {
                    doc = this._editor.DocumentManager.New(Path.GetFileName(this.ofdOpen.FileName), true);
                }

                //Open the file and display in new tab if necessary
                doc.Open(this.ofdOpen.FileName);
                if (!ReferenceEquals(active, doc))
                {
                    this.AddTextEditor(new TabPage(doc.Title), doc);
                }
            }
        }

        private void mnuFileClose_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                if (this._editor.DocumentManager.Close())
                {
                    this.tabFiles.TabPages.RemoveAt(this.tabFiles.SelectedIndex);
                }
            }
        }

        private void mnuFileCloseAll_Click(object sender, EventArgs e)
        {
            this._editor.DocumentManager.CloseAll();
            this.tabFiles.TabPages.Clear();

            //Recreate new Tabs for any Documents that were not closed
            foreach (Document<TextEditorControl> doc in this._editor.DocumentManager.Documents)
            {
                this.AddTextEditor(new TabPage(doc.Title), doc);
            }
        }

        private void mnuSaveAll_Click(object sender, EventArgs e)
        {
            this._editor.DocumentManager.SaveAll();
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            mnuFileCloseAll_Click(sender, e);
            if (this.tabFiles.TabCount == 0)
            {
                Application.Exit();
            }
        }

        #endregion

        #region Edit Menu

        private void mnuEditUndo_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Undo();
            }
        }

        private void mnuEditRedo_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Redo();
            }
        }

        private void mnuEditCut_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Cut();
            }
        }

        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Copy();
            }
        }

        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Paste();
            }
        }

        #endregion

        #region View Menu

        private void mnuViewNextDocument_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.Count > 1)
            {
                this._editor.DocumentManager.NextDocument();
                if (this.tabFiles.SelectedIndex < this.tabFiles.TabCount - 1)
                {
                    this.tabFiles.SelectedIndex++;
                }
                else
                {
                    this.tabFiles.SelectedIndex = 0;
                }
            }
        }

        private void mnuViewPrevDocument_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.Count > 1)
            {
                this._editor.DocumentManager.PrevDocument();
                if (this.tabFiles.SelectedIndex > 0)
                {
                    this.tabFiles.SelectedIndex--;
                }
                else
                {
                    this.tabFiles.SelectedIndex = this.tabFiles.TabCount - 1;
                }
            }
        }

        private void mnuViewLineNumbers_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowLineNumbers = this.mnuViewLineNumbers.Checked;
            }
        }

        private void mnuShowSpecialEol_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowEndOfLine = this.mnuShowSpecialEol.Checked;
            }
        }

        private void mnuShowSpecialSpaces_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowSpaces = this.mnuShowSpecialSpaces.Checked;
            }
        }

        private void mnuShowSpecialTabs_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowTabs = this.mnuShowSpecialTabs.Checked;
            }
        }

        private void mnuShowSpecialAll_Click(object sender, EventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                bool flag = this.mnuShowSpecialAll.Checked;
                this.mnuShowSpecialEol.Checked = flag;
                this.mnuShowSpecialSpaces.Checked = flag;
                this.mnuShowSpecialTabs.Checked = flag;
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowEndOfLine = flag;
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowSpaces = flag;
                this._editor.DocumentManager.ActiveDocument.TextEditor.ShowTabs = flag;
            }
        }

        #endregion

        #region Callbacks

        private SaveChangesMode SaveChangesCallback(Document<TextEditorControl> doc)
        {
            DialogResult result = MessageBox.Show(doc.Title + " has unsaved changes, do you wish to save these changes before closing the document?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    return SaveChangesMode.Save;
                case DialogResult.Cancel:
                    return SaveChangesMode.Cancel;
                case DialogResult.No:
                default:
                    return SaveChangesMode.Discard;
            }
        }

        private String SaveAsCallback(Document<TextEditorControl> doc)
        {
            sfdSave.Filter = Constants.AllFilter;
            if (doc.Filename == null || doc.Filename.Equals(String.Empty))
            {
                sfdSave.Title = "Save " + doc.Title + " As...";
            }
            else
            {
                sfdSave.Title = "Save " + Path.GetFileName(doc.Filename) + " As...";
                sfdSave.InitialDirectory = Path.GetDirectoryName(doc.Filename);
                sfdSave.FileName = doc.Filename;
            }

            if (this.sfdSave.ShowDialog() == DialogResult.OK)
            {
                return this.sfdSave.FileName;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
