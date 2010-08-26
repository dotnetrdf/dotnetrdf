using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.Syntax
{
    public interface ISyntaxValidator
    {
        bool Validate(String data, out String message);
    }
}
