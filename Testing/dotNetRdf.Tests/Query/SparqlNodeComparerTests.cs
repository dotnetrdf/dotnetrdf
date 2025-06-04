using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Query;

public enum ExpectCompare
{
    Error,
    Lower,
    Equal,
    Greater
}

public class SparqlNodeComparerTests
{
    public static IEnumerable<TheoryDataRow<INode, INode, ExpectCompare>> NodeCompareTestData = [
        // URI Nodes are not comparable
        new(
            new UriNode(new Uri("http://example/a")), new UriNode(new Uri("http://example/b")),
            ExpectCompare.Error
        ),
        // Blank Nodes are not comparable
        new(new BlankNode("a"), new BlankNode("b"), ExpectCompare.Error ),
        // Numeric nodes are comparable
        new(
            new LiteralNode("123", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            new LiteralNode("123.0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal)),
            ExpectCompare.Equal
        ),
        // Numeric nodes are comparable
        new(
            new LiteralNode("123", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)),
            new LiteralNode("1.24E2", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal)),
            ExpectCompare.Lower
        ),
        // Simple Literals are comparable
        new(new LiteralNode("a"), new LiteralNode("b"), ExpectCompare.Lower),
        // XSD string literals are comparable
        new(
            new LiteralNode("b", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
            new LiteralNode("a", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)), ExpectCompare.Greater
        ),
        // XSD boolean literals are comparable
        new(
            new LiteralNode("true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)),
            new LiteralNode("false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean)),
            ExpectCompare.Greater
        ),
        // XSD dateTime literals are comparable
        new(
            new LiteralNode("2020-01-01T00:00:00Z", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
            new LiteralNode("2020-01-01T01:00:00+01:00", new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime)),
            ExpectCompare.Equal
        )
    ];

    private readonly SparqlNodeComparer _comparer;

    public SparqlNodeComparerTests()
    {
        _comparer = new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);
    }

    [Theory]
    [MemberData(nameof(NodeCompareTestData))]
    [Obsolete("Tests obsolete compare method")]
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

    [Theory]
    [MemberData(nameof(NodeCompareTestData))]
    public void TestSparqlTryCompare(INode left, INode right, ExpectCompare expect)
    {
        int result;
        switch (expect)
        {
            case ExpectCompare.Error:
                Assert.False(_comparer.TryCompare(left, right, out result));
                break;
            case ExpectCompare.Equal:
                _comparer.TryCompare(left, right, out result).Should().BeTrue();
                result.Should().Be(0);
                break;
            case ExpectCompare.Lower:
                _comparer.TryCompare(left, right, out result).Should().BeTrue();
                result.Should().BeNegative();
                break;
            case ExpectCompare.Greater:
                _comparer.TryCompare(left, right, out result).Should().BeTrue();
                result.Should().BePositive();
                break;
        }
    }

}
