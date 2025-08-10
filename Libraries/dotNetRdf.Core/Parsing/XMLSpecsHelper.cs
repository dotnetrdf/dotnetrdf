/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Parsing;

/// <summary>
/// Static Helper Class which contains a set of Functions which model Name and Character validations as laid
/// out in the W3C XML and XML Namespaces specification.
/// </summary>
/// <remarks>
/// These are needed in the XML/RDF Parser
/// 
/// Also contains the Date Time format string used to format .Net's DateTime type into a String whose format conforms to the XML Schema Datatypes specification.
/// 
/// 
/// </remarks>
/// <seealso>http://www.w3.org/TR/REC-xml/</seealso>
/// <seealso>http://www.w3.org/TR/REC-xml-names/</seealso>
/// <seealso>http://www.w3.org/TR/xmlschema-2/</seealso>
public static class XmlSpecsHelper
{
    /// <summary>
    /// Namespace for XML.
    /// </summary>
    public const string NamespaceXml = "http://www.w3.org/XML/1998/namespace";
    /// <summary>
    /// Namespace for XML Namespaces.
    /// </summary>
    public const string NamespaceXmlNamespaces = "http://www.w3.org/2000/xmlns/";
    /// <summary>
    /// Namespace for XML Schema.
    /// </summary>
    public const string NamespaceXmlSchema = "http://www.w3.org/2001/XMLSchema#";

    /// <summary>
    /// Date Time Formatting string which meets the specified format for xsd:dateTime.
    /// </summary>
    /// <remarks>
    /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format.
    /// </remarks>
    public const string XmlSchemaDateTimeFormat = "yyyy-MM-dd\\THH:mm:ss.ffffffK";

    /// <summary>
    /// Date Time Formatting string which meets the specified format for xsd:dateTime, this formatting string is imprecise in the sense that it does not preserve the fractional seconds.
    /// </summary>
    /// <remarks>
    /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format.
    /// </remarks>
    public const string XmlSchemaDateTimeFormatImprecise = "yyyy-MM-dd\\THH:mm:ssK";

    /// <summary>
    /// Date Time Formatting string which meets the specified format for xsd:date.
    /// </summary>
    /// <remarks>
    /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format.
    /// </remarks>
    public const string XmlSchemaDateFormat = "yyyy-MM-ddK";

    /// <summary>
    /// Date Time Formatting string which meets the the specified format for xsd:time.
    /// </summary>
    /// <remarks>
    /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format.
    /// </remarks>
    public const string XmlSchemaTimeFormat = "HH:mm:ss.ffffffK";

    /// <summary>
    /// Date Time Formatting string which meets the the specified format for xsd:time, this formatting string is imprecise in the sense that it does not preserve the fractional seconds.
    /// </summary>
    /// <remarks>
    /// Use with the <see cref="DateTime.ToString()">DateTime.ToString()</see> method to format a DateTime into appropriate string format.
    /// </remarks>
    public const string XmlSchemaTimeFormatImprecise = "HH:mm:ssK";

