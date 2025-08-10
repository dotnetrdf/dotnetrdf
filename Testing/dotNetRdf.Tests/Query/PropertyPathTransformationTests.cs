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
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;



public class PropertyPathTransformationTests
{
    private readonly NodeFactory _factory = new NodeFactory();
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    private void RunTest(ISparqlPath path, IEnumerable<String> expectedOperators)
    {
        var x = new VariablePattern("?x");
        var y = new VariablePattern("?y");
        var context = new PathTransformContext(x, y);

        Console.WriteLine("Path: " + path.ToString());

        ISparqlAlgebra algebra = path.ToAlgebra(context);
        var result = algebra.ToString();
        Console.WriteLine("Algebra: " + result);

        try
        {
            var gp = algebra.ToGraphPattern();
            Console.WriteLine("GraphPattern:");
            Console.WriteLine(_formatter.Format(gp));
            Console.WriteLine();
        }
        catch
        {
            Console.WriteLine("Algebra not translatable to a GraphPattern");
        }

        foreach (var op in expectedOperators)
        {
            if (result.Contains(op)) continue;
            Console.WriteLine("Expected Operator '" + op + "' missing");
            Assert.Fail("Expected Operator '" + op + "' missing");
        }
    }

    [Fact]
    public void SparqlPropertyPathTransformationProperty()
    {
        RunTest(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationInverse()
    {
        RunTest(new InversePath(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))), new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationSequence()
    {
        var path = new SequencePath(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationAlternative()
    {
        var path = new AlternativePath(new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"))), new Property(_factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "comment"))));
        RunTest(path, new String[] { "BGP", "Union" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationOptional()
    {
        var path = new ZeroOrOne(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
        RunTest(path, new String[] { "BGP", "ZeroLengthPath" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationZeroOrMore()
    {
        var path = new ZeroOrMore(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
        RunTest(path, new String[] { "ZeroOrMorePath" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationOneOrMore()
    {
        var path = new OneOrMore(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
        RunTest(path, new String[] { "OneOrMorePath" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationFixed1()
    {
        var path = new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationFixed2()
    {
        var path = new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 2);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationFixed10()
    {
        var path = new FixedCardinality(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 10);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationVariable1To2()
    {
        var path = new NToM(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 2);
        RunTest(path, new String[] { "BGP", "Union" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationVariable1To10()
    {
        var path = new NToM(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 10);
        RunTest(path, new String[] { "BGP", "Union" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationVariable1To1()
    {
        var path = new NToM(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 1);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationVariable3To7()
    {
        var path = new NToM(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 3, 7);
        RunTest(path, new String[] { "BGP", "Union" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationNOrMore1()
    {
        var path = new NOrMore(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationNOrMore0()
    {
        var path = new NOrMore(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationZeroToN1()
    {
        var path = new ZeroToN(new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
        RunTest(path, new String[] { "BGP" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationNegatedPropertySet()
    {
        var path = new NegatedSet(new Property[] { new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) }, Enumerable.Empty<Property>());
        RunTest(path, new String[] { "NegatedPropertySet" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationNegatedPropertyInverseSet()
    {
        var path = new NegatedSet(Enumerable.Empty<Property>(), new Property[] { new Property(_factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) });
        RunTest(path, new String[] { "NegatedPropertySet" });
    }

    [Fact]
    public void SparqlPropertyPathTransformationSequencedAlternatives()
    {
        INode a = _factory.CreateUriNode(new Uri("ex:a"));
        INode b = _factory.CreateUriNode(new Uri("ex:b"));
        INode c = _factory.CreateUriNode(new Uri("ex:c"));
        INode d = _factory.CreateUriNode(new Uri("ex:d"));
        var path = new SequencePath(new AlternativePath(new Property(a), new Property(c)), new AlternativePath(new Property(b), new Property(d)));
        RunTest(path, new String[] { "BGP" });
    }
}
