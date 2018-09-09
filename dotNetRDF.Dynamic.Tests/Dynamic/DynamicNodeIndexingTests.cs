namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VDS.RDF;

    [TestClass]
    public class DynamicNodeIndexingTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexing_requires_base_uri_for_relative_uri()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s["p"];
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexing_requires_graph()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s["http://example.com/p"];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Indexing_requires_index()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s[null];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Indexing_requires_known_index_type()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s[0];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Indexing_requires_one_index()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s[null, null];
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Indexing_requires_valid_uri()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            var result = dynamic_s["http:///"];
        }

        [TestMethod]
        public void Indexing_supports_absolute_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_p = new Uri("http://example.com/p");
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uri_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["http://example.com/p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_dynamic_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var dynamic_p = g.Triples.Single().Predicate.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[dynamic_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_empty_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = new Uri("http://example.com/p");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[string.Empty] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_hash_base_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");

            var baseUri = new Uri("http://example.com/#");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_node_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_p = g.Triples.Single().Predicate;
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_qname_index()
        {
            var baseUri = new Uri("http://example.com/");

            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace("ex", baseUri);

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["ex:p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_qname_index_with_empty_prefix()
        {
            var baseUri = new Uri("http://example.com/");

            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace(string.Empty, baseUri);

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[":p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_relative_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = new Uri("http://example.com/");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_p = new Uri("p", UriKind.Relative);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        public void Indexing_supports_relative_uri_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = new Uri("http://example.com/");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.AreEqual(example_o, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Indexing_setter_requires_index()
        {
            var dynamic_s = new NodeFactory().CreateBlankNode().AsDynamic();

            dynamic_s[null] = null;
        }
    }
}
