using System;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;


public class SparqlTsvTests
{
    private readonly SparqlTsvParser _parser = new SparqlTsvParser();

    private void CheckVariables(SparqlResultSet results, params String[] vars)
    {
        foreach (var var in vars)
        {
            Assert.True(results.Variables.Contains(var), "Missing variable ?" + var);
        }
    }

    [Fact]
    public void ParsingSparqlTsv01()
    {
        const String data = "?x\t?y\n"
                            + "<http://x>\t<http://y>\n";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        TestTools.ShowResults(results);

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(2, results.Variables.Count());
        CheckVariables(results, "x", "y");
        Assert.Single(results.Results);
    }

    [Fact]
    public void ParsingSparqlTsv02()
    {
        // Relative URI - CORE-432
        const String data = "?x\t?y\n"
                            + "<http://x>\t<y>\n";

        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }

    [Fact]
    public void ParsingSparqlTsv03()
    {
        // Invalid URI - CORE-432
        const String data = "?x\t?y\n"
                            + "<http://x a bad uri>\t<y>\n";

        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => _parser.Load(results, new StringReader(data)));
    }
}