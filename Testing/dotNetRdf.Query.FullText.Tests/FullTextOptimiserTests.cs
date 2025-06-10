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

#if !NO_FULLTEXT

using System;
using System.Collections.Generic;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText;

[Trait("category", "explicit")]
[Trait("category", "fulltext")]
[Collection("FullText")]
public class FullTextOptimiserTests
{
    private readonly SparqlQueryParser _parser = new();

    private readonly List<IAlgebraOptimiser> _optimisers = new()
    {
        new StrictAlgebraOptimiser(), new FullTextOptimiser(new MockSearchProvider())
    };

    private readonly SparqlFormatter _formatter = new();

    private readonly ITestOutputHelper _output;

    public FullTextOptimiserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private SparqlQuery TestOptimisation(String query)
    {
        query = "PREFIX pf: <http://jena.hpl.hp.com/ARQ/property#>\n" + query;
        SparqlQuery q = _parser.ParseFromString(query);
        _output.WriteLine(_formatter.Format(q));

        _output.WriteLine("Normal Algebra: " + q.ToAlgebra());

        q.AlgebraOptimisers = _optimisers;

        var algebra = q.ToAlgebra().ToString();
        _output.WriteLine("Optimised Algebra: " + algebra);
        Assert.True(algebra.Contains("FullTextQuery("), "Optimised Algebra should use FullTextQuery operator");
        Assert.True(algebra.Contains("PropertyFunction("),
            "Optimised Algebra should use PropertyFunction operator");

        return q;
    }

    [Fact]
    public void FullTextOptimiserSimple1()
    {
        TestOptimisation("SELECT * WHERE { ?s pf:textMatch 'value' }");
    }

    [Fact]
    public void FullTextOptimiserSimple2()
    {
        TestOptimisation("SELECT * WHERE { ?s ?p ?o . ?s pf:textMatch 'value' }");
    }

    [Fact]
    public void FullTextOptimiserSimple3()
    {
        TestOptimisation("SELECT * WHERE { ?s pf:textMatch 'value' . FILTER(ISURI(?s)) }");
    }

    [Fact]
    public void FullTextOptimiserSimple4()
    {
        TestOptimisation("SELECT * WHERE { (?match ?score) pf:textMatch 'value' }");
    }

    [Fact]
    public void FullTextOptimiserComplex1()
    {
        TestOptimisation("SELECT * WHERE { ?s ?p ?o . ?s pf:textMatch 'value' }");
    }

    [Fact]
    public void FullTextOptimiserComplex2()
    {
        TestOptimisation("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) . ?s pf:textMatch 'value' }");
    }

    [Fact]
    public void FullTextOptimiserComplex3()
    {
        SparqlQuery q = TestOptimisation("SELECT * WHERE { ?s pf:textMatch 'value' . BIND(STR(?s) AS ?str) }");
        ISparqlAlgebra algebra = q.ToAlgebra();
        Assert.DoesNotContain("PropertyFunction(Extend(", algebra.ToString());
    }

    [Fact]
    public void FullTextOptimiserComplex4()
    {
        SparqlQuery q =
            TestOptimisation("SELECT * WHERE { (?s ?score) pf:textMatch 'value' . BIND(STR(?s) AS ?str) }");
        ISparqlAlgebra algebra = q.ToAlgebra();
        Assert.DoesNotContain("PropertyFunction(Extend(", algebra.ToString());
    }

    [Fact]
    public void FullTextOptimiserComplex5()
    {
        //Actual test case from FTXT-364
        var query = @"PREFIX rdf: <" + NamespaceMapper.RDF + @">
PREFIX rdfs: <" + NamespaceMapper.RDFS + @">
PREFIX my: <http://example.org/my#>

SELECT DISTINCT ?result ?isWebSite WHERE {
        _:sparql-autos2 rdf:rest rdf:nil .
        _:sparql-autos1 rdf:rest _:sparql-autos2 .
        _:sparql-autos2 rdf:first ?score .
        _:sparql-autos1 pf:textMatch _:sparql-autos3 .
        _:sparql-autos1 rdf:first ?result .
        BIND(IF (EXISTS { ?result a my:PersonalSite . } , true , false) AS ?isWebSite) .
        _:sparql-autos3 rdf:first 'securite~' .
        _:sparql-autos3 rdf:rest rdf:nil .
       ?result a my:Organization .
       ?result rdfs:label ?label .
    } ORDER BY DESC(?isWebSite) DESC(?score) ASC(?label)";
        SparqlQuery q = TestOptimisation(query);
        ISparqlAlgebra algebra = q.ToAlgebra();
        Assert.DoesNotContain("PropertyFunction(Extend(", algebra.ToString());
    }
}

class MockSearchProvider
    : IFullTextSearchProvider
{

    #region IFullTextSearchProvider Members

    public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(string text, int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(string text)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold,
        int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text,
        double scoreThreshold, int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text,
        double scoreThreshold)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text, int limit)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text)
    {
        throw new NotImplementedException();
    }

    public bool IsAutoSynced
    {
        get
        {
            return true;
        }
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    #endregion
}
#endif