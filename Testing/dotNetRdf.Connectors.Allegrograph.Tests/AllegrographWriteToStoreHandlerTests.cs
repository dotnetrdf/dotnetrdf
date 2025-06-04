using Xunit;

namespace VDS.RDF.Storage;

[Collection("AllegroGraph Test Collection")]
public class AllegrographWriteToStoreHandlerTests : WriteToStoreHandlerTests
{
    [Fact]
    public void CanWriteToStoreHandler()
    {
        TestWriteToStoreHandler(GetConnection());
    }

    protected override IStorageProvider GetConnection()
    {
        return AllegroGraphTests.GetConnection();
    }
}
