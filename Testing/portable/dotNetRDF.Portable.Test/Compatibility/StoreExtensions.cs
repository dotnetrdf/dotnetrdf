using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF
{
    public static class StoreExtensions
    {
        public static void LoadFromFile(this ITripleStore store, string fileName)
        {
            using (var input = new StreamReader(fileName))
            {
                IStoreReader reader =
                    MimeTypesHelper.GetStoreParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(fileName));
                reader.Load(store, input);
            }
        }

        public static void SaveToFile(this ITripleStore store, string fileName)
        {
            using (var output = new StreamWriter(fileName)) 
            {
                IStoreWriter writer = MimeTypesHelper.GetStoreWriterByFileExtension(MimeTypesHelper.GetTrueFileExtension(fileName));
                writer.Save(store, output);
            }
        }
    }
}
