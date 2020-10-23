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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Summary description for DefaultGraphTests
    /// </summary>

    public class DefaultGraphTests
    {
        private object ExecuteQuery(IInMemoryQueryableStore store, string query)
        {
            var parser = new SparqlQueryParser();
            var parsedQuery = parser.ParseFromString(query);
            var processor = new LeviathanQueryProcessor(store);
            return processor.ProcessQuery(parsedQuery);
        }

        [Fact]
        public void SparqlDefaultGraphExists()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);

            var results = ExecuteQuery(store, "ASK WHERE { GRAPH ?g { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.False(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.True(false, "ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlDefaultGraphExists2()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);

            var results = ExecuteQuery(store, "ASK WHERE { GRAPH <dotnetrdf:default-graph> { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.False(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.True(false, "ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlDatasetDefaultGraphManagement()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store);
            dataset.SetDefaultGraph(h.BaseUri);
            var processor = new LeviathanQueryProcessor(dataset);
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");

            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
                Assert.True(((SparqlResultSet)results).IsEmpty, "Results should be empty as an empty Graph was set as the Default Graph");
            }
            else
            {
                Assert.True(false, "ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlDatasetDefaultGraphManagement2()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store);
            dataset.SetDefaultGraph(g.BaseUri);
            var processor = new LeviathanQueryProcessor(dataset);
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");

            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
                Assert.False(((SparqlResultSet)results).IsEmpty, "Results should be false as a non-empty Graph was set as the Default Graph");
            }
            else
            {
                Assert.True(false, "ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [Fact(Skip="Remote configuration is not currently available")]
        public void SparqlDatasetDefaultGraphManagementWithUpdate()
        {
            var store = new TripleStore();
            var g = new Graph();
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store, h.BaseUri);
            var processor = new LeviathanUpdateProcessor(dataset);
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://www.dotnetrdf.org/configuration#>");

            processor.ProcessCommandSet(cmds);

            Assert.True(g.IsEmpty, "Graph with null URI (normally the default Graph) should be empty as the Default Graph for the Dataset should have been a named Graph so this Graph should not have been filled by the LOAD Command");
            Assert.False(h.IsEmpty, "Graph with name should be non-empty as it should have been the Default Graph for the Dataset and so filled by the LOAD Command");
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void SparqlDatasetDefaultGraphManagementWithUpdate2()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store, h.BaseUri);
            var processor = new LeviathanUpdateProcessor(dataset);
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <http://example.org/graph>");

            processor.ProcessCommandSet(cmds);

            Assert.False(g.IsEmpty, "First Graph should not be empty as should have been filled by the LOAD command");
            Assert.True(h.IsEmpty, "Second Graph should be empty as should not have been filled by the LOAD command");
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void SparqlDatasetDefaultGraphManagementWithUpdate3()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store, h.BaseUri);
            var processor = new LeviathanUpdateProcessor(dataset);
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <http://example.org/graph>; LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <http://example.org/someOtherGraph>");

            processor.ProcessCommandSet(cmds);

            Assert.False(g.IsEmpty, "First Graph should not be empty as should have been filled by the first LOAD command");
            Assert.False(h.IsEmpty, "Second Graph should not be empty as should not have been filled by the second LOAD command");
            Assert.Equal(g, h);
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void SparqlDatasetDefaultGraphManagementWithUpdate4()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store, h.BaseUri);
            var processor = new LeviathanUpdateProcessor(dataset);
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://www.dotnetrdf.org/configuration#>; WITH <http://example.org/graph> INSERT { ?s a ?type } USING <http://example.org/someOtherGraph> WHERE { ?s a ?type }");

            processor.ProcessCommandSet(cmds);

            Assert.False(g.IsEmpty, "First Graph should not be empty as should have been filled by the INSERT command");
            Assert.False(h.IsEmpty, "Second Graph should not be empty as should not have been filled by the LOAD command");
            Assert.True(h.HasSubGraph(g), "First Graph should be a subgraph of the Second Graph");
        }

        [Fact(Skip = "Remote configuration is not currently available")]
        public void SparqlDatasetDefaultGraphManagementWithUpdate5()
        {
            var store = new TripleStore();
            var g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);
            var h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            var dataset = new InMemoryDataset(store, h.BaseUri);
            var processor = new LeviathanUpdateProcessor(dataset);
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://www.dotnetrdf.org/configuration#>; WITH <http://example.org/graph> INSERT { ?s a ?type } USING <http://example.org/someOtherGraph> WHERE { ?s a ?type }; DELETE WHERE { ?s a ?type }");

            processor.ProcessCommandSet(cmds);

            Assert.False(g.IsEmpty, "First Graph should not be empty as should have been filled by the INSERT command");
            Assert.False(h.IsEmpty, "Second Graph should not be empty as should not have been filled by the  LOAD command");
            Assert.False(h.HasSubGraph(g), "First Graph should not be a subgraph of the Second Graph as the DELETE should have eliminated the subgraph relationship");
        }

        [Fact]
        public void SparqlGraphClause()
        {
            var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            var dataset = new InMemoryDataset();
            IGraph ex = new Graph();
            FileLoader.Load(ex, "resources\\InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            dataset.SetDefaultGraph(def.BaseUri);

            var processor = new LeviathanQueryProcessor(dataset);
            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.Equal(ex.Triples.Count, rset.Count);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlGraphClause2()
        {
            var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            var dataset = new InMemoryDataset();
            IGraph ex = new Graph();
            FileLoader.Load(ex, "resources\\InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            var processor = new LeviathanQueryProcessor(dataset);
            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.Equal(ex.Triples.Count, rset.Count);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlGraphClause3()
        {
            var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            var dataset = new InMemoryDataset(false);
            IGraph ex = new Graph();
            FileLoader.Load(ex, "resources\\InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            dataset.SetDefaultGraph(def.BaseUri);

            var processor = new LeviathanQueryProcessor(dataset);
            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.Equal(ex.Triples.Count, rset.Count);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlGraphClause4()
        {
            var query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            var dataset = new InMemoryDataset(false);
            IGraph ex = new Graph();
            FileLoader.Load(ex, "resources\\InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            var processor = new LeviathanQueryProcessor(dataset);
            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.Equal(ex.Triples.Count, rset.Count);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlGraphClause5()
        {
            var query = "SELECT * FROM NAMED <http://example.org/named> WHERE { GRAPH <http://example.org/other> { ?s ?p ?o } }";
            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            var store = new TripleStore();
            IGraph ex = new Graph();
            FileLoader.Load(ex, "resources\\InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/named");
            store.Add(ex);
            IGraph ex2 = new Graph();
            ex2.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            ex2.BaseUri = new Uri("http://example.org/other");
            store.Add(ex2);

            var dataset = new InMemoryDataset(store);

            var processor = new LeviathanQueryProcessor(dataset);
            var results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.Equal(0, rset.Count);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }
    }
}
