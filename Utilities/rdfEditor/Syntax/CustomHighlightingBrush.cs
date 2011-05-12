using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    class CustomHighlightingBrush : HighlightingBrush
    {
        private SolidColorBrush _brush;

        public CustomHighlightingBrush(Color c)
        {
            this._brush = new SolidColorBrush(c);
            this._brush.Freeze();
        }

        public CustomHighlightingBrush(SolidColorBrush b)
        {
            this._brush = b;
            this._brush.Freeze();
        }

        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return this._brush;
        }

        public override string ToString()
        {
            return this._brush.ToString();
        }
    }
}
