namespace VDS.RDF.Storage
{
    public class VirtuosoPersistentTripleStoreTests : PersistentTripleStoreTests
    {
        protected override IStorageProvider GetStorageProvider()
        {
            return VirtuosoTest.GetConnection();
        }
    }
}
