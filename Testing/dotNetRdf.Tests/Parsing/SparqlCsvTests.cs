using System;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;


public class SparqlCsvTests
{
    private readonly SparqlCsvParser _parser = new SparqlCsvParser();
    private readonly ITestOutputHelper _output;

    public SparqlCsvTests(ITestOutputHelper output)
    {
        _output = output;
    }
    private void CheckVariables(SparqlResultSet results, params String[] vars)
    {
        foreach (var var in vars)
        {
            Assert.True(results.Variables.Contains(var), "Missing variable ?" + var);
        }
    }

    [Fact]
    public void ParsingSparqlCsv01()
    {
        const String data = @"x,y
http://x,http://y
";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        TestTools.ShowResults(results);

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(2, results.Variables.Count());
        CheckVariables(results, "x", "y");
        Assert.Single(results.Results);
    }

    [Fact]
    public void ParsingSparqlCsv02()
    {
        // Header row has quoting - CORE-433
        const String data = @"""x"",""y""
http://x,http://y
";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        TestTools.ShowResults(results);

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(2, results.Variables.Count());
        CheckVariables(results, "x", "y");
        Assert.Single(results.Results);
    }

    [Fact]
    public void ParsingSparqlCsv03()
    {
        // Invalid URI - CORE-432
        // As CSV is lossy and doesn't distinguish between URIs and literals we treat it as a literal
        const String data = @"""x"",""y""
http://x a bad uri,http://y
";

        var results = new SparqlResultSet();
        _parser.Load(results, new StringReader(data));

        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Equal(2, results.Variables.Count());
        CheckVariables(results, "x", "y");
        Assert.Single(results.Results);

        INode n = results.Results[0]["x"];
        Assert.NotNull(n);
        Assert.Equal(NodeType.Literal, n.NodeType);
    }
}
