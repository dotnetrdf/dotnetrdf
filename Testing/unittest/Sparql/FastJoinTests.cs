using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class FastJoinTests
    {
        private InMemoryDataset _dataset;
        private NodeFactory _factory = new NodeFactory();

        private void Test(ISparqlAlgebra slow, ISparqlAlgebra fast)
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                this._dataset = new InMemoryDataset(store);
            }

            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._dataset);

            //Execute the Slower form of the Query first
            Stopwatch slowTime = new Stopwatch();
            slowTime.Start();
            BaseMultiset slowResult = slow.Evaluate(context);
            slowTime.Stop();
            Console.WriteLine("Slow Query returned " + slowResult.Count + " Result(s) in " + slowTime.Elapsed);

            //Execute the Faster form of the query
            Stopwatch fastTime = new Stopwatch();
            fastTime.Start();
            BaseMultiset fastResult = fast.Evaluate(context);
            fastTime.Stop();
            Console.WriteLine("Fast Query returned " + fastResult.Count + " Result(s) in " + fastTime.Elapsed);

            if (slowResult.Count != fastResult.Count)
            {
                //Results were different so print for comparison
                Console.WriteLine("Slow Results:");
                TestTools.ShowMultiset(slowResult);
                Console.WriteLine("Fast Results:");
                TestTools.ShowMultiset(fastResult);
            }

            Assert.AreEqual(slowResult.Count, fastResult.Count, "Result Counts should be equal");
            Assert.IsTrue(fastTime.Elapsed < slowTime.Elapsed, "Expected Fast Query to be faster");
        }

        [TestMethod]
        public void SparqlFastJoinSimple1()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));

            IBgp lhs = new Bgp(p1);
            IBgp rhs = new Bgp(p2);

            ISparqlAlgebra currJoin = new Join(lhs, rhs);
            ISparqlAlgebra fastJoin = new FastJoin(lhs, rhs);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinSimple2()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "comment"))), new VariablePattern("?comment"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);

            ISparqlAlgebra currJoin = new Join(new Join(a, b), c);
            ISparqlAlgebra fastJoin = new FastJoin(new FastJoin(a, b), c);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinSimple3()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "comment"))), new VariablePattern("?comment"));
            TriplePattern p4 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);
            IBgp d = new Bgp(p4);

            ISparqlAlgebra currJoin = new Join(new Join(new Join(a, b), c), d);
            ISparqlAlgebra fastJoin = new FastJoin(new FastJoin(new FastJoin(a, b), c), d);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinSimple4()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "comment"))), new VariablePattern("?comment"));
            TriplePattern p4 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));
            TriplePattern p5 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"))), new VariablePattern("?domain"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);
            IBgp d = new Bgp(p4);
            IBgp e = new Bgp(p5);

            ISparqlAlgebra currJoin = new Join(new Join(new Join(new Join(a, b), c), d), e);
            ISparqlAlgebra fastJoin = new FastJoin(new FastJoin(new FastJoin(new FastJoin(a, b), c), d), e);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinSimple5()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "comment"))), new VariablePattern("?comment"));
            TriplePattern p4 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"))), new VariablePattern("?domain"));
            TriplePattern p5 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);
            IBgp d = new Bgp(p4);
            IBgp e = new Bgp(p5);

            ISparqlAlgebra currJoin = new Join(new Join(new Join(new Join(a, b), c), d), e);
            ISparqlAlgebra fastJoin = new FastJoin(new FastJoin(new FastJoin(new FastJoin(a, b), c), d), e);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinSimple6()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "domain"))), new VariablePattern("?domain"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);

            ISparqlAlgebra currJoin = new Join(a, b);
            ISparqlAlgebra fastJoin = new FastJoin(a, b);

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinComplex1()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));

            IBgp lhs = new Bgp(p1);
            IBgp rhs = new Bgp(p2);

            ISparqlAlgebra currJoin = new Join(lhs, rhs);
            ISparqlAlgebra fastJoin = new FastJoin(lhs, rhs);

            this.Test(currJoin, fastJoin);
        }
    }
}
