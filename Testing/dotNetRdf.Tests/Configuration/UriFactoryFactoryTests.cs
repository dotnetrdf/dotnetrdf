using FluentAssertions;
using Xunit;

namespace VDS.RDF.Configuration;

public class UriFactoryFactoryTests
{
    [Fact]
    public void LoadBasicUriFactory()
    {
        const string graph = $@"{ConfigLookupTests.Prefixes}
_:a a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.CachingUriFactory"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IUriFactory;
        collection.Should().NotBeNull().And.BeAssignableTo<CachingUriFactory>().Which.InternUris.Should().BeTrue();
    }
    
    [Fact]
    public void DisableInterning()
    {
        const string graph = $@"{ConfigLookupTests.Prefixes}
_:a a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.CachingUriFactory"" ;
  dnr:internUris false .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IUriFactory;
        collection.Should().NotBeNull().And.BeAssignableTo<CachingUriFactory>().Which.InternUris.Should().BeFalse();
    }

    [Fact]
    public void ConfigureParentUriFactory()
    {
        const string graph = $@"{ConfigLookupTests.Prefixes}
_:a a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.CachingUriFactory"" .

_:b a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.CachingUriFactory"" ;
  dnr:withParent _:a .
";

        var g = new Graph();
        g.LoadFromString(graph);

        var factoryA = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IUriFactory;
        factoryA.Should().NotBeNull().And.BeAssignableTo<CachingUriFactory>();
        var factoryB = ConfigurationLoader.LoadObject(g, g.GetBlankNode("b")) as IUriFactory;
        factoryB.Should().NotBeNull().And.BeAssignableTo<CachingUriFactory>();
        factoryA?.Create("https://example.org");
        factoryB?.TryGetUri("https://example.org", out _).Should().BeTrue();
    }
}