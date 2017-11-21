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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which just determines whether any Triples are present terminating parsing as soon as the first triple is received
    /// </summary>
    public class AnyHandler 
        : BaseRdfHandler
    {
        private bool _any = false;

        /// <summary>
        /// Creates a new Any Handler
        /// </summary>
        public AnyHandler()
            : base(new MockNodeFactory()) { }

        /// <summary>
        /// Gets whether any Triples have been parsed
        /// </summary>
        public bool Any
        {
            get
            {
                return _any;
            }
        }

        /// <summary>
        /// Starts handling RDF by resetting the <see cref="AnyHandler.Any">Any</see> flag to false
        /// </summary>
        protected override void StartRdfInternal()
        {
            _any = false;
        }

        /// <summary>
        /// Handles Base URIs by ignoring them
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Namespaces by ignoring them
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Triples by setting the <see cref="AnyHandler.Any">Any</see> flag and terminating parsing
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            _any = true;
            return false;
        }

        /// <summary>
        /// Gets that this handler does not accept all triples since it stops as soon as it sees the first triple
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return false;
            }
        }
    }
}
