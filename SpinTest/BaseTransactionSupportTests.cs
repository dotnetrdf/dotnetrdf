#define CLEANUP
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using VDS.RDF;
using VDS.RDF.Storage;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query.Spin.Utility;
using System.Collections.Generic;

namespace SpinTest
{

    public abstract class BaseTransactionSupportTests
        : IDisposable
    {

        protected abstract IQueryableStorage Storage { get; }

        private Connection conn;
        private Connection conn2; 

        public void TansactionSupportTests()
        {
        }

        public void Dispose()
        {
        }


        public static void CleanupBeforeTest(IQueryableStorage Storage)
        {
            foreach (Uri graphUri in Storage.ListGraphs().Where(u => u!=null && u.ToString().StartsWith("tmp:dotnetrdf.org")))
            {
                Storage.DeleteGraph(graphUri);
            }
            foreach (Uri graphUri in Storage.ListGraphs().Where(u => u != null && u.ToString().StartsWith("unit-test:dotnetrdf.org")))
            {
                Storage.DeleteGraph(graphUri);
            }

        }

        [TestInitialize()]
        public void TestInitialize()
        {
            IGraph spinMemoryGraph = new ThreadSafeGraph();
            spinMemoryGraph.BaseUri = UriFactory.Create("http://spinrdf.org/spin");
            spinMemoryGraph.LoadFromUri(spinMemoryGraph.BaseUri, new RdfXmlParser());
            Storage.SaveGraph(spinMemoryGraph);

            conn = new Connection();
            conn2 = new Connection();
        }

        [TestCleanup()]
        public void CloseConnections() {
            conn.Close();
            conn2.Close();
        }

        [TestInitialize()]
        public void Initialize() {
        }

        [TestMethod]
        public void DefaultGraphsPropertyPathCompilationTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:test> { ?unboundS ?unboundP ?unboundO . ?unboundS ?unboundP ?unboundO . } GRAPH <unit-test:dotnetrdf.org:tests:DefaultGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING <http://spinrdf.org/spin> USING <http://spinrdf.org/spin2> WHERE { ?s rdfs:subClassOf+ ?p . }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:DefaultGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void ExplicitGraphsPropertyPathCompilationTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ExplicitGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> USING NAMED <http://spinrdf.org/spin2> WHERE { graph <http://spinrdf.org/spin> { ?s rdfs:subClassOf+ ?p } }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:ExplicitGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void DefaultGraphsBindingsTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:DefaultGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING <http://spinrdf.org/spin> WHERE { ?s rdfs:subClassOf ?p }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:DefaultGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void ExplicitGraphsBindingsTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ExplicitGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph <http://spinrdf.org/spin> { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:ExplicitGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void NamedGraphsBindingsTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:NamedGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph ?g { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:NamedGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void NamedGraphsPropertyPathCompilationTest()
        {
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:NamedGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> USING NAMED <http://spinrdf.org/spin2> WHERE { graph ?g { ?s rdfs:subClassOf+ ?p } }");
            conn.Commit();
            IGraph result = new Graph();
            Storage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:NamedGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        [TestMethod]
        public void ModificationUsingWithTest()
        {
            Storage.UpdateGraph("unit-test:dotnetrdf.org:tests:ModificationUsingWithTest", new List<Triple>() { new Triple(DOTNETRDF_TRANS.ClassConcurrentAssertionsGraph, RDF.PropertyType, DOTNETRDF_TRANS.ClassTemporaryGraph) }, null);
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> WITH <unit-test:dotnetrdf.org:tests:ModificationUsingWithTest> INSERT { ?s a rdfs:TestClass . } WHERE { ?s a ?p . }");
            conn.Commit();
        }

        [TestMethod]
        public void ConcurrentTransactionIsolationTest()
        {
            conn2.Open(Storage);
            SparqlResultSet emptyResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsTrue(emptyResult.IsEmpty, "Query should not return any result");
            conn.Open(Storage);
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ConcurrencyTest> { <urn:insertedS> <urn:insertedP> <urn:insertedO>. } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph ?g { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            conn.Close();
            SparqlResultSet newResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsTrue(newResult.IsEmpty, "Query should not return any result");

            conn2.Open(Storage);
            newResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsFalse(newResult.IsEmpty, "Query should return some result");
        }
    }
}
