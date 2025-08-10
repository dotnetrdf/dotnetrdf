/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using VDS.RDF.Shacl.Validation;
using System.Linq;
using Xunit;

namespace VDS.RDF.Shacl;

public class Examples
{
    [Fact]
    public void Validation()
    {
        var dataGraph = new Graph();
        dataGraph.LoadFromString(@"
@prefix : <urn:> .

:s :p :o .
");

        var shapesGraph = new Graph();
        shapesGraph.LoadFromString(@"
@prefix : <urn:> .
@prefix sh: <http://www.w3.org/ns/shacl#> .

[
    sh:targetNode :s ;
    sh:property [
        sh:path :p ;
        sh:class :C
    ]
] .
");

        var reportGraph = new Graph();
        reportGraph.LoadFromString(@"
@prefix : <urn:> .
@prefix sh: <http://www.w3.org/ns/shacl#> .

[
    a sh:ValidationReport ;
    sh:conforms false ;
    sh:result [
        a sh:ValidationResult ;
        sh:resultMessage ""Value node must be an instance of type urn:C."" ;
        sh:sourceConstraintComponent sh:ClassConstraintComponent ;
        sh:resultSeverity sh:Violation ;
        sh:sourceShape [] ;
        sh:focusNode :s ;
        sh:resultPath :p ;
        sh:value :o
    ]
] .
");

        var processor = new ShapesGraph(shapesGraph);
        var report = processor.Validate(dataGraph);

        Assert.Equal(reportGraph, report.Graph);
    }

    [Fact]
    public void Conformance()
    {
        var dataGraph = new Graph();
        dataGraph.LoadFromString(@"
@prefix : <urn:> .

:s :p :o .
");

        var shapesGraph = new Graph();
        shapesGraph.LoadFromString(@"
@prefix : <urn:> .
@prefix sh: <http://www.w3.org/ns/shacl#> .

[
    sh:targetNode :s ;
    sh:class :C ;
] .
");

        var processor = new ShapesGraph(shapesGraph);
        var conforms = processor.Conforms(dataGraph);

        Assert.False(conforms);
    }

    [Fact]
    public void Consume_validation_results()
    {
        var dataGraph = new Graph();
        dataGraph.LoadFromString(@"
@prefix : <urn:> .

:s :p :o .
");

        var shapesGraph = new Graph();
        shapesGraph.LoadFromString(@"
@prefix : <urn:> .
@prefix sh: <http://www.w3.org/ns/shacl#> .

[
    sh:targetNode :s ;
    sh:property [
        sh:path :p ;
        sh:class :C ;
        sh:message ""test message"" ;
    ]
] .
");

        var processor = new ShapesGraph(shapesGraph);
        Report report = processor.Validate(dataGraph);

        Assert.Single(report.Results);
        
        Result result = report.Results.Single();
        Assert.Equal("test message", result.Message.Value);
    }
}
