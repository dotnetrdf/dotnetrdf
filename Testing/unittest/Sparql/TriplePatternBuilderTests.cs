using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class TriplePatternBuilderTests
    {
        private TriplePatternBuilder _builder;
        private Mock<INamespaceMapper> _namespaceMapper;

        [TestInitialize]
        public void Setup()
        {
            _namespaceMapper = new Mock<INamespaceMapper>(MockBehavior.Strict);
            _builder = new TriplePatternBuilder(_namespaceMapper.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _namespaceMapper.VerifyAll();
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

        [TestMethod]
        public void CanCreateTriplePatternsUsingUriForPredicate()
        {
            // given
            var predicateUri = new Uri("http://www.example.com/property");

            // when
            _builder.Subject("s").PredicateUri(predicateUri).Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://www.example.com/property"), ((dynamic)pattern.Predicate).Node.Uri);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingQNameForSubject()
        {
            // given
            const string predicateQName = "foaf:name";
            _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            _builder.Subject<IUriNode>(predicateQName).Predicate("p").Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Subject).Node.Uri);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingQNameForPredicate()
        {
            // given
            const string predicateQName = "foaf:name";
            _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            _builder.Subject("s").PredicateUri(predicateQName).Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Predicate).Node.Uri);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingUriForSubject()
        {
            // when
            _builder.Subject(new Uri("http://xmlns.com/foaf/0.1/name")).Predicate("p").Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Subject).Node.Uri);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingQNameForObject()
        {
            // given
            const string predicateQName = "foaf:Person";
            _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            _builder.Subject("s").Predicate("p").Object<IUriNode>(predicateQName);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/Person"), ((dynamic)pattern.Object).Node.Uri);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingBlankNodeForObject()
        {
            // when
            _builder.Subject("s").Predicate("p").Object<IBlankNode>("bnode");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is BlankNodePattern);
            Assert.AreEqual("_:bnode", ((BlankNodePattern)pattern.Object).ID);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingBlankNodeForSubject()
        {
            // when
            _builder.Subject<IBlankNode>("s").Predicate("p").Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            var pattern = _builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is BlankNodePattern);
            Assert.AreEqual("_:s", ((BlankNodePattern)pattern.Subject).ID);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }
    }
}