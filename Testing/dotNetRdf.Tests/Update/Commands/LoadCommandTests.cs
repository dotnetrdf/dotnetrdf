using System;
using Xunit;

namespace VDS.RDF.Update.Commands;

public class LoadCommandTests
{
    [Fact]
    public void SourceUriIsRequired()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new LoadCommand(null, (IRefNode)null, false, null));
        Assert.Equal("sourceUri", ex.ParamName);
    }
}