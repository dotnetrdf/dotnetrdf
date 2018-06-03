namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF;

    [TestClass]
    public partial class DynamicNodeTests
    {
        private static readonly Uri exampleBase = new Uri("http://example.com/");
        private static readonly Uri exampleSubjectUri = new Uri(exampleBase, "s");
        private static readonly Uri ex_p_u = new Uri(exampleBase, "p");
        private static readonly Uri exampleObjectUri = new Uri(exampleBase, "o");
        private static readonly IUriNode ex_s = new NodeFactory().CreateUriNode(exampleSubjectUri);
        private static readonly IUriNode ex_p = new NodeFactory().CreateUriNode(ex_p_u);
        private static readonly IUriNode example_o = new NodeFactory().CreateUriNode(exampleObjectUri);
        private static readonly IGraph spoGraph = GenerateSPOGraph();

        private static IGraph GenerateSPOGraph()
        {
            var spoGraph = new Graph();
            spoGraph.Assert(ex_s, ex_p, example_o);

            return spoGraph;
        }
        [TestMethod]
        public void Member_names_are_predicate_uris()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/p";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "ex:p";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Member_names_reduce_to_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = ":p";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Member_names_become_relative_to_base()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic(exampleBase) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Member_names_become_relative_to_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            var s = g.Triples.Single().Subject;
            var d = s.AsDynamic(new Uri("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            CollectionAssert.Contains(collection, element);
        }

        [TestMethod]
        public void Property_access_is_translated_to_indexing_with_relative_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
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
            new DynamicNode(null);
        }

        [TestMethod]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var g = GenerateSPOGraph();
            g.BaseUri = exampleBase;
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
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

            // TODO: Add support for value types
            //var now = DateTimeOffset.Now;

            n2["p"] = "x"; // now;
            n1.p = "x"; // now;

            Assert.AreEqual(g2, g1);
        }


        [TestMethod]
        public void ObjectCollectionTests()
        {
            var g1 = new Graph();
            g1.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""1"" .
<http://example.com/s> <http://example.com/p> ""2"" .
");
            var d = g1.AsDynamic(exampleBase);
            var a = d.s.p as ICollection<object>;

            CollectionAssert.AreEquivalent(new[] { "1", "2" }.ToList(), a as ICollection);

            var g2 = new Graph();
            g2.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""0"" .
<http://example.com/s> <http://example.com/p> ""2"" .
");

            a.Remove("1");
            a.Add("0");
            Assert.AreEqual(g2, g1);

            var g3 = new Graph();
            g3.LoadFromString(@"
<http://example.com/s> <http://example.com/p> ""2"" .
");

            a.Remove("0");
            Assert.AreEqual(g3, g1);

            var g4 = new Graph();
            g4.LoadFromString(@"
            <http://example.com/s> <http://example.com/p> <http://example.com/o> .
            ");

            a.Remove("2");
            a.Add(new Uri("http://example.com/o"));
            Assert.AreEqual(g4, g1);

            a.Clear();
            Assert.IsTrue(g1.IsEmpty);

            a.Add("2");
            Assert.AreEqual(g3, g1);

            a.Add("0");
            Assert.AreEqual(g2, g1);
        }

        [TestMethod]
        public void Dynamic_node_is_INode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var result = g.Triples.Single().Subject.AsDynamic();

            Assert.IsInstanceOfType(result, typeof(INode));
        }

        [TestMethod]
        public void Dynamic_uri_node_is_IUriNode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IUriNode).Uri;
            var expected = (g.Triples.Single().Subject as IUriNode).Uri;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Dynamic_blank_node_is_IBlankNode()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IBlankNode).InternalID;
            var expected = (g.Triples.Single().Subject as IBlankNode).InternalID;

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Dynamic_uri_node_is_Not_IBlankNode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IBlankNode).InternalID;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Dynamic_blank_node_is_not_IUriNode()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IUriNode).Uri;
        }

        [TestMethod]
        public void Values_are_collections_by_default()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var result = dynamic_s["http://example.com/p"];

            Assert.IsInstanceOfType(result, typeof(ICollection));
        }

        [TestMethod]
        public void Values_can_collapse_predicates_with_single_objects()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic(collapseSingularArrays: true);

            var result = dynamic_s["http://example.com/p"];

            Assert.IsInstanceOfType(result, typeof(DynamicNode));
        }

        [TestMethod]
        public void Values_never_collapse_predicates_with_multiple_objects()
        {
            var g = new Graph();
            g.LoadFromString(@"
<http://example.com/s> <http://example.com/p> <http://example.com/o1> .
<http://example.com/s> <http://example.com/p> <http://example.com/o2> .
");

            var dynamic_s = g.Triples.First().Subject.AsDynamic(collapseSingularArrays: true);

            var result = dynamic_s["http://example.com/p"];

            Assert.IsInstanceOfType(result, typeof(ICollection));
        }

    }
}
