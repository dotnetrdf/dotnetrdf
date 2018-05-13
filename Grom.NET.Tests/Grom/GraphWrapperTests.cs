namespace Dynamic
{
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Writing;

    public class CustomClass
    {
        public Uri p { get; set; }
    }

    [TestClass]
    public class DynamicGraphTests
    {
        private static readonly Uri exampleBase = new Uri("http://example.com/");
        private static readonly Uri exampleSubjectUri = new Uri(exampleBase, "s");
        private static readonly IUriNode exampleSubjectNode = new NodeFactory().CreateUriNode(exampleSubjectUri);
        private static readonly DynamicNode exampleSubject = new DynamicNode(exampleSubjectNode);
        private static readonly IGraph spoGraph = GenerateSPOGraph();

        private static IGraph GenerateSPOGraph()
        {
            var spoGraph = new Graph();
            spoGraph.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            return spoGraph;
        }

        [TestMethod]
        public void Graph_support_wrapper_indices()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamic s = dynamicGraph.s;

            dynamicGraph[s] = new { p = 0 };

            Assert.AreEqual(
                0,
                graph.Triples.Single().Object.AsValuedNode().AsInteger());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cant_work_with_null_index()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            var actual = dynamicGraph[null];
        }

        [TestMethod]
        public void Node_support_wrapper_indices()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamic s = dynamicGraph.s;
            dynamic o = s.p[0];

            s[o] = "o";

            Assert.IsNotNull(
               graph.GetTriplesWithObject(graph.CreateLiteralNode("o")).SingleOrDefault());
        }

        [TestMethod]
        public void Indes_set_null_deletes_by_subject()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            dynamicGraph["http://example.com/s"] = null;

            Assert.IsTrue(graph.IsEmpty);
        }

        [TestMethod]
        public void Member_set_null_deletes_by_subject()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamicGraph.s = null;

            Assert.IsTrue(graph.IsEmpty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_set_without_readable_public_properties()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamicGraph.s = new { };
        }

        [TestMethod]
        public void Get_index_with_absolute_uri_string()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph["http://example.com/s"]);
        }

        [TestMethod]
        public void Indexing_supports_qnames()
        {
            var graph = GenerateSPOGraph();
            graph.NamespaceMap.AddNamespace("ex", exampleBase);

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph["ex:s"]);
        }

        [TestMethod]
        public void Get_index_with_qName_default_prefix()
        {
            var graph = GenerateSPOGraph();
            graph.NamespaceMap.AddNamespace(string.Empty, exampleBase);

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph[":s"]);
        }

        [TestMethod]
        public void Get_index_with_relative_uri_string()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph["s"]);
        }

        [TestMethod]
        public void Get_index_supports_hash_base()
        {
            var graph = new Graph();
            graph.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");

            dynamic dynamicGraph = new DynamicGraph(graph, new Uri("http://example.com/#"));

            var expected = new DynamicNode(graph.Triples.First().Subject);
            var actual = dynamicGraph["s"];

            Assert.AreEqual(
                new DynamicNode(graph.Triples.First().Subject),
                dynamicGraph["s"]);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uris()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph[exampleSubjectUri]);
        }

        [TestMethod]
        public void Indexing_supports_relative_uris()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph[new Uri("s", UriKind.Relative)]);
        }

        [TestMethod]
        public void Indexing_supports_uri_nodes()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph[exampleSubjectNode]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Relative_indexing_requires_base_uri()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph["s"];
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Property_access_requires_base_uri()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph.s;
        }

        [TestMethod]
        [ExpectedException(typeof(RdfException))]
        public void Cant_get_index_with_unknown_qName()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph["ex:s"];
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Cant_get_index_with_illegal_uri()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph["http:///"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_sbaolute_uri_string_index()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph["http://example.com/nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_relative_uri_string_index()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            var actual = dynamicGraph["nonexistent"];
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void Cant_get_nonexistent_member()
        {
            var graph = new Graph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            var actual = dynamicGraph.nonexistent;
        }

        [TestMethod]
        public void Only_subject_nodes_are_exposed()
        {
            var graph = GenerateSPOGraph();

            var dynamicGraph = new DynamicGraph(graph);

            CollectionAssert.DoesNotContain(
                dynamicGraph.GetDynamicMemberNames().ToArray(),
                "http://example.com/o");
        }

        [TestMethod]
        public void Only_uri_nodes_are_exposed()
        {
            var graph = new Graph();
            graph.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var dynamicGraph = new DynamicGraph(graph);

            Assert.IsFalse(dynamicGraph.GetDynamicMemberNames().Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_get_index_is_forbidden()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph[0, 0];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Multidimensional_set_index_is_forbidden()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            dynamicGraph[0, 0] = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_get_index_with_unknown_type()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph);

            var actual = dynamicGraph[0];
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
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph.s);
        }

        [TestMethod]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var graph = GenerateSPOGraph();
            graph.BaseUri = exampleBase;

            dynamic dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                exampleSubject,
                dynamicGraph.s);
        }

        [TestMethod]
        public void Predicate_base_uri_defaults_to_subject_base_uri()
        {
            var graph = GenerateSPOGraph();

            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            Assert.AreEqual(
                new DynamicNode(graph.Triples.First().Object),
                dynamicGraph.s.p[0]);
        }

        [TestMethod]
        public void Dynamic_member_names_become_relative_to_base()
        {
            var graph = GenerateSPOGraph();

            var dynamicGraph = new DynamicGraph(graph, exampleBase);

            Assert.AreEqual(
                "s",
                dynamicGraph.GetDynamicMemberNames().Single());
        }

        [TestMethod]
        public void Dynamic_member_names_without_base_remain_absolute()
        {
            var graph = GenerateSPOGraph();

            var dynamicGraph = new DynamicGraph(graph);

            Assert.AreEqual(
                "http://example.com/s",
                dynamicGraph.GetDynamicMemberNames().Single());
        }

        [TestMethod]
        public void Dynamic_member_names_unrelated_to_base_remain_absolute()
        {
            var graph = GenerateSPOGraph();

            var dynamicGraph = new DynamicGraph(graph, new Uri("http://example2.com/"));

            Assert.AreEqual(
                "http://example.com/s",
                dynamicGraph.GetDynamicMemberNames().Single());
        }

        [TestMethod]
        public void Dynamic_member_names_support_hash_base()
        {
            var graph = new Graph();
            graph.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");

            var dynamicGraph = new DynamicGraph(graph, new Uri("http://example.com/#"));

            Assert.AreEqual(
                "s",
                dynamicGraph.GetDynamicMemberNames().Single());
        }

        [TestMethod]
        public void Indexing_supports_setting_dictionaries()
        {
            var graph = new Graph();
            dynamic dynamicGraph = new DynamicGraph(graph);

            dynamicGraph["http://example.com/s"] = new Dictionary<string, Uri> {
                { "http://example.com/p", new Uri("http://example.com/o") }
            };

            Assert.AreEqual(
                spoGraph,
                graph);
        }

        [TestMethod]
        public void Indexing_supports_setting_anonymous_classes()
        {
            var graph = new Graph();
            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamicGraph["s"] = new { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                graph);
        }

        [TestMethod]
        public void Indexing_supports_setting_custom_classes()
        {
            var graph = new Graph();
            dynamic dynamicGraph = new DynamicGraph(graph, exampleBase);

            dynamicGraph["s"] = new CustomClass { p = new Uri("http://example.com/o") };

            Assert.AreEqual(
                spoGraph,
                graph);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Setter_requires_base_uri()
        {
            var graph = new Graph();
            dynamic dynamicGraph = new DynamicGraph(graph);

            dynamicGraph.s = new { p = "o" };
        }

        [TestMethod]
        public void Setter_delegates_to_index_setter()
        {
            var graph1 = new Graph();
            dynamic dynamicGraph = new DynamicGraph(graph1, exampleBase);

            dynamicGraph.s = new { p = "o" };

            var graph2 = new Graph();
            dynamic dynamicGraph2 = new DynamicGraph(graph2, exampleBase);

            dynamicGraph2["s"] = new { p = "o" };

            Assert.AreEqual(graph2, graph1);
        }
    }
}
