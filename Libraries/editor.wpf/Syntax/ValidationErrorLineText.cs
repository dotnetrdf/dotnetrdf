/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
