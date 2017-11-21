/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Ontology;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin
{
    internal static class XSD
    {
        public static readonly String NS_URI = NamespaceMapper.XMLSCHEMA;
        public static readonly IUriNode string_ = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA +"string"));

        public static readonly IUriNode DatatypeDecimal = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "decimal"));
        public static readonly IUriNode DatatypeDuration = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "duration"));
        public static readonly IUriNode DatatypeGDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gDay"));
        public static readonly IUriNode DatatypeGMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonth"));
        public static readonly IUriNode DatatypeGMonthDay = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gMonthDay"));
        public static readonly IUriNode DatatypeGYear = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYear"));
        public static readonly IUriNode DatatypeGYearMonth = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "gYearMonth"));
        public static readonly IUriNode DatatypeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "integer"));
        public static readonly IUriNode DatatypeNegativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "negativeInteger"));
        public static readonly IUriNode DatatypeNonNegativeInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonNegativeInteger"));
        public static readonly IUriNode DatatypeNonPositiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "nonPositiveInteger"));
        public static readonly IUriNode DatatypePositiveInteger = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "positiveInteger"));
        public static readonly IUriNode DatatypeUnsignedByte = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedByte"));
        public static readonly IUriNode DatatypeUnsignedInt = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedInt"));
        public static readonly IUriNode DatatypeUnsignedLong = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedLong"));
        public static readonly IUriNode DatatypeUnsignedShort = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "unsignedShort"));
        public static readonly IUriNode DatatypeByte = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "byte"));
        public static readonly IUriNode DatatypeDouble = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "double"));
        public static readonly IUriNode DatatypeFloat = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "float"));
        public static readonly IUriNode DatatypeInt = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "int"));
        public static readonly IUriNode DatatypeLong = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "long"));
        public static readonly IUriNode DatatypeShort = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "short"));

        public static readonly IUriNode DatatypeAnySimpleType = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anySimpleType"));
        public static readonly IUriNode DatatypeAnyURI = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "anyURI"));
        public static readonly IUriNode DatatypeBase64Binary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "base64Binary"));
        public static readonly IUriNode DatatypeDate = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "date"));
        public static readonly IUriNode DatatypeDateTime = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "dateTime"));
        public static readonly IUriNode DatatypeENTITY = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ENTITY"));
        public static readonly IUriNode DatatypeHexBinary = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "hexBinary"));
        public static readonly IUriNode DatatypeID = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "ID"));
        public static readonly IUriNode DatatypeIDREF = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "IDREF"));
        public static readonly IUriNode PropertyLanguage = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "language"));
        public static readonly IUriNode DatatypeName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "Name"));
        public static readonly IUriNode DatatypeNCName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NCName"));
        public static readonly IUriNode DatatypeNMTOKEN = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NMTOKEN"));
        public static readonly IUriNode DatatypeNormalizedString = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "normalizedString"));
        public static readonly IUriNode DatatypeNOTATION = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "NOTATION"));
        public static readonly IUriNode DatatypeQName = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "QName"));
        public static readonly IUriNode DatatypeTime = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "time"));
        public static readonly IUriNode DatatypeToken = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "token"));
        public static readonly IUriNode DatatypeBoolean = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));
   
    }

    internal static class RDF
    {
        public static readonly String NS_URI = NamespaceMapper.RDF;

        public static readonly IUriNode ClassResource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "Resource"));
        public static readonly IUriNode ClassHTML = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "HTML"));
        public static readonly IUriNode ClassPlainLiteral = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "PlainLiteral"));
        public static readonly IUriNode ClassXMLLiteral = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "XMLLiteral"));

        public static readonly IUriNode PropertyLangString = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "langString"));

        public static readonly IUriNode PropertyGraphLabel = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "graphLabel"));
        public static readonly IUriNode PropertySubject = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "subject"));
        public static readonly IUriNode PropertyPredicate = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "predicate"));
        public static readonly IUriNode PropertyObject = RDFUtil.CreateUriNode(UriFactory.Create(RDF.NS_URI + "object"));

        public static readonly IUriNode Nil = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
        public static readonly IUriNode PropertyFirst = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "first"));
        public static readonly IUriNode PropertyRest = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "rest"));
        public static readonly IUriNode PropertyType = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyType));
    }

    internal static class RDFS
    {
        public static readonly String NS_URI = NamespaceMapper.RDFS;

        public static readonly IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfsClass));
        public static readonly IUriNode PropertyComment = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyComment));
        public static readonly IUriNode ClassDatatype = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Datatype"));
        public static readonly IUriNode PropertyDomain = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDomain));
        public static readonly IUriNode PropertyIsDefinedBy = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIsDefinedBy));
        public static readonly IUriNode PropertyLabel = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyLabel));
        public static readonly IUriNode ClassLiteral = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Literal"));
        public static readonly IUriNode ClassProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.RdfProperty));
        public static readonly IUriNode PropertyRange = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyRange));
        public static readonly IUriNode ClassResource = RDFUtil.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "Resource"));
        public static readonly IUriNode PropertySeeAlso = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySeeAlso));
        public static readonly IUriNode PropertySubClassOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubClassOf));
        public static readonly IUriNode PropertySubPropertyOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySubPropertyOf));
    }

    internal static class OWL
    {
        public static readonly String NS_URI = NamespaceMapper.OWL;


        public static readonly IUriNode PropertyAllValuesFrom = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "allValuesFrom"));
        public static readonly IUriNode ClassAnnotationProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlAnnotationProperty));
        public static readonly IUriNode Class = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlClass));
        public static readonly IUriNode PropertyCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "cardinality"));
        public static readonly IUriNode ClassDatatypeProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
        public static readonly IUriNode PropertyDifferentFrom = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDifferentFrom));
        public static readonly IUriNode PropertyDisjointWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyDisjointWith));
        public static readonly IUriNode PropertyEquivalentClass = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentClass));
        public static readonly IUriNode PropertyEquivalentProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyEquivalentProperty));
        public static readonly IUriNode ClassFunctionalProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "FunctionalProperty"));
        public static readonly IUriNode PropertyImports = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyImports));
        public static readonly IUriNode PropertyIncompatibleWith = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyIncompatibleWith));
        public static readonly IUriNode PropertyInverseOf = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyInverseOf));
        public static readonly IUriNode PropertyMaxCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "maxCardinality"));
        public static readonly IUriNode PropertyMinCardinality = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "minCardinality"));
        public static readonly IUriNode Nothing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Nothing"));
        public static readonly IUriNode ClassObjectProperty = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlObjectProperty));
        public static readonly IUriNode PropertyOnProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "onProperty"));
        public static readonly IUriNode ClassOntology = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.OwlOntology));
        public static readonly IUriNode PropertyPriorVersion = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyPriorVersion));
        public static readonly IUriNode PropertySameAs = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertySameAs));
        public static readonly IUriNode SymmetricProperty = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "SymmetricProperty"));
        public static readonly IUriNode Thing = RDFUtil.CreateUriNode(UriFactory.Create(NS_URI + "Thing"));
        public static readonly IUriNode PropertyVersionInfo = RDFUtil.CreateUriNode(UriFactory.Create(OntologyHelper.PropertyVersionInfo));
    }

}
