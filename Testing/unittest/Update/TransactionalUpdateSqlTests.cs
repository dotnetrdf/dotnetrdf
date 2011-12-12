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
            MicrosoftAdoManager sql = new MicrosoftAdoManager("unit_test", "example", "password");
            sql.DeleteGraph(TestGraphUri);
            return new MicrosoftAdoDataset(sql);
        }

        protected override ISparqlDataset GetNonEmptyDataset()
        {
            MicrosoftAdoManager sql = new MicrosoftAdoManager("unit_test", "example", "password");
            sql.DeleteGraph(TestGraphUri);
            Graph g = new Graph();
            g.BaseUri = TestGraphUri;
            sql.SaveGraph(g);

            return new MicrosoftAdoDataset(sql);
        }
    }
}