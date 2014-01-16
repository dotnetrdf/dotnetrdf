/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using System;
using VDS.RDF.Query.Spin;
using VDS.RDF;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin.Util;
namespace org.topbraid.spin.util
{

    /**
     * Some static utilities dealing with datatypes and literals.
     * 
     * @author Holger Knublauch
     */
    public class JenaDatatypes
    {

        public static ILiteralNode FALSE = RDFUtil.CreateLiteralNode("false", XSD.boolean);

        public static ILiteralNode TRUE = RDFUtil.CreateLiteralNode("true", XSD.boolean);

        private static HashSet<String> numericDatatypeURIs = new HashSet<String>();

        private static HashSet<String> otherDatatypeURIs = new HashSet<String>();

        static JenaDatatypes()
        {
            numericDatatypeURIs.Add(XSD._decimal.ToString());
            numericDatatypeURIs.Add(XSD.duration.ToString());
            numericDatatypeURIs.Add(XSD.gDay.ToString());
            numericDatatypeURIs.Add(XSD.gMonth.ToString());
            numericDatatypeURIs.Add(XSD.gMonthDay.ToString());
            numericDatatypeURIs.Add(XSD.gYear.ToString());
            numericDatatypeURIs.Add(XSD.gYearMonth.ToString());
            numericDatatypeURIs.Add(XSD.integer.ToString());
            numericDatatypeURIs.Add(XSD.negativeInteger.ToString());
            numericDatatypeURIs.Add(XSD.nonNegativeInteger.ToString());
            numericDatatypeURIs.Add(XSD.nonPositiveInteger.ToString());
            numericDatatypeURIs.Add(XSD.positiveInteger.ToString());
            numericDatatypeURIs.Add(XSD.unsignedByte.ToString());
            numericDatatypeURIs.Add(XSD.unsignedInt.ToString());
            numericDatatypeURIs.Add(XSD.unsignedLong.ToString());
            numericDatatypeURIs.Add(XSD.unsignedShort.ToString());
            numericDatatypeURIs.Add(XSD._byte.ToString());
            numericDatatypeURIs.Add(XSD._double.ToString());
            numericDatatypeURIs.Add(XSD._float.ToString());
            numericDatatypeURIs.Add(XSD._int.ToString());
            numericDatatypeURIs.Add(XSD._long.ToString());
            numericDatatypeURIs.Add(XSD._short.ToString());

            otherDatatypeURIs.Add(XSD.anySimpleType.ToString());
            otherDatatypeURIs.Add(XSD.anyURI.ToString());
            otherDatatypeURIs.Add(XSD.base64Binary.ToString());
            otherDatatypeURIs.Add(XSD.date.ToString());
            otherDatatypeURIs.Add(XSD.dateTime.ToString());
            otherDatatypeURIs.Add(XSD.ENTITY.ToString());
            otherDatatypeURIs.Add(XSD.hexBinary.ToString());
            otherDatatypeURIs.Add(XSD.ID.ToString());
            otherDatatypeURIs.Add(XSD.IDREF.ToString());
            otherDatatypeURIs.Add(XSD.language.ToString());
            otherDatatypeURIs.Add(XSD.Name.ToString());
            otherDatatypeURIs.Add(XSD.NCName.ToString());
            otherDatatypeURIs.Add(XSD.NMTOKEN.ToString());
            otherDatatypeURIs.Add(XSD.normalizedString.ToString());
            otherDatatypeURIs.Add(XSD.NOTATION.ToString());
            otherDatatypeURIs.Add(XSD.QName.ToString());
            otherDatatypeURIs.Add(XSD.time.ToString());
            otherDatatypeURIs.Add(XSD.token.ToString());
            otherDatatypeURIs.Add(XSD.boolean.ToString());
            otherDatatypeURIs.Add(XSD._string.ToString());
            otherDatatypeURIs.Add(RDF.XMLLiteral.ToString());
        }


        public static ILiteralNode createInteger(int value)
        {
            return new NodeFactory().CreateLiteralNode("" + value, XSD.integer);
        }


        /**
         * Gets a List of all datatype URIs.
         * @return a List the datatype URIs
         */
        public static List<String> getDatatypeURIs()
        {
            List<String> list = new List<String>();
            list.AddRange(otherDatatypeURIs);
            list.AddRange(numericDatatypeURIs);
            list.Add(RDFx.PlainLiteral.ToString());
            return list;
        }


        /**
         * Checks if a given URI is a numeric datatype URI.
         * @param datatypeURI  the URI of the datatype to test
         * @return true if so
         */
        public static bool isNumeric(String datatypeURI)
        {
            return numericDatatypeURIs.Contains(datatypeURI);
        }


        /**
         * Checks if a given INode represents a system XSD datatype such as xsd:int.
         * Note: this will not return true on user-defined datatypes or rdfs:Literal.
         * @param node  the node to test
         * @return true if node is a datatype
         */
        public static bool isSystemDatatype(INode node)
        {
            if (node is INode && node is IUriNode)
            {
                String uri = ((INode)node).ToString();
                return isNumeric(uri) || otherDatatypeURIs.Contains(uri);
            }
            else
            {
                return false;
            }
        }
    }
}