    /// <summary>
    /// A permissive regular expression that matches the structure of the lexical space for an XML Schema Duration.
    /// </summary>
    /// <remarks>This regular expression will match certain invalid constructs, in particular a duration with a `T` followed by no time components.</remarks>
    public static Regex XmlSchemaDurationRegex =
        new(@"^-?P(\d+Y)?(\d+M)?(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// A permissive regular expression that matches the structure of xsd:dateTime.
    /// </summary>
    /// <remarks>This regular expression does not catch out-of-range errors on any of the date or time components.</remarks>
    public static Regex XmlSchemaDateTimeRegex =
        new(@"^-?(\d\d\d\d+)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d(\.\d+)?)(Z|(\+|-)?(\d\d):(\d\d))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// A permissive regular expression that matches the structure of xsd:date.
    /// </summary>
    /// <remarks>This regular expression does not catch out-of-range errors on any of the date components.</remarks>
    public static Regex XmlSchemaDateRegex =
        new(@"^-?(\d\d\d\d+)-(\d\d)-(\d\d)$", RegexOptions.Compiled);

    /// <summary>
    /// A permissive regular expression that matches the structure of xsd:time.
    /// </summary>
    /// <remarks>This regular expression does not catch out-of-range errors on any of the time components.</remarks>
    public static Regex XmlSchemaTimeRegex =
        new(@"^(\d\d):(\d\d):(\d\d(\.\d+)?)(Z|(\+|-)?(\d\d):(\d\d))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// A permissive regular expression that matches the structure of xsd:gMonthYear.
    /// </summary>
    public static Regex XmlSchemaYearMonthRegex =
        new (@"^-?(\d\d\d\d+)-(\d\d)(Z|(\+|-)?(\d\d):(\d\d))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// A permissive regular expression that matches the structure of xsd:gYear.
    /// </summary>
    public static Regex XmlSchemaYearRegex =
        new(@"^-?(\d\d\d\d+)(Z|(\+|-)?(\d\d):(\d\d))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Data Type Uri Constants for XML Schema Data Types.
    /// </summary>
    public const string XmlSchemaDataTypeAnyUri = NamespaceXmlSchema + "anyURI",
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
        XmlSchemaDataTypeUnsignedShort = NamespaceXmlSchema + "unsignedShort",
        XmlSchemaDataTypeYearMonth = NamespaceXmlSchema + "gYearMonth",
        XmlSchemaDataTypeYear = NamespaceXmlSchema + "gYear";

    /// <summary>
    /// Array of Constants for Data Types that are supported by the Literal Node CompareTo method.
    /// </summary>
    public static string[] SupportedTypes = new string[] {
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
        XmlSchemaDataTypeUnsignedShort,
        RdfSpecsHelper.RdfLangString,
        /*XmlSchemaDataTypeXmlLiteral*/
    };

    #region Name Validation

    /// <summary>
    /// Returns whether a String is a Name as defined by the W3C XML Specification.
    /// </summary>
    /// <param name="name">String to test.</param>
    /// <returns></returns>
    public static bool IsName(string name)
    {
        // Get the Characters
        var cs = name.ToCharArray();

        if (cs.Length == 0) return false;

        // Check first Character is a valid NameStartChar
        if (!IsNameStartChar(cs[0]))
        {
            return false;
        }

        // Check rest of Characters
        if (cs.Length > 1)
        {
            for (var i = 1; i < cs.Length; i++)
            {
                // Must be a valid NCNameChar
                if (!IsNameChar(cs[i]))
                {
                    return false;
                }
            }
        }

        // If we get here the String is OK
        return true;
    }

    /// <summary>
    /// Returns whether a String is a NCName as defined by the W3C XML Namespaces Specification.
    /// </summary>
    /// <param name="name">String to test.</param>
    /// <returns></returns>
    /// <see>http://www.w3.org/TR/REC-xml-names/#NT-NCName.</see>
    public static bool IsNCName(string name)
    {
        return IsName(name) && !name.Contains(":");
    }

    /// <summary>
    /// Returns whether a Character is a NameChar as defined by the W3C XML Specification.
    /// </summary>
    /// <param name="c">Character to Test.</param>
    /// <returns></returns>
    /// <see>http://www.w3.org/TR/REC-xml/#NT-NameChar.</see>
    public static bool IsNameChar(char c)
    {
        if (IsNameStartChar(c))
        {
            // NameStartChar's allowed
            return true;
        }
        else if (char.IsDigit(c))
        {
            // Digits allowed
            return true;
        }
        else if (c == '.' || c == '-')
        {
            // Period and Hyphen allowed
            return true;
        }
        else if (c == 0xB7) {
            // This Hex Character allowed
            return true;
        }
        else if (c >= 0x0300 && c <= 0x036F)
        {
            // This Hex Range allowed
            return true;
        }
        else if (c >= 0x203F && c <= 0x2040)
        {
            // This Hex Range allowed
            return true;
        }
        else
        {
            // Anything else forbidden
            return false;
        }
    }

    /// <summary>
    /// Returns whether a Character is a NameChar as defined by the W3C XML Specification.
    /// </summary>
    /// <param name="c">Character to test.</param>
    /// <returns></returns>
    /// <see>http://www.w3.org/TR/REC-xml/#NT-NameChar.</see>
    public static bool IsNameStartChar(char c)
    {
        if (c == '_' || c == ':')
        {
            // Underscore and Colon Allowed
            return true;
        }
        else if (c >= 'a' && c <= 'z')
        {
            return true;
        }
        else if (c >= 'A' && c <= 'Z')
        {
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
            // Whole load of Hex Ranges are allowed
            return true;
        }
        else
        {
            // Have to cast to an Integer because the next Hex Range is out of the range of Character
            var i = (int)c;
            if (i >= 0x10000 && i <= 0xEFFFF)
            {
                // This Hex Range is also allowed
                return true;
            }
            // Anything else is forbidden
            return false;
        }
    }

    #endregion

    /// <summary>
    /// Returns whether the given Type refers to one of the types supported by the <see cref="LiteralNode">LiteralNode</see> CompareTo method.
    /// </summary>
    /// <param name="type">Data Type Uri.</param>
    /// <returns></returns>
    public static bool IsSupportedType(string type)
    {
        return SupportedTypes.Contains(type);
    }

    /// <summary>
    /// Returns whether the given Type refers to one of the types supported by the <see cref="LiteralNode">LiteralNode</see> CompareTo method.
    /// </summary>
    /// <param name="type">Data Type Uri.</param>
    /// <returns></returns>
    public static bool IsSupportedType(Uri type)
    {
        if (type == null) return false;
        return IsSupportedType(type.AbsoluteUri);
    }

    /// <summary>
    /// Gets the Data Type Uri of the given Node if it has a supported type.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Only <see cref="ILiteralNode">ILiteralNode</see>'s can have a Data Type.
    /// </para>
    /// <para>
    /// The function only returns the Data Type Uri (as a String) if the Data Type of the Literal is one of the supported Data Types.
    /// </para>
    /// </remarks>
    public static string GetSupportedDataType(INode n)
    {
        if (n == null) throw new RdfException("Data Type cannot be determined for Nulls");

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfException("Data Type cannot be determined for non-Literal Nodes");
            case NodeType.Literal:
                var l = (ILiteralNode)n;
                if (l.DataType == null)
                {
                    if (!l.Language.Equals(string.Empty))
                    {
                        return RdfSpecsHelper.RdfLangString;
                        // throw new RdfException("Literals with Language Specifiers do not have a Data Type");
                    }
                    else
                    {
                        return XmlSchemaDataTypeString;
                    }
                }
                else
                {
                    var type = l.DataType.AbsoluteUri;
                    if (IsSupportedType(type))
                    {
                        return type;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            default:
                throw new RdfException("Data Type cannot be determined for unknown Node types");
        }
    }

    /// <summary>
    /// Gets the Compatible Supported Data Type assuming the two Nodes are Literals with support types and that those types are compatible.
    /// </summary>
    /// <param name="x">A Node.</param>
    /// <param name="y">A Node.</param>
    /// <param name="widen">Whether the compatible type should be the wider type.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics.
    /// </para>
    /// </remarks>
    public static string GetCompatibleSupportedDataType(INode x, INode y, bool widen)
    {
        return GetCompatibleSupportedDataType(GetSupportedDataType(x), GetSupportedDataType(y), widen);
    }

    /// <summary>
    /// Gets the Compatible Supported Data Type assuming the two Nodes are Literals with support types and that those types are compatible.
    /// </summary>
    /// <param name="x">A Node.</param>
    /// <param name="y">A Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics.
    /// </para>
    /// </remarks>
    public static string GetCompatibleSupportedDataType(INode x, INode y)
    {
        return GetCompatibleSupportedDataType(GetSupportedDataType(x), GetSupportedDataType(y), false);
    }

    /// <summary>
    /// Gets the Compatible Supported Data Type for the two Data Types.
    /// </summary>
    /// <param name="type1">A Data Type.</param>
    /// <param name="type2">A Data Type.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics.
    /// </para>
    /// </remarks>
    public static string GetCompatibleSupportedDataType(string type1, string type2)
    {
        return GetCompatibleSupportedDataType(type1, type2, false);
    }

    /// <summary>
    /// Gets the Compatible Supported Data Type for the two Data Types.
    /// </summary>
    /// <param name="type1">A Data Type.</param>
    /// <param name="type2">A Data Type.</param>
    /// <param name="widen">Whether the compatible type should be the wider type.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Currently this is only immplemented sufficiently for the types it needs to know are compatible for implementing SPARQL equality and ordering semantics.
    /// </para>
    /// </remarks>
    public static string GetCompatibleSupportedDataType(string type1, string type2, bool widen)
    {
        if (!SupportedTypes.Contains(type1) || !SupportedTypes.Contains(type2))
        {
            throw new RdfException("Unknown Types are not compatible");
        }

        if (type1.Equals(type2))
        {
            return type1;
        }

        // TODO: This is enough for our specific ordering needs, but could maybe be broken down further for more general usage?
        SparqlNumericType numericType1 = NumericTypesHelper.GetNumericTypeFromDataTypeUri(type1);
        if (numericType1 != SparqlNumericType.NaN)
        {
            SparqlNumericType numericType2 = NumericTypesHelper.GetNumericTypeFromDataTypeUri(type2);
            if (numericType2 != SparqlNumericType.NaN)
            {
                var compatibleType = (SparqlNumericType)Math.Max((int)numericType1, (int)numericType2);
                switch (compatibleType)
                {
                    case SparqlNumericType.Integer:
                        return XmlSchemaDataTypeInteger;
                    case SparqlNumericType.Decimal:
                        return XmlSchemaDataTypeDecimal;
                    case SparqlNumericType.Float:
                        return XmlSchemaDataTypeFloat;
                    case SparqlNumericType.Double:
                        return XmlSchemaDataTypeDouble;
                }
            }
        }

        switch (type1)
        {
            case XmlSchemaDataTypeDate when type2.Equals(XmlSchemaDataTypeDateTime):
                return widen ? XmlSchemaDataTypeDateTime : XmlSchemaDataTypeDate;

            case XmlSchemaDataTypeDateTime when type2.Equals(XmlSchemaDataTypeDate):
                return widen ? XmlSchemaDataTypeDateTime : XmlSchemaDataTypeDate;
            
            case RdfSpecsHelper.RdfLangString when type2.Equals(XmlSchemaDataTypeString):
            case XmlSchemaDataTypeString when type2.Equals(RdfSpecsHelper.RdfLangString):
                return XmlSchemaDataTypeString;

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Attempts to get the compatible datatype for the two specified data types.
    /// </summary>
    /// <param name="type1">A data type IRI string.</param>
    /// <param name="type2">A data type IRI string.</param>
    /// <param name="compatibleDataType">Receives the IRI of the compatible data type, if any.</param>
    /// <returns>True if a compatible datatype could be found for the two data types, false otherwise.</returns>
    public static bool TryGetCompatibleSupportedDataType(string type1, string type2, out string compatibleDataType)
    {
        return TryGetCompatibleSupportedDataType(type1, type2, false, out compatibleDataType);
    }


    /// <summary>
    /// Attempts to get the compatible datatype for the two specified data types.
    /// </summary>
    /// <param name="type1">A data type IRI string.</param>
    /// <param name="type2">A data type IRI string.</param>
    /// <param name="widen">Whether the compatible data type should be the wider type.</param>
    /// <param name="compatibleDataType">Receives the IRI of the compatible data type, if any.</param>
    /// <returns>True if a compatible datatype could be found for the two data types, false otherwise.</returns>
    public static bool TryGetCompatibleSupportedDataType(string type1, string type2, bool widen,
        out string compatibleDataType)
    {
        try
        {
            compatibleDataType = GetCompatibleSupportedDataType(type1, type2, widen);
            return !string.Empty.Equals(compatibleDataType);
        }
        catch (RdfException)
        {
            compatibleDataType = string.Empty;
            return false;
        }
    }
}
