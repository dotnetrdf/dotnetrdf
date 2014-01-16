using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Ontology;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin
{
    public static class XSD
    {
        public readonly static String NS_URI = NamespaceMapper.XMLSCHEMA;
        public readonly static IUriNode string_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA +"string"));

        public readonly static IUriNode decimal_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "decimal"));
        public readonly static IUriNode duration = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "duration"));
        public readonly static IUriNode gDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gDay"));
        public readonly static IUriNode gMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonth"));
        public readonly static IUriNode gMonthDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonthDay"));
        public readonly static IUriNode gYear = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYear"));
        public readonly static IUriNode gYearMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYearMonth"));
        public readonly static IUriNode integer = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "integer"));
        public readonly static IUriNode negativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "negativeInteger"));
        public readonly static IUriNode nonNegativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonNegativeInteger"));
        public readonly static IUriNode nonPositiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonPositiveInteger"));
        public readonly static IUriNode positiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "positiveInteger"));
        public readonly static IUriNode unsignedByte = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedByte"));
        public readonly static IUriNode unsignedInt = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedInt"));
        public readonly static IUriNode unsignedLong = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedLong"));
        public readonly static IUriNode unsignedShort = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedShort"));
        public readonly static IUriNode byte_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "byte"));
        public readonly static IUriNode double_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "double"));
        public readonly static IUriNode float_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "float"));
        public readonly static IUriNode int_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "int"));
        public readonly static IUriNode long_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "long"));
        public readonly static IUriNode short_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "short"));

        public readonly static IUriNode anySimpleType = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anySimpleType"));
        public readonly static IUriNode anyURI = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anyURI"));
        public readonly static IUriNode base64Binary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "base64Binary"));
        public readonly static IUriNode date = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "date"));
        public readonly static IUriNode dateTime = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "dateTime"));
        public readonly static IUriNode ENTITY = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ENTITY"));
        public readonly static IUriNode hexBinary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "hexBinary"));
        public readonly static IUriNode ID = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ID"));
        public readonly static IUriNode IDREF = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "IDREF"));
        public readonly static IUriNode language = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "language"));
        public readonly static IUriNode Name = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "Name"));
        public readonly static IUriNode NCName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NCName"));
        public readonly static IUriNode NMTOKEN = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NMTOKEN"));
        public readonly static IUriNode normalizedString = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "normalizedString"));
        public readonly static IUriNode NOTATION = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NOTATION"));
        public readonly static IUriNode QName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "QName"));
        public readonly static IUriNode time = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "time"));
        public readonly static IUriNode token = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "token"));
        public readonly static IUriNode boolean = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));
   
    }

    public static class RDF
    {
        public readonly static String NS_URI = NamespaceMapper.RDF;

        public readonly static IUriNode Resource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "Resource"));
        public readonly static IUriNode type = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
        public readonly static IUriNode XMLLiteral = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "XMLLiteral"));

        public readonly static IUriNode first = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "first"));
        public readonly static IUriNode rest = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "rest"));
        public readonly static IUriNode nil = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
    }

    public static class RDFS
    {
        public readonly static String NS_URI = NamespaceMapper.RDFS;

        public readonly static IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsClass));
        public readonly static IUriNode comment = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyComment));
        public readonly static IUriNode Datatype = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Datatype"));
        public readonly static IUriNode domain = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDomain));
        public readonly static IUriNode isDefinedBy = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIsDefinedBy));
        public readonly static IUriNode label = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyLabel));
        public readonly static IUriNode Literal = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Literal"));
        public readonly static IUriNode Property = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsProperty));
        public readonly static IUriNode range = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyRange));
        public readonly static IUriNode Resource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Resource"));
        public readonly static IUriNode seeAlso = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySeeAlso));
        public readonly static IUriNode subClassOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
        public readonly static IUriNode subPropertyOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf));
    }

    public static class OWL
    {
        public readonly static String NS_URI = NamespaceMapper.OWL;


        public readonly static IUriNode allValuesFrom = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "allValuesFrom"));
        public readonly static IUriNode AnnotationProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlAnnotationProperty));
        public readonly static IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlClass));
        public readonly static IUriNode cardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cardinality"));
        public readonly static IUriNode DatatypeProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
        public readonly static IUriNode differentFrom = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDifferentFrom));
        public readonly static IUriNode disjointWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
        public readonly static IUriNode equivalentClass = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
        public readonly static IUriNode equivalentProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentProperty));
        public readonly static IUriNode FunctionalProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "FunctionalProperty"));
        public readonly static IUriNode imports = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyImports));
        public readonly static IUriNode incompatibleWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIncompatibleWith));
        public readonly static IUriNode inverseOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf));
        public readonly static IUriNode maxCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "maxCardinality"));
        public readonly static IUriNode minCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "minCardinality"));
        public readonly static IUriNode Nothing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Nothing"));
        public readonly static IUriNode ObjectProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlObjectProperty));
        public readonly static IUriNode onProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "onProperty"));
        public readonly static IUriNode Ontology = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology));
        public readonly static IUriNode priorVersion = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyPriorVersion));
        public readonly static IUriNode sameAs = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySameAs));
        public readonly static IUriNode SymmetricProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SymmetricProperty"));
        public readonly static IUriNode Thing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Thing"));
        public readonly static IUriNode versionInfo = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyVersionInfo));
    }

}
