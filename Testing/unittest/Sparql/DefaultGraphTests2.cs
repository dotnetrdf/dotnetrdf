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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Sparql
{
    [TestClass]
    public class DefaultGraphTests2
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private ISparqlDataset GetDataset(List<IGraph> gs, bool unionDefaultGraph)
        {
            TripleStore store = new TripleStore();
            foreach (IGraph g in gs)
            {
                store.Add(g, false);
            }

            return new InMemoryDataset(store, unionDefaultGraph);
        }

        private ISparqlDataset GetDataset(List<IGraph> gs, Uri defaultGraphUri)
        {
            TripleStore store = new TripleStore();
            foreach (IGraph g in gs)
            {
                store.Add(g, false);
            }

            return new InMemoryDataset(store, defaultGraphUri);
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphUnion()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, true);
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(g.Triples.Count + h.Triples.Count, r.Triples.Count);
                Assert.IsTrue(r.HasSubGraph(g), "g should be a subgraph of the results");
                Assert.IsTrue(r.HasSubGraph(h), "h should be a subgraph of the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphUnionAndGraphClause()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, true);
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/unknown> { ?s ?p ?o } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(0, r.Triples.Count);
                Assert.IsFalse(r.HasSubGraph(g), "g should not be a subgraph of the results");
                Assert.IsFalse(r.HasSubGraph(h), "h should not be a subgraph of the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphNoUnion()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, false);
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(0, r.Triples.Count);
                Assert.IsFalse(r.HasSubGraph(g), "g should not be a subgraph of the results");
                Assert.IsFalse(r.HasSubGraph(h), "h should not be a subgraph of the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphNamed()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, new Uri("http://example.org/1"));
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(g.Triples.Count, r.Triples.Count);
                Assert.AreEqual(g, r, "g should be equal to the results");
                Assert.AreNotEqual(h, r, "h should not be equal to the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphNamedAndGraphClause()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, new Uri("http://example.org/1"));
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(h.Triples.Count, r.Triples.Count);
                Assert.AreNotEqual(g, r, "g should not be equal to the results");
                Assert.AreEqual(h, r, "h should be equal to the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphNamed2()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, new Uri("http://example.org/2"));
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(h.Triples.Count, r.Triples.Count);
                Assert.AreNotEqual(g, r, "g should not be equal to the results");
                Assert.AreEqual(h, r, "h should be equal to the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphNamedAndGraphClause2()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, new Uri("http://example.org/2"));
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(g.Triples.Count, r.Triples.Count);
                Assert.AreEqual(g, r, "g should be equal to the results");
                Assert.AreNotEqual(h, r, "h should not be equal to the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphUnknownName()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, new Uri("http://example.org/unknown"));
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(0, r.Triples.Count);
                Assert.IsFalse(r.HasSubGraph(g), "g should not be a subgraph of the results");
                Assert.IsFalse(r.HasSubGraph(h), "h should not be a subgraph of the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphUnnamed()
        {
            List<IGraph> gs = new List<IGraph>();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/1");
            gs.Add(g);
            Graph h = new Graph();
            h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            h.BaseUri = new Uri("http://example.org/2");
            gs.Add(h);

            ISparqlDataset dataset = this.GetDataset(gs, null);
            String query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph r = (IGraph)results;
                Assert.AreEqual(0, r.Triples.Count);
                Assert.IsFalse(r.HasSubGraph(g), "g should not be a subgraph of the results");
                Assert.IsFalse(r.HasSubGraph(h), "h should not be a subgraph of the results");
            }
            else
            {
                Assert.Fail("Did not return a Graph as expected");
            }
        }
    }
}
