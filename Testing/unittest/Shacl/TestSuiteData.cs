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


namespace VDS.RDF.Shacl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using IO = System.IO;

    internal static class TestSuiteData
    {
        private static readonly Uri baseUri = UriFactory.Create(IO.Path.GetFullPath("resources\\shacl\\test-suite\\manifest.ttl"));

        private static readonly NodeFactory factory = new NodeFactory();
        private static readonly INode mf_include = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#include"));
        private static readonly INode mf_entries = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#entries"));
        private static readonly INode mf_action = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#action"));
        private static readonly INode mf_result = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#result"));
        private static readonly INode sht_Failure = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#Failure"));
        private static readonly INode sht_dataGraph = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#dataGraph"));
        private static readonly INode sht_shapesGraph = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#shapesGraph"));

        private static TripleStore store;

        private static TripleStore Store
        {
            get
            {
                if (store is null)
                {
                    store = new DiskDemandTripleStore();

                    Populate(baseUri);

                    // Add proposed nodeValidator test missing from component manifest
                    Populate(new Uri(baseUri, "sparql/component/nodeValidator-001.ttl"));
                }

                return store;
            }
        }

        public static IEnumerable<object[]> CoreTestNames =>
           from name in TestNames
           where name.StartsWith("core")
           select new[] { name };

        public static IEnumerable<object[]> SparqlTestNames =>
            from name in TestNames
            where name.StartsWith("sparql")
            select new[] { name };

        public static IEnumerable<string> TestNames =>
            from entries in Store.GetTriplesWithPredicate(mf_entries)
            select baseUri.MakeRelativeUri(((IUriNode)entries.Subject).Uri).ToString();

        internal static void ExtractTestData(string name, out IGraph testGraph, out bool failure, out IGraph dataGraph, out IGraph shapesGraph)
        {
            testGraph = Store[new Uri(baseUri, name)];

            var entries = testGraph.GetTriplesWithPredicate(mf_entries).Single().Object;
            var entry = testGraph.GetListItems(entries).Single();
            var action = testGraph.GetTriplesWithSubjectPredicate(entry, mf_action).Single().Object;
            var dataGraphUri = ((IUriNode)testGraph.GetTriplesWithSubjectPredicate(action, sht_dataGraph).Single().Object).Uri;
            var shapesGraphUri = ((IUriNode)testGraph.GetTriplesWithSubjectPredicate(action, sht_shapesGraph).Single().Object).Uri;
            var result = testGraph.GetTriplesWithSubjectPredicate(entry, mf_result).Single().Object;
            failure = result.Equals(sht_Failure);

            Store.HasGraph(dataGraphUri);
            dataGraph = Store[dataGraphUri];

            Store.HasGraph(shapesGraphUri);
            shapesGraph = Store[shapesGraphUri];
        }

        private static void Populate(Uri u)
        {
            if (store.HasGraph(u))
            {
                var g = store[u];
                foreach (var t in g.GetTriplesWithPredicate(mf_include).ToList())
                {
                    Populate(((IUriNode)t.Object).Uri);
                }
            }
        }
    }
}
