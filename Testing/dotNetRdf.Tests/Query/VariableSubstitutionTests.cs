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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class VariableSubstitutionTests
{
    private SparqlQueryParser _parser = new SparqlQueryParser();
    private SparqlFormatter _formatter = new SparqlFormatter(new NamespaceMapper());
    private NodeFactory _factory = new NodeFactory();

    private void TestSubstitution(SparqlQuery q, String findVar, String replaceVar, IEnumerable<String> expected, IEnumerable<String> notExpected)
    {
        Console.WriteLine("Input Query:");
        Console.WriteLine(_formatter.Format(q));
        Console.WriteLine();

        ISparqlAlgebra algebra = q.ToAlgebra();
        var transformer = new VariableSubstitutionTransformer(findVar, replaceVar);
        try
        {
            ISparqlAlgebra resAlgebra = transformer.Optimise(algebra);
            algebra = resAlgebra;
        }
        catch (Exception ex)
        {
            //Ignore errors
            Console.WriteLine("Error Transforming - " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
        }

        SparqlQuery resQuery = algebra.ToQuery();
        var resText = _formatter.Format(resQuery);
        Console.WriteLine("Resulting Query:");
        Console.WriteLine(resText);
        Console.WriteLine();

        foreach (var x in expected)
        {
            Assert.True(resText.Contains(x), "Expected Transformed Query to contain string '" + x + "'");
        }
        foreach (var x in notExpected)
        {
            Assert.False(resText.Contains(x), "Transformed Query contained string '" + x + "' which was expected to have been transformed");
        }
    }

    private void TestSubstitution(SparqlQuery q, String findVar, INode replaceTerm, IEnumerable<String> expected, IEnumerable<String> notExpected)
    {
        Console.WriteLine("Input Query:");
        Console.WriteLine(_formatter.Format(q));
        Console.WriteLine();

        ISparqlAlgebra algebra = q.ToAlgebra();
        var transformer = new VariableSubstitutionTransformer(findVar, replaceTerm);
        try
        {
            ISparqlAlgebra resAlgebra = transformer.Optimise(algebra);
            algebra = resAlgebra;
        }
        catch (Exception ex)
        {
            //Ignore errors
            Console.WriteLine("Error Transforming - " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
        }

        SparqlQuery resQuery = algebra.ToQuery();
        var resText = _formatter.Format(resQuery);
        Console.WriteLine("Resulting Query:");
        Console.WriteLine(resText);
        Console.WriteLine();

        foreach (var x in expected)
        {
            Assert.True(resText.Contains(x), "Expected Transformed Query to contain string '" + x + "'");
        }
        foreach (var x in notExpected)
        {
            Assert.False(resText.Contains(x), "Transformed Query contained string '" + x + "' which was not expected");
        }
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub1()
    {
        var query = "SELECT * WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub2()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . ?s a ?type }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub3()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . FILTER(ISBLANK(?s)) }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub4()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . BIND(ISBLANK(?s) AS ?blank) }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub5()
    {
        try
        {
            _parser.SyntaxMode = SparqlQuerySyntax.Extended;

            var query = "SELECT * WHERE { ?s ?p ?o . LET (?blank := ISBLANK(?s)) }";
            SparqlQuery q = _parser.ParseFromString(query);
            TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }
        finally
        {
            _parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
        }
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub6()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . FILTER(EXISTS { ?s a ?type }) }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub7()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . { ?s a ?type } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub8()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x", "OPTIONAL" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub9()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . GRAPH <http://example.org/graph> { ?s a ?type } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x", "GRAPH" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub10()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . MINUS { ?s a ?type } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x", "MINUS" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub11()
    {
        var query = "SELECT * WHERE { { ?s ?p ?o . } UNION { ?s a ?type } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?x", "UNION" }, new String[] { "?s" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSub12()
    {
        var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "g", "x", new String[] { "?x", "GRAPH" }, new String[] { "?g" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub1()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub2()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . ?o ?x ?y }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub3()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . FILTER(ISURI(?o)) }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub4()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . BIND(ISURI(?o) AS ?uri) }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub5()
    {
        try
        {
            _parser.SyntaxMode = SparqlQuerySyntax.Extended;

            var query = "SELECT * WHERE { ?s ?p ?o . LET (?uri := ISURI(?o))}";
            SparqlQuery q = _parser.ParseFromString(query);
            var factory = new NodeFactory();
            TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>" }, new String[] { "?o" });
        }
        finally
        {
            _parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
        }
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub6()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?o ?x ?y } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>", "OPTIONAL" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub7()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . GRAPH <http://example.org/graph> { ?o ?x ?y } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>", "GRAPH" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub8()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . MINUS { ?o ?x ?y } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var factory = new NodeFactory();
        TestSubstitution(q, "o", factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>", "MINUS" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub9()
    {
        var query = "SELECT * WHERE { { ?s ?p ?o .} UNION { ?o ?x ?y } }";
        SparqlQuery q = _parser.ParseFromString(query);

        TestSubstitution(q, "o", _factory.CreateUriNode(UriFactory.Root.Create("http://example.org/object")), new String[] { "<http://example.org/object>", "UNION" }, new String[] { "?o" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraTermSub10()
    {
        var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "g", _factory.CreateUriNode(UriFactory.Root.Create("http://example.org/graph")), new String[] { "<http://example.org/graph>", "GRAPH" }, new String[] { "?g" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSubBad1()
    {
        var query = "SELECT * WHERE { ?s <http://predicate>+ ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?s", "+" }, new String[] { "?x" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSubBad2()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?s" }, new String[] { "?x" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSubBad3()
    {
        var query = "SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "s", "x", new String[] { "?s", "SERVICE" }, new String[] { "?x" });
    }

    [Fact]
    public void SparqlOptimiserAlgebraVarSubBad4()
    {
        var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        TestSubstitution(q, "g", _factory.CreateLiteralNode("graph"), new String[] { "?g" }, new String[] { "\"graph\"" });
    }
}
