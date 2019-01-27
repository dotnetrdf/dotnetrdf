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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicGraphDictionaryTests
    {
        [Fact]
        public void Values_are_dynamic_uri_nodes()
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

            Assert.Equal(d.Nodes.UriNodes(), d.Values);

            foreach (var value in d.Values)
            {
                Assert.IsType<DynamicNode>(value);
            }
        }

        [Fact]
        public void Counts_uri_nodes()
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

            Assert.Equal(d.Nodes.UriNodes().Count(), d.Count);
        }

        [Fact]
        public void Is_writable()
        {
            var g = new DynamicGraph();

            Assert.False(g.IsReadOnly);
        }

        [Fact]
        public void Enumerates_pairs_with_uri_key_and_dynamic_value()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1
<urn:s> <urn:s> <urn:p> .
<urn:s> <urn:s> <urn:o> .
<urn:s> <urn:p> <urn:s> .
<urn:s> <urn:p> <urn:p> .
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:o> <urn:s> .
<urn:s> <urn:o> <urn:p> .
<urn:s> <urn:o> <urn:o> .
<urn:p> <urn:s> <urn:s> . # 2
<urn:p> <urn:s> <urn:p> .
<urn:p> <urn:s> <urn:o> .
<urn:p> <urn:p> <urn:s> .
<urn:p> <urn:p> <urn:p> .
<urn:p> <urn:p> <urn:o> .
<urn:p> <urn:o> <urn:s> .
<urn:p> <urn:o> <urn:p> .
<urn:p> <urn:o> <urn:o> .
<urn:o> <urn:s> <urn:s> . # 3
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
            var d = (IEnumerable)g;
            var spo = new[] { s, p, o };

            var actual = d.GetEnumerator();
            var expected = spo.GetEnumerator();
            while (expected.MoveNext() | actual.MoveNext())
            {
                var current = (KeyValuePair<INode, object>)actual.Current;

                Assert.Equal(expected.Current, current.Key);
                Assert.IsType<DynamicNode>(current.Value);
                Assert.Equal(expected.Current, current.Value);
            }
        }
    }
}
