/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Literal Nodes.
    /// </summary>
    [Serializable,XmlRoot(ElementName="literal")]
    public abstract class BaseLiteralNode 
        : BaseNode, ILiteralNode, IEquatable<BaseLiteralNode>, IComparable<BaseLiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal BaseLiteralNode(IGraph g, string literal, bool normalize)
            : base(g, NodeType.Literal)
        {
            Value = normalize ? literal.Normalize() : literal;
            DataType = UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
            _hashcode = ComputeHashCode();
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal BaseLiteralNode(IGraph g, string literal, string langspec, bool normalize)
            : base(g, NodeType.Literal)
        {
            if (normalize)
            {
                Value = literal.Normalize();
            }
            else
            {
                Value = literal;
            }
            Language = langspec != null ? langspec.ToLowerInvariant() : string.Empty;

            // Compute Hash Code
            if (Language.Equals(string.Empty))
            {
                // Empty Language Specifier equivalent to a string literal
                DataType = UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
                _hashcode = ComputeHashCode();
            }
            else
            {
                DataType = UriFactory.Create(RdfSpecsHelper.RdfLangString);
                _hashcode = ComputeHashCode();
            }
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="datatype">Uri for the Literals Data Type.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal BaseLiteralNode(IGraph g, string literal, Uri datatype, bool normalize)
            : base(g, NodeType.Literal)
        {
            Value = normalize ? literal.Normalize() : literal;
            DataType = datatype;

            // Compute Hash Code
            _hashcode = ComputeHashCode();
        }

        /// <summary>
        /// Deserialization Only Constructor.
        /// </summary>
        protected BaseLiteralNode()
            : base(null, NodeType.Literal) { }

        /// <summary>
        /// Deserialization Constructor.
        /// </summary>
        /// <param name="info">Serialization Information.</param>
        /// <param name="context">Streaming Context.</param>
        protected BaseLiteralNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.Literal)
        {
            Value = info.GetString("value");
            Language = info.GetString("lang");
            DataType = UriFactory.Create(info.GetString("datatype"));
            _hashcode = ComputeHashCode();

        }

        /// <summary>
        /// Gives the String Value of the Literal.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gives the Language Specifier for the Literal (if it exists) or the Empty String.
        /// </summary>
        public string Language { get; private set; } = string.Empty;

        /// <summary>
        /// Gives the Data Type Uri for the Literal (if it exists) or a null.
        /// </summary>
        public Uri DataType { get; private set; }

        /// <summary>
        /// Implementation of the Equals method for Literal Nodes.
        /// </summary>
        /// <param name="obj">Object to compare the Node with.</param>
        /// <returns></returns>
        /// <remarks>
        /// The default behaviour is for Literal Nodes to be considered equal IFF
        /// <ol>
        /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
        /// <li>Their Data Types are identical (or neither has a Data Type)</li>
        /// <li>Their String values are identical</li>
        /// </ol>
        /// This behaviour can be overridden to use value equality by setting the <see cref="EqualityHelper.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            return obj is INode node && Equals(node);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private int ComputeHashCode()
        {
            return (Value, DataType, Language).GetHashCode();
        }

        /// <summary>
        /// Implementation of the Equals method for Literal Nodes.
        /// </summary>
        /// <param name="other">Object to compare the Node with.</param>
        /// <returns></returns>
        /// <remarks>
        /// The default behaviour is for Literal Nodes to be considered equal IFF
        /// <ol>
        /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
        /// <li>Their Data Types are identical (or neither has a Data Type)</li>
        /// <li>Their String values are identical</li>
        /// </ol>
        /// This behaviour can be overridden to use value equality by setting the <see cref="EqualityHelper.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
        /// </remarks>
        public override bool Equals(INode other)
        {
            if ((object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Literal)
            {
                return Equals((ILiteralNode)other);
            }
            else
            {
                // Can only be equal to a LiteralNode
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to a Blank Node (should always be false).
        /// </summary>
        /// <param name="other">Blank Node.</param>
        /// <returns></returns>
        public override bool Equals(IBlankNode other)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node (should always be false).
        /// </summary>
        /// <param name="other">Graph Literal Node.</param>
        /// <returns></returns>
        public override bool Equals(IGraphLiteralNode other)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node.
        /// </summary>
        /// <param name="other">Literal Node.</param>
        /// <returns></returns>
        public override bool Equals(ILiteralNode other)
        {
            if (other == null) return false;

            return EqualityHelper.AreLiteralsEqual(this, other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node (should always be false).
        /// </summary>
        /// <param name="other">URI Node.</param>
        /// <returns></returns>
        public override bool Equals(IUriNode other)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node (should always be false).
        /// </summary>
        /// <param name="other">Variable Node.</param>
        /// <returns></returns>
        public override bool Equals(IVariableNode other)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node.
        /// </summary>
        /// <param name="other">Literal Node.</param>
        /// <returns></returns>
        public bool Equals(BaseLiteralNode other)
        {
            return Equals((ILiteralNode)other);
        }

        /// <summary>
        /// Gets a String representation of a Literal Node.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Gives a value without quotes (as some syntaxes use) with the Data Type/Language Specifier appended using Notation 3 syntax.</remarks>
        public override string ToString()
        {
            StringBuilder stringOut = new StringBuilder();
            stringOut.Append(Value);
            if (!Language.Equals(string.Empty))
            {
                stringOut.Append("@");
                stringOut.Append(Language);
            }
            else if (!(DataType == null))
            {
                stringOut.Append("^^");
                stringOut.Append(DataType.AbsoluteUri);
            }

            return stringOut.ToString();
        }

        /// <summary>
        /// Implementation of CompareTo for Literal Nodes.
        /// </summary>
        /// <param name="other">Node to Compare To.</param>
        /// <returns></returns>
        /// <remarks>
        /// Literal Nodes are greater than Blank Nodes, Uri Nodes and Nulls, they are less than Graph Literal Nodes.
        /// <br /><br />
        /// Two Literal Nodes are initially compared based upon Data Type, untyped literals are less than typed literals.  Two untyped literals are compared purely on lexical value, Language Specifier has no effect on the ordering.  This means Literal Nodes are only partially ordered, for example "hello"@en and "hello"@en-us are considered to be the same for ordering purposes though they are different for equality purposes.  Datatyped Literals can only be properly ordered if they are one of a small subset of types (Integers, Booleans, Date Times, Strings and URIs).  If the datatypes for two Literals are non-matching they are ordered on Datatype Uri, this ensures that each range of Literal Nodes is sorted to some degree.  Again this also means that Literals are partially ordered since unknown datatypes will only be sorted based on lexical value and not on actual value.
        /// </remarks>
        public override int CompareTo(INode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                // Everything is greater than a null
                // Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Blank || other.NodeType == NodeType.Variable || other.NodeType == NodeType.Uri)
            {
                // Literal Nodes are greater than Blank, Variable and Uri Nodes
                // Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Literal)
            {
                return CompareTo((ILiteralNode)other);
            }
            else
            {
                // Anything else is considered greater than a Literal Node
                // Return -1 to indicate this
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public override int CompareTo(IBlankNode other)
        {
            // We are always greater than nulls/Blank Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ComparisonHelper.CompareLiterals(this, other);
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public override int CompareTo(IGraphLiteralNode other)
        {
            if (other == null)
            {
                // We are always greater than nulls
                return 1;
            }
            else
            {
                // Graph Literals are always greater than us
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public override int CompareTo(IUriNode other)
        {
            // We are always greater than nulls/URI Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public override int CompareTo(IVariableNode other)
        {
            // We are always greater than nulls/Variable Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node.
        /// </summary>
        /// <param name="other">Node to test against.</param>
        /// <returns></returns>
        public int CompareTo(BaseLiteralNode other)
        {
            return CompareTo((ILiteralNode)other);
        }


        #region ISerializable Members

        /// <summary>
        /// Gets the serialization information.
        /// </summary>
        /// <param name="info">Serialization Information.</param>
        /// <param name="context">Streaming Context.</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Value);
            info.AddValue("lang", Language);
            info.AddValue("datatype", DataType.AbsoluteUri);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reads the data for XML deserialization.
        /// </summary>
        /// <param name="reader">XML Reader.</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    switch (reader.Name)
                    {
                        case "lang":
                            Language = reader.Value;
                            break;
                        case "datatype":
                            DataType = UriFactory.Create(reader.Value);
                            break;
                    }
                }
            }
            reader.MoveToContent();
            Value = reader.ReadElementContentAsString();
            _hashcode = ComputeHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization.
        /// </summary>
        /// <param name="writer">XML Writer.</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            if (DataType != null)
            {
                writer.WriteAttributeString("datatype", DataType.AbsoluteUri);
            }
            if (!Language.Equals(string.Empty))
            {
                writer.WriteAttributeString("lang", Language);
            }
            writer.WriteString(Value);
        }

        #endregion

    }

    /// <summary>
    /// Class for representing Literal Nodes.
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="literal")]
#endif
    public class LiteralNode
        : BaseLiteralNode, IEquatable<LiteralNode>, IComparable<LiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal LiteralNode(IGraph g, string literal, bool normalize)
            : base(g, literal, normalize) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal LiteralNode(IGraph g, string literal, string langspec, bool normalize)
            : base(g, literal, langspec, normalize) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="datatype">Uri for the Literals Data Type.</param>
        /// <param name="normalize">Whether to Normalize the Literal Value.</param>
        protected internal LiteralNode(IGraph g, string literal, Uri datatype, bool normalize)
            : base(g, literal, datatype, normalize) { }

        /// <summary>
        /// Deserialization Only Constructor.
        /// </summary>
        protected LiteralNode()
            : base() { }

#if !NETCORE
        /// <summary>
        /// Deserialization Constructor.
        /// </summary>
        /// <param name="info">Serialization Information.</param>
        /// <param name="context">Streaming Context.</param>
        protected LiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        /// <summary>
        /// Implementation of Compare To for Literal Nodes.
        /// </summary>
        /// <param name="other">Literal Node to Compare To.</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method.
        /// </remarks>
        public int CompareTo(LiteralNode other)
        {
            return CompareTo((ILiteralNode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node.
        /// </summary>
        /// <param name="other">Literal Node.</param>
        /// <returns></returns>
        public bool Equals(LiteralNode other)
        {
            return base.Equals((ILiteralNode)other);
        }
    }

    /// <summary>
    /// Class for representing Literal Nodes where the Literal values are not normalized.
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="literal")]
#endif
    class NonNormalizedLiteralNode 
        : LiteralNode, IComparable<NonNormalizedLiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        protected internal NonNormalizedLiteralNode(IGraph g, string literal)
            : base(g, literal, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="langspec">Lanaguage Specifier for the Literal.</param>
        protected internal NonNormalizedLiteralNode(IGraph g, string literal, string langspec)
            : base(g, literal, langspec, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes.
        /// </summary>
        /// <param name="g">Graph this Node is in.</param>
        /// <param name="literal">String value of the Literal.</param>
        /// <param name="datatype">Uri for the Literals Data Type.</param>
        protected internal NonNormalizedLiteralNode(IGraph g, string literal, Uri datatype)
            : base(g, literal, datatype, false) { }

        /// <summary>
        /// Deserialization Only Constructor.
        /// </summary>
        protected NonNormalizedLiteralNode()
            : base() { }

#if !NETCORE
        /// <summary>
        /// Deserialization Constructor.
        /// </summary>
        /// <param name="info">Serialization Information.</param>
        /// <param name="context">Streaming Context.</param>
        protected NonNormalizedLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        /// <summary>
        /// Implementation of Compare To for Literal Nodes.
        /// </summary>
        /// <param name="other">Literal Node to Compare To.</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method.
        /// </remarks>
        public int CompareTo(NonNormalizedLiteralNode other)
        {
            return CompareTo((ILiteralNode)other);
        }
    }
}
