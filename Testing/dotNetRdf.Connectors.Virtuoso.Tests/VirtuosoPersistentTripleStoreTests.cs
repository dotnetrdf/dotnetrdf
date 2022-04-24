using Xunit;

namespace VDS.RDF.Storage
{
    [Collection("Virtuoso")]
    public class VirtuosoPersistentTripleStoreTests : PersistentTripleStoreTests
    {
        protected override IStorageProvider GetStorageProvider()
        {
            return VirtuosoTest.GetConnection();
        }
    }
}
