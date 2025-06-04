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
using System.IO;
using Xunit;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing;


[Collection("RdfServer")]
public class BaseUriAssignmentTests
{
    private readonly RdfServerFixture _serverFixture;
    public BaseUriAssignmentTests(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void ParsingBaseUriAssignmentFileLoader()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        Assert.NotNull(g.BaseUri);
    }

    [Fact]
    public void ParsingBaseUriAssignmentUriLoader()
    {
        var loader = new Loader(_serverFixture.Client);
        var g = new Graph();
        loader.LoadGraph(g, _serverFixture.UriFor("/resource/Southampton"));
        Assert.NotNull(g.BaseUri);
    }

    [Fact]
    public void ParsingBaseUriAssignmentRdfXml()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/RdfXml")
        };

        var strWriter = new System.IO.StringWriter();
        var writer = new RdfXmlWriter();
        writer.Save(g, strWriter);

        var h = new Graph();
        var parser = new RdfXmlParser();
        parser.Load(h, new System.IO.StringReader(strWriter.ToString()));
        Assert.NotNull(h.BaseUri);
    }
}
