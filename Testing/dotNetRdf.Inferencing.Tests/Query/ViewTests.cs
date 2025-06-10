using System;
using System.IO;
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using Xunit;

namespace VDS.RDF.Query;

public class ViewTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ViewTests(ITestOutputHelper helper)
    {
        _testOutputHelper = helper;
    }

    [Fact]
    public void SparqlViewAndReasonerInteraction1()
    {
        var store = new InferencingTripleStore();
        var view = new SparqlView("CONSTRUCT { ?s a ?type } WHERE { GRAPH ?g { ?s a ?type } }", store,
            new UriNode(new Uri("http://example.org/view")));
        store.Add(view);

        _testOutputHelper.WriteLine("SPARQL View Empty");
        TestTools.ShowGraph(view, _testOutputHelper);
        _testOutputHelper.WriteLine();

        //Load a Graph into the Store to cause the SPARQL View to update
        var g = new Graph(new UriNode(new Uri("http://example.org/data")));
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        Thread.Sleep(500);
        if (view.Triples.Count == 0) view.UpdateView();

        _testOutputHelper.WriteLine("SPARQL View Populated");
        TestTools.ShowGraph(view, _testOutputHelper);
        _testOutputHelper.WriteLine();

        Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
        var lastCount = view.Triples.Count;

        //Apply an RDFS reasoner
        var reasoner = new StaticRdfsReasoner();
        reasoner.Initialise(g);
        store.AddInferenceEngine(reasoner);

        Thread.Sleep(200);
        if (view.Triples.Count == lastCount) view.UpdateView();
        _testOutputHelper.WriteLine("SPARQL View Populated after Reasoner added");
        TestTools.ShowGraph(view);
    }


}
