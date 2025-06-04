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
using Xunit;

namespace VDS.RDF.Parsing.Handlers;

[Obsolete("Tests for an obsolete class that will be removed in a future release.")]
public class UriLoaderHandlerTests
{

    [Fact(Skip = "Remote configuration is not currently available")]
    public void ParsingUriLoaderGraphHandlerImplicit()
    {
        var g = new Graph();
        UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"));

        TestTools.ShowGraph(g);
        Assert.False(g.IsEmpty, "Graph should not be empty");
    }

    [Fact(Skip="Remote configuration is not currently available")]
    public void ParsingUriLoaderGraphHandlerExplicit()
    {
        var g = new Graph();
        var handler = new GraphHandler(g);
        UriLoader.Load(handler, new Uri("http://www.dotnetrdf.org/configuration#"));

        TestTools.ShowGraph(g);
        Assert.False(g.IsEmpty, "Graph should not be empty");
    }

    [Fact]
    public void ParsingUriLoaderCountHandler()
    {
        var orig = new Graph();
        orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var handler = new CountHandler();
        EmbeddedResourceLoader.Load(handler, "VDS.RDF.Configuration.configuration.ttl");

        Assert.Equal(orig.Triples.Count, handler.Count);
    }
}
