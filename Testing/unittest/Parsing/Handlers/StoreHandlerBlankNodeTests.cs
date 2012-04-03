using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class StoreHandlerBlankNodeTests
    {
        private const String TestFragment = @"@prefix : <http://example.org/bnodes#> .
{
  [] a :BNode , :AnonymousBNode .
  _:autos1 a :Node , :NamedBNode .
}
:graph {
  [] a :BNode , :AnonymousBNode .
  _:autos1 a :Node , :NamedBNode .
}
";
        private void EnsureTestData(String testFile)
        {
            if (!File.Exists(testFile))
            {
                TriGParser parser = new TriGParser();
                TripleStore store = new TripleStore();
                parser.Load(store, new TextReaderParams(new StringReader(TestFragment)));

                store.SaveToFile(testFile);
            }
        }

        private void EnsureTestResults(TripleStore store)
        {
            foreach (IGraph g in store.Graphs)
            {
                TestTools.ShowGraph(g);
                Console.WriteLine();
            }

            Assert.AreEqual(2, store.Graphs.Count, "Expected 2 Graphs");
            Assert.AreEqual(8, store.Graphs.Sum(g => g.Triples.Count), "Expected 4 Triples");

            IGraph def = store.Graph(null);
            IGraph named = store.Graph(new Uri("http://example.org/bnodes#graph"));

            HashSet<INode> subjects = new HashSet<INode>();
            foreach (Triple t in def.Triples)
            {
                subjects.Add(t.Subject);
            }
            foreach (Triple t in named.Triples)
            {
                subjects.Add(t.Subject);
            }

            Console.WriteLine("Subjects:");
            foreach (INode subj in subjects)
            {
                Console.WriteLine(subj.ToString() + " from Graph " + (subj.GraphUri != null ? subj.GraphUri.ToString() : "Default"));
            }
            Assert.AreEqual(4, subjects.Count, "Expected 4 distinct subjects");
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesTriG()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesTriGActual));
        }

        public void ParsingStoreHandlerBlankNodesTriGActual()
        {
            EnsureTestData("test-bnodes.trig");

            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.trig");

            EnsureTestResults(store);
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesTriX()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesTriXActual));
        }

        public void ParsingStoreHandlerBlankNodesTriXActual()
        {
            EnsureTestData("test-bnodes.xml");

            TriXParser parser = new TriXParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.xml");

            EnsureTestResults(store);
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesNQuads()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesNQuadsActual));
        }

        public void ParsingStoreHandlerBlankNodesNQuadsActual()
        {
            EnsureTestData("test-bnodes.nq");

            NQuadsParser parser = new NQuadsParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.nq");

            EnsureTestResults(store);
        }
    }
}
