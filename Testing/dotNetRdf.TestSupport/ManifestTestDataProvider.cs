using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using Xunit;

namespace dotNetRdf.TestSupport
{
    /// <summary>
    /// An XUnit test data provider class that loads test definitions from an W3C test manifest RDF file.
    /// </summary>
    public class ManifestTestDataProvider : IEnumerable<object[]>
    {
        private readonly IGraph _manifestGraph;
        private readonly Manifest _manifest;

        public ManifestTestDataProvider(Uri baseUri, string manifestPath)
        {
            _manifestGraph = new Graph() { BaseUri = baseUri };
            _manifestGraph.NamespaceMap.AddNamespace("mf", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#"));
            _manifestGraph.NamespaceMap.AddNamespace("qt", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-query#"));
            _manifestGraph.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
            _manifestGraph.LoadFromFile(manifestPath);
            _manifest = new Manifest(_manifestGraph, manifestPath);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            IUriNode rdfNil = _manifestGraph.CreateUriNode("rdf:nil");
            IUriNode rdfRest = _manifestGraph.CreateUriNode("rdf:rest");
            IUriNode rdfFirst = _manifestGraph.CreateUriNode("rdf:first");
            IEnumerable<INode> manifests = _manifestGraph.GetTriplesWithPredicateObject(_manifestGraph.CreateUriNode("rdf:type"), _manifestGraph.CreateUriNode("mf:Manifest")).Select(t => t.Subject);
            foreach(INode manifest in manifests)
            {
                IEnumerable<INode> testLists = _manifestGraph.GetTriplesWithSubjectPredicate(manifest, _manifestGraph.CreateUriNode("mf:entries")).Select(t => t.Object);
                foreach(INode testList in testLists)
                {
                    INode listNode = testList;
                    while(!listNode.Equals(rdfNil))
                    {
                        foreach(IUriNode testNode in _manifestGraph.GetTriplesWithSubjectPredicate(listNode, rdfFirst).Select(t => t.Object).OfType<IUriNode>())
                        {
                            yield return new object[] {new ManifestTestData(_manifest, testNode) };
                        }
                        listNode = _manifestGraph.GetTriplesWithSubjectPredicate(listNode, rdfRest).Select(t=>t.Object).First();
                    }
                }
            }
        }

        public ManifestTestData GetTestData(Uri testUri)
        {
            IUriNode testNode = _manifestGraph.GetUriNode(testUri);
            Assert.NotNull(testNode);
            return new ManifestTestData(_manifest, testNode);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}