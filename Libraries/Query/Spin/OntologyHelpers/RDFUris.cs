using System;
using VDS.RDF.Ontology;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin
{
    public static class XSD
    {
        public readonly static String NS_URI = NamespaceMapper.XMLSCHEMA;
        public readonly static IUriNode DatatypeString = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI +"string"));

        public readonly static IUriNode DatatypeDecimal = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "decimal"));
        public readonly static IUriNode DatatypeDuration = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "duration"));
        public readonly static IUriNode DatatypeGDay = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gDay"));
        public readonly static IUriNode DatatypeGMonth = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gMonth"));
        public readonly static IUriNode DatatypeGMonthDay = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gMonthDay"));
        public readonly static IUriNode DatatypeGYear = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gYear"));
        public readonly static IUriNode DatatypeGYearMonth = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "gYearMonth"));
        public readonly static IUriNode DatatypeInteger = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "integer"));
        public readonly static IUriNode DatatypeNegativeInteger = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "negativeInteger"));
        public readonly static IUriNode DatatypeNonNegativeInteger = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "nonNegativeInteger"));
        public readonly static IUriNode DatatypeNonPositiveInteger = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "nonPositiveInteger"));
        public readonly static IUriNode DatatypePositiveInteger = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "positiveInteger"));
        public readonly static IUriNode DatatypeUnsignedByte = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "unsignedByte"));
        public readonly static IUriNode DatatypeUnsignedInt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "unsignedInt"));
        public readonly static IUriNode DatatypeUnsignedLong = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "unsignedLong"));
        public readonly static IUriNode DatatypeUnsignedShort = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "unsignedShort"));
        public readonly static IUriNode DatatypeByte = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "byte"));
        public readonly static IUriNode DatatypeDouble = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "double"));
        public readonly static IUriNode DatatypeFloat = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "float"));
        public readonly static IUriNode DatatypeInt = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "int"));
        public readonly static IUriNode DatatypeLong = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "long"));
        public readonly static IUriNode DatatypeShort = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "short"));

        public readonly static IUriNode DatatypeAnySimpleType = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "anySimpleType"));
        public readonly static IUriNode DatatypeAnyURI = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "anyURI"));
        public readonly static IUriNode DatatypeBase64Binary = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "base64Binary"));
        public readonly static IUriNode DatatypeDate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "date"));
        public readonly static IUriNode DatatypeDateTime = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "dateTime"));
        public readonly static IUriNode DatatypeENTITY = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ENTITY"));
        public readonly static IUriNode DatatypeHexBinary = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hexBinary"));
        public readonly static IUriNode DatatypeID = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "ID"));
        public readonly static IUriNode DatatypeIDREF = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "IDREF"));
        public readonly static IUriNode PropertyLanguage = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "language"));
        public readonly static IUriNode DatatypeName = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Name"));
        public readonly static IUriNode DatatypeNCName = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NCName"));
        public readonly static IUriNode DatatypeNMTOKEN = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NMTOKEN"));
        public readonly static IUriNode DatatypeNormalizedString = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "normalizedString"));
        public readonly static IUriNode DatatypeNOTATION = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "NOTATION"));
        public readonly static IUriNode DatatypeQName = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "QName"));
        public readonly static IUriNode DatatypeTime = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "time"));
        public readonly static IUriNode DatatypeToken = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "token"));
        public readonly static IUriNode DatatypeBoolean = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "boolean"));
   
    }

    public static class RDF
    {
        public readonly static String NS_URI = NamespaceMapper.RDF;

        //public readonly static IUriNode ClassResource = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Resource"));
        public readonly static IUriNode ClassProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Property")); 
        public readonly static IUriNode ClassHTML = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "HTML"));
        public readonly static IUriNode ClassPlainLiteral = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "PlainLiteral"));
        public readonly static IUriNode ClassXMLLiteral = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "XMLLiteral"));

        public readonly static IUriNode PropertyLangString = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "langString"));

        public readonly static IUriNode PropertyGraphLabel = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "graphLabel"));
        public readonly static IUriNode PropertySubject = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "subject"));
        public readonly static IUriNode PropertyPredicate = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "predicate"));
        public readonly static IUriNode PropertyObject = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "object"));

        public readonly static IUriNode Nil = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "nil"));
        public readonly static IUriNode PropertyFirst = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "first"));
        public readonly static IUriNode PropertyRest = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "rest"));
        public readonly static IUriNode PropertyType = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
    }

    public static class RDFS
    {
        public readonly static String NS_URI = NamespaceMapper.RDFS;

        public readonly static IUriNode Class = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsClass));
        public readonly static IUriNode PropertyComment = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyComment));
        public readonly static IUriNode ClassDatatype = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Datatype"));
        public readonly static IUriNode PropertyDomain = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDomain));
        public readonly static IUriNode PropertyIsDefinedBy = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIsDefinedBy));
        public readonly static IUriNode PropertyLabel = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyLabel));
        public readonly static IUriNode ClassLiteral = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Literal"));
        public readonly static IUriNode PropertyRange = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyRange));
        public readonly static IUriNode ClassResource = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Resource"));
        public readonly static IUriNode PropertySeeAlso = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySeeAlso));
        public readonly static IUriNode PropertySubClassOf = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
        public readonly static IUriNode PropertySubPropertyOf = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf));
    }

    public static class OWL
    {
        public readonly static String NS_URI = NamespaceMapper.OWL;

        public readonly static IUriNode PropertyAllValuesFrom = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "allValuesFrom"));
        public readonly static IUriNode ClassAnnotationProperty = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.OwlAnnotationProperty));
        public readonly static IUriNode Class = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.OwlClass));
        public readonly static IUriNode PropertyCardinality = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "cardinality"));
        public readonly static IUriNode ClassDatatypeProperty = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
        public readonly static IUriNode PropertyDifferentFrom = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDifferentFrom));
        public readonly static IUriNode PropertyDisjointWith = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
        public readonly static IUriNode PropertyEquivalentClass = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
        public readonly static IUriNode PropertyEquivalentProperty = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentProperty));
        public readonly static IUriNode ClassFunctionalProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "FunctionalProperty"));
        public readonly static IUriNode PropertyImports = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyImports));
        public readonly static IUriNode PropertyIncompatibleWith = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIncompatibleWith));
        public readonly static IUriNode PropertyInverseOf = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf));
        public readonly static IUriNode PropertyMaxCardinality = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "maxCardinality"));
        public readonly static IUriNode PropertyMinCardinality = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "minCardinality"));
        public readonly static IUriNode Nothing = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Nothing"));
        public readonly static IUriNode ClassObjectProperty = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.OwlObjectProperty));
        public readonly static IUriNode PropertyOnProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "onProperty"));
        public readonly static IUriNode ClassOntology = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology));
        public readonly static IUriNode PropertyPriorVersion = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyPriorVersion));
        public readonly static IUriNode PropertySameAs = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySameAs));
        public readonly static IUriNode SymmetricProperty = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "SymmetricProperty"));
        public readonly static IUriNode Thing = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Thing"));
        public readonly static IUriNode PropertyVersionInfo = RDFHelper.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyVersionInfo));
    }

}
