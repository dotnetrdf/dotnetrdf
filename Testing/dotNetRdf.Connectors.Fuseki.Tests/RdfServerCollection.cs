using Xunit;

namespace VDS.RDF.Storage;

[CollectionDefinition("RdfServer", DisableParallelization = true)]
public class RdfServerCollection : ICollectionFixture<RdfServerFixture>
{
}