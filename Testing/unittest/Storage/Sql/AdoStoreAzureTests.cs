using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage.Sql
{
    [TestClass]
    public class AdoStoreAzureTests
    {
        private const String AzureDatabaseServer = "l08zahpqb0";
        private const String AzureDatabase = "rdfazure";
        private const String AzureUsername = "example";
        private const String AzurePassword = "ex_azure_2011_login";

        private IGenericIOManager GetConnection()
        {
            return new MicrosoftAdoManager(AzureDatabaseServer + ".database.windows.net", AzureDatabase, AzureUsername + "@" + AzureDatabaseServer, AzurePassword, true);
        }

        [TestMethod]
        public void StorageAdoMicrosoftAzureSaveGraph()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            IGenericIOManager manager = this.GetConnection();
            try
            {
                manager.SaveGraph(g);

                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);

                Assert.AreEqual(g, h, "Graphs should be equal");

            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                manager.Dispose();
            }
        }
    }
}
