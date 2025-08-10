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

using System.Diagnostics.CodeAnalysis;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.LDF.Client;

[SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped", Justification = BecauseThisIsExampleCode)]
public abstract class E2EBaseTests(ITestOutputHelper output)
{
    private const string BecauseThisIsExampleCode = "Skipping example code";

    private static readonly NodeFactory factory = new();

    protected abstract TpfLiveGraph Graph { get; }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void ContainsTriple()
    {
        using var g = this.Graph;
        var s = UriNode("http://dbpedia.org/ontology/extinctionDate");
        var p = UriNode("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
        var o = UriNode("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
        var t = new Triple(s, p, o);

        output.WriteLine("{0}", g.ContainsTriple(t));
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void EqualsTest()
    {
        using var g1 = this.Graph;
        using var g2 = this.Graph;
        var equals = g1.Equals(g2);

        output.WriteLine("{0}", equals);
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithObject()
    {
        using var g = this.Graph;
        var o = LiteralNode("1997-02-04", XmlSpecsHelper.XmlSchemaDataTypeDate);
        var triples = g.GetTriplesWithObject(o);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithPredicate()
    {
        using var g = this.Graph;
        var p = UriNode("http://dbpedia.org/ontology/extinctionDate");
        var triples = g.GetTriplesWithPredicate(p);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithPredicateObject()
    {
        using var g = this.Graph;
        var p = UriNode("http://dbpedia.org/ontology/extinctionDate");
        var o = LiteralNode("2011-10-05", XmlSpecsHelper.XmlSchemaDataTypeDate);
        var triples = g.GetTriplesWithPredicateObject(p, o);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithSubject()
    {
        using var g = this.Graph;
        var s = UriNode("http://0-access.newspaperarchive.com.topcat.switchinc.org/Viewer.aspx?img=7578853");
        var triples = g.GetTriplesWithSubject(s);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithSubjectObject()
    {
        using var g = this.Graph;
        var s = UriNode("http://dbpedia.org/resource/123_Democratic_Alliance");
        var o = LiteralNode("707366241", XmlSpecsHelper.XmlSchemaDataTypeInteger);
        var triples = g.GetTriplesWithSubjectObject(s, o);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void GetTriplesWithSubjectPredicate()
    {
        using var g = this.Graph;
        var s = UriNode("http://dbpedia.org/resource/123_Democratic_Alliance");
        var p = UriNode("http://dbpedia.org/ontology/extinctionDate");
        var triples = g.GetTriplesWithSubjectPredicate(s, p);

        foreach (var triple in triples)
        {
            output.WriteLine("{0}", triple);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void ObjectNodes()
    {
        using var g = this.Graph;
        using var triples = g.Triples.ObjectNodes.GetEnumerator();

        for (var i = 0; i < 100; i++)
        {
            triples.MoveNext();
            output.WriteLine("{0}", triples.Current);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void PredicateNodes()
    {
        using var g = this.Graph;
        using var triples = g.Triples.PredicateNodes.GetEnumerator();

        for (var i = 0; i < 25; i++)
        {
            triples.MoveNext();
            output.WriteLine("{0}", triples.Current);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void Sparql()
    {
        using var g = this.Graph;
        var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    <http://0-access.newspaperarchive.com.topcat.switchinc.org/Viewer.aspx?img=7578853> ?p ?o .
}
");

        var formatter = new SparqlFormatter();
        foreach (var result in results)
        {
            output.WriteLine(formatter.Format(result));
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void SubjectNodes()
    {
        using var g = this.Graph;
        using var triples = g.Triples.SubjectNodes.GetEnumerator();

        for (var i = 0; i < 20; i++)
        {
            triples.MoveNext();
            output.WriteLine("{0}", triples.Current);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void Triples()
    {
        using var g = this.Graph;
        using var triples = g.Triples.GetEnumerator();

        for (var i = 0; i < 110; i++)
        {
            triples.MoveNext();
            output.WriteLine("{0}", triples.Current);
        }
    }

    [Fact(Skip = BecauseThisIsExampleCode)]
    public void TriplesCount()
    {
        using var g = this.Graph;
        var count = g.Triples.Count;

        output.WriteLine("{0}", count);
    }

    private static IUriNode UriNode(string uri)
    {
        return factory.CreateUriNode(UriFactory.Create(uri));
    }

    private static ILiteralNode LiteralNode(string literal, string datatype)
    {
        return factory.CreateLiteralNode(literal, UriFactory.Create(datatype));
    }
}
