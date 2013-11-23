using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Contexts
{
    public class TriXParserContext
        : BaseParserContext
    {
        public TriXParserContext(IRdfHandler handler) 
            : base(handler, false) { }
    }
}
