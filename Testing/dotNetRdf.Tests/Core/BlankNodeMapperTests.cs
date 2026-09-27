using FluentAssertions;
using static FluentAssertions.FluentActions;
using System;
using Xunit;

namespace VDS.RDF;

public class BlankNodeMapperTests
{
    private const string GivenPrefix = "mine";

    [Fact]
    public void DefaultPrefixUsed() =>
        new BlankNodeMapper().GetNextID().Should().StartWith(BlankNodeMapper.DefaultPrefix);

    [Fact]
    public void GivenPrefixUsed() =>
        new BlankNodeMapper(GivenPrefix).GetNextID().Should().StartWith(GivenPrefix);

    [Fact]
    public void NullPrefixThrows() =>
        Invoking(() => new BlankNodeMapper(null)).Should().Throw<ArgumentNullException>();

    [Fact]
    public void EmptyPrefixThrows() =>
        Invoking(() => new BlankNodeMapper(string.Empty)).Should().Throw<ArgumentException>();

    [Fact]
    public void WhitespacePrefixThrows() =>
        Invoking(() => new BlankNodeMapper(" ")).Should().Throw<ArgumentException>();
}
