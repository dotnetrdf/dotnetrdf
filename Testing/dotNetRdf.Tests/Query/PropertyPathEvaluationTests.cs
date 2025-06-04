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

using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query;


public class PropertyPathEvaluationTests
{
    private readonly NodeFactory _factory = new NodeFactory();
    private ISparqlDataset _data;
    private LeviathanQueryProcessor _processor;
    private readonly ITestOutputHelper _output;
    public PropertyPathEvaluationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    private ISparqlAlgebra GetAlgebra(ISparqlPath path)
    {
        return GetAlgebra(path, null, null);
    }

    private ISparqlAlgebra GetAlgebra(ISparqlPath path, INode start, INode end)
    {
        PatternItem x, y;
        if (start == null)
        {
            x = new VariablePattern("?x");
        }
        else
        {
            x = new NodeMatchPattern(start);
        }
        if (end == null)
        {
            y = new VariablePattern("?y");
        }
        else
        {
            y = new NodeMatchPattern(end);
        }
        var context = new PathTransformContext(x, y);
        return path.ToAlgebra(context);
    }

    private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path)
    {
        return GetAlgebraUntransformed(path, null, null);
    }

    private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path, INode start, INode end)
    {
        PatternItem x, y;
        if (start == null)
        {
            x = new VariablePattern("?x");
        }
        else
        {
            x = new NodeMatchPattern(start);
        }
        if (end == null)
        {
            y = new VariablePattern("?y");
        }
        else
        {
            y = new NodeMatchPattern(end);
        }
        return new Bgp(new PropertyPathPattern(x, path, y));
    }

    private void EnsureTestData()
    {
        if (_data == null)
        {
            var store = new TripleStore();
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);
            _data = new InMemoryDataset(store, g.Name);
            _processor = new LeviathanQueryProcessor(_data);
        }
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroLength()
    {
        EnsureTestData();

        var path =
            new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
        var algebra = GetAlgebra(path);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroLengthWithTermEnd()
    {
        EnsureTestData();

        var path =
            new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
        INode rdfsClass = _factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
        var algebra = GetAlgebra(path, null, rdfsClass);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
        Assert.Equal(1, results.Count);
        Assert.Equal(rdfsClass, results[1]["x"]);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroLengthWithTermStart()
    {
        EnsureTestData();

        var path =
            new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
        var algebra = GetAlgebra(path,
            new Graph().CreateUriNode(
                UriFactory.Root.Create(ConfigurationLoader.ClassHttpHandler)), null);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroLengthWithBothTerms()
    {
        EnsureTestData();

        var path =
            new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
        var algebra = GetAlgebra(path,
            new Graph().CreateUriNode(
                UriFactory.Root.Create(ConfigurationLoader.ClassHttpHandler)),
            _factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")));
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.True(results.IsEmpty, "Results should  be empty");
        Assert.True(results is NullMultiset, "Results should be Null");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationNegatedPropertySet()
    {
        EnsureTestData();

        var path =
            new NegatedSet(
                new Property[] {new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))},
                Enumerable.Empty<Property>());
        var algebra = GetAlgebra(path);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationInverseNegatedPropertySet()
    {
        EnsureTestData();

        var path = new NegatedSet(Enumerable.Empty<Property>(),
                                         new Property[]
                                             {
                                                 new Property(
                                             _factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))
                                             });
        var algebra = GetAlgebra(path);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationSequencedAlternatives()
    {
        EnsureTestData();

        INode a = _factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
        INode b = _factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
        var path = new SequencePath(new AlternativePath(new Property(a), new Property(b)),
                                             new AlternativePath(new Property(a), new Property(a)));
        var algebra = GetAlgebraUntransformed(path);
        var context = new SparqlEvaluationContext(null, _data, new LeviathanQueryOptions());
        var results = algebra.Accept(_processor, context);

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationOneOrMorePath()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new OneOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        var algebra = GetAlgebra(path);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationOneOrMorePathForward()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new OneOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        INode sportsCar = _factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
        var algebra = GetAlgebra(path, sportsCar, null);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationOneOrMorePathReverse()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new OneOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        INode airVehicle = _factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
        var algebra = GetAlgebra(path, null, airVehicle);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroOrMorePath()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new ZeroOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        var algebra = GetAlgebra(path);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroOrMorePathForward()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new ZeroOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        INode sportsCar = _factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
        var algebra = GetAlgebra(path, sportsCar, null);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationZeroOrMorePathReverse()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);
        var dataset = new InMemoryDataset(store);
        _processor = new LeviathanQueryProcessor(dataset);

        var path =
            new ZeroOrMore(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        INode airVehicle = _factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
        var algebra = GetAlgebra(path, null, airVehicle);
        var results = algebra.Accept(_processor, new SparqlEvaluationContext(null, dataset, new LeviathanQueryOptions()));

        TestTools.ShowMultiset(results);

        Assert.False(results.IsEmpty, "Results should not be empty");
    }

    [Fact]
    public void SparqlPropertyPathEvaluationGraphInteraction()
    {
        var query = @"PREFIX ex: <http://www.example.org/schema#>
PREFIX in: <http://www.example.org/instance#>

SELECT ?x
FROM NAMED <http://example/1>
FROM NAMED <http://example/2>
WHERE
{
  GRAPH ?g { in:a ex:p1 / ex:p2 ?x . }
}";

        var data =
            @"<http://www.example.org/instance#a> <http://www.example.org/schema#p1> <http://www.example.org/instance#b> <http://example/1> .
<http://www.example.org/instance#b> <http://www.example.org/schema#p2> <http://www.example.org/instance#c> <http://example/2> .";

        var store = new TripleStore();
        store.LoadFromString(data, new NQuadsParser());
        var queryParser = new SparqlQueryParser();
        var q = queryParser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(store);

        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        Assert.Empty(results.Results);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void SparqlPropertyPathEvaluationDuplicates()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "schema-org.ttl"));

        var parser = new SparqlQueryParser();
        var q = parser.ParseFromFile(Path.Combine("resources", "schema-org.rq"));
        var qDistinct = parser.ParseFromFile(Path.Combine("resources", "schema-org.rq"));
        qDistinct.QueryType = SparqlQueryType.SelectDistinct;

        var dataset = new InMemoryDataset(g);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
        var resultsDistinct = processor.ProcessQuery(qDistinct) as SparqlResultSet;
        Assert.NotNull(resultsDistinct);
        Assert.False(resultsDistinct.IsEmpty);

        Assert.Equal(resultsDistinct.Count, results.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore349RigorousEvaluation()
    {
        //Test case from CORE-349
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-349.ttl"));
        var dataset = new InMemoryDataset(g);

        var query = @"SELECT * WHERE 
{ 
  ?subject <http://www.w3.org/2000/01/rdf-schema#label> ?name .
  ?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/<http://www.w3.org/2000/01/rdf-schema#subClassOf>* <http://example.org/unnamed#Level1_1> . 
?subject a ?type . } ";

        var q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset,
            options => { options.RigorousEvaluation = true;});
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);

        TestTools.ShowResults(results, _output);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore349NonRigorousEvaluation()
    {
        //Test case from CORE-349
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-349.ttl"));
        var dataset = new InMemoryDataset(g);

        var query = @"SELECT * WHERE 
{ 
  ?subject <http://www.w3.org/2000/01/rdf-schema#label> ?name .
  ?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/<http://www.w3.org/2000/01/rdf-schema#subClassOf>* <http://example.org/unnamed#Level1_1> . 
?subject a ?type . } ";

        var q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset,
            options => { options.RigorousEvaluation = false; }
        );
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationNonRigorous()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        var dataset = new InMemoryDataset(g);

        var query = "SELECT * WHERE { ?subClass <http://www.w3.org/2000/01/rdf-schema#subClassOf>* ?class }";

        var q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset, options=>options.RigorousEvaluation=true);
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.Equal(73, results.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore395ExactQuery()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "pp.rdf"));

        var dataset = new InMemoryDataset(g);
        var query = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "pp.rq"));
        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(query);
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.IsType<SparqlResultSet>(results);
        var rset = (SparqlResultSet) results;
        Assert.Equal(3, rset.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore395ListQuery()
    {
        IGraph g = new Graph();
        g.LoadFromFile(@"resources/pp.rdf");

        var dataset = new InMemoryDataset(g);
        var query = new SparqlQueryParser().ParseFromString(@"
prefix rdfs:  <http://www.w3.org/2000/01/rdf-schema#> 
prefix owl:   <http://www.w3.org/2002/07/owl#> 
prefix rdf:   <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 

select ?superclass where {
  ?s owl:intersectionOf/rdf:rest*/rdf:first ?superclass .
  filter(!isBlank(?superclass))
}
");
        var processor = new LeviathanQueryProcessor(dataset, options=>options.QueryExecutionTimeout = -1);
        var results = processor.ProcessQuery(query);
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.IsType<SparqlResultSet>(results);
        var rset = (SparqlResultSet)results;
        Assert.Equal(2, rset.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore441ZeroOrMorePath()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-441", "data.ttl"));

        var dataset = new InMemoryDataset(g);
        var query = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-441", "star-path.rq"));

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(query);
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.IsType<SparqlResultSet>(results);
        var rset = (SparqlResultSet)results;
        Assert.Equal(1, rset.Count);
        Assert.Equal(g.CreateUriNode("Frame:Sheep"), rset[0]["prey"]);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore441OneOrMorePath()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-441", "data.ttl"));

        var dataset = new InMemoryDataset(g);
        var query = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-441", "plus-path.rq"));

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(query);
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.IsType<SparqlResultSet>(results);
        var rset = (SparqlResultSet)results;
        Assert.Equal(0, rset.Count);
    }

    [Fact]
    public void SparqlPropertyPathEvaluationCore441NoPath()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-441", "data.ttl"));

        var dataset = new InMemoryDataset(g);
        var query = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-441", "no-path.rq"));

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(query);
        Assert.NotNull(results);
        TestTools.ShowResults(results, _output);

        Assert.IsType<SparqlResultSet>(results);
        var rset = (SparqlResultSet)results;
        Assert.Equal(1, rset.Count);
        Assert.Equal(g.CreateUriNode("Frame:Sheep"), rset[0]["prey"]);
    }

    [Fact]
    public void Issue571PropertyPathEvaluation()
    {
        IGraph graph = new Graph();
        graph.LoadFromString("""
                             @prefix : <http://example.org/> .
                             @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .
                             :age rdfs:domain :Person .
                             """);
        var results = graph.ExecuteQuery("""
                             PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                           
                             SELECT *
                                 WHERE {
                                 ?property rdfs:domain ?class .
                                 ?sub rdfs:subClassOf* ?class .
                             }
                           """) as SparqlResultSet;
        results.Count.Should().Be(1);
        var results2 = graph.ExecuteQuery("""
                                           PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                         
                                           SELECT *
                                               WHERE {
                                               ?property rdfs:domain ?class .
                                               ?c rdfs:subClassOf* ?class .
                                           }
                                         """) as SparqlResultSet;
        results2.Count.Should().Be(1);
    }
}
