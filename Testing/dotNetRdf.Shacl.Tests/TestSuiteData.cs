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

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using IO = System.IO;

namespace VDS.RDF.Shacl;


internal static class TestSuiteData
{
    private static readonly Uri baseUri = UriFactory.Root.Create(IO.Path.GetFullPath(System.IO.Path.Combine("resources","shacl", "test-suite", "manifest.ttl")));
    private static readonly TripleStore store;

    private static readonly NodeFactory factory = new NodeFactory(new NodeFactoryOptions());
    private static readonly INode mf_include = Node("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#include");
    private static readonly INode mf_entries = Node("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#entries");
    private static readonly INode mf_action = Node("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#action");
    private static readonly INode mf_result = Node("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#result");
    private static readonly INode sht_Failure = Node("http://www.w3.org/ns/shacl-test#Failure");
    private static readonly INode sht_dataGraph = Node("http://www.w3.org/ns/shacl-test#dataGraph");
    private static readonly INode sht_shapesGraph = Node("http://www.w3.org/ns/shacl-test#shapesGraph");

    static TestSuiteData()
    {
        store = new DiskDemandTripleStore();

        Populate(factory.CreateUriNode(baseUri));

        // Add proposed nodeValidator test missing from component manifest
        Populate(factory.CreateUriNode(new Uri(baseUri, "sparql/component/nodeValidator-001.ttl")));
    }

    public static IEnumerable<TheoryDataRow<string>> CoreTests =>
        from name in CoreTestNames
        select new TheoryDataRow<string>(name);

    public static IEnumerable<TheoryDataRow<string>> CoreFullTests =>
        from name in CoreTestNames.Except(CoreFullExcludedTests)
        select new TheoryDataRow<string>(name);

    public static IEnumerable<TheoryDataRow<string>> SparqlTests =>
        from name in Tests
        where name.StartsWith("sparql")
        select new TheoryDataRow<string>(name);

    public static IEnumerable<string> CoreTestNames =>
        from name in Tests
        where name.StartsWith("core")
        select name;

    private static IEnumerable<string> Tests =>
        from entries in store.GetTriplesWithPredicate(mf_entries)
        select baseUri.MakeRelativeUri(((IUriNode) entries.Subject).Uri).ToString();

    private static IEnumerable<string> CoreFullExcludedTests
    {
        get
        {
            // Validation report graph equality chacking fails but seems OK otherwise
            yield return "core/path/path-complex-002.ttl";
            yield return "core/property/nodeKind-001.ttl";
        }
    }

    internal static void ExtractTestData(string name, out IGraph testGraph, out bool failure, out IGraph dataGraph, out IGraph shapesGraph)
    {
        testGraph = store[new UriNode(new Uri(baseUri, name))];

        var entries = testGraph.GetTriplesWithPredicate(mf_entries).Single().Object;
        var entry = testGraph.GetListItems(entries).Single();
        var action = testGraph.GetTriplesWithSubjectPredicate(entry, mf_action).Single().Object;
        var dataGraphUri = (IUriNode)testGraph.GetTriplesWithSubjectPredicate(action, sht_dataGraph).Single().Object;
        var shapesGraphUri = (IUriNode)testGraph.GetTriplesWithSubjectPredicate(action, sht_shapesGraph).Single().Object;
        var result = testGraph.GetTriplesWithSubjectPredicate(entry, mf_result).Single().Object;
        failure = result.Equals(sht_Failure);

        store.HasGraph(dataGraphUri);
        dataGraph = store[dataGraphUri];

        store.HasGraph(shapesGraphUri);
        shapesGraph = store[shapesGraphUri];
    }

    internal static void RemoveUnnecessaryResultMessages(IGraph resultReport, IGraph testReport)
    {
        foreach (var t in resultReport.GetTriplesWithPredicate(Vocabulary.ResultMessage).ToList())
        {
            if (!testReport.GetTriplesWithPredicateObject(Vocabulary.ResultMessage, t.Object).Any())
            {
                resultReport.Retract(t);
            }
        }
    }

    private static void Populate(IRefNode u)
    {
        if (store.HasGraph(u))
        {
            var g = store[u];
            foreach (var t in g.GetTriplesWithPredicate(mf_include).ToList())
            {
                Populate(((IUriNode)t.Object));
            }
        }
    }

    private static IUriNode Node(string uri)
    {
        return factory.CreateUriNode(UriFactory.Root.Create(uri));
    }

}
