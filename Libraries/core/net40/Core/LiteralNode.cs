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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using System.Threading;
using System.Globalization;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Literal Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="literal")]
#endif
    public abstract class BaseLiteralNode 
        : BaseNode, IEquatable<BaseLiteralNode>, IComparable<BaseLiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        protected internal BaseLiteralNode(String literal)
            : this(literal, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal BaseLiteralNode(String literal, bool normalize)
            : base(NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
            this.Value = literal.Normalize();
#else
            this.Value = literal;
#endif
            } 
            else 
            {
                this.Value = literal;
            }

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        protected internal BaseLiteralNode(String literal, String langspec)
            : this(literal, langspec, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal BaseLiteralNode(String literal, String langspec, bool normalize)
            : base(NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
                this.Value = literal.Normalize();
#else
            this.Value = literal;
#endif
            }
            else
            {
                this.Value = literal;
            }
            this.Language = String.IsNullOrEmpty(langspec) ? null : langspec;
            // TODO: This should be set to rdf:langString for RDF 1.1 compliance
            this.DataType = null;

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        protected internal BaseLiteralNode(String literal, Uri datatype)
            : this(literal, datatype, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        protected internal BaseLiteralNode(String literal, Uri datatype, bool normalize)
            : base(NodeType.Literal)
        {
            if (normalize)
            {
#if !NO_NORM
                this.Value = literal.Normalize();
#else
            this.Value = literal;
#endif
            }
            else
            {
                this.Value = literal;
            }
            this.DataType = datatype;

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected BaseLiteralNode()
            : base(NodeType.Literal) { }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseLiteralNode(SerializationInfo info, StreamingContext context)
            : base(NodeType.Literal)
        {
            this.Value = info.GetString("value");
            byte mode = info.GetByte("mode");
            switch (mode)
            {
                case 0:
                    //Nothing more to do - plain literal
                    break;
                case 1:
                    //Get the Language
                    this.Language = info.GetString("lang");
                    if (this.Language.Equals(String.Empty)) this.Language = null;
                    break;
                case 2:
                    //Get the Datatype
                    this.DataType = UriFactory.Create(info.GetString("datatype"));
                    break;
                default:
                    throw new RdfException("Unable to deserialize a Literal Node");
            }
            this._hashcode = Tools.CreateHashCode(this);
        }

#endif

        /// <summary>
        /// Gives the lexical value of the literal
        /// </summary>
        public override String Value { get; protected set; }

        /// <summary>
        /// Gets whether the literal has a language specifier
        /// </summary>
        public override bool HasLanguage
        {
            get { return !String.IsNullOrEmpty(this.Language); }
        }

        /// <summary>
        /// Gives the alnguage specifier for the literal (if it exists) or null
        /// </summary>
        public override String Language { get; protected set; }

        /// <summary>
        /// Gets whether the literal has a data type URI
        /// </summary>
        public override bool HasDataType
        {
            get { return !ReferenceEquals(this.DataType, null); }
        }

        /// <summary>
        /// Gives the data type URI for the literal (if it exists) or null
        /// </summary>
        public override Uri DataType { get; protected set; }

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
                return this.Equals((INode)other);
            }
            else
            {
                //Can only be equal to a LiteralNode
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node
        /// </summary>
        /// <param name="other">Literal Node</param>
        /// <returns></returns>
        public bool Equals(BaseLiteralNode other)
        {
            return this.Equals((INode)other);
        }

        /// <summary>
        /// Gets a String representation of a Literal Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>Gives a value without quotes (as some syntaxes use) with the Data Type/Language Specifier appended using Notation 3 syntax</remarks>
        public override string ToString()
        {
            StringBuilder stringOut = new StringBuilder();
            stringOut.Append(this.Value);
            if (!this.HasLanguage)
            {
                stringOut.Append("@");
                stringOut.Append(this.Language);
            }
            else if (this.HasDataType)
            {
                stringOut.Append("^^");
                stringOut.Append(this.DataType.AbsoluteUri);
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
            if (ReferenceEquals(this, other)) return 0;

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
                return this.CompareTo((INode)other);
            }
            else
            {
                //Anything else is considered greater than a Literal Node
                //Return -1 to indicate this
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseLiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

#if !SILVERLIGHT

        #region ISerializable Members

        /// <summary>
        /// Gets the serialization information
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", this.Value);
            if (this.HasLanguage)
            {
                info.AddValue("mode", (byte)1);
                info.AddValue("lang", this.Language);
            }
            else if (this.HasDataType)
            {
                info.AddValue("mode", (byte)2);
                info.AddValue("datatype", this.DataType.AbsoluteUri);
            }
            else
            {
                info.AddValue("mode", (byte)0);
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                bool exit = false;
                while (!exit && reader.MoveToNextAttribute())
                {
                    switch (reader.Name)
                    {
                        case "lang":
                            this.Language = reader.Value;
                            exit = true;
                            break;
                        case "datatype":
                            this.DataType = UriFactory.Create(reader.Value);
                            exit = true;
                            break;
                    }
                }
            }
            reader.MoveToContent();
            this.Value = reader.ReadElementContentAsString();
            this._hashcode = Tools.CreateHashCode(this);
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            if (this.HasLanguage)
            {
                writer.WriteAttributeString("lang", this.Language);
            }
            else if (this.HasDataType)
            {
                writer.WriteAttributeString("datatype", this.DataType.AbsoluteUri);
            }
            writer.WriteString(this.Value);
        }

        #endregion

#endif
    }

    /// <summary>
    /// Class for representing Literal Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="literal")]
#endif
    public class LiteralNode
        : BaseLiteralNode, IEquatable<LiteralNode>, IComparable<LiteralNode>
    {
        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        public LiteralNode(String literal)
            : this(literal, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        public LiteralNode(String literal, bool normalize)
            : base(literal, normalize) { }

        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        public LiteralNode(String literal, String langspec)
            : this(literal, langspec, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">String value for the Language Specifier for the Literal</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        public LiteralNode(String literal, String langspec, bool normalize)
            : base(literal, langspec, normalize) { }

        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        public LiteralNode(String literal, Uri datatype)
            : this(literal, datatype, Options.LiteralValueNormalization) { }

        /// <summary>
        /// Constructor for Literal Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        /// <param name="normalize">Whether to Normalize the Literal Value</param>
        public LiteralNode(String literal, Uri datatype, bool normalize)
            : base(literal, datatype, normalize) { }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected LiteralNode()
            : base() { }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected LiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

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
        /// Determines whether this Node is equal to a Literal Node
        /// </summary>
        /// <param name="other">Literal Node</param>
        /// <returns></returns>
        public bool Equals(LiteralNode other)
        {
            return base.Equals((INode)other);
        }
    }

    /// <summary>
    /// Class for representing Literal Nodes where the Literal values are not normalized
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="literal")]
#endif
    class NonNormalizedLiteralNode 
        : LiteralNode, IComparable<NonNormalizedLiteralNode>
    {
        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        protected internal NonNormalizedLiteralNode(String literal)
            : base(literal, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="langspec">Lanaguage Specifier for the Literal</param>
        protected internal NonNormalizedLiteralNode(String literal, String langspec)
            : base(literal, langspec, false) { }

        /// <summary>
        /// Internal Only Constructor for Literal Nodes
        /// </summary>
        /// <param name="literal">String value of the Literal</param>
        /// <param name="datatype">Uri for the Literals Data Type</param>
        protected internal NonNormalizedLiteralNode(String literal, Uri datatype)
            : base(literal, datatype, false) { }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected NonNormalizedLiteralNode()
            : base() { }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected NonNormalizedLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

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
