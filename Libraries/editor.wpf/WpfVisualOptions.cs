using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    public class WpfVisualOptions
        : VisualOptions<FontFamily, Color>
    {
        public WpfVisualOptions()
        {
            this.Foreground = Colors.Black;
        }
    }
}
