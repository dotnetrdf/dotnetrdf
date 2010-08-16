using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Test
{
    [TestClass()]
    public class UpdateTests
    {
        [TestMethod()]
        public void SparqlUpdateCreateDropTest()
        {
            TripleStore store = new TripleStore();

            Console.WriteLine("Store has " + store.Graphs.Count + " Graphs");

            //Create a couple of Graphs using Create Commands
            CreateCommand create1 = new CreateCommand(new Uri("http://example.org/1"));
            CreateCommand create2 = new CreateCommand(new Uri("http://example.org/2"));

            store.ExecuteUpdate(create1);
            store.ExecuteUpdate(create2);

            Assert.AreEqual(2, store.Graphs.Count, "Store should have now had two Graphs");
            Assert.AreEqual(0, store.Triples.Count(), "Store should have no triples at this point");

            //Trying the same Create again should cause an error
            try
            {
                store.ExecuteUpdate(create1);
                Assert.Fail("Executing a CREATE command twice without the SILENT modifier should error");
            }
            catch (SparqlUpdateException)
            {
                Console.WriteLine("Executing a CREATE command twice without the SILENT modifier errored as expected");
            }

            //Equivalent Create with SILENT should not error
            CreateCommand create3 = new CreateCommand(new Uri("http://example.org/1"), true);
            try
            {
                store.ExecuteUpdate(create3);
                Console.WriteLine("Executing a CREATE for an existing Graph with the SILENT modifier suppressed the error as expected");
            }
            catch (SparqlUpdateException)
            {
                Assert.Fail("Executing a CREATE for an existing Graph with the SILENT modifier should not error");
            }

            DropCommand drop1 = new DropCommand(new Uri("http://example.org/1"));
            store.ExecuteUpdate(drop1);
            Assert.AreEqual(1, store.Graphs.Count, "Store should have only 1 Graph after we executed the DROP command");

            try
            {
                store.ExecuteUpdate(drop1);
                Assert.Fail("Trying to DROP a non-existent Graph should error");
            }
            catch (SparqlUpdateException)
            {
                Console.WriteLine("Trying to DROP a non-existent Graph produced an error as expected");
            }

            DropCommand drop2 = new DropCommand(new Uri("http://example.org/1"), true);
            try
            {
                store.ExecuteUpdate(drop2);
                Console.WriteLine("Trying to DROP a non-existent Graph with the SILENT modifier suppressed the error as expected");
            }
            catch (SparqlUpdateException)
            {
                Assert.Fail("Trying to DROP a non-existent Graph with the SILENT modifier should suppress the error");
            }
        }

        [TestMethod()]
        public void SparqlUpdateLoadTest()
        {
            TripleStore store = new TripleStore();

            LoadCommand loadLondon = new LoadCommand(new Uri("http://dbpedia.org/resource/London"));
            LoadCommand loadSouthampton = new LoadCommand(new Uri("http://dbpedia.org/resource/Southampton"), new Uri("http://example.org"));

            store.ExecuteUpdate(loadLondon);
            store.ExecuteUpdate(loadSouthampton);

            Assert.AreEqual(2, store.Graphs.Count, "Should now be 2 Graphs in the Store");
            Assert.AreNotEqual(0, store.Triples.Count(), "Should be some Triples in the Store");

            foreach (IGraph g in store.Graphs)
            {
                foreach (Triple t in g.Triples)
                {
                    Console.Write(t.ToString());
                    if (g.BaseUri != null)
                    {
                        Console.WriteLine(" from " + g.BaseUri.ToString());
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                }
            }
        }

        [TestMethod()]
        public void SparqlUpdateModifyTest()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;
            store.Add(g);

            UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            Assert.AreNotEqual(0, store.GetTriplesWithPredicate(rdfType).Count(), "Store should contain some rdf:type Triples");

            String update = "DELETE {?s a ?type} WHERE {?s a ?type}";
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(update);
            store.ExecuteUpdate(cmds);

            Assert.AreEqual(0, store.GetTriplesWithPredicate(rdfType).Count(), "Store should contain no rdf:type Triples after DELETE command executes");
        }
    }
}
