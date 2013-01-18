using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
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

        [TestMethod]
        public void SparqlMedusaUnion1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra union = new Union(new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")))
                                             , new Bgp(new TriplePattern(new VariablePattern("x"), new VariablePattern("y"), new VariablePattern("z"))));
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(union, null);
            Assert.IsTrue(sets.Any());
            Assert.AreEqual(g.Triples.Count * 2, sets.Count());
        }

        [TestMethod]
        public void SparqlMedusaFilter1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra filter = new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")));
            filter = new Filter(filter, new UnaryExpressionFilter(new IsBlankFunction(new VariableTerm("s"))));
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(filter, null);
            Assert.IsTrue(sets.Any());
            Assert.AreEqual(g.Triples.Where(t => t.Subject.NodeType == NodeType.Blank).Count(), sets.Count());
        }

        [TestMethod]
        public void SparqlMedusaExtend1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            ISparqlAlgebra extend = new Bgp(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")));
            extend = new Extend(extend, new IsBlankFunction(new VariableTerm("s")), "blank");
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset(g));

            IEnumerable<ISet> sets = processor.ProcessAlgebra(extend, null);
            Assert.IsTrue(sets.Any());
            Assert.IsTrue(sets.Any(s => s.ContainsVariable("blank")));
            Assert.AreEqual(g.Triples.Where(t => t.Subject.NodeType == NodeType.Blank).Count(), sets.Where(s => s["blank"].Equals((true).ToLiteral(g))).Count());
        }
    }
}
