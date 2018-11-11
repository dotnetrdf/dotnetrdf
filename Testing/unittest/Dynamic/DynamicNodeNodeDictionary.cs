namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using Xunit;

    public class DynamicNodeNodeDictionary
    {
        [Fact]
        public void Get_index_requires_key()
        {
            var g = new Graph();
            var n = g.CreateBlankNode();
            var s = new DynamicNode(n);

            Assert.Throws<ArgumentNullException>(() =>
                s[null as INode]
            );
        }

        [Fact]
        public void Get_index_requires_existing_key()
        {
            var g = new Graph();
            var n = g.CreateBlankNode();
            var s = new DynamicNode(n);
            var p = g.CreateBlankNode();

            Assert.Throws<KeyNotFoundException>(() =>
                s[p]
            );
        }

        [Fact]
        public void Get_index_returns_dynamic_collection()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var t = g.Triples.First();
            var n = t.Subject;
            var s = new DynamicNode(n);
            var p = t.Predicate;

            var actual = s[p];

            Assert.IsType<DynamicObjectCollection>(actual);
        }

        [Fact]
        public void Set_index_requires_key()
        {
            var g = new Graph();
            var n = g.CreateBlankNode();
            var s = new DynamicNode(n);

            Assert.Throws<ArgumentNullException>(() =>
                s[null as INode] = null
            );
        }

        [Fact]
        public void Set_index_with_null_value_retracts_by_subject_predicate()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s1> <urn:p2> ""o3"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new Graph();
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p1> ""o2"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var t = actual.Triples.First();
            var n = t.Subject;
            var p = t.Predicate;
            var s = new DynamicNode(n);

            s[p] = null;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Set_index_overwrites_by_subject()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s1> <urn:p1> ""o"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var actual = new Graph();
            actual.LoadFromString(@"
<urn:s1> <urn:p1> ""o1"" .
<urn:s1> <urn:p1> ""o2"" .
<urn:s1> <urn:p2> ""o3"" .
<urn:s2> <urn:s1> ""o6"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var t = actual.Triples.First();
            var n = t.Subject;
            var p = t.Predicate;
            var s = new DynamicNode(n);

            s[p] = "o";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Keys_are_predicate_nodes()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> .
<urn:s1> <urn:p1> <urn:o2> .
<urn:s1> <urn:p2> <urn:o3> .
<urn:s2> <urn:s1> <urn:o5> .
<urn:s3> <urn:p3> <urn:s1> .
");

            var t = g.Triples.First();
            var n = t.Subject;
            var p = t.Predicate;
            var s = new DynamicNode(n);

            var actual = ((IDictionary<INode, object>)s).Keys;
            var expected = g.GetTriplesWithSubject(n).Select(triple => triple.Predicate).Distinct();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_requires_key()
        {
            var g = new Graph();
            var n = g.CreateBlankNode();
            var s = new DynamicNode(n);

            Assert.Throws<ArgumentNullException>(() =>
            {
                s.Add(null as INode, null);
            });
        }

        [Fact]
        public void Add_requires_value()
        {
            var g = new Graph();
            var n = g.CreateBlankNode();
            var s = new DynamicNode(n);
            var p = g.CreateBlankNode();

            Assert.Throws<ArgumentNullException>(() =>
            {
                s.Add(p, null);
            });
        }

        [Fact]
        public void Add_rejects_existing_key()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var t = g.Triples.First();
            var n = t.Subject;
            var p = t.Predicate;
            var s = new DynamicNode(n);

            Assert.Throws<ArgumentException>(() =>
            {
                s.Add(p, 0);
            });
        }

        [Fact]
        public void Add_handles_enumerables()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
_:s <urn:p> ""o1"" .
_:s <urn:p> ""o2"" .
");

            var g = new Graph();
            var n = g.CreateBlankNode();
            var p = g.CreateUriNode(new Uri("urn:p"));
            var s = new DynamicNode(n);

            s.Add(p, new[] { "o1", "o2" });

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Add_rdf_collections_are_not_enumerables()
        {
            var unexpected = new Graph();
            unexpected.LoadFromString(@"
_:s <urn:p> ""a"" .
_:s <urn:p> ""b"" .
_:s <urn:p> ""c"" .
");

            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

_:s <urn:p> (""a"" ""b"" ""c"") .
");

            var g = new Graph();
            var n = g.CreateBlankNode();
            var p = g.CreateUriNode(new Uri("urn:p"));
            var s = new DynamicNode(n);

            s.Add(p, new RdfCollection(new[] { "a", "b", "c" }));

            Assert.NotEqual(unexpected, g);
            Assert.Equal(expected, g);
        }

        [Fact]
        public void Add_strings_are_not_enumerables()
        {
            var unexpected = new Graph();
            unexpected.LoadFromString(@"
_:s <urn:p> ""a"" .
_:s <urn:p> ""b"" .
_:s <urn:p> ""c"" .
_:s <urn:p> ""d"" .
_:s <urn:p> ""e"" .
_:s <urn:p> ""f"" .
_:s <urn:p> ""g"" .
_:s <urn:p> ""h"" .
_:s <urn:p> ""i"" .
_:s <urn:p> ""j"" .
");

            var expected = new Graph();
            expected.LoadFromString(@"
_:s <urn:p> ""abcdefghij"" .
");

            var g = new Graph();
            var n = g.CreateBlankNode();
            var p = g.CreateUriNode(new Uri("urn:p"));
            var s = new DynamicNode(n);

            s.Add(p, "abcdefghij");

            Assert.NotEqual(unexpected, g);
            Assert.Equal(expected, g);
        }

        [Fact]
        public void Contains_rejects_null_key()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var t = g.Triples.First();
            var n = t.Subject;
            var p = t.Predicate;
            var s = new DynamicNode(n);

            var condition = s.Contains(new KeyValuePair<INode, object>(null, null));

            Assert.False(condition);
        }
    }
}
