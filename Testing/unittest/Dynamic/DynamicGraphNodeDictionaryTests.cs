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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicGraphNodeDictionaryTests
    {
        [Fact]
        public void Get_index_requires_subject()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as INode]);
        }

        [Fact]
        public void Get_index_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));

            Assert.Equal(s, d[s]);
            Assert.IsType<DynamicNode>(d[s]);
        }

        [Fact]
        public void Set_index_requires_subject()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as INode] = null);
        }

        [Fact]
        public void Set_index_with_null_value_retracts_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
# <urn:s> <urn:s> <urn:s> .
# <urn:s> <urn:s> <urn:p> .
# <urn:s> <urn:s> <urn:o> .
# <urn:s> <urn:p> <urn:s> .
# <urn:s> <urn:p> <urn:p> .
# <urn:s> <urn:p> <urn:o> .
# <urn:s> <urn:o> <urn:s> .
# <urn:s> <urn:o> <urn:p> .
# <urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # should retract
<urn:s> <urn:s> <urn:p> . # should retract
<urn:s> <urn:s> <urn:o> . # should retract
<urn:s> <urn:p> <urn:s> . # should retract
<urn:s> <urn:p> <urn:p> . # should retract
<urn:s> <urn:p> <urn:o> . # should retract
<urn:s> <urn:o> <urn:s> . # should retract
<urn:s> <urn:o> <urn:p> . # should retract
<urn:s> <urn:o> <urn:o> . # should retract
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));

            d[s] = null;

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Set_index_overwrites_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> ""s"" .
<urn:s> <urn:s> ""p"" .
<urn:s> <urn:s> ""o"" .
<urn:s> <urn:p> ""s"" .
<urn:s> <urn:p> ""p"" .
<urn:s> <urn:p> ""o"" .
<urn:s> <urn:o> ""s"" .
<urn:s> <urn:o> ""p"" .
<urn:s> <urn:o> ""o"" .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));

            d[s] = new
            {
                s = new[] { "s", "p", "o" },
                p = new[] { "s", "p", "o" },
                o = new[] { "s", "p", "o" },
            };

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Keys_are_uri_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            Assert.Equal(d.Nodes.UriNodes(), ((IDictionary<INode, object>)d).Keys);
        }

        [Fact]
        public void Add_requires_subject()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d.Add(null as INode, null));
        }

        [Fact]
        public void Add_requires_value()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.Throws<ArgumentNullException>(() =>
                d.Add(s, null));
        }

        [Fact]
        public void Add_handles_public_properties()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = d.CreateUriNode(UriFactory.Create("urn:s"));
            var p = d.CreateUriNode(UriFactory.Create("urn:p"));
            var o = d.CreateUriNode(UriFactory.Create("urn:o"));
            var predicateAndObjects = new
            {
                s = new[] { s, p, o },
                p = new[] { s, p, o },
                o = new[] { s, p, o },
            };

            d.Add(s, predicateAndObjects);

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Add_handles_dictionaries()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = d.CreateUriNode(UriFactory.Create("urn:s"));
            var p = d.CreateUriNode(UriFactory.Create("urn:p"));
            var o = d.CreateUriNode(UriFactory.Create("urn:o"));
            var predicateAndObjects = new Dictionary<object, object>
            {
                { "s", new[] { s, p, o } },
                { new Uri("urn:p"), new[] { s, p, o } },
                { d.CreateUriNode(new Uri("urn:o")), new[] { s, p, o } }
            };

            d.Add(s, predicateAndObjects);

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Add_fails_unknown_key_type()
        {
            var g = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = g.CreateBlankNode();
            var predicateAndObjects = new Dictionary<object, object>
            {
                { 0, "o" }
            };

            Assert.Throws<InvalidOperationException>(() =>
                g.Add(s, predicateAndObjects));
        }

        [Fact]
        public void Add_handles_pairs()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = d.CreateUriNode(UriFactory.Create("urn:s"));
            var p = d.CreateUriNode(UriFactory.Create("urn:p"));
            var o = d.CreateUriNode(UriFactory.Create("urn:o"));

            var value = new
            {
                s = new[] { s, p, o },
                p = new[] { s, p, o },
                o = new[] { s, p, o }
            };

            ((IDictionary<INode, object>)d).Add(
                new KeyValuePair<INode, object>(s, value));

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Add_handles_foreign_nodes()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> <urn:o>.
");

            var d = new DynamicGraph(subjectBaseUri: UriFactory.Create("urn:"));
            var s = expected.CreateUriNode(UriFactory.Create("urn:s"));
            var o = expected.CreateUriNode(UriFactory.Create("urn:o"));

            d.Add(s, new { p = o });

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void Contains_rejects_null_subject()
        {
            var d = new DynamicGraph();

            Assert.False(d.Contains(null as INode, null));
        }

        [Fact]
        public void Contains_rejects_null_value()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.False(d.Contains(s, null));
        }

        [Fact]
        public void Contains_rejects_missing_subject()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.False(d.Contains(s, 0));
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));

            Assert.False(d.Contains(s, new { p = s }));
        }

        [Fact]
        public void Contains_searches_exisiting_statements()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));
            var o = d.CreateUriNode(UriFactory.Create("urn:o"));

            Assert.True(d.Contains(s, new { p = o }));
        }

        [Fact]
        public void Contains_fails_unknown_key_type()
        {
            var g = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var predicateAndObjects = new Dictionary<object, object>
            {
                { 0, "o" }
            };

            Assert.Throws<InvalidOperationException>(() =>
                g.Contains(s, predicateAndObjects));
        }

        [Fact]
        public void Contains_handles_pairs()
        {
            var g = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var o = g.CreateUriNode(UriFactory.Create("urn:o"));

            Assert.Contains(new KeyValuePair<INode, object>(s, new { p = o }), g);
        }

        [Fact]
        public void ContainsKey_rejects_null_subject()
        {
            var d = new DynamicGraph();

            Assert.False(d.ContainsKey(null as INode));
        }

        [Fact]
        public void ContainsKey_searches_uri_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = d.CreateUriNode(UriFactory.Create("urn:s"));
            var p = d.CreateUriNode(UriFactory.Create("urn:p"));
            var o = d.CreateUriNode(UriFactory.Create("urn:o"));

            Assert.True(d.ContainsKey(s));
            Assert.False(d.ContainsKey(p));
            Assert.True(d.ContainsKey(o));
        }

        [Fact]
        public void Copies_pairs_with_subject_key_and_dynamic_subject_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1 (subject)
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> . # 2 (subject)
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> . # 3 (subject)
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var o = g.CreateUriNode(UriFactory.Create("urn:o"));

            var array = new KeyValuePair<INode, object>[5];

            void isEmpty(KeyValuePair<INode, object> actual)
            {
                Assert.Equal(default(KeyValuePair<INode, object>), actual);
            }

            Action<KeyValuePair<INode, object>> isKVWith(INode expected)
            {
                return actual =>
                {
                    Assert.Equal(new KeyValuePair<INode, object>(expected, expected), actual);
                    Assert.IsType<DynamicNode>(actual.Value);
                };
            }

            (g as IDictionary<INode, object>).CopyTo(array, 1);

            Assert.Collection(
                array,
                isEmpty,
                isKVWith(s),
                isKVWith(p),
                isKVWith(o),
                isEmpty);
        }

        [Fact]
        public void Enumerates_pairs_with_subject_key_and_dynamic_subject_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1 (subject)
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> . # 2 (subject)
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> . # 3 (subject)
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var o = g.CreateUriNode(UriFactory.Create("urn:o"));

            using (var actual = g.Cast<KeyValuePair<INode, object>>().GetEnumerator())
            {
                using (var expected = new[] { s, p, o }.Cast<INode>().GetEnumerator())
                {
                    while (expected.MoveNext() | actual.MoveNext())
                    {
                        Assert.Equal(new KeyValuePair<INode, object>(expected.Current, expected.Current), actual.Current);
                        Assert.IsType<DynamicNode>(actual.Current.Value);
                    }
                }
            }
        }

        [Fact]
        public void Remove_p_rejects_null_subject()
        {
            var d = new DynamicGraph();

            Assert.False(d.Remove(null as INode));
        }

        [Fact]
        public void Remove_p_retracts_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s2> <urn:s1> ""o5"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new DynamicGraph();
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p1> ""o2"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s1> <urn:p2> ""o4"" .
<urn:s2> <urn:s1> ""o5"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var s1 = actual.Nodes.First();

            actual.Remove(s1);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Remove_p_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();

            Assert.True(d.Remove(s));
            Assert.False(d.Remove(s));
        }

        [Fact]
        public void Remove_po_rejects_null_subject()
        {
            var d = new DynamicGraph();

            Assert.False(d.Remove(null as INode, null));
        }

        [Fact]
        public void Remove_po_rejects_null_predicate_and_object()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.False(d.Remove(s, null));
        }

        [Fact]
        public void Remove_po_rejects_missing_statements()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();
            var p = d.CreateBlankNode();

            Assert.False(d.Remove(s, p));
        }

        [Fact]
        public void Remove_po_handles_public_properties()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s2> <urn:s1> ""o3"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p2> ""o2"" .
