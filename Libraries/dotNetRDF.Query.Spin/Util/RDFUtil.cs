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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.Util
{
    internal static class RDFUtil
    {

        public readonly static UriComparer uriComparer = new UriComparer();

        public readonly static IEqualityComparer<Triple> tripleEqualityComparer = new TripleEqualityComparer();

        public readonly static NodeFactory nodeFactory = new NodeFactory();

        #region UriComparison shortcuts
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

        #region NodeFactory shortcuts

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
            numericDatatypeURIs.Add(XSD.DatatypeDecimal.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeDuration.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGDay.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGMonth.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGMonthDay.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGYear.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGYearMonth.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNegativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNonNegativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNonPositiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypePositiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedByte.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedInt.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedLong.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedShort.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeByte.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeDouble.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeFloat.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeInt.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeLong.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeShort.Uri);

            otherDatatypeURIs.Add(XSD.DatatypeAnySimpleType.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeAnyURI.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeBase64Binary.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeDate.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeDateTime.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeENTITY.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeHexBinary.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeID.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeIDREF.Uri);
            otherDatatypeURIs.Add(XSD.PropertyLanguage.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNCName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNMTOKEN.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNormalizedString.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNOTATION.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeQName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeTime.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeToken.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeBoolean.Uri);
            otherDatatypeURIs.Add(XSD.string_.Uri);
            otherDatatypeURIs.Add(RDF.ClassXMLLiteral.Uri);

            isFullyInitialized = true;
        }

        internal static ILiteralNode createInteger(int value)
        {
            return CreateLiteralNode("" + value, XSD.DatatypeInteger.Uri);
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
            list.Add(RDF.ClassPlainLiteral.Uri);
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
