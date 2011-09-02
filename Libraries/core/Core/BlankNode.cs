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
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Blank Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="bnode")]
#endif
    public abstract class BaseBlankNode
        : BaseNode, IBlankNode, IEquatable<BaseBlankNode>, IComparable<BaseBlankNode>
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
            this._id = this._graph.GetNextBlankNodeID();
            this._autoassigned = true;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
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
            this._id = nodeId;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only constructor for Blank Nodes
        /// </summary>
        /// <param name="factory">Node Factory from which to obtain a Node ID</param>
        protected internal BaseBlankNode(INodeFactory factory)
            : base(null, NodeType.Blank)
        {
            this._id = factory.GetNextBlankNodeID();
            this._autoassigned = true;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Unparameterized Constructor for deserialization usage only
        /// </summary>
        protected BaseBlankNode()
            : base(null, NodeType.Blank)
        { }

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseBlankNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.Blank)
        {
            this._id = info.GetString("id");

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
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
                return this._id;
            }
        }

        /// <summary>
        /// Indicates whether this Blank Node had its ID assigned for it by the Graph
        /// </summary>
        public bool HasAutoAssignedID
        {
            get
            {
                return this._autoassigned;
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
                return this.Equals((INode)obj);
            }
            else
            {
                //Can only be equal to things which are Nodes
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
                //Can only be equal to Blank Nodes
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
            return this.Equals((IBlankNode)other);
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
                //Blank Nodes are considered greater than nulls
                //So we return a 1 to indicate we're greater than it
                return 1;
            }
            else if (other.NodeType == NodeType.Variable)
            {
                //Blank Nodes are considered greater than Variables
                return 1;
            }
            else if (other.NodeType == NodeType.Blank)
            {
                //Order Blank Nodes lexically by their ID
                return ComparisonHelper.CompareBlankNodes(this, (IBlankNode)other);
            }
            else
            {
                //Anything else is greater than a Blank Node
                //So we return a -1 to indicate we are less than the other Node
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
                //We are always greater than nulls
                return 1;
            }
            else
            {
                //Order lexically on ID
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
                //We are always greater than nulls
                return 1;
            }
            else
            {
                //We are less than Graph Literal Nodes
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
                //We are always greater than nulls
                return 1;
            }
            else
            {
                //We are less than Literal Nodes
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
                //We are always greater than nulls
                return 1;
            }
            else
            {
                //We are less than URI Nodes
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
            //We are always greater than Nulls/Variable Nodes
            return 1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseBlankNode other)
        {
            return this.CompareTo((IBlankNode)other);
        }

        /// <summary>
        /// Returns a string representation of this Blank Node in QName form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "_:" + this._id;
        }

#if !SILVERLIGHT

        #region ISerializable Members

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", this._id);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            this._id = reader.ReadElementContentAsString();
            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            writer.WriteString(this._id);
        }

        #endregion

#endif
    }

    /// <summary>
    /// Class for representing Blank RDF Nodes
    /// </summary>
#if !SILVERLIGHT
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

#if !SILVERLIGHT
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
            return this.CompareTo((IBlankNode)other);
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
