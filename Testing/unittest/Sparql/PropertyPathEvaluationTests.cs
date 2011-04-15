using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class PropertyPathEvaluationTests
    {
        private NodeFactory _factory = new NodeFactory();
        private ISparqlDataset _data;

        private ISparqlAlgebra GetAlgebra(ISparqlPath path)
        {
            return GetAlgebra(path, null, null);
        }

        private ISparqlAlgebra GetAlgebra(ISparqlPath path, INode start, INode end)
        {
            PatternItem x, y;
            if (start == null)
            {
                x = new VariablePattern("?x");
            }
            else
            {
                x = new NodeMatchPattern(start);
            }
            if (end == null)
            {
                y = new VariablePattern("?y");
            }
            else
            {
                y = new NodeMatchPattern(end);
            }
            PathTransformContext context = new PathTransformContext(x, y);
            return path.ToAlgebraOperator(context);
        }

        private void EnsureTestData()
        {
            if (this._data == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                this._data = new InMemoryDataset(store);
            }
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthAll()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermEnd()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")));
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");

            INode rdfType = this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsClass = this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));

            Assert.IsTrue(results.Sets.All(s => this._data.ContainsTriple(new Triple(s["x"].CopyNode(null), rdfType, rdfsClass))), "All returned values should be of rdf:type rdfs:Class");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermStart()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path, ConfigurationLoader.CreateConfigurationNode(new Graph(), ConfigurationLoader.ClassHttpHandler), null);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [TestMethod]
        public void SparqlPropertyPathEvaluationZeroLengthWithBothTerms()
        {
            EnsureTestData();

            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path, ConfigurationLoader.CreateConfigurationNode(new Graph(), ConfigurationLoader.ClassHttpHandler), this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")));
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
            Assert.IsTrue(results is IdentityMultiset, "Results should be Identity");
        }
    }
}
