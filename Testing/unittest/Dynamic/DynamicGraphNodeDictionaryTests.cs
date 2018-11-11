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
        public void Get_index_requires_existing_key()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.Throws<KeyNotFoundException>(() =>
                d[s]
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
        public void Keys_are_uri_subject_nodes()
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
            var expected = g.Triples.SubjectNodes.UriNodes();

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
        public void Add_rejects_existing_key()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();

            Assert.Throws<ArgumentException>(() =>
            {
                d.Add(s, 0);
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

            var condition = d.Contains(new KeyValuePair<INode, object>(null, null));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_null_value()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;
            var s = new NodeFactory().CreateBlankNode();

            var condition = d.Contains(new KeyValuePair<INode, object>(s, null));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph() as IDictionary<INode, object>;
            var s = new NodeFactory().CreateBlankNode();

            var condition = d.Contains(new KeyValuePair<INode, object>(s, 0));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = ((IDictionary<INode, object>)d);
            var s = d.Nodes.First();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o1" }));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_searches_exisiting_statements()
        {
            var d = new DynamicGraph(predicateBaseUri: UriFactory.Create("urn:"));
            d.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var dict = ((IDictionary<INode, object>)d);
            var s = d.Nodes.First();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o" }));

            Assert.True(condition);
        }

        [Fact]
        public void ContainsKey_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.ContainsKey(null as INode);
            });
        }

        [Fact]
        public void ContainsKey_searches_subject_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Nodes.First();
            var o = d.Nodes.Last();

            Assert.True(d.ContainsKey(s));
            Assert.False(d.ContainsKey(o));
        }

        [Fact]
        public void CopyTo_creates_pairs_with_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = g.Nodes.First();

            var dict = g as IDictionary<INode, object>;
            var array = new KeyValuePair<INode, object>[g.Triples.Count()];

            dict.CopyTo(array, 0);

            var pair = array.Single();
            var key = pair.Key;
            var value = pair.Value;

            Assert.Equal(key, s);
            Assert.Equal(value, s);
            Assert.IsType<DynamicNode>(value);
        }

        [Fact]
        public void GetEnumerator_creates_pairs_with_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = g.Nodes.First();

            var dict = g as IDictionary<INode, object>;

            using (var enumerator = dict.GetEnumerator())
            {
                enumerator.MoveNext();
                var pair = enumerator.Current;

                var key = pair.Key;
                var value = pair.Value;

                Assert.Equal(key, s);
                Assert.Equal(value, s);
                Assert.IsType<DynamicNode>(value);
            }
        }

        [Fact]
        public void Remove_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Remove(null as INode);
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
            var d = new DynamicGraph() as IDictionary<INode,object>;
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
        public void TryGetValue_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d.TryGetValue(null as INode, out var value)
            );
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

            var condition = d.TryGetValue(s, out var value);

            Assert.True(condition);
            Assert.Equal(value, s);
            Assert.NotNull(value);
            Assert.IsType<DynamicNode>(value);
        }
    }
}
