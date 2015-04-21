using System;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /**
     * Constants to access the arg: namespace.
     *
     * @author Holger Knublauch
     */

    public class ARG
    {
        public const String BASE_URI = "http://spinrdf.org/arg";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "arg";

        public readonly static IUriNode PropertyProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "property"));

        public readonly static IUriNode PropertyMaxCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "maxCount"));

        public readonly static IUriNode PropertyMinCount = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "minCount"));
    }
}