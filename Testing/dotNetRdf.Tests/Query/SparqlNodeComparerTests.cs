using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Query
{
    public enum ExpectCompare
    {
        Error,
        Lower,
        Equal,
        Greater
    }

    public class SparqlNodeComparerTests
    {
        public static IEnumerable<object[]> NodeCompareTestData = new[]
        {
            // URI Nodes are not comparable
            new object[]
            {
                new UriNode(new Uri("http://example/a")), new UriNode(new Uri("http://example/b")),
                ExpectCompare.Error
            },
            // Blank Nodes are not comparable
            new object[] { new BlankNode("a"), new BlankNode("b"), ExpectCompare.Error },
            // Numeric nodes are comparable
            new object[]
            {
                new LiteralNode("123", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger), true),
                new LiteralNode("123.0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal), true),
                ExpectCompare.Equal
            },
            // Numeric nodes are comparable
            new object[]
            {
                new LiteralNode("123", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger), true),
                new LiteralNode("1.24E2", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal), true),
                ExpectCompare.Lower
            },
            // Simple Literals are comparable
            new object[] { new LiteralNode("a", true), new LiteralNode("b", true), ExpectCompare.Lower },
            // XSD string literals are comparable
            new object[]
            {
                new LiteralNode("b", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString), true),
                new LiteralNode("a", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString), true), ExpectCompare.Greater
            },
            // XSD boolean literals are comparable
            new object[]
            {
                new LiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean), true),
                new LiteralNode("false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean), true),
                ExpectCompare.Greater
            },
            // XSD dateTime literals are comparable
            new object[]
            {
                new LiteralNode("2020-01-01T00:00:00Z", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime), true),
                new LiteralNode("2020-01-01T01:00:00+01:00", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime),
                    true),
                ExpectCompare.Equal
            }
        };

        private readonly SparqlNodeComparer _comparer;

        public SparqlNodeComparerTests()
        {
            _comparer = new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);
        }

        [Theory]
        [MemberData(nameof(NodeCompareTestData))]
        public void TestSparqlComparison(INode left, INode right, ExpectCompare expect)
        {
            switch (expect)
            {
                case ExpectCompare.Error:
                    Assert.Throws<RdfQueryComparisonException>(() => { _comparer.Compare(left, right); });
                    break;
                case ExpectCompare.Equal:
                    _comparer.Compare(left, right).Should().Be(0);
                    break;
                case ExpectCompare.Lower:
                    _comparer.Compare(left, right).Should().BeLessThan(0);
                    break;
                case ExpectCompare.Greater:
                    _comparer.Compare(left, right).Should().BeGreaterThan(0);
                    break;
            }
        }
        
    }
}
