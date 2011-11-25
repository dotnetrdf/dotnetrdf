using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public class WpfEditorFactory
        : IVisualTextEditorAdaptorFactory<TextEditor, FontFamily, Color>
    {
        public ITextEditorAdaptor<TextEditor> CreateAdaptor()
        {
            return new WpfEditorAdaptor();
        }

        public VisualOptions<FontFamily, Color> GetDefaultVisualOptions()
        {
            return new WpfVisualOptions();
        }
    }
}
