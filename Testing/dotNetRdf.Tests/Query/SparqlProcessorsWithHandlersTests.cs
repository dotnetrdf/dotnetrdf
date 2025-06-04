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
using System.Net.Http;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

public class SparqlProcessorsWithHandlersTests
{
    private SparqlQueryParser _parser = new SparqlQueryParser();

    #region Test Runner Methods

    private void TestCountHandler(ISparqlQueryProcessor processor, String query)
    {
        SparqlQuery q = _parser.ParseFromString(query);

        var expected = processor.ProcessQuery(q) as Graph;
        Assert.NotNull(expected);

        var handler = new CountHandler();
        processor.ProcessQuery(handler, null, q);

        Assert.Equal(expected.Triples.Count, handler.Count);
    }

    private void TestGraphHandler(ISparqlQueryProcessor processor, String query)
    {
        SparqlQuery q = _parser.ParseFromString(query);

        var expected = processor.ProcessQuery(q) as Graph;
        Assert.NotNull(expected);

        var actual = new Graph();
        var handler = new GraphHandler(actual);
        processor.ProcessQuery(handler, null, q);

        Assert.Equal(expected, actual);
    }

    private void TestPagingHandler(ISparqlQueryProcessor processor, String query, int limit, int offset)
    {
        throw new NotImplementedException();
    }

    private void TestResultCountHandler(ISparqlQueryProcessor processor, String query)
    {
        SparqlQuery q = _parser.ParseFromString(query);

        var expected = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(expected);

        var handler = new ResultCountHandler();
        processor.ProcessQuery(null, handler, q);

        Assert.Equal(expected.Count, handler.Count);
    }

    private void TestResultSetHandler(ISparqlQueryProcessor processor, String query)
    {
        SparqlQuery q = _parser.ParseFromString(query);

        var expected = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(expected);

        var actual = new SparqlResultSet();
        var handler = new ResultSetHandler(actual);
        processor.ProcessQuery(null, handler, q);

        Assert.Equal(expected, actual);
    }

    private void TestWriteThroughHandler(ISparqlQueryProcessor processor, String query)
    {
        var formatter = new NTriplesFormatter();
        var data = new StringWriter();

        SparqlQuery q = _parser.ParseFromString(query);
        var expected = processor.ProcessQuery(q) as Graph;
        Assert.NotNull(expected);

        var handler = new WriteThroughHandler(formatter, data, false);
        processor.ProcessQuery(handler, null, q);
        Console.WriteLine(data.ToString());

        var actual = new Graph();
        StringParser.Parse(actual, data.ToString(), new NTriplesParser());

        Assert.Equal(expected, actual);
    }

    #endregion

    #region Test Batch Methods

    private void TestCountHandlers(ISparqlQueryProcessor processor)
    {
        TestResultCountHandler(processor, "SELECT * WHERE { ?s a ?type }");
        TestResultCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?child rdfs:subClassOf ?parent }");
        TestCountHandler(processor, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        TestCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?child rdfs:subClassOf ?parent }");
    }

    private void TestGraphHandlers(ISparqlQueryProcessor processor)
    {
        TestGraphHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
        TestGraphHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
    }

    private void TestPagingHandlers(ISparqlQueryProcessor processor)
    {

    }

    private void TestWriteThroughHandlers(ISparqlQueryProcessor processor)
    {
        TestWriteThroughHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
        TestWriteThroughHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
    }

    #endregion

    #region Leviathan Tests

    private ISparqlDataset _dataset;
    private ISparqlQueryProcessor _leviathan;
    private ISparqlQueryProcessor _explainer;

    private void EnsureLeviathanReady()
    {
        if (_dataset == null)
        {
            var store = new TripleStore();
            var g = new Graph();
            g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
            store.Add(g);

            _dataset = new InMemoryDataset(store);
        }
        if (_leviathan == null)
        {
            _leviathan = new LeviathanQueryProcessor(_dataset);
        }
        if (_explainer == null)
        {
            _explainer = new ExplainQueryProcessor(_dataset, ExplanationLevel.Default);
        }
    }

    [Fact]
    public void SparqlWithHandlersLeviathanCount()
    {
        EnsureLeviathanReady();
        TestCountHandlers(_leviathan);
    }

    [Fact]
    public void SparqlWithHandlersLeviathanExplainCount()
    {
        EnsureLeviathanReady();
        TestCountHandlers(_explainer);
    }

    [Fact]
    public void SparqlWithHandlersLeviathanGraph()
    {
        EnsureLeviathanReady();
        TestGraphHandlers(_leviathan);
    }

    [Fact]
    public void SparqlWithHandlersLeviathanExplainGraph()
    {
        EnsureLeviathanReady();
        TestGraphHandlers(_explainer);
    }

    [Fact]
    public void SparqlWithHandlersLeviathanWriteThrough()
    {
        EnsureLeviathanReady();
        TestWriteThroughHandlers(_leviathan);
    }

    [Fact]
    public void SparqlWithHandlersLeviathanExplainWriteThrough()
    {
        EnsureLeviathanReady();
        TestWriteThroughHandlers(_explainer);
    }

    #endregion

    #region Remote Tests

    private ISparqlQueryProcessor _remote;

    private void EnsureRemoteReady()
    {
        if (_remote == null)
        {
            Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS), "Test Config marks IIS as unavailable, cannot run test");
            _remote = new RemoteQueryProcessor(new SparqlQueryClient(new HttpClient(),  new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalQueryUri))));
        }
    }

    [Fact]
    public void SparqlWithHandlersRemoteCount()
    {
        EnsureRemoteReady();
        TestCountHandlers(_remote);
    }

    [Fact]
    public void SparqlWithHandlersRemoteGraph()
    {
        EnsureRemoteReady();
        TestGraphHandlers(_remote);
    }

    [Fact]
    public void SparqlWithHandlersRemoteWriteThrough()
    {
        EnsureRemoteReady();
        TestWriteThroughHandlers(_remote);
    }

    #endregion

    #region Generic Tests

    private ISparqlQueryProcessor _generic;

    private void EnsureGenericReady()
    {
        EnsureLeviathanReady();
        if (_generic == null)
        {
            _generic = new GenericQueryProcessor(new InMemoryManager(_dataset));
        }
    }

    [Fact]
    public void SparqlWithHandlersGenericCount()
    {
        EnsureGenericReady();
        TestCountHandlers(_generic);
    }

    [Fact]
    public void SparqlWithHandlersGenericGraph()
    {
        EnsureGenericReady();
        TestGraphHandlers(_generic);
    }

    [Fact]
    public void SparqlWithHandlersGenericWriteThrough()
    {
        EnsureGenericReady();
        TestWriteThroughHandlers(_generic);
    }

    #endregion
}
