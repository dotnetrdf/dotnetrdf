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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Wpf.Syntax
{
    /// <summary>
    /// Element that represents an error highlight
    /// </summary>
    public class ValidationErrorLineText
        : VisualLineText
    {
        private VisualOptions<FontFamily, Color> _options;

        /// <summary>
        /// Creates an element
        /// </summary>
        /// <param name="options">Visual Options</param>
        /// <param name="parentLine">Visual Line</param>
        /// <param name="length">Element Length</param>
        public ValidationErrorLineText(VisualOptions<FontFamily, Color> options, VisualLine parentLine, int length)
            : base(parentLine, length)
        {
            this._options = options;
        }

        /// <summary>
        /// Creates a text run
        /// </summary>
        /// <param name="startVisualColumn">Staring visual column</param>
        /// <param name="context">Context</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns></returns>
        protected override VisualLineText CreateInstance(int length)
        {
            return new ValidationErrorLineText(this._options, this.ParentVisualLine, length);
        }

        /// <summary>
        /// Handle query cursor event
        /// </summary>
        /// <param name="e">Event Arguments</param>
        protected override void OnQueryCursor(System.Windows.Input.QueryCursorEventArgs e)
        {
            e.Handled = true;
        }
    }
}
