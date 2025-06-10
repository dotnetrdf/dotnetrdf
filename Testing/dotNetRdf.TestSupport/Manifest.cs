using dotNetRdf.TestSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VDS.RDF;

public class Manifest
{
    public Uri BaseUri { get; }
    public string LocalDirectory { get; }
    public Uri LocalManifestUri { get; }
    public IGraph Graph { get; }

    private readonly INode _mfInclude;

    private readonly List<Manifest> _childManifests = new List<Manifest>();

    private readonly Func<IGraph, INode, bool> _testNodeFilter;

    public Manifest(Uri baseUri, string localFilePath, Func<IGraph, INode, bool> testNodeFilter)
    {
        BaseUri = baseUri;
        Graph = new Graph { BaseUri = baseUri };
        _testNodeFilter = testNodeFilter;
        Graph.NamespaceMap.AddNamespace("mf", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#"));
        Graph.NamespaceMap.AddNamespace("qt", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-query#"));
        Graph.NamespaceMap.AddNamespace("ut", UriFactory.Root.Create("http://www.w3.org/2009/sparql/tests/test-update#"));
        Graph.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        Graph.NamespaceMap.AddNamespace("test", UriFactory.Root.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-dawg#"));
        _mfInclude = Graph.CreateUriNode("mf:include");
        LocalDirectory = Path.GetDirectoryName(localFilePath);
        LocalManifestUri = new Uri(new Uri("file://"), Path.GetFullPath(localFilePath));
        LoadManifest(localFilePath);
    }

    private void LoadManifest(string manifestPath)
    {
        Graph.LoadFromFile(manifestPath);
        Triple includeTriple = Graph.GetTriplesWithPredicate(_mfInclude).FirstOrDefault();
        while (includeTriple != null)
        {
            switch (includeTriple.Object)
            {
                // Load included manifests
                case IUriNode manifestRef:
                    {
                        _childManifests.Add(new Manifest(manifestRef.Uri, ResolveResourcePath(manifestRef.Uri), _testNodeFilter));
                        break;
                    }
                case IBlankNode manifestList:
                    {
                        foreach (IUriNode manifestItem in Graph.GetListItems(manifestList).OfType<IUriNode>())
                        {
                            _childManifests.Add(new Manifest(manifestItem.Uri, ResolveResourcePath(manifestItem.Uri), _testNodeFilter));
                        }
                        break;
                    }
            }

            Graph.Retract(includeTriple);
            includeTriple = Graph.GetTriplesWithPredicate(_mfInclude).FirstOrDefault();
        }
    }
    
    public string ResolveResourcePath(Uri resourcePath)
    {
        Uri relPath = BaseUri.MakeRelativeUri(resourcePath);
        return new Uri(LocalManifestUri, relPath).LocalPath;
    }

    public IEnumerable<ManifestTestData> GetTestData()
    {
        INode testApproval = Graph.CreateUriNode("test:approval");
        INode testWithdrawn = Graph.CreateUriNode("test:withdrawn");
        INode rdfType = Graph.CreateUriNode("rdf:type");
        IEnumerable<INode> manifests = Graph.GetTriplesWithPredicateObject(Graph.CreateUriNode("rdf:type"), Graph.CreateUriNode("mf:Manifest")).Select(t => t.Subject);
        foreach (INode manifest in manifests)
        {
            IEnumerable<INode> testLists = Graph.GetTriplesWithSubjectPredicate(manifest, Graph.CreateUriNode("mf:entries")).Select(t => t.Object);
            foreach (INode testList in testLists)
            {
                foreach (IUriNode testNode in Graph.GetListItems(testList).OfType<IUriNode>())
                {
                    if (!Graph.ContainsTriple(new Triple(testNode, testApproval, testWithdrawn))
                        && Graph.Triples.WithSubjectPredicate(testNode, rdfType).Any()
                        && (_testNodeFilter == null || _testNodeFilter(Graph, testNode)))
                    {
                        yield return new ManifestTestData(this, testNode);
                    }
                }
            }
        }

        foreach (ManifestTestData childTest in _childManifests.SelectMany(childManifest => childManifest.GetTestData()))
        {
            yield return childTest;
        }
    }
}