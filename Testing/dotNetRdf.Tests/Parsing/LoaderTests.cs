/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Algebra;
using WireMock.Logging;
using Xunit;

namespace VDS.RDF.Parsing;

[Collection("RdfServer")]
public class LoaderTests
{
    private readonly RdfServerFixture _serverFixture;
    public LoaderTests(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
        _serverFixture.Server.ResetLogEntries();
    }

    [Fact]
    public void ParsingDataUri1()
    {
        var rdfFragment = "@prefix : <http://example.org/> . :subject :predicate :object .";
        var rdfBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rdfFragment));
        var rdfAscii = Uri.EscapeDataString(rdfFragment);
        var uris = new List<string>()
        {
            "data:text/turtle;charset=UTF-8;base64," + rdfBase64,
            "data:text/turtle;base64," + rdfBase64,
            "data:;base64," + rdfBase64,
            "data:text/turtle;charset=UTF-8," + rdfAscii,
            "data:text/turtle," + rdfAscii,
            "data:," + rdfAscii
        };

        foreach (var uri in uris)
        {
            var u = new Uri(uri);
            var g = new Graph();
            DataUriLoader.Load(g, u);
            Assert.Equal(1, g.Triples.Count);
        }
    }

    [Fact]
    public void ParsingDataUri2()
    {
        var rdfFragment = "@prefix : <http://example.org/> . :subject :predicate :object .";
        var rdfBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rdfFragment));
        var rdfAscii = Uri.EscapeDataString(rdfFragment);
        var uris = new List<string>()
        {
            "data:text/turtle;charset=UTF-8;base64," + rdfBase64,
            "data:text/turtle;base64," + rdfBase64,
            "data:;base64," + rdfBase64,
            "data:text/turtle;charset=UTF-8," + rdfAscii,
            "data:text/turtle," + rdfAscii,
            "data:," + rdfAscii
        };

        foreach (var uri in uris)
        {
            var u = new Uri(uri);
            var loader = new Loader(_serverFixture.Client);
            var g = new Graph();
            loader.LoadGraph(g, u);
            Assert.Equal(1, g.Triples.Count);
        }
    }

    [Fact]
    public void ParsingEmbeddedResourceInDotNetRdf()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        Assert.False(g.IsEmpty, "Graph should be non-empty");
    }

    [Fact]
    public void ParsingEmbeddedResourceInDotNetRdf2()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl, dotNetRDF");
        Assert.False(g.IsEmpty, "Graph should be non-empty");
    }

    [Fact]
    public void ParsingEmbeddedResourceInExternalAssembly()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.embedded.ttl, dotNetRDF.Test");
        Assert.False(g.IsEmpty, "Graph should be non-empty");
    }

    [Fact]
    public void ParsingEmbeddedResourceLoaderGraphIntoTripleStore()
    {
        var store = new TripleStore();
        store.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        Assert.True(store.Triples.Any());
        Assert.Equal(1, store.Graphs.Count);
    }

    [Fact]
    public void ParsingFileLoaderGraphIntoTripleStore()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile("fileloader-graph-to-store.ttl");

        var store = new TripleStore();
        
        store.LoadFromFile("fileloader-graph-to-store.ttl");

        Assert.True(store.Triples.Any());
        Assert.Equal(1, store.Graphs.Count);
    }

    [Fact]
    public async Task LoadNewGraphFromHttpUri()
    {
        var g = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/one.ttl");
        await loader.LoadGraphAsync(g, resourceUri);
        g.Triples.Count.Should().Be(1);
        g.BaseUri.Should().Be(resourceUri, "the loader should set the base URI of the target graph if it is not already set.");
    }

    [Fact]
    public async Task FollowRelativeRedirect()
    {
        var g = new Graph();
        var loader = new Loader(_serverFixture.NoRedirectClient);
        Uri resourceUri = _serverFixture.UriFor("/redirectRelative");
        await loader.LoadGraphAsync(g, resourceUri);
        g.IsEmpty.Should().BeFalse();
        g.BaseUri.Should().Be(resourceUri);
    }

    [Fact]
    public async Task FollowAbsoluteRedirect()
    {
        var g = new Graph();
        var loader = new Loader(_serverFixture.NoRedirectClient);
        Uri resourceUri = _serverFixture.UriFor("/redirectAbsolute");
        await loader.LoadGraphAsync(g, resourceUri);
        g.IsEmpty.Should().BeFalse();
        g.BaseUri.Should().Be(resourceUri);
    }

    [Fact]
    public async Task FollowRelativeRedirectForDataset()
    {
        var s = new TripleStore();
        var loader = new Loader(_serverFixture.NoRedirectClient);
        Uri resourceUri = _serverFixture.UriFor("/redirectQuadsRelative");
        await loader.LoadDatasetAsync(s, resourceUri);
        s.Triples.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FollowAbsoluteRedirectForDataset()
    {
        var s = new TripleStore();
        var loader = new Loader(_serverFixture.NoRedirectClient);
        Uri resourceUri = _serverFixture.UriFor("/redirectQuadsAbsolute");
        await loader.LoadDatasetAsync(s, resourceUri);
        s.Triples.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RedirectsCanBeDisabled()
    {
        var g = new Graph();
        var loader = new Loader(_serverFixture.NoRedirectClient) {  MaxRedirects = 0 };
        Uri resourceUri = _serverFixture.UriFor("/redirectAbsolute");
        RdfException ex = await Assert.ThrowsAsync<RdfException>(() => loader.LoadGraphAsync(g, resourceUri));
        ex.Message.Should().Contain("303");
    }

    [Fact]
    public async Task LoadExistingGraphFromHttpUri()
    {
        var baseUri = new Uri("http://example.org/graph");
        var g = new Graph { BaseUri = baseUri };
        g.Assert(g.CreateUriNode(new Uri("http://example.org/s")),
            g.CreateUriNode(new Uri("http://example.org/p")),
            g.CreateUriNode(new Uri("http://example.org/o")));
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/one.ttl");

        await loader.LoadGraphAsync(g, resourceUri);

        g.Triples.Count.Should().Be(2, "the loader should add to existing graph content, not replace it.");
        g.BaseUri.Should().Be(baseUri, "the loader should not change the base URI of the target graph if it is already set.");
    }

    [Fact]
    public async Task ParseGraphToCustomHandler()
    {
        var h = new CountHandler();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/one.nt");
        await loader.LoadGraphAsync(h, resourceUri, null, CancellationToken.None);
        h.Count.Should().Be(1);
    }

    [Fact]
    public async Task LoadGraphFromHttpUriFailsIfRequestFails()
    {
        var graph = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/notfound.ttl");
        RdfException ex = await Assert.ThrowsAsync<RdfException>(() => loader.LoadGraphAsync(graph, resourceUri));
        ex.Message.Should().Contain(resourceUri.AbsoluteUri).And.Contain("404").And.Contain("Not Found");
    }

    [Fact]
    public async Task LoadGraphWithSpecificParserForcesHttpHeader()
    {
        var graph = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/resource");
        await loader.LoadGraphAsync(graph, resourceUri, new TurtleParser(TurtleSyntax.W3C, false), TestContext.Current.CancellationToken);
        ILogEntry requestLog = _serverFixture.Server.LogEntries.FirstOrDefault(e => e.RequestMessage.Path.EndsWith("/resource"));
        requestLog.Should().NotBeNull();
        requestLog.RequestMessage.Headers["Accept"].Should().Contain(v => v.Contains("text/turtle"));
        requestLog.RequestMessage.Headers["Accept"].Should().NotContain(v => v.Contains("application/n-triples"));
    }

    [Fact]
    public async Task LoadTripleStoreFromHttpUri()
    {
        var store = new TripleStore();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/one.trig");
        await loader.LoadDatasetAsync(store, resourceUri);
        store.Triples.Count().Should().Be(2);
        store.Graphs.Count.Should().Be(2);
    }

    [Fact]
    public async Task LoadGraphFromQuadFormatHttpUri()
    {
        var g = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/one.trig");
        await loader.LoadGraphAsync(g, resourceUri);
        g.Triples.Count.Should().Be(2);
    }

    [Fact]
    public async Task LoadTripleStoreFromHttpUriFailsIfRequestFails()
    {
        var store = new TripleStore();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/notfound.trig");
        RdfException ex = await Assert.ThrowsAsync<RdfException>(() => loader.LoadDatasetAsync(store, resourceUri));
        ex.Message.Should().Contain(resourceUri.AbsoluteUri).And.Contain("404").And.Contain("Not Found");
    }

    [Fact]
    public async Task LoadGraphCanBeCancelled()
    {
        var graph = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/wait");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(500);
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            loader.LoadGraphAsync(graph, resourceUri, null, cts.Token));
    }

    [Fact]
    public async Task LoadDatasetCanBeCancelled()
    {
        var store = new TripleStore();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = _serverFixture.UriFor("/wait");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(500);
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            loader.LoadDatasetAsync(store, resourceUri, null, cts.Token));
    }

    [Fact]
    public async Task LoadGraphFromFileUri()
    {
        var graph = new Graph();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = Path.DirectorySeparatorChar.Equals('/')
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
        var loader = new Loader(_serverFixture.Client);
        var resourceUri = new Uri("data:text/turtle,<http://example.org/s>%20a%20<http://example.org/t>%20.");
        await loader.LoadGraphAsync(graph, resourceUri);
        graph.Triples.Count.Should().Be(1);
    }

    [Fact]
    public async Task LoadDatasetFromFileUri()
    {
        var store = new TripleStore();
        var loader = new Loader(_serverFixture.Client);
        Uri resourceUri = Path.DirectorySeparatorChar.Equals('/')
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
        var loader = new Loader(_serverFixture.Client);
        var resourceUri = new Uri("data:application/x-trig,{<http://example.org/s>%20a%20<http://example.org/t>%20.}<http://example.org/g>{<http://example.org/s> <http://example.org/p> <http://example.org/o>%20.}");
        await loader.LoadDatasetAsync(store, resourceUri);
        store.Graphs.Count.Should().Be(2);
        store.Triples.Count().Should().Be(2);
    }
}
