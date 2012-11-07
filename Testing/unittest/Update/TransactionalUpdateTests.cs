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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    /// <summary>
    /// Provides a suite of unit tests for SPARQL Update Transactions run against the <see cref="InMemoryDataset">InMemoryDataset</see>
    /// </summary>
    /// <remarks>
    /// To run the same tests against an alternative <see cref="ISparqlDataset">ISparqlDataset</see> implementation extend this class, override the <see cref="TransactionalUpdateTests.GetEmptyDataset">GetEmptyDataset()</see> and <see cref="TransactionalUpdateTests.GetNonEmptyDataset()">GetNonEmptyDataset()</see> methods.  The latter should contain a single empty graph with the URI set to the constant <see cref="TransactionalUpdateTests.TestGraphUri">TestGraphUri</see>
    /// </remarks>
    [TestClass]
    public class TransactionalUpdateTests
    {
        protected readonly Uri TestGraphUri = new Uri("http://example.org/graph");
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        protected virtual ISparqlDataset GetEmptyDataset()
        {
            return new InMemoryDataset();
        }

        protected virtual ISparqlDataset GetNonEmptyDataset()
        {
            InMemoryDataset dataset = new InMemoryDataset();

            Graph g = new Graph();
            g.BaseUri = TestGraphUri;
            dataset.AddGraph(g);

            return dataset;
        }

        #region Test Driver Methods

        private void TestCreateGraphCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist");
        }

        private void TestCreateGraphRollback()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");

        }

        private void TestCreateGraphRollbackWithoutAutoCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.AutoCommit = false;
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not throw a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist as the Transaction has not been committed yet as Auto-Commit is off");

            //Try to Flush() which should error
            try
            {
                processor.Flush();
                Assert.Fail("Did not throw a SparqlUpdateException as expected on call to Flush()");
            }
            catch (SparqlUpdateException upEx)
            {
                Console.WriteLine("Threw error when attempting to Flush() as expected");
                TestTools.ReportError("Update Exception", upEx);
            }
            
            //Now discard
            processor.Discard();
            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");

        }

        private void TestDropGraphCommit()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist");
        }

        private void TestDropGraphRollback()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist as the Discard() should ensure it was still in the Dataset");
        }

        private void TestLoadCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist");
        }

        private void TestLoadRollback()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
        }

        private void TestCreateDropSequenceCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist");
        }

        private void TestCreateDropSequenceCommit2()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist");
        }

        private void TestCreateDropSequenceRollback()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Expected SPARQL Update Exception was not thrown");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist");

        }

        private void TestCreateDropSequenceRollback2()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Expected SPARQL Update Exception was not thrown");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should not exist");
        }

        private void TestInsertDataThenDropCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Flush() should cause it to be removed from the Dataset");
        }

        private void TestInsertDataThenDropRollback()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist as the Discard() should cause it to be added back to the Dataset");
            Assert.IsTrue(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should cause the inserted triple to be removed");
        }

        private void TestCreateThenInsertDataRollback()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsFalse(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
        }

        private void TestDropThenInsertDataRollback()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
            Assert.IsTrue(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should have reversed the INSERT DATA");
        }

        private void TestInsertDataThenDeleteDataCommit()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Flush() should persist first the insert then the delete");
        }

        private void TestInsertDataThenDeleteDataRollback()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();

            String updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsTrue(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should reverse first the delete then the insert");
        }

        private void TestDeleteDataThenInsertDataCommit()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();
            IGraph g = dataset.GetModifiableGraph(TestGraphUri);
            g.Assert(new Triple(g.CreateUriNode(new Uri("ex:subj")), g.CreateUriNode(new Uri("ex:pred")), g.CreateUriNode(new Uri("ex:obj"))));
            dataset.Flush();

            String updates = "DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsFalse(dataset[TestGraphUri].IsEmpty, "Graph should not be empty as the Flush() should persist first the delete then the insert so the end results should be the triple still being in the Graph");
        }

        private void TestDeleteDataThenInsertDataRollback()
        {
            ISparqlDataset dataset = this.GetNonEmptyDataset();
            IGraph g = dataset.GetModifiableGraph(TestGraphUri);
            g.Assert(new Triple(g.CreateUriNode(new Uri("ex:subj")), g.CreateUriNode(new Uri("ex:pred")), g.CreateUriNode(new Uri("ex:obj"))));

            String updates = "DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Did not thrown a SparqlUpdateException as expected");
            }
            catch (SparqlUpdateException upEx)
            {
                TestTools.ReportError("Update Exception", upEx);
            }

            Assert.IsFalse(dataset[TestGraphUri].IsEmpty, "Graph should not be empty as the Discard() should reverse first the insert then the delete so the end results should be the triple still being in the Graph");
        }

        private void TestInsertDataSequenceCommit()
        {
            ISparqlDataset dataset = this.GetEmptyDataset();

            String updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(updates);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(dataset.HasGraph(TestGraphUri), "Graph should exist");
            Assert.AreEqual(1, dataset[TestGraphUri].Triples.Count, "Expected 1 Triple");
        }

        #endregion

        #region CREATE GRAPH Tests

        [TestMethod]
        public void SparqlUpdateTransactionsCreateGraphCommit()
        {
            TestCreateGraphCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsCreateGraphRollback()
        {
            TestCreateGraphRollback();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsCreateGraphRollbackWithoutAutoCommit()
        {
            TestCreateGraphRollbackWithoutAutoCommit();
        }

        #endregion

        #region DROP GRAPH Tests

        [TestMethod]
        public void SparqlUpdateTransactionsDropGraphCommit()
        {
            TestDropGraphCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsDropGraphRollback()
        {
            TestDropGraphRollback();
        }

        #endregion

        #region LOAD Tests

        [TestMethod]
        public void SparqlUpdateTransactionsLoadCommit()
        {
            TestLoadCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsLoadRollback()
        {
            TestLoadRollback();
        }

        #endregion

        #region CREATE and DROP GRAPH Tests

        [TestMethod]
        public void SparqlUpdateTransactionsCreateDropSequenceCommit()
        {
            TestCreateDropSequenceCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsCreateDropSequenceCommit2()
        {
            TestCreateDropSequenceCommit2();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsCreateDropSequenceRollback()
        {
            TestCreateDropSequenceRollback();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsCreateDropSequenceRollback2()
        {
            TestCreateDropSequenceRollback2();
        }

        #endregion

        #region INSERT DATA and DROP GRAPH Tests

        [TestMethod]
        public void SparqlUpdateTransactionsInsertDataThenDropCommit()
        {
            this.TestInsertDataThenDropCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsInsertDataThenDropRollback()
        {
            this.TestInsertDataThenDropRollback();
        }

        #endregion

        #region CREATE GRAPH and INSERT DATA Tests

        [TestMethod]
        public void SparqlUpdateTransactionsCreateThenInsertRollback()
        {
            this.TestCreateThenInsertDataRollback();
        }

        #endregion

        #region DROP GRAPH and INSERT DATA Tests

        [TestMethod]
        public void SparqlUpdateTransactionsDropThenInsertRollback()
        {
            this.TestDropThenInsertDataRollback();
        }

        #endregion

        #region INSERT DATA and DELETE DATA Tests

        public void SparqlUpdateTransactionsInsertDataThenDeleteDataCommit()
        {
            this.TestInsertDataThenDeleteDataCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsInsertDataThenDeleteDataRollback()
        {
            this.TestInsertDataThenDeleteDataRollback();
        }

        #endregion

        #region DELETE DATA and INSERT DATA Tests

        [TestMethod]
        public void SparqlUpdateTransactionsDeleteDataThenInsertDataCommit()
        {
            this.TestDeleteDataThenInsertDataCommit();
        }

        [TestMethod]
        public void SparqlUpdateTransactionsDeleteDataThenInsertDataRollback()
        {
            this.TestDeleteDataThenInsertDataRollback();
        }

        #endregion

        #region INSERT DATA and INSERT DATA

        [TestMethod]
        public void SparqlUpdateTransactionsInsertDataSequenceCommit()
        {
            this.TestInsertDataSequenceCommit();
        }

        #endregion

    }
}
