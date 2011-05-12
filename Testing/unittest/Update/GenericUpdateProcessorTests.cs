using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public abstract class GenericUpdateProcessorTests
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        protected abstract IGenericIOManager GetManager();

        [TestMethod]
        public void SparqlUpdateGenericCreateAndInsertData()
        {
            IGenericIOManager manager = this.GetManager();
            GenericUpdateProcessor processor = new GenericUpdateProcessor(manager);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString("CREATE GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(g);

            Assert.IsFalse(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "[" + manager.ToString() + "] Graph should have 1 Triple");
        }

        [TestMethod]
        public void SparqlUpdateGenericCreateInsertDeleteData()
        {
            IGenericIOManager manager = this.GetManager();
            GenericUpdateProcessor processor = new GenericUpdateProcessor(manager);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString("CREATE GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(g);

            Assert.IsFalse(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "[" + manager.ToString() + "] Graph should have 1 Triple");

            cmds = this._parser.ParseFromString("DELETE DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");
            processor.ProcessCommandSet(cmds);

            Graph h = new Graph();
            manager.LoadGraph(h, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(h);

            Assert.IsTrue(h.IsEmpty, "[" + manager.ToString() + "] Graph should be empty");
        }
    }
}
