using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public partial class EditorWindow : Form
    {
        private Editor<TextEditorControl> _editor;

        public EditorWindow()
        {
            InitializeComponent();
            WinFormsHighlightingManager.Initialise();

            this._editor = new Editor<TextEditorControl>(new WinFormsEditorFactory());
            this._editor.DocumentManager.Add(new Document<TextEditorControl>(this._editor.TextEditorFactory));

            this._editor.DocumentManager.ActiveDocument.TextEditor.Control.Dock = DockStyle.Fill;
            this.tabFile1.Controls.Add(this._editor.DocumentManager.ActiveDocument.TextEditor.Control);
        }
    }
}
