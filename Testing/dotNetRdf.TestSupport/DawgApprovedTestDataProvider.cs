using System;

namespace VDS.RDF;

public class DawgApprovedTestDataProvider : ManifestTestDataProvider
{
    public DawgApprovedTestDataProvider(Uri baseUri, string manifestPath):base(baseUri, manifestPath, DawgApprovedTestFilter){}

    public static bool DawgApprovedTestFilter(IGraph g, INode testNode)
    {
        return g.ContainsTriple(
            new Triple(
                testNode,
                g.CreateUriNode(
                    g.UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-dawg#approval")),
                g.CreateUriNode(
                    g.UriFactory.Create("http://www.w3.org/2001/sw/DataAccess/tests/test-dawg#Approved"))));
    }
}