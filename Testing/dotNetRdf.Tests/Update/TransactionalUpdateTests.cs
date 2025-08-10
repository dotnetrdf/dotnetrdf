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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Update;

/// <summary>
/// Provides a suite of unit tests for SPARQL Update Transactions run against the <see cref="InMemoryDataset">InMemoryDataset</see>
/// </summary>
/// <remarks>
/// To run the same tests against an alternative <see cref="ISparqlDataset">ISparqlDataset</see> implementation extend this class, override the <see cref="TransactionalUpdateTests.GetEmptyDataset">GetEmptyDataset()</see> and <see cref="TransactionalUpdateTests.GetNonEmptyDataset()">GetNonEmptyDataset()</see> methods.  The latter should contain a single empty graph with the URI set to the constant <see cref="TransactionalUpdateTests.TestGraphUri">TestGraphUri</see>
/// </remarks>

public class TransactionalUpdateTests
{
    protected readonly IRefNode TestGraphUri = new UriNode(new Uri("http://example.org/graph"));
    private readonly SparqlUpdateParser _parser = new SparqlUpdateParser();

    protected virtual ISparqlDataset GetEmptyDataset()
    {
        return new InMemoryDataset();
    }

    protected virtual ISparqlDataset GetNonEmptyDataset()
    {
        var dataset = new InMemoryDataset();

        var g = new Graph(TestGraphUri);
        dataset.AddGraph(g);

        return dataset;
    }

    #region Test Driver Methods

