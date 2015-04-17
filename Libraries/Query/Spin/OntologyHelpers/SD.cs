
using System;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{

    /**
     * Vocabulary for http://www.w3.org/TR/sparql11-service-description/
     */
    public static class SD
    {

        public readonly static String NS_URI = "http://www.w3.org/ns/sparql-service-description#";

        public readonly static IUriNode ClassDataset = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Dataset"));
        public readonly static IUriNode ClassFeature = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Feature"));
        public readonly static IUriNode ClassGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Graph"));
        public readonly static IUriNode ClassGraphCollection = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "GraphCollection"));

        public readonly static IUriNode PropertyAvailableGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "availableGraph"));
        public readonly static IUriNode PropertyHasFeature = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasFeature"));
        public readonly static IUriNode PropertyNamedGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "namedGraph"));

    }
}
