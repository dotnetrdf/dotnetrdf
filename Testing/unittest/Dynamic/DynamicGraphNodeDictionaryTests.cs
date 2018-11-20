namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicGraphNodeDictionaryTests
    {
        [Fact]
        public void Get_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as INode]
            );
        }

        [Fact]
        public void Get_index_returns_dynamic_subject()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();

            var expected = s;
            var actual = d[s];

            Assert.Equal(expected, actual);
            Assert.IsType<DynamicNode>(actual);
        }

        [Fact]
        public void Set_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as INode] = null
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

            var s1 = actual.Nodes.First();

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

            var s1 = actual.Nodes.First();

            actual[s1] = new { p1 = "o" };

            Assert.Equal(expected as IGraph, actual as IGraph);
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

            var actual = ((IDictionary<INode, object>)g).Keys;
            var expected = g.Nodes.UriNodes();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Add(null as INode, null);
            });
        }

        [Fact]
        public void Add_requires_value()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Add(s, null);
            });
        }

        [Fact]
        public void Add_handles_public_properties()
        {
            var actual = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            var s = actual.CreateUriNode(UriFactory.Create("urn:s"));

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
            var s = actual.CreateUriNode(UriFactory.Create("urn:s"));

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
            var s = actual.CreateUriNode(UriFactory.Create("urn:s"));

            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            ((IDictionary<INode, object>)actual).Add(
                new KeyValuePair<INode, object>(
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
        public void Add_handles_foreign_nodes()
        {
            var expected = new Graph();
            expected.LoadFromString("<urn:s> <urn:p> <urn:o>.");

            var actual = new DynamicGraph(subjectBaseUri: UriFactory.Create("urn:"));

            var s = expected.Nodes.First();
            var o = expected.Nodes.Last();

            actual.Add(s, new { p = o });

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Contains_rejects_null_key()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;

            Assert.DoesNotContain(new KeyValuePair<INode, object>(null, null), d);
        }

        [Fact]
        public void Contains_rejects_null_value()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;
            var s = new NodeFactory().CreateBlankNode();

            Assert.DoesNotContain(new KeyValuePair<INode, object>(s, null), d);
        }

        [Fact]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;
            var s = new NodeFactory().CreateBlankNode();

            Assert.DoesNotContain(new KeyValuePair<INode, object>(s, 0), d);
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = ((IDictionary<INode, object>)d);
            var s = d.Nodes.First();

            Assert.DoesNotContain(new KeyValuePair<INode, object>(s, new { p = "o1" }), d);
        }

        [Fact]
        public void Contains_searches_exisiting_statements()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
");

            var dict = ((IDictionary<INode, object>)d);
            var s = d.Nodes.First();

            Assert.Contains(new KeyValuePair<INode, object>(s, new { p = "o" }), d);
        }

        [Fact]
        public void ContainsKey_rejects_null_key()
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

            var empty = new KeyValuePair<INode, object>();
            void isEmpty(KeyValuePair<INode, object> actual)
            {
                Assert.Equal(empty, actual);
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
                isEmpty
            );
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
        public void Remove_rejects_null_key()
        {
            var d = new DynamicGraph();

            Assert.False(d.Remove(null as INode));
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

            var s1 = actual.Nodes.First();

            actual.Remove(s1);

            Assert.Equal(expected as IGraph, actual as IGraph);
        }

        [Fact]
        public void Remove_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();

            Assert.True(d.Remove(s));
            Assert.False(d.Remove(s));
        }

        [Fact]
        public void Remove_pair_ignores_missing_statements()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;
            var s = new NodeFactory().CreateBlankNode();

            var condition = d.Remove(new KeyValuePair<INode, object>(s, null));

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

            var s1 = actual.Nodes.First();

            ((IDictionary<INode, object>)actual).Remove(
                new KeyValuePair<INode, object>(
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

            var s1 = actual.Nodes.First();

            ((IDictionary<INode, object>)actual).Remove(
                new KeyValuePair<INode, object>(
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
        public void TryGetValue_rejects_null_key()
        {
            var d = new DynamicGraph();

            Assert.False(d.TryGetValue(null as INode, out var value));
        }

        [Fact]
        public void TryGetValue_rejects_missing_key()
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
