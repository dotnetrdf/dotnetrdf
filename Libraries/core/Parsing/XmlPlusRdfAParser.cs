using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events.RdfA;

namespace VDS.RDF.Parsing
{
    public class XmlPlusRdfAParser : RdfACoreParser
    {
        protected override RdfACoreParserContext GetParserContext(IRdfHandler handler, TextReader reader)
        {
            return new RdfACoreParserContext(handler, new XmlHost(), reader);
        }
    }
}
