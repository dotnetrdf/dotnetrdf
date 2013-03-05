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
using System.Windows.Media;
using AvComplete = ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;

namespace VDS.RDF.Utilities.Editor.Wpf.AutoComplete
{
    /// <summary>
    /// Wrapper around standard <see cref="ICompletionData"/> to put it into the AvalonEdit framework
    /// </summary>
    public class WpfCompletionData
        : AvComplete.ICompletionData
    {
        private ICompletionData _data;

        /// <summary>
        /// Create new completion data
        /// </summary>
        /// <param name="data">Underlying data</param>
        public WpfCompletionData(ICompletionData data)
        {
            this._data = data;
        }

        /// <summary>
        /// Perform auto-complete
        /// </summary>
        /// <param name="textArea">Text Area</param>
        /// <param name="completionSegment">Completion Segmnet</param>
        /// <param name="insertionRequestEventArgs">Event Arguments</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        /// <summary>
        /// Gets the content to display
        /// </summary>
        public object Content
        {
            get 
            {
                return this._data.DisplayText; 
            }
        }

        /// <summary>
        /// Gets the description to display
        /// </summary>
        public object Description
        {
            get 
            {
                return this._data.Description; 
            }
        }

        /// <summary>
        /// Gets the image to display
        /// </summary>
        public ImageSource Image
        {
            get 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the priority of the data
        /// </summary>
        public double Priority
        {
            get 
            {
                return this._data.Priority; 
            }
        }

        /// <summary>
        /// Gets the actual text to insert
        /// </summary>
        public string Text
        {
            get 
            {
                return this._data.InsertionText; 
            }
        }
    }
}
