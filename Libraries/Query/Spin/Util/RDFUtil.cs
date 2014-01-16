using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using org.topbraid.spin.vocabulary;
using org.topbraid.spin.model;

namespace VDS.RDF.Query.Spin.Util
{
    internal static class RDFUtil
    {

        public readonly static UriComparer uriComparer = new UriComparer();
        public readonly static NodeFactory nodeFactory = new NodeFactory();

        #region "UriComparison shortcuts"
        internal static bool sameTerm(Uri uri1, Uri uri2)
        {
            return uriComparer.Equals(uri1, uri2);
        }

        internal static bool sameTerm(Uri uri1, INode uri2)
        {
            if (uri2 is IResource) uri2 = ((IResource)uri2).getSource();
            if (!(uri2 is IUriNode)) return false;
            return uriComparer.Equals(uri1, ((IUriNode)uri2).Uri);
        }

        internal static bool sameTerm(INode uri1, Uri uri2)
        {
            if (uri1 is IResource) uri1 = ((IResource)uri1).getSource();
            if (!(uri1 is IUriNode)) return false;
            return uriComparer.Equals(((IUriNode)uri1).Uri, uri2);
        }

        internal static bool sameTerm(INode uri1, INode uri2)
        {
            if (uri1 is IResource) uri1 = ((IResource)uri1).getSource();
            if (uri2 is IResource) uri2 = ((IResource)uri2).getSource();
            if (!(uri1 is IUriNode) || !(uri2 is IUriNode)) return false;
            return uriComparer.Equals(((IUriNode)uri1).Uri, ((IUriNode)uri2).Uri);
        }

        #endregion

        #region "NodeFactory shortcuts"

        internal static IUriNode CreateUriNode(Uri uri)
        {
            return nodeFactory.CreateUriNode(uri);
        }

        internal static ILiteralNode CreateLiteralNode(String literal)
        {
            return nodeFactory.CreateLiteralNode(literal);
        }

        internal static ILiteralNode CreateLiteralNode(String literal, String langspec)
        {
            return nodeFactory.CreateLiteralNode(literal, langspec);
        }

        internal static ILiteralNode CreateLiteralNode(String literal, Uri datatype)
        {
            return nodeFactory.CreateLiteralNode(literal, datatype);
        }

        internal static ILiteralNode FALSE = CreateLiteralNode("false", UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));
        internal static ILiteralNode TRUE = CreateLiteralNode("true", UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));

        internal static HashSet<Uri> numericDatatypeURIs = new HashSet<Uri>(uriComparer);

        internal static HashSet<Uri> otherDatatypeURIs = new HashSet<Uri>(uriComparer);

        private static bool isFullyInitialized = false;
        private static void Initialize() {
            if (isFullyInitialized) return;
            numericDatatypeURIs.Add(XSD.decimal_.Uri);
            numericDatatypeURIs.Add(XSD.duration.Uri);
            numericDatatypeURIs.Add(XSD.gDay.Uri);
            numericDatatypeURIs.Add(XSD.gMonth.Uri);
            numericDatatypeURIs.Add(XSD.gMonthDay.Uri);
            numericDatatypeURIs.Add(XSD.gYear.Uri);
            numericDatatypeURIs.Add(XSD.gYearMonth.Uri);
            numericDatatypeURIs.Add(XSD.integer.Uri);
            numericDatatypeURIs.Add(XSD.negativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.nonNegativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.nonPositiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.positiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.unsignedByte.Uri);
            numericDatatypeURIs.Add(XSD.unsignedInt.Uri);
            numericDatatypeURIs.Add(XSD.unsignedLong.Uri);
            numericDatatypeURIs.Add(XSD.unsignedShort.Uri);
            numericDatatypeURIs.Add(XSD.byte_.Uri);
            numericDatatypeURIs.Add(XSD.double_.Uri);
            numericDatatypeURIs.Add(XSD.float_.Uri);
            numericDatatypeURIs.Add(XSD.int_.Uri);
            numericDatatypeURIs.Add(XSD.long_.Uri);
            numericDatatypeURIs.Add(XSD.short_.Uri);

            otherDatatypeURIs.Add(XSD.anySimpleType.Uri);
            otherDatatypeURIs.Add(XSD.anyURI.Uri);
            otherDatatypeURIs.Add(XSD.base64Binary.Uri);
            otherDatatypeURIs.Add(XSD.date.Uri);
            otherDatatypeURIs.Add(XSD.dateTime.Uri);
            otherDatatypeURIs.Add(XSD.ENTITY.Uri);
            otherDatatypeURIs.Add(XSD.hexBinary.Uri);
            otherDatatypeURIs.Add(XSD.ID.Uri);
            otherDatatypeURIs.Add(XSD.IDREF.Uri);
            otherDatatypeURIs.Add(XSD.language.Uri);
            otherDatatypeURIs.Add(XSD.Name.Uri);
            otherDatatypeURIs.Add(XSD.NCName.Uri);
            otherDatatypeURIs.Add(XSD.NMTOKEN.Uri);
            otherDatatypeURIs.Add(XSD.normalizedString.Uri);
            otherDatatypeURIs.Add(XSD.NOTATION.Uri);
            otherDatatypeURIs.Add(XSD.QName.Uri);
            otherDatatypeURIs.Add(XSD.time.Uri);
            otherDatatypeURIs.Add(XSD.token.Uri);
            otherDatatypeURIs.Add(XSD.boolean.Uri);
            otherDatatypeURIs.Add(XSD.string_.Uri);
            otherDatatypeURIs.Add(RDF.XMLLiteral.Uri);

            isFullyInitialized = true;
        }

        internal static ILiteralNode createInteger(int value)
        {
            return CreateLiteralNode("" + value, XSD.integer.Uri);
        }


        /**
         * Gets a List of all datatype URIs.
         * @return a List the datatype URIs
         */
        internal static List<Uri> getDatatypeURIs()
        {
            Initialize();
            List<Uri> list = new List<Uri>();
            list.AddRange(otherDatatypeURIs);
            list.AddRange(numericDatatypeURIs);
            list.Add(RDFx.ClassPlainLiteral.Uri);
            return list;
        }


        /**
         * Checks if a given URI is a numeric datatype URI.
         * @param datatypeURI  the URI of the datatype to test
         * @return true if so
         */
        internal static bool isNumeric(Uri datatypeURI)
        {
            Initialize();
            return numericDatatypeURIs.Contains(datatypeURI);
        }


        /**
         * Checks if a given INode represents a system XSD datatype such as xsd:int.
         * Note: this will not return true on user-defined datatypes or rdfs:Literal.
         * @param node  the node to test
         * @return true if node is a datatype
         */
        internal static bool isSystemDatatype(INode node)
        {
            Initialize();
            if (node is IUriNode)
            {
                return isNumeric(((IUriNode)node).Uri) || otherDatatypeURIs.Contains(((IUriNode)node).Uri);
            }
            else
            {
                return false;
            }
        }

        #endregion

    }
}
