/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Visual options for text editors
    /// </summary>
    /// <typeparam name="TFont">Font Type</typeparam>
    /// <typeparam name="TColor">Colour Type</typeparam>
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
        private TFont _fontFace = null,
                      _errorFontFace = null;
        private double _fontSize = 13.0d;
        private TColor? _foreground = null,
                        _background = null,
                        _errorForeground = null,
                        _errorBackground = null;
        private String _errorDecoration = null;

        /// <summary>
        /// Gets/Sets whether clickable URIs are enabled
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether to show line numbers
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether to show spaces
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether to show tabs
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether to show new line characters
        /// </summary>
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

        /// <summary>
        /// Gets/Sets word wrap
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the font face
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the font face for error highlights
        /// </summary>
        public TFont ErrorFontFace
        {
            get
            {
                return this._errorFontFace;
            }
            set
            {
                if (value == null)
                {
                    if (this._errorFontFace != null)
                    {
                        this._errorFontFace = null;
                        this.RaiseChanged();
                    }
                }
                else
                {
                    if (this._errorFontFace == null)
                    {
                        this._errorFontFace = value;
                        this.RaiseChanged();
                    }
                    else if (!this._errorFontFace.Equals(value))
                    {
                        this._errorFontFace = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the font size
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the foreground colour
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the background colour
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the foreground colour for error highlights
        /// </summary>
        public TColor ErrorForeground
        {
            get
            {
                if (this._errorForeground != null)
                {
                    return this._errorForeground.Value;
                }
                else
                {
                    return default(TColor);
                }
            }
            set
            {
                if (this._errorForeground != null)
                {
                    this._errorForeground = value;
                    this.RaiseChanged();
                }
                else
                {
                    if (!this._errorForeground.Equals(value))
                    {
                        this._errorForeground = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the background colour for error highlights
        /// </summary>
        public TColor ErrorBackground
        {
            get
            {
                if (this._errorBackground != null)
                {
                    return this._errorBackground.Value;
                }
                else
                {
                    return default(TColor);
                }
            }
            set
            {
                if (this._errorBackground != null)
                {
                    this._errorBackground = value;
                    this.RaiseChanged();
                }
                else
                {
                    if (!this._errorBackground.Equals(value))
                    {
                        this._errorBackground = value;
                        this.RaiseChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the decoration for error highlights
        /// </summary>
        public String ErrorDecoration
        {
            get
            {
                return this._errorDecoration;
            }
            set
            {
                if (value != this._errorDecoration)
                {
                    this._errorDecoration = value;
                    this.RaiseChanged();
                }
            }
        }

        /// <summary>
        /// Event which is raised whenever options change
        /// </summary>
        public event OptionsChanged Changed;

        /// <summary>
        /// Helper method for raising the options changed event
        /// </summary>
        protected void RaiseChanged()
        {
            OptionsChanged d = this.Changed;
            if (d != null) d();
        }
    }

    /// <summary>
    /// Delegate for option changed events
    /// </summary>
    public delegate void OptionsChanged();
}
