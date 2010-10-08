using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.Alexandria;
using VDS.Alexandria.Datasets;

namespace alexandria_tests
{
    [TestClass]
    public class SparqlTests
    {

        #region File System Store

        [TestMethod]
        public void FSSparqlSelect()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            TestWrapper<StreamReader, TextWriter> wrapper = new TestWrapper<StreamReader, TextWriter>(manager);
            wrapper.DocumentManager.Flush();
            wrapper.IndexManager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }

            manager.Dispose();
        }

        #endregion

        #region MongoDB Store

        [TestMethod]
        public void MongoSparqlSelect()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            wrapper.DocumentManager.Flush();
            wrapper.IndexManager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }

            manager.Dispose();
        }

        #endregion
    }
}
