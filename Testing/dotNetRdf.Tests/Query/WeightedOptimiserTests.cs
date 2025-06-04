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
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class WeightedOptimiserTests
{
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    [Fact]
    public void SparqlOptimiserQueryWeightedSimple()
    {
        const string query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);
        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.False(q.RootGraphPattern.TriplePatterns[0].IsAcceptAll, "First Triple Pattern should not be the ?s ?p ?o Pattern");
        Assert.True(q.RootGraphPattern.TriplePatterns[1].IsAcceptAll, "Second Triple Pattern should be the ?s ?p ?o pattern");
    }

    [Fact]
    public void SparqlOptimiserQueryWeightedSimple2()
    {
        const string query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s rdfs:comment ?comment .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);

        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.True(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("comment"), "First Triple Pattern should contain ?comment");
        Assert.True(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("label"), "Second Triple Pattern should contain ?label");
    }

    [Fact]
    public void SparqlOptimiserQueryWeightedSimple3()
    {
        const string query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s a ?type .
  ?s rdfs:label ?label .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);

        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.True(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("label"), "First Triple Pattern should contain ?label");
        Assert.True(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("type"), "Second Triple Pattern should contain ?type");
    }

    [Fact]
    public void SparqlOptimiserQueryWeightedSimple4()
    {
        const string query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s a rdfs:Class .
  ?s rdfs:label 'example' .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);

        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.True(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[0]).Object).Node.NodeType == NodeType.Literal, "First Triple Pattern should have object 'example'");
        Assert.True(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[1]).Object).Node.NodeType == NodeType.Uri, "Second Triple Pattern should have object rdfs:Class");
    }

    [Fact]
    public void SparqlOptimiserQueryWeightedUnknowns()
    {
        const string query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:comment 'Predicates are weighted less than subjects' .
  rdfs:comment rdfs:comment 'Subjects are weighted higher than predicates' .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);

        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.False(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("s"), "First Triple Pattern should not contain ?s");
        Assert.True(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("s"), "Second Triple Pattern should contain ?s");
    }

    [Fact]
    public void SparqlOptimiserQueryWeightedUnknowns2()
    {
        var query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s <http://weightings/PredicatesLast> ?o .
  <http://weightings/SubjectsFirst> ?p ?o .
  ?s ?p 'Objects In Middle' .
}";

        var weightings = new Graph();
        weightings.LoadFromEmbeddedResource("VDS.RDF.Query.SampleWeightings.n3, dotNetRDF.Test");
        _parser.QueryOptimiser = new WeightedOptimiser(weightings);

        SparqlQuery q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.True(q.RootGraphPattern.TriplePatterns[0].Variables.Intersect(new String[] { "p", "o" }).Count() == 2, "First Triple Pattern should contain ?p and ?o");
        Assert.True(q.RootGraphPattern.TriplePatterns[1].Variables.Intersect(new String[] { "s", "p" }).Count() == 2, "Second Triple Pattern should contain ?s and ?p");
        Assert.True(q.RootGraphPattern.TriplePatterns[2].Variables.Intersect(new String[] { "s", "o" }).Count() == 2, "Second Triple Pattern should contain ?s and ?o");
    }
}
