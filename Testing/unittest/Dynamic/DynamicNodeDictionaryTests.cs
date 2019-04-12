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
    using Xunit;

    public class DynamicNodeDictionaryTests
    {
        [Fact]
        public void Values_are_objects_per_predicate_for_this_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1.1
<urn:s> <urn:s> <urn:p> . # 1.2
<urn:s> <urn:s> <urn:o> . # 1.3
<urn:s> <urn:p> <urn:s> . # 2.1
<urn:s> <urn:p> <urn:p> . # 2.2
<urn:s> <urn:p> <urn:o> . # 2.3
<urn:s> <urn:o> <urn:s> . # 3.1
<urn:s> <urn:o> <urn:p> . # 3.2
<urn:s> <urn:o> <urn:o> . # 3.3
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
            var d = new DynamicNode(s);
            var expected = new[] { s, p, o };

            void isSPO(object actual)
            {
                Assert.Equal(expected, actual);
                Assert.IsType<DynamicObjectCollection>(actual);
            }

            Assert.Collection(
                d.Values,
                isSPO,
                isSPO,
                isSPO);
        }

        [Fact]
        public void Counts_predicates_for_this_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1
<urn:s> <urn:s> <urn:p> . # 1
<urn:s> <urn:s> <urn:o> . # 1
<urn:s> <urn:p> <urn:s> . # 2
<urn:s> <urn:p> <urn:p> . # 2
<urn:s> <urn:p> <urn:o> . # 2
<urn:s> <urn:o> <urn:s> . # 3
<urn:s> <urn:o> <urn:p> . # 3
<urn:s> <urn:o> <urn:o> . # 3
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
            var d = new DynamicNode(s);

            Assert.Equal(3, d.Count);
        }

        [Fact]
        public void Is_writable()
        {
            var g = new Graph();
            var s = g.CreateBlankNode();
            var d = new DynamicNode(s);

            Assert.False(d.IsReadOnly);
        }

        [Fact]
        public void Clear_retracts_statements_with_this_subject()
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

            var g = new Graph();
            g.LoadFromString(@"
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

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = new DynamicNode(s);

            d.Clear();

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Enumerates_pairs_with_predicate_key_and_dynamic_objects_value()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:s> <urn:s> . # 1.1
<urn:s> <urn:s> <urn:p> . # 1.2
<urn:s> <urn:s> <urn:o> . # 1.3
<urn:s> <urn:p> <urn:s> . # 2.1
<urn:s> <urn:p> <urn:p> . # 2.2
<urn:s> <urn:p> <urn:o> . # 2.3
<urn:s> <urn:o> <urn:s> . # 3.1
<urn:s> <urn:o> <urn:p> . # 3.2
<urn:s> <urn:o> <urn:o> . # 3.3
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
            var d = (IEnumerable)new DynamicNode(s);
            var spo = new[] { s, p, o };

            var actual = d.GetEnumerator();
            var expected = spo.GetEnumerator();
            while (expected.MoveNext() | actual.MoveNext())
            {
                var current = (KeyValuePair<INode, object>)actual.Current;

                Assert.Equal(expected.Current, current.Key);
                Assert.IsType<DynamicObjectCollection>(current.Value);
                Assert.Equal(spo, current.Value);
            }
        }
    }
}
