using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public class UriLoaderTests: IClassFixture<RemoteRdfFixture>
    {
        private readonly RemoteRdfFixture _serverFixture;

        public UriLoaderTests(RemoteRdfFixture serverFixture)
        {
            _serverFixture = serverFixture;
            UriLoader.CacheDuration = TimeSpan.FromSeconds(2);
        }

        [Fact]
        public void CacheUpdatesFileCreationTimeOnReload()
        {
            var g = new Graph();
            var uri = new Uri(_serverFixture.Server.Urls[0] + "/rvesse.ttl");
            UriLoader.Load(g, uri);
            Assert.True(UriLoader.IsCached(uri));
            Thread.Sleep(UriLoader.CacheDuration + TimeSpan.FromMilliseconds(500));
            Assert.True(UriLoader.IsCached(uri));
            Assert.False(UriLoader.Cache.HasLocalCopy(uri, true));
            UriLoader.Load(g, uri);
            Assert.True(UriLoader.IsCached(uri));
            Assert.True(UriLoader.Cache.HasLocalCopy(uri, true));
        }
    }
}
