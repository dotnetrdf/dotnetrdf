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

    public class DynamicGraphUriDictionaryTests
    {
        [Fact]
        public void Get_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as Uri]);
        }

        [Fact]
        public void Get_index_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = UriFactory.Create("urn:s");

            var expected = d.CreateUriNode(s);
            var actual = d[s];

            Assert.Equal(expected, actual);
            Assert.IsType<DynamicNode>(actual);
        }

        [Fact]
        public void Set_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as Uri] = null);
        }

        [Fact]
        public void Set_index_with_null_value_retracts_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new DynamicGraph();
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p1> ""o2"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s1> <urn:p2> ""o4"" .
<urn:s1> <urn:p2> ""o5"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var s1 = UriFactory.Create("urn:s1");

            actual[s1] = null;

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Set_index_overwrites_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s1> <urn:p1> ""o"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p1> ""o2"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s1> <urn:p2> ""o4"" .
<urn:s1> <urn:p2> ""o5"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var s1 = UriFactory.Create("urn:s1");

            actual[s1] = new { p1 = "o" };

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Keys_are_uri_nodes()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> .
<urn:s1> <urn:p1> <urn:o2> .
<urn:s1> <urn:p2> <urn:o3> .
<urn:s1> <urn:p2> <urn:o4> .
<urn:s2> <urn:p3> <urn:o5> .
<urn:s2> <urn:p3> <urn:o6> .
<urn:s2> <urn:p4> <urn:o7> .
<urn:s2> <urn:p4> <urn:o8> .
");

            var actual = ((IDictionary<Uri, object>)g).Keys;
            var expected = g.Nodes.UriNodes().Select(n => n.Uri);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d.Add(null as Uri, null));
        }

        [Fact]
        public void Add_requires_value()
        {
            var d = new DynamicGraph();
            var s = UriFactory.Create("urn:s");

            Assert.Throws<ArgumentNullException>(() =>
                d.Add(s, null));
        }

        [Fact]
        public void Add_handles_public_properties()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = UriFactory.Create("urn:s");

            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p1> ""o1"" .
<urn:s> <urn:p2> ""o2"" .
");

            var predicateAndObjects = new
            {
                p1 = "o1",
                p2 = "o2"
            };

            actual.Add(s, predicateAndObjects);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Add_handles_dictionaries()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = UriFactory.Create("urn:s");

            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p1> ""o1"" .
