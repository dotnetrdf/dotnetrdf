using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Query
{
    [TestClass]
    public class SequenceTests
    {
        private SparqlQueryParser _queryParser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        [TestMethod]
        public void SparqlSequenceUpdateThenQuery1()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            LeviathanUpdateProcessor updateProcessor = new LeviathanUpdateProcessor(dataset);
            LeviathanQueryProcessor queryProcessor = new LeviathanQueryProcessor(dataset);
            Assert.AreEqual(1, dataset.Graphs.Count());

            SparqlUpdateCommandSet updates = this._updateParser.ParseFromFile("sparql\\protocol\\update_dataset_default_graph.ru");
            updateProcessor.ProcessCommandSet(updates);

            Assert.AreEqual(3, dataset.Graphs.Count());
            Assert.AreEqual(1, dataset[UriFactory.Create("http://example.org/protocol-update-dataset-test/")].Triples.Count());

            SparqlQuery query = this._queryParser.ParseFromFile("sparql\\protocol\\update_dataset_default_graph.rq");

            ISparqlAlgebra algebra = query.ToAlgebra();
            Console.WriteLine(algebra.ToString());

            SparqlResultSet results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(SparqlResultsType.Boolean, results.ResultsType);
            Assert.IsTrue(results.Result);
        }

        [TestMethod]
        public void SparqlSequenceUpdateThenQuery2()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            LeviathanUpdateProcessor updateProcessor = new LeviathanUpdateProcessor(dataset);
            LeviathanQueryProcessor queryProcessor = new LeviathanQueryProcessor(dataset);
            Assert.AreEqual(1, dataset.Graphs.Count());

            SparqlUpdateCommandSet updates = this._updateParser.ParseFromFile("sparql\\protocol\\update_dataset_default_graphs.ru");
            updateProcessor.ProcessCommandSet(updates);

            Assert.AreEqual(5, dataset.Graphs.Count());
            Assert.AreEqual(2, dataset[UriFactory.Create("http://example.org/protocol-update-dataset-graphs-test/")].Triples.Count());

            SparqlQuery query = this._queryParser.ParseFromFile("sparql\\protocol\\update_dataset_default_graphs.rq");

            ISparqlAlgebra algebra = query.ToAlgebra();
            Console.WriteLine(algebra.ToString());

            SparqlResultSet results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(SparqlResultsType.Boolean, results.ResultsType);
            Assert.IsTrue(results.Result);
        }

        [TestMethod]
        public void SparqlSequenceUpdateThenQuery3()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            LeviathanUpdateProcessor updateProcessor = new LeviathanUpdateProcessor(dataset);
            LeviathanQueryProcessor queryProcessor = new LeviathanQueryProcessor(dataset);
            Assert.AreEqual(1, dataset.Graphs.Count());

            SparqlUpdateCommandSet updates = this._updateParser.ParseFromFile("sparql\\protocol\\update_dataset_named_graphs.ru");
            updateProcessor.ProcessCommandSet(updates);

            Assert.AreEqual(5, dataset.Graphs.Count());
            Assert.AreEqual(2, dataset[UriFactory.Create("http://example.org/protocol-update-dataset-named-graphs-test/")].Triples.Count());

            SparqlQuery query = this._queryParser.ParseFromFile("sparql\\protocol\\update_dataset_named_graphs.rq");

            ISparqlAlgebra algebra = query.ToAlgebra();
            Console.WriteLine(algebra.ToString());

            SparqlResultSet results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(SparqlResultsType.Boolean, results.ResultsType);
            Assert.IsTrue(results.Result);
        }

        [TestMethod]
        public void SparqlSequenceUpdateThenQuery4()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            LeviathanUpdateProcessor updateProcessor = new LeviathanUpdateProcessor(dataset);
            LeviathanQueryProcessor queryProcessor = new LeviathanQueryProcessor(dataset);
            Assert.AreEqual(1, dataset.Graphs.Count());

            SparqlUpdateCommandSet updates = this._updateParser.ParseFromFile("sparql\\protocol\\update_dataset_full.ru");
            updateProcessor.ProcessCommandSet(updates);

            Console.WriteLine(updates.ToString());

            Assert.AreEqual(5, dataset.Graphs.Count());
            Assert.AreEqual(2, dataset[UriFactory.Create("http://example.org/protocol-update-dataset-full-test/")].Triples.Count());

            SparqlQuery query = this._queryParser.ParseFromFile("sparql\\protocol\\update_dataset_full.rq");

            ISparqlAlgebra algebra = query.ToAlgebra();
            Console.WriteLine(algebra.ToString());

            SparqlResultSet results = queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(SparqlResultsType.Boolean, results.ResultsType);
            Assert.IsTrue(results.Result);
        }
    }
}
