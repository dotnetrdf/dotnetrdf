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
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Blank Nodes
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="bnode")]
#endif
    public abstract class BaseBlankNode
        : BaseNode, IBlankNode, IEquatable<BaseBlankNode>, IComparable<BaseBlankNode>, IValuedNode
    {
        private String _id;
        private bool _autoassigned = false;

        /// <summary>
        /// Internal Only Constructor for Blank Nodes
        /// </summary>
        /// <param name="g">Graph this Node belongs to</param>
        protected internal BaseBlankNode(IGraph g)
            : base(g, NodeType.Blank)
        {
            _id = _graph.GetNextBlankNodeID();
            _autoassigned = true;

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only constructor for Blank Nodes
        /// </summary>
        /// <param name="g">Graph this Node belongs to</param>
        /// <param name="nodeId">Custom Node ID to use</param>
        protected internal BaseBlankNode(IGraph g, String nodeId)
            : base(g, NodeType.Blank)
        {
            if (nodeId.Equals(String.Empty)) throw new RdfException("Cannot create a Blank Node with an empty ID");
            _id = nodeId;

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only constructor for Blank Nodes
        /// </summary>
        /// <param name="factory">Node Factory from which to obtain a Node ID</param>
        protected internal BaseBlankNode(INodeFactory factory)
            : base(null, NodeType.Blank)
        {
            _id = factory.GetNextBlankNodeID();
            _autoassigned = true;

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Unparameterized Constructor for deserialization usage only
        /// </summary>
        protected BaseBlankNode()
            : base(null, NodeType.Blank)
        { }

#if !NETCORE

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseBlankNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.Blank)
        {
            _id = info.GetString("id");

            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

#endif

        /// <summary>
        /// Returns the Internal Blank Node ID this Node has in the Graph
        /// </summary>
        /// <remarks>
        /// Usually automatically assigned and of the form autosXXX where XXX is some number.  If an RDF document contains a Blank Node ID of this form that clashes with an existing auto-assigned ID it will be automatically remapped by the Graph using the <see cref="BlankNodeMapper">BlankNodeMapper</see> when it is created.
        /// </remarks>
        public String InternalID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Indicates whether this Blank Node had its ID assigned for it by the Graph
        /// </summary>
        public bool HasAutoAssignedID
        {
            get
            {
                return _autoassigned;
            }
        }

        /// <summary>
        /// Implementation of Equals for Blank Nodes
        /// </summary>
        /// <param name="obj">Object to compare with the Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Blank Nodes are considered equal if their internal IDs match precisely and they originate from the same Graph
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
                // Can only be equal to things which are Nodes
                return false;
            }
        }

        /// <summary>
        /// Implementation of Equals for Blank Nodes
        /// </summary>
        /// <param name="other">Object to compare with the Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Blank Nodes are considered equal if their internal IDs match precisely and they originate from the same Graph
        /// </remarks>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Blank)
            {
                IBlankNode temp = (IBlankNode)other;

                return EqualityHelper.AreBlankNodesEqual(this, temp);
            }
            else
            {
                // Can only be equal to Blank Nodes
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to another
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        public override bool Equals(IBlankNode other)
        {
            return EqualityHelper.AreBlankNodesEqual(this, other);
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
        /// Determines whether this Node is equal to a URI Node (should always be false)
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public override bool Equals(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
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
        /// Determines whether this Node is equal to a Blank Node
        /// </summary>
        /// <param name="other">Blank Node</param>
        /// <returns></returns>
        public bool Equals(BaseBlankNode other)
        {
            return Equals((IBlankNode)other);
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(INode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                // Blank Nodes are considered greater than nulls
                // So we return a 1 to indicate we're greater than it
                return 1;
            }
            else if (other.NodeType == NodeType.Variable)
            {
                // Blank Nodes are considered greater than Variables
                return 1;
            }
            else if (other.NodeType == NodeType.Blank)
            {
                // Order Blank Nodes lexically by their ID
                return ComparisonHelper.CompareBlankNodes(this, (IBlankNode)other);
            }
            else
            {
                // Anything else is greater than a Blank Node
                // So we return a -1 to indicate we are less than the other Node
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
            if (other == null)
            {
                // We are always greater than nulls
                return 1;
            }
            else
            {
                // Order lexically on ID
                return ComparisonHelper.CompareBlankNodes(this, other);
            }
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
                // We are always greater than nulls
                return 1;
            }
            else
            {
                // We are less than Graph Literal Nodes
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
                // We are always greater than nulls
                return 1;
            }
            else
            {
                // We are less than Literal Nodes
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null)
            {
                // We are always greater than nulls
                return 1;
            }
            else
            {
                // We are less than URI Nodes
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
            // We are always greater than Nulls/Variable Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseBlankNode other)
        {
            return CompareTo((IBlankNode)other);
        }

        /// <summary>
        /// Returns a string representation of this Blank Node in QName form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "_:" + _id;
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
            info.AddValue("id", _id);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            _id = reader.ReadElementContentAsString();
            // Compute Hash Code
            _hashcode = (_nodetype + ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            writer.WriteString(_id);
        }

        #endregion

#endif

        #region IValuedNode Members

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a String
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to an integer
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a decimal
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a float
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a double
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a boolean
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be cast to a date time offset
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Throws an error as a Blank Node cannot be case to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new RdfQueryException("Unable to cast a Blank Node to a type");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public string EffectiveType => string.Empty;

        /// <summary>
        /// Gets the Numeric Type of the Node
        /// </summary>
        public SparqlNumericType NumericType => SparqlNumericType.NaN;

        #endregion
    }

    /// <summary>
    /// Class for representing Blank RDF Nodes
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName = "bnode")]
#endif
    public class BlankNode 
        : BaseBlankNode, IEquatable<BlankNode>, IComparable<BlankNode>
    {
        /// <summary>
        /// Internal Only Constructor for Blank Nodes
        /// </summary>
        /// <param name="g">Graph this Node belongs to</param>
        protected internal BlankNode(IGraph g)
            : base(g) { }

        /// <summary>
        /// Internal Only constructor for Blank Nodes
        /// </summary>
        /// <param name="g">Graph this Node belongs to</param>
        /// <param name="id">Custom Node ID to use</param>
        protected internal BlankNode(IGraph g, String id)
            : base(g, id) { }

        /// <summary>
        /// Internal Only constructor for Blank Nodes
        /// </summary>
        /// <param name="factory">Node Factory from which to obtain a Node ID</param>
        protected internal BlankNode(INodeFactory factory)
            : base(factory) { }

        /// <summary>
        /// Constructor for deserialization usage only
        /// </summary>
        protected BlankNode()
            : base()
        { }

#if !NETCORE
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BlankNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        /// <summary>
        /// Implementation of Compare To for Blank Nodes
        /// </summary>
        /// <param name="other">Blank Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(BlankNode other)
        {
            return CompareTo((IBlankNode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Blank Node
        /// </summary>
        /// <param name="other">Blank Node</param>
        /// <returns></returns>
        public bool Equals(BlankNode other)
        {
            return base.Equals((IBlankNode)other);
        }
    }
}
