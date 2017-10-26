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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Query
{
    public class SparqlProcessorsWithHandlersTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        #region Test Runner Methods

        private void TestCountHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Graph expected = processor.ProcessQuery(q) as Graph;
            Assert.NotNull(expected);

            CountHandler handler = new CountHandler();
            processor.ProcessQuery(handler, null, q);

            Assert.Equal(expected.Triples.Count, handler.Count);
        }

        private void TestGraphHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Graph expected = processor.ProcessQuery(q) as Graph;
            Assert.NotNull(expected);

            Graph actual = new Graph();
            GraphHandler handler = new GraphHandler(actual);
            processor.ProcessQuery(handler, null, q);

            Assert.Equal(expected, actual);
        }

        private void TestPagingHandler(ISparqlQueryProcessor processor, String query, int limit, int offset)
        {
            throw new NotImplementedException();
        }

        private void TestResultCountHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            SparqlResultSet expected = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(expected);

            ResultCountHandler handler = new ResultCountHandler();
            processor.ProcessQuery(null, handler, q);

            Assert.Equal(expected.Count, handler.Count);
        }

        private void TestResultSetHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            SparqlResultSet expected = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(expected);

            SparqlResultSet actual = new SparqlResultSet();
            ResultSetHandler handler = new ResultSetHandler(actual);
            processor.ProcessQuery(null, handler, q);

            Assert.Equal(expected, actual);
        }

        private void TestWriteThroughHandler(ISparqlQueryProcessor processor, String query)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            StringWriter data = new StringWriter();

            SparqlQuery q = this._parser.ParseFromString(query);
            Graph expected = processor.ProcessQuery(q) as Graph;
            Assert.NotNull(expected);

            WriteThroughHandler handler = new WriteThroughHandler(formatter, data, false);
            processor.ProcessQuery(handler, null, q);
            Console.WriteLine(data.ToString());

            Graph actual = new Graph();
            StringParser.Parse(actual, data.ToString(), new NTriplesParser());

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Test Batch Methods

        private void TestCountHandlers(ISparqlQueryProcessor processor)
        {
            this.TestResultCountHandler(processor, "SELECT * WHERE { ?s a ?type }");
            this.TestResultCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?child rdfs:subClassOf ?parent }");
            this.TestCountHandler(processor, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
            this.TestCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?child rdfs:subClassOf ?parent }");
        }

        private void TestGraphHandlers(ISparqlQueryProcessor processor)
        {
            this.TestGraphHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            this.TestGraphHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
        }

        private void TestPagingHandlers(ISparqlQueryProcessor processor)
        {

        }

        private void TestWriteThroughHandlers(ISparqlQueryProcessor processor)
        {
            this.TestWriteThroughHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            this.TestWriteThroughHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
        }

        #endregion

        #region Leviathan Tests

        private ISparqlDataset _dataset;
        private ISparqlQueryProcessor _leviathan;
        private ISparqlQueryProcessor _explainer;

        private void EnsureLeviathanReady()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                store.Add(g);

                this._dataset = new InMemoryDataset(store);
            }
            if (this._leviathan == null)
            {
                this._leviathan = new LeviathanQueryProcessor(this._dataset);
            }
            if (this._explainer == null)
            {
                this._explainer = new ExplainQueryProcessor(this._dataset, ExplanationLevel.Default);
            }
        }

        [Fact]
        public void SparqlWithHandlersLeviathanCount()
        {
            this.EnsureLeviathanReady();
            this.TestCountHandlers(this._leviathan);
        }

        [Fact]
        public void SparqlWithHandlersLeviathanExplainCount()
        {
            this.EnsureLeviathanReady();
            this.TestCountHandlers(this._explainer);
        }

        [Fact]
        public void SparqlWithHandlersLeviathanGraph()
        {
            this.EnsureLeviathanReady();
            this.TestGraphHandlers(this._leviathan);
        }

        [Fact]
        public void SparqlWithHandlersLeviathanExplainGraph()
        {
            this.EnsureLeviathanReady();
            this.TestGraphHandlers(this._explainer);
        }

        [Fact]
        public void SparqlWithHandlersLeviathanWriteThrough()
        {
            this.EnsureLeviathanReady();
            this.TestWriteThroughHandlers(this._leviathan);
        }

        [Fact]
        public void SparqlWithHandlersLeviathanExplainWriteThrough()
        {
            this.EnsureLeviathanReady();
            this.TestWriteThroughHandlers(this._explainer);
        }

        #endregion

        #region Remote Tests

        private ISparqlQueryProcessor _remote;

        private void EnsureRemoteReady()
        {
            if (this._remote == null)
            {
                if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
                {
                    throw new SkipTestException("Test Config marks IIS as unavailabe, cannot run test");
                }
                this._remote = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalQueryUri))));
            }
        }

        [SkippableFact]
        public void SparqlWithHandlersRemoteCount()
        {
            this.EnsureRemoteReady();
            this.TestCountHandlers(this._remote);
        }

        [SkippableFact]
        public void SparqlWithHandlersRemoteGraph()
        {
            this.EnsureRemoteReady();
            this.TestGraphHandlers(this._remote);
        }

        [SkippableFact]
        public void SparqlWithHandlersRemoteWriteThrough()
        {
            this.EnsureRemoteReady();
            this.TestWriteThroughHandlers(this._remote);
        }

        #endregion

        #region Generic Tests

        private ISparqlQueryProcessor _generic;

        private void EnsureGenericReady()
        {
            this.EnsureLeviathanReady();
            if (this._generic == null)
            {
                this._generic = new GenericQueryProcessor(new InMemoryManager(this._dataset));
            }
        }

        [Fact]
        public void SparqlWithHandlersGenericCount()
        {
            this.EnsureGenericReady();
            this.TestCountHandlers(this._generic);
        }

        [Fact]
        public void SparqlWithHandlersGenericGraph()
        {
            this.EnsureGenericReady();
            this.TestGraphHandlers(this._generic);
        }

        [Fact]
        public void SparqlWithHandlersGenericWriteThrough()
        {
            this.EnsureGenericReady();
            this.TestWriteThroughHandlers(this._generic);
        }

        #endregion
    }
}
