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
    /// Event arguments for text editor events
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class TextEditorEventArgs<T> 
        : EventArgs
    {
        /// <summary>
        /// Creates new event arguments
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public TextEditorEventArgs(ITextEditorAdaptor<T> editor)
        {
            this.TextEditor = editor;
        }

        /// <summary>
        /// Gets the text editor that was affected by the event
        /// </summary>
        public ITextEditorAdaptor<T> TextEditor
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Delegate for text editor events
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="sender">Sender</param>
    /// <param name="args">Event Arguments</param>
    public delegate void TextEditorEventHandler<T>(Object sender, TextEditorEventArgs<T> args);
}
