namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using Xunit;

    public class DynamicObjectCollectionTests
    {
        [Fact]
        public void Requires_subject()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicObjectCollection(null, null)
            );
        }

        [Fact]
        public void Requires_predicate()
        {
            var s = new NodeFactory().CreateBlankNode();
            var d = new DynamicNode(s);

            Assert.Throws<ArgumentNullException>(() =>
                new DynamicObjectCollection(d, null)
            );
        }

        [Fact]
        public void Requires_subject_graph()
        {
            var s = new NodeFactory().CreateBlankNode();
            var p = new NodeFactory().CreateBlankNode();
            var d = new DynamicNode(s);

            Assert.Throws<InvalidOperationException>(() =>
                new DynamicObjectCollection(d, p)
            );
        }

        [Fact]
        public void Count()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> . # 1
<urn:s1> <urn:p1> <urn:o2> . # 2
<urn:s1> <urn:p2> <urn:o3> . # wrong predicate
<urn:s2> <urn:p1> <urn:o4> . # wrong subject
<urn:s2> <urn:s1> <urn:o5> . # wrong subject
<urn:s2> <urn:p3> <urn:s1> . # wrong subject
");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            Assert.Equal(
                g.GetTriplesWithSubjectPredicate(s, p).Count(),
                c.Count());
        }

        [Fact]
        public void IsReadOnly()
        {
            var g = new Graph();
            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            Assert.False(c.IsReadOnly);
        }

        [Fact]
        public void Add()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var g = new Graph();
            var s = g.CreateUriNode(new Uri("urn:s"));
            var p = g.CreateUriNode(new Uri("urn:p"));
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            c.Add("o");

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Clear()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s1> <urn:p2> <urn:o3> .
<urn:s2> <urn:p1> <urn:o4> .
<urn:s1> <urn:p3> <urn:p1> .
<urn:s2> <urn:s1> <urn:o5> .
<urn:s2> <urn:p3> <urn:s1> .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o1> . # should retract
<urn:s1> <urn:p1> <urn:o2> . # should retract
<urn:s1> <urn:p2> <urn:o3> . # should remain - wrong predicate
<urn:s1> <urn:p3> <urn:p1> . # should remain - wrong predicate
<urn:s2> <urn:p1> <urn:o4> . # should remain - wrong subject
<urn:s2> <urn:s1> <urn:o5> . # should remain - wrong subject
<urn:s2> <urn:p3> <urn:s1> . # should remain - wrong subject
");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            c.Clear();

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Contains()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var o = g.Triples.First().Object.AsValuedNode().AsString();
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

#pragma warning disable xUnit2017 // Contains is actually the method under test
            Assert.True(c.Contains(o));
            Assert.False(c.Contains("other"));
#pragma warning restore xUnit2017
        }

        [Fact]
        public void CopyTo()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var o = g.Triples.Single().Object.AsValuedNode().AsString();
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            var objects = new object[g.Triples.Count()];
            c.CopyTo(objects, 0);

            Assert.Equal(
                new[] { o },
                objects);
        }

        [Fact]
        public void GetEnumerator()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var o = g.Triples.Single().Object.AsValuedNode().AsString();
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            using (var enumerator = c.GetEnumerator())
            {
                enumerator.MoveNext();

                Assert.Equal(
                    o,
                    enumerator.Current);
            }
        }

        [Fact]
        public void Remove()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s1> <urn:p1> ""other"" .
<urn:s1> <urn:p2> ""o"" .
<urn:s2> <urn:p1> ""o"" .
<urn:s2> <urn:s1> ""o"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s1> <urn:p1> ""o"" .
<urn:s1> <urn:p1> ""other"" .
<urn:s1> <urn:p2> ""o"" .
<urn:s2> <urn:p1> ""o"" .
<urn:s2> <urn:s1> ""o"" .
<urn:s2> <urn:p3> <urn:s1> .
");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var o = g.Triples.First().Object.AsValuedNode().AsString();
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            c.Remove(o);

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void IEnumerableGetEnumerator()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var o = g.Triples.Single().Object.AsValuedNode().AsString();
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p) as IEnumerable;

            var enumerator = c.GetEnumerator();
            enumerator.MoveNext();

            Assert.Equal(
                o,
                enumerator.Current);
        }

        [Fact]
        public void x()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <uri:x> .
<urn:s> <urn:p> _:blank .
<urn:s> <urn:p> ""0""^^<http://www.w3.org/2001/XMLSchema#double> .
<urn:s> <urn:p> ""0""^^<http://www.w3.org/2001/XMLSchema#float> .
<urn:s> <urn:p> ""0""^^<http://www.w3.org/2001/XMLSchema#decimal> .
<urn:s> <urn:p> ""false""^^<http://www.w3.org/2001/XMLSchema#boolean> .
<urn:s> <urn:p> ""1900-01-01""^^<http://www.w3.org/2001/XMLSchema#dateTime> .
<urn:s> <urn:p> ""P1D""^^<http://www.w3.org/2001/XMLSchema#duration> .
<urn:s> <urn:p> ""0""^^<http://www.w3.org/2001/XMLSchema#int> .
<urn:s> <urn:p> ""s1"" .
<urn:s> <urn:p> ""s2""^^<http://example.com/datatype> .
<urn:s> <urn:p> ""s3""@en .
");

            var s = g.Triples.First().Subject;
            var p = g.Triples.First().Predicate;
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);

            var objects = new object[g.Triples.Count()];
            c.CopyTo(objects, 0);

            Assert.IsType<DynamicNode>(objects[0]);
            Assert.IsType<DynamicNode>(objects[1]);
            Assert.IsType<double>(objects[2]);
            Assert.IsType<float>(objects[3]);
            Assert.IsType<decimal>(objects[4]);
            Assert.IsType<bool>(objects[5]);
            Assert.IsType<DateTimeOffset>(objects[6]);
            Assert.IsType<TimeSpan>(objects[7]);
            Assert.IsType<long>(objects[8]);
            Assert.IsType<string>(objects[9]);
            Assert.IsType<LiteralNode>(objects[10]);
            Assert.IsType<LiteralNode>(objects[11]);
        }
    }
}
