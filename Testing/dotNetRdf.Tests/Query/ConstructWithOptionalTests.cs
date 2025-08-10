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
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;



public class ConstructWithOptionalTests
{
    private SparqlQueryParser _parser = new SparqlQueryParser();
    private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

    private ISparqlDataset AsDataset(IInMemoryQueryableStore store)
    {
        if (store.Graphs.Count == 1)
        {
            return new InMemoryDataset(store, store.Graphs.First().Name);
        }
        else
        {
            return new InMemoryDataset(store);
        }
    }

    private void TestConstruct(IGraph data, IGraph expected, String query)
    {
        var store = new TripleStore();
        store.Add(data);

        TestConstruct(store, expected, query);
    }

    private void TestConstruct(IInMemoryQueryableStore store, IGraph expected, String query)
    {
        SparqlQuery q = _parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var result = (IGraph)results;

            var formatter = new NTriplesFormatter();
            Console.WriteLine("Result Data");
            foreach (Triple t in result.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Console.WriteLine("Expected Data");
            foreach (Triple t in expected.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Assert.Equal(expected, result);
        }
        else
        {
            Assert.Fail("Did not get a Graph as expected");
        }
    }

    private void TestUpdate(IGraph data, IGraph expected, String update)
    {
        var store = new TripleStore();
        store.Add(data);

        TestUpdate(store, expected, update);
    }

    private void TestUpdate(IInMemoryQueryableStore store, IGraph expected, String update)
    {
        SparqlUpdateCommandSet cmds = _updateParser.ParseFromString(update);

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(cmds);

        Assert.True(store.HasGraph((IRefNode)null), "Store should have a default unnamed Graph");
        IGraph result = store[(IRefNode)null];
        
        var formatter = new NTriplesFormatter();
        Console.WriteLine("Result Data");
        foreach (Triple t in result.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
        Console.WriteLine();

        Console.WriteLine("Expected Data");
        foreach (Triple t in expected.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
        Console.WriteLine();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SparqlConstructWithOptional()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        var expected = new Graph();
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

        var query = "PREFIX ex: <http://example.org/vehicles/> CONSTRUCT { ?s a ?type . ?s ex:Speed ?speed } WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

        TestConstruct(g, expected, query);
    }

    [Fact]
    public void SparqlUpdateInsertWithOptional()
    {
        IGraph g = GetNamedTestGraph();

        var expected = new Graph();
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

        var update = "PREFIX ex: <http://example.org/vehicles/> INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

        TestUpdate(g, expected, update);
    }

    private IGraph GetNamedTestGraph()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/vehicles/")));
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        return g;
    }

    [Fact]
    public void SparqlUpdateDeleteWithOptional()
    {
        IGraph g = GetNamedTestGraph();

        var def = new Graph();
        def.Merge(g);

        var store = new TripleStore();
        store.Add(g);
        store.Add(def);

        var expected = new Graph();
        expected.NamespaceMap.Import(g.NamespaceMap);
        expected.Merge(g);
        expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")).ToList());
        expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")).ToList());

        var update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

        TestUpdate(store, expected, update);
    }

    [Fact]
    public void SparqlUpdateModifyWithOptional()
    {
        var g = GetNamedTestGraph();

        var expected = new Graph();
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
        expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

        var update = "PREFIX ex: <http://example.org/vehicles/> DELETE { } INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

        TestUpdate(g, expected, update);
    }

    [Fact]
    public void SparqlUpdateModifyWithOptional2()
    {
        var g = GetNamedTestGraph();

        var def = new Graph();
        def.Merge(g);

        var store = new TripleStore();
        store.Add(g);
        store.Add(def);

        var expected = new Graph();
        expected.NamespaceMap.Import(g.NamespaceMap);
        expected.Merge(g);
        expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")).ToList());
        expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")).ToList());

        var update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } INSERT { } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

        TestUpdate(store, expected, update);
    }
}
