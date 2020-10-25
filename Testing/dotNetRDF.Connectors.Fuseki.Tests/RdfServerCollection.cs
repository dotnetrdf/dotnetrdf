using Xunit;

namespace VDS.RDF.Storage
{
    [CollectionDefinition("RdfServer")]
    public class RdfServerCollection : ICollectionFixture<RdfServerFixture>
    {
    }
}