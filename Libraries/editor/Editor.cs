using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public class Editor<TControl, TFont, TColor>
        where TFont : class
        where TColor : struct
    {
        private DocumentManager<TControl, TFont, TColor> _docManager;
        private ITextEditorAdaptorFactory<TControl> _factory;

        public Editor(ITextEditorAdaptorFactory<TControl> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;
            this._docManager = new DocumentManager<TControl, TFont, TColor>(this._factory);
        }

        public DocumentManager<TControl, TFont, TColor> DocumentManager
        {
            get
            {
                return this._docManager;
            }
        }

        public ITextEditorAdaptorFactory<TControl> TextEditorFactory
        {
            get
            {
                return this._factory;
            }
        }
    }
}
