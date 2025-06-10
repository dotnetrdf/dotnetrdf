using FluentAssertions;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.Storage;

[Collection("AllegroGraph Test Collection")]
public class SparqlViewTests
{
    [Fact]
    public void SparqlViewNativeAllegroGraph()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        var store = new PersistentTripleStore(agraph);

        //Load a Graph into the Store to ensure there is some data for the view to retrieve
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources","InferenceTest.ttl"));
        agraph.SaveGraph(g);

        //Create the SPARQL View
        var view = new NativeSparqlView("CONSTRUCT { ?s ?p ?o } WHERE { GRAPH ?g { ?s ?p ?o . FILTER(IsLiteral(?o)) } }", store);
        view.Triples.Should().NotBeEmpty();
    }
}
