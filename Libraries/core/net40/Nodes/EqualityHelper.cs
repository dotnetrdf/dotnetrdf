/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Static Helper class containing standard implementations of Equality between various Node types
    /// </summary>
    public static class EqualityHelper
    {
        /// <summary>
        /// Determines whether two URIs are equal
        /// </summary>
        /// <param name="a">First URI</param>
        /// <param name="b">Second URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Unlike the Equals method provided by the <see cref="Uri">Uri</see> class by default this takes into account Fragment IDs which are essential for checking URI equality in RDF
        /// </remarks>
        public static bool AreUrisEqual(Uri a, Uri b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }

            if (a.IsAbsoluteUri && b.IsAbsoluteUri)
            {
                // Absolute URIs are equal if the Scheme, Host and Post are equivalent (case insensitive)
                // and the User Info (if any) is equivalent (case sensitive)
                // and the Path, Query and Fragment are equivalent (case sensitive)
                return a.Scheme.Equals(b.Scheme, StringComparison.OrdinalIgnoreCase)
                       && a.Host.Equals(b.Host, StringComparison.OrdinalIgnoreCase)
                       && a.Port.Equals(b.Port)
                       && a.UserInfo.Equals(b.UserInfo, StringComparison.Ordinal)
#if !SILVERLIGHT
                       && a.PathAndQuery.Equals(b.PathAndQuery, StringComparison.Ordinal)
#else
                   && a.PathAndQuery().Equals(b.PathAndQuery(), StringComparison.Ordinal)
#endif
                       && a.Fragment.Equals(b.Fragment, StringComparison.Ordinal);
            }
            if (!a.IsAbsoluteUri && !b.IsAbsoluteUri)
            {
                // Relative URIs are equal if the Path, Query and Fragment are equivalent (case sensitive)
#if !SILVERLIGHT
                return a.PathAndQuery.Equals(b.PathAndQuery, StringComparison.Ordinal)
#else
                return a.PathAndQuery().Equals(b.PathAndQuery(), StringComparison.Ordinal)
#endif
                       && a.Fragment.Equals(b.Fragment, StringComparison.Ordinal);
            }
            // An absolute URI can't be equal to a relative URI
            return false;
        }

        /// <summary>
        /// Determines whether two nodes are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreNodesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }
            return !ReferenceEquals(b, null) && a.Equals(b);
        }

        /// <summary>
        /// Determines whether two URI nodes are equal
        /// </summary>
        /// <param name="a">First URI Node</param>
        /// <param name="b">Second URI Node</param>
        /// <returns></returns>
        public static bool AreUrisEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && AreUrisEqual(a.Uri, b.Uri);
        }

        /// <summary>
        /// Determines whether two Literals are equal
        /// </summary>
        /// <param name="a">First Literal</param>
        /// <param name="b">Second Literal</param>
        /// <returns></returns>
        public static bool AreLiteralsEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }

            //Language Tags must be equal (if present)
            //If they don't have language tags then they'll both be set to String.Empty which will give true
            if (a.HasLanguage && b.HasLanguage)
            {
                if (a.Language.Equals(b.Language, StringComparison.OrdinalIgnoreCase))
                {
                    //Datatypes must be equal (if present)
                    //If they don't have Data Types then they'll both be null
                    //Otherwise the URIs must be equal
                    if (!a.HasDataType && !b.HasDataType)
                    {
                        //Use String equality to get the result
                        return a.Value.Equals(b.Value, StringComparison.Ordinal);
                    }
                    if (!a.HasDataType)
                    {
                        //We have a Null DataType but the other Node doesn't so can't be equal
                        return false;
                    }
                    if (!b.HasDataType)
                    {
                        //The other Node has a Null DataType but we don't so can't be equal
                        return false;
                    }
                    if (AreUrisEqual(a.DataType, b.DataType))
                    {
                        //We have equal DataTypes so use String Equality to evaluate
                        if (Options.LiteralEqualityMode == LiteralEqualityMode.Strict)
                        {
                            //Strict Equality Mode uses Ordinal Lexical Comparison for Equality as per W3C RDF Spec
                            return a.Value.Equals(b.Value, StringComparison.Ordinal);
                        }
                        //Loose Equality Mode uses Value Based Comparison for Equality of Typed Nodes
                        return (a.CompareTo(b) == 0);
                    }
                    //Data Types didn't match
                    return false;
                }
                //Language Tags didn't match
                return false;
            }
            // One has a language tag and the other does not
            return false;
        }

        /// <summary>
        /// Determines whether two Blank Nodes are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreBlankNodesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.AnonID.Equals(b.AnonID);
        }

        /// <summary>
        /// Determines whether two Graph Literals are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreGraphLiteralsEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.SubGraph.Equals(b.SubGraph);
        }

        /// <summary>
        /// Determines whether two Variable Nodes are equal
        /// </summary>
        /// <param name="a">First Variable Node</param>
        /// <param name="b">Second Variable Node</param>
        /// <returns></returns>
        public static bool AreVariablesEqual(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                return b == null;
            }
            return b != null && a.VariableName.Equals(b.VariableName, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Static Helper class containing standard implementations of Comparison between various Node types
    /// </summary>
    public static class ComparisonHelper
    {
        /// <summary>
        /// Compares two URIs
        /// </summary>
        /// <param name="a">First URI</param>
        /// <param name="b">Second URI</param>
        /// <returns></returns>
        public static int CompareUris(Uri a, Uri b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            if (b == null)
            {
                return 1;
            }

            if (a.IsAbsoluteUri && !b.IsAbsoluteUri)
            {
                // Consider absolute URIs greater than relative URIs
                return 1;
            }
            if (!a.IsAbsoluteUri && b.IsAbsoluteUri)
            {
                // Consider relative URIs less than absolute URIs
                return -1;
            }
            if (a.IsAbsoluteUri && b.IsAbsoluteUri)
            {
                // Comparisons for Absolute URIs are based on URI elements in the following order:
                // Scheme, UserInfo, Host the Port (all case insensitive except UserInfo)
                // Path, Query and Fragment (all case sensitive)

                int c = String.Compare(a.Scheme, b.Scheme, StringComparison.OrdinalIgnoreCase);
                if (c == 0)
                {
                    c = String.Compare(a.UserInfo, b.UserInfo, StringComparison.Ordinal);
                    if (c == 0)
                    {
                        c = String.Compare(a.Host, b.Host, StringComparison.OrdinalIgnoreCase);
                        if (c == 0)
                        {
                            c = a.Port.CompareTo(b.Port);
                            if (c == 0)
                            {
#if !SILVERLIGHT
                                c = String.Compare(a.PathAndQuery, b.PathAndQuery, StringComparison.Ordinal);
#else
                                c = String.Compare(a.PathAndQuery(), b.PathAndQuery(), StringComparison.Ordinal);
#endif
                                if (c == 0)
                                {
                                    c = String.Compare(a.Fragment, b.Fragment, StringComparison.Ordinal);
                                }
                            }
                        }
                    }
                }
                return c;
            }
            else
            {
                // Comparisons for Relative URIs are based on URI elements in the following order:
                // Path, Query and Fragment (all case sensitive)

#if !SILVERLIGHT
                int c = String.Compare(a.PathAndQuery, b.PathAndQuery, StringComparison.Ordinal);
#else
                int c = String.Compare(a.PathAndQuery(), b.PathAndQuery(), StringComparison.Ordinal);
#endif
                if (c == 0)
                {
                    c = String.Compare(a.Fragment, b.Fragment, StringComparison.Ordinal);
                }
                return c;
            }
        }

        /// <summary>
        /// Compares two URI Nodes
        /// </summary>
        /// <param name="a">First URI Node</param>
        /// <param name="b">Second URI Node</param>
        /// <returns></returns>
        public static int CompareUris(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            return b == null ? 1 : CompareUris(a.Uri, b.Uri);
        }

        /// <summary>
        /// Compares two Literal Nodes using global default comparison options where applicable
        /// </summary>
        /// <param name="a">First Literal Node</param>
        /// <param name="b">Second Literal Node</param>
        /// <returns></returns>
        public static int CompareLiterals(INode a, INode b)
        {
            return CompareLiterals(a, b, Options.DefaultCulture, Options.DefaultComparisonOptions);
        }

        /// <summary>
        /// Compares two Literal Nodes
        /// </summary>
        /// <param name="a">First Literal Node</param>
        /// <param name="b">Second Literal Node</param>
        /// <param name="culture">Culture to use for lexical string comparisons where more natural comparisons are not possible/applicable</param>
        /// <param name="comparisonOptions">String Comparison options used for lexical string comparisons where more natural comparisons are not possible/applicable</param>
        /// <returns></returns>
        public static int CompareLiterals(INode a, INode b, CultureInfo culture, CompareOptions comparisonOptions)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            if (b == null)
            {
                return 1;
            }

            // Initialize required culture and comparison options
            if (culture == null) culture = Options.DefaultCulture;
            if (comparisonOptions == CompareOptions.None) comparisonOptions = Options.DefaultComparisonOptions;

            //Literal Nodes are ordered based on Type and lexical form
            if (!a.HasDataType && b.HasDataType)
            {
                //Untyped Literals are less than Typed Literals
                //Return a -1 to indicate this
                return -1;
            }
            if (a.HasDataType && !b.HasDataType)
            {
                //Typed Literals are greater than Untyped Literals
                //Return a 1 to indicate this
                return 1;
            }
            if (a.HasLanguage)
            {
                if (b.HasLanguage)
                {
                    // Compare language tags if both language tagged
                    int c = String.Compare(a.Language, b.Language, culture, comparisonOptions);
                    // If different language tags return that comparison, otherwise compare by lexical value
                    return c != 0 ? c : String.Compare(a.Value, b.Value, culture, comparisonOptions);
                }
                // Language tagged literals are greater than plain literals but less than data typed literals
                return b.HasDataType ? -1 : 1;
            }
            if (b.HasLanguage)
            {
                // Language tagged literals are greated than plain literals but less than data typed literals
                return a.HasDataType ? 1 : -1;
            }
            if (!a.HasDataType && !b.HasDataType)
            {
                // Untyped and non-language tagged literals are compared by their lexical value
                return String.Compare(a.Value, b.Value, culture, comparisonOptions);
            }
            if (!EqualityHelper.AreUrisEqual(a.DataType, b.DataType))
            {
                // No way of ordering by value if the Data Types are different
                // Therefore order by Data Type Uri instead
                // This is required or the Value ordering between types won't occur correctly
                return CompareUris(a.DataType, b.DataType);
            }
            //Are we using a known and orderable DataType?
            String type = a.DataType.AbsoluteUri;
            if (!XmlSpecsHelper.IsSupportedType(type))
            {
                //Don't know how to order unsupported types so use specified order on the lexical value instead
                return String.Compare(a.Value, b.Value, culture, comparisonOptions);
            }
            try
            {
                switch (type)
                {
                    case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                        //Can use Lexical ordering for this so use specified order on the value
                        bool aBool, bBool;
                        if (Boolean.TryParse(a.Value, out aBool))
                        {
                            if (Boolean.TryParse(b.Value, out bBool))
                            {
                                return aBool.CompareTo(bBool);
                            }
                            return -1;
                        }
                        if (Boolean.TryParse(b.Value, out bBool))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeByte:
                        //Remember that xsd:byte is actually equivalent to SByte in .Net
                        //Extract the Byte Values and compare
                        sbyte aSByte, bSByte;
                        if (SByte.TryParse(a.Value, out aSByte))
                        {
                            if (SByte.TryParse(b.Value, out bSByte))
                            {
                                return aSByte.CompareTo(bSByte);
                            }
                            return -1;
                        }
                        if (SByte.TryParse(b.Value, out bSByte))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                        //Remember that xsd:unsignedByte is equivalent to Byte in .Net
                        //Extract the Byte Values and compare
                        byte aByte, bByte;
                        if (Byte.TryParse(a.Value, out aByte))
                        {
                            if (Byte.TryParse(b.Value, out bByte))
                            {
                                return aByte.CompareTo(bByte);
                            }
                            return -1;
                        }
                        if (Byte.TryParse(b.Value, out bByte))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeInt:
                    case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                    case XmlSpecsHelper.XmlSchemaDataTypeLong:
                    case XmlSpecsHelper.XmlSchemaDataTypeShort:
                        //Extract the Integer Values and compare
                        long aInt64, bInt64;
                        if (Int64.TryParse(a.Value, out aInt64))
                        {
                            if (Int64.TryParse(b.Value, out bInt64))
                            {
                                return aInt64.CompareTo(bInt64);
                            }
                            return -1;
                        }
                        if (Int64.TryParse(b.Value, out bInt64))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                    case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                        //Extract the Integer Values, ensure negative and compare
                        long aNegInt, bNegInt;
                        if (Int64.TryParse(a.Value, out aNegInt))
                        {
                            if (Int64.TryParse(b.Value, out bNegInt))
                            {
                                if (aNegInt >= 0)
                                {
                                    if (bNegInt >= 0)
                                    {
                                        goto default;
                                    }
                                    return 1;
                                }
                                if (bNegInt >= 0)
                                {
                                    return -1;
                                }
                                return aNegInt.CompareTo(bNegInt);
                            }
                            if (aNegInt >= 0)
                            {
                                goto default;
                            }
                            return -1;
                        }
                        if (Int64.TryParse(b.Value, out bNegInt))
                        {
                            if (bNegInt >= 0)
                            {
                                goto default;
                            }
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                    case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                    case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                    case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                    case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                        //Unsigned Integers
                        //Note that for NonNegativeInteger and PositiveInteger we don't need to do the
                        //same checking we have to do for their inverse types since parsing into an 
                        //Unsigned Long ensures that they must be positive
                        ulong aUInt64, bUInt64;
                        if (UInt64.TryParse(a.Value, out aUInt64))
                        {
                            if (UInt64.TryParse(b.Value, out bUInt64))
                            {
                                return aUInt64.CompareTo(bUInt64);
                            }
                            return -1;
                        }
                        if (UInt64.TryParse(b.Value, out bUInt64))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                        //Extract the Double Values and compare
                        double aDouble, bDouble;
                        if (Double.TryParse(a.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out aDouble))
                        {
                            if (Double.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bDouble))
                            {
                                return aDouble.CompareTo(bDouble);
                            }
                            return -1;
                        }
                        if (Double.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bDouble))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeDecimal:
                        //Extract the Decimal Values and compare
                        decimal aDecimal, bDecimal;
                        if (decimal.TryParse(a.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out aDecimal))
                        {
                            if (decimal.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bDecimal))
                            {
                                return aDecimal.CompareTo(bDecimal);
                            }
                            return -1;
                        }
                        if (decimal.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bDecimal))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                        //Extract the Float Values and compare
                        float aFloat, bFloat;
                        if (Single.TryParse(a.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out aFloat))
                        {
                            if (Single.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bFloat))
                            {
                                return aFloat.CompareTo(bFloat);
                            }
                            return -1;
                        }
                        if (Single.TryParse(b.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out bFloat))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeHexBinary:
                        //Extract the numeric value of the Hex encoded Binary and compare
                        long aHex, bHex;
                        if (Int64.TryParse(a.Value, NumberStyles.HexNumber, null, out aHex))
                        {
                            if (Int64.TryParse(b.Value, NumberStyles.HexNumber, null, out bHex))
                            {
                                return aHex.CompareTo(bHex);
                            }
                            return -1;
                        }
                        if (Int64.TryParse(b.Value, NumberStyles.HexNumber, null, out bHex))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeBase64Binary:
                        //Extract the numeric value of the Base 64 encoded Binary and compare
                        try
                        {
                            byte[] aBin = Convert.FromBase64String(a.Value);
                            try
                            {
                                byte[] bBin = Convert.FromBase64String(b.Value);

                                if (aBin.Length > bBin.Length)
                                {
                                    return 1;
                                }
                                if (aBin.Length < bBin.Length)
                                {
                                    return -1;
                                }
                                for (int i = 0; i < aBin.Length; i++)
                                {
                                    if (aBin[i] != bBin[i])
                                    {
                                        return aBin[i].CompareTo(bBin[i]);
                                    }
                                }
                                return 0;
                            }
                            catch
                            {
                                return -1;
                            }
                        }
                        catch
                        {
                            try
                            {
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                                // We just need to see whether the other value parses
                                // The actual value is irrelevant because the first value was illegal
                                // If it is valid then the valid value is greater than the invalid value
                                Convert.FromBase64String(b.Value);
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
                                return 1;
                            }
                            catch
                            {
                                goto default;
                            }
                        }

                    case XmlSpecsHelper.XmlSchemaDataTypeString:
                        //String Type
                        //Can use Lexical Ordering for thisgoto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeAnyUri:
                        //Uri Type
                        //Try and convert to a URI and use lexical ordering
                        try
                        {
                            Uri aUri = UriFactory.Create(a.Value);
                            try
                            {
                                Uri bUri = UriFactory.Create(b.Value);
                                return CompareUris(aUri, bUri);
                            }
                            catch
                            {
                                return -1;
                            }
                        }
                        catch
                        {
                            try
                            {
                                UriFactory.Create(b.Value);
                                return 1;
                            }
                            catch
                            {
                                goto default;
                            }
                        }

                    case XmlSpecsHelper.XmlSchemaDataTypeDate:
                    case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                        //Extract the Date Times and compare
                        DateTimeOffset aDateTimeOffset, bDateTimeOffset;
                        if (DateTimeOffset.TryParse(a.Value, out aDateTimeOffset))
                        {
                            if (DateTimeOffset.TryParse(b.Value, out bDateTimeOffset))
                            {
                                return aDateTimeOffset.CompareTo(bDateTimeOffset);
                            }
                            return -1;
                        }
                        if (DateTimeOffset.TryParse(b.Value, out bDateTimeOffset))
                        {
                            return 1;
                        }
                        goto default;

                    case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                    case XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration:
                        //Extract the TimeSpan's and compare
                        try
                        {
                            TimeSpan aTimeSpan = XmlConvert.ToTimeSpan(a.Value);
                            try
                            {
                                TimeSpan bTimeSpan = XmlConvert.ToTimeSpan(b.Value);
                                return aTimeSpan.CompareTo(bTimeSpan);
                            }
                            catch
                            {
                                return -1;
                            }
                        }
                        catch
                        {
                            try
                            {
                                XmlConvert.ToTimeSpan(b.Value);
                                return 1;
                            }
                            catch
                            {
                                goto default;
                            }
                        }

                    default:
                        //Don't know how to order so use lexical ordering on the value
                        return String.Compare(a.Value, b.Value, culture, comparisonOptions);
                }
            }
            catch
            {
                //There was some error suggesting a non-valid value for a type
                //e.g. "example"^^xsd:integer
                //In this case just use lexical ordering on the value
                return String.Compare(a.Value, b.Value, culture, comparisonOptions);
            }
        }

        /// <summary>
        /// Compares two Blank Nodes
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static int CompareBlankNodes(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            return b == null ? 1 : a.AnonID.CompareTo(b.AnonID);
        }

        /// <summary>
        /// Compares two Graph Literals
        /// </summary>
        /// <param name="a">First Graph Literal</param>
        /// <param name="b">Second Graph Literal</param>
        /// <returns></returns>
        public static int CompareGraphLiterals(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            return b == null ? 1 : a.SubGraph.Count.CompareTo(b.SubGraph.Count);
        }

        /// <summary>
        /// Compares two Variable Nodes
        /// </summary>
        /// <param name="a">First Variable Node</param>
        /// <param name="b">Second Variable Node</param>
        /// <returns></returns>
        public static int CompareVariables(INode a, INode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            return b == null ? 1 : String.Compare(a.VariableName, b.VariableName, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Equality comparer that compares URIs
    /// </summary>
    public class UriComparer
        : IComparer<Uri>, IEqualityComparer<Uri>
    {
        /// <summary>
        /// Compares two URIs
        /// </summary>
        /// <param name="x">URI</param>
        /// <param name="y">URI</param>
        /// <returns></returns>
        public int Compare(Uri x, Uri y)
        {
            return ComparisonHelper.CompareUris(x, y);
        }

        /// <summary>
        /// Determines whether two URIs are equal
        /// </summary>
        /// <param name="x">URI</param>
        /// <param name="y">URI</param>
        /// <returns></returns>
        public bool Equals(Uri x, Uri y)
        {
            return EqualityHelper.AreUrisEqual(x, y);
        }

        /// <summary>
        /// Gets the Hash Code for a URI
        /// </summary>
        /// <param name="obj">URI</param>
        /// <returns></returns>
        public int GetHashCode(Uri obj)
        {
            return obj == null ? 0 : obj.GetEnhancedHashCode();
        }
    }
}