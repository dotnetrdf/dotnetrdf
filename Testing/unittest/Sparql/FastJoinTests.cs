/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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

//This is not compiled currently as this was used while prototyping FastJoin which has been removed from the core library
//as the algorithm is now fully integrated rather than a custom Algebra operator

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class FastJoinTests
    {
        private InMemoryDataset _dataset;
        private NodeFactory _factory = new NodeFactory();

        private void SetupDataset()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                this._dataset = new InMemoryDataset(store);
            }
        }

        private void Test(ISparqlAlgebra slow, ISparqlAlgebra fast)
        {
            this.SetupDataset();

            //Execute the Slower form of the Query first
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._dataset);
            Stopwatch slowTime = new Stopwatch();
            slowTime.Start();
            BaseMultiset slowResult = slow.Evaluate(context);
            slowTime.Stop();
            Console.WriteLine("Slow Query returned " + slowResult.Count + " Result(s) in " + slowTime.Elapsed);

            //Execute the Faster form of the query
            context = new SparqlEvaluationContext(null, this._dataset);
            Stopwatch fastTime = new Stopwatch();
            fastTime.Start();
            BaseMultiset fastResult = fast.Evaluate(context);
            fastTime.Stop();
            Console.WriteLine("Fast Query returned " + fastResult.Count + " Result(s) in " + fastTime.Elapsed);
            Console.WriteLine();

                Console.WriteLine("Slow Results:");
                TestTools.ShowMultiset(slowResult);
                Console.WriteLine();
                Console.WriteLine("Fast Results:");
                TestTools.ShowMultiset(fastResult);

            Assert.AreEqual(slowResult.Count, fastResult.Count, "Result Counts should be equal");
            Assert.IsTrue(fastTime.Elapsed < slowTime.Elapsed, "Expected Fast Query to be faster");
        }

        private void TestCorrectness(ISparqlAlgebra algebra, int expected)
        {
            this.SetupDataset();

            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._dataset);
            BaseMultiset results = algebra.Evaluate(context);
            Console.WriteLine("Algebra generated " + results.Count + " Result(s)");
            Assert.AreEqual(expected, results.Count);
        }

        [TestMethod]
        public void SparqlJoinCorrectness1()
        {
            this.SetupDataset();

            TriplePattern p = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            IBgp bgp = new Bgp(p);

            this.TestCorrectness(bgp, this._dataset.GetTriplesWithPredicate(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))).Count());
        }

        [TestMethod]
        public void SparqlJoinCorrectness2()
        {
            this.SetupDataset();

            TriplePattern p = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            ISparqlAlgebra algebra = new Distinct(new Bgp(p));

            this.TestCorrectness(algebra, this._dataset.GetTriplesWithPredicate(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))).Count());
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

        [TestMethod]
        public void SparqlFastJoinComplex2()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);

            ISparqlAlgebra currJoin = new Join(a, new LeftJoin(b, c));
            ISparqlAlgebra fastJoin = new FastJoin(a, new LeftJoin(b, c));

            this.Test(currJoin, fastJoin);
        }

        [TestMethod]
        public void SparqlFastJoinComplex3()
        {
            TriplePattern p1 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType))), new VariablePattern("?type"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"))), new VariablePattern("?label"));
            TriplePattern p3 = new TriplePattern(new VariablePattern("?s"), new NodeMatchPattern(this._factory.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "range"))), new VariablePattern("?range"));

            IBgp a = new Bgp(p1);
            IBgp b = new Bgp(p2);
            IBgp c = new Bgp(p3);

            ISparqlAlgebra currJoin = new Join(new LeftJoin(b, c), a);
            ISparqlAlgebra fastJoin = new FastJoin(new LeftJoin(b, c), a);

            this.Test(currJoin, fastJoin);
        }
    }
}
