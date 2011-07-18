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
