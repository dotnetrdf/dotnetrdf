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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class SequenceTests
    {
        private SparqlQueryParser _queryParser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
