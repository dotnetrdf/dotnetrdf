using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Test.Update;

namespace VDS.RDF.Test.Storage
{
    public class AdoTransactionalUpdateTests : TransactionalUpdateTests
    {
        private MicrosoftAdoManager _manager;

        private void EnsureConnection()
        {
            if (this._manager == null)
            {
                this._manager = new MicrosoftAdoManager("adostore", "example", "password");
            }
        }

        protected override ISparqlDataset GetEmptyDataset()
        {
            this.EnsureConnection();

            //Ensure the Store is clear
            DbCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ClearStore";
            cmd.Connection = this._manager.Connection;
            cmd.ExecuteNonQuery();

            return new MicrosoftAdoDataset(this._manager);
        }

        protected override ISparqlDataset GetNonEmptyDataset()
        {
            this.EnsureConnection();

            //Ensure the Store is clear
            DbCommand cmd = this._manager.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ClearStore";
            cmd.Connection = this._manager.Connection;
            cmd.ExecuteNonQuery();

            Graph g = new Graph();
            g.BaseUri = this.TestGraphUri;
            this._manager.SaveGraph(g);

            return new MicrosoftAdoDataset(this._manager);
        }

        [TestCleanup]
        private void Cleanup()
        {
            this._manager.Dispose();
        }
    }
}
