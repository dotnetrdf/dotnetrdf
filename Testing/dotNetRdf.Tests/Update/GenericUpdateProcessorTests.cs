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

using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Update;


public abstract class GenericUpdateProcessorTests : BaseTest
{
    private readonly SparqlUpdateParser _parser = new SparqlUpdateParser();

    protected GenericUpdateProcessorTests(ITestOutputHelper output) : base(output)
    {

    }

    protected abstract IStorageProvider GetManager();

    [Fact]
    public void SparqlUpdateGenericCreateAndInsertData()
    {
        IStorageProvider manager = GetManager();
        var processor = new GenericUpdateProcessor(manager);
        SparqlUpdateCommandSet cmds = _parser.ParseFromString("CREATE SILENT GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

        processor.ProcessCommandSet(cmds);

        var g = new Graph();
        manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

        TestTools.ShowGraph(g);

        Assert.False(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void SparqlUpdateGenericCreateInsertDeleteData()
    {
        IStorageProvider manager = GetManager();
        var processor = new GenericUpdateProcessor(manager);
        SparqlUpdateCommandSet cmds = _parser.ParseFromString("CREATE SILENT GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

        processor.ProcessCommandSet(cmds);

        var g = new Graph();
        manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

        TestTools.ShowGraph(g);

        Assert.False(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
        Assert.Equal(1, g.Triples.Count);

        cmds = _parser.ParseFromString("DELETE DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");
        processor.ProcessCommandSet(cmds);

        var h = new Graph();
        manager.LoadGraph(h, "http://example.org/sparqlUpdate/created");

        TestTools.ShowGraph(h);

        Assert.True(h.IsEmpty, "[" + manager.ToString() + "] Graph should be empty");
    }
}
