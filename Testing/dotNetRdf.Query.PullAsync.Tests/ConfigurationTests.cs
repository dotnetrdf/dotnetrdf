using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Configuration;

namespace dotNetRdf.Query.Pull.Tests;

public class ConfigurationTests
{
    public ConfigurationTests()
    {
        ConfigurationLoader.AddObjectFactory(new PullQueryProcessorConfigurationFactory());
    }

    [Fact]
    public void ItLoadsConfigurationWithNoOptions()
    {
        var configGraph = new Graph();
        configGraph.LoadFromString(
            """
            @prefix : <http://example.org/> .
            @prefix dnr: <http://www.dotnetrdf.org/configuration#> .
            :queryProcessor dnr:type "VDS.RDF.Query.Pull.PullQueryProcessor" ;
              dnr:usingStore [
                dnr:type "VDS.RDF.TripleStore" ;
              ] ;
            .
            """);
        INode? configNode = configGraph.GetUriNode(UriFactory.Create("http://example.org/queryProcessor"));
        Assert.NotNull(configNode);
        var loadedObject = ConfigurationLoader.LoadObject(configGraph, configNode);
        Assert.NotNull(loadedObject);
        Assert.IsType<PullQueryProcessor>(loadedObject);
    }

    [Fact]
    public void ItLoadsConfigurationWithOptions()
    {
        var configGraph = new Graph();
        configGraph.LoadFromString(
            """
            @prefix : <http://example.org/> .
            @prefix dnr: <http://www.dotnetrdf.org/configuration#> .
            :queryProcessor a dnr:SparqlQueryProcessor ;
              dnr:type "VDS.RDF.Query.Pull.PullQueryProcessor" ;
              dnr:usingStore [
                dnr:type "VDS.RDF.TripleStore" ;
              ] ;
              dnr:timeout 60000 ;
              dnr:unionDefaultGraph true ;
            .
            """);
        INode? configNode = configGraph.GetUriNode(UriFactory.Create("http://example.org/queryProcessor"));
        Assert.NotNull(configNode);
        var loadedObject = ConfigurationLoader.LoadObject(configGraph, configNode);
        Assert.NotNull(loadedObject);
        PullQueryProcessor processor = Assert.IsType<PullQueryProcessor>(loadedObject);
        Assert.True(processor.UnionDefaultGraph);
        Assert.Equal(60000ul, processor.QueryExecutionTimeout);
    }

    [Fact]
    public void ItFailsToLoadIfThereIsNoConfiguredStoreOption()
    {
        var configGraph = new Graph();
        configGraph.LoadFromString(
            """
            @prefix : <http://example.org/> .
            @prefix dnr: <http://www.dotnetrdf.org/configuration#> .
            :queryProcessor a dnr:SparqlQueryProcessor ;
              dnr:type "VDS.RDF.Query.Pull.PullQueryProcessor" ;
              dnr:timeout 60000 ;
              dnr:unionDefaultGraph true ;
            .
            """);
        INode? configNode = configGraph.GetUriNode(UriFactory.Create("http://example.org/queryProcessor"));
        Assert.NotNull(configNode);
        Assert.Throws<DotNetRdfConfigurationException>(() => ConfigurationLoader.LoadObject(configGraph, configNode));
    }
}