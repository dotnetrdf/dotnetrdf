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
        private int _nextID = 0;

        public EditorWindow()
        {
            InitializeComponent();
            WinFormsHighlightingManager.Initialise();

            this._editor = new Editor<TextEditorControl>(new WinFormsEditorFactory());
            this.AddTextEditor();

            this.tabFiles.TabIndexChanged += new EventHandler(tabFiles_TabIndexChanged);
        }

        void tabFiles_TabIndexChanged(object sender, EventArgs e)
        {
            this._editor.DocumentManager.SwitchTo(this.tabFiles.SelectedIndex);
        }

        private TabPage GetTab()
        {
            return new TabPage("Untitled " + (++this._nextID));
        }

        private void AddTextEditor()
        {
            this.AddTextEditor(this.GetTab());
        }

        private void AddTextEditor(TabPage tab)
        {
            //Create the Document
            Document<TextEditorControl> doc = new Document<TextEditorControl>(this._editor.TextEditorFactory);
            doc.TextEditor.Control.Dock = DockStyle.Fill;

            //Add to Document Manager
            this._editor.DocumentManager.Add(doc);

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

            //Set Tab title where appropriate
            if (doc.Filename != null && !doc.Filename.Equals(String.Empty))
            {
                tab.Text = Path.GetFileName(doc.Filename);
            }

            //Add to Tabs
            this.tabFiles.TabPages.Add(tab);
            tab.Controls.Add(doc.TextEditor.Control);
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

        #region File Menu

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            this.AddTextEditor();
        }

        #endregion

        private void mnuFileNewFromActive_Click(object sender, EventArgs e)
        {
            Document<TextEditorControl> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                this._editor.DocumentManager.Copy(this._editor.TextEditorFactory, true);
                Document<TextEditorControl> newDoc = this._editor.DocumentManager.ActiveDocument;
                newDoc.TextEditor.Control.Dock = DockStyle.Fill;

                TabPage tab = this.GetTab();
                this.AddTextEditor(tab, newDoc);
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
                sfdSave.Filter = Constants.AllFilter;
                if (doc.Filename == null || doc.Filename.Equals(String.Empty))
                {
                    sfdSave.Title = "Save As...";
                }
                else
                {
                    sfdSave.Title = "Save " + Path.GetFileName(doc.Filename) + " As...";
                    sfdSave.InitialDirectory = Path.GetDirectoryName(doc.Filename);
                    sfdSave.FileName = doc.Filename;
                }

                if (sfdSave.ShowDialog() == DialogResult.OK)
                {
                    doc.SaveAs(sfdSave.FileName);
                }
            }
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
                    doc = new Document<TextEditorControl>(this._editor.TextEditorFactory, this.ofdOpen.FileName);
                    this._editor.DocumentManager.Add(doc, true);
                    doc.TextEditor.Control.Dock = DockStyle.Fill;
                }
 
                using (StreamReader reader = new StreamReader(doc.Filename))
                {
                    doc.Text = reader.ReadToEnd();
                }

                if (!ReferenceEquals(active, doc))
                {
                    this.AddTextEditor(this.GetTab(), doc);
                }
            }
        }

    }
}
