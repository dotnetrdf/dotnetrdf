using Xunit;

namespace VDS.RDF.Storage;

[Collection("Virtuoso")]
public class VirtuosoConnectorPersistentTripleStoreTests : PersistentTripleStoreTests
{
    protected override IStorageProvider GetStorageProvider()
    {
        return VirtuosoConnectorTest.GetConnection();
    }
}