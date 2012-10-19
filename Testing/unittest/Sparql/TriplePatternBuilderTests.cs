using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class TriplePatternBuilderTests
    {
        private TriplePatternBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new TriplePatternBuilder();
        }

        [TestMethod]
        public void CanCreateTriplePatternUsingVariableNames()
        {
            // when
            _builder.Subject("s").Predicate("p").Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }
    }
}