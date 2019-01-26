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

namespace VDS.RDF.Dynamic.Old
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicNodeTests
    {
        public void Supports_blank_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> _:o .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var actual = ((dynamic_s["http://example.com/p"] as IEnumerable<object>).Single() as IBlankNode).InternalID;
            var expected = "o";

            Assert.Equal(expected, actual);
        }

        public void Converts_native_data_types()
        {
            var g = new Graph();
            g.LoadFromString(@"
<http://example.com/s> <http://example.com/node> _:blank .
<http://example.com/s> <http://example.com/uri> <http://example.com/uri> .
<http://example.com/s> <http://example.com/bool> ""true""^^<http://www.w3.org/2001/XMLSchema#boolean> .
<http://example.com/s> <http://example.com/byte> ""255""^^<http://www.w3.org/2001/XMLSchema#unsignedByte> .
<http://example.com/s> <http://example.com/dateTime> ""9999-12-31T23:59:59.999999""^^<http://www.w3.org/2001/XMLSchema#dateTime> .
<http://example.com/s> <http://example.com/dateTimeOffset> ""9999-12-31T23:59:59.999999+00:00""^^<http://www.w3.org/2001/XMLSchema#dateTime> .
<http://example.com/s> <http://example.com/decimal> ""79228162514264337593543950335""^^<http://www.w3.org/2001/XMLSchema#decimal> .
<http://example.com/s> <http://example.com/double> ""1.79769313486232E+308""^^<http://www.w3.org/2001/XMLSchema#double> .
<http://example.com/s> <http://example.com/float> ""3.402823E+38""^^<http://www.w3.org/2001/XMLSchema#float> .
<http://example.com/s> <http://example.com/long> ""9223372036854775807""^^<http://www.w3.org/2001/XMLSchema#integer> .
<http://example.com/s> <http://example.com/int> ""2147483647""^^<http://www.w3.org/2001/XMLSchema#integer> .
<http://example.com/s> <http://example.com/string> """" .
<http://example.com/s> <http://example.com/char> ""\uFFFF"" .
<http://example.com/s> <http://example.com/timespan> ""P10675199DT2H48M5.4775807S""^^<http://www.w3.org/2001/XMLSchema#duration> .
");

            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));
            var node = new NodeFactory().CreateBlankNode("blank");

            d.s = new
            {
                node,
                uri = UriFactory.Create("http://example.com/uri"),
                @bool = true,
                @byte = byte.MaxValue,
                dateTime = DateTime.MaxValue,
                dateTimeOffset = DateTimeOffset.MaxValue,
                @decimal = decimal.MaxValue,
                @double = double.MaxValue,
                @float = float.MaxValue,
                @long = long.MaxValue,
                @int = int.MaxValue,
                @string = string.Empty,
                @char = char.MaxValue,
                timespan = TimeSpan.MaxValue,
            };

            Assert.Equal<IGraph>(g, d);
        }

        public void Requires_known_native_data_types()
        {
            var d = new Graph().AsDynamic(UriFactory.Create("http://example.com/"));

            Assert.Throws<InvalidOperationException>(() =>
                d.s = new
                {
                    unknown = new object()
                }
            );
        }
    }
}
