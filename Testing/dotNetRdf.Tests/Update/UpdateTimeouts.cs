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
using FluentAssertions;
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Update;

[Collection("RdfServer")]
public class UpdateTimeouts
{
    private readonly SparqlUpdateParser _parser = new();
    private readonly RdfServerFixture _serverFixture;

    public UpdateTimeouts(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void SparqlUpdateTimeout()
    {
        var update = $"CREATE GRAPH <http://example.org/1>; LOAD <{_serverFixture.UriFor("/slow/doap")}>; CREATE GRAPH <http://example.org/2>";
        SparqlUpdateCommandSet commandSet = _parser.ParseFromString(update);
        commandSet.Timeout = 1;

        var store = new TripleStore();
        var processor = new LeviathanUpdateProcessor(store);
        Assert.Throws<SparqlUpdateTimeoutException>(() => processor.ProcessCommandSet(commandSet));

        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        store.HasGraph(nodeFactory.CreateUriNode(new Uri("http://example.org/1"))).Should().BeFalse("Graph 1 should not exist");
        store.HasGraph(nodeFactory.CreateUriNode(new Uri("http://example.org/2"))).Should().BeFalse("Graph 2 should not exist");
    }
}
