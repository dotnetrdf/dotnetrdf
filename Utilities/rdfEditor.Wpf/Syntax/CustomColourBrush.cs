using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    public class CustomColourBrush : HighlightingBrush
    {
        private SolidColorBrush _brush;

        public CustomColourBrush(Color c)
        {
            this._brush = new SolidColorBrush(c);
        }

        public override Brush GetBrush(ICSharpCode.AvalonEdit.Rendering.ITextRunConstructionContext context)
        {
            return this._brush;
        }
    }
}
