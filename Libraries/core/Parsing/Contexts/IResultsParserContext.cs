using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Contexts
{
    public interface IResultsParserContext
    {
        ISparqlResultsHandler Handler
        {
            get;
        }
    }
}
