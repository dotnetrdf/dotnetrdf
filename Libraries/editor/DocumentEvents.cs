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
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Event arguments for document changed events
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class DocumentChangedEventArgs<T>
        : EventArgs
    {
        private Document<T> _doc;

        /// <summary>
        /// Creates new event arguments
        /// </summary>
        /// <param name="doc">Document</param>
        public DocumentChangedEventArgs(Document<T> doc)
        {
            this._doc = doc;
        }

        /// <summary>
        /// Gets the affected document
        /// </summary>
        public Document<T> Document
        {
            get
            {
                return this._doc;
            }
        }
    }

    /// <summary>
    /// Event arguments for document validation events
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class DocumentValidatedEventArgs<T>
        : DocumentChangedEventArgs<T>
    {
        private ISyntaxValidationResults _results;

        /// <summary>
        /// Creates new event arguments
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="results">Syntax Validation Results</param>
        public DocumentValidatedEventArgs(Document<T> doc, ISyntaxValidationResults results)
            : base(doc)
        {
            this._results = results;
        }

        /// <summary>
        /// Gets the validation results
        /// </summary>
        public ISyntaxValidationResults ValidationResults
        {
            get
            {
                return this._results;
            }
        }
    }

    /// <summary>
    /// Delegate type for document change events handlers
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="sender">Sender</param>
    /// <param name="args">Event Arguments</param>
    public delegate void DocumentChangedHandler<T>(Object sender, DocumentChangedEventArgs<T> args);

    /// <summary>
    /// Delegate type for document validation events handlers
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="sender">Sender</param>
    /// <param name="args">Event Arguments</param>
    public delegate void DocumentValidatedHandler<T>(Object sender, DocumentValidatedEventArgs<T> args);

    /// <summary>
    /// Delegate for document callbacks
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="doc">Document</param>
    /// <returns></returns>
    public delegate bool DocumentCallback<T>(Document<T> doc);

    /// <summary>
    /// Possible Save Changes Modes
    /// </summary>
    public enum SaveChangesMode
    {
        /// <summary>
        /// Save the Changes
        /// </summary>
        Save,
        /// <summary>
        /// Discard the Changes
        /// </summary>
        Discard,
        /// <summary>
        /// Cancel
        /// </summary>
        Cancel
    }

    /// <summary>
    /// Delegate for save changes callback
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="doc">Document</param>
    /// <returns>What to do with the changes</returns>
    public delegate SaveChangesMode SaveChangesCallback<T>(Document<T> doc);

    /// <summary>
    /// Delegate for save as callback
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    /// <param name="doc">Document</param>
    /// <returns>Filename to save as</returns>
    public delegate String SaveAsCallback<T>(Document<T> doc);
}
