using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    [TestClass]
    public class SparqlCastTests : BaseTest
    {
        private INodeFactory _graph;

        [TestInitialize]
        public void Setup()
        {
            _graph = new Graph();
        }

        [TestMethod]
        public void ShouldSuccesfullyEvaluateDecimalCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new DecimalCast(new ConstantTerm(3.4m.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.AreEqual(3.4m, valuedNode.AsDecimal());
                });
            }
        }

        [TestMethod]
        public void ShouldSuccesfullyEvaluateDoubleCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new DoubleCast(new ConstantTerm(3.4d.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.AreEqual(3.4d, valuedNode.AsDouble());
                });
            }
        }

        [TestMethod]
        public void ShouldSuccesfullyEvaluateFloatCastRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    // given
                    var cast = new FloatCast(new ConstantTerm(3.4f.ToLiteral(_graph)));

                    // when
                    IValuedNode valuedNode = cast.Evaluate(new SparqlEvaluationContext(new SparqlQuery(), new InMemoryDataset()), 0);

                    // then
                    Assert.AreEqual(3.4f, valuedNode.AsFloat());
                });
            }
        } 
    }
}