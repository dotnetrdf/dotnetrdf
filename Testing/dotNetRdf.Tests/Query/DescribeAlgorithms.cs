/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;
using VDS.RDF.Utils.Describe;

namespace VDS.RDF.Query;


public class DescribeAlgorithms : IDisposable
{
    private const String DescribeQuery = "PREFIX foaf: <http://xmlns.com/foaf/0.1/> DESCRIBE ?x WHERE {?x foaf:name \"Dave\" }";

    private SparqlQueryParser _parser;
    private InMemoryDataset _data;
    private LeviathanQueryProcessor _processor;

    public DescribeAlgorithms()
    {
        _parser = new SparqlQueryParser();
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "describe-algos.ttl"));
        store.Add(g);
        _data = new InMemoryDataset(store);
        _processor = new LeviathanQueryProcessor(_data);
    }

    public void Dispose()
    {
        _parser = null;
        _data = null;
    }

    private SparqlQuery GetQuery()
    {
        return _parser.ParseFromString(DescribeQuery);
    }

    [Theory]
    [InlineData(typeof(ConciseBoundedDescription))]
    [InlineData(typeof(SymmetricConciseBoundedDescription))]
    [InlineData(typeof(SimpleSubjectDescription))]
    [InlineData(typeof(SimpleSubjectObjectDescription))]
    [InlineData(typeof(MinimalSpanningGraph))]
    [InlineData(typeof(LabelledDescription))]
    public void SparqlDescribeAlgorithms(Type describerType)
    {
        SparqlQuery q = GetQuery();
        var processor = new LeviathanQueryProcessor(_data,
            options => options.Describer =
                new SparqlDescriber((IDescribeAlgorithm)Activator.CreateInstance(describerType)));
        var results = processor.ProcessQuery(q);
        Assert.IsType<Graph>(results, exactMatch: false);
        if (results is Graph)
        {
            TestTools.ShowResults(results);
        }
    }

    [Fact]
    public void SparqlDescribeDefaultGraphHandling1()
    {
        var dataset = new InMemoryDataset();

        IGraph g = new Graph(new UriNode(new Uri("http://graph")));
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        dataset.AddGraph(g);

        var processor = new LeviathanQueryProcessor(dataset);
        var description = processor.ProcessQuery(_parser.ParseFromString("DESCRIBE ?s FROM <http://graph> WHERE { ?s ?p ?o }")) as IGraph;
        Assert.NotNull(description);
        Assert.False(description.IsEmpty);
    }

    [Fact]
    public void SparqlDescribeDefaultGraphHandling2()
    {
        var dataset = new InMemoryDataset();

        IGraph g = new Graph(new UriNode(UriFactory.Root.Create("http://graph")));
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        dataset.AddGraph(g);

        var processor = new LeviathanQueryProcessor(dataset);
        var description = processor.ProcessQuery(_parser.ParseFromString("DESCRIBE ?s WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
        Assert.NotNull(description);
        Assert.True(description.IsEmpty);
    }

    [Fact]
    public void SparqlDescribeDefaultGraphHandling3()
    {
        var dataset = new InMemoryDataset();

        IGraph g = new Graph(new UriNode(UriFactory.Root.Create("http://graph")));
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        dataset.AddGraph(g);

        var processor = new LeviathanQueryProcessor(dataset);
        var description = processor.ProcessQuery(_parser.ParseFromString("DESCRIBE ?s FROM <http://graph> WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
        Assert.NotNull(description);
        Assert.True(description.IsEmpty);
    }

    [Fact]
    public void SparqlDescribeDefaultGraphHandling4()
    {
        var dataset = new InMemoryDataset();

        IGraph g = new Graph(new UriNode(UriFactory.Root.Create("http://graph")));
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        dataset.AddGraph(g);

        var processor = new LeviathanQueryProcessor(dataset);
        var description = processor.ProcessQuery(_parser.ParseFromString("DESCRIBE ?s FROM <http://graph> FROM NAMED <http://graph> WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
        Assert.NotNull(description);
        Assert.False(description.IsEmpty);
    }
}
