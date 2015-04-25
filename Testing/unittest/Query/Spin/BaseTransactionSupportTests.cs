#define CLEANUP
using System;
using NUnit.Framework;
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
using VDS.RDF.Query.Spin.Core.Runtime;

namespace SpinTest
{

    [TestFixture]
    public abstract class BaseTransactionSupportTests
    {

        protected abstract IUpdateableStorage PhysicalStorage { get; }
        protected abstract SpinStorageProvider SpinProvider { get; }

        protected abstract bool IsConfigured();

        private Connection conn;
        private Connection conn2;

        public void TansactionSupportTests()
        {
        }

        public void CleanupBeforeTest()
        {
            if (!IsConfigured()) return;
            foreach (Uri graphUri in PhysicalStorage.ListGraphs().Where(u => u != null && u.ToString().StartsWith("tmp:dotnetrdf.org")))
            {
                PhysicalStorage.DeleteGraph(graphUri);
            }
            foreach (Uri graphUri in PhysicalStorage.ListGraphs().Where(u => u != null && u.ToString().StartsWith("unit-test:dotnetrdf.org")))
            {
                PhysicalStorage.DeleteGraph(graphUri);
            }

        }

        [TearDown()]
        public void CloseConnections()
        {
            if (conn != null) conn.Close();
            if (conn2 != null) conn2.Close();
        }
        
        /// <remarks>
        /// This test doesn't run as expected with an in-memory storage, due to an issue in algebra evaluation
        /// see ExplicitGraphsPropertyPathCompilationTest for more explanations and possible solution
        /// </remarks>
        [Test]
        public void DefaultGraphsPropertyPathCompilationTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:test> { ?unboundS ?unboundP ?unboundO . ?unboundS ?unboundP ?unboundO . } GRAPH <unit-test:dotnetrdf.org:tests:DefaultGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING <http://spinrdf.org/spin> USING <http://spinrdf.org/spin2> WHERE { ?s rdfs:subClassOf+ ?p . }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:DefaultGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        /// <remarks>
        /// This test doesn't run as expected with an in-memory storage
        /// This is caused by the LeftJoin evaluation (from the OPTIONAL) that wraps the transactional triple expansion : 
        /// Checks return that :
        ///     => though the Extend are expressed in constant terms, the result variable is always considered floating (see <see cref="Query.Algebra.Extend">line 203 </see>
        ///     => even when forcing variable to fixed when ther is not any floating variable in the inner expression Query.Algrebra.LeftJoin.CanFlowResultsToRhs still returns false due to the test l.312
        ///         => should not LHS fixed variables allow to flow the results ?
        /// </remarks>
        [Test]
        public void ExplicitGraphsPropertyPathCompilationTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ExplicitGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph <http://spinrdf.org/spin> { ?s rdfs:subClassOf+ ?p } }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:ExplicitGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        [Test]
        public void DefaultGraphsBindingsTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:DefaultGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING <http://spinrdf.org/spin> WHERE { ?s rdfs:subClassOf ?p }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:DefaultGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        [Test]
        public void ExplicitGraphsBindingsTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ExplicitGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph <http://spinrdf.org/spin> { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:ExplicitGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        [Test]
        public void NamedGraphsBindingsTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:NamedGraphsBindingsTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph ?g { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:NamedGraphsBindingsTest");
            Assert.AreEqual(32, result.Triples.Count, "Wrong asserted triples count");
        }

        /// <remarks>
        /// This test doesn't run as expected with an in-memory storage, due to an issue in algebra evaluation
        /// see ExplicitGraphsPropertyPathCompilationTest for more explanations and possible solution
        /// </remarks>
        [Test]
        public void NamedGraphsPropertyPathCompilationTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:NamedGraphsPropertyPathCompilationTest> { ?s <tmp:dotnetrdf.org:rdfs:subClassOf> ?p . } }  USING NAMED <http://spinrdf.org/spin> USING NAMED <http://spinrdf.org/spin2> WHERE { graph ?g { ?s rdfs:subClassOf+ ?p } }");
            conn.Commit();
            conn.Close();
            IGraph result = new Graph();
            PhysicalStorage.LoadGraph(result, "unit-test:dotnetrdf.org:tests:NamedGraphsPropertyPathCompilationTest");
            Assert.AreEqual(69, result.Triples.Count, "Wrong asserted triples count");
        }

        [Test]
        public void ModificationUsingWithTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            PhysicalStorage.UpdateGraph("unit-test:dotnetrdf.org:tests:ModificationUsingWithTest", new List<Triple>() { new Triple(TransactionSupportStrategy.ClassConcurrentAssertionsGraph, RDF.PropertyType, RuntimeHelper.ClassTemporaryGraph) }, null);
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> WITH <unit-test:dotnetrdf.org:tests:ModificationUsingWithTest> INSERT { ?s a rdfs:TestClass . } WHERE { ?s a ?p . }");
            conn.Commit();
            conn.Close();
        }

        [Test]
        public void ConcurrentTransactionIsolationTest()
        {
            IsConfigured();
            CleanupBeforeTest();
            conn2 = SpinProvider.GetConnection();
            conn2.Open();
            SparqlResultSet emptyResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsTrue(emptyResult.IsEmpty, "Query should not return any result");
            conn = SpinProvider.GetConnection();
            conn.Open();
            conn.Update(@"PREFIX rdfs:<http://www.w3.org/2000/01/rdf-schema#> INSERT { GRAPH <unit-test:dotnetrdf.org:tests:ConcurrencyTest> { <urn:insertedS> <urn:insertedP> <urn:insertedO>. } }  USING NAMED <http://spinrdf.org/spin> WHERE { graph ?g { ?s rdfs:subClassOf ?p } }");
            conn.Commit();
            conn.Close();
            SparqlResultSet newResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsTrue(newResult.IsEmpty, "Query should not return any result");
            conn2.Close();
            conn2 = SpinProvider.GetConnection();
            conn2.Open(); 
            newResult = (SparqlResultSet)conn2.Query(@"SELECT * FROM <unit-test:dotnetrdf.org:tests:ConcurrencyTest> WHERE { ?s ?p ?o . }");
            Assert.IsFalse(newResult.IsEmpty, "Query should return some result");
            conn2.Close();
        }
    }
}
