using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public abstract class VisualOptions<TFont, TColor>
    {
        public bool EnableClickableUris
        {
            get;
            set;
        }

        public bool ShowLineNumbers
        {
            get;
            set;
        }

        public bool ShowSpaces
        {
            get;
            set;
        }

        public bool ShowTabs
        {
            get;
            set;
        }

        public bool ShowEndOfLine
        {
            get;
            set;
        }

        public TFont FontFace
        {
            get;
            set;
        }

        public double FontSize
        {
            get;
            set;
        }

        public TColor Foreground
        {
            get;
            set;
        }

        public TColor Background
        {
            get;
            set;
        }
    }
}
