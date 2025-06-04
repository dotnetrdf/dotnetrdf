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
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Query;


public class SequenceTests
{
    private readonly SparqlQueryParser _queryParser = new SparqlQueryParser();
    private readonly SparqlUpdateParser _updateParser = new SparqlUpdateParser();

    [Fact]
    public void SparqlSequenceUpdateThenQuery1()
    {
        var dataset = new InMemoryDataset();
        var updateProcessor = new LeviathanUpdateProcessor(dataset);
        var queryProcessor = new LeviathanQueryProcessor(dataset);
        Assert.Single(dataset.Graphs);

        SparqlUpdateCommandSet updates = _updateParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_default_graph.ru"));
        updateProcessor.ProcessCommandSet(updates);

        Assert.Equal(3, dataset.Graphs.Count());
        Assert.Single(dataset[new UriNode(UriFactory.Root.Create("http://example.org/protocol-update-dataset-test/"))].Triples);

        SparqlQuery query = _queryParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_default_graph.rq"));

        ISparqlAlgebra algebra = query.ToAlgebra();

        var results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.True(results.Result);
    }

    [Fact]
    public void SparqlSequenceUpdateThenQuery2()
    {
        var dataset = new InMemoryDataset();
        var updateProcessor = new LeviathanUpdateProcessor(dataset);
        var queryProcessor = new LeviathanQueryProcessor(dataset);
        Assert.Single(dataset.Graphs);

        SparqlUpdateCommandSet updates = _updateParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_default_graphs.ru"));
        updateProcessor.ProcessCommandSet(updates);

        Assert.Equal(5, dataset.Graphs.Count());
        Assert.Equal(2, dataset[new UriNode(UriFactory.Root.Create("http://example.org/protocol-update-dataset-graphs-test/"))].Triples.Count());

        SparqlQuery query = _queryParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_default_graphs.rq"));

        var results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.True(results.Result);
    }

    [Fact]
    public void SparqlSequenceUpdateThenQuery3()
    {
        var dataset = new InMemoryDataset();
        var updateProcessor = new LeviathanUpdateProcessor(dataset);
        var queryProcessor = new LeviathanQueryProcessor(dataset);
        Assert.Single(dataset.Graphs);

        SparqlUpdateCommandSet updates = _updateParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_named_graphs.ru"));
        updateProcessor.ProcessCommandSet(updates);

        Assert.Equal(5, dataset.Graphs.Count());
        Assert.Equal(2, dataset[new UriNode(UriFactory.Root.Create("http://example.org/protocol-update-dataset-named-graphs-test/"))].Triples.Count());

        SparqlQuery query = _queryParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_named_graphs.rq"));

        ISparqlAlgebra algebra = query.ToAlgebra();
        Console.WriteLine(algebra.ToString());

        var results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.True(results.Result);
    }

    [Fact]
    public void SparqlSequenceUpdateThenQuery4()
    {
        var dataset = new InMemoryDataset();
        var updateProcessor = new LeviathanUpdateProcessor(dataset);
        var queryProcessor = new LeviathanQueryProcessor(dataset);
        Assert.Single(dataset.Graphs);

        SparqlUpdateCommandSet updates = _updateParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_full.ru"));
        updateProcessor.ProcessCommandSet(updates);

        Console.WriteLine(updates.ToString());

        Assert.Equal(5, dataset.Graphs.Count());
        Assert.Equal(2, dataset[new UriNode(UriFactory.Root.Create("http://example.org/protocol-update-dataset-full-test/"))].Triples.Count());

        SparqlQuery query = _queryParser.ParseFromFile(
            Path.Combine("resources", "sparql", "protocol", "update_dataset_full.rq"));

        ISparqlAlgebra algebra = query.ToAlgebra();
        Console.WriteLine(algebra.ToString());

        var results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
        Assert.True(results.Result);
    }
}
