using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class TextEditorEventArgs<T> : EventArgs
    {
        private ITextEditorAdaptor<T> _editor;

        public TextEditorEventArgs(ITextEditorAdaptor<T> editor)
        {
            this._editor = editor;
        }

        public ITextEditorAdaptor<T> TextEditor
        {
            get
            {
                return this._editor;
            }
        }
    }

    public delegate void TextEditorEventHandler<T>(Object sender, TextEditorEventArgs<T> args);
}
