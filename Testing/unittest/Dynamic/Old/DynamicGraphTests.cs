namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.CSharp.RuntimeBinder;
    using VDS.RDF.Nodes;
    using Xunit;

    public class CustomClass
    {
        public Uri p { get; set; }
    }

    public class DynamicGraphTests
    {
        [Fact]
        public void Indexing_supports_setting_wrapper_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example.com/"));
            var s = d.s;
            d[s] = new { p = 0 };

            var expected = 0;
            var actual = g.Triples.Single().Object.AsValuedNode().AsInteger();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Get_index_with_absolute_uri_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["http://example.com/s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_qnames()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.com/"));
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["ex:s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_empty_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.BaseUri = new Uri("http://example.com/s");
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[string.Empty];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_qnames_with_default_prefix()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace(string.Empty, new Uri("http://example.com/"));
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[":s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Get_index_with_relative_uri_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Get_index_supports_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example.com/#"));

            var expected = g.Triples.First().Subject;
            var actual = d["s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_absolute_uris()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic();

            var expected = g.Triples.First().Subject;
            var actual = d[new Uri("http://example.com/s")];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_relative_uris()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(new Uri("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[new Uri("s", UriKind.Relative)];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Indexing_supports_uri_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(new Uri("http://example.com/"));
            var s = g.Triples.SubjectNodes.Single();

            var expected = s;
            var actual = d[s];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Relative_indexing_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = d["s"];
            });
        }

        [Fact]
        public void Property_access_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = d.s;
            });
        }

        [Fact]
        public void Cant_get_index_with_unknown_qName()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<RdfException>(() =>
            {
                var result = d["ex:s"];
            });
        }

        [Fact]
        public void Cant_get_index_with_illegal_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<FormatException>(() =>
            {
                var result = d["http:///"];
            });
        }

        [Fact]
        public void Cant_get_nonexistent_absolute_uri_string_index()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var result = d["http://example.com/nonexistent"];
            });
        }

        [Fact]
        public void Cant_get_nonexistent_relative_uri_string_index()
        {
            var d = new Graph().AsDynamic(new Uri("http://example.com/"));

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var result = d["nonexistent"];
            });
        }

        [Fact]
        public void Cant_get_nonexistent_member()
        {
            var d = new Graph().AsDynamic(new Uri("http://example.com/"));

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var result = d.nonexistent;
            });
        }

        [Fact]
        public void Indexing_requires_known_index_type()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<RuntimeBinderException>(() =>
            {
                var result = d[null];
            });
        }

        [Fact]
        public void Get_member()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(new Uri("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d.s;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Dynamic_member_names_only_subject_nodes_are_exposed()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/o";
            Assert.DoesNotContain(element, collection);
        }

        [Fact]
        public void Dynamic_member_names_only_uri_nodes_are_exposed()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var condition = meta.GetDynamicMemberNames().Any();

            Assert.False(condition);
        }

        [Fact]
        public void Dynamic_member_names_become_relative_to_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example.com/")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Dynamic_member_names_without_base_remain_absolute()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/s";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Dynamic_member_names_unrelated_to_base_remain_absolute()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(new Uri("http://example2.com/")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/s";

            Assert.Contains(element, collection);
        }

        [Fact]
        public void Dynamic_member_names_support_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(new Uri("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            Assert.Contains(element, collection);
        }

        // TODO: all kinds of dictionary entries
        [Fact]
        public void Indexing_supports_setting_dictionaries()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = new Graph().AsDynamic();

            d["http://example.com/s"] = new Dictionary<string, Uri> {
                { "http://example.com/p", new Uri("http://example.com/o") }
            };

            Assert.Equal(g as IGraph, d as IGraph);
        }

        // TODO: all kinds of properties
        [Fact]
        public void Indexing_supports_setting_anonymous_classes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = new Graph().AsDynamic(new Uri("http://example.com/"));

            d["s"] = new { p = new Uri("http://example.com/o") };

            Assert.Equal(g as IGraph, d as IGraph);
        }

        // TODO: all kinds of properties
        [Fact]
        public void Indexing_supports_setting_custom_classes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = new Graph().AsDynamic(new Uri("http://example.com/"));

            d["s"] = new CustomClass { p = new Uri("http://example.com/o") };

            Assert.Equal(g as IGraph, d as IGraph);
        }

        [Fact]
        public void Setter_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                d.s = new { p = "o" };
            });
        }

        [Fact]
        public void Setter_delegates_to_index_setter()
        {
            var d1 = new Graph().AsDynamic(new Uri("http://example.com/"));
            var d2 = new Graph().AsDynamic(new Uri("http://example.com/"));

            d1.s = new { p = "o" };
            d2["s"] = new { p = "o" };
            
            Assert.Equal(d2 as Graph, d1 as Graph);
        }
    }
}
