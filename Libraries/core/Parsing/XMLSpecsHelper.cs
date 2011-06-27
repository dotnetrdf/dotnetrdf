/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class which contains a set of Functions which model Name and Character validations as laid
    /// out in the W3C XML and XML Namespaces specification
    /// </summary>
    /// <remarks>
    /// These are needed in the XML/RDF Parser
    /// 
    /// Also contains the Date Time format string used to format .Net's DateTime type into a String whose format conforms to the XML Schema Datatypes specification
    /// 
    /// 
    /// </remarks>
    /// <seealso>http://www.w3.org/TR/REC-xml/</seealso>
    /// <seealso>http://www.w3.org/TR/REC-xml-names/</seealso>
    /// <seealso>http://www.w3.org/TR/xmlschema-2/</seealso>
    public static class XmlSpecsHelper
    {
        /// <summary>
        /// Namespace for XML
        /// </summary>
        public const String NamespaceXml = "http://www.w3.org/XML/1998/namespace";
        /// <summary>
        /// Namespace for XML Namespaces
        /// </summary>
        public const String NamespaceXmlNamespaces = "http://www.w3.org/2000/xmlns/";
        /// <summary>
        /// Namespace for XML Schema
        /// </summary>
        public const String NamespaceXmlSchema = "http://www.w3.org/2001/XMLSchema#";

        /// <summary>
        /// Date Time Formatting string which meets the specified format for xsd:dateTime
        /// </summary>
        /// <remarks>
        /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format
        /// </remarks>
        public const String XmlSchemaDateTimeFormat = "yyyy-MM-dd\\THH:mm:ssK";

        /// <summary>
        /// Date Time Formatting string which meets the specified format for xsd:date
        /// </summary>
        /// <remarks>
        /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format
        /// </remarks>
        public const String XmlSchemaDateFormat = "yyyy-MM-ddK";

        /// <summary>
        /// Date Time Formatting string which meets the the specified format for xsd:time
        /// </summary>
        /// <remarks>
        /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format
        /// </remarks>
        public const String XmlSchemaTimeFormat = "HH:mm:ssK";



        /// <summary>
        /// Data Type Uri Constants for XML Schema Data Types
        /// </summary>
        public const String XmlSchemaDataTypeAnyUri = NamespaceXmlSchema + "anyURI",
                            XmlSchemaDataTypeBase64Binary = NamespaceXmlSchema + "base64Binary",
                            XmlSchemaDataTypeBoolean = NamespaceXmlSchema + "boolean",
                            XmlSchemaDataTypeByte = NamespaceXmlSchema + "byte",
                            XmlSchemaDataTypeDate = NamespaceXmlSchema + "date",
                            XmlSchemaDataTypeDateTime = NamespaceXmlSchema + "dateTime",
                            XmlSchemaDataTypeDayTimeDuration = NamespaceXmlSchema + "dayTimeDuration",
                            XmlSchemaDataTypeDuration = NamespaceXmlSchema + "duration",
                            XmlSchemaDataTypeDecimal = NamespaceXmlSchema + "decimal",
                            XmlSchemaDataTypeDouble = NamespaceXmlSchema + "double",
                            XmlSchemaDataTypeFloat = NamespaceXmlSchema + "float",
                            XmlSchemaDataTypeHexBinary = NamespaceXmlSchema + "hexBinary",
                            XmlSchemaDataTypeInt = NamespaceXmlSchema + "int",
                            XmlSchemaDataTypeInteger = NamespaceXmlSchema + "integer",
                            XmlSchemaDataTypeLong = NamespaceXmlSchema + "long",
                            XmlSchemaDataTypeNegativeInteger = NamespaceXmlSchema + "negativeInteger",
                            XmlSchemaDataTypeNonNegativeInteger = NamespaceXmlSchema + "nonNegativeInteger",
                            XmlSchemaDataTypeNonPositiveInteger = NamespaceXmlSchema + "nonPositiveInteger",
                            XmlSchemaDataTypePositiveInteger = NamespaceXmlSchema + "positiveInteger",
                            XmlSchemaDataTypeShort = NamespaceXmlSchema + "short",
                            XmlSchemaDataTypeTime = NamespaceXmlSchema + "time",
                            XmlSchemaDataTypeString = NamespaceXmlSchema + "string",
                            XmlSchemaDataTypeUnsignedByte = NamespaceXmlSchema + "unsignedByte",
                            XmlSchemaDataTypeUnsignedInt = NamespaceXmlSchema + "unsignedInt",
                            XmlSchemaDataTypeUnsignedLong = NamespaceXmlSchema + "unsignedLong",
                            XmlSchemaDataTypeUnsignedShort = NamespaceXmlSchema + "unsignedShort";

        /// <summary>
        /// Array of Constants for Data Types that are supported by the Literal Node CompareTo method
        /// </summary>
        public static String[] SupportedTypes = new String[] {
            XmlSchemaDataTypeAnyUri,
            XmlSchemaDataTypeBase64Binary,
            XmlSchemaDataTypeBoolean,
            XmlSchemaDataTypeByte,
            XmlSchemaDataTypeDate,
            XmlSchemaDataTypeDateTime,
            /*XmlSchemaDataTypeDayTimeDuration,*/
            XmlSchemaDataTypeDuration,
            XmlSchemaDataTypeDecimal,
            XmlSchemaDataTypeDouble,
            XmlSchemaDataTypeFloat,
            XmlSchemaDataTypeHexBinary,
            XmlSchemaDataTypeInt,
            XmlSchemaDataTypeInteger,
            XmlSchemaDataTypeLong,
            XmlSchemaDataTypeNegativeInteger,
            XmlSchemaDataTypeNonNegativeInteger,
            XmlSchemaDataTypeNonPositiveInteger,
            XmlSchemaDataTypePositiveInteger,
            XmlSchemaDataTypeShort,
            XmlSchemaDataTypeString,
            XmlSchemaDataTypeUnsignedByte,
            XmlSchemaDataTypeUnsignedInt,
            XmlSchemaDataTypeUnsignedLong,
            XmlSchemaDataTypeUnsignedShort/*,
            XmlSchemaDataTypeXmlLiteral*/
        };

        #region Name Validation

        /// <summary>
        /// Returns whether a String is a Name as defined by the W3C XML Specification
        /// </summary>
        /// <param name="name">String to test</param>
        /// <returns></returns>
        public static bool IsName(String name)
        {
            //Get the Characters
            char[] cs = name.ToCharArray();

            //Check first Character is a valid NameStartChar
            if (!IsNameStartChar(cs[0]))
            {
                return false;
            }

            //Check rest of Characters
            if (cs.Length > 1)
            {
                for (int i = 1; i < cs.Length; i++)
                {
                    //Must be a valid NCNameChar
                    if (!IsNameChar(cs[i]))
                    {
                        return false;
                    }
                }
            }

            //If we get here the String is OK
            return true;
        }

        /// <summary>
        /// Returns whether a String is a NCName as defined by the W3C XML Namespaces Specification
        /// </summary>
        /// <param name="name">String to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml-names/#NT-NCName</see>
        public static bool IsNCName(String name)
        {
            //Get the Characters
            char[] cs = name.ToCharArray();

            if (cs.Length == 0) return false;

            //Check first Character is a valid NCNameStartChar
            if (!IsNCNameStartChar(cs[0]))
            {
                return false;
            }

            //Check rest of Characters
            if (cs.Length > 1)
            {
                for (int i = 1; i < cs.Length; i++)
                {
                    //Must be a valid NCNameChar
                    if (!IsNCNameChar(cs[i]))
                    {
                        return false;
                    }
                }
            }

            //If we get here the String is OK
            return true;
        }

        /// <summary>
        /// Returns whether a Character is a NCNameStartChar as defined by the W3C XML Namespaces Specification
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml-names/#NT-NCNameStartChar</see>
        public static bool IsNCNameStartChar(char c)
        {
            //Can be a letter or an underscore
            return (c == '_' || IsLetter(c));
        }

        /// <summary>
        /// Returns whether a Character is a NCNameChar as defined by the W3C XML Namespaces Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml-names/#NT-NCNameChar</see>
        public static bool IsNCNameChar(char c)
        {
            //Can be a name char but not a colon
            if (c == ':')
            {
                return false;
            }
            else
            {
                return IsNameChar(c);
            }
        }

        /// <summary>
        /// Returns whether a Character is a NameChar as defined by the W3C XML Specification
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml/#NT-NameChar</see>
        public static bool IsNameChar(char c)
        {
            if (IsNameStartChar(c))
            {
                //NameStartChar's allowed
                return true;
            }
            else if (Char.IsDigit(c))
            {
                //Digits allowed
                return true;
            }
            else if (c == '.' || c == '-')
            {
                //Period and Hyphen allowed
                return true;
            }
            else if (c == 0xB7) {
                //This Hex Character allowed
                return true;
            }
            else if (c >= 0x0300 && c <= 0x036F)
            {
                //This Hex Range allowed
                return true;
            }
            else if (c >= 0x203F && c <= 0x2040)
            {
                //This Hex Range allowed
                return true;
            }
            else
            {
                //Anything else forbidden
                return false;
            }
        }

        /// <summary>
        /// Returns whether a Character is a NameChar as defined by the W3C XML Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml/#NT-NameChar</see>
        public static bool IsNameStartChar(char c)
        {
            if (Char.IsLetter(c))
            {
                //Letters are allowed
                return true;
            }
            else if (c == '_' || c == ':')
            {
                //Underscore and Colon Allowed
                return true;
            }
            else if ((c >= 0xC0 && c <= 0xD6) ||
                       (c >= 0xD8 && c <= 0xF6) ||
                       (c >= 0xF8 && c <= 0x2FF) ||
                       (c >= 0x370 && c <= 0x37D) ||
                       (c >= 0x37F && c <= 0x1FFF) ||
                       (c >= 0x200C && c <= 0x200D) ||
                       (c >= 0x2070 && c <= 0x218F) ||
                       (c >= 0x2C00 && c <= 0x2FEF) ||
                       (c >= 0x3001 && c <= 0xD7FF) ||
                       (c >= 0xF900 && c <= 0xFDCF) ||
                       (c >= 0xFDF0 && c <= 0xFFFD))
            {
                //Whole load of Hex Ranges are allowed
                return true;
            }
            else
            {
                //Have to cast to an Integer because the next Hex Range is out of the range of Character
                int i = (int)c;
                if (i >= 0x10000 && i <= 0xEFFFF)
                {
                    //This Hex Range is also allowed
                    return true;
                }
                //Anything else is forbidden
                return false;
            }
        }

        /// <summary>
        /// Returns whether a Character is a Letter as defined by the W3C XMLSpecification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml/#NT-Letter</see>
        public static bool IsLetter(char c)
        {
            //Can be a Base Character or an Ideographic
            return (IsBaseChar(c) || IsIdeographic(c));
        }

        /// <summary>
        /// Returns whether a Character is a BaseChar as defined by the W3C XML Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml/#NT-BaseChar</see>
        public static bool IsBaseChar(char c)
        {
            if ((c >= 0x0041 && c <= 0x005A) ||
                (c >= 0x0061 && c <= 0x007A) ||
                (c >= 0x00C0 && c <= 0x00D6) ||
                (c >= 0x00D8 && c <= 0x00F6) ||
                (c >= 0x00F8 && c <= 0x00FF) ||
                (c >= 0x0100 && c <= 0x0131) ||
                (c >= 0x0134 && c <= 0x013E) ||
                (c >= 0x0141 && c <= 0x0148) ||
                (c >= 0x014A && c <= 0x017E) ||
                (c >= 0x0180 && c <= 0x01C3) ||
                (c >= 0x01CD && c <= 0x01F0) ||
                (c >= 0x01F4 && c <= 0x01F5) ||
                (c >= 0x01FA && c <= 0x0217) ||
                (c >= 0x0250 && c <= 0x02A8) ||
                (c >= 0x02BB && c <= 0x02C1) ||
                (c == 0x0386) ||
                (c >= 0x0388 && c <= 0x038A) ||
                (c == 0x038C) ||
                (c >= 0x038E && c <= 0x03A1) ||
                (c >= 0x03A3 && c <= 0x03CE) ||
                (c >= 0x03D0 && c <= 0x03D6) ||
                (c == 0x03DA) ||
                (c == 0x03DC) ||
                (c == 0x03DE) ||
                (c == 0x03E0) ||
                (c >= 0x03E2 && c <= 0x03F3) ||
                (c >= 0x0401 && c <= 0x040C) ||
                (c >= 0x040E && c <= 0x044F) ||
                (c >= 0x0451 && c <= 0x045C) ||
                (c >= 0x045E && c <= 0x0481) ||
                (c >= 0x0490 && c <= 0x04C4) ||
                (c >= 0x04C7 && c <= 0x04C8) ||
                (c >= 0x04CB && c <= 0x04CC) ||
                (c >= 0x04D0 && c <= 0x04EB) ||
                (c >= 0x04EE && c <= 0x04F5) ||
                (c >= 0x04F8 && c <= 0x04F9) ||
                (c >= 0x0531 && c <= 0x0556) ||
                (c == 0x0559) ||
                (c >= 0x0561 && c <= 0x0586) ||
                (c >= 0x05D0 && c <= 0x05EA) ||
                (c >= 0x05F0 && c <= 0x05F2) ||
                (c >= 0x0621 && c <= 0x063A) ||
                (c >= 0x0641 && c <= 0x064A) ||
                (c >= 0x0671 && c <= 0x06B7) ||
                (c >= 0x06BA && c <= 0x06BE) ||
                (c >= 0x06C0 && c <= 0x06CE) ||
                (c >= 0x06D0 && c <= 0x06D3) ||
                (c == 0x06D5) ||
                (c >= 0x06E5 && c <= 0x06E6) ||
                (c >= 0x0905 && c <= 0x0939) ||
                (c == 0x093D) ||
                (c >= 0x0958 && c <= 0x0961) ||
                (c >= 0x0985 && c <= 0x098C) ||
                (c >= 0x098F && c <= 0x0990) ||
                (c >= 0x0993 && c <= 0x09A8) ||
                (c >= 0x09AA && c <= 0x09B0) ||
                (c == 0x09B2) ||
                (c >= 0x09B6 && c <= 0x09B9) ||
                (c >= 0x09DC && c <= 0x09DD) ||
                (c >= 0x09DF && c <= 0x09E1) ||
                (c >= 0x09F0 && c <= 0x09F1) ||
                (c >= 0x0A05 && c <= 0x0A0A) ||
                (c >= 0x0A0F && c <= 0x0A10) ||
                (c >= 0x0A13 && c <= 0x0A28) ||
                (c >= 0x0A2A && c <= 0x0A30) ||
                (c >= 0x0A32 && c <= 0x0A33) ||
                (c >= 0x0A35 && c <= 0x0A36) ||
                (c >= 0x0A38 && c <= 0x0A39) ||
                (c >= 0x0A59 && c <= 0x0A5C) ||
                (c == 0x0A5E) ||
                (c >= 0x0A72 && c <= 0x0A74) ||
                (c >= 0x0A85 && c <= 0x0A8B) ||
                (c == 0x0A8D) ||
                (c >= 0x0A8F && c <= 0x0A91) ||
                (c >= 0x0A93 && c <= 0x0AA8) ||
                (c >= 0x0AAA && c <= 0x0AB0) ||
                (c >= 0x0AB2 && c <= 0x0AB3) ||
                (c >= 0x0AB5 && c <= 0x0AB9) ||
                (c == 0x0ABD) ||
                (c == 0x0AE0) ||
                (c >= 0x0B05 && c <= 0x0B0C) ||
                (c >= 0x0B0F && c <= 0x0B10) ||
                (c >= 0x0B13 && c <= 0x0B28) ||
                (c >= 0x0B2A && c <= 0x0B30) ||
                (c >= 0x0B32 && c <= 0x0B33) ||
                (c >= 0x0B36 && c <= 0x0B39) ||
                (c == 0x0B3D) ||
                (c >= 0x0B5C && c <= 0x0B5D) ||
                (c >= 0x0B5F && c <= 0x0B61) ||
                (c >= 0x0B85 && c <= 0x0B8A) ||
                (c >= 0x0B8E && c <= 0x0B90) ||
                (c >= 0x0B92 && c <= 0x0B95) ||
                (c >= 0x0B99 && c <= 0x0B9A) ||
                (c == 0x0B9C) ||
                (c >= 0x0B9E && c <= 0x0B9F) ||
                (c >= 0x0BA3 && c <= 0x0BA4) ||
                (c >= 0x0BA8 && c <= 0x0BAA) ||
                (c >= 0x0BAE && c <= 0x0BB5) ||
                (c >= 0x0BB7 && c <= 0x0BB9) ||
                (c >= 0x0C05 && c <= 0x0C0C) ||
                (c >= 0x0C0E && c <= 0x0C10) ||
                (c >= 0x0C12 && c <= 0x0C28) ||
                (c >= 0x0C2A && c <= 0x0C33) ||
                (c >= 0x0C35 && c <= 0x0C39) ||
                (c >= 0x0C60 && c <= 0x0C61) ||
                (c >= 0x0C85 && c <= 0x0C8C) ||
                (c >= 0x0C8E && c <= 0x0C90) ||
                (c >= 0x0C92 && c <= 0x0CA8) ||
                (c >= 0x0CAA && c <= 0x0CB3) ||
                (c >= 0x0CB5 && c <= 0x0CB9) ||
                (c == 0x0CDE) ||
                (c >= 0x0CE0 && c <= 0x0CE1) ||
                (c >= 0x0D05 && c <= 0x0D0C) ||
                (c >= 0x0D0E && c <= 0x0D10) ||
                (c >= 0x0D12 && c <= 0x0D28) ||
                (c >= 0x0D2A && c <= 0x0D39) ||
                (c >= 0x0D60 && c <= 0x0D61) ||
                (c >= 0x0E01 && c <= 0x0E2E) ||
                (c == 0x0E30) ||
                (c >= 0x0E32 && c <= 0x0E33) ||
                (c >= 0x0E40 && c <= 0x0E45) ||
                (c >= 0x0E81 && c <= 0x0E82) ||
                (c == 0x0E84) ||
                (c >= 0x0E87 && c <= 0x0E88) ||
                (c == 0x0E8A) ||
                (c == 0x0E8D) ||
                (c >= 0x0E94 && c <= 0x0E97) ||
                (c >= 0x0E99 && c <= 0x0E9F) ||
                (c >= 0x0EA1 && c <= 0x0EA3) ||
                (c == 0x0EA5) ||
                (c == 0x0EA7) ||
                (c >= 0x0EAA && c <= 0x0EAB) ||
                (c >= 0x0EAD && c <= 0x0EAE) ||
                (c == 0x0EB0) ||
                (c >= 0x0EB2 && c <= 0x0EB3) ||
                (c == 0x0EBD) ||
                (c >= 0x0EC0 && c <= 0x0EC4) ||
                (c >= 0x0F40 && c <= 0x0F47) ||
                (c >= 0x0F49 && c <= 0x0F69) ||
                (c >= 0x10A0 && c <= 0x10C5) ||
                (c >= 0x10D0 && c <= 0x10F6) ||
                (c == 0x1100) ||
                (c >= 0x1102 && c <= 0x1103) ||
                (c >= 0x1105 && c <= 0x1107) ||
                (c == 0x1109) ||
                (c >= 0x110B && c <= 0x110C) ||
                (c >= 0x110E && c <= 0x1112) ||
                (c == 0x113C) ||
                (c == 0x113E) ||
                (c == 0x1140) ||
                (c == 0x114C) ||
                (c == 0x114E) ||
                (c == 0x1150) ||
                (c >= 0x1154 && c <= 0x1155) ||
                (c == 0x1159) ||
                (c >= 0x115F && c <= 0x1161) ||
                (c == 0x1163) ||
                (c == 0x1165) ||
                (c == 0x1167) ||
                (c == 0x1169) ||
                (c >= 0x116D && c <= 0x116E) ||
                (c >= 0x1172 && c <= 0x1173) ||
                (c == 0x1175) ||
                (c == 0x119E) ||
                (c == 0x11A8) ||
                (c == 0x11AB) ||
                (c >= 0x11AE && c <= 0x11AF) ||
                (c >= 0x11B7 && c <= 0x11B8) ||
                (c == 0x11BA) ||
                (c >= 0x11BC && c <= 0x11C2) ||
                (c == 0x11EB) ||
                (c == 0x11F0) ||
                (c == 0x11F9) ||
                (c >= 0x1E00 && c <= 0x1E9B) ||
                (c >= 0x1EA0 && c <= 0x1EF9) ||
                (c >= 0x1F00 && c <= 0x1F15) ||
                (c >= 0x1F18 && c <= 0x1F1D) ||
                (c >= 0x1F20 && c <= 0x1F45) ||
                (c >= 0x1F48 && c <= 0x1F4D) ||
                (c >= 0x1F50 && c <= 0x1F57) ||
                (c == 0x1F59) ||
                (c == 0x1F5B) ||
                (c == 0x1F5D) ||
                (c >= 0x1F5F && c <= 0x1F7D) ||
                (c >= 0x1F80 && c <= 0x1FB4) ||
                (c >= 0x1FB6 && c <= 0x1FBC) ||
                (c == 0x1FBE) ||
                (c >= 0x1FC2 && c <= 0x1FC4) ||
                (c >= 0x1FC6 && c <= 0x1FCC) ||
                (c >= 0x1FD0 && c <= 0x1FD3) ||
                (c >= 0x1FD6 && c <= 0x1FDB) ||
                (c >= 0x1FE0 && c <= 0x1FEC) ||
                (c >= 0x1FF2 && c <= 0x1FF4) ||
                (c >= 0x1FF6 && c <= 0x1FFC) ||
                (c == 0x2126) ||
                (c >= 0x212A && c <= 0x212B) ||
                (c == 0x212E) ||
                (c >= 0x2180 && c <= 0x2182) ||
                (c >= 0x3041 && c <= 0x3094) ||
                (c >= 0x30A1 && c <= 0x30FA) ||
                (c >= 0x3105 && c <= 0x312C) ||
                (c >= 0xAC00 && c <= 0xD7A3))
            {
                //Whole Ton of Hex Ranges and Values are allowed
                return true;
            }
            else
            {
                //Anything else is forbidden
                return false;
            }

        }

        /// <summary>
        /// Returns whether a Character is an Ideographic as defined by the W3C XML Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        /// <see>http://www.w3.org/TR/REC-xml/#NT-Ideographic</see>
        public static bool IsIdeographic(char c)
        {
            //Couple of Hex Ranges and a particular Hex value allowed
            if (c >= 0x4E00 && c <= 0x9FA5)
            {
                return true;
            }
            else if (c == 0x3007)
            {
                return true;
            }
            else if (c >= 0x3021 && c <= 0x3029)
            {
                return true;
            }
            else
            {
                //Anything else if foridden
                return true;
            }

        }

        #endregion

        /// <summary>
        /// Returns whether the given Type refers to one of the types supported by the <see cref="LiteralNode">LiteralNode</see> CompareTo method
        /// </summary>
        /// <param name="type">Data Type Uri</param>
        /// <returns></returns>
        public static bool IsSupportedType(String type)
        {
            return XmlSpecsHelper.SupportedTypes.Contains(type);
        }

        /// <summary>
        /// Returns whether the given Type refers to one of the types supported by the <see cref="LiteralNode">LiteralNode</see> CompareTo method
        /// </summary>
        /// <param name="type">Data Type Uri</param>
        /// <returns></returns>
        public static bool IsSupportedType(Uri type)
        {
            if (type == null) return false;
            return IsSupportedType(type.ToString());
        }

        /// <summary>
        /// Gets the Data Type Uri of the given Node if it has a supported type
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Only <see cref="ILiteralNode">ILiteralNode</see>'s can have a Data Type
        /// </para>
        /// <para>
        /// The function only returns the Data Type Uri (as a String) if the Data Type of the Literal is one of the supported Data Types
        /// </para>
        /// </remarks>
        public static String GetSupportedDataType(INode n)
        {
            if (n == null) throw new RdfException("Data Type cannot be determined for Nulls");

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfException("Data Type cannot be determined for non-Literal Nodes");
                case NodeType.Literal:
                    ILiteralNode l = (ILiteralNode)n;
                    if (l.DataType == null)
                    {
                        if (!l.Language.Equals(String.Empty))
                        {
                            throw new RdfException("Literals with Language Specifiers do not have a Data Type");
                        }
                        else
                        {
                            return XmlSchemaDataTypeString;
                        }
                    }
                    else
                    {
                        String type = l.DataType.ToString();
                        if (XmlSpecsHelper.IsSupportedType(type))
                        {
                            return type;
                        }
                        else
                        {
                            return String.Empty;
                        }
                    }
                default:
                    throw new RdfException("Data Type cannot be determined for unknown Node types");
            }
        }

        /// <summary>
        /// Gets the Compatible Supported Data Type assuming the two Nodes are Literals with support types and that those types are compatible
        /// </summary>
        /// <param name="x">A Node</param>
        /// <param name="y">A Node</param>
        /// <param name="widen">Whether the compatible type should be the wider type</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics
        /// </para>
        /// </remarks>
        public static String GetCompatibleSupportedDataType(INode x, INode y, bool widen)
        {
            return GetCompatibleSupportedDataType(GetSupportedDataType(x), GetSupportedDataType(y), widen);
        }

        /// <summary>
        /// Gets the Compatible Supported Data Type assuming the two Nodes are Literals with support types and that those types are compatible
        /// </summary>
        /// <param name="x">A Node</param>
        /// <param name="y">A Node</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics
        /// </para>
        /// </remarks>
        public static String GetCompatibleSupportedDataType(INode x, INode y)
        {
            return GetCompatibleSupportedDataType(GetSupportedDataType(x), GetSupportedDataType(y), false);
        }

        /// <summary>
        /// Gets the Compatible Supported Data Type for the two Data Types
        /// </summary>
        /// <param name="type1">A Data Type</param>
        /// <param name="type2">A Data Type</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics
        /// </para>
        /// </remarks>
        public static String GetCompatibleSupportedDataType(String type1, String type2)
        {
            return GetCompatibleSupportedDataType(type1, type2, false);
        }

        /// <summary>
        /// Gets the Compatible Supported Data Type for the two Data Types
        /// </summary>
        /// <param name="type1">A Data Type</param>
        /// <param name="type2">A Data Type</param>
        /// <param name="widen">Whether the compatible type should be the wider type</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics
        /// </para>
        /// </remarks>
        public static String GetCompatibleSupportedDataType(String type1, String type2, bool widen)
        {
            if (!SupportedTypes.Contains(type1) || !SupportedTypes.Contains(type2))
            {
                throw new RdfException("Unknown Types are not compatible");
            }
            else if (type1.Equals(type2))
            {
                return type1;
            }
            else
            {
                switch (type1)
                {
                    //TODO: Implement type compatability detection for numeric types

                    case XmlSchemaDataTypeDate:
                        if (type2.Equals(XmlSchemaDataTypeDateTime))
                        {
                            if (widen)
                            {
                                return XmlSchemaDataTypeDateTime;
                            }
                            else
                            {
                                return XmlSchemaDataTypeDate;
                            }
                        }
                        else
                        {
                            return String.Empty;
                        }
                    case XmlSchemaDataTypeDateTime:
                        if (type2.Equals(XmlSchemaDataTypeDate))
                        {
                            if (widen)
                            {
                                return XmlSchemaDataTypeDateTime;
                            }
                            else
                            {
                                return XmlSchemaDataTypeDate;
                            }
                        }
                        else
                        {
                            return String.Empty;
                        }
                    default:
                        return String.Empty;
                }
            }
        }
    }
}
