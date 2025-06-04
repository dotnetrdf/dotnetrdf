using System;
using System.Linq;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
using Xunit;

namespace VDS.RDF.Storage;

public class StardogStoreFixture : IDisposable
{
    public StardogConnector Connector { get; }

    public StardogStoreFixture()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
        var connector = new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
            TestConfigManager.GetSetting(TestConfigManager.StardogDatabase),
            TestConfigManager.GetSetting(TestConfigManager.StardogUser),
            TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        var testStore = TestConfigManager.GetSetting(TestConfigManager.StardogDatabase);
        if (!connector.ParentServer.ListStores().Contains(testStore))
        {
            connector.ParentServer.CreateStore(new StardogMemTemplate(testStore));
        }

        Connector = connector;
    }

    public StardogServer GetServer()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
        return new StardogServer(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
            TestConfigManager.GetSetting(TestConfigManager.StardogUser),
            TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
    }


    public void Dispose()
    {

    }
}
