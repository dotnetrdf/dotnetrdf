using System;

namespace VDS.RDF.TestSuite.Rdf11;

public class ResolverDemandGraphCollection(Func<Uri, IGraph> _resolver) : BaseDemandGraphCollection
{
    protected override IGraph LoadOnDemand(Uri graphUri)
    {
        return _resolver(graphUri);
    }
}