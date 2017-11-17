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
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for URI Nodes
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="uri")]
#endif
    public abstract class BaseUriNode 
        : BaseNode, IUriNode, IEquatable<BaseUriNode>, IComparable<BaseUriNode>, IValuedNode
    {
        private Uri _uri;

        /// <summary>
        /// Internal Only Constructor for URI Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="uri">URI</param>
        protected internal BaseUriNode(IGraph g, Uri uri)
            : base(g, NodeType.Uri)
        {
            _uri = uri;

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only Constructor for URI Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="qname">QName for the Node</param>
        /// <remarks>
        /// This Constructor tries to resolve the QName using the NamespaceMapper and Base Uri of the Graph it is in.  Exceptions may occur if we cannot resolve the QName correctly.
        /// </remarks>
        protected internal BaseUriNode(IGraph g, String qname)
            : base(g, NodeType.Uri)
        {
            if (qname.Contains(':'))
            {
                if (_graph != null)
                {
                    _uri = UriFactory.Create(Tools.ResolveQName(qname, _graph.NamespaceMap, _graph.BaseUri));
                }
                else
                {
                    throw new RdfException("Cannot create a URI Node using a QName in a Null Graph");
                }
            }
            else
            {
                throw new RdfException("Cannot create a URI Node since the QName '" + qname + "' appears to be invalid");
            }

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only Constructor for URI Nodes
        /// </summary>
        /// <param name="uri">URI</param>
        protected internal BaseUriNode(Uri uri)
            : this(null, uri) { }

        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected BaseUriNode()
            : base(null, NodeType.Uri) { }

#if !NETCORE

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseUriNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.Uri)
        {
            _uri = UriFactory.Create(info.GetString("uri"));

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

#endif

        /// <summary>
        /// Gets the Uri for this Node
        /// </summary>
        public virtual Uri Uri
        {
            get
            {
                return _uri;
            }
        }

        /// <summary>
        /// Implementation of Equality for Uri Nodes
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns></returns>
        /// <remarks>
        /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is INode)
            {
                return Equals((INode)obj);
            }
            else
            {
                // Can only be equal to other Nodes
                return false;
            }
        }

        /// <summary>
        /// Implementation of Equality for Uri Nodes
        /// </summary>
        /// <param name="other">Object to compare with</param>
        /// <returns></returns>
        /// <remarks>
        /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison
        /// </remarks>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Uri)
            {
                Uri temp = ((IUriNode)other).Uri;

                return EqualityHelper.AreUrisEqual(_uri, temp);
            }
            else
            {
                // Can only be equal to UriNodes
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to a Blank Node (should always be false)
        /// </summary>
        /// <param name="other">Blank Node</param>
        /// <returns></returns>
        public override bool Equals(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node (should always be false)
        /// </summary>
        /// <param name="other">Graph Literal Node</param>
        /// <returns></returns>
        public override bool Equals(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node (should always be false)
        /// </summary>
        /// <param name="other">Literal Node</param>
        /// <returns></returns>
        public override bool Equals(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public override bool Equals(IUriNode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            return EqualityHelper.AreUrisEqual(_uri, other.Uri);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node (should always be false)
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public override bool Equals(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public bool Equals(BaseUriNode other)
        {
            return Equals((IUriNode)other);
        }

        /// <summary>
        /// Gets a String representation of a Uri as a plain text Uri
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _uri.AbsoluteUri;
        }

        /// <summary>
        /// Implementation of Compare To for Uri Nodes
        /// </summary>
        /// <param name="other">Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Uri Nodes are greater than Blank Nodes and Nulls, they are less than Literal Nodes and Graph Literal Nodes.
        /// <br /><br />
        /// Uri Nodes are ordered based upon lexical ordering of the string value of their URIs
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
            else if (other.NodeType == NodeType.Blank || other.NodeType == NodeType.Variable)
            {
                // Uri Nodes are greater than Blank and Variable Nodes
                // Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Uri)
            {
                // Return the result of CompareTo using the IUriNode comparison method

                return CompareTo((IUriNode)other);
            }
            else
            {
                // Anything else is considered greater than a Uri Node
                // Return -1 to indicate this
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            // URI Nodes are greater than nulls and Blank Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                // Everything is greater than a null
                // Return a 1 to indicate this
                return 1;
            }
            else
            {
                // URI Nodes are less than Graph Literal Nodes
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                // Everything is greater than a null
                // Return a 1 to indicate this
                return 1;
            }
            else
            {
                // URI Nodes are less than Literal Nodes
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            // URI Nodes are greater than nulls and Variable Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            return ComparisonHelper.CompareUris(Uri, other.Uri);
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseUriNode other)
        {
            return CompareTo((IUriNode)other);
        }

#if !NETCORE

        #region ISerializable Members

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("uri", _uri.AbsoluteUri);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            _uri = UriFactory.Create(reader.ReadElementContentAsString());
            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            writer.WriteString(_uri.AbsoluteUri);
        }

        #endregion

#endif

        #region IValuedNode Members

        /// <summary>
        /// Gets the value of the node as a string
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return _uri.AbsoluteUri;
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to numerics
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to numerics
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to numerics
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to numerics
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to a boolean
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Cannot cast a URI to a type");
        }

        /// <summary>
        /// Throws an error as URIs cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new RdfQueryException("Cannot case a URI to a type");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return String.Empty;
            }
        }
        
        /// <summary>
        /// Gets the numeric type of the expression
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for representing URI Nodes
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="uri")]
#endif
    public class UriNode
        : BaseUriNode, IEquatable<UriNode>, IComparable<UriNode>
    {
        /// <summary>
        /// Internal Only Constructor for URI Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="uri">URI for the Node</param>
        protected internal UriNode(IGraph g, Uri uri)
            : base(g, uri) { }

        /// <summary>
        /// Internal Only Constructor for URI Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="qname">QName for the Node</param>
        /// <remarks>
        /// This Constructor tries to resolve the QName using the NamespaceMapper and Base Uri of the Graph it is in.  Exceptions may occur if we cannot resolve the QName correctly.
        /// </remarks>
        protected internal UriNode(IGraph g, String qname)
            : base(g, qname) { }

        /// <summary>
        /// Deserilization Only Constructor
        /// </summary>
        protected UriNode()
            : base() { }

#if !NETCORE

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected UriNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

#endif

        /// <summary>
        /// Implementation of Compare To for URI Nodes
        /// </summary>
        /// <param name="other">URI Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(UriNode other)
        {
            return base.CompareTo((IUriNode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public bool Equals(UriNode other)
        {
            return base.Equals((IUriNode)other);
        }
    }
}
