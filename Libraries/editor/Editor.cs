using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class Editor<T>
    {
        private DocumentManager<T> _docManager;
        private ITextEditorAdaptorFactory<T> _factory;

        public Editor(ITextEditorAdaptorFactory<T> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;
            this._docManager = new DocumentManager<T>(this._factory);
        }

        public DocumentManager<T> DocumentManager
        {
            get
            {
                return this._docManager;
            }
        }

        public ITextEditorAdaptorFactory<T> TextEditorFactory
        {
            get
            {
                return this._factory;
            }
        }
    }
}