    private void TestCreateGraphCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist");
    }

    private void TestCreateGraphRollback()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");

    }

    private void TestCreateGraphRollbackWithoutAutoCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset,
            options => { options.AutoCommit = false;});
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not throw a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist as the Transaction has not been committed yet as Auto-Commit is off");

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
        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");

    }

    private void TestDropGraphCommit()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist");
    }

    private void TestDropGraphRollback()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist as the Discard() should ensure it was still in the Dataset");
    }

    private void TestLoadCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist");
    }

    private void TestLoadRollback()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "LOAD <http://www.dotnetrdf.org/configuration#> INTO GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
    }

    private void TestCreateDropSequenceCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist");
    }

    private void TestCreateDropSequenceCommit2()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist");
    }

    private void TestCreateDropSequenceRollback()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Expected SPARQL Update Exception was not thrown");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist");

    }

    private void TestCreateDropSequenceRollback2()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; CREATE GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        Assert.Throws<SparqlUpdateException>(() => processor.ProcessCommandSet(cmds));
        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should not exist");
    }

    private void TestInsertDataThenDropCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; DROP GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Flush() should cause it to be removed from the Dataset");
    }

    private void TestInsertDataThenDropRollback()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; DROP GRAPH <" + TestGraphUri.ToString() + ">; DROP GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist as the Discard() should cause it to be added back to the Dataset");
        Assert.True(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should cause the inserted triple to be removed");
    }

    private void TestCreateThenInsertDataRollback()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "CREATE GRAPH <" + TestGraphUri.ToString() + ">; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.False(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
    }

    private void TestDropThenInsertDataRollback()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "DROP GRAPH <" + TestGraphUri.ToString() + ">; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subject> <ex:predicate> <ex:object> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should not exist as the Discard() should cause it to be removed from the Dataset");
        Assert.True(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should have reversed the INSERT DATA");
    }

    private void TestInsertDataThenDeleteDataCommit()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.True(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Flush() should persist first the insert then the delete");
    }

    private void TestInsertDataThenDeleteDataRollback()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();

        var updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.True(dataset[TestGraphUri].IsEmpty, "Graph should be empty as the Discard() should reverse first the delete then the insert");
    }

    private void TestDeleteDataThenInsertDataCommit()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();
        IGraph g = dataset.GetModifiableGraph(TestGraphUri);
        g.Assert(new Triple(g.CreateUriNode(new Uri("ex:subj")), g.CreateUriNode(new Uri("ex:pred")), g.CreateUriNode(new Uri("ex:obj"))));
        dataset.Flush();

        var updates = "DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.False(dataset[TestGraphUri].IsEmpty, "Graph should not be empty as the Flush() should persist first the delete then the insert so the end results should be the triple still being in the Graph");
    }

    private void TestDeleteDataThenInsertDataRollback()
    {
        ISparqlDataset dataset = GetNonEmptyDataset();
        IGraph g = dataset.GetModifiableGraph(TestGraphUri);
        g.Assert(new Triple(g.CreateUriNode(new Uri("ex:subj")), g.CreateUriNode(new Uri("ex:pred")), g.CreateUriNode(new Uri("ex:obj"))));

        var updates = "DELETE DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }; CREATE GRAPH <" + TestGraphUri.ToString() + ">";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        try
        {
            processor.ProcessCommandSet(cmds);
            Assert.Fail("Did not thrown a SparqlUpdateException as expected");
        }
        catch (SparqlUpdateException upEx)
        {
            TestTools.ReportError("Update Exception", upEx);
        }

        Assert.False(dataset[TestGraphUri].IsEmpty, "Graph should not be empty as the Discard() should reverse first the insert then the delete so the end results should be the triple still being in the Graph");
    }

    private void TestInsertDataSequenceCommit()
    {
        ISparqlDataset dataset = GetEmptyDataset();

        var updates = "INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { } }; INSERT DATA { GRAPH <" + TestGraphUri.ToString() + "> { <ex:subj> <ex:pred> <ex:obj> } }";

        SparqlUpdateCommandSet cmds = _parser.ParseFromString(updates);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.True(dataset.HasGraph(TestGraphUri), "Graph should exist");
        Assert.Equal(1, dataset[TestGraphUri].Triples.Count);
    }

    #endregion

    #region CREATE GRAPH Tests

    [Fact]
    public void SparqlUpdateTransactionsCreateGraphCommit()
    {
        TestCreateGraphCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsCreateGraphRollback()
    {
        TestCreateGraphRollback();
    }

    [Fact]
    public void SparqlUpdateTransactionsCreateGraphRollbackWithoutAutoCommit()
    {
        TestCreateGraphRollbackWithoutAutoCommit();
    }

    #endregion

    #region DROP GRAPH Tests

    [Fact]
    public void SparqlUpdateTransactionsDropGraphCommit()
    {
        TestDropGraphCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsDropGraphRollback()
    {
        TestDropGraphRollback();
    }

    #endregion

    #region LOAD Tests

    [Fact(Skip="Remote configuration not currently available")]
    public void SparqlUpdateTransactionsLoadCommit()
    {
        TestLoadCommit();
    }

    [Fact(Skip = "Remote configuration not currently available")]
    public void SparqlUpdateTransactionsLoadRollback()
    {
        TestLoadRollback();
    }

    #endregion

    #region CREATE and DROP GRAPH Tests

    [Fact]
    public void SparqlUpdateTransactionsCreateDropSequenceCommit()
    {
        TestCreateDropSequenceCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsCreateDropSequenceCommit2()
    {
        TestCreateDropSequenceCommit2();
    }

    [Fact]
    public void SparqlUpdateTransactionsCreateDropSequenceRollback()
    {
        TestCreateDropSequenceRollback();
    }

    [Fact]
    public void SparqlUpdateTransactionsCreateDropSequenceRollback2()
    {
        TestCreateDropSequenceRollback2();
    }

    #endregion

    #region INSERT DATA and DROP GRAPH Tests

    [Fact]
    public void SparqlUpdateTransactionsInsertDataThenDropCommit()
    {
        TestInsertDataThenDropCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsInsertDataThenDropRollback()
    {
        TestInsertDataThenDropRollback();
    }

    #endregion

    #region CREATE GRAPH and INSERT DATA Tests

    [Fact]
    public void SparqlUpdateTransactionsCreateThenInsertRollback()
    {
        TestCreateThenInsertDataRollback();
    }

    #endregion

    #region DROP GRAPH and INSERT DATA Tests

    [Fact]
    public void SparqlUpdateTransactionsDropThenInsertRollback()
    {
        TestDropThenInsertDataRollback();
    }

    #endregion

    #region INSERT DATA and DELETE DATA Tests

    [Fact]
    public void SparqlUpdateTransactionsInsertDataThenDeleteDataCommit()
    {
        TestInsertDataThenDeleteDataCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsInsertDataThenDeleteDataRollback()
    {
        TestInsertDataThenDeleteDataRollback();
    }

    #endregion

    #region DELETE DATA and INSERT DATA Tests

    [Fact]
    public void SparqlUpdateTransactionsDeleteDataThenInsertDataCommit()
    {
        TestDeleteDataThenInsertDataCommit();
    }

    [Fact]
    public void SparqlUpdateTransactionsDeleteDataThenInsertDataRollback()
    {
        TestDeleteDataThenInsertDataRollback();
    }

    #endregion

    #region INSERT DATA and INSERT DATA

    [Fact]
    public void SparqlUpdateTransactionsInsertDataSequenceCommit()
    {
        TestInsertDataSequenceCommit();
    }

    #endregion

}
