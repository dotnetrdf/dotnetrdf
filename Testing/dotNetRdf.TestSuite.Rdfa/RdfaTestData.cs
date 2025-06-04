using dotNetRdf.TestSuite.Rdfa;
using System.Linq;
using VDS.RDF;

namespace dotNetRdf.TestSupport;

public class RdfaTestData
{
    public readonly RdfaTestManifest Manifest;
    public readonly TripleStore Store;
    public readonly IGraph Graph;
    public readonly IUriNode TestNode;

    public RdfaTestData(RdfaTestManifest manifest, IUriNode testNode)
    {
        Manifest = manifest;
        Store = manifest.Store;
        Graph = manifest.Graph;
        TestNode = testNode;
    }

    public string Id => TestNode.Uri.AbsoluteUri;
    public string? Description => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("dc:title")).Select(t => t.Object).OfType<ILiteralNode>().Select(lit => lit.Value).FirstOrDefault();
    public Uri? Input => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("test:informationResourceInput"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();
    public Uri? Results => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("test:informationResourceResults"))
        .Select(t => t.Object).OfType<IUriNode>().Select(n => n.Uri).FirstOrDefault();

    public IEnumerable<string> Versions => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("rdfatest:rdfaVersion"))
        .Select(t => t.Object).OfType<ILiteralNode>().Select(n => n.Value);

    public IEnumerable<string> HostLangauges => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("rdfatest:hostLanguage"))
        .Select(t => t.Object).OfType<ILiteralNode>().Select(n => n.Value);
    public bool ExpectedResults => Store.GetTriplesWithSubjectPredicate(TestNode, Graph.GetUriNode("test:expectedResults"))
        .Select(t => t.Object).OfType<ILiteralNode>().Select(n => n.EffectiveBooleanValue()).FirstOrDefault();

    public string InputPath => Manifest.ResolveResourcePath(Input);

    public string GetInputPath(string version, string hostLanguage)
    {
        var fileName = GetInputFileName(version, hostLanguage);
        return Path.Combine(InputPath, $"..", version, hostLanguage, fileName);
    }

    private string GetInputFileName(string version, string hostLanguage)
    {
        var fileName = Path.GetFileNameWithoutExtension(InputPath);
        if (hostLanguage.Contains("xhtml"))
        {
            fileName += ".xhtml";
        }
        else if (hostLanguage.Contains("html"))
        {
            fileName += ".html";
        }
        else if (hostLanguage.Contains("xml"))
        {
            fileName += ".xml";
        }

        return fileName;
    }

    public Uri GetInputUrl(string version, string hostLanguage)
    {
        var fileName = GetInputFileName(version, hostLanguage);
        return new Uri(TestNode.Uri, $"{version}/{hostLanguage}/{fileName}");
    }

    public string GetResultPath(string version, string hostLanguage)
    {
        if (Results == null) throw new ArgumentException("There is no results path for this test.");
        var resultsPath = Manifest.ResolveResourcePath(Results);
        var fileName = Path.GetFileName(resultsPath);
        return Path.Combine(resultsPath, $"..", version, hostLanguage, fileName);
    }

    public override string ToString()
    {
        return Id;
    }
}
