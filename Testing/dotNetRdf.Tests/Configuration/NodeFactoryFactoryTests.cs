using FluentAssertions;
using System;
using Xunit;

namespace VDS.RDF.Configuration;

public class NodeFactoryFactoryTests
{
    [Fact]
    public void LoadBasicNodeFactory()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.NodeFactory"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as INodeFactory;
        collection.Should().NotBeNull().And.BeAssignableTo<NodeFactory>();
    }
    
    [Fact]
    public void ConfigureBaseUri()
    {
        var graph = ConfigLookupTests.Prefixes + 
                        """
                         _:a a dnr:NodeFactory ;
                           dnr:type "VDS.RDF.NodeFactory" ;
                           dnr:assignUri <http://example.org/> .
                         """;
        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as INodeFactory;
        collection.Should().NotBeNull()
            .And.BeAssignableTo<NodeFactory>()
            .Which.BaseUri.Should().Be(new Uri("http://example.org/"));
    }

    [Fact]
    public void ConfigureLiteralNormalization()
    {
        var graph = ConfigLookupTests.Prefixes + 
                    """
                    _:a a dnr:NodeFactory ;
                      dnr:type "VDS.RDF.NodeFactory" ;
                      dnr:normalizeLiterals true .
                    """;
        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as INodeFactory;
        collection.Should().NotBeNull()
            .And.BeAssignableTo<NodeFactory>()
            .Which.NormalizeLiteralValues.Should().Be(true);
    }

    [Theory]
    [InlineData("none", LanguageTagValidationMode.None)]
    [InlineData("false", LanguageTagValidationMode.None)]
    [InlineData("true", LanguageTagValidationMode.Turtle)]
    [InlineData("turtle", LanguageTagValidationMode.Turtle)]
    [InlineData("wellformed", LanguageTagValidationMode.WellFormed)]
    [InlineData("bcp47", LanguageTagValidationMode.WellFormed)]
    public void ConfigureLanguageTagValidation(string configValue, LanguageTagValidationMode expectedMode)
    {
        var graph = $@"{ConfigLookupTests.Prefixes}
                    _:a a dnr:NodeFactory ;
                      dnr:type ""VDS.RDF.NodeFactory"" ;
                      dnr:withLanguageTagValidation ""{configValue}"" .
                    ";
        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as INodeFactory;
        collection.Should().NotBeNull()
            .And.BeAssignableTo<NodeFactory>()
            .Which.LanguageTagValidation.Should().Be(expectedMode);
    }

    [Fact]
    public void ConfigureUriFactory()
    {
        var graph = $@"{ConfigLookupTests.Prefixes}
                    _:a a dnr:NodeFactory ;
                      dnr:type ""VDS.RDF.NodeFactory"" ;
                      dnr:usingUriFactory _:b .
                    _:b a dnr:UriFactory ;
                        dnr:type ""VDS.RDF.CachingUriFactory"" .
                    ";
        var g = new Graph();
        g.LoadFromString(graph);

        var nodeFactory = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as INodeFactory;
        var uriFactory = ConfigurationLoader.LoadObject(g, g.GetBlankNode("b")) as IUriFactory;
        nodeFactory.Should().NotBeNull()
            .And.BeAssignableTo<NodeFactory>()
            .Which.UriFactory.Should().BeSameAs(uriFactory);
    }

}