using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Virtualisation
{
    public class SimpleVirtualBlankNode
        : BaseVirtualBlankNode<int, int>, IEquatable<SimpleVirtualBlankNode>, IComparable<SimpleVirtualBlankNode>
    {
        public SimpleVirtualBlankNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        public SimpleVirtualBlankNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IBlankNode value)
            : base(g, id, provider, value) { }

        public bool Equals(SimpleVirtualBlankNode other)
        {
            return this.Equals((IBlankNode)other);
        }

        public int CompareTo(SimpleVirtualBlankNode other)
        {
            return this.CompareTo((IBlankNode)other);
        }

        public override INode CopyNode(IGraph target)
        {
            if (this._value != null)
            {
                return new SimpleVirtualBlankNode(target, this.VirtualID, this.Provider, (IBlankNode)this._value);
            } 
            else 
            {
                return new SimpleVirtualBlankNode(target, this.VirtualID, this.Provider);
            }
        }
    }

    public class SimpleVirtualGraphLiteralNode
        : BaseVirtualGraphLiteralNode<int, int>, IEquatable<SimpleVirtualGraphLiteralNode>, IComparable<SimpleVirtualGraphLiteralNode>
    {
        public SimpleVirtualGraphLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        public SimpleVirtualGraphLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IGraphLiteralNode value)
            : base(g, id, provider, value) { }

        public bool Equals(SimpleVirtualGraphLiteralNode other)
        {
            return this.Equals((IGraphLiteralNode)other);
        }

        public int CompareTo(SimpleVirtualGraphLiteralNode other)
        {
            return this.CompareTo((IGraphLiteralNode)other);
        }

        public override INode CopyNode(IGraph target)
        {
            if (this._value != null)
            {
                return new SimpleVirtualGraphLiteralNode(target, this.VirtualID, this.Provider, (IGraphLiteralNode)this._value);
            }
            else
            {
                return new SimpleVirtualGraphLiteralNode(target, this.VirtualID, this.Provider);
            }
        }
    }

    public class SimpleVirtualLiteralNode
        : BaseVirtualLiteralNode<int, int>, IEquatable<SimpleVirtualLiteralNode>, IComparable<SimpleVirtualLiteralNode>
    {
        public SimpleVirtualLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        public SimpleVirtualLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, ILiteralNode value)
            : base(g, id, provider, value) { }

        public bool Equals(SimpleVirtualLiteralNode other)
        {
            return this.Equals((ILiteralNode)other);
        }

        public int CompareTo(SimpleVirtualLiteralNode other)
        {
            return this.CompareTo((ILiteralNode)other);
        }

        public override INode CopyNode(IGraph target)
        {
            if (this._value != null)
            {
                return new SimpleVirtualLiteralNode(target, this.VirtualID, this.Provider, (ILiteralNode)this._value);
            }
            else
            {
                return new SimpleVirtualLiteralNode(target, this.VirtualID, this.Provider);
            }
        }
    }

    public class SimpleVirtualUriNode
        : BaseVirtualUriNode<int, int>, IEquatable<SimpleVirtualUriNode>, IComparable<SimpleVirtualUriNode>
    {
        public SimpleVirtualUriNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        public SimpleVirtualUriNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IUriNode value)
            : base(g, id, provider, value) { }

        public bool Equals(SimpleVirtualUriNode other)
        {
            return this.Equals((IUriNode)other);
        }

        public int CompareTo(SimpleVirtualUriNode other)
        {
            return this.CompareTo((IUriNode)other);
        }

        public override INode CopyNode(IGraph target)
        {
            if (this._value != null)
            {
                return new SimpleVirtualUriNode(target, this.VirtualID, this.Provider, (IUriNode)this._value);
            }
            else
            {
                return new SimpleVirtualUriNode(target, this.VirtualID, this.Provider);
            }
        }
    }

    public class SimpleVirtualVariableNode
        : BaseVirtualVariableNode<int, int>, IEquatable<SimpleVirtualVariableNode>, IComparable<SimpleVirtualVariableNode>
    {
        public SimpleVirtualVariableNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        public SimpleVirtualVariableNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IVariableNode value)
            : base(g, id, provider, value) { }

        public bool Equals(SimpleVirtualVariableNode other)
        {
            return this.Equals((IVariableNode)other);
        }

        public int CompareTo(SimpleVirtualVariableNode other)
        {
            return this.CompareTo((IVariableNode)other);
        }

        public override INode CopyNode(IGraph target)
        {
            if (this._value != null)
            {
                return new SimpleVirtualVariableNode(target, this.VirtualID, this.Provider, (IVariableNode)this._value);
            }
            else
            {
                return new SimpleVirtualVariableNode(target, this.VirtualID, this.Provider);
            }
        }
    }
}
