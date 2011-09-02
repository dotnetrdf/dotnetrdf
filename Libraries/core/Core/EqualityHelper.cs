/*

Copyright Robert Vesse 2009-11
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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using VDS.RDF.Parsing;

namespace VDS.RDF
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
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            //URIs are equal if the Scheme, Host and Post are equivalent (case insensitive)
            //and the User Info (if any) is equivalent (case sensitive)
            //and the Path, Query and Fragment are equivalent (case sensitive)
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

        /// <summary>
        /// Determines whether two URIs are equal
        /// </summary>
        /// <param name="a">First URI Node</param>
        /// <param name="b">Second URI Node</param>
        /// <returns></returns>
        public static bool AreUrisEqual(IUriNode a, IUriNode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            return EqualityHelper.AreUrisEqual(a.Uri, b.Uri);
        }

        /// <summary>
        /// Determines whether two Literals are equal
        /// </summary>
        /// <param name="a">First Literal</param>
        /// <param name="b">Second Literal</param>
        /// <returns></returns>
        public static bool AreLiteralsEqual(ILiteralNode a, ILiteralNode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            //Language Tags must be equal (if present)
            //If they don't have language tags then they'll both be set to String.Empty which will give true
            if (a.Language.Equals(b.Language, StringComparison.OrdinalIgnoreCase))
            {
                //Datatypes must be equal (if present)
                //If they don't have Data Types then they'll both be null
                //Otherwise the URIs must be equal
                if (a.DataType == null && b.DataType == null)
                {
                    //Use String equality to get the result
                    return a.Value.Equals(b.Value, StringComparison.Ordinal);
                }
                else if (a.DataType == null)
                {
                    //We have a Null DataType but the other Node doesn't so can't be equal
                    return false;
                }
                else if (b.DataType == null)
                {
                    //The other Node has a Null DataType but we don't so can't be equal
                    return false;
                }
                else if (EqualityHelper.AreUrisEqual(a.DataType, b.DataType))
                {
                    //We have equal DataTypes so use String Equality to evaluate
                    if (Options.LiteralEqualityMode == LiteralEqualityMode.Strict)
                    {
                        //Strict Equality Mode uses Ordinal Lexical Comparison for Equality as per W3C RDF Spec
                        return a.Value.Equals(b.Value, StringComparison.Ordinal);
                    }
                    else
                    {
                        //Loose Equality Mode uses Value Based Comparison for Equality of Typed Nodes
                        return (a.CompareTo(b) == 0);
                    }
                }
                else
                {
                    //Data Types didn't match
                    return false;
                }
            }
            else
            {
                //Language Tags didn't match
                return false;
            }
        }

        /// <summary>
        /// Determines whether two Blank Nodes are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreBlankNodesEqual(IBlankNode a, IBlankNode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            return a.InternalID.Equals(b.InternalID) && ReferenceEquals(a.Graph, b.Graph);
        }

        /// <summary>
        /// Determines whether two Graph Literals are equal
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static bool AreGraphLiteralsEqual(IGraphLiteralNode a, IGraphLiteralNode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            return a.SubGraph.Equals(b.SubGraph);
        }

        /// <summary>
        /// Determines whether two Variable Nodes are equal
        /// </summary>
        /// <param name="a">First Variable Node</param>
        /// <param name="b">Second Variable Node</param>
        /// <returns></returns>
        public static bool AreVariablesEqual(IVariableNode a, IVariableNode b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null)
            {
                if (b == null) return true;
                return false;
            }
            else if (b == null)
            {
                return false;
            }

            return a.VariableName.Equals(b.VariableName, StringComparison.Ordinal);
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
            else if (b == null)
            {
                return 1;
            }

            //Comparisons are based on URI elements in the following order:
            //Scheme, UserInfo, Host the Port (all case insensitive except UserInfo)
            //Path, Query and Fragment (all case sensitive)

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

        /// <summary>
        /// Compares two URI Nodes
        /// </summary>
        /// <param name="a">First URI Node</param>
        /// <param name="b">Second URI Node</param>
        /// <returns></returns>
        public static int CompareUris(IUriNode a, IUriNode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }

            return ComparisonHelper.CompareUris(a.Uri, b.Uri);
        }

        /// <summary>
        /// Compares two Literal Nodes
        /// </summary>
        /// <param name="a">First Literal Node</param>
        /// <param name="b">Second Literal Node</param>
        /// <returns></returns>
        public static int CompareLiterals(ILiteralNode a, ILiteralNode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }

            //Literal Nodes are ordered based on Type and lexical form
            if (a.DataType == null && b.DataType != null)
            {
                //Untyped Literals are less than Typed Literals
                //Return a -1 to indicate this
                return -1;
            }
            else if (a.DataType != null && b.DataType == null)
            {
                //Typed Literals are greater than Untyped Literals
                //Return a 1 to indicate this
                return 1;
            }
            else if (a.DataType == null && b.DataType == null)
            {
                //If neither are typed use Lexical Ordering on the value
                return a.Value.CompareTo(b.Value);
            }
            else if (EqualityHelper.AreUrisEqual(a.DataType, b.DataType))
            {
                //Are we using a known and orderable DataType?
                String type = a.DataType.ToString();
                if (!XmlSpecsHelper.IsSupportedType(type))
                {
                    //Don't know how to order so use lexical order on the value
                    return a.Value.CompareTo(b.Value);
                }
                else
                {
                    try
                    {
                        switch (type)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                //Can use Lexical ordering for this
                                return a.Value.ToLower().CompareTo(b.Value.ToLower());

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
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (SByte.TryParse(b.Value, out bSByte))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

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
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Byte.TryParse(b.Value, out bByte))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

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
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Int64.TryParse(b.Value, out bInt64))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

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
                                                return a.Value.CompareTo(b.Value);
                                            }
                                            else
                                            {
                                                return 1;
                                            }
                                        }
                                        else if (bNegInt >= 0)
                                        {
                                            return -1;
                                        }
                                        else
                                        {
                                            return aNegInt.CompareTo(bNegInt);
                                        }
                                    }
                                    else if (aNegInt >= 0)
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Int64.TryParse(b.Value, out bNegInt))
                                    {
                                        if (bNegInt >= 0)
                                        {
                                            return a.Value.CompareTo(b.Value);
                                        }
                                        else
                                        {
                                            return 1;
                                        }
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

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
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (UInt64.TryParse(b.Value, out bUInt64))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                                //Extract the Double Values and compare
                                double aDouble, bDouble;
                                if (Double.TryParse(a.Value, out aDouble))
                                {
                                    if (Double.TryParse(b.Value, out bDouble))
                                    {
                                        return aDouble.CompareTo(bDouble);
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Double.TryParse(b.Value, out bDouble))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                                //Extract the Float Values and compare
                                float aFloat, bFloat;
                                if (Single.TryParse(a.Value, out aFloat))
                                {
                                    if (Single.TryParse(b.Value, out bFloat))
                                    {
                                        return aFloat.CompareTo(bFloat);
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Single.TryParse(b.Value, out bFloat))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            case XmlSpecsHelper.XmlSchemaDataTypeHexBinary:
                                //Extract the numeric value of the Hex encoded Binary and compare
                                long aHex, bHex;
                                if (Int64.TryParse(a.Value, System.Globalization.NumberStyles.HexNumber, null, out aHex))
                                {
                                    if (Int64.TryParse(b.Value, System.Globalization.NumberStyles.HexNumber, null, out bHex))
                                    {
                                        return aHex.CompareTo(bHex);
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (Int64.TryParse(b.Value, System.Globalization.NumberStyles.HexNumber, null, out bHex))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            case XmlSpecsHelper.XmlSchemaDataTypeBase64Binary:
                                //Extract the numeric value of the Base 64 encoded Binary and compare
                                byte[] aBin, bBin;
                                try
                                {
                                    aBin = Convert.FromBase64String(a.Value);
                                    try
                                    {
                                        bBin = Convert.FromBase64String(b.Value);

                                        if (aBin.Length > bBin.Length)
                                        {
                                            return 1;
                                        }
                                        else if (aBin.Length < bBin.Length)
                                        {
                                            return -1;
                                        }
                                        else
                                        {
                                            for (int i = 0; i < aBin.Length; i++)
                                            {
                                                if (aBin[i] != bBin[i])
                                                {
                                                    return aBin[i].CompareTo(bBin[i]);
                                                }
                                            }
                                            return 0;
                                        }
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
                                        bBin = Convert.FromBase64String(b.Value);
                                        return 1;
                                    }
                                    catch
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            case XmlSpecsHelper.XmlSchemaDataTypeString:
                                //String Type
                                //Can use Lexical Ordering for this
                                return a.Value.CompareTo(b.Value);

                            case XmlSpecsHelper.XmlSchemaDataTypeAnyUri:
                                //Uri Type
                                //Try and convert to a URI and use lexical ordering
                                Uri aUri, bUri;
                                try
                                {
                                    aUri = new Uri(a.Value);
                                    try
                                    {
                                        bUri = new Uri(b.Value);
                                        return ComparisonHelper.CompareUris(aUri, bUri);
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
                                        bUri = new Uri(b.Value);
                                        return 1;
                                    }
                                    catch
                                    {
                                        return a.Value.CompareTo(b.Value);
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
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (DateTimeOffset.TryParse(b.Value, out bDateTimeOffset))
                                    {
                                        return 1;
                                    }
                                    else
                                    {
                                        return a.Value.CompareTo(b.Value);
                                    }
                                }

                            default:
                                //Don't know how to order so use lexical order
                                return a.Value.CompareTo(b.Value);
                        }
                    }
                    catch
                    {
                        //There was some error suggesting a non-valid value for a type
                        //e.g. "example"^^xsd:integer
                        //In this case just use Lexical Ordering
                        return a.Value.CompareTo(b.Value);
                    }
                }
            }
            else
            {
                //No way of ordering by value if the Data Types are different
                //Order by Data Type Uri
                //This is required or the Value ordering between types won't occur correctly
                return ComparisonHelper.CompareUris(a.DataType, b.DataType);
            }
        }

        /// <summary>
        /// Compares two Blank Nodes
        /// </summary>
        /// <param name="a">First Blank Node</param>
        /// <param name="b">Second Blank Node</param>
        /// <returns></returns>
        public static int CompareBlankNodes(IBlankNode a, IBlankNode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }

            return a.InternalID.CompareTo(b.InternalID);
        }

        /// <summary>
        /// Compares two Graph Literals
        /// </summary>
        /// <param name="a">First Graph Literal</param>
        /// <param name="b">Second Graph Literal</param>
        /// <returns></returns>
        public static int CompareGraphLiterals(IGraphLiteralNode a, IGraphLiteralNode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }

            return a.SubGraph.Triples.Count.CompareTo(b.SubGraph.Triples.Count);
        }

        /// <summary>
        /// Compares two Variable Nodes
        /// </summary>
        /// <param name="a">First Variable Node</param>
        /// <param name="b">Second Variable Node</param>
        /// <returns></returns>
        public static int CompareVariables(IVariableNode a, IVariableNode b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null)
            {
                if (b == null) return 0;
                return -1;
            }
            else if (b == null)
            {
                return 1;
            }

            return String.Compare(a.VariableName, b.VariableName, StringComparison.Ordinal);
        }
    }
}
