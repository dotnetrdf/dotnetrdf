using dotNetRdf.TestSupport;
using System.Collections;
using VDS.RDF;

namespace dotNetRdf.TestSuite.Rdfa;

public class RdfaTestDataProvider : IEnumerable<object[]>
{
    private readonly RdfaTestManifest _manifest;
    public RdfaTestDataProvider(Uri baseUri, string manifestPath)
    {
        _manifest = new RdfaTestManifest(baseUri, manifestPath);
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return _manifest.GetTestData().Select(testData => new object[] { testData }).GetEnumerator();
    }

    public RdfaTestData GetTestData(Uri testUri)
    {
        IUriNode testNode = _manifest.Graph.GetUriNode(testUri);
        Assert.NotNull(testNode);
        return new RdfaTestData(_manifest, testNode);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
