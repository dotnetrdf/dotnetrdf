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

        public readonly static IUriNode DatatypeDecimal = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "decimal"));
        public readonly static IUriNode DatatypeDuration = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "duration"));
        public readonly static IUriNode DatatypeGDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gDay"));
        public readonly static IUriNode DatatypeGMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonth"));
        public readonly static IUriNode DatatypeGMonthDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonthDay"));
        public readonly static IUriNode DatatypeGYear = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYear"));
        public readonly static IUriNode DatatypeGYearMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYearMonth"));
        public readonly static IUriNode DatatypeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "integer"));
        public readonly static IUriNode DatatypeNegativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "negativeInteger"));
        public readonly static IUriNode DatatypeNonNegativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonNegativeInteger"));
        public readonly static IUriNode DatatypeNonPositiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonPositiveInteger"));
        public readonly static IUriNode DatatypePositiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "positiveInteger"));
        public readonly static IUriNode DatatypeUnsignedByte = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedByte"));
        public readonly static IUriNode DatatypeUnsignedInt = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedInt"));
        public readonly static IUriNode DatatypeUnsignedLong = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedLong"));
        public readonly static IUriNode DatatypeUnsignedShort = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedShort"));
        public readonly static IUriNode DatatypeByte = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "byte"));
        public readonly static IUriNode DatatypeDouble = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "double"));
        public readonly static IUriNode DatatypeFloat = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "float"));
        public readonly static IUriNode DatatypeInt = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "int"));
        public readonly static IUriNode DatatypeLong = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "long"));
        public readonly static IUriNode DatatypeShort = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "short"));

        public readonly static IUriNode DatatypeAnySimpleType = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anySimpleType"));
        public readonly static IUriNode DatatypeAnyURI = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anyURI"));
        public readonly static IUriNode DatatypeBase64Binary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "base64Binary"));
        public readonly static IUriNode DatatypeDate = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "date"));
        public readonly static IUriNode DatatypeDateTime = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "dateTime"));
        public readonly static IUriNode DatatypeENTITY = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ENTITY"));
        public readonly static IUriNode DatatypeHexBinary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "hexBinary"));
        public readonly static IUriNode DatatypeID = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ID"));
        public readonly static IUriNode DatatypeIDREF = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "IDREF"));
        public readonly static IUriNode PropertyLanguage = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "language"));
        public readonly static IUriNode DatatypeName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "Name"));
        public readonly static IUriNode DatatypeNCName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NCName"));
        public readonly static IUriNode DatatypeNMTOKEN = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NMTOKEN"));
        public readonly static IUriNode DatatypeNormalizedString = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "normalizedString"));
        public readonly static IUriNode DatatypeNOTATION = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NOTATION"));
        public readonly static IUriNode DatatypeQName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "QName"));
        public readonly static IUriNode DatatypeTime = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "time"));
        public readonly static IUriNode DatatypeToken = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "token"));
        public readonly static IUriNode DatatypeBoolean = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));
   
    }

    public static class RDF
    {
        public readonly static String NS_URI = NamespaceMapper.RDF;

        public readonly static IUriNode ClassResource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "Resource"));
        public readonly static IUriNode ClassHTML = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "HTML"));
        public readonly static IUriNode ClassPlainLiteral = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "PlainLiteral"));
        public readonly static IUriNode ClassXMLLiteral = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "XMLLiteral"));

        public readonly static IUriNode PropertyLangString = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "langString"));

        public readonly static IUriNode PropertyGraphLabel = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "graphLabel"));
        public readonly static IUriNode PropertySubject = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "subject"));
        public readonly static IUriNode PropertyPredicate = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "predicate"));
        public readonly static IUriNode PropertyObject = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "object"));

        public readonly static IUriNode Nil = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
        public readonly static IUriNode PropertyFirst = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "first"));
        public readonly static IUriNode PropertyRest = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "rest"));
        public readonly static IUriNode PropertyType = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
    }

    public static class RDFS
    {
        public readonly static String NS_URI = NamespaceMapper.RDFS;

        public readonly static IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsClass));
        public readonly static IUriNode PropertyComment = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyComment));
        public readonly static IUriNode ClassDatatype = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Datatype"));
        public readonly static IUriNode PropertyDomain = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDomain));
        public readonly static IUriNode PropertyIsDefinedBy = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIsDefinedBy));
        public readonly static IUriNode PropertyLabel = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyLabel));
        public readonly static IUriNode ClassLiteral = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Literal"));
        public readonly static IUriNode ClassProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfProperty));
        public readonly static IUriNode PropertyRange = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyRange));
        public readonly static IUriNode ClassResource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Resource"));
        public readonly static IUriNode PropertySeeAlso = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySeeAlso));
        public readonly static IUriNode PropertySubClassOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
        public readonly static IUriNode PropertySubPropertyOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf));
    }

    public static class OWL
    {
        public readonly static String NS_URI = NamespaceMapper.OWL;


        public readonly static IUriNode PropertyAllValuesFrom = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "allValuesFrom"));
        public readonly static IUriNode ClassAnnotationProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlAnnotationProperty));
        public readonly static IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlClass));
        public readonly static IUriNode PropertyCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cardinality"));
        public readonly static IUriNode ClassDatatypeProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
        public readonly static IUriNode PropertyDifferentFrom = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDifferentFrom));
        public readonly static IUriNode PropertyDisjointWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
        public readonly static IUriNode PropertyEquivalentClass = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
        public readonly static IUriNode PropertyEquivalentProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentProperty));
        public readonly static IUriNode ClassFunctionalProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "FunctionalProperty"));
        public readonly static IUriNode PropertyImports = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyImports));
        public readonly static IUriNode PropertyIncompatibleWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIncompatibleWith));
        public readonly static IUriNode PropertyInverseOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf));
        public readonly static IUriNode PropertyMaxCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "maxCardinality"));
        public readonly static IUriNode PropertyMinCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "minCardinality"));
        public readonly static IUriNode Nothing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Nothing"));
        public readonly static IUriNode ClassObjectProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlObjectProperty));
        public readonly static IUriNode PropertyOnProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "onProperty"));
        public readonly static IUriNode ClassOntology = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology));
        public readonly static IUriNode PropertyPriorVersion = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyPriorVersion));
        public readonly static IUriNode PropertySameAs = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySameAs));
        public readonly static IUriNode SymmetricProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SymmetricProperty"));
        public readonly static IUriNode Thing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Thing"));
        public readonly static IUriNode PropertyVersionInfo = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyVersionInfo));
    }

}
