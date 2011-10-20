using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public class SparqlUpdateAutoCompleter<T>
        : SparqlAutoCompleter<T>
    {
        public SparqlUpdateAutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor, null)
        {
            foreach (String keyword in SparqlSpecsHelper.SparqlUpdate11Keywords)
            {
                this._keywords.Add(new KeywordData(keyword));
                this._keywords.Add(new KeywordData(keyword.ToLower()));
            }
        }
    }
}
