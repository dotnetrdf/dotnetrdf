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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    /// <summary>
    /// A <see cref="WrapperNode">wrapper</see> that provides read/write dictionary and dynamic functionality.
    /// </summary>
    public partial class DynamicNode : WrapperNode, IUriNode, IBlankNode, IDynamicMetaObjectProvider
    {
        private readonly Uri baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNode"/> class.
        /// </summary>
        /// <param name="node">The node to wrap.</param>
        /// <param name="baseUri">The URI used to resolve relative predicate references.</param>
        /// <exception cref="InvalidOperationException">When <paramref name="node"/> has no graph.</exception>
        // TODO: Make sure all instantiations copy original node to appropriate host graph
        public DynamicNode(INode node, Uri baseUri = null)
            : base(node)
        {
            if (Graph is null)
            {
                throw new InvalidOperationException("Node must have graph");
            }

            this.baseUri = baseUri;
        }

        /// <summary>
        /// Gets the URI used to resolve relative predicate references.
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this.baseUri ?? this.Graph.BaseUri;
            }
        }

        /// <inheritdoc/>
        Uri IUriNode.Uri
        {
            get
            {
                return this.Node is IUriNode uriNode ?
                    uriNode.Uri :
                    throw new InvalidOperationException("is not a uri node");
            }
        }

        /// <inheritdoc/>
        string IBlankNode.InternalID
        {
            get
            {
                return this.Node is IBlankNode blankNode ?
                    blankNode.InternalID :
                    throw new InvalidOperationException("is not a blank node");
            }
        }

        /// <inheritdoc/>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }
    }
}
