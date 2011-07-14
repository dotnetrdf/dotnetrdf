using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class DocumentChangedEventArgs
    {
        private Document _doc;

        public DocumentChangedEventArgs(Document doc)
        {
            this._doc = doc;
        }

        public Document Document
        {
            get
            {
                return this._doc;
            }
        }
    }

    public delegate void DocumentChangedHandler(Object sender, DocumentChangedEventArgs args);
}
