using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentChangedEventArgs<T>
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

    public delegate void DocumentChangedHandler<T>(Object sender, DocumentChangedEventArgs<T> args);
}
