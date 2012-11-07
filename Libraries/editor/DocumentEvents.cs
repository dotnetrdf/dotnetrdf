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
    public class DocumentChangedEventArgs<T> : EventArgs
    {
        private Document<T> _doc;

        public DocumentChangedEventArgs(Document<T> doc)
        {
            this._doc = doc;
        }

        public Document<T> Document
        {
            get
            {
                return this._doc;
            }
        }
    }

    public class DocumentValidatedEventArgs<T> : DocumentChangedEventArgs<T>
    {
        private ISyntaxValidationResults _results;

        public DocumentValidatedEventArgs(Document<T> doc, ISyntaxValidationResults results)
            : base(doc)
        {
            this._results = results;
        }

        public ISyntaxValidationResults ValidationResults
        {
            get
            {
                return this._results;
            }
        }
    }

    public delegate void DocumentChangedHandler<T>(Object sender, DocumentChangedEventArgs<T> args);

    public delegate void DocumentValidatedHandler<T>(Object sender, DocumentValidatedEventArgs<T> args);

    public delegate bool DocumentCallback<T>(Document<T> doc);

    public enum SaveChangesMode
    {
        Save,
        Discard,
        Cancel
    }

    public delegate SaveChangesMode SaveChangesCallback<T>(Document<T> doc);

    public delegate String SaveAsCallback<T>(Document<T> doc);
}
