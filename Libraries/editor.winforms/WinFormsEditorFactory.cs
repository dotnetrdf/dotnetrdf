using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.TextEditor;

namespace VDS.RDF.Utilities.Editor.WinForms
{
    public class WinFormsEditorFactory : ITextEditorAdaptorFactory<TextEditorControl>
    {
        public ITextEditorAdaptor<TextEditorControl> CreateAdaptor()
        {
            WinFormsEditorAdaptor adaptor = new WinFormsEditorAdaptor();
            adaptor.SetHighlighter("Turtle");
            return adaptor;
        }
    }
}
