namespace VDS.RDF;

public class TripleCollectionTests: AbstractTripleCollectionTests
{
    protected override BaseTripleCollection GetInstance()
    {
        return new TripleCollection();
    }
}
