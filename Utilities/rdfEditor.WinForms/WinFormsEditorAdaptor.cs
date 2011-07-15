using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.TextEditor;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public class WinFormsEditorAdaptor : BaseTextEditorAdaptor<TextEditorControl>
    {
        public WinFormsEditorAdaptor()
            : base(new TextEditorControl())
        {

        }

        public override string Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                this.Control.Text = value;
            }
        }

        public override int TextLength
        {
            get 
            {
                return this.Control.Document.TextLength; 
            }
        }

        public override int CaretOffset
        {
            get 
            {
                return this.Control.ActiveTextAreaControl.Caret.Offset;
            }
        }

        public override int SelectionStart
        {
            get
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    return this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Offset;
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    this.Control.ActiveTextAreaControl.SelectionManager.ClearSelection();
                }
                this.Control.ActiveTextAreaControl.SelectionManager.SetSelection(this.Control.Document.OffsetToPosition(value), this.Control.Document.OffsetToPosition(value + 1));
            }
        }

        public override int SelectionLength
        {
            get
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    return this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Length;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
                {
                    TextLocation start = this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().StartPosition;
                    int startOffset = this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.First().Length;
                    this.Control.ActiveTextAreaControl.SelectionManager.SelectionCollection.Clear();
                    this.Control.ActiveTextAreaControl.SelectionManager.SetSelection(start, this.Control.Document.OffsetToPosition(startOffset + value));
                }
            }
        }

        public override void Cut()
        {
            if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
            {
                Clipboard.SetText(this.Control.ActiveTextAreaControl.SelectionManager.SelectedText, TextDataFormat.UnicodeText);
                this.Control.ActiveTextAreaControl.SelectionManager.RemoveSelectedText();
            }
        }

        public override void Copy()
        {
            if (this.Control.ActiveTextAreaControl.SelectionManager.HasSomethingSelected)
            {
                Clipboard.SetText(this.Control.ActiveTextAreaControl.SelectionManager.SelectedText, TextDataFormat.UnicodeText);
            }
        }

        public override void Paste()
        {
            String text = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (text != null)
            {
                this.Control.Document.Insert(this.Control.ActiveTextAreaControl.Caret.Offset, text);
            }
        }

        public override void Undo()
        {
            this.Control.Undo();
        }

        public override void Redo()
        {
            this.Control.Redo();
        }

        public override void SetHighlighter(string name)
        {
            this.Control.SetHighlighting(name);
        }
    }
}
