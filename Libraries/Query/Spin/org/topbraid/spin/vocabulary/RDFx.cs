using System;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
namespace org.topbraid.spin.vocabulary
{

    /**
     * Defines RDF resources that are not yet in the corresponding Jena class.
     * 
     * @author Holger Knublauch
     */
    public class RDFx
    {

        public readonly static IUriNode ClassHTML = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "HTML"));

        public readonly static IUriNode PropertyLangString = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "langString"));

        public readonly static IUriNode ClassPlainLiteral = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "PlainLiteral"));
    }
}
