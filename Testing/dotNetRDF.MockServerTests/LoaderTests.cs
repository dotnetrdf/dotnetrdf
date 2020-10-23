using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    [Collection("RdfServerTests")]
    public class LoaderTests : IClassFixture<RdfServerFixture>
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private RdfServerFixture ServerFixture { get; }
        private string ServerUri { get; }
        public LoaderTests(RdfServerFixture serverFixture)
        {
            ServerFixture = serverFixture;
            ServerUri = ServerFixture.Server.Urls[0];
            ServerFixture.Server.ResetLogEntries();
        }

        [Fact]
        public async Task LoadNewGraphFromHttpUri()
        {
            var g = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/one.ttl");
            await loader.LoadGraphAsync(g, resourceUri);
            g.Triples.Count.Should().Be(1);
            g.BaseUri.Should().Be(resourceUri, "the loader should set the base URI of the target graph if it is not already set.");
        }

        [Fact]
        public async Task LoadExistingGraphFromHttpUri()
        {
            var baseUri = new Uri("http://example.org/graph");
            var g = new Graph {BaseUri = baseUri};
            g.Assert(g.CreateUriNode(new Uri("http://example.org/s")),
                g.CreateUriNode(new Uri("http://example.org/p")),
                g.CreateUriNode(new Uri("http://example.org/o")));
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/one.ttl");

            await loader.LoadGraphAsync(g, resourceUri);

            g.Triples.Count.Should().Be(2, "the loader should add to existing graph content, not replace it.");
            g.BaseUri.Should().Be(baseUri, "the loader should not change the base URI of the target graph if it is already set.");
        }

        [Fact]
        public async Task ParseGraphToCustomHandler()
        {
            var h = new CountHandler();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/one.nt");
            await loader.LoadGraphAsync(h, resourceUri, null, CancellationToken.None);
            h.Count.Should().Be(1);
        }

        [Fact]
        public async Task LoadGraphFromHttpUriFailsIfRequestFails()
        {
            var graph = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/notfound.ttl");
            var ex = await Assert.ThrowsAsync<RdfException>(() => loader.LoadGraphAsync(graph, resourceUri));
            ex.Message.Should().Contain(resourceUri.AbsoluteUri).And.Contain("404").And.Contain("Not Found");
        }

        [Fact]
        public async Task LoadGraphWithSpecificParserForcesHttpHeader()
        {
            var graph = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/resource");
            await loader.LoadGraphAsync(graph, resourceUri, new TurtleParser(TurtleSyntax.W3C, false));
            var requestLog = ServerFixture.Server.LogEntries.FirstOrDefault(e => e.RequestMessage.Path.EndsWith("/resource"));
            requestLog.Should().NotBeNull();
            requestLog.RequestMessage.Headers["Accept"].Should().Contain(v => v.Contains("text/turtle"));
            requestLog.RequestMessage.Headers["Accept"].Should().NotContain(v => v.Contains("application/n-triples"));
        }

        [Fact]
        public async Task LoadTripleStoreFromHttpUri()
        {
            var store = new TripleStore();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/one.trig");
            await loader.LoadDatasetAsync(store, resourceUri);
            store.Triples.Count().Should().Be(2);
            store.Graphs.Count.Should().Be(2);
        }

        [Fact]
        public async Task LoadTripleStoreFromHttpUriFailsIfRequestFails()
        {
            var store = new TripleStore();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/notfound.trig");
            var ex = await Assert.ThrowsAsync<RdfException>(() => loader.LoadDatasetAsync(store, resourceUri));
            ex.Message.Should().Contain(resourceUri.AbsoluteUri).And.Contain("404").And.Contain("Not Found");
        }

        [Fact]
        public async Task LoadGraphCanBeCancelled()
        {
            var graph = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/wait");
            var cts = new CancellationTokenSource();
            cts.CancelAfter(500);
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                loader.LoadGraphAsync(graph, resourceUri, null, cts.Token));
        }

        [Fact]
        public async Task LoadDatasetCanBeCancelled()
        {
            var store = new TripleStore();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri(ServerUri + "/wait");
            var cts = new CancellationTokenSource();
            cts.CancelAfter(500);
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                loader.LoadDatasetAsync(store, resourceUri, null, cts.Token));
        }

        [Fact]
        public async Task LoadGraphFromFileUri()
        {
            var graph = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = Path.DirectorySeparatorChar.Equals('/')
                ? new Uri("file://" + Path.GetFullPath(Path.Combine("resources", "simple.ttl")))
                : new Uri(Path.GetFullPath(Path.Combine("resources", "simple.ttl")));
            resourceUri.Scheme.Should().Be("file");
            await loader.LoadGraphAsync(graph, resourceUri);
            graph.Triples.Count.Should().Be(1);
        }

        [Fact]
        public async Task LoadGraphFromDataUri()
        {
            var graph = new Graph();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri("data:text/turtle,<http://example.org/s>%20a%20<http://example.org/t>%20.");
            await loader.LoadGraphAsync(graph, resourceUri);
            graph.Triples.Count.Should().Be(1);
        }

        [Fact]
        public async Task LoadDatasetFromFileUri()
        {
            var store = new TripleStore();
            var loader = new Loader(HttpClient);
            var resourceUri = Path.DirectorySeparatorChar.Equals('/')
                ? new Uri("file://" + Path.GetFullPath(Path.Combine("resources", "simple.trig")))
                : new Uri(Path.GetFullPath(Path.Combine("resources", "simple.trig")));
            resourceUri.Scheme.Should().Be("file");
            await loader.LoadDatasetAsync(store, resourceUri);
            store.Graphs.Count.Should().Be(2);
            store.Triples.Count().Should().Be(2);
        }

        [Fact]
        public async Task LoadDatasetFromDataUri()
        {
            var store = new TripleStore();
            var loader = new Loader(HttpClient);
            var resourceUri = new Uri("data:application/x-trig,{<http://example.org/s>%20a%20<http://example.org/t>%20.}<http://example.org/g>{<http://example.org/s> <http://example.org/p> <http://example.org/o>%20.}");
            await loader.LoadDatasetAsync(store, resourceUri);
            store.Graphs.Count.Should().Be(2);
            store.Triples.Count().Should().Be(2);
        }
    }
}
