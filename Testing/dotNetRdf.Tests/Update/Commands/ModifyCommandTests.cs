
using System;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Update.Commands;

public class ModifyCommandTests
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
        var insertPattern = new GraphPattern();
        insertPattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new ModifyCommand(null, insertPattern, wherePattern));
        Assert.Equal("deletions", ex.ParamName);
    }

    [Fact]
    public void InsertionPatternIsRequired()
    {
        var wherePattern = new GraphPattern();
        wherePattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var deletePattern = new GraphPattern();
        deletePattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new ModifyCommand(deletePattern, null, wherePattern));
        Assert.Equal("insertions", ex.ParamName);
    }

    [Fact]
    public void WherePatternIsRequired()
    {
        var deletePattern = new GraphPattern();
        deletePattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var insertPattern = new GraphPattern();
        insertPattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<ArgumentNullException>(() => new ModifyCommand(deletePattern, insertPattern, null));
        Assert.Equal("where", ex.ParamName);
    }
}
