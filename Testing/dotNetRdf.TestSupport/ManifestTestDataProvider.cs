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
        private readonly Manifest _manifest;

        public ManifestTestDataProvider(Uri baseUri, string manifestPath)
        {
            _manifest = new Manifest(baseUri, manifestPath);
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}