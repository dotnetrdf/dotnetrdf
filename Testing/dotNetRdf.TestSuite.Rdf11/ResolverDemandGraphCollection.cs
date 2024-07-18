using System;

namespace VDS.RDF.TestSuite.Rdf11;

public class ResolverDemandGraphCollection(Func<Uri, IGraph> resolver) : BaseDemandGraphCollection
{
    protected override IGraph LoadOnDemand(Uri graphUri)
    {
        return resolver(graphUri);
    }
}