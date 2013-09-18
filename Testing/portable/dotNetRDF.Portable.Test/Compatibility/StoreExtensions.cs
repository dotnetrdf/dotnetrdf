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
            IStoreReader parser = null;
            using (var input = new StreamReader(fileName))
            {
                string ext = MimeTypesHelper.GetTrueFileExtension(fileName);
                try
                {
                    parser = MimeTypesHelper.GetStoreParserByFileExtension(ext);
                }
                catch (RdfParserSelectionException)
                {
                    try
                    {
                        IRdfReader rdfParser = MimeTypesHelper.GetParserByFileExtension(ext);
                        var storeHandler = new StoreHandler(store);
                        rdfParser.Load(storeHandler, input);
                        return;
                    }
                    catch (RdfParserSelectionException)
                    {
                        // Ignore this. Will try and use format guessing and assume it is a dataset format
                    }
                }
                if (parser == null)
                {
                    string data = input.ReadToEnd();
                    input.Close();
                    parser = StringParser.GetDatasetParser(data);
                    var handler = new StoreHandler(store);
                    parser.Load(handler, new StringReader(data));
                }
                else
                {
                    parser.Load(new StoreHandler(store), input );
                }
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
