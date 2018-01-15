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
    /// An RDF Handler which wraps another handler, stripping explicit xsd:string datatypes on object literals
    /// </summary>
    public class StripStringHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private static readonly Uri DataTypeString = new Uri(XmlSpecsHelper.XmlSchemaDataTypeString);

        /// <summary>
        /// Creates a new StripStringHandler
        /// </summary>
        /// <param name="handler">Inner handler to use</param>
        public StripStringHandler(IRdfHandler handler) : base(handler)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        /// <summary>
        /// Handles triples by stripping explicit xsd:string datatype on object literals before delegating to inner handler
        /// </summary>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (t.Object is ILiteralNode literal && EqualityHelper.AreUrisEqual(literal.DataType, DataTypeString))
                t = new Triple(t.Subject, t.Predicate, t.Graph.CreateLiteralNode(literal.Value));

            return _handler.HandleTriple(t);
        }

        #region Delegate to inner handler

        /// <summary>
        /// Gets the handler wrapped by this handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers => _handler.AsEnumerable();

        /// <summary>
        /// Starts inner handler
        /// </summary>
        protected override void StartRdfInternal() => _handler.StartRdf();

        /// <summary>
        /// Ends inner handler
        /// </summary>
        protected override void EndRdfInternal(bool ok) => _handler.EndRdf(ok);

        /// <summary>
        /// Delegates base Uri handling to inner handler
        /// </summary>
        protected override bool HandleBaseUriInternal(Uri baseUri) => _handler.HandleBaseUri(baseUri);

        /// <summary>
        /// Delegates namespace handling to inner handler
        /// </summary>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri) => _handler.HandleNamespace(prefix, namespaceUri);

        /// <summary>
        /// Gets whether inner handler accepts all triples
        /// </summary>
        public override bool AcceptsAll => _handler.AcceptsAll;

        #endregion
    }
}