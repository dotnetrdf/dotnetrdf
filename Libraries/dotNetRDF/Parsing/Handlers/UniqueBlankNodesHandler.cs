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

using System;
using System.Collections.Generic;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A decorator for handlers which ensures that all blank nodes get unique IDs even if a blank node identifier is reused
    /// </summary>
    /// <remarks>
    /// <para>
    /// In most parsing scenarios this handler is not suitable for usage as it may unintentionally modify the RDF data being parsed, in non-parsing scenarios where this handler is instead being used as a means to generate RDF data from some non-RDF source it may prove very useful.
    /// </para>
    /// <para>
    /// This handler essentially works by redirecting all calls to the argument taking form of <see cref="INodeFactory.CreateBlankNode()"/> with the non-argument form which should always generate a new blank node thus guaranteeing the uniqueness of nodes.
    /// </para>
    /// </remarks>
    public class UniqueBlankNodesHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;

        /// <summary>
        /// Creates a new Unique Blank Nodes handler
        /// </summary>
        /// <param name="handler"></param>
        public UniqueBlankNodesHandler(IRdfHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _handler = handler;
        }

        /// <summary>
        /// Gets the inner handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return _handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Creates a Blank Node
        /// </summary>
        /// <param name="nodeId">Node ID which will be ignored by this Handler</param>
        /// <returns></returns>
        public override IBlankNode CreateBlankNode(string nodeId)
        {
            return base.CreateBlankNode();
        }

        /// <summary>
        /// Starts handling RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            _handler.StartRdf();
        }

        /// <summary>
        /// Ends handling RDF
        /// </summary>
        /// <param name="ok">Whether parsing completed OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            _handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles a Base URI declaration
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return _handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles a Namespace declaration
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return _handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            return _handler.HandleTriple(t);
        }

        /// <summary>
        /// Gets whether the inner handler accepts all
        /// </summary>
        public override bool AcceptsAll
        {
            get
            {
                return _handler.AcceptsAll;
            }
        }
    }
}
