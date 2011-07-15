using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class Editor
    {
        private DocumentManager _docManager = new DocumentManager();
        private ITextEditorAdaptorFactory _factory;

        public Editor(ITextEditorAdaptorFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;
        }

        public DocumentManager DocumentManager
        {
            get
            {
                return this._docManager;
            }
        }

        public ITextEditorAdaptorFactory TextEditorFactory
        {
            get
            {
                return this._factory;
            }
        }
    }
}
