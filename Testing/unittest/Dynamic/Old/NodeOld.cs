namespace VDS.RDF.Dynamic.Old
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class NodeTests
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

        [Fact]
        public void Member_names_are_predicate_uris()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/p";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Member_names_reduce_to_qnames()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace("ex", exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "ex:p";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Member_names_reduce_to_qnames_with_empty_prefix()
        {
            var g = GenerateSPOGraph();
            g.NamespaceMap.AddNamespace(string.Empty, exampleBase);
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = ":p";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Member_names_become_relative_to_base()
        {
            var s = spoGraph.GetTriplesWithSubject(ex_s).Single().Subject;
            var d = s.AsDynamic(exampleBase) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Member_names_become_relative_to_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");
            var s = g.Triples.Single().Subject;
            var d = s.AsDynamic(new Uri("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "p";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Property_access_is_translated_to_indexing_with_relative_uri_strings()
        {
            var g = GenerateSPOGraph();
            var s = g.GetTriplesWithSubject(ex_s).Single().Subject;
            dynamic d = s.AsDynamic(exampleBase);
            var result = (d.p as ICollection<object>).Single();
            var expected = (d["p"] as ICollection<object>).Single();

            Assert.Equal(result, expected);
        }

        [Fact]
        public void ToString_delegates_to_graphNode()
        {
            var n = new NodeFactory().CreateBlankNode();

            dynamic d = n.AsDynamic();

            Assert.Equal(
                n.ToString(),
                d.ToString());
        }

        [Fact]
        public void GetHashCode_delegates_to_node()
        {
            var n = new NodeFactory().CreateBlankNode();

            dynamic d = n.AsDynamic();

            Assert.Equal(
                d.GetHashCode(),
                d.GetHashCode());
        }

        [Fact]
        public void Cant_construct_without_graph_node()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DynamicNode(null);
            });
        }

        [Fact]
        public void Subject_base_uri_defaults_to_graph_base_uri1()
        {
            var d = new DynamicNode(new NodeFactory().CreateBlankNode());

            Assert.Null(d.BaseUri);
        }

        [Fact]
        public void Subject_base_uri_defaults_to_graph_base_uri2()
        {
            var d = new DynamicNode(new Graph() { BaseUri = new Uri("http://example.com/") }.CreateBlankNode());

            Assert.Equal(new Uri("http://example.com/"), d.BaseUri);
        }

        [Fact]
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

            Assert.Equal(g2, g1);
        }

        [Fact]
        public void Setter_requires_base_uri()
        {
            var a = new NodeFactory().CreateBlankNode().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                a.p = null;
            });
        }

        [Fact]
        public void Dynamic_node_is_INode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var result = g.Triples.Single().Subject.AsDynamic();

            Assert.IsAssignableFrom<INode>(result);
        }

        [Fact]
        public void Dynamic_uri_node_is_IUriNode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IUriNode).Uri;
            var expected = (g.Triples.Single().Subject as IUriNode).Uri;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Dynamic_blank_node_is_IBlankNode()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var result = (g.Triples.Single().Subject.AsDynamic() as IBlankNode).InternalID;
            var expected = (g.Triples.Single().Subject as IBlankNode).InternalID;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Dynamic_uri_node_is_Not_IBlankNode()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (g.Triples.Single().Subject.AsDynamic() as IBlankNode).InternalID;
            });
        }

        [Fact]
        public void Dynamic_blank_node_is_not_IUriNode()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (g.Triples.Single().Subject.AsDynamic() as IUriNode).Uri;
            });
        }
    }
}
