using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.TestSuite.RdfStar
{
    public class ManifestTestData : IEnumerable<object[]>
    {
        private readonly IGraph _manifestGraph;
        private readonly Manifest _manifest;

        public ManifestTestData(Uri baseUri, string manifestPath)
        {
            _manifestGraph = new Graph() { BaseUri = baseUri };
            _manifestGraph.NamespaceMap.AddNamespace("mf", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#"));
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
                            yield return new object[] {new ManifestTest(_manifest, testNode) };
                        }
                        listNode = _manifestGraph.GetTriplesWithSubjectPredicate(listNode, rdfRest).Select(t=>t.Object).First();
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Manifest
    {
        public Uri BaseUri { get; }
        public string LocalDirectory { get; }
        public Uri LocalManifestUri { get; }
        public IGraph Graph { get; }
        public Manifest(IGraph manifestGraph, string localFilePath)
        {
            Graph = manifestGraph;
            BaseUri = manifestGraph.BaseUri;
            LocalDirectory = Path.GetDirectoryName(localFilePath);
            LocalManifestUri = new Uri(new Uri("file://"), Path.GetFullPath(localFilePath));
        }

        public string ResolveResourcePath(Uri resourcePath)
        {
            var relPath = BaseUri.MakeRelativeUri(resourcePath);
            return new Uri(LocalManifestUri, relPath).LocalPath;
        }
    }
    public class ManifestTest
    {
        public readonly Manifest Manifest;
        public readonly IGraph Graph;
        public readonly IUriNode TestNode;

        public ManifestTest(Manifest manifest, IUriNode testNode)
        {
            Manifest = manifest;
            Graph = manifest.Graph;
            TestNode = testNode;
        }

        public string Id => TestNode.Uri.AbsoluteUri;

        public string Name => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("mf:name"))
            .Select(t => t.Object).OfType<ILiteralNode>().Select(lit => lit.Value).FirstOrDefault();

        public Uri Type => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("rdf:type"))
            .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

        public Uri Action => Graph.GetTriplesWithSubjectPredicate(TestNode, Graph.CreateUriNode("mf:action"))
            .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

        public override string ToString()
        {
            return Id;
        }
    }

    public class ManifestTestRunnerAttribute : Attribute
    {
        public readonly string TestType;

        public ManifestTestRunnerAttribute(string testType)
        {
            TestType = testType;
        }
    }
}
