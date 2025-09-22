using System;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Update.Commands;
public class DeleteDataCommandTests
{
    [Fact]
    public void DeletionPatternIsRequired()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DeleteDataCommand((GraphPattern)null));
        Assert.Equal("pattern", ex.ParamName);
    }
}