<urn:s> <urn:p2> ""o2"" .
");

            var predicateAndObjects = new Dictionary<object, object>
            {
                { "p1", "o1" },
                { "p2", "o2" }
            };

            actual.Add(s, predicateAndObjects);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Add_handles_key_value_pairs()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = UriFactory.Create("urn:s");

            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var value = new
            {
                p = "o"
            };

            ((IDictionary<Uri, object>)actual).Add(new KeyValuePair<Uri, object>(s, value));

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Contains_rejects_null_key()
        {
            var d = new DynamicGraph() as IDictionary<Uri, object>;

            Assert.False(d.Contains(new KeyValuePair<Uri, object>(null, null)));
        }

        [Fact]
        public void Contains_rejects_null_value()
        {
            var d = new DynamicGraph() as IDictionary<Uri, object>;
            var s = UriFactory.Create("urn:s");

            Assert.False(d.Contains(new KeyValuePair<Uri, object>(s, null)));
        }

        [Fact]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph() as IDictionary<Uri, object>;
            var s = UriFactory.Create("urn:s");

            var condition = d.Contains(new KeyValuePair<Uri, object>(s, 0));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = (IDictionary<Uri, object>)d;
            var s = UriFactory.Create("urn:s");

            var condition = dict.Contains(new KeyValuePair<Uri, object>(s, new { p = "o1" }));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_searches_exisiting_statements()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = (IDictionary<Uri, object>)d;
            var s = UriFactory.Create("urn:s");

            Assert.True(dict.Contains(new KeyValuePair<Uri, object>(s, new { p = "o" })));
        }

        [Fact]
        public void ContainsKey_rejects_null_key()
        {
            var d = new DynamicGraph();

            Assert.False(d.ContainsKey(null as Uri));
        }

        [Fact]
        public void ContainsKey_searches_uri_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = UriFactory.Create("urn:s");
            var p = UriFactory.Create("urn:p");
            var o = UriFactory.Create("urn:o");

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

            var s = UriFactory.Create("urn:s");
            var p = UriFactory.Create("urn:p");
            var o = UriFactory.Create("urn:o");

            var array = new KeyValuePair<Uri, object>[5];

            void isEmpty(KeyValuePair<Uri, object> expected)
            {
                Assert.Equal(default(KeyValuePair<Uri, object>), expected);
            }

            Action<KeyValuePair<Uri, object>> isKVWith(Uri expectedKey)
            {
                return actual =>
                {
                    var expectedNode = g.CreateUriNode(expectedKey);

                    Assert.Equal(new KeyValuePair<Uri, object>(expectedKey, expectedNode), actual);
                    Assert.IsType<DynamicNode>(actual.Value);
                };
            }

            (g as IDictionary<Uri, object>).CopyTo(array, 1);

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

            var s = UriFactory.Create("urn:s");
            var p = UriFactory.Create("urn:p");
            var o = UriFactory.Create("urn:o");

            using (var actual = g.Cast<KeyValuePair<Uri, object>>().GetEnumerator())
            {
                using (var expected = new[] { s, p, o }.Cast<Uri>().GetEnumerator())
                {
                    while (expected.MoveNext() | actual.MoveNext())
                    {
                        var keyNode = g.CreateUriNode(expected.Current);

                        Assert.Equal(new KeyValuePair<Uri, object>(expected.Current, keyNode), actual.Current);
                        Assert.IsType<DynamicNode>(actual.Current.Value);
                    }
                }
            }
        }

        [Fact]
        public void Remove_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d.Remove(null as Uri));
        }

        [Fact]
        public void Remove_retracts_by_subject()
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

            var s1 = UriFactory.Create("urn:s1");

            actual.Remove(s1);

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Remove_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = UriFactory.Create("urn:s");

            Assert.True(d.Remove(s));
            Assert.False(d.Remove(s));
        }

        [Fact]
        public void Remove_pair_requires_key()
        {
            var d = new DynamicGraph() as IDictionary<Uri, object>;

            var condition = d.Remove(new KeyValuePair<Uri, object>(null, null));

            Assert.False(condition);
        }

        [Fact]
        public void Remove_pair_ignores_missing_statements()
        {
            var d = new DynamicGraph() as IDictionary<Uri, object>;
            var s = UriFactory.Create("urn:s");

            var condition = d.Remove(new KeyValuePair<Uri, object>(s, null));

            Assert.False(condition);
        }

        [Fact]
        public void Remove_pair_handles_public_properties()
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

            var s1 = UriFactory.Create("urn:s1");

            var value = new
            {
                p1 = "o1",
                p2 = "o2"
            };

            ((IDictionary<Uri, object>)actual).Remove(new KeyValuePair<Uri, object>(s1, value));

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void Remove_pair_handles_public_dictionaries()
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

            var s1 = UriFactory.Create("urn:s1");

            var value = new Dictionary<object, object>
            {
                { "p1", "o1" },
                { "p2", "o2" }
            };

            ((IDictionary<Uri, object>)actual).Remove(new KeyValuePair<Uri, object>(s1, value));

            Assert.Equal<IGraph>(expected, actual);
        }

        [Fact]
        public void TryGetValue_rejects_null_key()
        {
            var d = new DynamicGraph();

            Assert.False(d.TryGetValue(null as Uri, out var value));
        }

        [Fact]
        public void TryGetValue_rejects_missing_key()
        {
            var d = new DynamicGraph();
            var s = UriFactory.Create("urn:s");

            var condition = d.TryGetValue(s, out var value);

            Assert.False(condition);
        }

        [Fact]
        public void TryGetValue_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = UriFactory.Create("urn:s");

            Assert.True(d.TryGetValue(s, out var value));
            Assert.Equal(value, d.CreateUriNode(s));
            Assert.IsType<DynamicNode>(value);
        }
    }
}
