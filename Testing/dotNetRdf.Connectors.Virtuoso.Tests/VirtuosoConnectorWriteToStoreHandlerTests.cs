using Xunit;

namespace VDS.RDF.Storage;

[Collection("Virtuoso")]
public class VirtuosoConnectorWriteToStoreHandlerTests : WriteToStoreHandlerTests
{
    [Fact]
    public void CanWriteToStoreHandler()
    {
        TestWriteToStoreHandler(GetConnection());
    }

    [Fact]
    public void CanWriteToDatasetHandler()
    {
        TestWriteToStoreDatasetsHandler(GetConnection());
    }

    [Fact]
    public void HandlerCanWriteBNodesToStore()
    {
        TestWriteToStoreHandlerWithBNodes(GetConnection());
    }

    protected override IStorageProvider GetConnection()
    {
        return VirtuosoConnectorTest.GetConnection();
    }
}