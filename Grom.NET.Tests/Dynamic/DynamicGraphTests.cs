namespace Dynamic
{
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;
    using VDS.RDF.Nodes;

    public class CustomClass
    {
        public Uri p { get; set; }
    }

    [TestClass]
    public class DynamicGraphTests
    {
        private static readonly Uri exampleBase = new Uri("http://example.com/");
        private static readonly Uri exampleSubjectUri = new Uri(exampleBase, "s");
        private static readonly Uri examplePredicateUri = new Uri(exampleBase, "p");
        private static readonly Uri exampleObjectUri = new Uri(exampleBase, "o");
        private static readonly IUriNode exampleSubject = new NodeFactory().CreateUriNode(exampleSubjectUri);
        private static readonly IUriNode examplePredicate = new NodeFactory().CreateUriNode(examplePredicateUri);
        private static readonly IUriNode exampleObject = new NodeFactory().CreateUriNode(exampleObjectUri);
        private static readonly IGraph spoGraph = GenerateSPOGraph();

        private static IGraph GenerateSPOGraph()
        {
            var spoGraph = new Graph();
            spoGraph.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            return spoGraph;
        }

        [TestMethod]
        public void Indexing_supports_setting_wrapper_indices()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            var s = d.s;
            d[s] = new { p = 0 };

            var expected = 0;
            var actual = g.Triples.Single().Object.AsValuedNode().AsInteger();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cant_work_with_null_index()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();
            var result = d[null];
        }

        [TestMethod]
        public void Node_support_wrapper_indices()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            var s = d.s;
            var o = s.p[0];
            s[o] = "o";

            var value = g.GetTriplesWithObject(g.CreateLiteralNode("o")).SingleOrDefault();

            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void Index_set_null_deletes_by_subject()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();
            d["http://example.com/s"] = null;

            var condition = g.IsEmpty;

            Assert.IsTrue(condition);
        }

        [TestMethod]
        public void Member_set_null_deletes_by_subject()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            d.s = null;

            var condition = g.IsEmpty;

            Assert.IsTrue(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_set_without_readable_public_properties()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            d.s = new { };
        }

        [TestMethod]
        public void Get_index_with_absolute_uri_string()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();

            var expected = exampleSubject;
            var actual = d["http://example.com/s"];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Indexing_supports_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var d = g.AsDynamic();

            var expected = exampleSubject;
            var actual = d["ex:s"];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Indexing_supports_empty_string()
        {
            var g = GenerateSPOGraph();
            g.BaseUri = exampleSubjectUri;
            var d = g.AsDynamic();

            var expected = exampleSubject;
            var actual = d[string.Empty];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Indexing_supports_qnames_with_default_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var d = g.AsDynamic();

            var expected = exampleSubject;
            var actual = d[":s"];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Get_index_with_relative_uri_string()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);

            var expected = exampleSubject;
            var actual = d["s"];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Get_index_supports_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example.com/#"));

            var expected = g.Triples.First().Subject;
            var actual = d["s"];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uris()
        {
            var g = GenerateSPOGraph();

            var d = g.AsDynamic();

            Assert.AreEqual(
                exampleSubject,
                d[exampleSubjectUri]);
        }

        [TestMethod]
        public void Indexing_supports_relative_uris()
        {
            var g = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(g, exampleBase);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph[new Uri("s", UriKind.Relative)]);
        }

        [TestMethod]
        public void Indexing_supports_uri_nodes()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();

            Assert.AreEqual(
                exampleSubject,
                d[exampleSubject]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Relative_indexing_requires_base_uri()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            var result = d["s"];
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Property_access_requires_base_uri()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            var result = d.s;
        }

        [TestMethod]
        [ExpectedException(typeof(RdfException))]
        public void Cant_get_index_with_unknown_qName()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            var result = d["ex:s"];
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Cant_get_index_with_illegal_uri()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            var result = d["http:///"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_absolute_uri_string_index()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            var result = d["http://example.com/nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_relative_uri_string_index()
        {
            var g = new Graph();
            var d = g.AsDynamic(exampleBase);

            var result = d["nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_member()
        {
            var g = new Graph();
            var d = g.AsDynamic(exampleBase);

            var result = d.nonexistent;
        }

        [TestMethod]
        public void Only_subject_nodes_are_exposed()
        {
            var g = GenerateSPOGraph();

            var d = new DynamicGraph(g);
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            CollectionAssert.DoesNotContain(
                memberNames.ToArray(),
                "http://example.com/o");
        }

        [TestMethod]
        public void Only_uri_nodes_are_exposed()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var d = new DynamicGraph(g);
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            Assert.IsFalse(memberNames.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_get_index_is_forbidden()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();

            var result = d[0, 0];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_set_index_is_forbidden()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();

            d[0, 0] = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_get_index_with_unknown_type()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic();

            var result = d[0];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cant_construct_without_graph()
        {
            new DynamicGraph(null);
        }

        [TestMethod]
        public void Get_member()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);

            Assert.AreEqual(
                exampleSubject,
                d.s);
        }

        [TestMethod]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var g = GenerateSPOGraph();
            g.BaseUri = exampleBase;
            var d = g.AsDynamic();

            Assert.AreEqual(
                exampleSubject,
                d.s);
        }

        [TestMethod]
        public void Predicate_base_uri_defaults_to_subject_base_uri()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);

            Assert.AreEqual(
                g.Triples.First().Object.AsDynamic(),
                d.s.p[0]);
        }

        [TestMethod]
        public void Dynamic_member_names_become_relative_to_base()
        {
            var g = GenerateSPOGraph();
            var d = new DynamicGraph(g, exampleBase);
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            Assert.AreEqual(
                "s",
                memberNames.Single());
        }

        [TestMethod]
        public void Dynamic_member_names_without_base_remain_absolute()
        {
            var g = GenerateSPOGraph();
            var d = new DynamicGraph(g);
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            Assert.AreEqual(
                "http://example.com/s",
                memberNames.Single());
        }

        [TestMethod]
        public void Dynamic_member_names_unrelated_to_base_remain_absolute()
        {
            var g = GenerateSPOGraph();
            var d = new DynamicGraph(g, new Uri("http://example2.com/"));
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            Assert.AreEqual(
                "http://example.com/s",
                memberNames.Single());
        }

        [TestMethod]
        public void Dynamic_member_names_support_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");
            var d = new DynamicGraph(g, new Uri("http://example.com/#"));
            var memberNames = d.GetMetaObject(Expression.Empty()).GetDynamicMemberNames();

            Assert.AreEqual(
                "s",
                memberNames.Single());
        }

        [TestMethod]
        public void Indexing_supports_setting_dictionaries()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            d["http://example.com/s"] = new Dictionary<string, Uri> {
                { "http://example.com/p", new Uri("http://example.com/o") }
            };

            Assert.AreEqual(
                spoGraph,
                g);
        }

        [TestMethod]
        public void Indexing_supports_setting_anonymous_classes()
        {
            var g = new Graph();
            var d = g.AsDynamic(exampleBase);

            d["s"] = new { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                g);
        }

        [TestMethod]
        public void Indexing_supports_setting_custom_classes()
        {
            var g = new Graph();
            var d = g.AsDynamic(exampleBase);

            d["s"] = new CustomClass { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                g);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Setter_requires_base_uri()
        {
            var g = new Graph();
            var d = g.AsDynamic();

            d.s = new { p = "o" };
        }

        [TestMethod]
        public void Setter_delegates_to_index_setter()
        {
            var g1 = new Graph();
            var g2 = new Graph();
            var d1 = g1.AsDynamic(exampleBase);
            var d2 = g2.AsDynamic(exampleBase);

            d1.s = new { p = "o" };
            d2["s"] = new { p = "o" };

            Assert.AreEqual(d2, d1);
        }
    }
}
