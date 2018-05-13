namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using VDS.RDF;

    [TestClass]
    public class DynamicNodeTests
    {
        private static readonly Uri exampleBase = new Uri("http://example.com/");
        private static readonly Uri exampleSubjectUri = new Uri(exampleBase, "s");
        private static readonly Uri examplePredicateUri = new Uri(exampleBase, "p");
        private static readonly Uri exampleObjectUri = new Uri(exampleBase, "o");
        private static readonly IUriNode exampleSubjectNode = new NodeFactory().CreateUriNode(exampleSubjectUri);
        private static readonly IUriNode examplePredicateNode = new NodeFactory().CreateUriNode(examplePredicateUri);
        private static readonly IUriNode exampleObjectNode = new NodeFactory().CreateUriNode(exampleObjectUri);
        private static readonly DynamicNode exampleSubject = new DynamicNode(exampleSubjectNode);
        private static readonly DynamicNode examplePredicate = new DynamicNode(examplePredicateNode);
        private static readonly DynamicNode exampleObject = new DynamicNode(exampleObjectNode);
        private static readonly IGraph spoGraph = GenerateSPOGraph();

        private static IGraph GenerateSPOGraph()
        {
            var spoGraph = new Graph();
            spoGraph.Assert(exampleSubjectNode, examplePredicateNode, exampleObjectNode);

            return spoGraph;
        }

        [TestMethod]
        public void Indexing_supports_wrappers()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = examplePredicate;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_uri_nodes()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = examplePredicateNode;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = examplePredicateUri;
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_relative_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s, exampleBase);
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
            dynamic d = new DynamicNode(g.Triples.Single().Subject, new Uri("http://example.com/#"));
            var i = "p";
            var result = d[i];
            var expected = new DynamicNode(g.Triples.Single().Object);

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_absolute_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = "http://example.com/p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Indexing_supports_relative_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s, exampleBase);
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
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
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
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = ":p";
            var result = d[i];
            var expected = exampleObject;

            CollectionAssert.Contains(result, expected);
        }

        [TestMethod]
        public void Member_names_are_predicate_uris()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            var d = new DynamicNode(s);
            var result = d.GetDynamicMemberNames().ToArray();
            var expected = new[] { "http://example.com/p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            var d = new DynamicNode(s);
            var result = d.GetDynamicMemberNames().ToArray();
            var expected = new[] { "ex:p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            var d = new DynamicNode(s);
            var result = d.GetDynamicMemberNames().ToArray();
            var expected = new[] { ":p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_become_relative_to_base()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            var d = new DynamicNode(s, exampleBase);
            var result = d.GetDynamicMemberNames().ToArray();
            var expected = new[] { "p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Member_names_become_relative_to_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            var d = new DynamicNode(g.Triples.Single().Subject, new Uri("http://example.com/#"));
            var result = d.GetDynamicMemberNames().ToArray();
            var expected = new[] { "p" };

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Values_are_arrays()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s);
            var i = "http://example.com/p";
            var result = d[i];
            var expected = typeof(Array);

            Assert.IsInstanceOfType(result, expected);
        }

        [TestMethod]
        public void Values_can_collaps_if_single()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(exampleSubjectNode).Single().Subject;
            dynamic d = new DynamicNode(s, collapseSingularArrays: true);
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
            var s = g.GetTriplesWithSubject(exampleSubjectNode).First().Subject;
            dynamic d = new DynamicNode(s, collapseSingularArrays: true);
            var i = "http://example.com/p";
            var result = d[i];
            var expected = typeof(Array);

            Assert.IsInstanceOfType(result, expected);
        }
    }
}
