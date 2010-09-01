using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using rdfEditor.AutoComplete.Data;

namespace rdfEditor.AutoComplete
{
    public class SparqlUpdateAutoCompleter : SparqlAutoCompleter
    {
        public SparqlUpdateAutoCompleter()
            : base(null)
        {
            foreach (String keyword in SparqlSpecsHelper.SparqlUpdate11Keywords)
            {
                this._keywords.Add(new KeywordCompletionData(keyword));
                this._keywords.Add(new KeywordCompletionData(keyword.ToLower()));
            }
        }

        public override object Clone()
        {
            return new SparqlUpdateAutoCompleter();
        }
    }
}
