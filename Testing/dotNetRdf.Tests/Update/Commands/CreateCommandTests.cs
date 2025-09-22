using System;
using Xunit;

namespace VDS.RDF.Update.Commands;
public class CreateCommandTests
{
    [Fact]
    public void GraphNameIsRequired()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new CreateCommand((IRefNode)null));
        Assert.Equal("graphName", ex.ParamName);
    }
}