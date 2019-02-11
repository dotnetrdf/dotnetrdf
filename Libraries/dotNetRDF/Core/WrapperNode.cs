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

namespace VDS.RDF
{
    using System;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    /// <summary>
    /// Abstract decorator for Nodes to make it easier to layer functionality on top of existing implementations.
    /// </summary>
    public abstract partial class WrapperNode : INode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperNode"/> class.
        /// </summary>
        /// <param name="node">The node this is a wrapper around.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null.</exception>
        protected WrapperNode(INode node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        /// <inheritdoc/>
        public NodeType NodeType
        {
            get
            {
                return Node.NodeType;
            }
        }

        /// <inheritdoc/>
        public IGraph Graph
        {
            get
            {
                return Node.Graph;
            }
        }

        /// <inheritdoc/>
        public Uri GraphUri
        {
            get
            {
                return Node.GraphUri;
            }

            set
            {
                Node.GraphUri = value;
            }
        }

        /// <summary>
        /// Gets the underlying node this is a wrapper around.
        /// </summary>
        protected INode Node { get; private set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Node.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Node.ToString();
        }

        /// <inheritdoc/>
        public int CompareTo(INode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(IBlankNode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(IGraphLiteralNode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(ILiteralNode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(IUriNode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public int CompareTo(IVariableNode other)
        {
            return Node.CompareTo(other);
        }

        /// <inheritdoc/>
        public bool Equals(INode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IBlankNode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IGraphLiteralNode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(ILiteralNode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IUriNode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IVariableNode other)
        {
            return Node.Equals(other);
        }

        /// <inheritdoc/>
        public string ToString(INodeFormatter formatter)
        {
            return Node.ToString(formatter);
        }

        /// <inheritdoc/>
        public string ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return Node.ToString(formatter, segment);
        }
    }
}
