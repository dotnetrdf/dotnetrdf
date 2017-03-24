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
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace VDS.RDF.Utilities.Editor.Wpf.Syntax
{
    /// <summary>
    /// A custom highlighting brush that uses a solid colour brush
    /// </summary>
    public class CustomHighlightingBrush 
        : HighlightingBrush
    {
        private SolidColorBrush _brush;

        /// <summary>
        /// Creates a new brush
        /// </summary>
        /// <param name="c">Colour</param>
        public CustomHighlightingBrush(Color c)
        {
            this._brush = new SolidColorBrush(c);
            this._brush.Freeze();
        }

        /// <summary>
        /// Creates a new brush from an existing brush
        /// </summary>
        /// <param name="b">Brish</param>
        public CustomHighlightingBrush(SolidColorBrush b)
        {
            this._brush = b;
            this._brush.Freeze();
        }

        /// <summary>
        /// Gets the brush
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns></returns>
        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return this._brush;
        }

        /// <summary>
        /// Gets the string representation of the brush
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._brush.ToString();
        }
    }
}
