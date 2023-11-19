using FluentAssertions;
using System.Text;
using Xunit;

namespace VDS.RDF;

public class ExtensionsTests
{
    [Fact]
    public void AppendBlockIndentedLfOnlyNoFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one\nline two\nline three", "    ");
        sb.ToString().Should().Be("""
                                  line one
                                      line two
                                      line three
                                  """);
    }

    [Fact]
    public void AppendBlockIndentedLfOnlyFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one\nline two\nline three", "    ", true);
        sb.ToString().Should().Be("""
                                      line one
                                      line two
                                      line three
                                  """);
    }
    [Fact]
    public void AppendBlockIndentedCrlfNoFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one\r\nline two\r\nline three", "    ");
        sb.ToString().Should().Be("""
                                  line one
                                      line two
                                      line three
                                  """);
    }
    
    [Fact]
    public void AppendBlockIndentedCrlfFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one\r\nline two\r\nline three", "    ", true);
        sb.ToString().Should().Be("""
                                      line one
                                      line two
                                      line three
                                  """);
    }

    [Fact]
    public void AppendBlockIndentedNoLineEndingNoFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one", "    ");
        sb.ToString().Should().Be("line one");
    }
    
    [Fact]
    public void AppendBlockIndentedNoLineEndingFirstLineIndent()
    {
        var sb = new StringBuilder();
        sb.AppendBlockIndented("line one", "    ", true);
        sb.ToString().Should().Be("    line one");
    }

}