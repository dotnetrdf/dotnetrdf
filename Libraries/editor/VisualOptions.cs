using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public abstract class VisualOptions<TFont, TColor>
        where TFont : class
        where TColor: struct
    {
        private bool _clickableUris = false,
                     _showLineNumbers = true,
                     _showSpaces = false,
                     _showTabs = false,
                     _showEndOfLine = false,
                     _wordWrap = false;
        private TFont _fontFace = null;
        private double _fontSize = 13.0d;
        private TColor? _foreground = null,
                        _background = null;

        public bool EnableClickableUris
        {
            get
            {
                return this._clickableUris;
            }
            set
            {
                if (value != this._clickableUris)
                {
                    this._clickableUris = value;
                    this.RaiseChanged();
                }
            }
        }

        public bool ShowLineNumbers
        {
            get
            {
                return this._showLineNumbers;
            }
            set
            {
                if (value != this._showLineNumbers)
                {
                    this._showLineNumbers = value;
                    this.RaiseChanged();
                }
            }
        }

        public bool ShowSpaces
        {
            get
            {
                return this._showSpaces;
            }
            set
            {
                if (value != this._showSpaces)
                {
                    this._showSpaces = value;
                    this.RaiseChanged();
                }
            }
        }

        public bool ShowTabs
        {
            get
            {
                return this._showTabs;
            }
            set
            {
                if (value != this._showTabs)
                {
                    this._showTabs = value;
                    this.RaiseChanged();
                }
            }
        }

        public bool ShowEndOfLine
        {
            get
            {
                return this._showEndOfLine;
            }
            set
            {
                if (value != this._showEndOfLine)
                {
                    this._showEndOfLine = value;
                    this.RaiseChanged();
                }
            }
        }

        public bool WordWrap
        {
            get
            {
                return this._wordWrap;
            }
            set
            {
                if (value != this._wordWrap)
                {
                    this._wordWrap = value;
                    this.RaiseChanged();
                }
            }
        }

        public TFont FontFace
        {
            get
            {
                return this._fontFace;
            }
            set
            {
                if (value == null)
                {
                    if (this._fontFace != null)
                    {
                        this._fontFace = null;
                        this.RaiseChanged();
                    }
                }
                else
                {
                    if (this._fontFace == null)
                    {
                        this._fontFace = value;
                        this.RaiseChanged();
                    }
                    else if (!this._fontFace.Equals(value))
                    {
                        this._fontFace = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        public double FontSize
        {
            get
            {
                return this._fontSize;
            }
            set
            {
                if (value != this._fontSize)
                {
                    this._fontSize = value;
                    this.RaiseChanged();
                }
            }
        }

        public TColor Foreground
        {
            get
            {
                if (this._foreground != null)
                {
                    return this._foreground.Value;
                }
                else
                {
                    return default(TColor);
                }
            }
            set
            {
                if (this._foreground != null)
                {
                    this._foreground = value;
                    this.RaiseChanged();
                }
                else
                {
                    if (!this._foreground.Equals(value))
                    {
                        this._foreground = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        public TColor Background
        {
            get
            {
                if (this._background != null)
                {
                    return this._background.Value;
                }
                else
                {
                    return default(TColor);
                }
            }
            set
            {
                if (this._background != null)
                {
                    this._background = value;
                    this.RaiseChanged();
                }
                else
                {
                    if (!this._background.Equals(value))
                    {
                        this._background = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        public event OptionsChanged Changed;

        protected void RaiseChanged()
        {
            OptionsChanged d = this.Changed;
            if (d != null) d();
        }
    }

    public delegate void OptionsChanged();
}
