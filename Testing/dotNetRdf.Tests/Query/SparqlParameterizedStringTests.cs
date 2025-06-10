using Xunit;

namespace VDS.RDF.Query;

public class SparqlParameterizedStringTests
{
    [Fact]
    public void ShouldReplaceVariableStartingWithUnderscore()
    {
        var sut = new SparqlParameterizedString("SELECT * WHERE { ?s a ?_o }");
        sut.SetVariable("_o", new NodeFactory().CreateLiteralNode("test"));

        Assert.Equal("SELECT * WHERE { ?s a \"test\" }", sut.ToString());
    }

    [Fact]
    public void ShouldReplaceParameterStartingWithUnderscore()
    {
        var sut = new SparqlParameterizedString("SELECT * WHERE { ?s a @_o }");
        sut.SetParameter("_o", new NodeFactory().CreateLiteralNode("test"));

        Assert.Equal("SELECT * WHERE { ?s a \"test\" }", sut.ToString());
    }

    [Fact]
    public void SetVariableShouldIgnoreVariablePrefixChar()
    {
        var sut = new SparqlParameterizedString("SELECT * WHERE { ?s a ?o }");
        sut.SetVariable("?o", new NodeFactory().CreateLiteralNode("test"));

        Assert.Equal("SELECT * WHERE { ?s a \"test\" }", sut.ToString());
    }

    [Fact]
    public void SetVariableShouldIgnoreVariablePrefixChar2()
    {
        var sut = new SparqlParameterizedString("SELECT * WHERE { ?s a ?o }");
        sut.SetVariable("$o", new NodeFactory().CreateLiteralNode("test"));

        Assert.Equal("SELECT * WHERE { ?s a \"test\" }", sut.ToString());
    }

    [Fact]
    public void SetParameterShouldIgnoreParameterPrefixChar()
    {
        var sut = new SparqlParameterizedString("SELECT * WHERE { ?s a @o }");
        sut.SetParameter("@o", new NodeFactory().CreateLiteralNode("test"));

        Assert.Equal("SELECT * WHERE { ?s a \"test\" }", sut.ToString());
    }

}
