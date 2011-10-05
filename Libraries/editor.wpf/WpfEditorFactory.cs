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
        public WpfVisualOptions VisualOptions
        {
            get;
            set;
        }

        public ITextEditorAdaptor<TextEditor> CreateAdaptor()
        {
            WpfEditorAdaptor adaptor = new WpfEditorAdaptor();
            if (this.VisualOptions != null) adaptor.Apply(this.VisualOptions);
            return adaptor;
        }
    }
}
