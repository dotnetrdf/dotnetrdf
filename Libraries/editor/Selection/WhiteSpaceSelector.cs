using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.Selection
{
    public class WhiteSpaceSelector<T> : DefaultSelector<T>
    {
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsStartingDeliminator(c);
        }

        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsEndingDeliminator(c);
        }

        protected override char? RequireMatchingDeliminator(char c)
        {
            return null;
        }
    }

    public class PunctuationSelector<T> : DefaultSelector<T>
    {
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsPunctuation(c) || base.IsStartingDeliminator(c);
        }

        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsPunctuation(c) || base.IsEndingDeliminator(c);
        }

        protected override char? RequireMatchingDeliminator(char c)
        {
            return null;
        }
    }

    public class WhiteSpaceOrPunctuationSelection<T> : DefaultSelector<T>
    {
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || Char.IsPunctuation(c) || base.IsStartingDeliminator(c);
        }

        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || Char.IsPunctuation(c) || base.IsEndingDeliminator(c);
        }

        protected override char? RequireMatchingDeliminator(char c)
        {
            return null;
        }
    }
}
