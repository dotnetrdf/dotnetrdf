using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class BindingsTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        [TestMethod]
        public void SparqlBindingsEmpty()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car } BINDINGS {}";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }

        [TestMethod]
        public void SparqlBindingsEmpty2()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car } BINDINGS { () }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }

        [TestMethod]
        public void SparqlBindingsEmpty3()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car } BINDINGS { () () }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }

        [TestMethod]
        public void SparqlBindingsSimple()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ?type } BINDINGS ?type { ( ex:Car ) }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }

        private void TestBindings(ISparqlDataset data, SparqlParameterizedString queryWithBindings, SparqlParameterizedString queryWithoutBindings)
        {
            this.TestBindings(data, queryWithBindings.ToString(), queryWithoutBindings.ToString());
        }

        private void TestBindings(ISparqlDataset data, String queryWithBindings, String queryWithoutBindings)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery bindingsQuery = this._parser.ParseFromString(queryWithBindings);
            SparqlQuery noBindingsQuery = this._parser.ParseFromString(queryWithoutBindings);

            SparqlResultSet bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;
            SparqlResultSet noBindingsResults = processor.ProcessQuery(noBindingsQuery) as SparqlResultSet;

            if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");
            if (noBindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Non-Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();
            Console.WriteLine("Non-Bindings Results");
            TestTools.ShowResults(noBindingsResults);
            Console.WriteLine();

            Assert.AreEqual(noBindingsResults, bindingsResults, "Result Sets should have been equal");
        }

        private void TestBindings(ISparqlDataset data, String queryWithBindings)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery bindingsQuery = this._parser.ParseFromString(queryWithBindings);

            SparqlResultSet bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;

            if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();

            Assert.IsTrue(bindingsResults.IsEmpty, "Result Set should be empty");
        }

        private ISparqlDataset GetTestData()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            dataset.AddGraph(g);

            return dataset;
        }
    }
}
