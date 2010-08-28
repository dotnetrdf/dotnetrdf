using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.AutoComplete
{
    public enum AutoCompleteState
    {
        Disabled,
        None,
        Inserted,

        QName,
        Prefix,
        Uri,
        Keyword,
        KeywordOrQName,
        BNode,
        Literal,
        LongLiteral,
        Comment,

        XmlAttribute
    }
}
