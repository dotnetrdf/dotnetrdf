namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicGraphStringDictionaryTests
    {
        [Fact]
        public void Get_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as string]
            );
        }

        [Fact]
        public void Get_index_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            var expected = d.CreateUriNode(UriFactory.Create(s));
            var actual = d[s];

            Assert.Equal(expected, actual);
            Assert.IsType<DynamicNode>(actual);
        }

        [Fact]
        public void Set_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as string] = null
            );
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

            var s1 = "urn:s1";

            actual[s1] = null;

            Assert.Equal(expected as IGraph, actual as IGraph);
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

            var s1 = "urn:s1";

            actual[s1] = new { p1 = "o" };

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Keys_are_uri_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> .
<urn:s1> <urn:p1> <urn:o2> .
<urn:s1> <urn:p2> <urn:o3> .
<urn:s1> <urn:p2> <urn:o4> .
<urn:s2> <urn:p3> <urn:o5> .
<urn:s2> <urn:p3> <urn:o6> .
<urn:s2> <urn:p4> <urn:o7> .
<urn:s2> <urn:p4> <urn:o8> .
");

            var actual = d.Keys;
            var expected = d.Nodes.UriNodes().Select(n => n.Uri.AbsoluteUri);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Add(null as string, null);
            });
        }

        [Fact]
        public void Add_requires_value()
        {
            var d = new DynamicGraph();
            var s = "urn:s";

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Add(s, null);
            });
        }

        [Fact]
        public void Add_rejects_existing_key()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            Assert.Throws<ArgumentException>(() =>
            {
                d.Add(s, 0);
            });
        }

        [Fact]
        public void Add_handles_public_properties()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = "urn:s";

            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p1> ""o1"" .
<urn:s> <urn:p2> ""o2"" .
");

            actual.Add(
                s,
                new
                {
                    p1 = "o1",
                    p2 = "o2"
                }
            );

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Add_handles_dictionaries()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = "urn:s";

            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p1> ""o1"" .
<urn:s> <urn:p2> ""o2"" .
");

            actual.Add(
                s,
                new Dictionary<object, object> {
                    { "p1" , "o1" },
                    { "p2" , "o2" }
            });

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Add_handles_key_value_pairs()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = "urn:s";

            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            ((IDictionary<string, object>)actual).Add(
                new KeyValuePair<string, object>(
                    s,
                    new
                    {
                        p = "o"
                    }
                )
            );

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Contains_rejects_null_key()
        {
            var d = new DynamicGraph() as IDictionary<string, object>;

            var condition = d.Contains(new KeyValuePair<string, object>(null, null));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_null_value()
        {
            var d = new DynamicGraph() as IDictionary<string, object>;
            var s = "urn:s";

            var condition = d.Contains(new KeyValuePair<string, object>(s, null));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph() as IDictionary<string, object>;
            var s = "urn:s";

            var condition = d.Contains(new KeyValuePair<string, object>(s, 0));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = ((IDictionary<string, object>)d);
            var s = "urn:s";

            var condition = dict.Contains(new KeyValuePair<string, object>(s, new { p = "o1" }));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_searches_exisiting_statements()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = ((IDictionary<string, object>)d);
            var s = "urn:s";

            var condition = dict.Contains(new KeyValuePair<string, object>(s, new { p = "o" }));

            Assert.True(condition);
        }

        [Fact]
        public void ContainsKey_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.ContainsKey(null as string);
            });
        }

        [Fact]
        public void ContainsKey_searches_uri_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";
            var p = "urn:p";
            var o = "urn:o";

            Assert.True(d.ContainsKey(s));
            Assert.False(d.ContainsKey(p));
            Assert.True(d.ContainsKey(o));
        }

        [Fact]
        public void CopyTo_creates_pairs_with_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            var dict = g as IDictionary<string, object>;
            var array = new KeyValuePair<string, object>[2];

            dict.CopyTo(array, 0);

            var pair = array.First();
            var key = pair.Key;
            var value = pair.Value;

            Assert.Equal(key, s);
            Assert.Equal(value, g.CreateUriNode(UriFactory.Create(s)));
            Assert.IsType<DynamicNode>(value);
        }

        [Fact]
        public void GetEnumerator_creates_pairs_with_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            var dict = g as IDictionary<string, object>;

            using (var enumerator = dict.GetEnumerator())
            {
                enumerator.MoveNext();
                var pair = enumerator.Current;

                var key = pair.Key;
                var value = pair.Value;

                Assert.Equal(key, s);
                Assert.Equal(value, g.CreateUriNode(UriFactory.Create(s)));
                Assert.IsType<DynamicNode>(value);
            }
        }

        [Fact]
        public void Remove_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Remove(null as string);
            });
        }

        [Fact]
        public void Remove_retracts_statements_with_subject()
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

            var s1 = "urn:s1";

            actual.Remove(s1);

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Remove_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            Assert.True(d.Remove(s));
            Assert.False(d.Remove(s));
        }

        [Fact]
        public void Remove_pair_requires_key()
        {
            var d = new DynamicGraph() as IDictionary<string, object>;

            var condition = d.Remove(new KeyValuePair<string, object>(null, null));

            Assert.False(condition);
        }

        [Fact]
        public void Remove_pair_ignores_missing_statements()
        {
            var d = new DynamicGraph() as IDictionary<string, object>;
            var s = "urn:s";

            var condition = d.Remove(new KeyValuePair<string, object>(s, null));

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

            var s1 = "urn:s1";

            ((IDictionary<string, object>)actual).Remove(
                new KeyValuePair<string, object>(
                    s1,
                    new
                    {
                        p1 = "o1",
                        p2 = "o2"
                    }
                )
            );

            Assert.Equal(expected as IGraph, actual as IGraph);
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

            var s1 = "urn:s1";

            ((IDictionary<string, object>)actual).Remove(
                new KeyValuePair<string, object>(
                    s1,
                    new Dictionary<object, object> {
                        { "p1" , "o1" },
                        { "p2" , "o2" }
                    }
                )
            );


            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void TryGetValue_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d.TryGetValue(null as string, out var value)
            );
        }

        [Fact]
        public void TryGetValue_rejects_missing_key()
        {
            var d = new DynamicGraph();
            var s = "urn:s";

            var condition = d.TryGetValue(s, out var value);

            Assert.False(condition);
        }

        [Fact]
        public void TryGetValue_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = "urn:s";

            var condition = d.TryGetValue(s, out var value);

            Assert.True(condition);
            Assert.Equal(value, d.CreateUriNode(UriFactory.Create(s)));
            Assert.IsType<DynamicNode>(value);
        }
    }
}
