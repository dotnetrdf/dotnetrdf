/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VDS.RDF
{
    using System;
    using System.Diagnostics;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    /// <summary>
    /// Abstract decorator for Nodes to make it easier to layer functionality on top of existing implementations.
    /// </summary>
    public abstract class WrapperNode : INode, IBlankNode, IUriNode, ILiteralNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperNode"/> class.
        /// </summary>
        /// <param name="node">The node this is a wrapper around.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null.</exception>
        [DebuggerStepThrough]
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

        /// <inheritdoc/>
        string IBlankNode.InternalID
        {
            get
            {
                if (Node.NodeType != NodeType.Blank)
                {
                    throw new InvalidCastException();
                }

                return ((IBlankNode)Node).InternalID;
            }
        }

        /// <inheritdoc/>
        Uri IUriNode.Uri
        {
            get
            {
                if (Node.NodeType != NodeType.Uri)
                {
                    throw new InvalidCastException();
                }

                return ((IUriNode)Node).Uri;
            }
        }

        /// <inheritdoc/>
        string ILiteralNode.Value
        {
            get
            {
                if (Node.NodeType != NodeType.Literal)
                {
                    throw new InvalidCastException();
                }

                return ((ILiteralNode)Node).Value;
            }
        }

        /// <inheritdoc/>
        string ILiteralNode.Language
        {
            get
            {
                if (Node.NodeType != NodeType.Literal)
                {
                    throw new InvalidCastException();
                }

                return ((ILiteralNode)Node).Language;
            }
        }

        /// <inheritdoc/>
        Uri ILiteralNode.DataType
        {
            get
            {
                if (Node.NodeType != NodeType.Literal)
                {
                    throw new InvalidCastException();
                }

                return ((ILiteralNode)Node).DataType;
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

        /// <inheritdoc/>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException("This INode implementation does not support Serialization.");
        }

        /// <inheritdoc/>
        XmlSchema IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }

        /// <inheritdoc/>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }

        /// <inheritdoc/>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }
    }
}
