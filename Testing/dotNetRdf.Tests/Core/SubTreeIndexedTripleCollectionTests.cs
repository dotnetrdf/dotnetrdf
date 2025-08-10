namespace VDS.RDF;

public class SubTreeIndexedTripleCollectionTests : AbstractTripleCollectionTests
{
    protected override BaseTripleCollection GetInstance()
    {
        return new SubTreeIndexedTripleCollection();
    }
}
