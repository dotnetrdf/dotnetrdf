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
    public abstract class BaseVirtualNode<TNodeID, TGraphID> 
        : IVirtualNode<TNodeID, TGraphID>, IEquatable<BaseVirtualNode<TNodeID, TGraphID>>, IComparable<BaseVirtualNode<TNodeID, TGraphID>>
    {
        private IGraph _g;
        private Uri _graphUri;
        private TNodeID _id;
        private IVirtualRdfProvider<TNodeID, TGraphID> _provider;
        private NodeType _type;
        protected INode _value;
        private bool _collides = false;

        public BaseVirtualNode(IGraph g, NodeType type, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._g = g;
            if (this._g != null) this._graphUri = this._g.BaseUri;
            this._type = type;
            this._id = id;
            this._provider = provider;
        }

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
                if (this._value == null) this.MaterialiseValue();
                return this._value;
            }
        }

        #endregion

        #region INode Members

        public NodeType NodeType
        {
            get
            {
                return this._type;
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

        #region IComparable Implementations

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

        public int CompareTo(BaseVirtualNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IVirtualNode<TNodeID, TGraphID>)other);
        }

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
                    break;

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
                    break;

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
                    break;

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
                    break;

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
                    break;

                default:
                    //Things are always greater than unknown node types
                    return 1;
            }
        }

        public virtual int CompareTo(IBlankNode other)
        {
            return this.CompareTo((INode)other);
        }

        public virtual int CompareTo(IGraphLiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

        public virtual int CompareTo(ILiteralNode other)
        {
            return this.CompareTo((INode)other);
        }

        public virtual int CompareTo(IUriNode other)
        {
            return this.CompareTo((INode)other);
        }

        public virtual int CompareTo(IVariableNode other)
        {
            return this.CompareTo((INode)other);
        }

        #endregion

        #region IEquatable Implementations

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

        public bool Equals(IVirtualNode<TNodeID, TGraphID> other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ReferenceEquals(this._provider, other.Provider) && this._id.Equals(other.VirtualID);
        }

        public bool Equals(BaseVirtualNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IVirtualNode<TNodeID, TGraphID>)other);
        }

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

        public virtual bool Equals(IBlankNode other)
        {
            return this.TypedEquality(other);
        }

        public virtual bool Equals(IGraphLiteralNode other)
        {
            return this.TypedEquality(other);
        }

        public virtual bool Equals(ILiteralNode other)
        {
            return this.TypedEquality(other);
        }

        public virtual bool Equals(IUriNode other)
        {
            return this.TypedEquality(other);
        }

        public virtual bool Equals(IVariableNode other)
        {
            return this.TypedEquality(other);
        }

        #endregion

        public abstract INode CopyNode(IGraph target);

        public sealed override int GetHashCode()
        {
            return this._id.GetHashCode();
        }

        public sealed override string ToString()
        {
            if (this._value == null) this.MaterialiseValue();
            return this._value.ToString();
        }
    }

    public abstract class BaseVirtualBlankNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IBlankNode, 
          IEquatable<BaseVirtualBlankNode<TNodeID, TGraphID>>, IComparable<BaseVirtualBlankNode<TNodeID, TGraphID>>
    {
        private String _internalID;

        public BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Blank, id, provider) { }

        public BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IBlankNode value)
            : base(g, NodeType.Blank, id, provider, value) { }

        protected sealed override void OnMaterialise()
        {
            IBlankNode temp = (IBlankNode)this._value;
            this._internalID = temp.InternalID;
        }

        public string InternalID
        {
            get 
            {
                if (this._value == null) this.MaterialiseValue();
                return this._internalID;
            }
        }

        public override int CompareTo(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareBlankNodes(this, other);
        }

        public override bool Equals(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreBlankNodesEqual(this, other);
        }

        public bool Equals(BaseVirtualBlankNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IBlankNode)other);
        }

        public int CompareTo(BaseVirtualBlankNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IBlankNode)other);
        }
    }

    public abstract class BaseVirtualGraphLiteralNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IGraphLiteralNode,
          IEquatable<BaseVirtualGraphLiteralNode<TNodeID, TGraphID>>, IComparable<BaseVirtualGraphLiteralNode<TNodeID, TGraphID>>
    {
        private IGraph _subgraph;

        public BaseVirtualGraphLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.GraphLiteral, id, provider) { }

        public BaseVirtualGraphLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IGraphLiteralNode value)
            : base(g, NodeType.GraphLiteral, id, provider, value) { }

        protected sealed override void OnMaterialise()
        {
            IGraphLiteralNode temp = (IGraphLiteralNode)this._value;
            this._subgraph = temp.SubGraph;
        }

        public IGraph SubGraph
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._subgraph;
            }
        }

        public override int CompareTo(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareGraphLiterals(this, other);
        }

        public override bool Equals(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreGraphLiteralsEqual(this, other);
        }

        public bool Equals(BaseVirtualGraphLiteralNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IGraphLiteralNode)other);
        }

        public int CompareTo(BaseVirtualGraphLiteralNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IGraphLiteralNode)other);
        }
    }

    public abstract class BaseVirtualLiteralNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, ILiteralNode,
          IEquatable<BaseVirtualLiteralNode<TNodeID, TGraphID>>, IComparable<BaseVirtualLiteralNode<TNodeID, TGraphID>>
    {
        private String _litValue, _lang;
        private Uri _datatype;

        public BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Literal, id, provider) { }

        public BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, ILiteralNode value)
            : base(g, NodeType.Literal, id, provider, value) { }

        protected sealed override void  OnMaterialise()
        {
            ILiteralNode temp = (ILiteralNode)this._value;
            this._litValue = temp.Value;
            this._lang = temp.Language;
            this._datatype = temp.DataType;
        }

        public String Value
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._litValue;
            }
        }

        public String Language
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._lang;
            }
        }

        public Uri DataType
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._datatype;
            }
        }

        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareLiterals(this, other);
        }

        public override bool Equals(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreLiteralsEqual(this, other);
        }

        public bool Equals(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
        {
            return this.Equals((ILiteralNode)other);
        }

        public int CompareTo(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((ILiteralNode)other);
        }
        
    }

    public abstract class BaseVirtualUriNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IUriNode,
          IEquatable<BaseVirtualUriNode<TNodeID, TGraphID>>, IComparable<BaseVirtualUriNode<TNodeID, TGraphID>>
    {
        private Uri _u;

        public BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Uri, id, provider) { }

        public BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IUriNode value)
            : base(g, NodeType.Uri, id, provider, value) { }

        protected sealed override void OnMaterialise()
        {
            IUriNode temp = (IUriNode)this._value;
            this._u = temp.Uri;
        }

        public Uri Uri
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._u;
            }
        }


        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareUris(this, other);
        }

        public override bool Equals(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreUrisEqual(this, other);
        }

        public bool Equals(BaseVirtualUriNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IUriNode)other);
        }

        public int CompareTo(BaseVirtualUriNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IUriNode)other);
        }
    }

    public abstract class BaseVirtualVariableNode<TNodeID, TGraphID>
        : BaseVirtualNode<TNodeID, TGraphID>, IVariableNode,
          IEquatable<BaseVirtualVariableNode<TNodeID, TGraphID>>, IComparable<BaseVirtualVariableNode<TNodeID, TGraphID>>
    {
        private String _var;

        public BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
            : base(g, NodeType.Variable, id, provider) { }

        public BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IVariableNode value)
            : base(g, NodeType.Variable, id, provider, value) { }

        protected sealed override void OnMaterialise()
        {
            IVariableNode temp = (IVariableNode)this._value;
            this._var = temp.VariableName;
        }

        public String VariableName
        {
            get
            {
                if (this._value == null) this.MaterialiseValue();
                return this._var;
            }
        }

        public override int CompareTo(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other == null) return 1;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual) && areEqual) return 0;

            return ComparisonHelper.CompareVariables(this, other);
        }

        public override bool Equals(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            bool areEqual;
            if (this.TryVirtualEquality(other, out areEqual)) return areEqual;

            return EqualityHelper.AreVariablesEqual(this, other);
        }

        public bool Equals(BaseVirtualVariableNode<TNodeID, TGraphID> other)
        {
            return this.Equals((IVariableNode)other);
        }

        public int CompareTo(BaseVirtualVariableNode<TNodeID, TGraphID> other)
        {
            return this.CompareTo((IVariableNode)other);
        }
    }
}
