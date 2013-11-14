using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Callbacks for methods that process the resulting triples with an RDF Handler asynchronously
    /// </summary>
    /// <param name="handler">RDF Handler</param>
    /// <param name="state">State</param>
    public delegate void RdfHandlerCallback(IRdfHandler handler, Object state);
}