<urn:s2> <urn:s1> ""o3"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var s1 = actual.Nodes.First();
            var predicateAndObjects = new
            {
                p1 = "o1",
                p2 = "o2"
            };

            actual.Remove(s1, predicateAndObjects);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Remove_po_handles_dictionaries()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
# <urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
# <urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
# <urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var actual = new DynamicGraph();
            actual.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # should retract
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> . # should retract
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> . # should retract
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = actual.CreateUriNode(UriFactory.Create("urn:s"));
            var p = actual.CreateUriNode(UriFactory.Create("urn:p"));
            var o = actual.CreateUriNode(UriFactory.Create("urn:o"));
            var predicateAndObjects = new Dictionary<object, object>
            {
                { s, s },
                { p.Uri, p },
                { o.Uri.AbsoluteUri, o }
            };

            actual.Remove(s, predicateAndObjects);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Remove_po_handles_pairs()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
# <urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var g = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> .
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> . # should retract
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> .
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> .
<urn:o> <urn:s> <urn:p> .
<urn:o> <urn:s> <urn:o> .
<urn:o> <urn:p> <urn:s> .
<urn:o> <urn:p> <urn:p> .
<urn:o> <urn:p> <urn:o> .
<urn:o> <urn:o> <urn:s> .
<urn:o> <urn:o> <urn:p> .
<urn:o> <urn:o> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var o = g.CreateUriNode(UriFactory.Create("urn:o"));
            var d = (IDictionary<INode, object>)g;

            d.Remove(new KeyValuePair<INode, object>(s, new { p = o }));

            Assert.Equal<IGraph>(expected, g);
        }

        [Fact]
        public void TryGetValue_rejects_null_subject()
        {
            var d = new DynamicGraph();

            Assert.False(d.TryGetValue(null as INode, out var value));
        }

        [Fact]
        public void TryGetValue_rejects_missing_subject()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            var condition = d.TryGetValue(s, out var value);

            Assert.False(condition);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();

            Assert.True(d.TryGetValue(s, out var value));
            Assert.Equal(value, s);
            Assert.NotNull(value);
            Assert.IsType<DynamicNode>(value);
        }
    }
}
