using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Medusa
{
    [TestClass]
    public class MedusaQueryEngineTests
    {
        [TestMethod]
        public void SparqlMedusaAsk1()
        {
            ISparqlAlgebra ask = new Ask(new Bgp());
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset());

            IEnumerable<ISet> sets = processor.ProcessAlgebra(ask, null);
            Assert.IsTrue(sets.Any());
        }

        [TestMethod]
        public void SparqlMedusaBgpSimple1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra bgp = new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")));
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(bgp, null);
            Assert.IsTrue(sets.Any());
            Assert.AreEqual(g.Triples.Count, sets.Count());
        }

        [TestMethod]
        public void SparqlMedusaSlice1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra slice = new Slice(new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"))), 10, 0);
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(slice, null);
            Assert.IsTrue(sets.Any());
            Assert.AreEqual(10, sets.Count());
        }

        [TestMethod]
        public void SparqlMedusaSlice2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra slice = new Slice(new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"))), 0, 0);
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(slice, null);
            Assert.IsFalse(sets.Any());
            Assert.AreEqual(0, sets.Count());
        }

        [TestMethod]
        public void SparqlMedusaSlice3()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra slice = new Slice(new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"))), -1, g.Triples.Count - 3);
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(slice, null);
            Assert.IsTrue(sets.Any());
            Assert.AreEqual(3, sets.Count());
        }
    }
}
