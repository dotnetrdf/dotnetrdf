using System;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Update.Commands;
public class DeleteCommandTests
{
    [Fact]
    public void DeletionPatternIsRequired()
    {
        var wherePattern = new GraphPattern();
        wherePattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new DeleteCommand(null, wherePattern));
        Assert.Equal("deletions", ex.ParamName);
    }

    [Fact]
    public void WherePatternIsRequired()
    {
        var graphPattern = new GraphPattern();
        graphPattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new DeleteCommand(graphPattern, (GraphPattern)null));
        Assert.Equal("where", ex.ParamName);
    }
}