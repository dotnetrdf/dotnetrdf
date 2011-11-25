using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public enum AutoCompleteState
    {
        Disabled,
        None,
        Inserted,

        Prefix,
        Base,
        Declaration,

        Uri,
        QName,
        Keyword,
        KeywordOrQName,

        BNode,

        Variable,

        Literal,
        LongLiteral,
        AlternateLiteral,
        AlternateLongLiteral,
        NumericLiteral,

        Comment
    }
}
