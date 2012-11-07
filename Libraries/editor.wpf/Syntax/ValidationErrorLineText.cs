/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Wpf.Syntax
{
    public class ValidationErrorLineText
        : VisualLineText
    {
        private VisualOptions<FontFamily, Color> _options;

        public ValidationErrorLineText(VisualOptions<FontFamily, Color> options, VisualLine parentLine, int length)
            : base(parentLine, length)
        {
            this._options = options;
        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
            if (this._options.ErrorDecoration != null && !this._options.ErrorDecoration.Equals(String.Empty))
            {
                switch (this._options.ErrorDecoration)
                {
                    case "Baseline":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Baseline);
                        break;
                    case "OverLine":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.OverLine);
                        break;
                    case "Strikethrough":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
                        break;
                    case "Underline":
                        this.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                        break;
                }
                
            }
            this.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(this._options.ErrorBackground));
            this.TextRunProperties.SetForegroundBrush(new SolidColorBrush(this._options.ErrorForeground));
            if (this._options.ErrorFontFace != null)
            {
                this.TextRunProperties.SetTypeface(new Typeface(this._options.ErrorFontFace, new FontStyle(), new FontWeight(), new FontStretch()));
            }
            return base.CreateTextRun(startVisualColumn, context);
        }

        protected override VisualLineText CreateInstance(int length)
        {
            return new ValidationErrorLineText(this._options, this.ParentVisualLine, length);
        }

        protected override void OnQueryCursor(System.Windows.Input.QueryCursorEventArgs e)
        {
            e.Handled = true;
            
        }
    }
}
