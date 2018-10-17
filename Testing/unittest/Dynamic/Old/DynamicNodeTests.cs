namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ObjectCollectionTests
    {
        [Fact]
        public void Supports_blank_nodes()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> _:o .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var actual = ((dynamic_s["http://example.com/p"] as IEnumerable<object>).Single() as IBlankNode).InternalID;
            var expected = "o";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Converts_native_data_types()
        {
            var eg = new Graph();
            eg.LoadFromString(@"
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

            var d = new Graph().AsDynamic(new Uri("http://example.com/"));
            var node = new NodeFactory().CreateBlankNode("blank");

            d.s = new
            {
                node,
                uri = new Uri("http://example.com/uri"),
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

            Assert.Equal(eg as IGraph, d as IGraph);
        }

        [Fact]
        public void Requires_known_native_data_types()
        {
            var d = new Graph().AsDynamic(new Uri("http://example.com/"));

            Assert.Throws<InvalidOperationException>(() =>
            {
                d.s = new
                {
                    unknown = new object()
                };
            });
        }

        [Fact]
        public void Supports_remove_with_missing_item()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();

            var objects = dynamic_s["http://example.com/p"] as ICollection<object>;
            var result = objects.Remove(0);

            Assert.False(result);
        }
    }
}
