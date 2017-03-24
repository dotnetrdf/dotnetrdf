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
    /// Interface for text editor factories
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public interface ITextEditorAdaptorFactory<T>
    {
        /// <summary>
        /// Create a new text editor
        /// </summary>
        /// <returns>New Text Editor</returns>
        ITextEditorAdaptor<T> CreateAdaptor();
    }

    /// <summary>
    /// Interface for text editor factories that support visual options
    /// </summary>
    /// <typeparam name="TControl">Control Type</typeparam>
    /// <typeparam name="TFont">Font Type</typeparam>
    /// <typeparam name="TColor">Colour Type</typeparam>
    public interface IVisualTextEditorAdaptorFactory<TControl, TFont, TColor>
        : ITextEditorAdaptorFactory<TControl>
          where TFont : class
          where TColor : struct
    {
        /// <summary>
        /// Gets the default visual options
        /// </summary>
        /// <returns>Default Visual Options</returns>
        VisualOptions<TFont, TColor> GetDefaultVisualOptions();
    }
}
