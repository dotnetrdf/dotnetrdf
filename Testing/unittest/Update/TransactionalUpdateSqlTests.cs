using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public class TransactionalUpdateSqlTests : TransactionalUpdateTests
    {
        protected override ISparqlDataset GetEmptyDataset()
        {
            MicrosoftSqlStoreManager sql = new MicrosoftSqlStoreManager("unit_test", "example", "password");
            sql.DeleteGraph(TestGraphUri);
            sql.Flush();
            return new SqlDataset(sql);
        }

        protected override ISparqlDataset GetNonEmptyDataset()
        {
            MicrosoftSqlStoreManager sql = new MicrosoftSqlStoreManager("unit_test", "example", "password");
            sql.DeleteGraph(TestGraphUri);
            Graph g = new Graph();
            g.BaseUri = TestGraphUri;
            sql.SaveGraph(g);
            sql.Flush();

            return new SqlDataset(sql);
        }
    }
}