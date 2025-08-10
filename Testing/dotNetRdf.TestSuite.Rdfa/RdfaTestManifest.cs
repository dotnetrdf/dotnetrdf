using dotNetRdf.TestSupport;
using VDS.RDF;

namespace dotNetRdf.TestSuite.Rdfa;

public class RdfaTestManifest
{
    public Uri BaseUri { get; }
    public string? LocalDirectory { get; }
    public Uri LocalManifestUri { get; }
    public TripleStore Store { get; }
    public IGraph Graph { get; private set; }

    public RdfaTestManifest(Uri baseUri, string localFilePath)
    {
        if (localFilePath == null) throw new ArgumentNullException(nameof(localFilePath));
        BaseUri = baseUri;
        Store = new TripleStore();
        Graph = new Graph();
        Store.Add(Graph);
        Graph.NamespaceMap.AddNamespace("dc", UriFactory.Root.Create("http://purl.org/dc/terms/"));
        Graph.NamespaceMap.AddNamespace("rdfatest", UriFactory.Root.Create("http://rdfa.info/vocabs/rdfa-test#"));
        Graph.NamespaceMap.AddNamespace("test", UriFactory.Root.Create("http://www.w3.org/2006/03/test-description#"));
        LocalDirectory = Path.GetDirectoryName(localFilePath);
        LocalManifestUri = new Uri(new Uri("file://"), Path.GetFullPath(localFilePath));
        LoadManifest(localFilePath);
    }

    private void LoadManifest(string manifestPath)
    {
        Store.LoadFromFile(manifestPath);
    }

    public string ResolveResourcePath(Uri resourcePath)
    {
        Uri relPath = BaseUri.MakeRelativeUri(resourcePath);
        return new Uri(LocalManifestUri, relPath).LocalPath;
    }

    public IEnumerable<RdfaTestData> GetTestData()
    {
        INode testCase = Graph.CreateUriNode("test:TestCase");
        IEnumerable<INode> tests = Graph.GetTriplesWithPredicateObject(
            Graph.CreateUriNode("rdf:type"),
            Graph.CreateUriNode("test:TestCase")).Select(triple => triple.Subject);
        foreach (IUriNode testNode in tests.OfType<IUriNode>())
        {
            yield return new RdfaTestData(this, testNode);
        }
    }
}
