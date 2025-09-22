using System;
using Xunit;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands;

public class InsertCommandTests
{
    [Fact]
    public void InsertionPatternIsRequired()
    {
        var wherePattern = new GraphPattern();
        wherePattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new InsertCommand(null, wherePattern));
        Assert.Equal("insertions", ex.ParamName);
    }

    [Fact]
    public void WherePatternIsRequired() {
        var graphPattern = new GraphPattern();
        graphPattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new InsertCommand(graphPattern, (GraphPattern)null));
        Assert.Equal("where", ex.ParamName);
    }
}