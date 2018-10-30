namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicGraphDictionaryTests
    {
        [Fact]
        public void Values()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o01> .
<urn:s1> <urn:p1> <urn:o02> .
<urn:s1> <urn:p2> <urn:o03> .
<urn:s1> <urn:p2> <urn:o04> .
<urn:s2> <urn:p3> <urn:o05> .
<urn:s2> <urn:p3> <urn:o06> .
<urn:s2> <urn:p4> <urn:o07> .
<urn:s2> <urn:p4> <urn:o08> .
_:s3 <urn:p5> <urn:o9> .
_:s3 <urn:p5> <urn:o10> .
_:s3 <urn:p6> <urn:o11> .
_:s3 <urn:p6> <urn:o12> .
");


            Assert.Equal(d.Triples.SubjectNodes.UriNodes(), d.Values);

            foreach (var value in d.Values)
            {
                Assert.IsType<DynamicNode>(value);
            }
        }

        [Fact]
        public void Count()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s1> <urn:p1> <urn:o01> .
<urn:s1> <urn:p1> <urn:o02> .
<urn:s1> <urn:p2> <urn:o03> .
<urn:s1> <urn:p2> <urn:o04> .
<urn:s2> <urn:p3> <urn:o05> .
<urn:s2> <urn:p3> <urn:o06> .
<urn:s2> <urn:p4> <urn:o07> .
<urn:s2> <urn:p4> <urn:o08> .
_:s3 <urn:p5> <urn:o9> .
_:s3 <urn:p5> <urn:o10> .
_:s3 <urn:p6> <urn:o11> .
_:s3 <urn:p6> <urn:o12> .
");

            Assert.Equal(d.Triples.SubjectNodes.UriNodes().Count(), d.Count);
        }

        [Fact]
        public void Is_writable()
        {
            var g = new DynamicGraph();

            Assert.False(g.IsReadOnly);
        }

        [Fact]
        public void Enumerates_pairs_with_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var s = g.Nodes.First();

            var dict = g as IEnumerable;

            var enumerator = dict.GetEnumerator();
            enumerator.MoveNext();

            var pair = (KeyValuePair<INode, object>)enumerator.Current;

            var key = pair.Key;
            var value = pair.Value;

            Assert.Equal(key, s);
            Assert.Equal(value, s);
            Assert.IsType<DynamicNode>(value);
        }
    }
}
