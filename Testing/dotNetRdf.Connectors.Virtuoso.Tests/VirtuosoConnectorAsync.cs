using Xunit;

namespace VDS.RDF.Storage
{
    [Collection("Virtuoso")]
    public class VirtuosoConnectorAsync
        : BaseAsyncTests
    {
        protected override IAsyncStorageProvider GetAsyncProvider()
        {
            return VirtuosoConnectorTest.GetConnection();
        }
    }
}
