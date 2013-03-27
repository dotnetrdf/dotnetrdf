using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF
{
    public static class StoreReaderExtensions
    {
        public static void Load(this IStoreReader reader, ITripleStore store, string filename)
        {
            using (var input = new StreamReader(filename))
            {
                reader.Load(store, input);
            }
        }

        public static void Load(this IStoreReader reader, IRdfHandler handler, string filename)
        {
            using (var input = new StreamReader(filename))
            {
                reader.Load(handler, input);
            }
        }
    }
}
