using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler that ignores everything it handles
    /// </summary>
    /// <remarks>
    /// Useful if you simply want to parse some RDF to see if it parses and don't care about the actual data being parsed
    /// </remarks>
    public class NullHandler : BaseRdfHandler
    {
        public NullHandler()
        { }

        protected override bool HandleTripleInternal(Triple t)
        {
            return true;
        }
    }
}
