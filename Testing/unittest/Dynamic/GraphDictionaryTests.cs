namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class GraphDictionaryTests
    {
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

            var s = d.Triples.SubjectNodes.Single();
            var o = d.Triples.ObjectNodes.Single();

            Assert.True(d.ContainsKey(s));
            Assert.False(d.ContainsKey(o));
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
            var g = new Graph();
            g.LoadFromString(@"
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p1> <urn:o1> .
<urn:s> <urn:p1> <urn:o2> .
<urn:s> <urn:p2> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var s = d.Triples.First().Subject;

            d.Remove(s);

            Assert.Equal(g as IGraph, d as IGraph);
        }

        [Fact]
        public void Remove_reports_retraction_success()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s2> <urn:p3> <urn:o5> .");

            var s = d.Triples.First().Subject;

            Assert.True(d.Remove(s));
            Assert.False(d.Remove(s));
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

            Assert.Throws<ArgumentNullException>(() =>
            {
                d.Add(d.CreateBlankNode(), null);
            });
        }

        [Fact]
        public void Add_cant_add_existing_key()
        {
            var d = new DynamicGraph();
            d.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = d.Triples.SubjectNodes.Single();

            Assert.Throws<ArgumentException>(() =>
            {
                d.Add(s, 0);
            });
        }

        [Fact]
        public void Add_handles_nodes_from_other_graphs()
        {
            var other = new Graph();
            other.LoadFromString("<urn:s> <urn:p> <urn:o>.");

            var d = new DynamicGraph(subjectBaseUri: new Uri("urn:"));

            var s = other.Triples.SubjectNodes.Single();
            var o = other.Triples.ObjectNodes.Single();

            d[s] = new { p = o };

            Assert.Equal(other as IGraph, d as IGraph);
        }

        [Fact]
        public void Keys_are_IRI_subject_nodes()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
_:s <urn:p> <urn:o> .
");

            var dict = d as IDictionary<INode, object>;

            Assert.Equal(d.Triples.SubjectNodes.UriNodes().ToArray(), dict.Keys.ToArray());
        }

        [Fact]
        public void Set_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
            {
                d[null as INode] = null;
            });
        }

        [Fact]
        public void Set_index_null_value_removes_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p1> <urn:o1> .
<urn:s> <urn:p1> <urn:o2> .
<urn:s> <urn:p2> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s2> <urn:s> <urn:o5> .
<urn:s2> <urn:p3> <urn:s> .
");

            var s = d.Triples.First().Subject;

            d[s] = null;

            Assert.Equal(g as IGraph, d as IGraph);
        }

        [Fact]
        public void Set_index_replaces_statements_with_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
");

            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o1> .
<urn:s> <urn:p> <urn:o2> .
<urn:s> <urn:p1> <urn:o3> .
<urn:s> <urn:p2> <urn:o4> .
<urn:s1> <urn:p> <urn:o> .
<urn:s2> <urn:p> <urn:o> .
");

            var s = d.Triples.First().Subject;

            d[s] = new { p = new Uri("urn:o") };

            Assert.Equal(g as IGraph, d as IGraph);
        }

        [Fact]
        public void Contains_rejects_null_keys()
        {
            var d = new DynamicGraph();
            var dict = ((IDictionary<INode, object>)d);
            var o = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(null, o));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_null_values()
        {
            var d = new DynamicGraph();
            var dict = ((IDictionary<INode, object>)d);
            var s = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, null));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_key()
        {
            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            var dict = ((IDictionary<INode, object>)d);
            var s = d.CreateBlankNode();

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o" }));

            Assert.False(condition);
        }

        [Fact]
        public void Contains_rejects_missing_statement()
        {
            var d = new DynamicGraph(predicateBaseUri: new Uri("urn:"));
            d.LoadFromString("<urn:s> <urn:p> \"o\" .");

            var dict = ((IDictionary<INode, object>)d);
            var s = d.Triples.First().Subject;

            var condition = dict.Contains(new KeyValuePair<INode, object>(s, new { p = "o1" }));

            Assert.False(condition);
        }

        [Fact]
        public void Get_index_rejects_missing_key()
        {
            var d = new DynamicGraph();
            var s = d.CreateBlankNode();

            Assert.Throws<KeyNotFoundException>(() =>
                d[s]
            );
        }

        [Fact]
        public void Get_index_requires_key()
        {
            var d = new DynamicGraph();

            Assert.Throws<ArgumentNullException>(() =>
                d[null as INode]
            );
        }

        [Fact]
        public void x()
        {
            var a = new Dictionary<string, string>();
            a.Add("a", "b");
            //a.Remove(null);
            var item = new KeyValuePair<string, string>("a", "b");
            //var b = a.Remove(item);
            //Assert.True(b);
        }
    }
}
