using System.IO;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    public static class SparqlIOExtensions
    {
        public static void Load(this ISparqlResultsReader parser, ISparqlResultsHandler handler, TextReader input)
        {
            parser.Load(handler, input, new ParserProfile());
        }
    }
}
