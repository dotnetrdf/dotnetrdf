using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class KeywordData : BaseCompletionData
    {
        public KeywordData(String keyword)
            : this(keyword, "The " + keyword + " Keyword") { }

        public KeywordData(String keyword, String description)
            : base(keyword, keyword, description) { }
    }
}
