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
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText;

[Trait("category", "explicit")]
[Trait("category", "fulltext")]
[Collection("FullText")]
public class FullTextSparqlTests2
{
    private SparqlQueryParser _parser = new SparqlQueryParser();
    private INamespaceMapper _nsmap;
    private ISparqlDataset _dataset;
    private LuceneTestHarness _testHarness = new LuceneTestHarness();

    public FullTextSparqlTests2()
    {
        _nsmap = new NamespaceMapper();
        _nsmap.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        _dataset = new InMemoryDataset(store, true);
    }

    private INamespaceMapper GetQueryNamespaces()
    {
        return _nsmap;
    }

    private void RunTest(IFullTextIndexer indexer, String query, IEnumerable<INode> expected)
    {

        indexer.Index(_dataset);
        indexer.Dispose();

        //Build the SPARQL Query and parse it
        var queryString = new SparqlParameterizedString(query)
        {
            Namespaces = GetQueryNamespaces()
        };
        SparqlQuery q = _parser.ParseFromString(queryString);

        var formatter = new SparqlFormatter(q.NamespaceMap);
        Console.WriteLine("Parsed Query:");
        Console.WriteLine(formatter.Format(q));

        Console.WriteLine("Expected Results:");
        foreach (INode n in expected)
        {
            Console.WriteLine(n.ToString(formatter));
        }
        Console.WriteLine();

        var provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
        try
        {
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(provider) };
            var processor = new LeviathanQueryProcessor(_dataset);
            var results = processor.ProcessQuery(q) as SparqlResultSet;
            if (results != null)
            {
                TestTools.ShowResults(results);

                foreach (INode n in expected)
                {
                    Assert.True(results.Any(r => r.HasValue("match") && r["match"] != null && r["match"].Equals(n)), "Did not get expected ?match => " + formatter.Format(n));
                }
                foreach (SparqlResult r in results)
                {
                    Assert.True(r.HasValue("match") && r["match"] != null && expected.Contains(r["match"]), "Unexpected Match " + formatter.Format(r["match"]));
                }
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }
        finally
        {
            provider.Dispose();
            _testHarness.Index.Dispose();
        }
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects1()
    {
        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a <http://example.org/noSuchThing> }", Enumerable.Empty<INode>());
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects2()
    {
        var expected = (from t in _dataset.Triples
                                where t.Object.NodeType == NodeType.Literal
                                      && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                select t.Subject).ToList();
        var factory = new NodeFactory(new NodeFactoryOptions());
        expected.RemoveAll(n => !_dataset.ContainsTriple(new Triple(n, factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
        Assert.True(expected.Any());

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class }", expected);
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects3()
    {
        var expected = (from t in _dataset.Triples
                                where t.Object.NodeType == NodeType.Literal
                                      && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                select t.Subject).ToList();
        var factory = new NodeFactory(new NodeFactoryOptions());
        expected.RemoveAll(n => !_dataset.ContainsTriple(new Triple(n, factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
        Assert.True(expected.Any());

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match a rdfs:Class . { ?match pf:textMatch 'http' } }", expected);
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects4()
    {
        var expected = (from t in _dataset.Triples
                                where t.Object.NodeType == NodeType.Literal
                                      && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                select t.Subject).ToList();
        var factory = new NodeFactory(new NodeFactoryOptions());
        expected.RemoveAll(n => !_dataset.ContainsTriple(new Triple(n, factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
        expected.RemoveAll(n => !_dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());
        Assert.True(expected.Any());

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match }", expected);
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects5()
    {
        var expected = (from t in _dataset.Triples
                                where t.Object.NodeType == NodeType.Literal
                                      && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                select t.Subject).ToList();
        var factory = new NodeFactory(new NodeFactoryOptions());
        expected.RemoveAll(n => !_dataset.ContainsTriple(new Triple(n, factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
        expected.RemoveAll(n => !_dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());


        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match . OPTIONAL { ?property rdfs:label ?label } }", expected);
    }

    [Fact]
    public void FullTextSparqlComplexLuceneSubjects6()
    {
        var expected = (from t in _dataset.Triples
                                where t.Object.NodeType == NodeType.Literal
                                      && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                select t.Subject).ToList();
        var factory = new NodeFactory(new NodeFactoryOptions());

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . OPTIONAL { ?match ?p ?o } }", expected);
    }
}
#endif