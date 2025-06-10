using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VDS.RDF;

/// <summary>
/// An XUnit test data provider class that loads test definitions from an W3C test manifest RDF file.
/// </summary>
public class ManifestTestDataProvider : IEnumerable<object[]>
{
    private readonly Manifest _manifest;

    public ManifestTestDataProvider(Uri baseUri, string manifestPath, Func<IGraph, INode, bool> testNodeFilter = null)
    {
        _manifest = new Manifest(baseUri, manifestPath, testNodeFilter);
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return _manifest.GetTestData().Select(testData => new object[] { testData }).GetEnumerator();
    }

    public ManifestTestData GetTestData(Uri testUri)
    {
        IUriNode testNode = _manifest.Graph.GetUriNode(testUri);
        Assert.NotNull(testNode);
        return new ManifestTestData(_manifest, testNode);
    }

    public ManifestTestData GetTestData(string testId)
    {
        ManifestTestData data = _manifest.GetTestData().SingleOrDefault(x => x.Id == testId);
        Assert.NotNull(data);
        return data;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}