/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace VDS.RDF.Dynamic.Old
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.CSharp.RuntimeBinder;
    using VDS.RDF.Nodes;
    using Xunit;

    public class DynamicGraphTests
    {
        public void Indexing_supports_setting_wrapper_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(UriFactory.Create("http://example.com/"));
            var s = d.s;
            d[s] = new { p = 0 };

            var expected = 0;
            var actual = g.Triples.Single().Object.AsValuedNode().AsInteger();

            Assert.Equal(expected, actual);
        }

        public void Get_index_with_absolute_uri_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["http://example.com/s"];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_qnames()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.com/"));
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["ex:s"];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_empty_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.BaseUri = UriFactory.Create("http://example.com/s");
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[string.Empty];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_qnames_with_default_prefix()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace(string.Empty, UriFactory.Create("http://example.com/"));
            var d = g.AsDynamic();

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[":s"];

            Assert.Equal(expected, actual);
        }

        public void Get_index_with_relative_uri_string()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(UriFactory.Create("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d["s"];

            Assert.Equal(expected, actual);
        }

        public void Get_index_supports_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(UriFactory.Create("http://example.com/#"));

            var expected = g.Triples.First().Subject;
            var actual = d["s"];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_absolute_uris()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic();

            var expected = g.Triples.First().Subject;
            var actual = d[UriFactory.Create("http://example.com/s")];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_relative_uris()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(UriFactory.Create("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d[new Uri("s", UriKind.Relative)];

            Assert.Equal(expected, actual);
        }

        public void Indexing_supports_uri_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(UriFactory.Create("http://example.com/"));
            var s = g.Triples.SubjectNodes.Single();

            var expected = s;
            var actual = d[s];

            Assert.Equal(expected, actual);
        }

        public void Relative_indexing_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
                d["s"]);
        }

        public void Property_access_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = d.s;
            });
        }

        public void Cant_get_index_with_unknown_qName()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<RdfException>(() =>
                d["ex:s"]);
        }

        public void Cant_get_index_with_illegal_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<FormatException>(() =>
                d["http:///"]);
        }

        public void Cant_get_nonexistent_absolute_uri_string_index()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<KeyNotFoundException>(() =>
                d["http://example.com/nonexistent"]);
        }

        public void Cant_get_nonexistent_relative_uri_string_index()
        {
            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var result = d["nonexistent"];
            });
        }

        public void Cant_get_nonexistent_member()
        {
            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            Assert.Throws<KeyNotFoundException>(() =>
                d.nonexistent);
        }

        public void Indexing_requires_known_index_type()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<RuntimeBinderException>(() =>
                d[null]);
        }

        public void Get_member()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(UriFactory.Create("http://example.com/"));

            var expected = g.Triples.SubjectNodes.Single();
            var actual = d.s;

            Assert.Equal(expected, actual);
        }

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

        public void Dynamic_member_names_only_uri_nodes_are_exposed()
        {
            var g = new Graph();
            g.LoadFromString("_:s <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic() as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var condition = meta.GetDynamicMemberNames().Any();

            Assert.False(condition);
        }

        public void Dynamic_member_names_become_relative_to_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(UriFactory.Create("http://example.com/")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            Assert.Contains(element, collection);
        }

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

        public void Dynamic_member_names_unrelated_to_base_remain_absolute()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            var d = g.AsDynamic(UriFactory.Create("http://example2.com/")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "http://example.com/s";

            Assert.Contains(element, collection);
        }

        public void Dynamic_member_names_support_hash_base()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/p> <http://example.com/o> .");

            var d = g.AsDynamic(UriFactory.Create("http://example.com/#")) as IDynamicMetaObjectProvider;
            var meta = d.GetMetaObject(Expression.Parameter(typeof(object), "debug"));
            var collection = meta.GetDynamicMemberNames().ToArray();
            var element = "s";

            Assert.Contains(element, collection);
        }

        // TODO: all kinds of properties
        public void Indexing_supports_setting_anonymous_classes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            d["s"] = new { p = UriFactory.Create("http://example.com/o") };

            Assert.Equal<IGraph>(g, d);
        }

        // TODO: all kinds of properties
        public void Indexing_supports_setting_custom_classes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            d["s"] = new CustomClass { P = UriFactory.Create("http://example.com/o") };

            Assert.Equal<IGraph>(g, d);
        }

        public void Setter_requires_base_uri()
        {
            var d = new Graph().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
                d.s = new { p = "o" });
        }

        public void Setter_delegates_to_index_setter()
        {
            var d1 = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));
            var d2 = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            d1.s = new { p = "o" };
            d2["s"] = new { p = "o" };

            Assert.Equal<IGraph>(d2, d1);
        }

        public class CustomClass
        {
            public Uri P { get; set; }
        }
    }
}
