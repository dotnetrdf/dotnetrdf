using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public class TransactionalUpdateQuadTests
        : TransactionalUpdateTests
    {
        protected override ISparqlDataset GetEmptyDataset()
        {
            return new InMemoryQuadDataset();
        }

        protected virtual ISparqlDataset GetNonEmptyDataset()
        {
            InMemoryQuadDataset dataset = new InMemoryQuadDataset();

            Graph g = new Graph();
            g.BaseUri = TestGraphUri;
            dataset.AddGraph(g);

            return dataset;
        }
    }
}
