using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using Xunit;

namespace VDS.RDF.Ontology;

public class OntologyReasoningTests
{
    private readonly ITestOutputHelper _output;
    public OntologyReasoningTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OntologyReasonerGraph()
    {
        //Load Test Data
        _output.WriteLine("Loading in the standard test data InferenceTest.ttl");
        var g = new OntologyGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        OntologyClass c = g.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
        _output.WriteLine("Things which are cars in an Ontology Graph");
        foreach (OntologyResource r in c.Instances)
        {
            _output.WriteLine(r.Resource.ToString());
        }
        _output.WriteLine(string.Empty);

        var g2 = new ReasonerGraph(g, new RdfsReasoner());
        OntologyClass c2 = g2.CreateOntologyClass(new Uri("http://example.org/vehicles/Car"));
        _output.WriteLine("Things which are cars in a Reasoner Graph using an RDFS Reasoner");
        foreach (OntologyResource r in c2.Instances)
        {
            _output.WriteLine(r.Resource.ToString());
        }
        _output.WriteLine(string.Empty);

        _output.WriteLine("Original Graph has " + g.Triples.Count + " Triples");
        _output.WriteLine("Reasoner Graph has " + g2.Triples.Count + " Triples");
        Assert.True(g2.Triples.Count > g.Triples.Count, "Reasoner Graph should have more Triples");
        Assert.Equal(g, g2.BaseGraph);

        _output.WriteLine(string.Empty);

        _output.WriteLine("Going to do a GetTriplesWithSubject() call on both Graphs to see if ReasonerGraph behaves as expected");
        IUriNode fiesta = g.CreateUriNode(new Uri("http://example.org/vehicles/FordFiesta"));
        _output.WriteLine("Original Graph:");
        foreach (Triple t in g.GetTriplesWithSubject(fiesta))
        {
            _output.WriteLine(t.ToString());
        }
        _output.WriteLine(string.Empty);
        _output.WriteLine("Reasoner Graph:");
        foreach (Triple t in g2.GetTriplesWithSubject(fiesta))
        {
            _output.WriteLine(t.ToString());
        }
    }

}
