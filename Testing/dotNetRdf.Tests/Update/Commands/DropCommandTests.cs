using System;
using Xunit;

namespace VDS.RDF.Update.Commands;
public class DropCommandTests
{
    [Fact]
    public void GraphModeWithNullGraphNameIsTreatedAsDefaultMode()
    {
        var cmd = new DropCommand((IRefNode)null, ClearMode.Graph, false);
        Assert.Null(cmd.TargetGraphName);
        Assert.Equal(ClearMode.Default, cmd.Mode);
        Assert.False(cmd.Silent);
    }

    [Fact]
    public void DefaultModeIgnoresGraphName()
    {
        var cmd = new DropCommand(new UriNode(new Uri("http://example.org/graph")), ClearMode.Default, true);
        Assert.Null(cmd.TargetGraphName);
        Assert.Equal(ClearMode.Default, cmd.Mode);
        Assert.True(cmd.Silent);
    }

    [Fact]
    public void ModeCanBeAll()
    {
        var cmd = new DropCommand((IRefNode)null, ClearMode.All, false);
        Assert.Null(cmd.TargetGraphName);
        Assert.Equal(ClearMode.All, cmd.Mode);
        Assert.False(cmd.Silent);
    }

    [Fact]
    public void ModeCanBeGraph()
    {
        var cmd = new DropCommand(new UriNode(new Uri("http://example.org/graph")), ClearMode.Graph, true);
        Assert.NotNull(cmd.TargetGraphName);
        Assert.Equal(new Uri("http://example.org/graph"), ((IUriNode)cmd.TargetGraphName).Uri);
        Assert.Equal(ClearMode.Graph, cmd.Mode);
        Assert.True(cmd.Silent);
    }
}