
using VDS.RDF;
using System;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.LibraryOntology
{

    /**
     * Vocabulary for http://spinrdf.org/spra
     *
     * @author Holger Knublauch
     */
    public class SPRA
    {

        public const String BASE_URI = "http://spinrdf.org/spra";

        public const String NS_URI = BASE_URI + "#";

        public const String PREFIX = "spra";


        public readonly static IUriNode ClassTable = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Table"));

    }
}
