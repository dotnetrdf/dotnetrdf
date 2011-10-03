using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public class WpfEditorFactory
        : ITextEditorAdaptorFactory<TextEditor>
    {
        public ITextEditorAdaptor<TextEditor> CreateAdaptor()
        {
            return new WpfEditorAdaptor();
        }
    }
}
