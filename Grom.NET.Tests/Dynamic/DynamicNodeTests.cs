namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;

    [TestClass]
    public class DynamicNodeTests
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
            spoGraph.Assert(exampleSubject, examplePredicate, exampleObject);

            return spoGraph;
        }

        [TestMethod]
        public void Indexing_supports_wrappers()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = examplePredicate;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_uri_nodes()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = examplePredicate;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = examplePredicateUri;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_relative_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic(exampleBase);
            var i = new Uri("p", UriKind.Relative);
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_hash_base_uris()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            dynamic d = g.Triples.Single().Subject.AsDynamic(new Uri("http://example.com/#"));
            var i = "p";
            var result = d[i];
            var expected = g.Triples.Single().Object.AsDynamic();

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = "http://example.com/p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_relative_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic(exampleBase);
            var i = "p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = "ex:p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = ":p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Member_names_are_predicate_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var memberNames = (d.InnerMetaObjectProvider as DynamicNodeDispatcher).GetDynamicMemberNames();
            var result = memberNames.ToArray();
            var expected = new[] { "http://example.com/p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var memberNames = (d.InnerMetaObjectProvider as DynamicNodeDispatcher).GetDynamicMemberNames();
            var result = memberNames.ToArray();
            var expected = new[] { "ex:p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var memberNames = (d.InnerMetaObjectProvider as DynamicNodeDispatcher).GetDynamicMemberNames();
            var result = memberNames.ToArray();
            var expected = new[] { ":p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_become_relative_to_base()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic(exampleBase);
            var memberNames = (d.InnerMetaObjectProvider as DynamicNodeDispatcher).GetDynamicMemberNames();
            var result = memberNames.ToArray();
            var expected = new[] { "p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_become_relative_to_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            var d = g.Triples.Single().Subject.AsDynamic(new Uri("http://example.com/#"));
            var memberNames = (d.InnerMetaObjectProvider as DynamicNodeDispatcher).GetDynamicMemberNames();
            var result = memberNames.ToArray();
            var expected = new[] { "p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Values_are_arrays()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var i = "http://example.com/p";
            var result = d[i];
            var expected = typeof(Array);

            Assert.IsInstanceOfType(result, expected);
        }

        [TestMethod]
        public void Values_can_collaps_if_single()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic(collapseSingularArrays: true);
            var i = "http://example.com/p";
            var result = d[i];
            var expected = typeof(Array);

            Assert.IsNotInstanceOfType(result, expected);
        }

        [TestMethod]
        public void Values_never_collaps_if_not_single()
        {
            var g = new Graph();
            g.LoadFromString(@"
<http://example.com/s> <http://example.com/p> <http://example.com/o1> .
<http://example.com/s> <http://example.com/p> <http://example.com/o2> .
");
            var s = g.GetTriplesWithSubject(exampleSubject).First().Subject;
            dynamic d = s.AsDynamic(collapseSingularArrays: true);
            var i = "http://example.com/p";
            var result = d[i];
            var expected = typeof(Array);

            Assert.IsInstanceOfType(result, expected);
        }

        [TestMethod]
        public void Property_access_is_translated_to_indexing_with_relative_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic(exampleBase);
            var result = d.p;
            var expected = d["p"];

            CollectionAssert.AreEqual(result, expected);
        }


        [TestMethod]
        public void ToString_delegates_to_graphNode()
        {
            var n = new NodeFactory().CreateBlankNode();

            dynamic d = n.AsDynamic();

            Assert.AreEqual(
                n.ToString(),
                d.ToString());
        }

        [TestMethod]
        public void GetHashCode_delegates_to_node()
        {
            var n = new NodeFactory().CreateBlankNode();

            dynamic d = n.AsDynamic();

            Assert.AreEqual(
                d.GetHashCode(),
                d.GetHashCode());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cant_construct_without_graph_node()
        {
            new DynamicUriNode(null);
        }

        [TestMethod]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var g = GenerateSPOGraph();
            g.BaseUri = exampleBase;
            var s = g.GetTriplesWithSubject(exampleSubject).Single().Subject;
            dynamic d = s.AsDynamic();
            var result = d.p;
            var expected = d["p"];

            CollectionAssert.AreEqual(result, expected);
        }

        [TestMethod]
        public void Setter_delegates_to_index_setter()
        {
            var g1 = GenerateSPOGraph();
            var g2 = GenerateSPOGraph();
            var n1 = g1.Triples.SubjectNodes.First().AsDynamic(exampleBase);
            var n2 = g2.Triples.SubjectNodes.First().AsDynamic(exampleBase);

            var now = DateTimeOffset.Now;

            n2["p"] = now;
            n1.p = now;

            Assert.AreEqual(g2, g1);
        }

    }
}
