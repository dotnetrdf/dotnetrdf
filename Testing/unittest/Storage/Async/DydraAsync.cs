using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage.Async
{
    [TestClass]
    public class DydraAsync
        : BaseAsyncTests
    {
        protected override IAsyncStorageProvider GetAsyncProvider()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseDydra))
            {
                Assert.Inconclusive("Test Config marks Dydra as unavailable, cannot run this test");
            }

            return new DydraConnector(TestConfigManager.GetSetting(TestConfigManager.DydraAccount), TestConfigManager.GetSetting(TestConfigManager.DydraRepository), TestConfigManager.GetSetting(TestConfigManager.DydraApiKey));
        }
    }
}
