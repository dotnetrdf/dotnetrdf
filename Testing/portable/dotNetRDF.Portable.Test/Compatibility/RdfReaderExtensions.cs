using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF
{
    public static class RdfHandlerExtensions
    {
        public static void Load(this IRdfHandler handler, string filename)
        {
            using (var textReader = new StreamReader(filename))
            {
                
            }
        }
    }
    public static class RdfReaderExtensions
    {
        public static void Load(this IRdfReader  rdr, Graph g, string fileName)
        {
            using (var textReader = new StreamReader(fileName))
            {
                rdr.Load(g, fileName);
            }
        }

        public static void Load(this IRdfReader parser, GraphHandler handler, string filename)
        {
            using (var inputStream = new StreamReader(filename))
            {
                parser.Load(handler, inputStream);
            }
        }

        public static void Load(this IRdfReader parser, IGraph g, string filename)
        {
            using (var inputStream = new StreamReader(filename))
            {
                parser.Load(g, inputStream);
            }
        }

        public static void Load(this IRdfReader parser, IRdfHandler handler, string filename)
        {
            using (var inputStream = new StreamReader(filename))
            {
                parser.Load(handler, inputStream);
            }
        }

        public static void Load(this IRdfReader parser, ITripleStore store, string filename)
        {
            parser.Load(new StoreHandler(store), filename );
        }
    }

}