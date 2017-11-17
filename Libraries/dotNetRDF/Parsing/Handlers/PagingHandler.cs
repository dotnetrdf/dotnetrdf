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
    /// A RDF Handler which wraps another handler passing only the chunk of triples falling within a given limit and offset to the underlying Handler
    /// </summary>
    /// <remarks>
    /// This handler does not guarantee that you will receive exactly the chunk specified by the limit and offset for two reasons:
    /// <ol>
    /// <li>It does not perform any sort of data de-duplication so it is possible that if this handler receives duplicate triples and the underlying handler performs de-duplication then you may see less triples than you expect in your final output since although the underlying handler will receive at most the specified chunk size of triples it may not retain them all</li>
    /// <li>If there are fewer triples than the chunk size or if the chunk exceeds the bounds of the data then you will only receive the triples that fall within the chunk (if any)</li>
    /// </ol>
    /// </remarks>
    public class PagingHandler 
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private readonly IRdfHandler _handler;
        private readonly int _limit = 0, _offset = 0;
        private int _counter = 0;

        /// <summary>
        /// Creates a new Paging Handler
        /// </summary>
        /// <param name="handler">Inner Handler to use</param>
        /// <param name="limit">Limit</param>
        /// <param name="offset">Offset</param>
        /// <remarks>
        /// If you just want to use an offset and not apply a limit then set limit to be less than zero
        /// </remarks>
        public PagingHandler(IRdfHandler handler, int limit, int offset)
            : base(handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _handler = handler;
            _limit = Math.Max(-1, limit);
            _offset = Math.Max(0, offset);
        }

        /// <summary>
        /// Creates a new Paging Handler
        /// </summary>
        /// <param name="handler">Inner Handler to use</param>
        /// <param name="limit">Limit</param>
        public PagingHandler(IRdfHandler handler, int limit)
            : this(handler, limit, 0) { }

        /// <summary>
        /// Gets the Inner Handler wrapped by this Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return _handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Starts RDF Handler
        /// </summary>
        protected override void StartRdfInternal()
        {
            _handler.StartRdf();
            _counter = 0;
        }

        /// <summary>
        /// Ends RDF Handler
        /// </summary>
        /// <param name="ok">Indicated whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            _handler.EndRdf(ok);
            _counter = 0;
        }

        /// <summary>
        /// Handles a Triple by passing it to the Inner Handler only if the Offset has been passed and the Limit has yet to be reached
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        /// <remarks>
        /// Terminates handling immediately upon the reaching of the limit
        /// </remarks>
        protected override bool HandleTripleInternal(Triple t)
        {
            // If the Limit is zero stop parsing immediately
            if (_limit == 0) return false;

            _counter++;
            if (_limit > 0)
            {
                // Limit greater than zero means get a maximum of limit triples after the offset
                if (_counter > _offset && _counter <= _limit + _offset)
                {
                    return _handler.HandleTriple(t);
                }
                else if (_counter > _limit + _offset)
                {
                    // Stop parsing when we've reached the limit
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // Limit less than zero means get all triples after the offset
                if (_counter > _offset)
                {
                    return _handler.HandleTriple(t);
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Handles Namespace Declarations by allowing the inner handler to handle it
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return _handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles Base URI Declarations by allowing the inner handler to handle it
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return _handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Gets whether the Handler will accept all Triples based on its Limit setting
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return _limit < 0;
            }
        }
    }
}
