/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage.Virtualisation;

/// <summary>
/// Abstract Base implementation of a Virtual Node which is a Node that is represented only by some ID until such time as its value actually needs materializing.
/// </summary>
/// <typeparam name="TNodeID">Node ID Type.</typeparam>
/// <typeparam name="TGraphID">Graph ID Type.</typeparam>
/// <remarks>
/// <para>
/// As far as possible equality checks are carried out using these IDs and limited comparisons may also be done this way.  More specific implementations may wish to derive from this class in order to override the default comparison implementation to further reduce the number of places where value materialisation is done.
/// </para>
/// <para>
/// Note that this class does not implement any of the specialized Node interfaces and instead relies on the casting of its materialized value to an appropriately typed node to provide the true values to code that needs it.
/// </para>
/// </remarks>
public abstract class BaseVirtualNode<TNodeID, TGraphID> 
    : IVirtualNode<TNodeID, TGraphID>, IEquatable<BaseVirtualNode<TNodeID, TGraphID>>, IComparable<BaseVirtualNode<TNodeID, TGraphID>>,
        ICanCopy
{
    private IGraph _g;
    private Uri _graphUri;
    private TNodeID _id;
    private IVirtualRdfProvider<TNodeID, TGraphID> _provider;
    private NodeType _type;

    /// <summary>
    /// The materialized value of the Virtual Node.
    /// </summary>
    protected INode _value;

    /// <summary>
    /// Creates a new Base Virtual Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="type">Type of the node.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    protected BaseVirtualNode(IGraph g, NodeType type, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
    {
        _g = g;
        if (_g != null) _graphUri = _g.BaseUri;
        _type = type;
        _id = id;
        _provider = provider;
    }

    /// <summary>
    /// Creates a new Base Virtual Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="type">Type of the node.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    /// <param name="value">Materialized Value.</param>
    protected BaseVirtualNode(IGraph g, NodeType type, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, INode value)
        : this(g, type, id, provider)
    {
        _value = value;
        if (_value.NodeType != _type) throw new RdfException("Cannot create a pre-materialized Virtual Node where the materialized value is a Node of the wrong type! Expected " + _type + " but got " + _value.NodeType);
        OnMaterialise();
    }

    /// <summary>
    /// Materializes the Value if it is not already materialized.
    /// </summary>
    protected void MaterialiseValue()
    {
        if (_value == null)
        {
            // Materialise the value
            _value = _provider.GetValue(_g, _id);
            if (_value.NodeType != _type) throw new RdfException("The Virtual RDF Provider materialized a Node of the wrong type! Expected " + _type + " but got " + _value.NodeType);
            OnMaterialise();
        }
    }

    /// <summary>
    /// Called after the value is materialized for the first time.
    /// </summary>
    protected virtual void OnMaterialise()
    {

    }

    #region IVirtualNode<TNodeID,TGraphID> Members

    /// <summary>
    /// Gets the Virtual ID of the Node.
    /// </summary>
    public TNodeID VirtualID
    {
        get
        {
            return _id;
        }
    }

    /// <summary>
    /// Gets the Virtual RDF Provider of the Node.
    /// </summary>
    public IVirtualRdfProvider<TNodeID, TGraphID> Provider
    {
        get
        {
            return _provider;
        }
    }

    /// <summary>
    /// Gets whether the Nodes value has been materialized.
    /// </summary>
    public bool IsMaterialised
    {
        get
        {
            return _value != null;
        }
    }

    /// <summary>
    /// Gets the materialized value of the Node forcing it to be materialized if it hasn't already.
    /// </summary>
    public INode MaterialisedValue
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _value;
        }
    }

    #endregion

    #region INode Members

    /// <summary>
    /// Gets the Type of the Node.
    /// </summary>
    public NodeType NodeType
    {
        get
        {
            return _type;
        }
    }

    /// <summary>
    /// Gets the Graph the Node belongs to.
    /// </summary>
    public IGraph Graph
    {
        get 
        {
            return _g; 
        }
    }

    /// <summary>
    /// Gets/Sets the Graph URI of the Node.
    /// </summary>
    public Uri GraphUri
    {
        get
        {
            return _graphUri;
        }
        set
        {
            _graphUri = value;
        }
    }

    /// <summary>
    /// Gets the String representation of the Node formatted with the given Node formatter.
    /// </summary>
    /// <param name="formatter">Formatter.</param>
    /// <returns></returns>
    public string ToString(INodeFormatter formatter)
    {
        return formatter.Format(this);
    }

    /// <summary>
    /// Gets the String representation of the Node formatted with the given Node formatter.
    /// </summary>
    /// <param name="formatter">Formatter.</param>
    /// <param name="segment">Triple Segment.</param>
    /// <returns></returns>
    public string ToString(INodeFormatter formatter, TripleSegment segment)
    {
        return formatter.Format(this, segment);
    }

    #endregion

    #region IComparable Implementations

    /// <summary>
    /// Compares this Node to another Virtual Node.
    /// </summary>
    /// <param name="other">Other Virtual Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public int CompareTo(IVirtualNode<TNodeID, TGraphID> other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;

        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual)
        {
            return 0;
        }
        else
        {
            return CompareTo((INode)other);
        }
    }

    /// <summary>
    /// Compares this Node to another Virtual Node.
    /// </summary>
    /// <param name="other">Other Virtual Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public int CompareTo(BaseVirtualNode<TNodeID, TGraphID> other)
    {
        return CompareTo((IVirtualNode<TNodeID, TGraphID>)other);
    }

    /// <summary>
    /// Compares this Node to another Node.
    /// </summary>
    /// <param name="other">Other Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual) return 0;

        switch (_type)
        {
            case NodeType.Blank:
                if (other.NodeType == NodeType.Variable)
                {
                    // Blank Nodes are greater than variables
                    return 1;
                }
                else if (other.NodeType == NodeType.Blank)
                {
                    // Compare Blank Node appropriately
                    return ComparisonHelper.CompareBlankNodes((IBlankNode)this, (IBlankNode)other);
                }
                else
                {
                    // Blank Nodes are less than everything else
                    return -1;
                }

            case NodeType.GraphLiteral:
                if (other.NodeType == NodeType.GraphLiteral)
                {
                    // Compare Graph Literals appropriately
                    return ComparisonHelper.CompareGraphLiterals((IGraphLiteralNode)this, (IGraphLiteralNode)other);
                }
                else
                {
                    // Graph Literals are greater than everything else
                    return 1;
                }

            case NodeType.Literal:
                if (other.NodeType == NodeType.GraphLiteral)
                {
                    // Literals are less than Graph Literals
                    return -1;
                }
                else if (other.NodeType == NodeType.Literal)
                {
                    // Compare Literals appropriately
                    return ComparisonHelper.CompareLiterals((ILiteralNode)this, (ILiteralNode)other);
                }
                else
                {
                    // Literals are greater than anything else (i.e. Blanks, Variables and URIs)
                    return 1;
                }

            case NodeType.Uri:
                if (other.NodeType == NodeType.GraphLiteral || other.NodeType == NodeType.Literal)
                {
                    // URIs are less than Literals and Graph Literals
                    return -1;
                }
                else if (other.NodeType == NodeType.Uri)
                {
                    // Compare URIs appropriately
                    return ComparisonHelper.CompareUris((IUriNode)this, (IUriNode)other);
                }
                else
                {
                    // URIs are greater than anything else (i.e. Blanks and Variables)
                    return 1;
                }

            case NodeType.Variable:
                if (other.NodeType == NodeType.Variable)
                {
                    // Compare Variables accordingly
                    return ComparisonHelper.CompareVariables((IVariableNode)this, (IVariableNode)other);
                }
                else
                {
                    // Variables are less than anything else
                    return -1;
                }

            default:
                // Things are always greater than unknown node types
                return 1;
        }
    }

    /// <summary>
    /// Compares this Node to another Ref Node.
    /// </summary>
    /// <param name="other">Other Ref Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(IRefNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(IBlankNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another Graph LiteralNode.
    /// </summary>
    /// <param name="other">Other Graph Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(IGraphLiteralNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(ILiteralNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(IUriNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(IVariableNode other)
    {
        return CompareTo((INode)other);
    }

    /// <summary>
    /// Compares this Node to another Triple Node.
    /// </summary>
    /// <param name="other">Other Triple Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform comparison.
    /// </remarks>
    public virtual int CompareTo(ITripleNode other)
    {
        return CompareTo((INode)other);
    }

    #endregion

    #region IEquatable Implementations

    /// <summary>
    /// Checks this Node for equality against another Object.
    /// </summary>
    /// <param name="obj">Other Object.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public sealed override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj == null) return false;

        if (obj is INode)
        {
            return Equals((INode)obj);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks this Node for equality against another Virtual Node.
    /// </summary>
    /// <param name="other">Other Virtual Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public bool Equals(IVirtualNode<TNodeID, TGraphID> other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;

        return ReferenceEquals(_provider, other.Provider) && _id.Equals(other.VirtualID);
    }

    /// <summary>
    /// Checks this Node for equality against another Virtual Node.
    /// </summary>
    /// <param name="other">Other Virtual Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public bool Equals(BaseVirtualNode<TNodeID, TGraphID> other)
    {
        return Equals((IVirtualNode<TNodeID, TGraphID>)other);
    }

    /// <summary>
    /// Checks this Node for equality against another Node.
    /// </summary>
    /// <param name="other">Other Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public bool Equals(INode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;

        bool areEqual;
        if (_type != other.NodeType)
        {
            // Non-equal node types cannot be equal
            return false;
        }
        else if (TryVirtualEquality(other, out areEqual))
        {
            // If Virtual Nodes originate from same virtual RDF provider can compare based on their virtual Node IDs
            return areEqual;
        }
        else
        {
            // If not both virtual and are of the same type the only way to determine equality is to
            // materialise the value of this node and then check that against the other node
            if (_value == null) MaterialiseValue();
            return _value.Equals(other);
        }
    }

    /// <summary>
    /// Checks the Node Types and if they are equal invokes the INode based comparison.
    /// </summary>
    /// <param name="other">Node to compare with for equality.</param>
    /// <returns></returns>
    private bool TypedEquality(INode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;

        if (_type == other.NodeType)
        {
            return Equals(other);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to check for equality using virtual node IDs.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <param name="areEqual">Whether the virtual nodes are equal.</param>
    /// <returns>
    /// Whether the virtual equality test was valid, if false then other means must be used to determine equality.
    /// </returns>
    protected bool TryVirtualEquality(INode other, out bool areEqual)
    {
        areEqual = false;
        if (other is IVirtualNode<TNodeID, TGraphID>)
        {
            var virt = (IVirtualNode<TNodeID, TGraphID>)other;
            if (ReferenceEquals(_provider, virt.Provider))
            {
                areEqual = _id.Equals(virt.VirtualID);
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
    /// Checks this Node for equality against another Ref Node.
    /// </summary>
    /// <param name="other">Other Ref Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(IRefNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(IBlankNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another Graph Literal Node.
    /// </summary>
    /// <param name="other">Other Graph Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(IGraphLiteralNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(ILiteralNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(IUriNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(IVariableNode other)
    {
        return TypedEquality(other);
    }

    /// <summary>
    /// Checks this Node for equality against another Triple Node.
    /// </summary>
    /// <param name="other">Other Triple Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialized in order to perform the equality check.
    /// </remarks>
    public virtual bool Equals(ITripleNode other)
    {
        return TypedEquality(other);
    }
    #endregion

    #region ICopyNodes abstract Member
    /// <summary>
    /// Copies the Virtual Node into another Graph.
    /// </summary>
    /// <param name="target">Target Graph.</param>
    /// <returns></returns>
    public abstract INode CopyNode(IGraph target);
    #endregion

    /// <summary>
    /// Gets the Hash Code of the Virtual Node.
    /// </summary>
    /// <returns></returns>
    public sealed override int GetHashCode()
    {
        return _id.GetHashCode();
    }

    /// <summary>
    /// Method to be implemented in derived classes to provide comparison of VirtualId values.
    /// </summary>
    /// <param name="other">The other virtual ID value to be compared with this node's virtual ID value.</param>
    /// <returns>The comparison result.</returns>
    public abstract int CompareVirtualId(TNodeID other);

    /// <summary>
    /// Attempt to compare this node with another node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <param name="comparisonResult">The comparison result.</param>
    /// <returns>True if the comparison could be performed, false otherwise.</returns>
    /// <remarks>This node can only be compared to <paramref name="other"/> if <paramref name="other"/>
    /// is a <see cref="IVirtualNode{TNodeID,TGraphID}"/> from the same 
    /// <see cref="IVirtualRdfProvider{TNodeID,TGraphID}"/> as this node.</remarks>
    public bool TryCompareVirtualId(INode other, out int comparisonResult)
    {
        if (other is IVirtualNode<TNodeID, TGraphID>)
        {
            var virt = other as IVirtualNode<TNodeID, TGraphID>;
            if (ReferenceEquals(_provider, virt.Provider))
            {
                if (_id.Equals(virt.VirtualID))
                {
                    comparisonResult = 0;
                }
                else
                {
                    comparisonResult = CompareVirtualId(virt.VirtualID);
                }
                return true;
            }
        }
        comparisonResult = 0;
        return false;
    }

    /// <summary>
    /// Gets the String representation of the Node.
    /// </summary>
    /// <returns></returns>
    public sealed override string ToString()
    {
        if (_value == null) MaterialiseValue();
        return _value.ToString();
    }

}