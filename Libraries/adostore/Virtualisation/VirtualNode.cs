using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// A Virtual Node is a Node that is represented only by some ID until such time as it actually needs materialising
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
    public class VirtualNode<TNodeID, TGraphID> : IVirtualNode<TNodeID, TGraphID>
    {
        private IGraph _g;
        private Uri _graphUri;
        private TNodeID _id;
        private IVirtualRdfProvider<TNodeID, TGraphID> _provider;
        private NodeType? _type;
        private INode _value;
        private bool _collides = false;

        public VirtualNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._g = g;
            this._graphUri = this._g.BaseUri;
            this._id = id;
            this._provider = provider;
        }

        public VirtualNode(IGraph g, INode value, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._g = g;
            this._graphUri = this._g.BaseUri;
            this._value = value;
            this._type = this._value.NodeType;
            this._provider = provider;
            if (this._value.NodeType != NodeType.Blank)
            {
                this._id = this._provider.GetID(this._value, true);
            }
            else
            {
                throw new RdfException("Cannot create a Virtual RDF Node with a predefined value when that value is a Blank Node using this constructor overload");
            }
        }

        public VirtualNode(IGraph g, IBlankNode value, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._g = g;
            this._graphUri = this._g.BaseUri;
            this._value = value;
            this._type = this._value.NodeType;
            this._provider = provider;
            this._id = this._provider.GetBlankNodeID(value, true);
        }

        private void EnsureValue()
        {
            if (this._value == null)
            {
                //Materialise the value
                this._value = this._provider.GetValue(this._g, this._id);
                this._type = this._value.NodeType;
            }
        }

        #region IVirtualNode<TNodeID,TGraphID> Members

        public TNodeID VirtualID
        {
            get
            {
                return this._id;
            }
        }

        public IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get
            {
                return this._provider;
            }
        }

        public bool IsMaterialised
        {
            get
            {
                return this._value != null;
            }
        }

        public INode MaterialisedValue
        {
            get
            {
                if (this._value == null) this.EnsureValue();
                return this._value;
            }
        }

        #endregion

        #region INode Members

        public NodeType NodeType
        {
            get
            {
                if (this._type == null)
                {
                    this.EnsureValue();
                }
                return this._type.Value;
            }
        }

        public IGraph Graph
        {
            get 
            {
                return this._g; 
            }
        }

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

        public string ToString(INodeFormatter formatter)
        {
            return formatter.Format(this);
        }

        public string ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return formatter.Format(this, segment);
        }

        #endregion

        #region IComparable<INode> Members

        public int CompareTo(INode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IBlankNode> Members

        public int CompareTo(IBlankNode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IGraphLiteralNode> Members

        public int CompareTo(IGraphLiteralNode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<ILiteralNode> Members

        public int CompareTo(ILiteralNode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IUriNode> Members

        public int CompareTo(IUriNode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IVariableNode> Members

        public int CompareTo(IVariableNode other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(INode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (other is IVirtualNode<TNodeID, TGraphID>)
            {
                IVirtualNode<TNodeID, TGraphID> virt = (IVirtualNode<TNodeID, TGraphID>)other;
                return ReferenceEquals(this._provider, virt.Provider) && this._id.Equals(virt.VirtualID);
            }
            else
            {
                //If not both virtual the only way to determine equality is to
                //materialise the value of this node and then check that against the other node
                if (this._value == null) this.EnsureValue();
                return this._value.Equals(other);
            }
        }

        private bool TypedEquality(INode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            if (this._type == null) this.EnsureValue();
            if (this._type == other.NodeType)
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(IBlankNode other)
        {
            return this.TypedEquality(other);
        }

        public bool Equals(IGraphLiteralNode other)
        {
            return this.TypedEquality(other);
        }

        public bool Equals(ILiteralNode other)
        {
            return this.TypedEquality(other);
        }

        public bool Equals(IUriNode other)
        {
            return this.TypedEquality(other);
        }

        public bool Equals(IVariableNode other)
        {
            return this.TypedEquality(other);
        }

        #endregion

        //#region Cast Implemenations

        //public static explicit operator IBlankNode(VirtualNode<TNodeID, TGraphID> node)
        //{
        //    if (node.NodeType == NodeType.Blank)
        //    {
        //        return (IBlankNode)node.MaterialisedValue;
        //    } 
        //    else 
        //    {
        //        throw new InvalidCastException("Cannot cast a non-Blank Node to a Blank Node");
        //    }
        //}

        //public static explicit operator IGraphLiteralNode(VirtualNode<TNodeID, TGraphID> node)
        //{
        //    if (node.NodeType == NodeType.GraphLiteral)
        //    {
        //        return (IGraphLiteralNode)node.MaterialisedValue;
        //    }
        //    else
        //    {
        //        throw new InvalidCastException("Cannot cast a non-Graph Literal Node to a Graph Literal Node");
        //    }
        //}

        //public static explicit operator ILiteralNode(VirtualNode<TNodeID, TGraphID> node)
        //{
        //    if (node.NodeType == NodeType.Literal)
        //    {
        //        return (ILiteralNode)node.MaterialisedValue;
        //    }
        //    else
        //    {
        //        throw new InvalidCastException("Cannot cast a non-Literal Node to a Literal Node");
        //    }
        //}

        //public static explicit operator IUriNode(VirtualNode<TNodeID, TGraphID> node)
        //{
        //    if (node.NodeType == NodeType.Uri)
        //    {
        //        return (IUriNode)node.MaterialisedValue;
        //    }
        //    else
        //    {
        //        throw new InvalidCastException("Cannot cast a non-URI Node to a URI Node");
        //    }
        //}

        //public static explicit operator IVariableNode(VirtualNode<TNodeID, TGraphID> node)
        //{
        //    if (node.NodeType == NodeType.Variable)
        //    {
        //        return (IVariableNode)node.MaterialisedValue;
        //    }
        //    else
        //    {
        //        throw new InvalidCastException("Cannot cast a non-Variable Node to a Variable Node");
        //    }
        //}

        //#endregion
    }
}
