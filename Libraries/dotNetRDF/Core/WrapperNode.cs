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

    public abstract partial class WrapperNode : INode
    {
        protected WrapperNode(INode node) => Node = node ?? throw new ArgumentNullException(nameof(node));

        public NodeType NodeType => Node.NodeType;

        public IGraph Graph => Node.Graph;

        public Uri GraphUri { get => Node.GraphUri; set => Node.GraphUri = value; }

        protected INode Node { get; private set; }

        public override bool Equals(object obj) => Node.Equals(obj);

        public override int GetHashCode() => Node.GetHashCode();

        public override string ToString() => Node.ToString();

        public int CompareTo(INode other) => Node.CompareTo(other);

        public int CompareTo(IBlankNode other) => Node.CompareTo(other);

        public int CompareTo(IGraphLiteralNode other) => Node.CompareTo(other);

        public int CompareTo(ILiteralNode other) => Node.CompareTo(other);

        public int CompareTo(IUriNode other) => Node.CompareTo(other);

        public int CompareTo(IVariableNode other) => Node.CompareTo(other);

        public bool Equals(INode other) => Node.Equals(other);

        public bool Equals(IBlankNode other) => Node.Equals(other);

        public bool Equals(IGraphLiteralNode other) => Node.Equals(other);

        public bool Equals(ILiteralNode other) => Node.Equals(other);

        public bool Equals(IUriNode other) => Node.Equals(other);

        public bool Equals(IVariableNode other) => Node.Equals(other);

        public string ToString(INodeFormatter formatter) => Node.ToString(formatter);

        public string ToString(INodeFormatter formatter, TripleSegment segment) => Node.ToString(formatter, segment);
    }
}
