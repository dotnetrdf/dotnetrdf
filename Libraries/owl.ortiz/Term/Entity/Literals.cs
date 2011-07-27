using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface ILiteral
        : ILiteralExpression
    {
        String LexicalValue
        {
            get;
        }

        bool IsTyped
        {
            get;
        }
    }

    public interface IPlainLiteral
        : ILiteral
    {
        String Language
        {
            get;
        }

        bool HasLanguage
        {
            get;
        }
    }

    public interface ITypedLiteral
        : ILiteral
    {
        INamedDataType DataType
        {
            get;
        }
    }
}
