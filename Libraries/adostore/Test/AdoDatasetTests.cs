using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Virtualisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class AdoDatasetTests
    {
        private MicrosoftAdoManager _manager;
        private InMemoryDataset _mem;
        private MicrosoftAdoDataset _db;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private IEnumerable<IAlgebraOptimiser> _optimisers;
        private LeviathanQueryProcessor _procMem, _procDb;
        private SparqlParameterizedString _baseQuery;

        private void EnsureTestData()
        {
            Graph g = new Graph();
            if (this._mem == null || this._db == null)
            {
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            }
            if (this._mem == null)
            {
                TripleStore store = new TripleStore();
                store.Add(g);
                this._mem = new InMemoryDataset(store);
                this._procMem = new LeviathanQueryProcessor(this._mem);
            }
            if (this._db == null)
            {
                if (this._manager != null) this._manager.Dispose();
                this._manager = new MicrosoftAdoManager("adostore", "example", "password");
                this._optimisers = new IAlgebraOptimiser[] { new SimpleVirtualAlgebraOptimiser(this._manager) };
                this._manager.SaveGraph(g);
                this._db = new MicrosoftAdoDataset(this._manager);
                this._procDb = new LeviathanQueryProcessor(this._db);
            }
        }

        private void TestQuery(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);
            Stopwatch timer = new Stopwatch();

            //Do in-memory
            timer.Start();
            SparqlResultSet memResults = this._procMem.ProcessQuery(q) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("In-Memory Dataset answered query in " + timer.Elapsed);
            if (memResults == null) Assert.Fail("In-Memory Dataset did not return a Result Set as expected");
            Console.WriteLine("In-Memory Dataset returned " + memResults.Count + " Result(s)");
            timer.Reset();

            //Do on database
            try
            {
                q.AlgebraOptimisers = this._optimisers;
                timer.Start();
                Object results = this._procDb.ProcessQuery(q);
                timer.Stop();
                Console.WriteLine("ADO Dataset answered query in " + timer.Elapsed);

                if (results is SparqlResultSet)
                {
                    SparqlResultSet dbResults = (SparqlResultSet)results;
                    Console.WriteLine("ADO Dataset returned " + dbResults.Count + " Result(s)");

                    Assert.AreEqual(memResults.Count, dbResults.Count, "Should get same number of results for both datasets");

                    //Print the materialised results
                    NTriplesFormatter formatter = new NTriplesFormatter();
                    foreach (SparqlResult r in dbResults)
                    {
                        Console.WriteLine(r.ToString(formatter));
                    }

                    Assert.AreEqual(memResults, dbResults, "Result Sets should be equal");
                } 
                else 
                {
                    Assert.Fail("ADO Dataset did not return a Result Set as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        private void TestQuery(SparqlParameterizedString query)
        {
            this.TestQuery(query.ToString());
        }

        private SparqlParameterizedString GetBaseQuery()
        {
            if (this._baseQuery == null)
            {
                this._baseQuery = new SparqlParameterizedString();
                this._baseQuery.Namespaces = new NamespaceMapper();
                this._baseQuery.Namespaces.AddNamespace("dnr", new Uri(Configuration.ConfigurationLoader.ConfigurationNamespace));
            }
            return this._baseQuery;
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql1()
        {
            this.EnsureTestData();

            String query = "SELECT * WHERE { ?s a ?type }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql2()
        {
            this.EnsureTestData();

            String query = "SELECT * WHERE { ?s a <http://example.org/noSuchThing> }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql3()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s a ?type . ?s rdfs:domain ?domain }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql4()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s a ?type . ?s rdfs:range ?range }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql5()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s a ?type . ?s rdfs:domain ?domain . ?s rdfs:range ?range }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql6()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql7()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s ?p ?o . FILTER(ISBLANK(?s)) }";
            this.TestQuery(query);
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftSparql8()
        {
            this.EnsureTestData();

            SparqlParameterizedString query = this.GetBaseQuery();
            query.CommandText = "SELECT * WHERE { ?s ?p ?o . FILTER(SAMETERM(?s, dnr:HttpHandler)) }";
            this.TestQuery(query);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this._manager != null) this._manager.Dispose();
        }
    }
}
