
using System;
using VDS.RDF;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.vocabulary
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


        public readonly static IUriNode PropertyProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "property"));

        public readonly static IUriNode PropertyMaxCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "maxCount"));

        public readonly static IUriNode PropertyMinCount = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "minCount"));
    }
}