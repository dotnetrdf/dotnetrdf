using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage.Async
{
    [TestClass]
    public class AllegroGraphAsync
        : BaseAsyncTests
    {
        protected override IAsyncStorageProvider GetAsyncProvider()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseAllegroGraph))
            {
                Assert.Inconclusive("Test Config marks AllegroGraph as unavailable, cannot run this test");
            }

            return new AllegroGraphConnector(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository));
        }
    }
}
