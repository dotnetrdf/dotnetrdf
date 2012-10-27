using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VDS.RDF.Parsing;
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
            IMatchTriplePattern pattern = (IMatchTriplePattern) _builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Subject).Node.Uri);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingUriForObject()
        {
            // when
            _builder.Subject("s").Predicate("p").Object(new Uri("http://xmlns.com/foaf/0.1/Person"));

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual(new Uri("http://xmlns.com/foaf/0.1/Person"), ((dynamic)pattern.Object).Node.Uri);
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
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
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is BlankNodePattern);
            Assert.AreEqual("_:s", ((BlankNodePattern)pattern.Subject).ID);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingINodeForSubject()
        {
            // given
            var node = new NodeFactory().CreateBlankNode("bnode");

            // when
            _builder.Subject(node).Predicate("p").Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is NodeMatchPattern);
            Assert.AreSame(node, ((NodeMatchPattern)pattern.Subject).Node);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingIUriNodeForPredicate()
        {
            // given
            var node = new NodeFactory().CreateUriNode(new Uri("http://www.example.com/predicate"));

            // when
            _builder.Subject("s").PredicateUri(node).Object("o");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
            Assert.IsTrue(pattern.Predicate is NodeMatchPattern);
            Assert.AreSame(node, ((NodeMatchPattern)pattern.Predicate).Node);
            Assert.IsTrue(pattern.Object is VariablePattern);
            Assert.AreEqual("o", pattern.Object.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingINodeForObject()
        {
            // given
            var node = new NodeFactory().CreateUriNode(new Uri("http://www.example.com/object"));

            // when
            _builder.Subject("s").Predicate("p").Object(node);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreSame(node, ((NodeMatchPattern)pattern.Object).Node);
            Assert.IsTrue(pattern.Predicate is VariablePattern);
            Assert.AreEqual("p", pattern.Predicate.VariableName);
            Assert.IsTrue(pattern.Subject is VariablePattern);
            Assert.AreEqual("s", pattern.Subject.VariableName);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingIntegerLiteralObject()
        {
            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(42);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual("42", ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.IsNull(((dynamic)pattern.Object).Node.Language);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingTypedLiteralObject()
        {
            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(42, new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual("42", ((dynamic)pattern.Object).Node.Value);
            Assert.AreEqual(new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger), ((dynamic)pattern.Object).Node.DataType);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingLiteralObjectWithLanuageTag()
        {
            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(42, "pl-PL");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual("42", ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.AreEqual("pl-PL", ((dynamic)pattern.Object).Node.Language);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingLiteralObjectWithLanuageTag2()
        {
            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(42, "pl-PL");

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual("42", ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.AreEqual("pl-PL", ((dynamic)pattern.Object).Node.Language);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingDateLiteralObject()
        {
            // given
            var dateTime = new DateTime(2012, 10, 13);

            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual(dateTime.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.IsNull(((dynamic)pattern.Object).Node.Language);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingDateTimeLiteralObject()
        {
            // given
            var dateTime = new DateTime(2012, 10, 13, 15, 45, 15);

            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual(dateTime.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.IsNull(((dynamic)pattern.Object).Node.Language);
        }

        [TestMethod]
        public void CanCreateTriplePatternsUsingDateTimeOffsetLiteralObject()
        {
            // given
            var dateTime = new DateTimeOffset(2012, 10, 13, 20, 35, 10, new TimeSpan(0, 1, 30, 0));

            // when
            _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

            // then
            Assert.AreEqual(1, _builder.Patterns.Length);
            IMatchTriplePattern pattern = (IMatchTriplePattern)_builder.Patterns.Single();
            Assert.IsTrue(pattern.Object is NodeMatchPattern);
            Assert.AreEqual("2012-10-13T20:35:10+01:30", ((dynamic)pattern.Object).Node.Value);
            Assert.IsNull(((dynamic)pattern.Object).Node.DataType);
            Assert.IsNull(((dynamic)pattern.Object).Node.Language);
        }
    }
}