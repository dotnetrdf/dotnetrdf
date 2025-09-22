using System;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Update.Commands;

public class InsertDataCommandTests
{
    [Fact]
    public void InsertionPatternIsRequired()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new InsertDataCommand(null));
        Assert.Equal("pattern", ex.ParamName);
    }

    [Fact]
    public void InsertionPatternMustBeConcreteTriples()
    {
        var pattern = new GraphPattern();
        pattern.AddTriplePattern(new TriplePattern(
            new VariablePattern("s"),
            new VariablePattern("p"),
            new VariablePattern("o")
        ));
        var ex = Assert.Throws<SparqlUpdateException>(() => new InsertDataCommand(pattern));
        Assert.Equal("Cannot create a INSERT DATA command where any of the Triple Patterns are not concrete triples (variables are not permitted) or a GRAPH clause has nested Graph Patterns", ex.Message);
    }
}