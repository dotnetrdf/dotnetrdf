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
    using System.IO;
    using System.Linq;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;
    using VDS.RDF.Writing;
    using Xunit;
    using Xunit.Abstractions;

    public class ShaclTestSuite
    {
        private const string basePath = "resources\\shacl\\manifest.ttl";

        private readonly ITestOutputHelper output;

        private static readonly NodeFactory factory = new NodeFactory();
        private static readonly INode mf_Manifest = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#Manifest"));
        private static readonly INode mf_action = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#action"));
        private static readonly INode mf_include = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#include"));
        private static readonly INode mf_entries = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#entries"));
        private static readonly INode mf_result = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#result"));
        private static readonly INode sht_Validate = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#Validate"));
        private static readonly INode sht_Failure = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#Failure"));
        private static readonly INode sht_dataGraph = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#dataGraph"));
        private static readonly INode sht_shapesGraph = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/ns/shacl-test#shapesGraph"));
        private static readonly INode rdf_type = factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        public ShaclTestSuite(ITestOutputHelper output)
        {
            this.output = output;
        }

        private static TripleStore store;
        public static TripleStore Store
        {
            get
            {
                if (store is null)
                {
                    store = new DiskDemandTripleStore();
                    Populate(new Uri(Path.GetFullPath(basePath)));
                }

                return store;
            }
        }

        public static IEnumerable<object[]> TestNameData =>
            from entries in Store.GetTriplesWithPredicate(mf_entries)
            let name = new Uri(Path.GetFullPath(basePath)).MakeRelativeUri(((IUriNode)entries.Subject).Uri).ToString()
            select new[] { name };

        [Theory]
        [MemberData(nameof(TestNameData))]
        public void Validate(string name)
        {
            var b = new Uri(new Uri(Path.GetFullPath(basePath)), name);

            var g = Store[b];
            var entries = g.GetTriplesWithPredicate(mf_entries).Single().Object;
            var entry = g.GetListItems(entries).Single();
            var action = g.GetTriplesWithSubjectPredicate(entry, mf_action).Single().Object;
            var dataGraphNode = (IUriNode)g.GetTriplesWithSubjectPredicate(action, sht_dataGraph).Single().Object;
            var shapesGraphNode = (IUriNode)g.GetTriplesWithSubjectPredicate(action, sht_shapesGraph).Single().Object;

            Store.HasGraph(dataGraphNode.Uri);
            Store.HasGraph(shapesGraphNode.Uri);

            var dataGraph = Store[dataGraphNode.Uri];
            var shapesGraph = Store[shapesGraphNode.Uri];

            var result = entries.Graph.GetTriplesWithSubjectPredicate(entry, mf_result).Single().Object;
            var failure = result.Equals(sht_Failure);
            var conforms = !failure && entries.Graph.GetTriplesWithSubjectPredicate(result, Shacl.Conforms).Single().Object.AsValuedNode().AsBoolean();


            var validationResult = false;
            var validationFailure = false;
            try
            {
                validationResult = new ShaclShapesGraph(shapesGraph).Validate(dataGraph, out var report);
                output.WriteLine(Writing.StringWriter.Write(report.Graph, new CompressingTurtleWriter()));
            }
            catch
            {
                validationFailure = true;
                throw;
            }

            if (failure)
            {
                Assert.True(validationFailure);
            }
            else
            {
                Assert.Equal(conforms, validationResult);
            }
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
