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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Literal Nodes
    /// </summary>
    public class LiteralNode : BaseNode, IComparable<LiteralNode>
    {
        private String _value;
        private String _language;
        private Uri _datatype;

        /// <summary>
        /// Constants used to add salt to the hashes of different Literal Nodes
        /// </summary>
        private const String LangSpecLiteralHashCodeSalt = "languageSpecified",
                             DataTypedLiteralHashCodeSalt = "typed",
                             PlainLiteralHashCodeSalt = "plain";

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        protected internal LiteralNode(IGraph g, String literal)
            : this(g, literal, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal LiteralNode(IGraph g, String literal, bool normalize)
            : base(g, NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
            this._value = literal.Normalize();
#else
            this._value = literal;
#endif
            } 
            else 
            {
                this._value = literal;
            }
            this._language = String.Empty;
            this._datatype = null;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString() + PlainLiteralHashCodeSalt).GetHashCode();
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        protected internal LiteralNode(IGraph g, String literal, String langspec)
            : this(g, literal, langspec, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal LiteralNode(IGraph g, String literal, String langspec, bool normalize)
            : base(g, NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
                this._value = literal.Normalize();
#else
            this._value = literal;
#endif
            }
            else
            {
                this._value = literal;
            }
            this._language = langspec;
            this._datatype = null;

            //Compute Hash Code
            if (langspec.Equals(String.Empty))
            {
                //Empty Language Specifier equivalent to a Plain Literal
                this._hashcode = (this._nodetype + this.ToString() + PlainLiteralHashCodeSalt).GetHashCode();
            }
            else
            {
                this._hashcode = (this._nodetype + this.ToString() + LangSpecLiteralHashCodeSalt).GetHashCode();
            }
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        protected internal LiteralNode(IGraph g, String literal, Uri datatype)
            : this(g, literal, datatype, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal LiteralNode(IGraph g, String literal, Uri datatype, bool normalize)
            : base(g, NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
                this._value = literal.Normalize();
#else
            this._value = literal;
#endif
            }
            else
            {
                this._value = literal;
            }
            this._language = String.Empty;
            this._datatype = datatype;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString() + DataTypedLiteralHashCodeSalt).GetHashCode();
        }

        /// <summary>
        /// Gives the String Value of the Literal
        /// </summary>
        public String Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gives the Language Specifier for the Literal (if it exists) or the Empty String
        /// </summary>
        public String Language
        {
            get
            {
                return _language;
            }
        }

        /// <summary>
        /// Gives the Data Type Uri for the Literal (if it exists) or a null
        /// </summary>
        public Uri DataType
        {
            get
            {
                return _datatype;
            }
        }

        /// <summary>
        /// Implementation of the Equals method for Literal Nodes
        /// </summary>
        /// <param name="obj">Object to compare the Node with</param>
        /// <returns></returns>
        /// <remarks>
        /// The default behaviour is for Literal Nodes to be considered equal IFF
        /// <ol>
        /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
        /// <li>Their Data Types are identical (or neither has a Data Type)</li>
        /// <li>Their String values are identical</li>
        /// </ol>
        /// This behaviour can be overridden to use value equality by setting the <see cref="Options.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            else
            {
                //Can only be equal to other Nodes
                return false;
            }
        }

        /// <summary>
        /// Implementation of the Equals method for Literal Nodes
        /// </summary>
        /// <param name="other">Object to compare the Node with</param>
        /// <returns></returns>
        /// <remarks>
        /// The default behaviour is for Literal Nodes to be considered equal IFF
        /// <ol>
        /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
        /// <li>Their Data Types are identical (or neither has a Data Type)</li>
        /// <li>Their String values are identical</li>
        /// </ol>
        /// This behaviour can be overridden to use value equality by setting the <see cref="Options.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
        /// </remarks>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Literal)
            {
                LiteralNode temp = (LiteralNode)other;

                //Language Tags must be equal (if present)
                //If they don't have language tags then they'll both be set to String.Empty which will give true
                if (this._language.Equals(temp.Language, StringComparison.OrdinalIgnoreCase))
                {
                    //Datatypes must be equal (if present)
                    //If they don't have Data Types then they'll both be null
                    //Otherwise the URIs must be equal
                    if (this._datatype == null && temp.DataType == null)
                    {
                        //Use String equality to get the result
                        return this._value.Equals(temp.Value, StringComparison.Ordinal);
                    }
                    else if (this._datatype == null)
                    {
                        //We have a Null DataType but the other Node doesn't so can't be equal
                        return false;
                    }
                    else if (temp.DataType == null)
                    {
                        //The other Node has a Null DataType but we don't so can't be equal
                        return false;
                    }
                    else if (this._datatype.ToString().Equals(temp.DataType.ToString(), StringComparison.Ordinal))
                    {
                        //We have equal DataTypes so use String Equality to evaluate
                        if (Options.LiteralEqualityMode == LiteralEqualityMode.Strict)
                        {
                            //Strict Equality Mode uses Ordinal Lexical Comparison for Equality as per W3C RDF Spec
                            return this._value.Equals(temp.Value, StringComparison.Ordinal);
                        }
                        else
                        {
                            //Loose Equality Mode uses Value Based Comparison for Equality of Typed Nodes
                            return (this.CompareTo(temp) == 0);
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
            else
            {
                //Can only be equal to a LiteralNode
                return false;
            }
        }

        /// <summary>
        /// Implementation of Compare To for Literal Nodes
        /// </summary>
        /// <param name="other">Literal Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(LiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

        /// <summary>
        /// Gets a String representation of a Literal Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>Gives a value without quotes (as some syntaxes use) with the Data Type/Language Specifier appended using Notation 3 syntax</remarks>
        public override string ToString()
        {
            StringBuilder stringOut = new StringBuilder();
            stringOut.Append(this._value);
            if (!this._language.Equals(String.Empty))
            {
                stringOut.Append("@");
                stringOut.Append(this._language);
            }
            else if (!(this._datatype == null))
            {
                stringOut.Append("^^");
                stringOut.Append(this._datatype.ToString());
            }

            return stringOut.ToString();
        }

        /// <summary>
        /// Implementation of CompareTo for Literal Nodes
        /// </summary>
        /// <param name="other">Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Literal Nodes are greater than Blank Nodes, Uri Nodes and Nulls, they are less than Graph Literal Nodes.
        /// <br /><br />
        /// Two Literal Nodes are initially compared based upon Data Type, untyped literals are less than typed literals.  Two untyped literals are compared purely on lexical value, Language Specifier has no effect on the ordering.  This means Literal Nodes are only partially ordered, for example "hello"@en and "hello"@en-us are considered to be the same for ordering purposes though they are different for equality purposes.  Datatyped Literals can only be properly ordered if they are one of a small subset of types (Integers, Booleans, Date Times, Strings and URIs).  If the datatypes for two Literals are non-matching they are ordered on Datatype Uri, this ensures that each range of Literal Nodes is sorted to some degree.  Again this also means that Literals are partially ordered since unknown datatypes will only be sorted based on lexical value and not on actual value.
        /// </remarks>
        public override int CompareTo(INode other)
        {
            if (other == null)
            {
                //Everything is greater than a null
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Blank || other.NodeType == NodeType.Variable || other.NodeType == NodeType.Uri)
            {
                //Literal Nodes are greater than Blank, Variable and Uri Nodes
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Literal)
            {
                if (ReferenceEquals(this, other)) return 0;

                //Literal Nodes are ordered based on Type and lexical form
                LiteralNode l = (LiteralNode)other;
                if (this._datatype == null && !(l.DataType == null))
                {
                    //Untyped Literals are less than Typed Literals
                    //Return a -1 to indicate this
                    return -1;
                }
                else if (!(this._datatype == null) && l.DataType == null)
                {
                    //Typed Literals are greater than Untyped Literals
                    //Return a 1 to indicate this
                    return 1;
                }
                else if (this._datatype == null && l.DataType == null)
                {
                    //If neither are typed use Lexical Ordering
                    return this._value.CompareTo(l.Value);
                }
                else if (this._datatype.ToString().Equals(l.DataType.ToString()))
                {
                    //Are we using a known and orderable DataType?
                    String type = this._datatype.ToString();
                    if (!XmlSpecsHelper.IsSupportedType(type))
                    {
                        //Don't know how to order so use lexical order
                        return this._value.CompareTo(l.Value);
                    }
                    else
                    {
                        try
                        {
                            switch (type)
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                    //Can use Lexical ordering for this
                                    return this._value.ToLower().CompareTo(l.Value.ToLower());

                                case XmlSpecsHelper.XmlSchemaDataTypeByte:
                                    //Remember that xsd:byte is actually equivalent to SByte in .Net
                                    //Extract the Byte Values and compare
                                    sbyte aSByte, bSByte;
                                    aSByte = SByte.Parse(this._value);
                                    bSByte = SByte.Parse(l.Value);

                                    return aSByte.CompareTo(bSByte);

                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                                    //Remember that xsd:unsignedByte is equivalent to Byte in .Net
                                    //Extract the Byte Values and compare
                                    byte aByte, bByte;
                                    aByte = Byte.Parse(this._value);
                                    bByte = Byte.Parse(l.Value);

                                    return aByte.CompareTo(bByte);

                                case XmlSpecsHelper.XmlSchemaDataTypeInt:
                                case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypeLong:
                                case XmlSpecsHelper.XmlSchemaDataTypeShort:
                                    //Extract the Integer Values and compare
                                    long aInt, bInt;
                                    aInt = Int64.Parse(this._value);
                                    bInt = Int64.Parse(l.Value);

                                    return aInt.CompareTo(bInt);

                                case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                                    //Extract the Integer Values, ensure negative and compare
                                    long aNegInt, bNegInt;
                                    aNegInt = Int64.Parse(this._value);
                                    bNegInt = Int64.Parse(l.Value);

                                    if (aNegInt >= 0 || bNegInt >= 0) throw new RdfException("One of the Negative/Non-Positive Integers has a Positive Value");

                                    return aNegInt.CompareTo(bNegInt);

                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                                case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                                    //Unsigned Integers
                                    ulong aUInt, bUInt;
                                    aUInt = UInt64.Parse(this._value);
                                    bUInt = UInt64.Parse(l.Value);

                                    return aUInt.CompareTo(bUInt);

                                case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                                    //Extract the Double Values and compare
                                    double aDbl, bDbl;
                                    aDbl = Double.Parse(this._value);
                                    bDbl = Double.Parse(l.Value);

                                    return aDbl.CompareTo(bDbl);

                                case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                                    //Extract the Float Values and compare
                                    float aFlt, bFlt;
                                    aFlt = Single.Parse(this._value);
                                    bFlt = Single.Parse(l.Value);

                                    return aFlt.CompareTo(bFlt);

                                case XmlSpecsHelper.XmlSchemaDataTypeHexBinary:
                                    //Extract the numeric value of the Hex encoded Binary and compare
                                    long aHex, bHex;
                                    aHex = Convert.ToInt64(this._value, 16);
                                    bHex = Convert.ToInt64(l.Value, 16);

                                    return aHex.CompareTo(bHex);

                                case XmlSpecsHelper.XmlSchemaDataTypeBase64Binary:
                                    //Extract the numeric value of the Base 64 encoded Binary and compare
                                    byte[] aBin = Convert.FromBase64String(this._value);
                                    byte[] bBin = Convert.FromBase64String(l.Value);

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

                                case XmlSpecsHelper.XmlSchemaDataTypeString:
                                case XmlSpecsHelper.XmlSchemaDataTypeAnyUri:
                                    //Uri or String Type
                                    //Can use Lexical Ordering for this
                                    return this._value.CompareTo(l.Value);

                                case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                    //Extract the Date Times and compare
                                    DateTime aDate, bDate;
                                    aDate = DateTime.Parse(this._value);
                                    bDate = DateTime.Parse(l.Value);

                                    return aDate.CompareTo(bDate);

                                default:
                                    //Don't know how to order so use lexical order
                                    return this._value.CompareTo(l.Value);
                            }
                        }
                        catch
                        {
                            //There was some error suggesting a non-valid value for a type
                            //e.g. "example"^^xsd:integer
                            //In this case just use Lexical Ordering
                            return this._value.CompareTo(l.Value);
                        }
                    }
                }
                else
                {
                    //No way of ordering by value if the Data Types are different
                    //Order by Data Type Uri
                    //This is required or the Value ordering between types won't occur correctly
                    return this._datatype.ToString().CompareTo(l.DataType.ToString());
                }
            }
            else
            {
                //Anything else is considered greater than a Literal Node
                //Return -1 to indicate this
                return -1;
            }
        }
    }

    /// <summary>
    /// Class for representing Literal Nodes where the Literal values are not normalized
    /// </summary>
    class NonNormalizedLiteralNode : LiteralNode, IComparable<NonNormalizedLiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        protected internal NonNormalizedLiteralNode(IGraph g, String literal)
            : base(g, literal, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">Lanaguage Specifier for the Literal</param>
        protected internal NonNormalizedLiteralNode(IGraph g, String literal, String langspec)
            : base(g, literal, langspec, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        protected internal NonNormalizedLiteralNode(IGraph g, String literal, Uri datatype)
            : base(g, literal, datatype, false) { }

        /// <summary>
        /// Implementation of Compare To for Literal Nodes
        /// </summary>
        /// <param name="other">Literal Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(NonNormalizedLiteralNode other)
        {
            return this.CompareTo((INode)other);
        }
    }
}
