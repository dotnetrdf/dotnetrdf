using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    class FakeTextRunContext : ITextRunConstructionContext
    {
        public ICSharpCode.AvalonEdit.Document.TextDocument Document
        {
            get 
            { 
                return null; 
            }
        }

        public TextRunProperties GlobalTextRunProperties
        {
            get 
            { 
                return null; 
            }
        }

        public TextView TextView
        {
            get 
            { 
                return null;
            }
        }

        public VisualLine VisualLine
        {
            get 
            { 
                return null;
            }
        }
    }
}
