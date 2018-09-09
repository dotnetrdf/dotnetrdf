namespace Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void Indexing_supports_setting_wrapper_index()
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
            var d = new Graph().AsDynamic();
            var result = d[null];
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
        public void Set_null_deletes_by_subject()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            d["s"] = null;

            var condition = g.IsEmpty;

            Assert.IsTrue(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_set_index_without_readable_public_properties()
        {
            var d = new Graph().AsDynamic(exampleBase);

            d["s"] = new { };
        }

        [TestMethod]
        public void Get_index_with_absolute_uri_string()
        {
            var d = spoGraph.AsDynamic();

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
            var d = spoGraph.AsDynamic(exampleBase);

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
            var d = spoGraph.AsDynamic();

            Assert.AreEqual(
                exampleSubject,
                d[exampleSubjectUri]);
        }

        [TestMethod]
        public void Indexing_supports_relative_uris()
        {
            var d = GenerateSPOGraph().AsDynamic(exampleBase);

            Assert.AreEqual(
                exampleSubject,
                d[new Uri("s", UriKind.Relative)]);
        }

        [TestMethod]
        public void Indexing_supports_uri_nodes()
        {
            var d = GenerateSPOGraph().AsDynamic();

            Assert.AreEqual(
                exampleSubject,
                d[exampleSubject]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Relative_indexing_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            var result = d["s"];
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Property_access_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            var result = d.s;
        }

        [TestMethod]
        [ExpectedException(typeof(RdfException))]
        public void Cant_get_index_with_unknown_qName()
        {
            var d = new Graph().AsDynamic();

            var result = d["ex:s"];
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Cant_get_index_with_illegal_uri()
        {
            var d = new Graph().AsDynamic();

            var result = d["http:///"];
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Cant_get_nonexistent_absolute_uri_string_index()
        {
            var d = new Graph().AsDynamic();

            var result = d["http://example.com/nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Cant_get_nonexistent_relative_uri_string_index()
        {
            var d = GenerateSPOGraph().AsDynamic(exampleBase);

            var result = d["nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Cant_get_nonexistent_member()
        {
            var d = new Graph().AsDynamic(exampleBase);

            var result = d.nonexistent;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_get_index_is_forbidden()
        {
            var d = new Graph().AsDynamic();

            var result = d[null, null];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_set_index_is_forbidden()
        {
            var d = new Graph().AsDynamic();

            d[null, null] = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_get_index_with_unknown_type()
        {
            var d = new Graph().AsDynamic();

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
            var d = spoGraph.AsDynamic(exampleBase);

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

            var expected = exampleSubject;
            var actual = d.s;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Predicate_base_uri_defaults_to_subject_base_uri()
        {
            var g = GenerateSPOGraph();
            var d = g.AsDynamic(exampleBase);
            var objects = d.s.p as IEnumerable<object>;

            var expected = g.Triples.First().Object;
            var actual = objects.First();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Dynamic_member_names_only_subject_nodes_are_exposed()
        {
            var d = spoGraph.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/o";

            CollectionAssert.DoesNotContain(collection, element);
        }

        [TestMethod]
        public void Dynamic_member_names_only_uri_nodes_are_exposed()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var condition = meta.GetDynamicMemberNames().Any();

            Assert.IsFalse(condition);
        }

        [TestMethod]
        public void Dynamic_member_names_become_relative_to_base()
        {
            var d = spoGraph.AsDynamic(exampleBase) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Dynamic_member_names_without_base_remain_absolute()
        {
            var d = spoGraph.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/s";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Dynamic_member_names_unrelated_to_base_remain_absolute()
        {
            var d = spoGraph.AsDynamic(new Uri("http://example2.com/")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/s";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Dynamic_member_names_support_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(new Uri("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            CollectionAssert.Contains(collection, element);
        }

        // TODO: all kinds of dictionary entries
        [TestMethod]
        public void Indexing_supports_setting_dictionaries()
        {
            var d = new Graph().AsDynamic();

            d["http://example.com/s"] = new Dictionary<string, Uri> {
                { "http://example.com/p", new Uri("http://example.com/o") }
            };

            Assert.AreEqual(
                spoGraph,
                d);
        }

        // TODO: all kinds of properties
        [TestMethod]
        public void Indexing_supports_setting_anonymous_classes()
        {
            var d = new Graph().AsDynamic(exampleBase);

            d["s"] = new { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                d);
        }

        // TODO: all kinds of properties
        [TestMethod]
        public void Indexing_supports_setting_custom_classes()
        {
            var d = new Graph().AsDynamic(exampleBase);

            d["s"] = new CustomClass { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                d);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Setter_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            d.s = new { p = "o" };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Set_index_requires_index()
        {
            var d = new Graph().AsDynamic();

            d[null] = null;
        }

        [TestMethod]
        public void Setter_delegates_to_index_setter()
        {
            var d1 = new Graph().AsDynamic(exampleBase);
            var d2 = new Graph().AsDynamic(exampleBase);

            d1.s = new { p = "o" };
            d2["s"] = new { p = "o" };

            Assert.AreEqual(d2, d1);
        }
    }
}
