using System;

namespace VDS.RDF.TestSuite.W3C;

public class ResolverDemandGraphCollection(Func<Uri, IGraph> resolver) : BaseDemandGraphCollection
{
    protected override IGraph LoadOnDemand(Uri graphUri)
    {
        return resolver(graphUri);
    }
}