using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.Selection
{
    public class WhiteSpaceSelector : BaseSelector
    {
        protected override bool IsStartingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsStartingDeliminator(c);
        }

        protected override bool IsEndingDeliminator(char c)
        {
            return Char.IsWhiteSpace(c) || base.IsEndingDeliminator(c);
        }
    }
}
