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
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// Abstract Base implementation of a Virtual Node which is a Node that is represented only by some ID until such time as its value actually needs materialising
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    /// <remarks>
    /// <para>
    /// As far as possible equality checks are carried out using these IDs and limited comparisons may also be done this way.  More specific implementations may wish to derive from this class in order to override the default comparison implementation to further reduce the number of places where value materialisation is done.
    /// </para>
    /// <para>
    /// Note that this class does not implement any of the specialised Node interfaces and instead relies on the casting of its materialised value to an appropriately typed node to provide the true values to code that needs it
    /// </para>
    /// </remarks>
    public abstract class BaseVirtualNode<TNodeID, TGraphID> 
        : IVirtualNode<TNodeID, TGraphID>, IEquatable<BaseVirtualNode<TNodeID, TGraphID>>, IComparable<BaseVirtualNode<TNodeID, TGraphID>>
    {
        private IGraph _g;
        private Uri _graphUri;
        private TNodeID _id;
        private IVirtualRdfProvider<TNodeID, TGraphID> _provider;
        private NodeType _type;
        /// <summary>
        /// The materialised value of the Virtual Node
        /// </summary>
        protected INode _value;
        private bool _collides = false;

        /// <summary>
        /// Creates a new Base Virtual Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="type">Type of the node</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualNode(IGraph g, NodeType type, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._g = g;
            if (this._g != null) this._graphUri = this._g.BaseUri;
            this._type = type;
            this._id = id;
            this._provider = provider;
        }

        /// <summary>
        /// Creates a new Base Virtual Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="type">Type of the node</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualNode(IGraph g, NodeType type, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, INode value)
            : this(g, type, id, provider)
        {
            this._value = value;
            if (this._value.NodeType != this._type) throw new RdfException("Cannot create a pre-materialised Virtual Node where the materialised value is a Node of the wrong type! Expected " + this._type.ToString() + " but got " + this._value.NodeType.ToString());
            this.OnMaterialise();
        }

        /// <summary>
        /// Materialises the Value if it is not already materialised
        /// </summary>
        protected void MaterialiseValue()
        {
            if (this._value == null)
            {
                //Materialise the value
                this._value = this._provider.GetValue(this._g, this._id);
                if (this._value.NodeType != this._type) throw new RdfException("The Virtual RDF Provider materialised a Node of the wrong type! Expected " + this._type.ToString() + " but got " + this._value.NodeType.ToString());
                this.OnMaterialise();
            }
        }

        /// <summary>
        /// Called after the value is materialised for the first time
        /// </summary>
        protected virtual void OnMaterialise()
        {

        }

        #region IVirtualNode<TNodeID,TGraphID> Members

        /// <summary>
        /// Gets the Virtual ID of the Node
        /// </summary>
        public TNodeID VirtualID
        {
            get
            {
                return this._id;
            }
        }

        /// <summary>
        /// Gets the Virtual RDF Provider of the Node
        /// </summary>
        public IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get
            {
                return this._provider;
            }
        }

        /// <summary>
        /// Gets whether the Nodes value has been materialised
        /// </summary>
        public bool IsMaterialised
        {
            get
            {
                return this._value != null;
            }
        }

        /// <summary>
        /// Gets the materialised value of the Node forcing it to be materialised if it hasn't already
        /// </summary>
        public INode MaterialisedValue
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._value;
            }
        }

        #endregion

        #region INode Members

        /// <summary>
        /// Gets the Type of the Node
        /// </summary>
        public NodeType NodeType
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets the Graph the Node belongs to
        /// </summary>
        public IGraph Graph
        {
            get 
            {
                return this._g; 
            }
        }

        /// <summary>
        /// Gets/Sets the Graph URI of the Node
        /// </summary>
        public Uri GraphUri
        {
            get
            {
                return this._graphUri;
            }
            set
            {
                this._graphUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Nodes Hash Code collides with other Nodes in the Graph
        /// </summary>
        /// <remarks>
        /// Designed for internal use only, exposed via the Interface in order to simplify implementation.  For Triples the equivalent method is protected internal since we pass a concrete class as the parameter and can do this but without switching the entire API to use <see cref="BaseNode">BaseNode</see> as the type for Nodes the same is not possible and this is not a change we wish to make to the API as it limits extensibility
        /// </remarks>
        public bool Collides
        {
            get
            {
                return this._collides;
            }
            set
            {
                this._collides = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns></returns>
        public string ToString(INodeFormatter formatter)
        {
            return formatter.Format(this);
        }

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        public string ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return formatter.Format(this, segment);
        }

        #endregion

        #region IComparable Implementations

        /// <summary>
        /// Compares this Node to another Virtual Node
        /// </summary>
        /// <param name="other">Other Virtual Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(IVirtualNode<TNodeID, TGraphID> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;

            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual)
            {
                return 0;
            }
            else
            {
                return this.CompareTo((INode)other);
            }
        }

        /// <summary>
        /// Compares this Node to another Virtual Node
        /// </summary>
        /// <param name="other">Other Virtual Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IVirtualNode<TNodeID, TGraphID>)other);
        }

        /// <summary>
        /// Compares this Node to another Node
        /// </summary>
        /// <param name="other">Other Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(INode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            switch (this._type)
            {
                case NodeType.Blank:
                    if (other.NodeType == NodeType.Variable)
                    {
                        //Blank Nodes are greater than variables
                        return 1;
                    }
                    else if (other.NodeType == NodeType.Blank)
                    {
                        //Compare Blank Node appropriately
                        return ComparisonHelper.CompareBlankNodes((IBlankNode)this, (IBlankNode)other);
                    }
                    else
                    {
                        //Blank Nodes are less than everything else
                        return -1;
                    }

                case NodeType.GraphLiteral:
                    if (other.NodeType == NodeType.GraphLiteral)
                    {
                        //Compare Graph Literals appropriately
                        return ComparisonHelper.CompareGraphLiterals((IGraphLiteralNode)this, (IGraphLiteralNode)other);
                    }
                    else
                    {
                        //Graph Literals are greater than everything else
                        return 1;
                    }

                case NodeType.Literal:
                    if (other.NodeType == NodeType.GraphLiteral)
                    {
                        //Literals are less than Graph Literals
                        return -1;
                    }
                    else if (other.NodeType == NodeType.Literal)
                    {
                        //Compare Literals appropriately
                        return ComparisonHelper.CompareLiterals((ILiteralNode)this, (ILiteralNode)other);
                    }
                    else
                    {
                        //Literals are greater than anything else (i.e. Blanks, Variables and URIs)
                        return 1;
                    }

                case NodeType.Uri:
                    if (other.NodeType == NodeType.GraphLiteral || other.NodeType == NodeType.Literal)
                    {
                        //URIs are less than Literals and Graph Literals
                        return -1;
                    }
                    else if (other.NodeType == NodeType.Uri)
                    {
                        //Compare URIs appropriately
                        return ComparisonHelper.CompareUris((IUriNode)this, (IUriNode)other);
                    }
                    else
                    {
                        //URIs are greater than anything else (i.e. Blanks and Variables)
                        return 1;
                    }

                case NodeType.Variable:
                    if (other.NodeType == NodeType.Variable)
                    {
                        //Compare Variables accordingly
                        return ComparisonHelper.CompareVariables((IVariableNode)this, (IVariableNode)other);
                    }
                    else
                    {
                        //Variables are less than anything else
                        return -1;
                    }

                default:
                    //Things are always greater than unknown node types
                    return 1;
            }
        }

        /// <summary>
        /// Compares this Node to another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public virtual int CompareTo(IBlankNode other)
        {
            return this.CompareTo((INode)other);
        }

        /// <summary>
        /// Compares this Node to another Graph LiteralNode
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public virtual int CompareTo(IGraphLiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

        /// <summary>
        /// Compares this Node to another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public virtual int CompareTo(ILiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

        /// <summary>
        /// Compares this Node to another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public virtual int CompareTo(IUriNode other)
        {
            return this.CompareTo((INode)other);
        }

        /// <summary>
        /// Compares this Node to another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public virtual int CompareTo(IVariableNode other)
        {
            return this.CompareTo((INode)other);
        }

        #endregion

        #region IEquatable Implementations

        /// <summary>
        /// Checks this Node for equality against another Object
        /// </summary>
        /// <param name="obj">Other Object</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null) return false;

            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks this Node for equality against another Virtual Node
        /// </summary>
        /// <param name="other">Other Virtual Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(IVirtualNode<TNodeID, TGraphID> other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ReferenceEquals(this._provider, other.Provider) && this._id.Equals(other.VirtualID);
        }

        /// <summary>
        /// Checks this Node for equality against another Virtual Node
        /// </summary>
        /// <param name="other">Other Virtual Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IVirtualNode<TNodeID, TGraphID>)other);
        }

        /// <summary>
        /// Checks this Node for equality against another Node
        /// </summary>
        /// <param name="other">Other Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(INode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            bool areEqual;
            if (this._type != other.NodeType)
            {
                //Non-equal node types cannot be equal
                return false;
            }
            else if (this.TryVirtualEquality(other, out areEqual))
            {
                //If Virtual Nodes originate from same virtual RDF provider can compare based on their virtual Node IDs
                return areEqual;
            }
            else
            {
                //If not both virtual and are of the same type the only way to determine equality is to
                //materialise the value of this node and then check that against the other node
                if (this._value == null) this.MaterialiseValue();
                return this._value.Equals(other);
            }
        }

        /// <summary>
        /// Checks the Node Types and if they are equal invokes the INode based comparison
        /// </summary>
        /// <param name="other">Node to compare with for equality</param>
        /// <returns></returns>
        private bool TypedEquality(INode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (this._type == other.NodeType)
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to check for equality using virtual node IDs
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <param name="areEqual">Whether the virtual nodes are equal</param>
        /// <returns>
        /// Whether the virtual equality test was valid, if false then other means must be used to determine equality
        /// </returns>
        protected bool TryVirtualEquality(INode other, out bool areEqual)
        {
            areEqual = false;
            if (other is IVirtualNode<TNodeID, TGraphID>)
            {
                IVirtualNode<TNodeID, TGraphID> virt = (IVirtualNode<TNodeID, TGraphID>)other;
                if (ReferenceEquals(this._provider, virt.Provider))
                {
                    areEqual = this._id.Equals(virt.VirtualID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks this Node for equality against another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public virtual bool Equals(IBlankNode other)
        {
            return this.TypedEquality(other);
        }

        /// <summary>
        /// Checks this Node for equality against another Graph Literal Node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public virtual bool Equals(IGraphLiteralNode other)
        {
            return this.TypedEquality(other);
        }

        /// <summary>
        /// Checks this Node for equality against another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public virtual bool Equals(ILiteralNode other)
        {
            return this.TypedEquality(other);
        }

        /// <summary>
        /// Checks this Node for equality against another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public virtual bool Equals(IUriNode other)
        {
            return this.TypedEquality(other);
        }

        /// <summary>
        /// Checks this Node for equality against another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public virtual bool Equals(IVariableNode other)
        {
            return this.TypedEquality(other);
        }

        #endregion

        /// <summary>
        /// Copies the Virtual Node into another Graph
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        public abstract INode CopyNode(IGraph target);

        /// <summary>
        /// Gets the Hash Code of the Virtual Node
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
            return this._id.GetHashCode();
        }

        /// <summary>
        /// Gets the String representation of the Node
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            if (this._value == null) this.MaterialiseValue();
            return this._value.ToString();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        /// <exception cref="NotImplementedException">Thrown because serializing a Virtual Node would be lossy</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException("This INode implementation does not support Serialization");
        }

        /// <summary>
        /// Gets the schema for XML serialization
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        /// <exception cref="NotImplementedException">Thrown because serializing a Virtual Node would be lossy</exception>
        public virtual void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }

        /// <summary>
        /// Writes the data for XML deserialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        /// <exception cref="NotImplementedException">Thrown because serializing a Virtual Node would be lossy</exception>
        public virtual void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }
#endif
    }

    /// <summary>
    /// Abstract Base implementation of a Virtual Blank Node
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class BaseVirtualBlankNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IBlankNode, 
          IEquatable<BaseVirtualBlankNode<TNodeID, TGraphID>>, IComparable<BaseVirtualBlankNode<TNodeID, TGraphID>>
    {
        private String _internalID;

        /// <summary>
        /// Creates a new Virtual Blank Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Blank, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Blank Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IBlankNode value)
            : base(g, NodeType.Blank, id, provider, value) { }

        /// <summary>
        /// Takes post materialisation actions
        /// </summary>
        protected sealed override void OnMaterialise()
        {
            IBlankNode temp = (IBlankNode)this._value;
            this._internalID = temp.InternalID;
        }

        /// <summary>
        /// Gets the Internal ID of the Blank Node
        /// </summary>
        public string InternalID
        {
            get 
            {
                if (this._value == null) this.MaterialiseValue();
                return this._internalID;
            }
        }

        /// <summary>
        /// Compares this Node to another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public override int CompareTo(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareBlankNodes(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public override bool Equals(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreBlankNodesEqual(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualBlankNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IBlankNode)other);
        }

        /// <summary>
        /// Compares this Node to another Blank Node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualBlankNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IBlankNode)other);
        }
    }

    /// <summary>
    /// Abstract Base implementation of a Virtual Graph Literal Node
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class BaseVirtualGraphLiteralNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IGraphLiteralNode,
          IEquatable<BaseVirtualGraphLiteralNode<TNodeID, TGraphID>>, IComparable<BaseVirtualGraphLiteralNode<TNodeID, TGraphID>>
    {
        private IGraph _subgraph;

        /// <summary>
        /// Creates a new Virtual Graph Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualGraphLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.GraphLiteral, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Graph Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualGraphLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IGraphLiteralNode value)
            : base(g, NodeType.GraphLiteral, id, provider, value) { }

        /// <summary>
        /// Takes post materialisation actions
        /// </summary>
        protected sealed override void OnMaterialise()
        {
            IGraphLiteralNode temp = (IGraphLiteralNode)this._value;
            this._subgraph = temp.SubGraph;
        }

        /// <summary>
        /// Gets the subgraph this Graph Literal represents
        /// </summary>
        public IGraph SubGraph
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._subgraph;
            }
        }

        /// <summary>
        /// Compares this Node to another Graph Literal Node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public override int CompareTo(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareGraphLiterals(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Graph Literal Node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public override bool Equals(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreGraphLiteralsEqual(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Graph Literal Node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualGraphLiteralNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IGraphLiteralNode)other);
        }

        /// <summary>
        /// Compares this Node to another Graph Literal Node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualGraphLiteralNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IGraphLiteralNode)other);
        }
    }

    /// <summary>
    /// Abstract Base implementation of a Virtual Literal Node
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class BaseVirtualLiteralNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, ILiteralNode,
          IEquatable<BaseVirtualLiteralNode<TNodeID, TGraphID>>, IComparable<BaseVirtualLiteralNode<TNodeID, TGraphID>>
    {
        private String _litValue, _lang;
        private Uri _datatype;

        /// <summary>
        /// Creates a new Virtual Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Literal, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, ILiteralNode value)
            : base(g, NodeType.Literal, id, provider, value) { }

        /// <summary>
        /// Takes post materialisation actions
        /// </summary>
        protected sealed override void  OnMaterialise()
        {
            ILiteralNode temp = (ILiteralNode)this._value;
            this._litValue = temp.Value;
            this._lang = temp.Language;
            this._datatype = temp.DataType;
        }

        /// <summary>
        /// Gets the lexical value of the Literal
        /// </summary>
        public String Value
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._litValue;
            }
        }

        /// <summary>
        /// Gets the language specifier (if any) of the Literal
        /// </summary>
        public String Language
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._lang;
            }
        }

        /// <summary>
        /// Gets the Datatype (if any) of the Literal
        /// </summary>
        public Uri DataType
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._datatype;
            }
        }

        /// <summary>
        /// Compares this Node to another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareLiterals(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public override bool Equals(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreLiteralsEqual(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
        {
            return this.Equals((ILiteralNode)other);
        }

        /// <summary>
        /// Compares this Node to another Literal Node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((ILiteralNode)other);
        }
        
    }

    /// <summary>
    /// Abstract Base implementation of a Virtual URI Node
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class BaseVirtualUriNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IUriNode,
          IEquatable<BaseVirtualUriNode<TNodeID, TGraphID>>, IComparable<BaseVirtualUriNode<TNodeID, TGraphID>>
    {
        private Uri _u;

        /// <summary>
        /// Creates a new Virtual URI Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Uri, id, provider) { }

        /// <summary>
        /// Creates a new Virtual URI Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IUriNode value)
            : base(g, NodeType.Uri, id, provider, value) { }

        /// <summary>
        /// Takes post materialisation actions
        /// </summary>
        protected sealed override void OnMaterialise()
        {
            IUriNode temp = (IUriNode)this._value;
            this._u = temp.Uri;
        }

        /// <summary>
        /// Gets the URI
        /// </summary>
        public Uri Uri
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._u;
            }
        }

        /// <summary>
        /// Compares this Node to another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareUris(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public override bool Equals(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreUrisEqual(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualUriNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IUriNode)other);
        }

        /// <summary>
        /// Compares this Node to another URI Node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualUriNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IUriNode)other);
        }
    }

    /// <summary>
    /// Abstract Base implementation of a Virtual Variable Node
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class BaseVirtualVariableNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IVariableNode,
          IEquatable<BaseVirtualVariableNode<TNodeID, TGraphID>>, IComparable<BaseVirtualVariableNode<TNodeID, TGraphID>>
    {
        private String _var;

        /// <summary>
        /// Creates a new Virtual Variable Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Variable, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Variable Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IVariableNode value)
            : base(g, NodeType.Variable, id, provider, value) { }

        /// <summary>
        /// Takes post materialisation actions
        /// </summary>
        protected sealed override void OnMaterialise()
        {
            IVariableNode temp = (IVariableNode)this._value;
            this._var = temp.VariableName;
        }

        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        public String VariableName
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._var;
            }
        }

        /// <summary>
        /// Compares this Node to another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public override int CompareTo(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareVariables(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public override bool Equals(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreVariablesEqual(this, other);
        }

        /// <summary>
        /// Checks this Node for equality against another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
        /// </remarks>
        public bool Equals(BaseVirtualVariableNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IVariableNode)other);
        }

        /// <summary>
        /// Compares this Node to another Variable Node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
        /// </remarks>
        public int CompareTo(BaseVirtualVariableNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IVariableNode)other);
        }
    }
}
