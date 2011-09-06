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

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// Simple implementation of a Virtual Blank Node where the virtual IDs are integers
    /// </summary>
    public class SimpleVirtualBlankNode
        : BaseVirtualBlankNode<int, int>, IEquatable<SimpleVirtualBlankNode>, IComparable<SimpleVirtualBlankNode>
    {
        /// <summary>
        /// Creates a new Virtual Blank Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public SimpleVirtualBlankNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Blank Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public SimpleVirtualBlankNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IBlankNode value)
            : base(g, id, provider, value) { }

        /// <summary>
        /// Determines whether this Node is equal to another virtual Blank node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        public bool Equals(SimpleVirtualBlankNode other)
        {
            return this.Equals((IBlankNode)other);
        }

        /// <summary>
        /// Compares this Node to another virtual Blank node
        /// </summary>
        /// <param name="other">Other Blank Node</param>
        /// <returns></returns>
        public int CompareTo(SimpleVirtualBlankNode other)
        {
            return this.CompareTo((IBlankNode)other);
        }

        /// <summary>
        /// Copies the Node to another Graph
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        public override INode CopyNode(IGraph target)
        {
            return new SimpleVirtualBlankNode(target, this.VirtualID, this.Provider);
        }
    }

    /// <summary>
    /// Simple implementation of a Virtual Graph Literal Node where the virtual IDs are integers
    /// </summary>
    public class SimpleVirtualGraphLiteralNode
        : BaseVirtualGraphLiteralNode<int, int>, IEquatable<SimpleVirtualGraphLiteralNode>, IComparable<SimpleVirtualGraphLiteralNode>
    {
        /// <summary>
        /// Creates a new Virtual Graph Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public SimpleVirtualGraphLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Graph Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Values</param>
        public SimpleVirtualGraphLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IGraphLiteralNode value)
            : base(g, id, provider, value) { }

        /// <summary>
        /// Determines whether this Node is equal to another virtual Graph Literal node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        public bool Equals(SimpleVirtualGraphLiteralNode other)
        {
            return this.Equals((IGraphLiteralNode)other);
        }

        /// <summary>
        /// Compares this Node to another virtual Graph Literal node
        /// </summary>
        /// <param name="other">Other Graph Literal Node</param>
        /// <returns></returns>
        public int CompareTo(SimpleVirtualGraphLiteralNode other)
        {
            return this.CompareTo((IGraphLiteralNode)other);
        }

        /// <summary>
        /// Copies the Node to another Graph including the materialised value if present
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
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

    /// <summary>
    /// Simple implementation of a Virtual Literal Node where the virtual IDs are integers
    /// </summary>
    public class SimpleVirtualLiteralNode
        : BaseVirtualLiteralNode<int, int>, IEquatable<SimpleVirtualLiteralNode>, IComparable<SimpleVirtualLiteralNode>
    {
        /// <summary>
        /// Creates a new Virtual Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public SimpleVirtualLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Literal Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public SimpleVirtualLiteralNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, ILiteralNode value)
            : base(g, id, provider, value) { }

        /// <summary>
        /// Determines whether this Node is equal to another virtual Literal node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        public bool Equals(SimpleVirtualLiteralNode other)
        {
            return this.Equals((ILiteralNode)other);
        }

        /// <summary>
        /// Compares this Node to another virtual Literal node
        /// </summary>
        /// <param name="other">Other Literal Node</param>
        /// <returns></returns>
        public int CompareTo(SimpleVirtualLiteralNode other)
        {
            return this.CompareTo((ILiteralNode)other);
        }

        /// <summary>
        /// Copies the Node to another Graph including the materialised value if present
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
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

    /// <summary>
    /// Simple implementation of a Virtual URI Node where the virtual IDs are integers
    /// </summary>
    public class SimpleVirtualUriNode
        : BaseVirtualUriNode<int, int>, IEquatable<SimpleVirtualUriNode>, IComparable<SimpleVirtualUriNode>
    {
        /// <summary>
        /// Creates a new Virtual URI Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public SimpleVirtualUriNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        /// <summary>
        /// Creates a new Virtual URI Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public SimpleVirtualUriNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IUriNode value)
            : base(g, id, provider, value) { }

        /// <summary>
        /// Determines whether this Node is equal to another virtual URI node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        public bool Equals(SimpleVirtualUriNode other)
        {
            return this.Equals((IUriNode)other);
        }

        /// <summary>
        /// Compares this Node to another virtual URI node
        /// </summary>
        /// <param name="other">Other URI Node</param>
        /// <returns></returns>
        public int CompareTo(SimpleVirtualUriNode other)
        {
            return this.CompareTo((IUriNode)other);
        }

        /// <summary>
        /// Copies the Node to another Graph including the materialised value if present
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
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

    /// <summary>
    /// Simple implementation of a Virtual URI Node where the virtual IDs are integers
    /// </summary>
    public class SimpleVirtualVariableNode
        : BaseVirtualVariableNode<int, int>, IEquatable<SimpleVirtualVariableNode>, IComparable<SimpleVirtualVariableNode>
    {
        /// <summary>
        /// Creates a new Virtual Variable Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        public SimpleVirtualVariableNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider)
            : base(g, id, provider) { }

        /// <summary>
        /// Creates a new Virtual Variable Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="id">Virtual ID</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="value">Materialised Value</param>
        public SimpleVirtualVariableNode(IGraph g, int id, IVirtualRdfProvider<int, int> provider, IVariableNode value)
            : base(g, id, provider, value) { }

        /// <summary>
        /// Determines whether this Node is equal to another virtual variable node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        public bool Equals(SimpleVirtualVariableNode other)
        {
            return this.Equals((IVariableNode)other);
        }

        /// <summary>
        /// Compares this Node to another virtual Variable node
        /// </summary>
        /// <param name="other">Other Variable Node</param>
        /// <returns></returns>
        public int CompareTo(SimpleVirtualVariableNode other)
        {
            return this.CompareTo((IVariableNode)other);
        }

        /// <summary>
        /// Copies the Node to another Graph including the materialised value if present
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
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
