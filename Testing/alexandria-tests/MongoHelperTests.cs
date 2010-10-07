using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.Alexandria.Utilities;

namespace alexandria_tests
{
    [TestClass]
    public class MongoHelperTests
    {
        [TestMethod]
        public void MongoJsonToDocument()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            String json = StringWriter.Write(g, new JsonNTriplesWriter());
            Console.WriteLine(json);
            Console.WriteLine();

            Document doc = new Document();
            doc["graph"] = MongoDBHelper.JsonArrayToObjects(json);
            Console.WriteLine(doc.ToString());
            Console.WriteLine();

            String json2 = MongoDBHelper.DocumentToJson(doc);
            Console.WriteLine(json2);
        }
    }
}
